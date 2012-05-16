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
using System.Windows.Forms;
using GradientPanelCode;

namespace microcash
{
    public class ImageMenu
    {
        Form m_ParentForm;
        public ContextMenu m_Menu;
        EventHandler m_onClick;
        Font m_Font;

        public ImageMenu(Form ParentForm, Font menuFont, EventHandler onClick)
        {
            m_Font = menuFont;
            m_ParentForm = ParentForm;
            m_Menu = new ContextMenu();
            m_onClick = onClick;
        }

        public ContextMenu GetMenu() { return m_Menu; }

        public void AddItem(Bitmap bmp, string text)
        {
                        

            MenuItem mnuItem = new MenuItem();
            mnuItem.Tag = bmp;
            //m_BitMapList.Add(bmp);
            mnuItem.OwnerDraw = true;
            mnuItem.Text = text;
            mnuItem.DrawItem += new System.Windows.Forms.DrawItemEventHandler(DrawMenuItemCopy);
            mnuItem.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureItemMenuItemCopy);            
            mnuItem.Click += m_onClick;

            m_Menu.MenuItems.Add(mnuItem);
        }

        public void AddSeparator()
        {

            MenuItem mnuItem = new MenuItem();
            mnuItem.Tag = null;
            mnuItem.OwnerDraw = true;
            mnuItem.Text = "";
            mnuItem.Enabled = false;
            mnuItem.DrawItem += new System.Windows.Forms.DrawItemEventHandler(DrawMenuItemCopy);
            mnuItem.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureItemMenuItemCopy);
            m_Menu.MenuItems.Add(mnuItem);
        }
        public void Show()
        {
            Point p = m_ParentForm.PointToClient(Cursor.Position);
            m_Menu.Show(m_ParentForm, p);
        }

        private void MeasureItemMenuItemCopy(object obj, MeasureItemEventArgs miea)
        {
            MenuItem mi = (MenuItem)obj;

            if (mi.Tag==null && (mi.Text == null || mi.Text.Length==0))
            {
                //separator
                miea.ItemWidth = 150;
                miea.ItemHeight = 16;
                return;
            }

            Font menuFont = SystemInformation.MenuFont;
            if (m_Font != null) menuFont = m_Font;

            StringFormat strfmt = new StringFormat();

            SizeF sizef =   miea.Graphics.MeasureString(mi.Text,menuFont,1000,strfmt);

            // Get image so size can be computed
            Bitmap bmp = (Bitmap)mi.Tag;

            miea.ItemWidth = (int)Math.Ceiling(sizef.Width);
            miea.ItemHeight = (int)Math.Ceiling(sizef.Height);

            if (bmp != null)
            {
                miea.ItemWidth += bmp.Width+10;
                if(miea.ItemHeight<bmp.Height+10)   miea.ItemHeight = bmp.Height+10;                
            }            
        }

        private void DrawMenuItemCopy(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            SolidBrush menuBrush;

            // Fill rectangle with proper background color [use this instead of e.DrawBackground() ]
            if ( (mi.Text.Length>0 ||mi.Tag!=null) && (e.State & DrawItemState.Selected) != 0)
            {
                // Selected color
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                menuBrush = new SolidBrush(SystemColors.HighlightText); 
            }
            else
            {
                // Normal background color (when not selected)
                e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds);
                menuBrush = new SolidBrush(SystemColors.MenuText);                
            }

            // Center the text portion (out to side of image portion)
            // Rectanble for text portion
            Rectangle rectText = e.Bounds;

            StringFormat strfmt = new StringFormat();
            strfmt.LineAlignment = System.Drawing.StringAlignment.Near;

            // Get image associated with this menu item
            if (mi.Tag != null)
            {
                Bitmap bmp = (Bitmap)mi.Tag;

                // Rectangle for image portion
                Rectangle rectImage = e.Bounds;

                // Set image rectangle same dimensions as image
                if (mi.Text.Length > 0)
                {
                    rectImage.X += 5;
                    rectImage.Y += 5;
                }
                else
                {
                    rectImage.X += (rectImage.Width - bmp.Width) / 2;
                    rectImage.Y += (rectImage.Height - bmp.Height) / 2;
                }


                rectImage.Width = bmp.Width;
                rectImage.Height = bmp.Height;
                rectText.X += rectImage.X + rectImage.Width + 5;
                e.Graphics.DrawImage(bmp, rectImage);
            }
            else
            {
                rectText.X += 5;
            }

            if (mi.Text.Length == 0 && mi.Tag == null)
            {
                e.Graphics.DrawLine(new Pen(Color.Black, 2), new Point(e.Bounds.X + 5, e.Bounds.Y + 7), new Point((e.Bounds.Width - 5), e.Bounds.Y + 7));

            }
            else if (mi.Text.Length>0)
            {
                Font menuFont = SystemInformation.MenuFont;
                if (m_Font != null) menuFont = m_Font;
                e.Graphics.DrawString(mi.Text, menuFont, menuBrush, rectText.X, rectText.Y + ((e.Bounds.Height - menuFont.Height) / 2), strfmt);
            }
            
        }
            
    }

}