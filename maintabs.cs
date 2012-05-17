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
        CustomTabControl m_TabControl;
        private ListView m_TXDetails;
        private ListView m_AddressBook;
        Label m_SendTotalLabel;
        TextBox m_SendTotalAmount;
        Label m_SendFeeLabel;
        TextBox m_SendFeeAmount;
        Button m_SendButton;
        Button m_CreatePaymentCodeButton;

        PictureBox m_AboutLogo;
        
        WebBrowser m_HelpBrowse;

        Label m_SendFromLabel;
        ComboBox m_SendFrom;
        TextBox m_SendFromAmount;

        Label m_SendToLabel;
        ComboBox m_SendTo;
        TextBox m_SendToAmount;

        Label m_TXInfoLabel;
        TextBox m_TXInfo;

        void InitTabs()
        {
            m_TabControl = new CustomTabControl();
            m_TabControl.DisplayStyle = TabStyle.VS2010;
            m_TabControl.DisplayStyleProvider.ShowTabCloser = false;
            m_TabControl.DisplayStyleProvider.Padding = new Point(10,8);
            m_TabControl.DisplayStyleProvider.BorderColorHot = Color.FromArgb(100, 100,255);            
            m_TabControl.Font = new Font(Constants.FONTNAME, 8,FontStyle.Bold);
            m_TabControl.Location = new Point(0, 0);
            //m_TabControl.Appearance = TabAppearance.Buttons;
            //m_TabControl.Alignment = TabAlignment.Bottom;
            TabPage page1 = new TabPage("New Payment");
            TabPage page2 = new TabPage("History");
            TabPage page3 = new TabPage("Address Book");
            TabPage page4 = new TabPage("Exchange");
            TabPage page5 = new TabPage("Options");
            TabPage page6 = new TabPage("Help");
            m_TabControl.TabPages.Add(page1);
            m_TabControl.TabPages.Add(page2);
            m_TabControl.TabPages.Add(page3);
            m_TabControl.TabPages.Add(page4);
            m_TabControl.TabPages.Add(page5);
            m_TabControl.TabPages.Add(page6);

            DoExchangePage();
                        
            m_SendButton = new Button();
            m_SendButton.Text = "Send";
            m_SendButton.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendButton.Click += sendFunds;

            m_CreatePaymentCodeButton = new Button();
            m_CreatePaymentCodeButton.Text = "Create Payment Code";
            m_CreatePaymentCodeButton.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_CreatePaymentCodeButton.Click += createPaymentCode;

            ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(m_CreatePaymentCodeButton, "Enter an address, an amount and some info then\nclick this button to create a payment code.\n\nUsing a payment code you can give exact payment\ndetails to people instead of just an address.");
            

            m_SendTotalLabel = new Label();
            m_SendTotalLabel.Text = "Total:";
            m_SendTotalLabel.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendTotalLabel.TextAlign = ContentAlignment.MiddleRight;
            m_SendTotalAmount = new TextBox();
            m_SendTotalAmount.ReadOnly = true;
            m_SendTotalAmount.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendTotalAmount.Text = "0";
            m_SendTotalAmount.TextAlign = HorizontalAlignment.Center;
            

            m_SendFeeLabel = new Label();
            m_SendFeeLabel.Text = "Fee:";
            m_SendFeeLabel.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendFeeLabel.TextAlign = ContentAlignment.MiddleRight;
            m_SendFeeAmount = new TextBox();
            m_SendFeeAmount.ReadOnly = true;
            m_SendFeeAmount.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendFeeAmount.Text = "$0.005";
            m_SendFeeAmount.TextAlign = HorizontalAlignment.Center;
            
            m_SendFromLabel = new Label();
            m_SendFromLabel.Text = "Send From:";
            m_SendFromLabel.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendFromLabel.TextAlign = ContentAlignment.MiddleRight;

            m_SendFrom = new ComboBox();
            m_SendFrom.DropDownStyle = ComboBoxStyle.DropDownList;
            m_SendFrom.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendFrom.SelectedIndexChanged += onFromChange;

            m_SendFromAmount = new TextBox();
            m_SendFromAmount.ReadOnly = true;
            m_SendFromAmount.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendFromAmount.TextAlign = HorizontalAlignment.Center;
            
            m_TXInfoLabel = new Label();
            m_TXInfoLabel.Text = "Extra Info:";
            m_TXInfoLabel.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_TXInfoLabel.TextAlign = ContentAlignment.MiddleRight;

            m_TXInfo = new TextBox();
            m_TXInfo.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_TXInfo.TextAlign = HorizontalAlignment.Center;
            m_TXInfo.TextChanged += info_textchange;

            m_SendToLabel = new Label();
            m_SendToLabel.Text = "Send To:";
            m_SendToLabel.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);            
            m_SendToLabel.TextAlign = ContentAlignment.MiddleRight;
                       
            
            m_SendTo = new ComboBox();
            m_SendTo.DropDownStyle = ComboBoxStyle.DropDown;
            m_SendTo.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendTo.TextChanged += sendto_textchange;

            m_SendToAmount = new TextBox();
            m_SendToAmount.Font = new Font(Constants.FONTNAME, 8, FontStyle.Bold);
            m_SendToAmount.TextAlign = HorizontalAlignment.Center;
            m_SendToAmount.KeyPress += sendamount_KeyPress;


            m_TabControl.TabPages[0].Controls.Add(m_SendFromLabel);
            m_TabControl.TabPages[0].Controls.Add(m_SendFrom);
            m_TabControl.TabPages[0].Controls.Add(m_SendFromAmount);            
            m_TabControl.TabPages[0].Controls.Add(m_SendToLabel);
            m_TabControl.TabPages[0].Controls.Add(m_SendTo);
            m_TabControl.TabPages[0].Controls.Add(m_SendToAmount);
            m_TabControl.TabPages[0].Controls.Add(m_TXInfoLabel);
            m_TabControl.TabPages[0].Controls.Add(m_TXInfo);


            m_TabControl.TabPages[0].Controls.Add(m_CreatePaymentCodeButton);
            m_TabControl.TabPages[0].Controls.Add(m_SendButton);
            m_TabControl.TabPages[0].Controls.Add(m_SendTotalLabel);
            m_TabControl.TabPages[0].Controls.Add(m_SendTotalAmount);
            m_TabControl.TabPages[0].Controls.Add(m_SendFeeLabel);
            m_TabControl.TabPages[0].Controls.Add(m_SendFeeAmount);
            
            //GradientPanel aboutbg = new GradientPanel(Color.FromArgb(96, 96, 96), Color.FromArgb(0, 0, 0),2);
            //aboutbg.Dock = DockStyle.Fill;

            Bitmap bmp = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.logo.png"));
            m_AboutLogo = new PictureBox();
            m_AboutLogo.BackColor = Color.Transparent;
            m_AboutLogo.Location = new Point(20, 20);
            m_AboutLogo.Size = new Size(450, 114);
            m_AboutLogo.Image = bmp;
            page6.Controls.Add(m_AboutLogo);
            //page5.Controls.Add(aboutbg);


            m_TXDetails = new ListView();            
            m_TXDetails.Size = new Size(0, 0);
            m_TXDetails.View = System.Windows.Forms.View.Details;
            m_TXDetails.FullRowSelect = true;
            //m_TXDetails.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            m_TXDetails.Dock = DockStyle.Fill;
            m_TXDetails.Columns.Add("Account");
            m_TXDetails.Columns.Add("Amount");
            m_TXDetails.Columns.Add("Info");
            m_TXDetails.Columns.Add("Time");

            m_TXDetails.Columns[0].Width = 170;
            m_TXDetails.Columns[1].Width = 130;
            m_TXDetails.Columns[2].Width = 200;
            m_TXDetails.Columns[3].Width = 200;
            page2.Controls.Add(m_TXDetails);

            m_AddressBook = new ListView();
            m_AddressBook.Size = new Size(0, 0);
            m_AddressBook.View = System.Windows.Forms.View.Details;
            m_AddressBook.FullRowSelect = true;
            m_AddressBook.Dock = DockStyle.Fill;
            m_AddressBook.Columns.Add("Name");
            m_AddressBook.Columns.Add("Address");

            m_AddressBook.Columns[0].Width = 250;
            m_AddressBook.Columns[1].Width = 250;
            page3.Controls.Add(m_AddressBook);

            //m_HelpBrowse = new WebBrowser();
            //m_HelpBrowse.Dock = DockStyle.Fill;
            //m_HelpBrowse.Navigate("http://microcash.org");            
            //page5.Controls.Add(m_HelpBrowse);

            
            m_RightPanel.Controls.Add(m_TabControl);
        }

        void ResizeTabs()
        {
            int nRightWidth = 120;
            m_TabControl.Location = new Point(0, 4);
            m_TabControl.Size = new Size(m_RightPanel.ClientSize.Width, m_RightPanel.ClientSize.Height-4);

            int w = m_TabControl.ClientSize.Width;

            m_SendFrom.Location = new Point(110, 10);
            m_SendFrom.Size = new Size(w - (110+nRightWidth+20), 10);
            m_SendFromLabel.Location = new Point(0, 10);
            m_SendFromLabel.Size = new Size(110, m_SendFrom.Size.Height);
            m_SendFromAmount.Location = new Point(w - (nRightWidth + 20), m_SendFrom.Location.Y);
            m_SendFromAmount.Size = new Size(nRightWidth, 0);

            m_SendTo.Location = new Point(110, m_SendFrom.Size.Height+20);
            m_SendTo.Size = new Size(w - (110 + nRightWidth + 20), 10);
            m_SendToLabel.Location = new Point(0, m_SendTo.Location.Y);
            m_SendToLabel.Size = new Size(110, m_SendTo.Size.Height);
            m_SendToAmount.Location = new Point(w - (nRightWidth+20), m_SendTo.Location.Y);
            m_SendToAmount.Size = new Size(nRightWidth, 0);
                        


            int nSendToHeight = m_SendTo.Location.Y + m_SendTo.Size.Height + 10;

            m_TXInfoLabel.Location = new Point(0, nSendToHeight);
            m_TXInfoLabel.Size = new Size(110, m_TXInfo.Size.Height);
            m_TXInfo.Location = new Point(110, nSendToHeight);
            m_TXInfo.Size = new Size(120, m_TXInfo.Size.Height);


            m_SendFeeAmount.Location = new Point(w - (nRightWidth+20), nSendToHeight);
            m_SendFeeAmount.Size = new Size(nRightWidth, 20);

            m_SendFeeLabel.Location = new Point(w - (nRightWidth + 20+100), nSendToHeight);
            m_SendFeeLabel.Size = new Size(100, m_SendFeeAmount.Height);

            nSendToHeight += m_SendFeeAmount.Height + 10;

            m_CreatePaymentCodeButton.Location = new Point(110, nSendToHeight);
            m_CreatePaymentCodeButton.Size = new Size(180, m_SendTotalAmount.Height);

            m_SendTotalAmount.Location = new Point(w - (nRightWidth+20), nSendToHeight);
            m_SendTotalAmount.Size = new Size(nRightWidth, 40);
            m_SendTotalLabel.Location = new Point(w - (100 + nRightWidth+20), nSendToHeight);
            m_SendTotalLabel.Size = new Size(100, m_SendTotalAmount.Height);

            nSendToHeight += m_SendTotalAmount.Height + 10;

                        

            m_SendButton.Location = new Point(w - (nRightWidth+20), nSendToHeight);
            m_SendButton.Size = new Size(nRightWidth, 30);

            //exchange
            ResizeExchangePage();

            //about
            m_AboutLogo.Location = new Point( (w - m_AboutLogo.Size.Width)/2, 10);

        }
        void OnAccountFromSelect()
        {

        }

        void DoTabAccounts()
        {
            int n = m_SendFrom.SelectedIndex;
            m_SendFrom.Items.Clear();
            m_SendTo.Items.Clear();
            foreach (Account account in m_ThinUser.m_Accounts)
            {
                m_SendFrom.Items.Add(account.Name);
                m_SendTo.Items.Add(account.Name);
            }
            if (n == -1 || n > m_SendFrom.SelectedIndex) m_SendFrom.SelectedIndex = 0;
            else m_SendFrom.SelectedIndex = n;

            //do stored accounts for send to
        }


        private void onFromChange(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            m_RPCMutex.WaitOne();
            m_SendFromAmount.Text = DoBalanceString(m_ThinUser.m_Accounts[comboBox.SelectedIndex].Balance);
            m_RPCMutex.ReleaseMutex();
            

        }

        private void info_textchange(object sender, EventArgs e)
        {
            if (m_TXInfo.Text.Length > 8) m_TXInfo.Text = m_TXInfo.Text.Substring(0, 8);

        }
        private void sendto_textchange(object sender, EventArgs e)
        {
            bool bValid = false;
            if (GetInfoFromAddressBook(true,m_SendTo.Text) != null) bValid = true;

            if (bValid == false)
            {
                MicroCashAddress address = new MicroCashAddress(m_SendTo.Text);
                if (address.IsPaymentCode)
                {
                    m_SendToAmount.ReadOnly = true;
                    m_TXInfo.ReadOnly = true;
                    m_CreatePaymentCodeButton.Enabled = false;
                    m_TXInfo.Text = address.GetPaymentAmountInfoString();
                    m_SendToAmount.Text = DoBalanceString(address.GetPaymentAmount());
                }
                else if (address.IsLongAddress())
                {
                    m_TXInfo.ReadOnly = true;
                    m_CreatePaymentCodeButton.Enabled = false;
                    m_TXInfo.Text = address.GetPaymentAmountInfoString();

                    m_SendToAmount.ReadOnly = false;
                }
                else
                {
                    m_CreatePaymentCodeButton.Enabled = true;
                    m_SendToAmount.ReadOnly = false;
                    m_TXInfo.ReadOnly = false;
                    m_TXInfo.Text = "";
                    m_SendToAmount.Text = "";
                }
                bValid = address.IsValid();
            }

            if(bValid)  m_SendTo.ForeColor = Color.FromArgb(0,0,0);
            else        m_SendTo.ForeColor = Color.FromArgb(255,0,0);

        }
        private void sendamount_KeyPress(object sender, KeyPressEventArgs e)
        {
            //reject non numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true;
            //allow one decimal point
            int nDec = (sender as TextBox).Text.IndexOf('.');
            int nInsert = (sender as TextBox).SelectionStart;
            int nLength = (sender as TextBox).Text.Length;
            if (e.KeyChar == '.' && nDec != -1) e.Handled = true;
            if (!char.IsControl(e.KeyChar) && nDec != -1 && nLength > 0 && nLength - nDec > 4 && nInsert > nDec) e.Handled = true;
        }

        private string GetInfoFromAddressBook(bool bIsAccountName, string name)
        {
            //send to is in our address book, first check our accounts, then the real address book
            foreach (Account account in m_ThinUser.m_Accounts)
            {
                if (bIsAccountName && account.Name == name) return account.GetAddressString();
                else if (!bIsAccountName && account.GetAddressString() == name) return account.Name;
            }
            return null;
        }

        private void sendFunds(object sender, EventArgs e)
        {
            try
            {
                if (m_SendToAmount.Text.Length <= 0) { MessageBox.Show("Please enter an amount to send", "Error Creating Payment"); return; }

                string addrstr = m_SendTo.Text;
                if (m_SendTo.Items.IndexOf(addrstr) >= 0)
                {
                    addrstr = GetInfoFromAddressBook(true, addrstr);
                }

                MicroCashAddress address = new MicroCashAddress(addrstr);
                if (address.IsValid() == false) { MessageBox.Show("Please enter a valid address to send to", "Error Creating Payment"); return; }

                int nAccount = m_SendFrom.SelectedIndex;
                Account account = m_ThinUser.m_Accounts[nAccount];

                SendTxResult result = null;

                if (address.IsPaymentCode)
                {
                    //send a payment code style transaction
                    result = account.SendTx(address);
                }
                else
                {
                    //attempt to parse the send amount text box
                    double amount = 0;
                    if (!Double.TryParse(m_SendToAmount.Text, out amount))
                        MessageBox.Show(String.Format("Could not convert {0} to a number!", m_SendToAmount.Text), "Unable to Parse!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    //convert the tx info text box, if populated
                    byte[] info = null;
                    if (!string.IsNullOrEmpty(m_TXInfo.Text))
                        info = UTF8Encoding.Default.GetBytes(m_TXInfo.Text);

                    //send custom transaction
                    result = account.SendTx(address, amount, info);
                }

                if (result != null && result.IsSent == true)
                {
                    m_SendToAmount.Text = "";
                    m_SendTo.Text = "";
                    m_TXInfo.Text = "";
                    m_bUpdateNow = true;
                    MessageBox.Show("Transaction sent!");
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage, "Error sending transaction");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }



        private void createPaymentCode(object sender, EventArgs e)
        {
            bool bAmount=true;
            if (m_SendToAmount.Text.Length <= 0) bAmount = false;
            
            string addrstr = m_SendTo.Text;
            if (m_SendTo.Items.IndexOf(addrstr) >= 0) addrstr = GetInfoFromAddressBook(true,addrstr);
            MicroCashAddress address = new MicroCashAddress(addrstr);
            if (address.IsValid() == false) { MessageBox.Show("Please enter a valid address to send to", "Error Creating Payment Code"); return; }

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            address.SetInfo(encoding.GetBytes(m_TXInfo.Text));
            if (bAmount)
            {
                address.SetAmount((Int64)(Convert.ToDouble(m_SendToAmount.Text) * 10000));
                m_SendTo.Text = address.GetPaymentCodeString();
            }
            else
            {
                m_SendTo.Text = address.GetAddressInfoString();
            }
        }
    }
    
}
