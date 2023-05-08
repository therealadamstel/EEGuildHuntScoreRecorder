using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using TesseractOCR;

namespace EEGuildHuntTool
{
    static class ScoreImageParser
    {
        public static List<ScoreLog> Parse(string imageName)
        {
            try
            {
                // 1 - Load the original image
                using (Image fullPictureBitmap = Bitmap.FromFile(imageName))
                {
                    // Get the X coordinate of where the "Member" text is
                    var xTrimSpot = TextCoordinateFinder.FindXCoordinateOfMember(imageName);
                    if (xTrimSpot == Rect.Empty)
                        throw new InvalidOperationException("Cannot find X coordinate");

                    // 2 - Chop X% off of the left, with a 8 pixel border
                    // (The way the scores are laid out the avatar frame will overflow the bounding rectangle, throwing off
                    // edge detection and that makes it harder to find the part with the scores)
                    // NOTE: in my testing images it was .620M or .64317M, this allows me to take a variety of resolutions
                    decimal horizPortion = 1M - (xTrimSpot.X1 / (decimal)fullPictureBitmap.Width);
                    int newWidth = (int)Math.Floor(fullPictureBitmap.Width * horizPortion) + 8; // 8 pixels to offset just a bit

                    Bitmap choppedImg = ReduceImageToRelevantSection(fullPictureBitmap, newWidth, "Chopped.png");

                    // 3 - Find the edges
                    using (var image = new Image<Bgr, byte>("Chopped.png"))
                    {
                        // Convert the image to grayscale (showing the "Member" bounding box)
                        var grayImage = image.Convert<Gray, byte>();
                        grayImage.Draw(new Rectangle(xTrimSpot.X1, xTrimSpot.Y1, xTrimSpot.Width, xTrimSpot.Height), new Gray(255), 1);

                        DisplayImage(grayImage, 0);

                        // Apply Canny edge detection
                        var cannyEdges = grayImage.Canny(50, 900, 3, false);

                        DisplayImage(cannyEdges, 0);

                        // Find contours in the new image
                        List<Rectangle> rects = FindRectangles(image, cannyEdges);

                        DisplayImage(image, 0);

                        // 4 - For each rect, break it apart and look for scores
                        List<ScoreLog> scores = new List<ScoreLog>();
                        foreach(var rect in rects)
                        {
                            // Create a new image with only our target score section in it
                            var bm = new Bitmap(rect.Width, rect.Height);
                            bm.SetResolution(fullPictureBitmap.HorizontalResolution, fullPictureBitmap.VerticalResolution);

                            using (var graphics = Graphics.FromImage(bm))
                            {
                                graphics.Clear(Color.White);
                                graphics.DrawImage(choppedImg, -rect.Left, -rect.Top);
                            }

                            // Break apart the new image into sections to read the scores from
                            var log = QuandrantizeImage(bm);
                            if (log.IsValid())
                            {
                                scores.Add(log);
                            }
                        }

                        return scores;
                    }
                }
            }
            finally
            {
                // Delete the temp files
                File.Delete("Chopped.png");
            }
        }

        private static Bitmap ReduceImageToRelevantSection(Image fullPictureBitmap, int newWidth, string fileName)
        {
            var choppedImg = new Bitmap(newWidth, fullPictureBitmap.Height, fullPictureBitmap.PixelFormat);
            choppedImg.SetResolution(fullPictureBitmap.HorizontalResolution, fullPictureBitmap.VerticalResolution);
            using (var graphics = Graphics.FromImage(choppedImg))
            {
                // All black background
                graphics.Clear(Color.Yellow);

                // Write the original image, but at an offset
                graphics.DrawImage(fullPictureBitmap, newWidth - fullPictureBitmap.Width, 0);

                // Draw a black line on the left to ensure there is a hard surface for the edge detection to find
                // (This will cover up any accidental avatar frames that still show up)
                graphics.DrawLine(new Pen(Brushes.Black, 6), new Point(0, 0), new Point(0, fullPictureBitmap.Height));
            }
            choppedImg.Save(fileName);
            return choppedImg;
        }

