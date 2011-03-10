function [histogram] = hwa_hsv( Image_path, bins)
%HWA Summary of this function goes here
%   Detailed explanation goes here

[Image, map, alpha] = imread(Image_path);

sizeimg = size(Image);
width = sizeimg(2);
height = sizeimg(1);

Image = rgb2hsv(Image);

pixel_count = 1;

for u=1:width,
    for v=1:height,
        if(alpha(v,u) > 0),
            pixel_list_h(pixel_count) = Image(v,u,1);
            pixel_list_s(pixel_count) = Image(v,u,2);
            pixel_list_v(pixel_count) = Image(v,u,3);
            pixel_count = pixel_count + 1;
        end
    end
end

histogram.h = hist(double(pixel_list_h),bins);%  /  max(hist(double(pixel_list_h),bins)) ;
histogram.s = hist(double(pixel_list_s),bins);% /  max(hist(double(pixel_list_s),bins)) ;
histogram.v = hist(double(pixel_list_v),bins);%  /  max(hist(double(pixel_list_v),bins)) ;

end

