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
    public class Person
    {
        public Guid id { get; set; }
        private String _firstName;
        public String firstName  
        { 
            get { return _firstName; } 
            set { _firstName = value; UpdateListBoxString(); }
        }

        private String _lastName;
        public String lastName
        { 
            get { return _lastName; } 
            set { _lastName = value; UpdateListBoxString(); }
        }

        private LifeEvent _birth;
        public LifeEvent birth 
        {
            get { return _birth; } 
            set { _birth = value; UpdateListBoxString(); } 
        }
        
        public Gender gender { get; set; }
        public LifeEvent death { get; set; }
        public List<Guid> marriages { get; set; }
        public List<Guid> children { get; set; }
        public String notes { get; set; }

        public Guid father { get; set; }
        public Guid mother { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public String listBoxString { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        private TorpgenDB _db;

        //----------------------------------------------------------------------
        // METHODS
        //----------------------------------------------------------------------

        /// <summary>
        /// Constructor
        /// </summary>
        public Person()
        {
            birth = new LifeEvent(LifeEventTypes.Birth, "", "", "");
            death = new LifeEvent(LifeEventTypes.Death, "", "", "");
        }

        public Person(TorpgenDB db)
        {
            birth = new LifeEvent(LifeEventTypes.Birth, "", "", "");
            death = new LifeEvent(LifeEventTypes.Death, "", "", "");
            _db = db;
        }

        /// <summary>
        /// Adds a marriage to the person
        /// </summary>
        /// <param name="marriageId"></param>
        public void AddMarriage(Guid marriageId)
        {
            if (marriages != null && marriages.Contains(marriageId))
            {
                // Already there.
                return;
            }

            if (marriages == null)
            {
                marriages = new List<Guid>();
            }

            marriages.Add(marriageId);
        }

        /// <summary>
        /// Removes a marriage from the person
        /// </summary>
        /// <param name="marriageId"></param>
        public void RemoveMarriage(Guid marriageId)
        {
            if (marriages == null)
            {
                return;
            }

            marriages.Remove(marriageId);
        }


        public void AddChild(Guid childId)
        {
            if (children == null)
            {
                children = new List<Guid>();
            }

            if (children.Contains(childId))
            {
                // Already there
                return;
            }

                      
            children.Add(childId);

            children = _db.GetListSortedByBirth(children);
        }

        public void RemoveChild(Guid id)
        {
            if (children == null || children.Contains(id) == false)
            {
                return;
            }

            children.Remove(id);
        }



        // Build a string for use in the person listbox
        public String GetListboxString()
        {
            return listBoxString;
        }

        public void UpdateListBoxString()
        {
            String s;

            s = lastName + ", " + firstName;

            if (birth != null)
            {
                s = s + " (" + birth.date + ")";
            }

            listBoxString = s;
        }

        /// <summary>
        /// Used to print a person.
        /// </summary>
        /// <param name="mSettings"></param>
        /// <param name="ev"></param>
        /// <param name="db"></param>
        public void Print(PrintSettings mSettings, PrintPageEventArgs ev, TorpgenDB db)
        {
            String tmp = "";
            float currY = mSettings.Y;
            float x = mSettings.X;

            // Name
            tmp = firstName + " " + lastName;

            mSettings.graphics.DrawString(tmp, mSettings.font[(int)PrintSettings.Fonts.Header1], Brushes.Black, x, currY);
            currY += mSettings.yMargin;

            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Header1].Height + mSettings.yMargin;

            // Separator
            PersonPrint.Instance.DrawSeparator(x, ev.PageBounds.Width - x, currY);
            currY += mSettings.yMargin;

            // Birth
            currY = birth.Print(mSettings, currY);
            // Death
            currY = death.Print(mSettings, currY);

            // Families (Marriages)
            
            
            currY += 40;
            mSettings.graphics.DrawString("Relationships", mSettings.font[(int)PrintSettings.Fonts.Header2], Brushes.Black, x, currY);
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Header2].Height + mSettings.yMargin;
            // Separator
            PersonPrint.Instance.DrawSeparator(x, ev.PageBounds.Width - x, currY);
            currY += mSettings.yMargin;


            for (int i = 0; i < marriages.Count; i++)
            {
                bool useWife = (gender == Gender.Male) ? true : false;
                Marriage m = db.GetMarriage(marriages[i]);
                currY = m.Print(mSettings, currY, useWife, ev, db);
            }



            // Notes
            currY += 40;
            mSettings.graphics.DrawString("Notes", mSettings.font[(int)PrintSettings.Fonts.Header2], Brushes.Black, x, currY);
            currY += mSettings.fontSize[(int)PrintSettings.Fonts.Header2].Height + (2 * mSettings.yMargin);
            // Separator
            PersonPrint.Instance.DrawSeparator(x, ev.PageBounds.Width - x, currY);
            currY += mSettings.yMargin;

            // See how wide the rectangle can be.
            SizeF size = mSettings.graphics.MeasureString(notes, mSettings.font[(int)PrintSettings.Fonts.Regular]);
            float width = mSettings.Width - mSettings.X - mSettings.X;
            RectangleF rect = new RectangleF(mSettings.X, currY, width, size.Height);
            mSettings.graphics.DrawString(notes, mSettings.font[(int)PrintSettings.Fonts.Regular], Brushes.Black, new PointF(mSettings.X, currY));
            mSettings.graphics.DrawRectangle(Pens.Transparent, Rectangle.Round(rect));

            currY += size.Height + mSettings.yMargin;
        }
    }

}
