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
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MicroCash.Client.Thin.JsonRpc;
using MicroCash.Client.Thin.JsonRpc.Contracts;

namespace MicroCash.Client.Thin
{    
    
    public partial class Form1 : Form
    {
        private static Mutex m_RPCMutex = new Mutex();
        DateTime m_LastWWWListTry;

        Int64 m_qTotalSC;
        Int64 m_qTotalAccounts;
        internal List<ThinUser> m_ThinUsers;        
        private GradientPanel m_LeftPanel;
        private GradientPanel m_BotPanel;
        private Panel m_RightPanel;
        private GradientPanel m_UserPanel;
                     
        List<string> m_PublicNodeList;
        private List<AccountItemGUI> m_AccountItemsGUI;
        Label m_AddNewAccount;
        Label m_AddNewUser;
        
        Label m_UserText;        
        Label m_UserTotalSC;
        Label m_UserTotalInterest;
        PictureBox m_UserImage;
                               
        
        List<Bitmap> m_UserBMPList;
        List<Bitmap> m_AccountBMPList;

        Thread m_RPCThread;
        bool m_bRPCThreadQuit;
        bool m_bRPCThreadUpdate;

        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;

        ThinUser m_ThinUser;
        
        public Form1()
        {
            m_LastWWWListTry = new DateTime(1900,1,1);
            m_bUpdateNow = false;
            m_bRPCThreadQuit=false;
            m_bRPCThreadUpdate=true;
            m_qTotalSC = 0;
            m_qTotalAccounts = 0;
            m_AccountItemsGUI = null;            
            m_ThinUsers = new List<ThinUser>();
            m_PublicNodeList = new List<string>();
            string folder = ".";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folder);
            foreach (System.IO.FileInfo f in dir.GetFiles("user_*.xml"))
            {
                ThinUser newuser = new ThinUser();
                newuser.LoadFromFile(f.Name);
                m_ThinUsers.Add(newuser);
            } 
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //GetInfo networkInfo = MicroCashFunctions.GetInfo();
            //GetHistory history = MicroCashFunctions.GetHistory("sSj4DdD5MExLgGBXrHxrNEr9WCfzBd2gx9");
            //int blockNumber = 123456;
            //BlockHeader networkBlock = MicroCashFunctions.GetBlockData(blockNumber);
            //GetInputs getinputs = MicroCashFunctions.GetInputs("sNvkB6KTsTyv5xPRo9Y4aq8D4AhnZZSEJE", 1000);
           

            /*
            ThinUser newuser = new ThinUser("user");
            for (int z = 0; z < 5; z++)
            {                
                newuser.AddNewAccount("Savings Account");                    
            }
            newuser.Save();
            m_ThinUsers.Add(newuser);

            newuser = new ThinUser("Ahimoth");
            newuser.AddNewAccount("Savings Account");                    
            newuser.Save();
             * */

            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", onExit);

            trayIcon      = new NotifyIcon();
            trayIcon.Text = "MicroCash";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible     = true;


            LoadSettings();
            m_ThinUser = m_ThinUsers[0];
            
            m_UserBMPList = new List<Bitmap>();
            m_UserBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.user_general.png")));
            m_UserBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.user_guy.png")));
            m_UserBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.user_guy2.png")));
            m_UserBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.user_woman.png")));
            
