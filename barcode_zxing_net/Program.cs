using ImageMagick;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace barcode_zxing_net
{
    class Program
    {
        static void Main(string[] args)
        {
            BarcodeReader reader = new BarcodeReader { AutoRotate = true, TryInverted = true };

            string path = "barcode_test_1.jpg";
            Console.WriteLine($"Can read JPG? Using {path}");
            var bmp1 = (Bitmap)Bitmap.FromFile(path);
            WriteResult(reader.Decode(bmp1));

            path = "barcode_test_2.png";
            Console.WriteLine($"Can read PNG? Using {path}");
            var bmp2 = (Bitmap)Bitmap.FromFile(path);
            WriteResult(reader.Decode(bmp2));

            path = "barcode_test_1.jpg";
            Console.WriteLine($"Can read JPG loaded w/ MagickImage? Using {path}");
            var image3 = new MagickImage(path);
            WriteResult(reader.Decode(image3.ToBitmap()));

            path = "barcode_test_2.png";
            Console.WriteLine($"Can read PNG loaded w/ MagickImage? Using {path}");
            var image4 = new MagickImage(path);
            WriteResult(reader.Decode(image4.ToBitmap()));

            path = "png_screenshot_from_PDF.png";
            Console.WriteLine($"Can read PNG screenshotted from PDF? Using {path}");
            var image5 = new MagickImage(path);
            WriteResult(reader.Decode(image5.ToBitmap()));

            path = "png_screenshot_from_pdf_then_saved_to_pdf.pdf";
            Console.WriteLine($"Can read PDF printed from PNG screenshotted from PDF? Using {path}");
            var page = 0;
            foreach (var bitmap in GetBitmapsFromPdf(path))
            {
                page++;
                Console.WriteLine($"Reading page {page}");
                WriteResult(reader.Decode(bitmap));
            }

            path = "barcode_test.pdf";
            Console.WriteLine($"Can read barcode on a PDF sheet? Using {path}");
            page = 0;
            foreach (var bitmap in GetBitmapsFromPdf(path))
            {
                page++;
                Console.WriteLine($"Reading page {page}");
                WriteResult(reader.Decode(bitmap));
            }

            path = "barcode_original_small_size_poor_quality.pdf";
            Console.WriteLine($"Can read small, blurry barcode on a PDF sheet? Using {path}");
            page = 0;
            foreach (var bitmap in GetBitmapsFromPdf(path))
            {
                page++;
                Console.WriteLine($"Reading page {page}");
                WriteResult(reader.Decode(bitmap));
            }






            //Console.WriteLine($"Can read PDF printed from PNG screenshotted from PDF? Using {path}");
            //var settings = new MagickReadSettings { Density = new Density(600) };
            //using (MagickImageCollection images = new MagickImageCollection())
            //{
            //    images.Read(path, settings);
            //    var page1 = images[0];
            //    var page1Bmp = page1.ToBitmap(ImageFormat.Png, BitmapDensity.Use);
            //    WriteResult(reader.Decode(page1Bmp));
            //}



            //path = "barcode_test_3.bmp";
            //Console.WriteLine($"test {path}");
            //var bmp4 = (Bitmap)Bitmap.FromFile(path);
            //WriteResult(reader.Decode(bmp4));
            ////// JPG test
            //Console.WriteLine("Decoding from JPG");
            //var barcodeBitmap = (Bitmap)Bitmap.FromFile(JPG_PATH);
            ////for (var i = 0; i < 1000; i++)
            ////{
            //    WriteResult(reader.Decode(barcodeBitmap));
            ////}
            ////barcodeBitmap.Save("barcodeJpegRewritten.png", ImageFormat.Png);

            //// JPG test w/ imagemagick
            //Console.WriteLine("Decoding from JPG using ImageMagic");
            //var mBitmap = new MagickImage(JPG_PATH);
            //WriteResult(reader.Decode(mBitmap.ToBitmap()));

            //// JPG using output image
            //Console.WriteLine("Decoding from a PNG originally from a JPG");
            //barcodeBitmap.Save("barcodeJpegRewritten.png", ImageFormat.Png);
            //var m2Bitmap = new MagickImage("barcodeJpegRewritten.png");
            //WriteResult(reader.Decode(m2Bitmap.ToBitmap()));

            //// PDF test
            //Console.WriteLine("Decoding from PDF");
            //MagickReadSettings settings = new MagickReadSettings();
            //settings.Density = new Density(1000);
            //using (MagickImageCollection images = new MagickImageCollection())
            //{
            //    images.Read(PDF_PATH, settings);
            //    int page = 1;
            //    foreach (MagickImage image in images)
            //    {
            //        var pdfBitmap = image.ToBitmap(ImageFormat.Png, BitmapDensity.Use);
            //        pdfBitmap.Save($"{page}-{PDF_PATH}.2.bmp");
            //        WriteResult(reader.Decode(pdfBitmap));
            //        //image.Write($"{page}-{PDF_PATH}.png");
            //        var secondPdfBitmap = new MagickImage($"{page}-{PDF_PATH}.2.bmp");
            //        WriteResult(reader.Decode(secondPdfBitmap.ToBitmap()));
            //        page++;
            //    }
            //}
            ////var pdfBitmap = (Bitmap)Bitmap.FromFile(PDF_PATH);
            ////WriteResult(reader.Decode(pdfBitmap));

            //Console.WriteLine("Decoding from manually saved");
            //var manBitmap = (Bitmap)Bitmap.FromFile("manually_saved.png");
            //WriteResult(reader.Decode(manBitmap)); 

            //Console.WriteLine("Decoding from manually saved");
            //var manJpgBitmap = (Bitmap)Bitmap.FromFile("manually_saved_jpg.jpg");
            //WriteResult(reader.Decode(manJpgBitmap));

            //Console.WriteLine("Press any key to exit..."); 
            //Console.ReadKey();

        }


        private static IEnumerable<Bitmap> GetBitmapsFromPdf(string path)
        {
            var doc = PdfReader.Open(path);
            foreach (var page in doc.Pages)
            {
                // Get resources dictionary
                PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                if (resources != null)
                {
                    // Get external objects dictionary
                    PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                    if (xObjects != null)
                    {
                        var items = xObjects.Elements.Values;
                        // Iterate references to external objects
                        foreach (PdfItem item in items)
                        {
                            PdfReference reference = item as PdfReference;
                            if (reference != null)
                            {
                                PdfDictionary xObject = reference.Value as PdfDictionary;
                                // Is external object an image?
                                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                {
                                    var bitmap = ExportImage(xObject);
                                    if (bitmap != null)
                                    {
                                        //WriteResult(reader.Decode(bitmap));
                                        //Console.WriteLine("Bitmap is not null");
                                        yield return bitmap;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Bitmap ExportImage(PdfDictionary image)
        {
            var filter = string.Empty;
            var obj = image.Elements.GetObject("/Filter");
            if (obj is PdfArray)
            {
                filter = ((PdfArray)obj).Elements.GetName(0);
                foreach (var element in ((PdfArray)obj).Elements)
                {
                    // TODO
                }
            }
            else
            {
                filter = image.Elements.GetName("/Filter");
            }
            switch (filter)
            {
                case "/DCTDecode":
                    return ExportJpegImage(image);

                case "/FlateDecode":
                    return ExportAsPngImage(image);

                case "/JBIG2Decode":
                    break;
            }
            return null;
        }

        private static Bitmap ExportJpegImage(PdfDictionary image)
        {
            byte[] stream = image.Stream.Value;
            using (var memStream = new MemoryStream())
            {
                using (var bw = new BinaryWriter(memStream))
                {
                    bw.Write(stream);
                    memStream.Position = 0;
                    var result = (Bitmap)Bitmap.FromStream(memStream);
                    bw.Close();

                    return result;
                }
            }
        }

        private static Bitmap ExportAsPngImage(PdfDictionary image)
        {
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            // TODO: You can put the code here that converts vom PDF internal image format to a Windows bitmap
            // and use GDI+ to save it in PNG format.
            // It is the work of a day or two for the most important formats. Take a look at the file
            // PdfSharp.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
            // We don't need that feature at the moment and therefore will not implement it.
            // If you write the code for exporting images I would be pleased to publish it in a future release
            // of PDFsharp.

            return null;
        }

        static void ImageTest(string path)
        {
            Console.WriteLine();
            BarcodeReader reader = new BarcodeReader { AutoRotate = true, TryInverted = true };
            Console.WriteLine($"test {path}");
            var bmp1 = (Bitmap)Bitmap.FromFile(path);
            WriteResult(reader.Decode(bmp1));
        }

        static void WriteResult(ZXing.Result result)
        {
            if (result != null)
            {
                Console.WriteLine($"Decoded barcode {result.Text}");
                Console.WriteLine($"Format is {result.BarcodeFormat}");
            }
            else
            {
                Console.WriteLine("Unable to recognize barcode");
            }
            Console.WriteLine();

        }

    }


}
