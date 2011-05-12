function [ hist ] = imhist3( image, alpha, bins )

pixel_count = 1;
sizeimg = size(image);
width = sizeimg(2);
height = sizeimg(1);
for u=1:width,
    for v=1:height,
        if(alpha(v,u) > 0 & image(v,u,3) > 50),
            pixel_list(pixel_count, 1) = image(v,u,1);
            pixel_list(pixel_count, 2) = image(v,u,2);
            pixel_count = pixel_count + 1;
        end
    end
end

%array =reshape(image(:,:,1:2), size(image,1)*size(image,2),2);
range = linspace(0, 255, bins);
hist3(pixel_list, {range, range})
%h = bar3(0:5:255,hist);
graph = get(gca, 'child');

%for i = 1:length(h)
    xsize = size(get(graph,'XData'));
    
    for i = 1:xsize(1)
        sat(i,:,2) = linspace(0,1,xsize(1));
        sat(i,:,1) = i/xsize(1);
    end
    sat(:,:,3) = 1;
    
    %sat(1:xsize(1),:,2) = linspace(0,1,xsize(1));
    %sat(1:xsize(1),:,1) = .5;
    %sat(1:xsize(1),:,3) = 1;
    
    index(1:xsize(1),1:xsize(1), 1:3) = hsv2rgb(sat);
    %index(1:xsize(1),2,1:3) = hsv2rgb(sat);
    %index(1:xsize(1),3,1:3) = hsv2rgb(sat);
    %index(1:xsize(1),4,1:3) = hsv2rgb(sat);
    
    set(graph,'CData', index);
%end

xlabel('Hue'); ylabel('Saturation');
colormap(hsv);

end