            m_AccountBMPList = new List<Bitmap>();            
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_sc.png")));
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_mining.png")));
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_savings.png")));
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_checking.png")));
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_heart.png")));
            m_AccountBMPList.Add(new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.account_star.png")));


            DoConnectionPanel();
            
            m_UserPanel = new GradientPanel(Color.FromArgb(80, 80,110), Color.FromArgb(30, 30, 40),0);
            m_UserPanel.BorderStyle = BorderStyle.FixedSingle;            
            
            m_LeftPanel = new GradientPanel(Color.FromArgb(64, 64, 64),Color.FromArgb(32,32,32),0);
            m_LeftPanel.AutoScroll = true;
            m_LeftPanel.MouseEnter += General_MouseEnter;


            m_BotPanel = new GradientPanel(Color.FromArgb(80, 80, 110), Color.FromArgb(30, 30,40), 0);            
            m_BotPanel.BorderStyle = BorderStyle.FixedSingle;            
            


            m_AddNewAccount = new Label();
            m_AddNewAccount.Text = "Add Account";
            m_AddNewAccount.TextAlign = ContentAlignment.MiddleCenter;
            
            m_AddNewAccount.BorderStyle = BorderStyle.FixedSingle;
            m_AddNewAccount.BackColor = Color.Transparent;
            m_AddNewAccount.ForeColor = Color.White;
            m_AddNewAccount.MouseEnter += addMouseEnter;
            m_AddNewAccount.MouseLeave += addMouseLeave;
            m_AddNewAccount.Cursor = Cursors.Hand;
            m_AddNewAccount.Click += userNewAccountClick;
            m_BotPanel.Controls.Add(m_AddNewAccount);

            m_AddNewUser = new Label();
            m_AddNewUser.Text = "Add User";
            m_AddNewUser.TextAlign = ContentAlignment.MiddleCenter;            
            m_AddNewUser.BorderStyle = BorderStyle.FixedSingle;
            m_AddNewUser.BackColor = Color.Transparent;
            m_AddNewUser.ForeColor = Color.White;
            m_AddNewUser.MouseEnter += addMouseEnter;
            m_AddNewUser.MouseLeave += addMouseLeave;
            m_AddNewUser.Cursor = Cursors.Hand;
            m_AddNewUser.Click += userNewUserClick;            
            m_BotPanel.Controls.Add(m_AddNewUser);


            
            

            //m_LeftPanel.Controls.Add(m_Logo);

            m_RightPanel = new Panel();
            m_RightPanel.BackColor = Color.FromArgb(32,32,32);
           
            
            m_UserImage = new PictureBox();
            m_UserImage.BackColor = Color.Transparent;
            m_UserImage.Location = new Point(5, 5);
            m_UserImage.Size = new Size(70, 70);
            m_UserImage.BackgroundImageLayout = ImageLayout.Stretch;
            m_UserImage.BackgroundImage = m_UserBMPList[m_ThinUser.m_icon];
            m_UserImage.Cursor = Cursors.Hand;

            m_UserText = new Label();
            m_UserText.BackColor = Color.Transparent;
            //m_UserText.ForeColor = Color.White;
                        
            m_UserText.Font = new Font(Constants.FONTNAME, 18);
            m_UserText.Text = m_ThinUser.m_name;
            m_UserText.Cursor = Cursors.Hand;
            m_UserText.TextAlign = ContentAlignment.MiddleLeft;
            m_UserText.ForeColor = Color.White;

            

            m_UserTotalSC = new Label();
            m_UserTotalSC.BackColor = Color.Transparent;
            m_UserTotalSC.ForeColor = Color.FromArgb(192, 192, 192);
            m_UserTotalSC.Font = new Font(Constants.FONTNAMEMONO, 7, FontStyle.Bold);
            m_UserTotalSC.Text = "Balance:";
            m_UserTotalSC.Cursor = Cursors.Hand;
            m_UserTotalSC.TextAlign = ContentAlignment.MiddleLeft;
            m_UserTotalSC.AutoSize = true;

            m_UserTotalInterest = new Label();
            m_UserTotalInterest.BackColor = Color.Transparent;
            m_UserTotalInterest.ForeColor = Color.FromArgb(192, 192, 192);
            m_UserTotalInterest.Font = new Font(Constants.FONTNAMEMONO, 7, FontStyle.Bold);
            m_UserTotalInterest.Text = "Interest:";
            m_UserTotalInterest.Cursor = Cursors.Hand;
            m_UserTotalInterest.TextAlign = ContentAlignment.MiddleLeft;
            m_UserTotalInterest.AutoSize = true;


            ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(m_UserTotalSC, "This is how much MicroCash you have for all your accounts.\nThe number in brackets is your percentage of all the MicroCash currently existing");
            ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(m_UserTotalInterest, 
                "This is how much daily interest you get.\n\n"+
                "Each account in the MicroCash network requires a small daily fee,\n"+
                "These fees are then added up and given back to all the accounts proportionally\n"+
                "to the amount of MicroCash stored in each account. The more MicroCash in an\n"+
                "account the higher the percentage of interest you receive.");
                        
            m_UserPanel.MouseEnter += userMouseEnter;
            m_UserPanel.MouseLeave += userMouseLeave;
            m_UserPanel.MouseClick += userMouseClick;
            m_UserText.MouseEnter += userMouseEnter;
            m_UserText.MouseLeave += userMouseLeave;
            m_UserText.MouseClick += userMouseClick;
            m_UserTotalSC.MouseEnter += userMouseEnter;
            m_UserTotalSC.MouseLeave += userMouseLeave;
            m_UserTotalSC.MouseClick += userMouseClick;
            m_UserTotalInterest.MouseEnter += userMouseEnter;
            m_UserTotalInterest.MouseLeave += userMouseLeave;
            m_UserTotalInterest.MouseClick += userMouseClick;
            

            m_UserImage.MouseClick += userAvatarClick;


            m_UserPanel.Controls.Add(m_UserImage);            
            m_UserPanel.Controls.Add(m_UserText);            
            m_UserPanel.Controls.Add(m_UserTotalSC);
            m_UserPanel.Controls.Add(m_UserTotalInterest);
            
            

            InitTabs();
            

            //this.Controls.Add(m_LogoPanel);
            this.Controls.Add(m_LeftPanel);
            this.Controls.Add(m_RightPanel);
            this.Controls.Add(m_UserPanel);            
            this.Controls.Add(m_BotPanel);
            
            
            
            this.Resize += Form1_Resize;
            this.FormClosing += Form1_Close;
                        
            //m_AccountItemsGUI[1] = new AccountItemGUI(1, m_LeftPanel, "Checking Account", "srTGvDYJzxHR752xTC3sAzDbwRadWWWecd", "100.00");
            //m_AccountItems[1] = new AccountItem(true,1,m_LeftPanel);                                    
            switchUser();

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Tick += OnTimerEvent;
                      
            OnTimerEvent(null, null);

            m_RPCThread = new Thread(new ThreadStart(RPCThread));
            m_RPCThread.Start();

            m_RPCMutex.WaitOne();
            //for(int x=0;x<5000;x++) DoTransactionTest();
            m_RPCMutex.ReleaseMutex();
            
            //DoTransactionTest();
        }

     
        private void onExit(object sender, System.EventArgs e)
        {

        }

