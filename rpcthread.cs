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
        bool m_bUpdateNow;
        

        private void RPCThread()
        {
            int y = (4 * 60) - 1;
            while (m_bRPCThreadQuit == false)
            {
                int x = 0;
                bool bUpdate = false;

                m_bUpdateNow = false;

                MicroCashRPC mcrpc = CreateMCRPC();

                if (++y == 4 * 60)
                {
                    y = 0;                    
                    GetInfo networkInfo = mcrpc.GetInfo();
                    if (networkInfo != null)
                    {
                        m_RPCMutex.WaitOne();
                        m_qTotalSC = networkInfo.MicroCashs_created;
                        m_qTotalAccounts = networkInfo.Addresses;
                        m_RPCMutex.ReleaseMutex();
                    }
                }

                List<string> addresses = new List<string>();

                m_RPCMutex.WaitOne();
                foreach (AccountItem account in m_ThinUser.m_Accounts) addresses.Add(account.GetAddressString());
                m_RPCMutex.ReleaseMutex();

                GetBalance balances = mcrpc.GetBalance(addresses);
                if (mcrpc.m_ErrorMessage.Length > 0)
                {
                    m_RPCMutex.WaitOne();
                    m_LogItems.Add(mcrpc.m_ErrorMessage);
                    m_bUpdateConnectionLog = true;
                    m_bLoggedIn = false;
                    m_RPCMutex.ReleaseMutex();
                }
                else
                {
                    if (m_bLoggedIn != true) m_bUpdateConnectionLog = true;
                    m_bLoggedIn = true;
                }
                if (balances != null)
                {
                    m_RPCMutex.WaitOne();

                    foreach (AddressBalance balance in balances.AddressBalances)
                    {
                        AccountItem account = m_ThinUser.m_Accounts.Find(delegate(AccountItem o) { return o.GetAddressString() == balance.Address; });
                        if (account == null) continue;
                        account.m_balance = balance.Balance;
                        if (balance.TxCount != account.m_tx)
                        {
                            account.m_tx = balance.TxCount;
                            account.m_addressid = balance.AddressID;


                            GetHistory gethistory = mcrpc.GetHistory(account.GetAddressString(), 0);
                            if (gethistory != null && gethistory.AddressHistory!=null)
                            {
                                account.AddTransactions(gethistory.AddressHistory);                                
                            }
                            bUpdate = true;
                        }
                    }
                    x++;
                    m_RPCMutex.ReleaseMutex();
                }

                if (bUpdate) m_bRPCThreadUpdate = true;


                for (int nSleep = 0; nSleep < 50; nSleep++)
                {
                    Thread.Sleep(100);
                    if (m_bRPCThreadQuit == true) break;
                    if (m_bUpdateNow == true) break;
                }

            }
        }

    }
}
