using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NeoLABNcodeSDK;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using PDFLibAgent;      // NeoLAB wrapper class of Datalogics' Adobe PDF library
using System.IO.Compression;


/*
 * Generating Ncode image & Ncoded PDF sample code 
 * This code can be updated from user's request.
 */

namespace sampleApp_Ncode_cs_adobe_
{
    class Program
    {
        CNcodeSDK sdk = null;   // NeoLAB Ncode SDK
        PDFControl lib = null;  // NeoLAB wrapper class of Datalogics' Adobe PDF library
        static string[] ncodeImageFilename;

        public Program()
        {
            // Initializing libraries
            Console.WriteLine("//////////////////////////");
            Console.WriteLine("  Initializing libraries");
            Console.WriteLine("//////////////////////////");
            Console.WriteLine();

            sdk = new CNcodeSDK();
            lib = new PDFControl();

            // This is sample app key for testing.
            // If you have your app key, enter here.
            string appKey_NcodeSDK = "juyhgt54redfv7ujmnhgt5esq0poli";

            if (!sdk.Init(appKey_NcodeSDK))
            {
                Console.WriteLine("Initializing Ncode SDK failed.");
                return;
            }
            else
            {
                Console.WriteLine("Ncode SDK version : " + sdk.GetVersion());
                Console.WriteLine();
            }

            // Input your Adobe pdf library key.
            // If you don't have it, you should perchase it.
            string libPath = Directory.GetCurrentDirectory();
            string libKey = "Input your key";       // key string is similar to "Gqmeu6ollrqna8Pub5..."
            if (!lib.init(libPath, libKey))
            {
                Console.WriteLine("Initializing Adobe PDF lib failed.");
                return;
            }




            Console.WriteLine("///////////////////");
            Console.WriteLine("  Creating Ncode");
            Console.WriteLine("///////////////////");
            Console.WriteLine();

            IPDFDocument doc = lib.openDocument("input_sample.pdf");
            int pageCount = doc.getPageCount();        
            ncodeImageFilename = new string[pageCount];
            GenerateNcode(pageCount);




            Console.WriteLine("////////////////////////");
            Console.WriteLine("  Creating Ncoded PDF");
            Console.WriteLine("////////////////////////");

            RemoveK_and_AddNcode_from_Image("input_sample.pdf", "output.pdf", ncodeImageFilename);
            lib.libraryCleanUp();




            Console.WriteLine();
            Console.WriteLine("-- Complete --");
            Console.ReadLine();
        }


        static void Main(string[] args)
        {
            new Program();
        }


