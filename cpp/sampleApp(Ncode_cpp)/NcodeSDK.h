#pragma once

#include <iostream>
#include <string>
#include <vector>
#include <cstddef>

using namespace std;

enum NCODE_TYPE
{
	NONE = 0,
	N3C6,
	G3C6,
	S1C6,   // Scode
	P1C6    // Acode, Pcode
};

typedef struct _NcodePage
{
	NCODE_TYPE ncodeType;
	int section = 0;
	int owner = 0;
	int book = 0;
	int page = 0;
	double width = 0.0;
	double height = 0.0;
	unsigned char *data = NULL;
	int dataLength = 0;
} NcodePage;

typedef struct _TicketInfo
{
	NCODE_TYPE ncodeType;
	int section = 0;
	int ownerStart = 0;
	int ownerSize = 0;
	int bookStart = 0;
	int bookSize = 0;
	int pageStart = 0;
	int pageSize = 0;
	int period = 0;
	string extraInfo = "";
} TicketInfo;

typedef struct _SizeF
{
	float width;
	float height;
} SizeF;

typedef struct _PointF
{
	float x;
	float y;
} PointF;

class CNcodeSDK
{
public:
	CNcodeSDK();
	~CNcodeSDK();

private:
	string appSecretKey;
	string lastErrorMsg;
	string version;
	string cacheFolder;
	string workingFolder;
	bool isDot;
	int dotPixSize;

public:
	void Init(string appSecretKey, string workingFolder = "", string cacheFolder = "");
	vector<TicketInfo> GetTickets();
	TicketInfo SetStartPageFromTicket(TicketInfo ticket, int ownerOffset, int bookOffset, int pageOffset);
	bool IsValidTicket(TicketInfo ticket);
	SizeF GetInchValueFromPaperName(string paper, bool isLandscape = false);
	int GenerateNcode(vector<NcodePage> &pages, TicketInfo startPageInfo, float width, float height, int pageCount);
	int GenerateNcode(vector<NcodePage> &pages, NCODE_TYPE ncodeType, int section, int owner, int book, int page, float width, float height, int pageCount);
	int GetImage(NcodePage page, int dpi, string filename, bool isBold);
	int GetPostscript(vector<NcodePage> &pages, string filename, float dotScale = 1.0);
	string GetLastError();
	string GetVersion();

private:
	string RequestRestAPI(string json, string url);
	bool CheckCodeInfoRange(int s, int o, int b, int p, NCODE_TYPE ncodeType = N3C6);
	bool CheckImageSize(int s, int w, int h, int dpi, NCODE_TYPE ncodeType = N3C6);
	int GetImageData(unsigned char *data, NCODE_TYPE ncodeType, double widthInch, double heightInch, int dpi, bool isBold, string filename);
	void DrawNcodeDot(unsigned char *buf, int x, int y, int w, int h, int value, int lineLength, int dpi, bool isBold, NCODE_TYPE ncodeType);
	vector<string> GetNcodeString(int width, int height, int dpi, unsigned char *data, int dataLength, NCODE_TYPE ncodeType = N3C6);
	int GetScodeData(unsigned char oneData);
	void WriteLineCircle(ofstream &file, SizeF *cSize, PointF point);
};

