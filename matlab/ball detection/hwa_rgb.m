function [histogram] = hwa_rgb( Image_path, bins)
%HWA Summary of this function goes here
%   Detailed explanation goes here

[Image, map, alpha] = imread(Image_path);

sizeimg = size(Image);
width = sizeimg(2);
height = sizeimg(1);

pixel_count = 1;

for u=1:width,
    for v=1:height,
        if(alpha(v,u) > 0),
            pixel_list_r(pixel_count) = Image(v,u,1);
            pixel_list_g(pixel_count) = Image(v,u,2);
            pixel_list_b(pixel_count) = Image(v,u,3);
            pixel_count = pixel_count + 1;
        end
    end
end

histogram.r = hist(double(pixel_list_r),bins)  /  max(hist(double(pixel_list_r),bins)) ;
histogram.g = hist(double(pixel_list_g),bins) /  max(hist(double(pixel_list_g),bins)) ;
histogram.b = hist(double(pixel_list_b),bins)  /  max(hist(double(pixel_list_b),bins)) ;

end

