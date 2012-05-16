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
using System.Runtime.Serialization;
using System.Threading;

namespace microcash
{
    public partial class Form1 : Form
    {
        ListView m_MCBuyList;
        ListView m_MCSellList;
        Label m_MCBuyLabel;
        Label m_MCSellLabel;
        Thread m_ExchangeThread;

        List<Exchange_Pair> m_MCExchangeBuys;
        List<Exchange_Pair> m_MCExchangeSells;
        private static Mutex m_ExchangeMutex;
        bool m_bMCExchangeUpdate;
        bool m_bMCExchangeGUIUpdate;

        void DoExchangePage()
        {
            m_MCExchangeBuys = new List<Exchange_Pair>();
            m_MCExchangeSells = new List<Exchange_Pair>();
            m_ExchangeMutex= new Mutex();
            TabPage page = m_TabControl.TabPages[3];
            m_bMCExchangeUpdate = true;
            m_bMCExchangeGUIUpdate = false;

            m_MCBuyLabel = new Label();
            m_MCBuyLabel.Text = "MicroCash Buy Orders";
            m_MCBuyLabel.TextAlign = ContentAlignment.MiddleCenter;

            m_MCBuyList = new ListView();
            m_MCBuyList.View = System.Windows.Forms.View.Details;
            m_MCBuyList.FullRowSelect = true;
            m_MCBuyList.Columns.Add("Amount");
            m_MCBuyList.Columns.Add("Price");

            m_MCSellLabel = new Label();
            m_MCSellLabel.Text = "MicroCash Sell Orders";
            m_MCSellLabel.TextAlign = ContentAlignment.MiddleCenter;

            m_MCSellList = new ListView();
            m_MCSellList.View = System.Windows.Forms.View.Details;
            m_MCSellList.FullRowSelect = true;
            m_MCSellList.Columns.Add("Price");
            m_MCSellList.Columns.Add("Amount");


            page.Controls.Add(m_MCBuyLabel);
            page.Controls.Add(m_MCBuyList);
            page.Controls.Add(m_MCSellLabel);
            page.Controls.Add(m_MCSellList);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += OnExchangeGUIUpdate;

            m_ExchangeThread = new Thread(new ThreadStart(RefreshThreadExchangeOrders));
            m_ExchangeThread.Start();            
        }

        void ResizeExchangePage()
        {
            int w = m_TabControl.ClientSize.Width;
            int h = m_TabControl.ClientSize.Height;

            int nOffset = 30;
            m_MCBuyLabel.Location = new Point(10, nOffset);
            m_MCBuyLabel.Size = new Size((w / 2) - 15, 30);

            m_MCSellLabel.Location = new Point(m_MCBuyLabel.Location.X + m_MCBuyLabel.Size.Width, nOffset);
            m_MCSellLabel.Size = new Size((w / 2) - 15, 30);

            nOffset += 30;
            m_MCBuyList.Location = new Point(10, nOffset);
            m_MCBuyList.Size = new Size(m_MCBuyLabel.Size.Width, h - 230);
            m_MCBuyList.Columns[0].Width = (m_MCBuyList.ClientSize.Width / 10) * 6;
            m_MCBuyList.Columns[1].Width = (m_MCBuyList.ClientSize.Width / 10) * 4;

            m_MCSellList.Location = new Point(m_MCSellLabel.Location.X, nOffset);
            m_MCSellList.Size = new Size(m_MCSellLabel.Size.Width, m_MCBuyList.Size.Height);
            m_MCSellList.Columns[0].Width = (m_MCSellList.ClientSize.Width / 10) * 4;
            m_MCSellList.Columns[1].Width = (m_MCSellList.ClientSize.Width / 10) * 6;

        }

        public void OnExchangeGUIUpdate(object source, EventArgs e)
        {
            if (m_bMCExchangeGUIUpdate == false)
            {
                return;
            }
            m_MCBuyList.Items.Clear();
            m_MCSellList.Items.Clear();

            double dBTCAmount=0;
            double dMCAmount = 0;

            m_ExchangeMutex.WaitOne();
            m_bMCExchangeGUIUpdate = false;
            foreach (Exchange_Pair pair in m_MCExchangeBuys)
            {
                string[] row1 = { pair.price.ToString("0.######") + " btc" };
                m_MCBuyList.Items.Insert(0, pair.amount.ToString("0.####")).SubItems.AddRange(row1);
                dBTCAmount+=pair.amount*pair.price;
            }
            foreach (Exchange_Pair pair in m_MCExchangeSells)
            {
                string[] row1 = { pair.amount.ToString("0.####") };
                m_MCSellList.Items.Insert(0, pair.price.ToString("0.######")+" btc").SubItems.AddRange(row1);                    
                dMCAmount+=pair.amount;
            }

            m_MCBuyLabel.Text = "Buy Orders (" + dBTCAmount .ToString("0.##")+ " BTC)";
            m_MCSellLabel.Text = "Sell Orders (" + dMCAmount.ToString("0.##") + " MCD)";
            m_ExchangeMutex.ReleaseMutex();

        }

