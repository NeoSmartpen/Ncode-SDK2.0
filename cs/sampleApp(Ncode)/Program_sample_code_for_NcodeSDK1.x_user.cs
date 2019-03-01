using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NeoLABNcodeSDK;
using System.IO;

namespace sampleApp_Ncode_
{
    class Program
    {
        static void Main(string[] args)
        {
            Generate_Ncode_With_Predeterminded_PageInfo();
        }

        static void Generate_Ncode_With_Predeterminded_PageInfo()
        {
            CNcodeSDK1 sdk = new CNcodeSDK1();


            Console.WriteLine("1) Initializing with app key");
            Console.WriteLine();
            
            // this is sample app key for testing
            sdk.Init("juyhgt54redfv7ujmnhgt5esq0poli");



            Console.WriteLine("2) Getting tickets list");
            Console.WriteLine();
            CNcodeSDK1.TicketInfo[] tickets;
            int getTicketRet = sdk.GetTickets(out tickets);
            if (getTicketRet == -1)
            {
                Console.WriteLine("   Ticket error : " + getTicketRet.ToString());
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }
            Console.WriteLine("   Found " + getTicketRet.ToString() + " ticket(s)");
            Console.WriteLine();

            for (int i = 0; i < tickets.Length; ++i)
            {
                Console.WriteLine("   Ticket[" + i.ToString() + "]");
                Console.Write("   Ncode type : ");
                if (tickets[i].type == 0) Console.WriteLine("N3C6");
                else if (tickets[i].type == 1) Console.WriteLine("G3C6");   // v2.0 new feature
                else if (tickets[i].type == 2) Console.WriteLine("S1C6");   // v2.0 new feature
                else if (tickets[i].type == 3) Console.WriteLine("P1C6");   // v2.0 new feature
                Console.WriteLine("   Section : " + tickets[i].section.ToString());
                Console.WriteLine("   Owner   : " + tickets[i].owner.ToString());
                Console.WriteLine("   Book    : " + tickets[i].bookStart.ToString() + "~" + (tickets[i].bookStart + tickets[i].bookSize - 1).ToString());
                Console.WriteLine("   Page    : " + tickets[i].pageStart.ToString() + "~" + (tickets[i].pageStart + tickets[i].pageSize - 1).ToString());
                Console.WriteLine();
            }


            Console.WriteLine("3) Choose ticket and set page for generating");
            Console.WriteLine();

            int ticketIndex = 1;
            int bookOffset = 0;
            int pageOffset = 0;
            CNcodeSDK1.TicketInfo pageInfo = sdk.SetStartPageByOffset(tickets[ticketIndex], bookOffset, pageOffset);

            if (pageInfo == null)
            {
                Console.WriteLine("   Ticket range error");
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }
            Console.WriteLine("   Selected ticket index : " + ticketIndex.ToString());
            Console.WriteLine("   Book offset : " + bookOffset.ToString());
            Console.WriteLine("   Page offset : " + pageOffset.ToString());
            Console.WriteLine();


            Console.WriteLine("3-1) Get size from paper size name");
            Console.WriteLine();
            string paperSizeName = "A4";
            int dpi = 600;
            Size imgSize = sdk.GetImageSizeFromPaperSize(paperSizeName, dpi, false);
            Console.WriteLine("   Paper Size (" + paperSizeName + ") : " + "(" + imgSize.Width.ToString() + ", " + imgSize.Height.ToString() + ")");
            Console.WriteLine();




            Console.WriteLine("4) Generating Ncode data");
            Console.WriteLine();
            sdk.SetDotType(false);      // v2.0 new feature
            CNcodeSDK1.NcodeData codeData = sdk.GenerateNcode(
                pageInfo,               // page information from tickets
                imgSize.Width,          // width (pixel)
                imgSize.Height,         // height (pixel)
                dpi,                    // dpi (600 or 1200)
                false);                 // is bold

            if (codeData.errorCode != 0)
            {
                Console.WriteLine("   Generate Ncode error : " + codeData.errorCode.ToString());
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }
            else
            {
                Console.WriteLine("   Generating Ncode complete");
                Console.WriteLine("   Section : " + codeData.section.ToString());
                Console.WriteLine("   Owner : " + codeData.owner.ToString());
                Console.WriteLine("   Book : " + codeData.book.ToString());
                Console.WriteLine("   Page : " + codeData.page.ToString());
                Console.WriteLine("   Image size : (" + codeData.imgWidth.ToString() + "," + codeData.imgHeight.ToString() + ")");
                Console.WriteLine();
            }



            Console.WriteLine("4-1) Saving Ncode image file");
            Console.WriteLine();
            string outputFilename =
                pageInfo.section.ToString() + "_" +
                pageInfo.owner.ToString() + "_" +
                pageInfo.bookStart.ToString() + "_" +
                pageInfo.pageStart.ToString() + ".png";

            codeData.image.Save(outputFilename);

            // v2.0 new feature
            Console.WriteLine("5) Generate Ncode postscipt data");
            Console.WriteLine();
            CNcodeSDK1.NcodeData codeDataPostscript = sdk.GenerateNcodedPS(
                pageInfo,
                imgSize.Width,          // width (pixel)
                imgSize.Height,         // height (pixel)
                dpi,
                1.0);

            if (codeDataPostscript.errorCode != 0)
            {
                Console.WriteLine("   Generate Ncode error : " + codeData.errorCode.ToString());
                Console.WriteLine("   Error message : " + sdk.GetLastError());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("5-1) Saving Ncode postscript file");
            Console.WriteLine();
            string outputPsFilename =
                pageInfo.section.ToString() + "_" +
                pageInfo.owner.ToString() + "_" +
                pageInfo.bookStart.ToString() + "_" +
                pageInfo.pageStart.ToString() + ".ps";

            File.WriteAllText(outputPsFilename, codeDataPostscript.data);

            Console.WriteLine("6) Complete");
            Console.WriteLine("   press a key");
            Console.ReadLine();
        }
    }
}
