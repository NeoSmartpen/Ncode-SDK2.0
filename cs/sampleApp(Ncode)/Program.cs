using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NeoLABNcodeSDK;
using System.IO;
using System.Diagnostics;

namespace sampleApp_Ncode_
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateNcode();
        }

        static void GenerateNcode()
        {
            CNcodeSDK sdk = new CNcodeSDK();

            Console.WriteLine();
            Console.WriteLine("Ncode SDK version : " + sdk.GetVersion());
            Console.WriteLine();

            Console.WriteLine("1) Initializing with app key");
            Console.WriteLine();
            
            // this is sample app key for testing
            if(sdk.Init("184b265d3aed5ccfab05c6b5167f3") == false)
            {
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }


            Console.WriteLine("2) Getting tickets list (optional)");
            Console.WriteLine();
            List<TicketInfo> tickets = sdk.GetTickets();
            if(tickets == null)
            {
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("   Found " + tickets.Count + " ticket(s)");
            Console.WriteLine();

            ////// notice
            // section 44 is intented for certain firmware.
            // Your neo smartpen may not recognize section 44 code.

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



            Console.WriteLine("3) Choose ticket and set start page (optional)");
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



            Console.WriteLine("4) Set size for inch from paper name (optional)");
            Console.WriteLine();
            string paperSizeName = "A4";
            SizeF pageSize = sdk.GetInchValueFromPaperName(paperSizeName, false);
            Console.WriteLine("   Paper Size (" + paperSizeName + ") : " + "(" + pageSize.Width.ToString() + ", " + pageSize.Height.ToString() + ")");
            Console.WriteLine();



            Console.WriteLine("5) Generating Ncode data");
            Console.WriteLine();
            List<NcodePage> codeData = new List<NcodePage>();
            int pageCount = 1;

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
            //    CNcodeSDK.NCODE_TYPE.N3C6,  // Ncode type
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



            Console.WriteLine("5-1) Saving Ncode image file");
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
            }


            /////////////////////////////////////////////////
            // caution!
            // Temporarily support only S1C6 and P1C6.
            // Using N3C6 and G3C6 causes 343 error code.
            /////////////////////////////////////////////////
            //Console.WriteLine("5-2) Saving Ncode postscript file");
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

            Console.WriteLine("6) Complete");
            Console.WriteLine("   press a key");
            Console.ReadLine();
        }
    }
}