        private void Form1_Close(object sender, System.EventArgs e)
        {
            m_bRPCThreadQuit = true;
            m_RPCThread.Join();
            SaveSettings();
        }
        private void OnResize()
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            int nPanelSize=370;            
            ResizeConnectionPanel(nPanelSize);

            int nOffset = m_ConnectPanelTop.Location.Y + m_ConnectPanelTop.Size.Height;
            m_UserPanel.Location = new Point(0, nOffset);
            m_UserPanel.Size = new Size(nPanelSize, 80);



            m_UserText.Location = new Point(80, 5);
            m_UserText.Size = new Size(m_UserPanel.ClientSize.Width-80, 30);
            m_UserTotalSC.Location = new Point(85, 40);
            m_UserTotalInterest.Location = new Point(85, 40 + m_UserTotalSC.Size.Height);
            

            

            nOffset = m_UserPanel.Location.Y + m_UserPanel.Size.Height;
            m_LeftPanel.Location = new Point(0, nOffset);
            m_LeftPanel.Size = new Size(nPanelSize, this.ClientSize.Height - (nOffset+30));

            foreach (AccountItemGUI account in m_AccountItemsGUI)
            {
                account.Resize();
            }

            m_BotPanel.Location = new Point(0,this.ClientSize.Height - 30);
            m_BotPanel.Size = new Size(nPanelSize, 30);
            m_AddNewUser.Location = new Point(0, 0);
            m_AddNewAccount.Location = new Point(m_BotPanel.ClientSize.Width / 2, 0);
            m_AddNewAccount.Size = new Size(m_BotPanel.ClientSize.Width / 2, m_BotPanel.ClientSize.Height);
            m_AddNewUser.Size = new Size(m_BotPanel.ClientSize.Width / 2, m_BotPanel.ClientSize.Height);

            m_RightPanel.Location = new Point(nPanelSize, 0);
            m_RightPanel.Size = new Size(this.ClientSize.Width - nPanelSize, this.ClientSize.Height);
            ResizeTabs();
        }

        public void UpdateTransactionHistory()
        {
            List<AddressHistory> txlist = new List<AddressHistory>();

            string newTXOut = "";

            foreach (Account account in m_ThinUser.m_Accounts)
            {
                if (account.IsEnabled == false) continue;
                
                foreach(AddressHistory item in account.TxHistory)
                {
                    item.account = account.Name;
                    txlist.Add(item);
                }
                foreach (AddressHistory item in account.NewTx)
                {
                    string from = GetInfoFromAddressBook(false, item.fromto);
                    if (from == null) from = item.fromto;
                    if ((item.type & 6) == 6)       newTXOut += "You created MicroCash: " +DoBalanceString(item.amount) +"\n";
                    else if( (item.type&2) == 2)    newTXOut += DoBalanceString(item.amount) + " received from "+from+"\n";
                    else if( (item.type&1) == 1)    newTXOut += DoBalanceString(item.amount) + " sent to "+from+"\n";                   

                }                
                account.NewTx.Clear();
            }

            if(newTXOut.Length>0) trayIcon.ShowBalloonTip(10000, "MicroCash Transaction", newTXOut, ToolTipIcon.Info);

            txlist.Sort(delegate(AddressHistory p1, AddressHistory p2)
            {
                return p1.time.CompareTo(p2.time);
            });
            
            m_TXDetails.Items.Clear();
            foreach (AddressHistory history in txlist)
            {
                string info = "";
                string amount = MicroCashHelper.MoneyToString(history.amount);

                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime time = epoch.AddSeconds(history.time);


                if ((history.type & 0x04) == 0x04)
                {
                    info = "Currency Created";
                    amount = "+" + amount;
                }
                else
                {
                    string formattedaddr = GetInfoFromAddressBook(false, history.fromto);
                    if (formattedaddr == null) formattedaddr = history.fromto;
                    if ((history.type & 0x02) == 0x02)
                    {
                        amount = "+" + amount;
                        info = "From: " + formattedaddr;
                    }
                    else
                    {
                        amount = "-" + amount;
                        info = "To: " + formattedaddr;
                    }
                }

                string[] row1 = { amount, info, time.ToString() };
                m_TXDetails.Items.Insert(0, history.account).SubItems.AddRange(row1);
                if ((history.type & 0x02) == 0x02) m_TXDetails.Items[0].BackColor = Color.FromArgb(220, 255, 220);
                else m_TXDetails.Items[0].BackColor = Color.FromArgb(255, 200, 200);

            }
        }



