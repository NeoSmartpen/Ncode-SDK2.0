#include <iostream>
#include "main.h"


/*
* Ncoded PDF sample code using mupdf library
* This code can be updated from user's request.
* This project is tested under win32 environment.
*/



static fz_context *ctx = NULL;			// global mupdf context
static pdf_document *doc_dest = NULL;	// for Ncoded pdf



static void add_image_res(pdf_obj *resources, char *name, fz_image *image)
{
	pdf_obj *subres, *ref;

	subres = pdf_dict_get(ctx, resources, PDF_NAME_XObject);
	if (!subres)
	{
		subres = pdf_new_dict(ctx, doc_dest, 10);
		pdf_dict_put_drop(ctx, resources, PDF_NAME_XObject, subres);
	}

	ref = pdf_add_image(ctx, doc_dest, image, 0);
	pdf_dict_puts(ctx, subres, name, ref);
	pdf_drop_obj(ctx, ref);

	fz_drop_image(ctx, image);
}


static void make_contents(fz_buffer* contents, char *name, int w, int h)
{
	char line[4096];

	fz_append_string(ctx, contents, "q");
	fz_append_byte(ctx, contents, '\n');

	sprintf(line, "%d 0 0 %d 0 0 cm", w, h);
	fz_append_string(ctx, contents, line);
	fz_append_byte(ctx, contents, '\n');

	sprintf(line, "/%s Do", name);
	fz_append_string(ctx, contents, line);
	fz_append_byte(ctx, contents, '\n');

	fz_append_string(ctx, contents, "Q");
	fz_append_byte(ctx, contents, '\n');
}


static void create_page(char *name, int w, int h, fz_image *image)
{
	int rotate = 0;


	fz_rect mediabox = { 0, 0, w, h };
	pdf_obj *resources;
	pdf_obj *page;
	fz_buffer *contents;

	// make resource
	resources = pdf_new_dict(ctx, doc_dest, 2);
	add_image_res(resources, name, image);


	// make contents
	contents = fz_new_buffer(ctx, 1024);
	make_contents(contents, name, w, h);


	// make and insert page
	page = pdf_add_page(ctx, doc_dest, &mediabox, rotate, resources, contents);
	pdf_insert_page(ctx, doc_dest, -1, page);


	// release
	pdf_drop_obj(ctx, page);
	fz_drop_buffer(ctx, contents);
	pdf_drop_obj(ctx, resources);
}


void main()
{
	printf("1) Initializing context, handler, pdf document\n\n");
	pdf_write_options opts = { 0 };

	ctx = fz_new_context(NULL, NULL, FZ_STORE_UNLIMITED);

	if (!ctx)
	{
		printf("Cannot initialize context\n\n");
		exit(1);
	}
	pdf_parse_write_options(ctx, &opts, "compress");
	fz_register_document_handlers(ctx);				// get handle
	doc_dest = pdf_create_document(ctx);			// open destination document


	printf("2) Loading source pdf file\n\n");
	fz_document* doc_src;
	doc_src = fz_open_document(ctx, "input.pdf");	// open source document


	printf("3) Making Ncoded pages\n\n");
	int i;
	int pageCount = doc_src->count_pages(ctx, doc_src);
	int dpi = 600;

	for (i = 0; i < pageCount; ++i)
	{
		printf("   3-1(page %d)) Loading source page and Ncode image\n", i);
		fz_page *page;
		fz_rect rect;
		fz_matrix ctm = fz_identity;

		page = fz_load_page(ctx, doc_src, i);		// load page
		fz_bound_page(ctx, page, &rect);			// get size of page

		// calculate scale and set matrix
		float scale = dpi / 72.0f;
		int w = (int)(rect.x1 * scale);
		int h = (int)(rect.y1 * scale);
		fz_scale(&ctm, scale, scale);

		// make Ncoded pixmap
		// pixSrc : background from pdf
		// pixNcode : Ncode image
		// pixRemovedK : destination image = pixSrc(remove K) + pixNcode
		fz_pixmap *pixSrc;
		fz_pixmap *pixRemovedK;

		pixSrc = fz_new_pixmap_from_page_contents(ctx, page, &ctm, fz_device_rgb(ctx), 0);	// RGB
		pixRemovedK = fz_new_pixmap(ctx, fz_device_cmyk(ctx), pixSrc->w, pixSrc->h, 0);		// CMYK

		char name_Ncoded_image[100];	// filename
		sprintf(name_Ncoded_image, "Ncode_%d.png", i);
		fz_image *imageNcodeSrc = fz_new_image_from_file(ctx, name_Ncoded_image);
		fz_pixmap *pixNcode = imageNcodeSrc->get_pixmap(ctx, imageNcodeSrc, NULL, pixSrc->w, pixSrc->h, NULL);


		printf("   3-2(page %d)) Combining background image(removed K) and Ncode image\n", i);
		int x, y;
		for (y = 0; y < pixSrc->h; ++y)
		{
			for (x = 0; x < pixSrc->w; ++x)
			{
				// p_dest = p_src + p_Ncode
				unsigned char *p_src = &pixSrc->samples[(y * pixSrc->w + x) * pixSrc->n];
				unsigned char *p_Ncode = &pixNcode->samples[(y * pixNcode->w + x) * pixNcode->n];
				unsigned char *p_dest = &pixRemovedK->samples[(y * pixSrc->w + x) * pixRemovedK->n];

				// draw Ncode dot
				if (p_Ncode[0] == 0)
				{
					p_dest[0] = 0;
					p_dest[1] = 0;
					p_dest[2] = 0;
					p_dest[3] = 255;
				}
				// draw background with removed K
				else
				{
					p_dest[0] = 255 - p_src[0];		// C
					p_dest[1] = 255 - p_src[1];		// M
					p_dest[2] = 255 - p_src[2];		// Y
					p_dest[3] = 0;					// K = 0
				}
			}
		}

		// final Ncoded pixmap -> image
		fz_image *imageNcoded = fz_new_image_from_pixmap(ctx, pixRemovedK, NULL);


		printf("   3-3(page %d)) Inserting Ncoded image to destination pdf file\n", i);
		char id[100];
		sprintf(id, "I%d", i);
		create_page(id, (int)rect.x1, (int)rect.y1, imageNcoded);

		// release objects
		fz_drop_page(ctx, page);
		fz_drop_pixmap(ctx, pixSrc);
		fz_drop_pixmap(ctx, pixRemovedK);
		fz_drop_pixmap(ctx, pixNcode);
		fz_drop_image(ctx, imageNcodeSrc);
		fz_drop_image(ctx, imageNcoded);

		printf("\n");
	}


	printf("4) Saving Ncoded pdf file\n\n");
	pdf_save_document(ctx, doc_dest, "Ncoded.pdf", &opts);


	// release objects
	pdf_drop_document(ctx, doc_dest);
	fz_drop_document(ctx, doc_src);
	fz_flush_warnings(ctx);
	fz_drop_context(ctx);


	printf("complete");
	getchar();
}