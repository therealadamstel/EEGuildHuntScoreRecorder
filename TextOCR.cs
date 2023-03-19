using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesseractOCR;
using TesseractOCR.Enums;

namespace EEGuildHuntTool
{
    static class TextOCR
    {
        private static readonly Engine _tessEngine;

        static TextOCR()
        {
            _tessEngine = new Engine("./tessdata", "eng", EngineMode.Default);
            _tessEngine.SetVariable("tessedit_pageseg_mode", "7");
        }

        public static Page ReadPage(string imagePath, PageSegMode pageSegMode = PageSegMode.Auto)
        {
            // Load the image file
            using (TesseractOCR.Pix.Image image = TesseractOCR.Pix.Image.LoadFromFile(imagePath))
                return _tessEngine.Process(image, pageSegMode);
        }

        public static string Execute(string imagePath)
        {
            // Recognize text from image
            using (var page = ReadPage(imagePath))
            {
                return page.Text;
            }
        }
    }
}
