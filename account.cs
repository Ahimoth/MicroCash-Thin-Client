/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright an licensing details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using MicroCashLibrary;
using MicroCashClient;
using System.Xml;

namespace AccountItemCode
{
    public class AccountItem
    {
        public bool m_bEnabled;
        public string m_name;        
        public Int64 m_balance;
        public int m_tx;
        public uint m_addressid;
        public int m_icon;
        public CMicroCashKeyPair m_Key;
        public List<AddressHistory> m_txhistory;
        public List<AddressHistory> m_newtx;

        public AccountItem()
        {
            m_bEnabled = true;
            m_balance = 0;
            m_icon = 0;
            m_tx = 0;
            m_addressid = 0;
            m_Key = new CMicroCashKeyPair(false);
            m_txhistory = new List<AddressHistory>();
            m_newtx = new List<AddressHistory>();
        }

        
        public void GenerateKey()
        {
            m_Key = new CMicroCashKeyPair(true);
        }

        public void SetInfo(string name, string pubkey, string privkey, int balance)
        {            
            m_name=name;
            SetKeyByString(pubkey,privkey);
            m_balance = balance;            
        }
        
        public void SetKeyByString(string pubkey, string privkey)
        {
            m_Key.SetKeyByString(pubkey, privkey);            
        }
              
        public string GetAddressString()
        {
            return m_Key.GetAddressString();
        }
        public byte[] GetAddressBytes()
        {
            return m_Key.GetAddressBytes();
        }
        public string GetPubKey()
        {
            return m_Key.m_PubKeyString;
        }
        public byte[] GetPubKeyBytes()
        {
            return m_Key.m_Pub;
        }
        public string GetPrivKey()
        {
            return m_Key.m_PrivKeyString;
        }
        public Int64 GetBalance()
        {
            return m_balance;
        }

        public byte[] Sign(byte[] data)
        {
            return m_Key.Sign(data);
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

        public void AccountXMLSave(XmlTextWriter writer)
        {
            writer.WriteStartElement("account");
            writer.WriteElementString("name", m_name);
            writer.WriteElementString("icon", m_icon.ToString());
            writer.WriteElementString("numtx", m_tx.ToString());
            writer.WriteElementString("addressid", m_addressid.ToString());
            writer.WriteElementString("balance", m_balance.ToString());
            writer.WriteElementString("pubkey", GetPubKey());
            writer.WriteElementString("privkey", GetPrivKey());
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
                            m_Key.SetKeyByString(priv, pub);                            
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
    }


}