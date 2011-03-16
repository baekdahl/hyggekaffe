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
CvPoint matchHist(IplImage* patchImage, IplImage* image, IplImage* resultImage) {
	
    IplImage* src[2],*dst=0,*ftmp=0, *largeBall=0; //dst is what to display on
	int i = 0;
	int type = 1;

	//Load 2 images, first on is to build histogram of, 2nd is to run on
	src[0] = patchImage; src[1] = image;
	
	// Compute the HSV image, and decompose it into separate planes.
	//
	IplImage *hsv[2], *r_plane[2],*g_plane[2],*b_plane[2],*planes[2][3]; 
	IplImage* hist_img[2];
	CvHistogram* hist[2];
	int bins = 30;

	int    hist_size[] = { bins, bins, bins};
	float  r_ranges[]  = { 0, 255 }; 
	float* ranges[]    = {r_ranges, r_ranges, r_ranges};
	int scale = 10;

	int iwidth = src[1]->width - patchx + 1;
	int iheight = src[1]->height - patchy + 1;
	ftmp = cvCreateImage( cvSize(iwidth,iheight),32,1);
	cvZero(ftmp);
	
	dst = cvCreateImage( cvGetSize(src[1]),8,1);
	
	cvZero(dst);
	for(i = 0; i<2; ++i){ 
		//hsv[i] = cvCreateImage( cvGetSize(src[i]), 8, 3 );
		//cvCvtColor( src[i], hsv[i], CV_BGR2HSV );
		hsv[i] = src[i];
		
		r_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		g_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		b_plane[i]  = cvCreateImage( cvGetSize(src[i]), 8, 1 );
		planes[i][0] = r_plane[i];
		planes[i][1] = g_plane[i];
		planes[i][2] = b_plane[i];
		
		cvCvtPixToPlane( hsv[i], r_plane[i], g_plane[i], b_plane[i], 0 );
		// Build the histogram and compute its contents.
		//
		hist[i] = cvCreateHist( 
							   3, 
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
									cvSize( bins * scale, bins * scale ), 
									8, 
									3
									); 
		cvZero( hist_img[i] );
		
	}//For the 2 images
	
	//DO THE BACK PROJECTION

	printf("Doing cvCalcBackProjectPatch() with type =%d\n",type);
	cvCalcBackProjectPatch(planes[1],ftmp,cvSize(35,35),hist[0],type,1.0);
	printf("ftmp count = %d\n",cvCountNonZero(ftmp));

		
	
	double min, max;
	CvPoint minl, maxl;
	
	cvMinMaxLoc(ftmp, &min, &max, &minl, &maxl);
	cvCircle(ftmp, maxl, 4, cvScalar(255,0,0));
	
	//DISPLAY
	largeBall = cvCreateImage(cvSize(src[0]->width*2, src[0]->height*2), 8, 3);
	cvResize(src[0], largeBall);
	cvShowImage(   "Ball", largeBall );
	//cvNamedWindow( "Model H-S Histogram", 0 );
	//cvShowImage(   "Model H-S Histogram", hist_img[0] );
	
	//cvNamedWindow( "Test Image", 0 );
	//cvShowImage(   "Test Image", src[1] );
	//cvNamedWindow( "Test H-S Histogram", 0 );
	//cvShowImage(   "Test H-S Histogram", hist_img[1] );
	
	
	cvShowImage(   "Back Projection", ftmp );
	return minl;
}

int main()
{
	IplImage* ball, *table, *resultImage;
	CvPoint ballLocation;
	
	cvNamedWindow( "Table", 0);
	cvNamedWindow( "Back Projection",0);
	cvNamedWindow( "Ball", 0);
	
	cvWaitKey(0);
	
	//const int matchSeq[16] = {8, 16, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15}
	string path;
	for (int tableNum=0; tableNum<2; tableNum++) {
		path = "tables/" + intToStr(tableNum) + ".jpg";
		table = cvLoadImage(path.c_str(),1);
		for (int ballNum=0; ballNum<NUM_BALLS; ballNum++) {
			path = "balls/" + intToStr(ballNum) + ".jpg";
			ball = cvLoadImage(path.c_str(),1);
			
			ballLocation = matchHist(ball, table, resultImage);
			ballLocation.x += (patchx/2)+1;
			ballLocation.y += patchy/2+1;
			
			cvCircle(table, ballLocation, 14, cvScalar(0,0,255), 2);
			
			cvShowImage(   "Table", table);
			cvWaitKey(0);
		}
		cvWaitKey(0);
	}
}