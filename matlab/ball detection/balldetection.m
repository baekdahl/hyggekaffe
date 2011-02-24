d=dir('c:/hyggekaffe/pool/balls/*.jpg');

for k=1:length(d),
    fname=d(k).name;
    Image = imread(fname);
    Image_HSV = rgb2hsv(Image);
    subplot(3,3,k);
    h = Image_HSV(:,:,1)*255;
    hist(h,255);
    title(fname);  
end

