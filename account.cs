/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright and licensing details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MicroCash.Client.Thin.JsonRpc.Contracts;
using MicroCash.Client.Thin.JsonRpc;

namespace MicroCash.Client.Thin
{
    internal class Account
    {
        #region Fields
        private bool m_bEnabled;
        private string m_name;
        private Int64 m_balance;
        private int m_tx;
        private uint m_addressid;
        private int m_icon;
        private CMicroCashKeyPair m_KeyPair;
        private List<AddressHistory> m_txhistory;
        private List<AddressHistory> m_newtx;
        #endregion

        #region Properties
        public bool IsEnabled
        {
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Int64 Balance
        {
            get { return m_balance; }
            set { m_balance = value; }
        }

        public int TxCount
        {
            get { return m_tx; }
            set { m_tx = value; }
        }

        public uint AddressId
        {
            get { return m_addressid; }
            set { m_addressid = value; }
        }

        public int IconId
        {
            get { return m_icon; }
            set { m_icon = value; }
        }

        public List<AddressHistory> TxHistory
        {
            get { return m_txhistory; }
            set { m_txhistory = value; }
        }

        public List<AddressHistory> NewTx
        {
            get { return m_newtx; }
            set { m_newtx = value; }
        }
        #endregion

        public Account()
        {
            m_bEnabled = true;
            m_balance = 0;
            m_icon = 0;
            m_tx = 0;
            m_addressid = 0;
            m_KeyPair = new CMicroCashKeyPair(false);
            m_txhistory = new List<AddressHistory>();
            m_newtx = new List<AddressHistory>();
        }
        
        public void GenerateKeyPair()
        {
            if (m_KeyPair == null || m_KeyPair.m_Priv == null)
                m_KeyPair = new CMicroCashKeyPair(true);
            else
                throw new InvalidOperationException("Account already has a key! A new key cannot be assigned to this account!");
        }
              
        public string GetAddressString()
        {
            return m_KeyPair.GetAddressString();
        }

        public Int64 GetBalance()
        {
            return m_balance;
        }

        public void AddTransactions(List<AddressHistory> txlist)
        {

            foreach (AddressHistory tx in txlist)
            {
                if (m_txhistory.Exists(item => item.hash==tx.hash) == false)
                {
                    m_newtx.Add(tx);
                    m_txhistory.Add(tx);
                }
            }
        }

        internal SendTxResult SendTx(MicroCashAddress paymentCodeAddress)
        {
            if (!paymentCodeAddress.IsPaymentCode)
                throw new ArgumentException("Address is not a payment code!", "paymentCodeAddress");

            double amount = paymentCodeAddress.GetPaymentAmount() / 10000.0000;

            byte[] info = new byte[8];
            Array.Copy(paymentCodeAddress.GetInfoBytes(), info, 8);

            return SendTx(paymentCodeAddress, amount, info);
        }

        internal SendTxResult SendTx(MicroCashAddress address, double amount, byte[] info)
        {
            MicroCashTransaction tx = new MicroCashTransaction();

            Array.Copy(m_KeyPair.m_Pub, 1, tx.m_Extra1, 0, 64);    //this isnt always sent, but may as well just copy it 
            tx.m_dwAddressID = this.AddressId;
            if (tx.m_dwAddressID == 0)
            {
                tx.m_dwType = 1;
            }

            tx.m_FromAddress = m_KeyPair.GetAddressBytes();
            tx.m_RecvAddress = address.GetAddressBytes();

            tx.m_qAmount = (Int64)(amount * 10000);

            if (info != null)
            {
                int nLen = info.Length;
                if (nLen > 8) nLen = 8;
                for (int x = 0; x < 8; x++) tx.m_Info[x] = 0;
                Array.Copy(info, tx.m_Info, nLen);
            }

            byte[] hash = tx.GetHash(false);
            //string s = MicroCashFunctions.ToHex(hash);
            tx.m_Signature = m_KeyPair.Sign(hash);

            MicroCashRpcClient mcrpc = new MicroCashRpcClient(GlobalSettings.RpcUrl);
            SendTransactionRpcResponse sendtx = mcrpc.SendTransaction(MicroCashFunctions.ToHex(tx.GetByteBuffer(true)));

            SendTxResult result = new SendTxResult();
            result.IsSent = sendtx.sent;
            result.ErrorMessage = mcrpc.ErrorMessage;

            return result;
        }

        public void AccountXMLSave(XmlTextWriter writer)
        {
            writer.WriteStartElement("account");
            writer.WriteElementString("name", m_name);
            writer.WriteElementString("icon", m_icon.ToString());
            writer.WriteElementString("numtx", m_tx.ToString());
            writer.WriteElementString("addressid", m_addressid.ToString());
            writer.WriteElementString("balance", m_balance.ToString());
            writer.WriteElementString("pubkey", m_KeyPair.m_PubKeyString);
            writer.WriteElementString("privkey", m_KeyPair.m_PrivKeyString);
            writer.WriteElementString("enabled", m_bEnabled.ToString());
            writer.WriteStartElement("transactions");
            foreach (AddressHistory tx in m_txhistory)
            {
                writer.WriteStartElement("tx");
                writer.WriteAttributeString("amnt", tx.amount.ToString());
                writer.WriteAttributeString("hash",tx.hash);
                writer.WriteAttributeString("from", tx.fromto);                
                writer.WriteAttributeString("type", tx.type.ToString());
                writer.WriteAttributeString("info", tx.info);                
                writer.WriteAttributeString("time", tx.time.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();

        }

        public bool AccountXMLLoad(XmlTextReader reader)
        {
            string priv = "", pub = "";

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        string name = reader.Name;
                        
                        string value = "";
                        if (name == "transactions" || name=="tx")
                        {
                            
                           
                        }
                        else
                        {
                            reader.MoveToContent();
                            value = reader.ReadElementContentAsString();
                        }
                        

                        
                        //reader.read
                        switch (name)
                        {
                            case "name": m_name = value; break;
                            case "privkey": priv = value; break;
                            case "pubkey": pub = value; break;
                            case "icon": m_icon = Convert.ToInt32(value); break;
                            case "numtx": m_tx = Convert.ToInt32(value); break;
                            case "addressid": m_addressid = Convert.ToUInt32(value); break;
                            case "balance": m_balance = Convert.ToInt64(value); break;
                            case "enabled": m_bEnabled = Convert.ToBoolean(value); break;
                            case "tx":
                                {
                                    AddressHistory tx = new AddressHistory();

                                    tx.hash = reader.GetAttribute("hash");
                                    tx.amount = Convert.ToInt64(reader.GetAttribute("amnt"));
                                    tx.fromto = reader.GetAttribute("from");
                                    tx.info = reader.GetAttribute("info");
                                    tx.type = Convert.ToInt32(reader.GetAttribute("type"));
                                    tx.time = Convert.ToInt64(reader.GetAttribute("time"));
                                    m_txhistory.Add(tx);
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "account")
                        {
                            m_KeyPair.SetKeyByString(priv, pub);                            
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
    }


}