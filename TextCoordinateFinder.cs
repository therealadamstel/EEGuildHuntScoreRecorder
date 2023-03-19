using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TesseractOCR;
using TesseractOCR.Enums;
using System.Runtime.ExceptionServices;
using TesseractOCR.Layout;
using OpenCvSharp.Detail;

namespace EEGuildHuntTool
{
    static class TextCoordinateFinder
    {
        public static Rect FindXCoordinateOfMember(string file)
        {
            // The section of text underneath a member name is the left-most are we're looking
            // for in the target image. 
            var rect = FindXCoordinateOfText(file, "Member");
            if (rect == Rect.Empty)
            {
                rect = FindXCoordinateOfText(file, "Official");
                if (rect == Rect.Empty)
                {
                    rect = FindXCoordinateOfText(file, "Chancellor");
                }
            }

            if (rect == Rect.Empty)
            {
                Console.WriteLine($"File {file} unprocessable - cannot find X coordinate");
            }

            return rect;
        }

        public static Rect FindXCoordinateOfText(string file, string textToFind)
        {
            // Recognize the text in the image
            using (var page = TextOCR.ReadPage(file, PageSegMode.SparseText))
            {
                // We're looking for the specific word sent in, so loop through
                // all the blocks and words to get to each individual word.
                var matches = new List<Rect>();
                foreach (var block in page.Layout)
                {
                    foreach (var paragraph in block.Paragraphs)
                    {
                        foreach (var textLine in paragraph.TextLines)
                        {
                            foreach (var word in textLine.Words)
                            {
                                if (word.Text.Trim().Equals(textToFind, StringComparison.OrdinalIgnoreCase))
                                {
                                    matches.Add(word.BoundingBox.Value);
                                }
                            }
                        }
                    }
                }

                if (matches.Count > 0)
                {
                    // Return the rectangle with the smallest area
                    return matches.OrderBy(x => x.Width * x.Height).First();
                }
            }

            return Rect.Empty;
        }
    }
}
