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
using System.Drawing.Printing;

namespace TorpGen2
{
    [Serializable()]
    public class Marriage
    {
        /// <summary>
        /// Marriage GUID
        /// </summary>
        public Guid id { get; set; }
        /// <summary>
        /// Husband GUID
        /// </summary>
        public Guid husband { get; set; }
        /// <summary>
        /// Wife GUID
        /// </summary>
        public Guid wife { get; set; }
        /// <summary>
        /// Date, Place, Notes etc.
        /// </summary>
        public LifeEvent info { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manId">GUID of husband</param>
        /// <param name="wifeId">GUID of wife</param>
        /// <param name="date">Date of marriage</param>
        /// <param name="place">Place of marriage</param>
        /// <param name="note">Notes</param>
        public Marriage(Guid manId, Guid wifeId, String date, String place, String note)
        {
            id = Guid.NewGuid();
            info = new LifeEvent(LifeEventTypes.Marriage, date, place, note);
            husband = manId;
            wife = wifeId;
        }

        public Marriage()
        {
        }

        /// <summary>
        /// Prints marriage information.
        /// </summary>
        /// <param name="mSettings">Printing settings</param>
        /// <param name="currY">Current Y coordinate value</param>
        /// <param name="useWife">Flag if it is the wife part that should be printed</param>
        public float Print(PrintSettings mSettings, float currY, bool useWife, PrintPageEventArgs ev, TorpgenDB db)
        {
            var spouse = db.GetPerson(useWife ? wife : husband);
            float x = mSettings.X;

            if (spouse == null)
            {
                return currY;
            }

            string tmp = spouse.firstName + " " + spouse.lastName + " (" + spouse.birth.ToString() + "). ";

            mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, PersonPrint.CurrentPrintSettings.tab0, currY);
            currY += mSettings.yMargin;
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Regular].Height + mSettings.yMargin;

            tmp = "Married: " + info.date + ", " + info.place;
            mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, PersonPrint.CurrentPrintSettings.tab1, currY);
            currY += mSettings.yMargin;
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Regular].Height + mSettings.yMargin;

            tmp = "Children";
            mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Bold], Brushes.Black, PersonPrint.CurrentPrintSettings.tab1, currY);
            currY += mSettings.yMargin;
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Regular].Height + mSettings.yMargin;

            // Print all children these two persons have together.
            List<Person> kids = db.GetChildren(husband, wife);

            foreach (var k in kids)
            {
                tmp = k.firstName + " " + k.lastName + " (" + k.birth + ")";

                mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, PersonPrint.CurrentPrintSettings.tab2, currY);
                currY += mSettings.yMargin;
                currY += mSettings.fontSize[(int)PrintSettings.Fonts.Regular].Height + mSettings.yMargin;
            }

            return currY;
        }
    }

}