        /// <summary>
        /// Request Ncode data and generate Ncode image.
        /// </summary>
        void GenerateNcode(int pageCount)
        {
            // If you want getting your ticket information, inquiry ticket list.
            // But this is an optional process.
            Console.WriteLine("1) Getting tickets list");
            Console.WriteLine();
            List<TicketInfo> tickets = sdk.GetTickets();
            if (tickets == null)
            {
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }
            Console.WriteLine("   Found " + tickets.Count + " ticket(s)");
            Console.WriteLine();

            for (int i = 0; i < tickets.Count; ++i)
            {
                Console.WriteLine("   Ticket[" + i.ToString() + "]");
                if (tickets[i].ncodeType == NCODE_TYPE.N3C6)
                    Console.WriteLine("   Type    : N3C6");
                else if (tickets[i].ncodeType == NCODE_TYPE.G3C6)
                    Console.WriteLine("   Type    : G3C6");
                else if (tickets[i].ncodeType == NCODE_TYPE.S1C6)
                    Console.WriteLine("   Type    : S1C6");
                else if (tickets[i].ncodeType == NCODE_TYPE.P1C6)
                    Console.WriteLine("   Type    : P1C6");
                Console.WriteLine("   Section : " + tickets[i].section.ToString());
                Console.WriteLine("   Owner   : " + tickets[i].ownerStart.ToString() + "~" + (tickets[i].ownerStart + tickets[i].ownerSize - 1).ToString());
                Console.WriteLine("   Book    : " + tickets[i].bookStart.ToString() + "~" + (tickets[i].bookStart + tickets[i].bookSize - 1).ToString());
                Console.WriteLine("   Page    : " + tickets[i].pageStart.ToString() + "~" + (tickets[i].pageStart + tickets[i].pageSize - 1).ToString());
                Console.WriteLine("   Info    : " + tickets[i].extraInfo);
                Console.WriteLine("   Period  : " + tickets[i].period);

                Console.WriteLine();
            }



            Console.WriteLine("2) Choose ticket and set start page (optional)");
            Console.WriteLine();

            int ticketIndex = 0;
            int ownerOffset = 0;
            int bookOffset = 0;
            int pageOffset = 0;
            TicketInfo startPageInfo = sdk.SetStartPageFromTicket(tickets[ticketIndex], ownerOffset, bookOffset, pageOffset);

            if (startPageInfo == null)
            {
                Console.WriteLine("   Ticket range error");
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }
            Console.WriteLine("   Selected ticket index : " + ticketIndex.ToString());
            Console.WriteLine("   Owner offset : " + ownerOffset.ToString());
            Console.WriteLine("   Book offset : " + bookOffset.ToString());
            Console.WriteLine("   Page offset : " + pageOffset.ToString());
            Console.WriteLine();



            Console.WriteLine("3) Set size for inch from paper name (optional)");
            Console.WriteLine();
            string paperSizeName = "A4";
            SizeF pageSize = sdk.GetInchValueFromPaperName(paperSizeName, false);
            Console.WriteLine("   Paper Size (" + paperSizeName + ") : " + "(" + pageSize.Width.ToString() + ", " + pageSize.Height.ToString() + ")");
            Console.WriteLine();



            Console.WriteLine("4) Generating Ncode data");
            Console.WriteLine();

            List<NcodePage> codeData = new List<NcodePage>();

            if (sdk.GenerateNcode(
                out codeData,
                startPageInfo,
                pageSize.Width,     // inch
                pageSize.Height,    // inch
                pageCount) != 0)
            {
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }

            // You can also create Ncode data via entering code informaion directly.
            // Use it when you do not need to inquiry tickets and you know exactly what code information you need.
            //
            //if (sdk.GenerateNcode(
            //    out codeData,
            //    NCODE_TYPE.N3C6,  // Ncode type
            //    3,                          // section
            //    28,                         // owner
            //    10,                         // book
            //    1,                          // page
            //    6.0,                        // inch
            //    8.0,                        // inch
            //    pageCount) != 0)
            //{
            //    Console.WriteLine("   Error message : " + sdk.GetLastError());
            //    Console.ReadLine();
            //    return;
            //}

            Console.WriteLine("4-1) Saving Ncode image file");
            Console.WriteLine();

            // When you generate N3C6 or G3C6 code image, you can select dot type, "dot" or "line".
            // If you set "true", it generate "dot" code image.
            // If you set "false", it generate "line" code image.
            // S1C6, P1C6 and postscript output use just only "dot" code.
            sdk.SetDotType(false);

            for (int i = 0; i < pageCount; ++i)
            {
                string outputFilename = string.Format("{0}_{1}_{2}_{3}_{4}_{5}.png",
                    codeData[i].section.ToString(),
                    codeData[i].owner.ToString(),
                    codeData[i].book.ToString(),
                    codeData[i].page.ToString(),
                    pageSize.Width,
                    pageSize.Height
                    );
                if (sdk.GetImage(codeData[i], 600, outputFilename, false) != 0)
                {
                    Console.WriteLine("   Error message : " + sdk.GetLastError());
                    Console.ReadLine();
                    return;
                }
                ncodeImageFilename[i] = outputFilename;
            }


            /////////////////////////////////////////////////
            // caution!
            // Temporarily support only S1C6 and P1C6.
            // Using N3C6 and G3C6 causes 343 error code.
            /////////////////////////////////////////////////
            //Console.WriteLine("4-2) Saving Ncode postscript file");
            //Console.WriteLine();

            //{
            //    string outputFilename = string.Format("{0}_{1}_{2}_{3}-{4}.ps",
            //            codeData[0].section.ToString(),
            //            codeData[0].owner.ToString(),
            //            codeData[0].book.ToString(),
            //            codeData[0].page.ToString(),
            //            codeData.Count - 1);
            //    if (sdk.GetPostscript(codeData, outputFilename) != 0)
            //    {
            //        Console.WriteLine("   Error message : " + sdk.GetLastError());
            //        Console.WriteLine("   Error message : Temporarily support only S1C6 and P1C6.");
            //        Console.ReadLine();
            //        return;
            //    }

            //    Console.WriteLine("5-2-1) Converting postscript to PDF via Ghostscript");
            //    Console.WriteLine();

            //    Process process = new System.Diagnostics.Process();
            //    process.StartInfo.FileName = "cmd.exe";
            //    process.StartInfo.Arguments = "/C ps2pdf " + outputFilename;
            //    process.Start();
            //}

            Console.WriteLine("5) Ncode created");
            Console.WriteLine();
        }


