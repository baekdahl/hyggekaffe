path(path, '../ball detection');
clc
clear
close all

balls_path = '../../pics/balls/';

bins = 50;

balls = [0 1 9 14
         3 5 7 11
         8 2 6 16];
 
for page=1:1
    figure;

    for ball=1:1
        %subplot(2,2,ball);
        imagePath = [balls_path int2str(balls(page,ball)) '.png']
        %histograms(j) = hwa_hsv(imagePath,bins);
        %histograms(j).h = histograms(j).h / max(histograms(j).h);
        %histograms(j).s = histograms(j).s / max(histograms(j).s);
        [Image, map, alpha] = imread(imagePath);
        Image = rgb2hsv(Image);
        Image = im2uint8(Image);
        imhist3(Image, alpha, bins);
        title(balls(page,ball))
        hold on;      %# Add to the plot
        
        axes('Position',[0.7 0.5 0.2 0.2]);
        [Image, map, alpha] = imread(imagePath);
        imshow(Image);
        %xImage = [0 50; 0 50];   %# The x data for the image corners
        %yImage = [255 255; 255 255];             %# The y data for the image corners
        %zImage = [0 0; 50 50];   %# The z data for the image corners
        %surf(xImage,yImage,zImage,...    %# Plot the surface
        %'CData',img,...
        %'FaceColor','texturemap');
    end
end