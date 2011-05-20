function [ hist ] = imhist3( image, alpha, bins )

pixel_count = 1;
sizeimg = size(image);
width = sizeimg(2);
height = sizeimg(1);
for u=1:width,
    for v=1:height,
        if(alpha(v,u) > 0), %& image(v,u,3) > 50),
            pixel_list(pixel_count, 1) = image(v,u,1);
            pixel_list(pixel_count, 2) = image(v,u,2);
            pixel_count = pixel_count + 1;
        end
    end
end

range = linspace(0, 255, bins);
hist3(pixel_list, {range, range})

graph = get(gca, 'child');
xsize = size(get(graph,'XData'));

%Generating colors
for i = 1:xsize(1)
    sat(i,:,2) = linspace(0,1,xsize(1));
    sat(i,:,1) = i/xsize(1);
end
sat(:,:,3) = 1;
index(1:xsize(1),1:xsize(1), 1:3) = hsv2rgb(sat);


%Converting RGB into indexed color to make export to pdf compatible
[i, map] = rgb2ind(index, 255);
set(graph, 'CData', double(i));
colormap(map);

xlabel('Hue'); ylabel('Saturation');

end

