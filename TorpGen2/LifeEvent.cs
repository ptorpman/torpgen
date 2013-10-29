//=============================================================================================================================================
// Copyright (c) 2013-  Peter R. Torpman (peter at torpman dot se)
//
// This file is part of TorpGen (http://torpgen.sourceforge.net)
//
// TorpGen is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// TorpGen is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
//=============================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TorpGen2
{
    [Serializable()]
    public enum LifeEventTypes
    {
        Birth = 0,
        Death = 1,
        Marriage = 2
    }

    [Serializable()]
    public class LifeEvent
    {
        public LifeEventTypes eventType { get; set; }
        public String date  { get; set; }
        public String place  { get; set; }
        public String note   { get; set; }

        public LifeEvent(LifeEventTypes type, String date, String place, String note)
        {
            this.eventType = type;
            this.date = date;
            this.place = place;
            this.note = note;
        }

        public LifeEvent()
        {
        }

        public override string ToString()
        {
            return date + ", " + place;
        }

        /// <summary>
        /// Used to print a LifeEvent
        /// </summary>
        /// <param name="mSettings"></param>
        /// <param name="currY"></param>
        /// <returns></returns>
        public float Print(PrintSettings mSettings, float currY)
        {
            // Birth
            String tmp;

            switch (eventType)
            {
                case LifeEventTypes.Birth:
                    tmp = "Birth:";
                    break;
                case LifeEventTypes.Death:
                    tmp = "Death:";
                    break;
                case LifeEventTypes.Marriage:
                    tmp = "Marriage:";
                    break;
                default:
                    tmp = "";
                    break;
            }

            mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Bold], Brushes.Black, mSettings.X, currY);

            if (!(date == "" && place == ""))
            {
                tmp = date + ", " + place;
                mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, mSettings.tab1, currY);
            }

            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Bold].Height + mSettings.yMargin;

            if (note == "")
            {
                // Nothing more to print
                return currY;
            }

            // Print the notes text with a header and rectangle filled with text
            mSettings.graphics.DrawString("Notes", mSettings.font[(int)PrintSettings.Fonts.Bold], Brushes.Black, mSettings.tab1, currY);
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Bold].Height + mSettings.yMargin;


            // See how wide the rectangle can be.
            SizeF size = mSettings.graphics.MeasureString(note, mSettings.font[(int)PrintSettings.Fonts.Regular]);
            float width = mSettings.Width - mSettings.tab1 - mSettings.X;
            RectangleF rect = new RectangleF(mSettings.tab1, currY, width, size.Height);
            mSettings.graphics.DrawString(note, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, new PointF(mSettings.tab1, currY));
            mSettings.graphics.DrawRectangle(Pens.Transparent, Rectangle.Round(rect));

            currY += size.Height + mSettings.yMargin;

            return currY;
        }
    }
}
