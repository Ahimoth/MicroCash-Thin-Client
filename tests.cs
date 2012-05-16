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
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GradientPanelCode;
using AccountItemCode;
using MicroCashLibrary;
using ThinClientUser;
using MicroCashClient;
using System.Threading;

namespace microcash
{
    public partial class Form1 : Form
    {
        public void SendTX(AccountItem account, byte[] to, Int64 qAmount)
        {
            SC_Transaction tx = new SC_Transaction();
            tx.m_dwAddressID = account.m_addressid;
            Array.Copy(account.GetPubKeyBytes(), 1, tx.m_Extra1, 0, 64);    //this isnt always sent, but may as well just copy it 
            if (tx.m_dwAddressID == 0)  tx.m_dwType = 1;
            for (int x = 0; x < 8; x++) tx.m_Info[x] = 0;
            tx.m_FromAddress = account.GetAddressBytes();
            tx.m_RecvAddress = to;
            tx.m_qAmount = qAmount*10000;

            byte[] hash = tx.GetHash(false);
            string s = MicroCashFunctions.ToHex(hash);
            tx.m_Signature = account.Sign(hash);
            
            MicroCashRPC mcrpc = CreateMCRPC();
            byte[] txserialized = tx.GetByteBuffer(true);
            if (txserialized.Length != 108)
            {
                int n = 0;
            }
            SendTransaction sendtx = mcrpc.SendTransaction(MicroCashFunctions.ToHex(txserialized));
            if (sendtx != null && sendtx.sent == true)
            {
                account.m_addressid++;
            }
            else
            {
                m_LogItems.Add("Failed to send transaction from "+account.m_name + " amount "+qAmount.ToString() + " error:"+mcrpc.m_ErrorMessage);
            }
            
        }
        public void DoTransactionTest()
        {            

            MicroCashAddress address1 = new MicroCashAddress("micro(QSM3PKS3EOZBUZO4A)cash");
            MicroCashAddress address2 = new MicroCashAddress("micro(IZ2P65UBDJKTV6ZIN)cash");
            MicroCashAddress address3 = new MicroCashAddress("micro(R7MEB3HTYHB55XLO2)cash");
            MicroCashAddress address4 = new MicroCashAddress("micro(JZBHXGUIISIID5JWS)cash");

            
            try
            {
                //feed the 3 seed accounts
                SendTX(m_ThinUser.m_Accounts[0], address2.GetAddressBytes(), 50);
                SendTX(m_ThinUser.m_Accounts[0], address3.GetAddressBytes(), 50);
                SendTX(m_ThinUser.m_Accounts[0], address4.GetAddressBytes(), 50);

                //send 50 to #3 so it has 100
                SendTX(m_ThinUser.m_Accounts[1], address3.GetAddressBytes(), 50);

                //send 100 to #4 so it has 150
                SendTX(m_ThinUser.m_Accounts[2], address4.GetAddressBytes(), 100);

                //send 50 back to #2 and #3
                SendTX(m_ThinUser.m_Accounts[3], address2.GetAddressBytes(), 50);                
                SendTX(m_ThinUser.m_Accounts[3], address3.GetAddressBytes(), 50);

                //send back
                SendTX(m_ThinUser.m_Accounts[1], address1.GetAddressBytes(), 50);
                SendTX(m_ThinUser.m_Accounts[2], address1.GetAddressBytes(), 50);
                SendTX(m_ThinUser.m_Accounts[3], address1.GetAddressBytes(), 50);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

    }
}