        void RefreshThreadExchangeOrders()
        {
            while (m_bRPCThreadQuit == false)
            {
                if(m_bMCExchangeUpdate==false)
                {
                    Thread.Sleep(100);
                    continue;
                }
                object[] parameters = { null };
                //JsonResponse<Exchange_SLC24> response = JsonHelper.GetObjectFromJsonRPC<Exchange_SLC24>("https://slc24.com", "api/exchange/orderbook/btc", null, null, parameters);
                Exchange_SLC24 response = null;
                Exchange_BTCE response2 = null;
                try
                {
                    response = ((Exchange_SLC24)JsonHelper.GetObjectFromHTML<Exchange_SLC24>("https://slc24.com/api/exchange/orderbook/btc"));
                    response2 = ((Exchange_BTCE)JsonHelper.GetObjectFromHTML<Exchange_BTCE>("https://btc-e.com/api/2/5/depth"));
                }
                catch(Exception e)
                {
                }

                m_ExchangeMutex.WaitOne();
                m_bMCExchangeUpdate = false;
                if (response != null || response2 != null)
                {
                    m_MCExchangeBuys.Clear();
                    m_MCExchangeSells.Clear();
                    m_bMCExchangeGUIUpdate = true;
                }
                if (response!=null)
                {
                    m_MCExchangeBuys.AddRange(response.GetBuyList());
                    m_MCExchangeSells.AddRange(response.GetSellList());
                }
                if (response2 != null)
                {
                    m_MCExchangeBuys.AddRange(response2.GetBuyList());
                    m_MCExchangeSells.AddRange(response2.GetSellList());
                }


                m_MCExchangeBuys.Sort(delegate(Exchange_Pair p1, Exchange_Pair p2)
                    {
                        return p1.price.CompareTo(p2.price);
                    });
                m_MCExchangeSells.Sort(delegate(Exchange_Pair p1, Exchange_Pair p2)
                {
                    return p2.price.CompareTo(p1.price);
                });
                m_ExchangeMutex.ReleaseMutex();
            }                        

        }


        public class Exchange_Pair
        {
            public Exchange_Pair() 
            { 
            }
                        
            public double price;
            public double amount;            

        }

        [DataContract]
        public class Exchange_Pair1
        {
            public Exchange_Pair1() { }
            [DataMember(Name = "price")]
            public double price { get; set; }
            [DataMember(Name = "amount")]
            public double amount { get; set; }
        }
        


        [DataContract]
        public class Exchange_BTCE
        {
            public Exchange_BTCE() { }
            [DataMember(Name = "bids")]
            public List<double[]> buys { get; set; }
            [DataMember(Name = "asks")]
            public List<double[]> sells { get; set; }
            public List<Exchange_Pair> GetBuyList()
            {
                List<Exchange_Pair> ret = new List<Exchange_Pair>();
                foreach(double[] pair in buys)
                {
                    Exchange_Pair item = new Exchange_Pair();
                    item.amount = pair[1];
                    item.price = pair[0];
                    ret.Add(item);                                         
                }                                 
                return ret;
            }
            public List<Exchange_Pair> GetSellList()
            {
                List<Exchange_Pair> ret = new List<Exchange_Pair>();
                foreach(double[] pair in sells)
                {
                    Exchange_Pair item = new Exchange_Pair();
                    item.amount = pair[1];
                    item.price = pair[0];
                    ret.Add(item);                                         
                }                                 
                return ret;
            }
        }

        [DataContract]
        public class Exchange_SLC24
        {
            public Exchange_SLC24() { }
            [DataMember(Name = "bid")]
            public List<Exchange_Pair1> buys { get; set; }
            [DataMember(Name = "ask")]
            public List<Exchange_Pair1> sells { get; set; }

            public List<Exchange_Pair> GetBuyList()
            {
                List<Exchange_Pair> ret = new List<Exchange_Pair>();
                foreach(Exchange_Pair1 pair in buys)
                {
                    Exchange_Pair item = new Exchange_Pair();
                    item.amount = pair.amount;
                    item.price = pair.price;
                    ret.Add(item);                                         
                }                                 
                return ret;
            }
            public List<Exchange_Pair> GetSellList()
            {
                List<Exchange_Pair> ret = new List<Exchange_Pair>();
                foreach(Exchange_Pair1 pair in sells)
                {
                    Exchange_Pair item = new Exchange_Pair();
                    item.amount = pair.amount;
                    item.price = pair.price;
                    ret.Add(item);                                         
                }                                 
                return ret;
            }
        }
    }
}
