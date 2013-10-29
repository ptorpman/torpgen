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
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;


namespace TorpGen2
{   
    public sealed class PersonPrint
    {
        private Person mPerson = null;

        /// <summary>
        /// Used to keep print settings
        /// </summary>
        public static PrintSettings CurrentPrintSettings = new PrintSettings();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly PersonPrint _instance = new PersonPrint();

        public TorpgenDB mDatabase = null;


        /// <summary>
        /// Constructor
        /// </summary>
        private PersonPrint() { }

        /// <summary>
        /// Returns the singleton
        /// </summary>
        public static PersonPrint Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Prints a person
        /// </summary>
        /// <param name="p">Instance of person</param>
        /// <param name="p">Database instance</param>
        public void PrintPerson(Person p, TorpgenDB db)
        {
            mPerson = p;
            mDatabase = db;

            var ppd = new PrintPreviewDialog();
            var doc = new PrintDocument();

            var printSettings = new PrinterSettings();
            var pageSettings = new PageSettings();

            pageSettings.Margins.Left = 50;
            pageSettings.Margins.Right = 100;
            pageSettings.Margins.Top = 50;
            pageSettings.Margins.Bottom = 100;
            pageSettings.PaperSize = new PaperSize("A4", 850, 1100);

            //printSettings.PaperSizes = ;

            doc.PrintPage += new PrintPageEventHandler(HandlePrintPage);
            doc.DocumentName = p.listBoxString;
            doc.DefaultPageSettings = pageSettings;
            doc.PrinterSettings = printSettings;
            ppd.Document = doc;
            ((Form)ppd).WindowState = FormWindowState.Maximized;
            ppd.ShowDialog();
        }

        public void HandlePrintPage(object sender, PrintPageEventArgs ev)
        {
            CurrentPrintSettings.graphics = ev.Graphics;
            CurrentPrintSettings.Width = ev.PageBounds.Width;

            mPerson.Print(CurrentPrintSettings, ev, mDatabase);       
        }

        /// <summary>
        /// Draws a separator on the paper
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="stopX"></param>
        /// <param name="ypos"></param>
        public void DrawSeparator(float startX, float stopX, float ypos)
        {
            CurrentPrintSettings.graphics.DrawLine(Pens.Black, startX, ypos, stopX, ypos);
        }

    }
}
