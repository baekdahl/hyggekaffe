path(path, '../export_fig');
close all;
clear;

analtable2= imread('analtable1.jpg');
analtable3= imread('analtable2.jpg');
analtable1 = imread('analtable3.jpg');
analtable4 = imread('analtable4.jpg');


%RGB
figure(1)

hsbHists(analtable1, analtable2, analtable3, analtable4);
set(gcf, 'Color', 'white');
export_fig('hsv_hist_table.pdf', '-nocrop');


%HSB
figure(2)
rgbHists(analtable1, analtable2, analtable3, analtable4);
set(gcf, 'Color', 'white');
export_fig('rgb_hist_table.pdf', '-nocrop');