        internal MicroCashRpcClient CreateMCRPC()
        {
            /*
            TimeSpan ts = DateTime.Now - m_LastWWWListTry;
            if ( ts.TotalSeconds > 10 && m_PublicNodeList.Count == 0)
            {
                m_LastWWWListTry = DateTime.Now;
                try
                {
                    string str = null;
                    using (var webClient = new System.Net.WebClient())
                    {
                        str = webClient.DownloadString("http://microcash.org/public_nodes.php");
                        string[] lines = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        m_RPCMutex.WaitOne();
                        foreach (string s in lines)
                        {
                            m_PublicNodeList.Add(s);
                        }
                        m_LogItems.Add("Successfully got MicroCash node list.");
                        m_bUpdateConnectionLog = true;
                        m_RPCMutex.ReleaseMutex();
                    }
                }
                catch (Exception ex)
                {
                    m_RPCMutex.WaitOne();
                    m_LogItems.Add("Unable to get MicroCash Node List.");
                    m_bUpdateConnectionLog = true;
                    m_RPCMutex.ReleaseMutex();                    
                }
            }
            */

            return new MicroCashRpcClient(GlobalSettings.RpcUrl);

        }

        

        public void OnTimerEvent(object source, EventArgs e)
        {
            connectionPanelTimerEvent();
            if (m_bRPCThreadUpdate == false) return;
            m_bRPCThreadUpdate = false;

            m_RPCMutex.WaitOne();
            
            m_ThinUser.Save();
            UpdateTransactionHistory();

            Int64 qTotalSC=0;
            int x = 0;
            foreach (Account account in m_ThinUser.m_Accounts)
            {
                m_AccountItemsGUI[x++].SetBalance(account.Balance);
                qTotalSC+=account.GetBalance();
                
            }
            m_UserTotalSC.Text =       " Balance: $" + (qTotalSC / 10000.0).ToString("0.00");
            if (m_qTotalSC != 0)
            {
                m_UserTotalSC.Text += " (" + ((qTotalSC/(double)m_qTotalSC)*100).ToString("0.####") + "%)";
            }

            if (m_qTotalAccounts != 0)
            {
                Int64 qDailyInterest = 0;
                foreach (Account account in m_ThinUser.m_Accounts)
                {
                    if (account.Balance <= 0) continue;
                    qDailyInterest += -50 + (Int64)(((double)account.Balance / m_qTotalSC) * (m_qTotalAccounts * 50));
                }

                if (qDailyInterest < 0)
                {
                    m_UserTotalInterest.Text = "Interest: (" + DoBalanceString(-qDailyInterest) + ")";
                    m_UserTotalInterest.ForeColor = Color.FromArgb(255, 160, 160);
                }
                else
                {
                    m_UserTotalInterest.Text = "Interest: " + (qDailyInterest / 10000.0).ToString("0.0000");
                    m_UserTotalInterest.ForeColor = Color.FromArgb(200, 200, 255);
                }
            }

            m_RPCMutex.ReleaseMutex();
        }

