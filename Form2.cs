/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright an licensing details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace microcash
{

    public partial class Form2 : Form
    {
        PictureBox[] m_Images;
        Bitmap m_tick;
        Bitmap m_cross;

        public string m_user;
        public string m_pass1;
        public string m_pass2;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            m_tick = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.tick.png"));
            m_cross = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MicroCash.Client.Thin.pngs.cross.png"));
            m_Images = new PictureBox[3];
            for (int x = 0; x < 3; x++)
            {
                m_Images[x] = new PictureBox();
                m_Images[x].SizeMode = PictureBoxSizeMode.StretchImage;
                m_Images[x].BackColor = Color.Transparent;
                
                m_Images[x].Image = m_cross;
                this.Controls.Add(m_Images[x]);
            }


            m_Images[0].Location = new Point(username.Location.X + username.Size.Width + 5, username.Location.Y);
            m_Images[1].Location = new Point(password1.Location.X + password1.Size.Width + 5, password1.Location.Y);
            m_Images[2].Location = new Point(password2.Location.X + password2.Size.Width + 5, password2.Location.Y);
            
            m_Images[0].Size = new Size(username.Size.Height, username.Size.Height);
            m_Images[1].Size = new Size(username.Size.Height, username.Size.Height);
            m_Images[2].Size = new Size(username.Size.Height, username.Size.Height);

            ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip ToolTip2 = new System.Windows.Forms.ToolTip();
            ToolTip ToolTip3 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(m_Images[0], "*) Must be at least 3 characters in length.\r\n*) Must only contain characters which can be used on the filesystem.");
            ToolTip2.SetToolTip(m_Images[1], "*) Must be at least 8 characters in length.");
            ToolTip3.SetToolTip(m_Images[2], "*) Must be at least 8 characters in length.\r\n*) Must not be the same as password 1");



            TextChanged(null,null);
        }

        bool IsValidFilename(string testName)
        {
            char[] namechars = System.IO.Path.GetInvalidFileNameChars();
            char[] pathchars = System.IO.Path.GetInvalidPathChars();
            foreach (char c in pathchars)
            {
                if (testName.IndexOf(c) != -1)  return false;
            }
            foreach (char c in namechars)
            {
                if (testName.IndexOf(c) != -1) return false;
            }

            return true;

           
        }


        private int VerifyDetails(string user, string pass1, string pass2)
        {
            int nRet = 0;

            if (user.Length >= 3 && IsValidFilename(user)==true)
            {                   
                nRet |= 1;
            }
            if (pass1.Length >= 8)
            {
                nRet |= 2;
            }
            if (pass2.Length >= 8 && pass2!=pass1)
            {               
                nRet |= 4;
            }

            return nRet;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_user = ((TextBox)(this.Controls.Find("username", true)[0])).Text;
            m_pass1 = ((TextBox)(this.Controls.Find("password1", true)[0])).Text;
            m_pass2 = ((TextBox)(this.Controls.Find("password2", true)[0])).Text;
            
            int nRet = VerifyDetails(m_user, m_pass1, m_pass2);
            if (nRet != 7) return;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void TextChanged(object sender, EventArgs e)
        {
            string user = ((TextBox)(this.Controls.Find("username", true)[0])).Text;
            string pass1 = ((TextBox)(this.Controls.Find("password1", true)[0])).Text;
            string pass2 = ((TextBox)(this.Controls.Find("password2", true)[0])).Text;
            int nRet = VerifyDetails(user, pass1, pass2);

            if (nRet == 7) createbutton.Enabled = true;
            else createbutton.Enabled = false;



            if ( (nRet & 1) == 1)   m_Images[0].Image = m_tick;
            else                    m_Images[0].Image = m_cross;
            if ( (nRet & 2) == 2)   m_Images[1].Image = m_tick;
            else                    m_Images[1].Image = m_cross;
            if ( (nRet & 4) == 4)   m_Images[2].Image = m_tick;
            else                    m_Images[2].Image = m_cross;
        }
              
      
    }
}
