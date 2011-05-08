function [histogram] = hwa_hsv( Image_path, bins)
%HWA Summary of this function goes here
%   Detailed explanation goes here

[Image, map, alpha] = imread(Image_path);



sizeimg = size(Image);
width = sizeimg(2);
height = sizeimg(1);

Image = rgb2hsv(Image);
Image = im2uint8(Image);

pixel_count = 1;

for u=1:width,
    for v=1:height,
        if(alpha(v,u) > 0),
            %if(abs(Image(v,u,1)-131)>3),
            pixel_list_h(pixel_count) = Image(v,u,1);
            pixel_list_s(pixel_count) = Image(v,u,2);
            pixel_list_v(pixel_count) = Image(v,u,3);
            pixel_count = pixel_count + 1;
            %end
        end
    end
end

edges = 0:255;

histogram.h = histc(pixel_list_h,edges);
histogram.s = histc(pixel_list_s,edges);
histogram.v = histc(pixel_list_v,edges);

end

