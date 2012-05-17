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


namespace MicroCash.Client.Thin
{

    public partial class Form1 : Form
    {
        List<string> m_LogItems;
        bool m_bUpdateConnectionLog;
        bool m_bLoggedIn;
        bool m_bLoggedInGUI;

        private GradientPanel m_ConnectPanelTop;
        Label m_ConnectInfo;
        PictureBox m_ConnectLogo;
        PictureBox m_ConnectionArrow;
        Bitmap m_ConnectArrowBMP;
        Bitmap m_ConnectLogoBMP;
        GradientPanel m_ConnectPanel;
        ComboBox m_ConnectType;
        Label m_ConnectTypeInfo;
        TextBox m_ConnectTypeIP;
        ListView m_ConnectLog;

        void DoConnectionPanel()
        {
            m_bUpdateConnectionLog = false;
            m_bLoggedInGUI = m_bLoggedIn = false;
            

            m_LogItems = new List<string>();

            m_ConnectPanelTop = new GradientPanel(Color.FromArgb(120, 30, 30), Color.FromArgb(255, 80, 80), 0);
            m_ConnectPanelTop.BorderStyle = BorderStyle.FixedSingle;
            m_ConnectPanelTop.Cursor = Cursors.Hand;

            m_ConnectPanel = new GradientPanel(Color.FromArgb(255, 255, 255), Color.FromArgb(192, 192, 192), 0);
            m_ConnectPanel.BorderStyle = BorderStyle.FixedSingle;            
            m_ConnectPanel.Visible = false;

            m_ConnectLogoBMP = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.logosmall.png"));
            m_ConnectLogo = new PictureBox();
            m_ConnectLogo.BackColor = Color.Transparent;
            m_ConnectLogo.Location = new Point(3, 2);
            m_ConnectLogo.Size = new Size(200, 40);
            m_ConnectLogo.Image = Grayscale(m_ConnectLogoBMP);
            m_ConnectLogo.Cursor = Cursors.Hand;

            m_ConnectArrowBMP = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.downarrow.png"));
            m_ConnectionArrow = new PictureBox();
            m_ConnectionArrow.BackColor = Color.Transparent;
            m_ConnectionArrow.Location = new Point(8, 5);
            m_ConnectionArrow.Size = new Size(108, 30);
            m_ConnectionArrow.Image = m_ConnectArrowBMP;
            m_ConnectionArrow.Cursor = Cursors.Hand;
            

            m_ConnectInfo = new Label();
            m_ConnectInfo.BackColor = Color.Transparent;
            m_ConnectInfo.ForeColor = Color.White;            
            m_ConnectInfo.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_ConnectInfo.Text = "Connecting...";
            m_ConnectInfo.Cursor = Cursors.Hand;
            m_ConnectInfo.TextAlign = ContentAlignment.MiddleLeft;

            m_ConnectType = new ComboBox();
            m_ConnectType.DropDownStyle = ComboBoxStyle.DropDownList;
            m_ConnectType.Font = new Font(Constants.FONTNAME, 8,FontStyle.Bold);
            m_ConnectType.Items.Add(" Connect: MicroCash Official Nodes");
            m_ConnectType.Items.Add(" Connect: Specific MicroCash Node");
            m_ConnectType.SelectedIndexChanged += onConnectTypeChange;

            m_ConnectTypeInfo = new Label();
            m_ConnectTypeInfo.Padding = new Padding(10);
            m_ConnectTypeInfo.BackColor = Color.White;
            m_ConnectTypeInfo.BorderStyle = BorderStyle.FixedSingle;

            m_ConnectTypeIP = new TextBox();
            
            m_ConnectTypeIP.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);


            m_ConnectLog = new ListView();
            m_ConnectLog.BackColor = Color.FromArgb(192, 192, 192);
            m_ConnectLog.Columns.Add("log item");
            m_ConnectLog.FullRowSelect = true;
            m_ConnectLog.View = System.Windows.Forms.View.Details;
            m_ConnectLog.ShowItemToolTips = true;
            m_ConnectLog.HeaderStyle = ColumnHeaderStyle.None;

            m_ConnectInfo.MouseEnter += connectMouseEnter;
            m_ConnectInfo.MouseLeave += connectMouseLeave;
            m_ConnectInfo.MouseDown += connectMouseClick;
            m_ConnectPanelTop.MouseEnter += connectMouseEnter;
            m_ConnectPanelTop.MouseLeave += connectMouseLeave;
            m_ConnectPanelTop.MouseDown += connectMouseClick;
            m_ConnectLogo.MouseEnter += connectMouseEnter;
            m_ConnectLogo.MouseLeave += connectMouseLeave;
            m_ConnectLogo.MouseDown += connectMouseClick;
            m_ConnectionArrow.MouseEnter += connectMouseEnter;
            m_ConnectionArrow.MouseLeave += connectMouseLeave;
            m_ConnectionArrow.MouseDown += connectMouseClick;
            
            m_ConnectPanelTop.Controls.Add(m_ConnectionArrow);
            m_ConnectPanelTop.Controls.Add(m_ConnectInfo);
            m_ConnectPanelTop.Controls.Add(m_ConnectLogo);



            m_ConnectPanel.Controls.Add(m_ConnectLog);
            m_ConnectPanel.Controls.Add(m_ConnectTypeIP);
            m_ConnectPanel.Controls.Add(m_ConnectType);
            m_ConnectPanel.Controls.Add(m_ConnectTypeInfo);

            this.Controls.Add(m_ConnectPanelTop);
            this.Controls.Add(m_ConnectPanel);

