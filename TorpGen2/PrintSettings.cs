using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorpGen2
{
    public class PrintSettings
    {
        // Used to select fonts
        public enum Fonts : int
        {
            Header1 = 0,
            Header2 = 1,
            Header3 = 2,
            Bold = 3,
            Regular = 4,
            Small = 5
        }


        private Graphics _graphics;

        /// <summary>
        /// Graphics instance
        /// </summary>
        public Graphics graphics
        {
            get { return _graphics; }
            set
            {
                _graphics = value;
                // Initialize the fonts and their sizes
                font[(int)Fonts.Header1] = new Font("Segoe UI", 22, FontStyle.Regular);
                font[(int)Fonts.Header2] = new Font("Segoe UI", 18, FontStyle.Regular);
                font[(int)Fonts.Header3] = new Font("Segoe UI", 14, FontStyle.Regular);
                font[(int)Fonts.Bold] = new Font("Segoe UI", 11, FontStyle.Bold);
                font[(int)Fonts.Regular] = new Font("Segoe UI", 11, FontStyle.Regular);
                font[(int)Fonts.Small] = new Font("Segoe UI", 9, FontStyle.Regular);

                fontSize[(int)Fonts.Header1] = _graphics.MeasureString("Test", font[(int)Fonts.Header1]);
                fontSize[(int)Fonts.Header2] = _graphics.MeasureString("Test", font[(int)Fonts.Header2]);
                fontSize[(int)Fonts.Header3] = _graphics.MeasureString("Test", font[(int)Fonts.Header3]);
                fontSize[(int)Fonts.Bold] = _graphics.MeasureString("Test", font[(int)Fonts.Bold]);
                fontSize[(int)Fonts.Regular] = _graphics.MeasureString("Test", font[(int)Fonts.Regular]);
                fontSize[(int)Fonts.Small] = _graphics.MeasureString("Test", font[(int)Fonts.Small]);
            }
        }

        /// <summary>
        /// Fonts used
        /// </summary>
        public Font[] font = new Font[6];
        /// <summary>
        /// Fonts sizes used
        /// </summary>
        public SizeF[] fontSize = new SizeF[6];
        /// <summary>
        /// Left X position (where to start)
        /// </summary>
        public float X = 20;
        /// <summary>
        /// Topy X position (where to start)
        /// </summary>
        public float Y = 20;
        /// <summary>
        /// X position of tab stop 0
        /// </summary>
        public float tab0 = 50;
        /// <summary>
        /// X position of tab stop 1
        /// </summary>
        public float tab1 = 100;
        /// <summary>
        /// X position of tab stop 2
        /// </summary>
        public float tab2 = 150;
        /// <summary>
        /// Margin in Y direction
        /// </summary>
        public float yMargin = 5;

        /// <summary>
        /// The width available on the paper
        /// </summary>
        public float Width;


        public PrintSettings()
        {
        }
    }
}