        private static List<Rectangle> FindRectangles(Image<Bgr, byte> image, Image<Gray, byte> cannyEdges)
        {
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(cannyEdges, contours, null, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            // Find rectangular shapes in the contours, we're looking for the gray box that contains player data
            List<Rectangle> rects = new List<Rectangle>();
            for (int i = 0; i < contours.Size; i++)
            {
                var contour = contours[i];
                var area = CvInvoke.ContourArea(contour);

                if (area > 20000) // Minimum area requirement (arbitrary)
                {
                    var perimeter = CvInvoke.ArcLength(contour, true);
                    var approxCurve = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(contour, approxCurve, 0.04 * perimeter, true);

                    if (approxCurve.Size == 4) // Only consider quadrilaterals
                    {
                        var rect = CvInvoke.BoundingRectangle(approxCurve);
                        image.Draw(rect, new Bgr(0, 255, 0), 2);
                        rects.Add(rect);
                    }
                }
            }

            return rects.Distinct(new RectEqualityComparer()).ToList();
        }

        private static void DisplayImage(Image<Bgr, byte> image, int timeout)
        {
            if (Debugger.IsAttached)
            {
                CvInvoke.Imshow("Result", image);
                CvInvoke.WaitKey(timeout);
            }
        }

        private static void DisplayImage(Image<Gray, byte> image, int timeout)
        {
            if (Debugger.IsAttached)
            {
                CvInvoke.Imshow("Result", image);
                CvInvoke.WaitKey(timeout);
            }
        }

        private static ScoreLog QuandrantizeImage(Bitmap bm)
        {
            try
            {
                // Break the scoring box into quadrants
                int leftWidth = (int)Math.Floor(bm.Width * .35M);
                int rightWidth = bm.Width - leftWidth;
                int height = (int)Math.Floor(bm.Height / 2M);

                // Save an image for 3 of the 4 quadrants that has the data we care about
                // Then read the text out of that image and store it

                // Top Left Quadrant: Name
                using (var topLeft = new Bitmap(leftWidth, height))
                {
                    topLeft.SetResolution(bm.HorizontalResolution, bm.VerticalResolution);
                    using (var g = Graphics.FromImage(topLeft))
                    {
                        g.Clear(Color.Red);
                        g.DrawImage(bm, 0, 0);
                    }
                    topLeft.Save("topleft.png");
                }
                string topLeftText = TextOCR.Execute("topLeft.png").Trim();

                // Top Right Quadrant: Damage
                using (var topRight = new Bitmap(rightWidth, height))
                {
                    topRight.SetResolution(bm.HorizontalResolution, bm.VerticalResolution);
                    using (var g = Graphics.FromImage(topRight))
                    {
                        g.Clear(Color.Red);
                        g.DrawImage(bm, -leftWidth, 0);
                    }
                    topRight.Save("topright.png");
                }

                string topRightText = TextOCR.Execute("topRight.png").Trim();

                // Bottom Right Quadrant: Number of Challenges
                using (var bottomRight = new Bitmap(rightWidth, height))
                {
                    bottomRight.SetResolution(bm.HorizontalResolution, bm.VerticalResolution);
                    using (var g = Graphics.FromImage(bottomRight))
                    {
                        g.Clear(Color.Red);
                        g.DrawImage(bm, -leftWidth, -height);
                    }
                    bottomRight.Save("bottomright.png");
                }

                string bottomRightText = TextOCR.Execute("bottomright.png").Trim();

                // Return a log object of all the text sections (note: IsValid called on it later)
                return new ScoreLog(topLeftText, topRightText, bottomRightText);
            }
            finally
            {
                File.Delete("topLeft.png");
                File.Delete("topRight.png");
                File.Delete("bottomright.png");
            }
        }
    }
}