        /// <summary>
        /// 1) Remove carbon black field(K) from CMYK colorspace from PDF file.
        /// 2) Make Ncoded PDF with Ncode image files.
        /// </summary>
        /// <param name="inputPdfFilename"></param>
        /// <param name="outputPdfFilename"></param>
        /// <param name="ncodeIamgeFilenames"></param>
        void RemoveK_and_AddNcode_from_Image(string inputPdfFilename, string outputPdfFilename, string[] ncodeIamgeFilenames)
        {
            IPDFDocument doc = lib.openDocument(inputPdfFilename);
            IPDFDocument newDoc = lib.copyDocumentOnlyPageStructure(doc, false);

            if (doc == null)
            {
                Console.WriteLine("   Cannot open input PDF file.");
                return;
            }

            if (doc.getPageCount() != ncodeImageFilename.Length)
            {
                Console.WriteLine("   Page count is not correct");
                Console.WriteLine("   input PDF pages : " + doc.getPageCount().ToString());
                Console.WriteLine("   Ncode image pages : " + ncodeImageFilename.Length.ToString());

                return;
            }

            Console.WriteLine();
            Console.WriteLine("1) Removing carbon black field(K) from CMYK colorspace from PDF file");
            Console.WriteLine();

            for (int i = 0; i < newDoc.getPageCount(); i++)
            {
                using (IPDFPage newPage = newDoc.getPageObj(i))
                {
                    if (newPage.convertColorSpaceToCMY(doc, 200) == false)
                    {
                        Console.WriteLine(newPage.lastErrorMsg());
                        return;
                    }
                }
            }


            Console.WriteLine("2) Making Ncoded PDF with Ncode image files.");
            Console.WriteLine();
            /////////////////////////////////////////////////////////////////////
            // caution : This code will not work unless Ncode image's bpp is 1.
            /////////////////////////////////////////////////////////////////////
            System.IO.MemoryStream[] ms = new System.IO.MemoryStream[ncodeIamgeFilenames.Length];

            for (int i = 0; i < ncodeIamgeFilenames.Length; ++i)
            {
                ms[i] = new System.IO.MemoryStream();
                System.Drawing.Image ss = System.Drawing.Image.FromFile(ncodeIamgeFilenames[i]);
                ss.Save(ms[i], System.Drawing.Imaging.ImageFormat.Tiff);

                using (var page = newDoc.getPageObj(i))
                {
                    double x0, y0, x1, y1;
                    x0 = y0 = 0;
                    x1 = ss.Width * 72 / 600;
                    y1 = ss.Height * 72 / 600;

                    if (page.addImageContentOver_usingStream(ms[i], true, x0, y0, x1, y1) == false)
                    {
                        Console.WriteLine(page.lastErrorMsg());
                        return;
                    }
                }

                ms[i].Dispose();
            }

            newDoc.saveDocumentAs(outputPdfFilename);
            newDoc.closeDocument();
            doc.closeDocument();
        }
    }
}