            if (GlobalSettings.ConnectionType < 0 || GlobalSettings.ConnectionType > 1)
                GlobalSettings.ConnectionType = 0;
            m_ConnectType.SelectedIndex = GlobalSettings.ConnectionType;
            m_ConnectTypeIP.Text = GlobalSettings.RpcAddress;
            m_ConnectTypeIP.Validating += new CancelEventHandler(m_ConnectTypeIP_Validating);
            //selectConnectType(0);

            AddLogItem("Initializing client");
            
        }

        void m_ConnectTypeIP_Validating(object sender, CancelEventArgs e)
        {
            GlobalSettings.RpcAddress = m_ConnectTypeIP.Text;
        }

        void ResizeConnectionPanel(int nPanelWidth)
        {
            m_ConnectPanelTop.Size = new Size(nPanelWidth, 46);
            m_ConnectionArrow.Location = new Point(nPanelWidth - 30, 10);
            m_ConnectionArrow.Size = new Size(20, 20);

            int nOffset = m_ConnectPanelTop.Location.Y + m_ConnectPanelTop.Size.Height;

            m_ConnectPanel.Location = new Point(0, nOffset);
            m_ConnectPanel.Size = new Size(nPanelWidth, this.ClientSize.Height - nOffset);
            
            m_ConnectTypeInfo.Location = new Point(10, 10);
            m_ConnectTypeInfo.Size = new Size(m_ConnectPanel.ClientSize.Width - 20, 60);

            nOffset = 20 + 60;

            m_ConnectType.Location = new Point(10, nOffset);
            m_ConnectType.Size = new Size(m_ConnectPanel.ClientSize.Width - 20, 0);

            nOffset+= m_ConnectType.Size.Height;
            m_ConnectTypeIP.Location = new Point(10,nOffset);
            m_ConnectTypeIP.Size = new Size(m_ConnectPanel.ClientSize.Width - 20,0);

            nOffset+= 10 + m_ConnectTypeIP.Size.Height;
            m_ConnectLog.Location = new Point(10, nOffset);
            m_ConnectLog.Size = new Size(m_ConnectPanel.ClientSize.Width - 20, m_ConnectPanel.ClientSize.Height- (nOffset+10) );

            m_ConnectLog.Columns[0].Width = m_ConnectLog.ClientSize.Width - 30;


            
            m_ConnectInfo.Location = new Point(205, 5);
            m_ConnectInfo.Size = new Size(m_ConnectPanelTop.ClientSize.Width, 36);
        }

        void connectionPanelTimerEvent()
        {
            if (m_bUpdateConnectionLog)
            {
                
                m_RPCMutex.WaitOne();


                if (m_bLoggedInGUI != m_bLoggedIn)
                {
                    m_bLoggedInGUI = m_bLoggedIn;
                    if (m_bLoggedInGUI)
                    {                        
                        m_ConnectPanelTop.SetColor(Color.FromArgb(40, 30, 30), Color.FromArgb(80, 80, 110));
                        m_ConnectInfo.Text = "Connected";
                        m_ConnectLogo.Image = m_ConnectLogoBMP;
                    }
                    else
                    {
                        m_ConnectPanelTop.SetColor(Color.FromArgb(120, 30, 30), Color.FromArgb(255, 80, 80));                        
                        m_ConnectInfo.Text = "Connecting";
                        m_ConnectLogo.Image = Grayscale(m_ConnectLogoBMP);
                    }
                }

                m_bUpdateConnectionLog = false;
                foreach (string s in m_LogItems)
                {
                    AddLogItem(s);
                }
                m_LogItems.Clear();
                m_RPCMutex.ReleaseMutex();
                
                
            }

        }

        private void connectMouseEnter(object sender, EventArgs e)
        {
            if (m_bLoggedInGUI) m_ConnectPanelTop.SetColor(Color.FromArgb(60, 60, 160), Color.FromArgb(100, 100, 240));
            else                m_ConnectPanelTop.SetColor(Color.FromArgb(160, 100, 100),Color.FromArgb(255, 150, 150));
        }
        private void connectMouseLeave(object sender, EventArgs e)
        {
            if (m_bLoggedInGUI)     m_ConnectPanelTop.SetColor(Color.FromArgb(30, 30, 40), Color.FromArgb(80, 80, 110));
            else                    m_ConnectPanelTop.SetColor(Color.FromArgb(120, 30, 30), Color.FromArgb(255, 80, 80));
        }

        private void connectMouseClick(object sender, EventArgs e)
        {
            if (m_ConnectPanel.Visible)
            {                
                m_ConnectPanel.Visible = false;
                m_ConnectArrowBMP.RotateFlip(RotateFlipType.Rotate180FlipX);
                m_ConnectionArrow.Image = m_ConnectArrowBMP;
            }
            else
            {
                m_ConnectPanel.BringToFront();
                m_ConnectPanel.Visible = true;

                m_ConnectArrowBMP.RotateFlip(RotateFlipType.Rotate180FlipX);
                m_ConnectionArrow.Image = m_ConnectArrowBMP;
            }
        }
        private void onConnectTypeChange(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            
            selectConnectType(comboBox.SelectedIndex);
        }

        private void AddLogItem(string log)
        {
            string text = DateTime.Now.ToString("HH:mm:ss") + " - "+log;

            m_ConnectLog.Items.Insert(0,text);
            m_ConnectLog.Items[0].ToolTipText = text;
            if (m_ConnectLog.Items.Count > 100) m_ConnectLog.Items.RemoveAt(100);

        }

        private void selectConnectType(int type)
        {
            if (type == 1)
            {
                m_ConnectTypeInfo.Text = "Supply an IP address of a specific node you want to connect to in ipaddress:port format";
                m_ConnectTypeIP.Visible = true;
            }
            else
            {
                m_ConnectTypeInfo.Text = "Connects to the MicroCash network using a list of public nodes";
                m_ConnectTypeIP.Visible = false;
            }

        }
    }
}