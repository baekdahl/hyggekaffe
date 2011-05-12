path(path, '../ball detection');
clc
clear
close all

balls_path = '../../pics/balls/';

bins = 255;

balls = [0 1 9 14
         3 5 7 11
         8 2 6 16];
 
for page=1:size(balls,1)
    %figure;

    for ball=1:size(balls,2) 
        figure;
        %subplot(2,2,ball);
        imagePath = [balls_path int2str(balls(page,ball)) '.png']
        %histograms(j) = hwa_hsv(imagePath,bins);
        %histograms(j).h = histograms(j).h / max(histograms(j).h);
        %histograms(j).s = histograms(j).s / max(histograms(j).s);
        [Image, map, alpha] = imread(imagePath);
        Image = rgb2hsv(Image);
        Image = im2uint8(Image);
        imhist3(Image, alpha);
        title(balls(page,ball))
        print('-dpdf', '-painters', int2str(balls(page,ball)));
    end
end