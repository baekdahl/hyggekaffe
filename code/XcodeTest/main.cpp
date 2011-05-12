#include <cv.h>
#include <cxcore.h>
#include <highgui.h>
#include <stdio.h>
#include <stdlib.h>

#define NUM_BALLS 16
#define patchx 35
#define patchy 35

using namespace std;

string intToStr(int someint)
{
	std::stringstream strm;
	std::string num;
	strm << someint;
	strm >> num;
	
	return num;
}

void help(){
	printf("\nCall is:\n" 
		   "  ch7_ex7_5_HistBackProj modelImage testImage patch_type\n"
		   "     patch_type takes on the following methods of matching:\n"
		   "        0 Correlation, 1 ChiSqr, 2 Intersect, 3 Bhattacharyya\n"
		   "     Projection is done using cvCalcBackProject()\n\n");
}

//Learns histogram from derived from the first image, and backprojects it onto the second 
//  If patch_type is present, it does cvCalcBackProjectPatch()
//     patch_type takes on the following methods of matching:
//     0 Correlation, 1 ChiSqr, 2 Intersect, 3 Bhattacharyya
//  it does cvCalcBackProject().
// Call is: 
//    ch7BackProj modelImage testImage patch_type
// 
CvPoint matchHist(IplImage* patchImage, IplImage* image, IplImage* resultImage, CvMat *matchMask) {
	
    IplImage* src[2],*dst=0,*ftmp=0; //dst is what to display on
	int i = 0;
	int type = 3;

	//Load 2 images, first on is to build histogram of, 2nd is to run on
	src[0] = patchImage; src[1] = image;
	
	// Compute the HSV image, and decompose it into separate planes.
	//
	IplImage *hsv[2], *h_plane[2],*s_plane[2],*v_plane[2],*planes[2][2]; 
	IplImage* hist_img[2];
	CvHistogram* hist[2];
	// int h_bins = 30, s_bins = 32; 
	int h_bins = 16, s_bins = 16;
	int    hist_size[] = { h_bins, s_bins };
	float  h_ranges[]  = { 0, 180 };          // hue is [0,180]
	float  s_ranges[]  = { 0, 255 }; 
	float* ranges[]    = { h_ranges, s_ranges };
	int scale = 10;
#define patchx 35
#define patchy 35

		int iwidth = src[1]->width - patchx + 1;
		int iheight = src[1]->height - patchy + 1;
		ftmp = cvCreateImage( cvSize(iwidth,iheight),32,1);
		cvZero(ftmp);

	dst = cvCreateImage( cvGetSize(src[1]),8,1);
	
	cvZero(dst);
	for(i = 0; i<2; ++i){ 
		hsv[i] = cvCreateImage( cvGetSize(src[i]), 8, 3 ); 
		cvCvtColor( src[i], hsv[i], CV_BGR2HSV );
		
		h_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		s_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		v_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		planes[i][0] = h_plane[i];
		planes[i][1] = s_plane[i];
		cvCvtPixToPlane( hsv[i], h_plane[i], s_plane[i], v_plane[i], 0 );
		// Build the histogram and compute its contents.
		//
		hist[i] = cvCreateHist( 
							   2, 
							   hist_size, 
							   CV_HIST_ARRAY, 
							   ranges, 
							   1 
							   ); 
		cvCalcHist( planes[i], hist[i], 0, 0 );

		cvNormalizeHist( hist[i], 1.0 ); //Don't normalize for cvCalcBackProject(), 
		
		// Create an image to use to visualize our histogram.
		//
		hist_img[i] = cvCreateImage(  
									cvSize( h_bins * scale, s_bins * scale ), 
									8, 
									3
									); 
		cvZero( hist_img[i] );
		
		// populate our visualization with little gray squares.
		//
		float max_value = 0;
		float *fp,fval;
		cvGetMinMaxHistValue( hist[i], 0, &max_value, 0, 0 );
		
		for( int h = 0; h < h_bins; h++ ) {
			for( int s = 0; s < s_bins; s++ ) {
				float bin_val = cvQueryHistValue_2D( hist[i], h, s );
				int intensity = cvRound( bin_val * 255 / max_value );
				cvRectangle( 
							hist_img[i], 
							cvPoint( h*scale, s*scale ),
							cvPoint( (h+1)*scale - 1, (s+1)*scale - 1),
							CV_RGB(intensity,intensity,intensity), 
							CV_FILLED
							);
			}
		}
	}//For the 2 images
	
	//DO THE BACK PROJECTION

	printf("Doing cvCalcBackProjectPatch() with type =%d\n",type);
	cvCalcBackProjectPatch(planes[1],ftmp,cvSize(35,35),hist[0],type,1.0);
	printf("ftmp count = %d\n",cvCountNonZero(ftmp));

	
	double min, max;
	CvPoint minl, maxl;
	
	cvMinMaxLoc(ftmp, &min, &max, &minl, &maxl, matchMask);
	
	cvNamedWindow( "Back Projection",0);
	cvShowImage(   "Back Projection", ftmp );
	return minl;
}

int main()
{
	IplImage* ball, *table, *resultImage;
	CvMat *matchMask;
	CvFont font;
	CvPoint ballLocation;
	
	cvInitFont(&font, CV_FONT_HERSHEY_PLAIN, 1.0f, 1.0f, 1.0f, 1.5f);
	
	cvNamedWindow( "Table", 0);
	cvNamedWindow( "Back Projection",0);
	cvNamedWindow( "Ball", 0);
	cvNamedWindow("Matching mask", 0);
	
	
	cvWaitKey(0);
	
	//const int matchSeq[16] = {8, 16, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15}
	string path;
	for (int tableNum=0; tableNum<2; tableNum++) {
		path = "tables/" + intToStr(tableNum) + ".jpg";
		table = cvLoadImage(path.c_str(),1);
		//Initialize matchin mask which excludes already matched ballareas from matching
		matchMask = cvCreateMat(table->height - patchx + 1, table->width - patchy + 1, CV_8UC1);
		cvSet(matchMask, cvScalar(1));
		
		for (int ballNum=0; ballNum<NUM_BALLS; ballNum++) {
			path = "balls/" + intToStr(ballNum) + ".jpg";
			ball = cvLoadImage(path.c_str(),1);
			
			ballLocation = matchHist(ball, table, resultImage, matchMask);
			//Update matching mask
			cvCircle(matchMask, ballLocation, 14, cvScalar(0), -1);
			cvShowImage("Matching mask", matchMask);

			//Display the matched balls on original image
			ballLocation.x += (patchx/2)+1;
			ballLocation.y += patchy/2+1;
			cvCircle(table, ballLocation, 14, cvScalar(255,255,255), 2);
			ballLocation.x += 2;
			ballLocation.y += 2;
			cvPutText(table, intToStr(ballNum).c_str(), ballLocation, &font, cvScalar(255,255,255));
			cvShowImage("Table", table);
			
			
			
			//cvWaitKey(0);
		
		}
		cvWaitKey(0);
	}
}