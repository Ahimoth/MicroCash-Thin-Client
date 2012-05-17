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
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using MicroCash.Client.Thin.JsonRpc;
using MicroCash.Client.Thin.JsonRpc.Contracts;

namespace MicroCash.Client.Thin
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

                MicroCashRpcClient mcrpc = CreateMCRPC();

                if (++y == 4 * 60)
                {
                    y = 0;                    
                    GetInfoRpcResponse networkInfo = mcrpc.GetInfo();
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
                foreach (Account account in m_ThinUser.m_Accounts) addresses.Add(account.GetAddressString());
                m_RPCMutex.ReleaseMutex();

                GetBalanceRpcResponse balances = mcrpc.GetBalance(addresses);
                if (mcrpc.ErrorMessage.Length > 0)
                {
                    m_RPCMutex.WaitOne();
                    m_LogItems.Add(mcrpc.ErrorMessage);
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
                        Account account = m_ThinUser.m_Accounts.Find(delegate(Account o) { return o.GetAddressString() == balance.Address; });
                        if (account == null) continue;
                        account.Balance = balance.Balance;
                        if (balance.TxCount != account.TxCount)
                        {
                            account.TxCount = balance.TxCount;
                            account.AddressId = balance.AddressID;


                            GetHistoryRpcResponse gethistory = mcrpc.GetHistory(account.GetAddressString(), 0);
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