        string DoBalanceString(Int64 qBalance)
        {
            if (qBalance < 0) return "-$" + (-qBalance / 10000.0).ToString("0.0000");
            return "$" + (qBalance / 10000.0).ToString("0.0000");
            
            
        }
      

        
        

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            OnResize();         
        }

        private void userMouseEnter(object sender, EventArgs e)
        {
            m_UserPanel.SetColor(Color.FromArgb(100, 100, 240), Color.FromArgb(60, 60, 160));
        }
        private void userMouseLeave(object sender, EventArgs e)
        {
            m_UserPanel.SetColor(Color.FromArgb(80, 80, 110), Color.FromArgb(50, 50, 64));
        }

        private void addMouseEnter(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.BackColor = Color.FromArgb(80, 80, 220);
        }
        private void addMouseLeave(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.BackColor = Color.Transparent;
        }

        private void CreateUserGUIAccounts()
        {
            if (m_AccountItemsGUI != null)
            {
                m_AccountItemsGUI.Clear();
            }
            m_AccountItemsGUI = new List<AccountItemGUI>();
            int x = 0;
            foreach (Account account in m_ThinUser.m_Accounts)
            {
                account.TxCount = 0;
                m_AccountItemsGUI.Add(new AccountItemGUI(x, this, m_LeftPanel, m_AccountBMPList, account.Name, account.GetAddressString(), account.GetBalance(), account.IconId, account.IsEnabled));
                //m_AccountSendItemsGUI.Add(new AccountSendItemGUI(x,m_TabControl.TabPages[0],account) );

                x++;
            }   

            DoUserAccounts();
        }
     
        private void DoUserAccounts()
        {

            int x=0;
            foreach (Account account in m_ThinUser.m_Accounts)
            {
                m_AccountItemsGUI[x].SetName(account.Name);
                x++;
            }

            DoTabAccounts();
            /*
            if (m_AccountSendItemsGUI != null)
            {
                foreach (AccountSendItemGUI actgui in m_AccountSendItemsGUI)
                {
                    actgui.Dispose();
                }
                m_AccountSendItemsGUI.Clear();
            }
            m_AccountSendItemsGUI = new List<AccountSendItemGUI>(); 
             * */

        }
        private void switchUser()
        {

            m_UserText.Text = m_ThinUser.m_name;       
            m_UserImage.BackgroundImage = m_UserBMPList[m_ThinUser.m_icon];

            for (int ix = m_LeftPanel.Controls.Count - 1; ix >= 0; ix--)
                m_LeftPanel.Controls[ix].Dispose();

            CreateUserGUIAccounts();
            DoUserAccounts();
            OnResize();
            m_bUpdateNow = true;
        }

        private void userMouseClick(object sender, EventArgs e)
        {
            EventHandler onClick = new EventHandler(delegate(object s2, EventArgs e2)
            {
                MenuItem mi = (MenuItem)s2;
                m_RPCMutex.WaitOne();
                m_ThinUser = m_ThinUsers[mi.Index];
                m_RPCMutex.ReleaseMutex();
                switchUser();

            });
            ImageMenu menu = new ImageMenu(this, m_UserText.Font, onClick);
            foreach (ThinUser user in m_ThinUsers)
            {
                menu.AddItem(m_UserBMPList[user.m_icon], user.m_name);
            }
            menu.Show();
            
        }
        

        private void userAvatarClick(object sender, EventArgs e)
        {
            EventHandler onClick = new EventHandler(delegate(object s2, EventArgs e2)
            {
                MenuItem mi = (MenuItem)s2;
                m_ThinUser.m_icon = mi.Index;
                m_UserImage.BackgroundImage = m_UserBMPList[m_ThinUser.m_icon];
                SaveAccount();
            });
            ImageMenu menu = new ImageMenu(this,null, onClick);
            foreach (Bitmap bmp in m_UserBMPList)
            {
                menu.AddItem(bmp, "");
            }
            menu.Show();            
        }

        private void userNewAccountClick(object sender, EventArgs e)
        {
            
            EventHandler onClick = new EventHandler(delegate(object s2, EventArgs e2)
            {
                MenuItem mi = (MenuItem)s2;
                switch (mi.Text)
                {
                    case "Create Normal Account":
                        {
                            m_RPCMutex.WaitOne();
                            m_ThinUser.AddNewAccount("New Account");
                            m_ThinUser.Save();
                            m_RPCMutex.ReleaseMutex();
                            CreateUserGUIAccounts();
                        }
                        break;
                    case "Repair Transaction History": RepairTransactionHistory(); break;
                    default: break;
                }
                //m_ThinUser.m_icon = mi.Index;
                //m_UserImage.BackgroundImage = m_UserBMPList[m_ThinUser.m_icon];
                //SaveAccount();
            });

            ImageMenu menu = new ImageMenu(this, new Font(Constants.FONTNAME,12), onClick);

            menu.AddItem(null, "Create Normal Account");
            menu.AddItem(null, "Create 2 Key Account");
            menu.AddItem(null, "Create Account with existing keys");
            menu.AddSeparator();
            menu.AddItem(null, "Repair Transaction History");
            menu.Show();

            /*
            
             */
        }

        private void userNewUserClick(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            if (f2.ShowDialog() == DialogResult.OK)
            {
                addNewUser(f2.m_user, f2.m_pass1, f2.m_pass2);
            }
        }

        public void addNewUser(string name, string pass1, string pass2)
        {
            ThinUser newuser = new ThinUser();
            newuser.Create(name, pass1, pass2);
            newuser.AddNewAccount("Savings Account");
            newuser.Save();
            m_ThinUsers.Add(newuser);

        }
        

       
        private void General_MouseEnter(object sender, System.EventArgs e)
        {
            m_LeftPanel.Focus();
        }

        void RepairTransactionHistory()
        {
            m_RPCMutex.WaitOne();

            foreach (Account account in m_ThinUser.m_Accounts)
            {
                account.TxHistory.Clear();
                account.NewTx.Clear();
                account.TxCount = 0;
                account.Balance = 0;
                account.AddressId=0;
            }
            DoUserAccounts();
            m_ThinUser.Save();
            m_RPCMutex.ReleaseMutex();

            UpdateTransactionHistory();
        }

        public string DoAccountName(int nAccount, string name)
        {
            m_ThinUser.m_Accounts[nAccount].Name = name;
            DoUserAccounts();
            SaveAccount();
            UpdateTransactionHistory();
            return name;
        }
        public void SaveAccount()
        {
            m_RPCMutex.WaitOne();
            m_ThinUser.Save();
            m_RPCMutex.ReleaseMutex();
        }
        public void ToggleAccountEnabled(int nAccount)
        {
            m_ThinUser.m_Accounts[nAccount].IsEnabled = !m_ThinUser.m_Accounts[nAccount].IsEnabled;
            m_AccountItemsGUI[nAccount].SetEnabled(m_ThinUser.m_Accounts[nAccount].IsEnabled);
            SaveAccount();
            UpdateTransactionHistory();
        }
        public void DoAccountIconMenu(int nAccount)
        {
            EventHandler onClick=new EventHandler(delegate(object sender, EventArgs e)
                {
                    MenuItem mi = (MenuItem)sender;
                    m_ThinUser.m_Accounts[nAccount].IconId = mi.Index;
                    m_AccountItemsGUI[nAccount].SetIcon(mi.Index);
                    SaveAccount();
                });
            ImageMenu menu = new ImageMenu(this,null, onClick);
            foreach (Bitmap bmp in m_AccountBMPList)
            {
                menu.AddItem(bmp,"");
            }                       
            menu.Show();
        }

        public void LoadSettings()
        {
            GlobalSettings.ConnectionType = -1;
            GlobalSettings.RpcAddress = "127.0.0.1:8555";
                        
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader("settings.xml");
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            switch (reader.Name)
                            {
                                case "connecttype": reader.MoveToContent(); GlobalSettings.ConnectionType = reader.ReadElementContentAsInt(); break;
                                case "connectip": reader.MoveToContent(); GlobalSettings.RpcAddress = reader.ReadElementContentAsString(); break;                                
                            }
                            break;
                        case XmlNodeType.EndElement:
                            break;
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }
            catch (Exception exception)
            {
                MessageBox.Show(String.Format("Error reading settings.xml: {0}", exception.Message));
            }
            finally
            {
                reader.Close();
            }
        }

        public bool SaveSettings()
        {
            XmlTextWriter writer = null;

            try
            {
                writer = new XmlTextWriter("settings.xml", System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");

                writer.WriteElementString("connecttype", GlobalSettings.ConnectionType.ToString());
                writer.WriteElementString("connectip", GlobalSettings.RpcAddress);
               

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to write settings.xml");
                return false;
            }
            finally
            {
                writer.Close();
            }


            return true;
        }

        public static Bitmap Grayscale(Bitmap bitmap)
        {
            Bitmap myBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int x = 0; x < bitmap.Height; x++)
                {
                    Color BitmapColor = bitmap.GetPixel(i, x);
                    int grayScale = (int)((BitmapColor.R * 0.3) + (BitmapColor.G * 0.59) + (BitmapColor.B * 0.11));
                    Color myColor = Color.FromArgb(BitmapColor.A / 2, grayScale, grayScale, grayScale);
                    myBitmap.SetPixel(i, x, myColor);
                }
            }
            return myBitmap;
        }

        private void Form1_Resize_1(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        } 
        
        

    }

    static class Constants
    {
        public const string FONTNAME = "Arial";
        public const string FONTNAMEMONO = "Courier New";
    }

    public class AccountItemGUI: IDisposable
    {
        int m_AccountNumber;
        int m_icon;
        Int64 m_balance;
        Panel m_Parent;
        GradientPanel m_Inner;
        Label m_AccountName;
        Label m_Address;
        Label m_Amount;
        Form1 m_ParentForm;
        PictureBox m_Image;
        List<Bitmap> m_IconBmpList;
        TextBox m_EditName;
        bool m_bEnabled;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Parent != null)
                    m_Parent.Dispose();
                if (m_Inner != null)
                    m_Inner.Dispose();
                if (m_AccountName != null)
                    m_AccountName.Dispose();
                if (m_Address != null)
                    m_Address.Dispose();
                if (m_Amount != null)
                    m_Amount.Dispose();
                if (m_ParentForm != null)
                    m_ParentForm.Dispose();
                if (m_Image != null)
                    m_Image.Dispose();
                if (m_IconBmpList != null)
                    foreach (Bitmap bm in m_IconBmpList)
                        bm.Dispose();
                if (m_EditName != null)
                    m_EditName.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public AccountItemGUI(int nNumber, Form1 ParentForm, Panel parentpanel, List<Bitmap> IconBmp, string name, string address, Int64 balance, int icon, bool bEnabled)
        {            
            m_IconBmpList = IconBmp;
            m_ParentForm=ParentForm;
            m_AccountNumber = nNumber;
            m_Parent= parentpanel;
            
            m_icon = icon;
            
            m_Inner = new GradientPanel(Color.FromArgb(96, 96, 96),Color.FromArgb(16, 16, 16),0);            
            m_Inner.Location = new Point(0, (nNumber*70));
            m_Inner.BorderStyle = BorderStyle.FixedSingle;
            m_Inner.MouseEnter += General_MouseEnter;

            
                                    
            m_Image = new PictureBox();
            m_Image.BackColor = Color.Transparent;            
            m_Image.Image = m_IconBmpList[m_icon];
            m_Image.Cursor = Cursors.Hand;
            m_Image.MouseClick += iconMouseClick;
            m_Image.MouseEnter += iconMouseEnter;
            m_Image.MouseLeave += iconMouseLeave;
            
            m_AccountName = new Label();
            m_AccountName.BackColor = Color.Transparent;
            m_AccountName.ForeColor = Color.White;
            m_AccountName.Font = new Font(Constants.FONTNAME, 12);
            m_AccountName.Text = name;
            m_AccountName.TextAlign = ContentAlignment.MiddleLeft;
            m_AccountName.AutoEllipsis = true;
            m_AccountName.MouseEnter += accountMouseEnter;
            m_AccountName.MouseLeave += accountMouseLeave;
            m_AccountName.MouseClick += accountMouseClick;

            m_EditName = new TextBox();
            m_EditName.Text = name;
            m_EditName.Font = new Font(Constants.FONTNAME, 12);
            m_EditName.BorderStyle = BorderStyle.None;
            m_EditName.Visible = false;
            m_EditName.BackColor = Color.FromArgb(192,192,192);
            //m_EditName.ForeColor = Color.f;
            m_EditName.LostFocus += onEditLostFocus;
            m_EditName.KeyDown += KeyDown;
                        
            m_Address = new Label();            
            m_Address.Font = new Font("Courier New", 9);
            m_Address.BackColor = Color.Transparent;
            m_Address.ForeColor = Color.FromArgb(192,192,192);
            m_Address.Text = address;
            m_Address.AutoEllipsis = true;
            m_Address.TextAlign = ContentAlignment.MiddleLeft;
            m_Address.MouseEnter += addressMouseEnter;
            m_Address.MouseLeave += addressMouseLeave;
            m_Address.MouseClick += addressMouseClick;
            m_Address.MouseDoubleClick += addressMouseClick;

            

            ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(m_Address, "Click to copy address");
            
            m_Amount = new Label();
            
            m_Amount.BackColor = Color.Transparent;
            m_Amount.Font = new Font(Constants.FONTNAME, 12);
            m_Amount.TextAlign = ContentAlignment.MiddleLeft;
            m_Amount.AutoEllipsis = true;
            m_Amount.MouseEnter += General_MouseEnter;
            m_Amount.TextAlign = ContentAlignment.MiddleRight;
            m_Amount.AutoSize = true;
            m_Amount.ForeColor = Color.FromArgb(255,255,220);

            m_Inner.Controls.Add(m_Image);
            m_Inner.Controls.Add(m_EditName);
            m_Inner.Controls.Add(m_AccountName);            
            
            m_Inner.Controls.Add(m_Address);
            m_Inner.Controls.Add(m_Amount);

            SetBalance(balance);
            SetEnabled(bEnabled);
            m_Parent.Controls.Add(m_Inner);

            Resize();
        }

        public void SetIcon(int icon)
        {
            m_icon = icon;
            m_Image.Image = m_IconBmpList[m_icon];
        }

        public void SetEnabled(bool bEnabled)
        {
            m_bEnabled = bEnabled;

            if (m_bEnabled)
            {
                m_Inner.SetColor(Color.FromArgb(96, 96, 96), Color.FromArgb(16, 16, 16));
                m_Image.Image = m_IconBmpList[m_icon];

            }
            else
            {
                m_Inner.SetColor(Color.FromArgb(160, 160, 160), Color.FromArgb(140, 140, 140));
                m_Image.Image = Form1.Grayscale(m_IconBmpList[m_icon]);
            }
        }
        public void SetBalance(Int64 balance)
        {
            m_balance = balance;
            m_Amount.Text = "$" + (m_balance / 10000.0).ToString("0.0000");
            Resize();
        }

        public void SetName(string name)
        {
            m_AccountName.Text = name;
        }
        
        public void Resize()
        {
            int w = m_Parent.ClientSize.Width;
                        
            m_Inner.Size = new Size(w, 70);

            w = m_Inner.ClientSize.Width;
            
            m_Image.Location = new Point(5, 10);
            m_Image.Size = new Size(50, 50);
            m_AccountName.Location = new Point(60, 10);
            m_AccountName.Size = new Size(w - (65+m_Amount.Size.Width), 30);

            m_EditName.Size = m_AccountName.Size;
            m_EditName.Location = new Point(65, 10 + ((30 - m_EditName.Size.Height) / 2));
            

            m_Address.Location = new Point(65, 40);
            m_Address.Size = new Size(w - 70, 20);
            m_Amount.Location = new Point(w - m_Amount.Size.Width, 14);
            //m_Amount.Size = new Size(130,30);            

        }

        private void iconMouseEnter(object sender, EventArgs e)
        {
            if (m_bEnabled) m_Inner.SetColor(Color.FromArgb(60, 60, 160), Color.FromArgb(100, 100, 240));
            if (m_EditName.Visible == false) m_Parent.Focus();
        }
        private void iconMouseLeave(object sender, EventArgs e)
        {
            if(m_bEnabled) m_Inner.SetColor(Color.FromArgb(96, 96, 96), Color.FromArgb(16, 16, 16));
        }
        private void iconMouseClick(object sender, EventArgs e)
        {
            //m_ParentForm.DoAccountIconMenu(m_AccountNumber);                        
            m_ParentForm.ToggleAccountEnabled(m_AccountNumber);            
        }
        
        private void addressMouseEnter(object sender, EventArgs e)
        {
            m_Address.ForeColor = Color.FromArgb(255,255, 255);
            m_Address.BackColor = Color.FromArgb(80,80,220);
            m_Address.Cursor = Cursors.Hand;
            if (m_EditName.Visible == false) m_Parent.Focus();
            
        }
        private void addressMouseLeave(object sender, EventArgs e)
        {
            m_Address.ForeColor = Color.FromArgb(192, 192, 192);
            m_Address.BackColor = Color.Transparent;
            m_Address.Cursor = Cursors.Arrow;
        }
        private void addressMouseClick(object sender, EventArgs e)
        {
            Clipboard.SetText(m_Address.Text);
        }

        private void onEditLostFocus(object sender, EventArgs e)
        {
            m_EditName.Visible = false;
            m_AccountName.Visible = true;

            string ret = m_ParentForm.DoAccountName(m_AccountNumber, m_EditName.Text);
            m_AccountName.Text = ret;
            m_EditName.Text = ret;
        }

        private void KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) onEditLostFocus(null, null);            
        }

        private void accountMouseEnter(object sender, EventArgs e)
        {
            m_AccountName.ForeColor = Color.FromArgb(255, 255, 255);
            m_AccountName.BackColor = Color.FromArgb(80, 80, 220);
            m_AccountName.Cursor = Cursors.IBeam;
            if (m_EditName.Visible == false) m_Parent.Focus();

        }
        private void accountMouseLeave(object sender, EventArgs e)
        {
            m_AccountName.ForeColor = Color.White;
            m_AccountName.BackColor = Color.Transparent;
            m_AccountName.Cursor = Cursors.Arrow;
        }
        private void accountMouseClick(object sender, EventArgs e)
        {
            m_EditName.Visible = true;
            m_AccountName.Visible = false;
            m_EditName.Focus();
            
        }

        private void General_MouseEnter(object sender, System.EventArgs e)
        {
            if(m_EditName.Visible==false)   m_Parent.Focus();
        }

    }
}
