#include "NcodeSDK.h"
#include <iostream>
#include <string>
#include <crtdbg.h>

#if _DEBUG
#pragma comment(lib, "NeoLABNcodeSDK_cpp_x64d.lib")
#else
#pragma comment(lib, "NeoLABNcodeSDK_cpp_x64.lib")
#endif

using namespace std;

void GenerateNeoLABNcode()
{
	CNcodeSDK *sdk = new CNcodeSDK();
	
	cout << "Ncode SDK(cpp)version : " << sdk->GetVersion() + "\n";


	cout << "\n";
	cout << "1) Initializing with app secret key\n";


	// this is sample app key for testing
	sdk->Init("juyhgt54redfv7ujmnhgt5esq0poli");


	cout << "\n";
	cout << "2) Getting tickets list (optional)\n";

	vector<TicketInfo> tickets = sdk->GetTickets();

	if (tickets.size() == 0)
	{
		cout << "   Error message : " + sdk->GetLastError() + "\n";
		getchar();
		return;
	}
	
	cout << "   Found " + to_string(tickets.size()) + " ticket(s)\n";

	for (int i = 0; i < tickets.size(); ++i)
	{
		cout << "   Ticket[" + to_string(i) + "]\n";
		if (tickets[i].ncodeType == N3C6)
			cout << "   Type    : N3C6\n";
		else if (tickets[i].ncodeType == G3C6)
			cout << "   Type    : G3C6\n";
		else if (tickets[i].ncodeType == S1C6)
			cout << "   Type    : S1C6\n";
		else if (tickets[i].ncodeType == P1C6)
			cout << "   Type    : P1C6\n";
		cout << "   Section : " + to_string(tickets[i].section) + "\n";
		cout << "   Owner   : " + to_string(tickets[i].ownerStart) + "~" + to_string(tickets[i].ownerStart + tickets[i].ownerSize - 1) + "\n";
		cout << "   Book    : " + to_string(tickets[i].bookStart) + "~" + to_string(tickets[i].bookStart + tickets[i].bookSize - 1) + "\n";
		cout << "   Page    : " + to_string(tickets[i].pageStart) + "~" + to_string(tickets[i].pageStart + tickets[i].pageSize - 1) + "\n";
	}


	cout << "\n";
	cout << "3) Choose ticket and set start page (optional)\n";

	int ticketIndex = 0;
	int ownerOffset = 0;
	int bookOffset = 0;
	int pageOffset = 0;

	TicketInfo startPageInfo = sdk->SetStartPageFromTicket(tickets[ticketIndex], ownerOffset, bookOffset, pageOffset);

	if (sdk->IsValidTicket(startPageInfo) == false)
	{
		cout << "   Ticket range error\n";
		cout << "   Error message : " + sdk->GetLastError() + "\n";
		getchar();
		return;
	}
	cout << "   Selected ticket index : " + to_string(ticketIndex) + "\n";
	cout << "   Owner offset : " + to_string(ownerOffset) + "\n";
	cout << "   Book offset : " + to_string(bookOffset) + "\n";
	cout << "   Page offset : " + to_string(pageOffset) + "\n";


	cout << "\n";
	cout << "4) Set size for inch from paper name (optional)\n";
	
	string paperSizeName = "A10";
	SizeF pageSize = sdk->GetInchValueFromPaperName(
		paperSizeName,		// A4, A6, LETTER, LEGAL...
		false);				// true = landscape, false = portrait
	cout << "   Paper SizeF (" + paperSizeName + ") : " + "(" + to_string(pageSize.width) + ", " + to_string(pageSize.height) + ")\n";


	cout << "\n";
	cout << "5) Generating Ncode data\n";

	vector<NcodePage> codeData;
	int pageCount = 1;
	
	if (sdk->GenerateNcode(
		codeData,			// out
		startPageInfo,
		pageSize.width,     // inch
		pageSize.height,    // inch
		pageCount) != 0)
	{
		cout << "   Error message : " + sdk->GetLastError() + "\n";
		getchar();
		return;
	}

	// You can also create Ncode data via entering code informaion directly.
	// Use it when you do not need to inquiry tickets and you know exactly what code information you need.
	//
	//if (sdk->GenerateNcode(
	//	codeData,		// out
	//	N3C6,			// Ncode type
	//	3,				// section
	//	28,				// owner
	//	10,				// book
	//	1,				// page
	//	6.0,			// inch
	//	8.0,			// inch
	//	pageCount) != 0)
	//{
	//	cout << "   Error message : " + sdk->GetLastError() + "\n";
	//	getchar();
	//	return;
	//}

	cout << "\n";
	cout << "5-1) Saving Ncode png image file\n";
	for (int i = 0; i < pageCount; ++i)
	{
		string outputFilename =
			to_string(codeData[i].section) + "_" +
			to_string(codeData[i].owner) + "_" +
			to_string(codeData[i].book) + "_" +
			to_string(codeData[i].page) + "_" +
			to_string(pageSize.width) + "_" +
			to_string(pageSize.height) + ".png";

		if (sdk->GetImage(codeData[i], 600, outputFilename, false) != 0)
		{
			cout << "   Error message : " + sdk->GetLastError() + "\n";
			getchar();
			return;
		}
	}

	/////////////////////////////////////////////////
	// caution!
	// Temporarily support only S1C6 and P1C6.
	// Using N3C6 and G3C6 causes 343 error code.
	/////////////////////////////////////////////////
	//cout << "5-2) Saving Ncode postscript file\n";
	//{
	//	string outputFilename =
	//		to_string(codeData[0].section) + "_" +
	//		to_string(codeData[0].owner) + "_" +
	//		to_string(codeData[0].book) + "_" +
	//		to_string(codeData[0].page) + "_" +
	//		to_string(codeData.size()) + ".ps";

	//    if (sdk->GetPostscript(codeData, outputFilename) != 0)
	//    {
	//		cout << "   Error message : " + sdk->GetLastError() + "\n";
	//		cout << "   Error message : Temporarily support only S1C6 and P1C6.\n";
	//		_CrtDumpMemoryLeaks();
	//		getchar();
	//        return;
	//    }

	//	cout << "5-2-1) Converting postscript to PDF via Ghostscript\n";
	//	string ghostscriptCmd = "ps2pdf " + outputFilename;
	//	system(ghostscriptCmd.c_str());
	//}

	for (int i = 0; i < pageCount; ++i)
	{
		if (codeData[i].data != NULL)
			delete codeData[i].data;
	}
	tickets.clear();
	codeData.clear();

	cout << "\n";
	cout << "6) Complete\n";
	cout << "   press a key\n";

	delete sdk;
	getchar();

	return;
}

void main()
{
	GenerateNeoLABNcode();
}
