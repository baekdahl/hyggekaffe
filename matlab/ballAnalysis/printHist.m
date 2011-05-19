path(path, '../export_fig');
clc
clear
close all

balls_path = '../../pics/balls/';

bins = 25;

for ball=0:16 
    figure;

    imagePath = [balls_path int2str(ball) '.png']

    [Image, map, alpha] = imread(imagePath);
    Image = rgb2hsv(Image);
    Image = im2uint8(Image);
    imhist3(Image, alpha, bins);
    %title(ball)
    set(gca, 'FontSize', 14);
    ylabel('Saturation','fontsize',14);
    xlabel('Hue','fontsize',14);
    set(gcf, 'Color', 'white')
    set(gca, 'ZTick', [])
    %print('-dpdf', '-painters', int2str(ball));
    
     hold on;      %# Add to the plot
        
    axes('Position',[0.7 0.7 0.15 0.15]);
    [Image, map, alpha] = imread(imagePath);
    imshow(Image);
    
    export_fig([int2str(ball) '.pdf'], '-painters');
end
