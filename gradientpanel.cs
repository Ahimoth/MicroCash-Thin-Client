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
using System.Text;
using System.Windows.Forms;

namespace GradientPanelCode
{
    public partial class GradientPanel : System.Windows.Forms.Panel
    {
        // member variables
        System.Drawing.Color mStartColor;
        System.Drawing.Color mEndColor;
        int m_Mode;

        public GradientPanel(Color start, Color end, int mode)
        {
            m_Mode = mode;
            SetColor(start,end);            
        }
        
        public void SetColor(Color start, Color end)
        {
            mStartColor = start;
            mEndColor = end;
            PaintGradient();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            PaintGradient();
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        private void PaintGradient()
        {

            System.Drawing.Drawing2D.LinearGradientBrush gradBrush;
            switch (m_Mode)
            {
                default: gradBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(0, this.Height), mStartColor, mEndColor); break;
                case 1: gradBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(this.Width, 0), mStartColor, mEndColor); break;
                case 2: gradBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(this.Width, this.Height), mStartColor, mEndColor); break;
            }

            Bitmap bmp = new Bitmap(this.Width, this.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(gradBrush, new Rectangle(0, 0,this.Width, this.Height));
            this.BackgroundImage = bmp;
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
    }
}