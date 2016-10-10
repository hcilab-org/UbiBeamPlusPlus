using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// A selection of colour manipulation functions.
    /// </summary>
    /// <author>John Hardy</author>
    /// <date>31st October 2011</date>
    public class ColourUtils
    {
        /// <summary>
        /// Convert a value (between min and max) into blended RGB space.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="min">The minimum cap on the value parameter.</param>
        /// <param name="max">The maximum cap on the value parameter.</param>
        /// <param name="r">The red byte to write out too.</param>
        /// <param name="g">The green byte to write out too.</param>
        /// <param name="b">The blue byte to write out too.</param>
        public static void FauxColourRGB(double val, double min, double max, ref byte r, ref byte g, ref byte b)
        {
            r = 0;
            g = 0;
            b = 0;
            val = (val - min) / (max - min);
            if (val <= 0.2)
            {
                b = (byte)((val / 0.2) * 255);
            }
            else if (val > 0.2 && val <= 0.7)
            {
                b = (byte)((1.0 - ((val - 0.2) / 0.5)) * 255);
            }
            if (val >= 0.2 && val <= 0.6)
            {
                g = (byte)(((val - 0.2) / 0.4) * 255);
            }
            else if (val > 0.6 && val <= 0.9)
            {
                g = (byte)((1.0 - ((val - 0.6) / 0.3)) * 255);
            }
            if (val >= 0.5)
            {
                r = (byte)(((val - 0.5) / 0.5) * 255);
            }
        }

        /// <summary>
        /// Convert a value (between min and max) into blended RGB space.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="min">The minimum cap on the value parameter.</param>
        /// <param name="max">The maximum cap on the value parameter.</param>
        /// <param name="r">The red byte to write out too.</param>
        /// <param name="g">The green byte to write out too.</param>
        /// <param name="b">The blue byte to write out too.</param>
        public static int FauxColourRGB(double val, double min, double max)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            val = (val - min) / (max - min);
            if (val <= 0.2)
            {
                b = (byte)((val / 0.2) * 255);
            }
            else if (val > 0.2 && val <= 0.7)
            {
                b = (byte)((1.0 - ((val - 0.2) / 0.5)) * 255);
            }
            if (val >= 0.2 && val <= 0.6)
            {
                g = (byte)(((val - 0.2) / 0.4) * 255);
            }
            else if (val > 0.6 && val <= 0.9)
            {
                g = (byte)((1.0 - ((val - 0.6) / 0.3)) * 255);
            }
            if (val >= 0.5)
            {
                r = (byte)(((val - 0.5) / 0.5) * 255);
            }

            int iColour;
            iColour = r << 16; // R
            iColour |= g << 8;  // G
            iColour |= b << 0;  // B
            return iColour;
        }

        /// <summary>
        /// Converte a value (between min and max) into blended grayscale space.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="min">The minimum cap on the value parameter.</param>
        /// <param name="max">The maximum cap on the value parameter.</param>
        /// <param name="r">The red byte to write out too.</param>
        /// <param name="g">The green byte to write out too.</param>
        /// <param name="b">The blue byte to write out too.</param>
        public static void Greyscale(double val, double min, double max, ref byte r, ref byte g, ref byte b)
        {
            val = (val - min) / (max - min);
            r = (byte)((1.0 - val) * 255);
            g = r;
            b = r;
        }

        /// <summary>
        /// Converte a value (between min and max) into blended grayscale space.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="min">The minimum cap on the value parameter.</param>
        /// <param name="max">The maximum cap on the value parameter.</param>
        public static int Greyscale(double val, double min, double max)
        {
            val = (val - min) / (max - min);
            byte r = (byte)((1.0 - val) * 255);

            int iColour;
            iColour  = r << 16; // R
            iColour |= r << 8;  // G
            iColour |= r << 0;  // B
            return iColour;
        }
    }
}
