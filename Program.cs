/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright and licensing details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MicroCash.Client.Thin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 f1 = new Form1();
            if (f1.m_ThinUsers.Count == 0)
            {
                //create new user
                Form2 f2 = new Form2();

                if (f2.ShowDialog() != DialogResult.OK) return;
                f1.addNewUser(f2.m_user, f2.m_pass1, f2.m_pass2);
            }
            Application.Run(f1);
        }
    }
}
