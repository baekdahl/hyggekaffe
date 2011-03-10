function [ ball_histogram ] = hsvhist( ball_img, bins )
%BALLID Summary of this function goes here
%   Detailed explanation goes here

Image = imread(ball_img);
Image_HSV = rgb2hsv(Image);

ball_histogram.h = zeros(1,bins);
ball_histogram.s = zeros(1,bins);

hist_h = hist(Image_HSV(:,:,1),bins)';
hist_s = hist(Image_HSV(:,:,2),bins)';

hist_h = sum(hist_h);
hist_s = sum(hist_s);
       
max_h = max(hist_h);
max_s = max(hist_s);
        
ball_histogram.h  = hist_h / max_h;
ball_histogram.s  = hist_s / max_s;      

end

