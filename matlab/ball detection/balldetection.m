clc
clear
d = dir('c:/hyggekaffe/pool/balls/*.jpg');

for k=1:length(d),
    fname=d(k).name;
    Image = imread(fname);
    Image_HSV = rgb2hsv(Image);
    fname
    h = double(Image_HSV(:,:,1))*255;
    s = double(Image_HSV(:,:,2))*255;
    
    bins = 51;
    
    hist_h = hist(h,bins);
    hist_s = hist(s,bins);
    
    for i=1:bins,
       histograms(k).h(i) = sum(hist_h(i,:));
       histograms(k).s(i) = sum(hist_s(i,:));
    end
end


for i=1:bins,
    distance(i) = histograms(1).h(i) - histograms(2).h(i);
end

SSE = distance.^2;
SSE = sum(sqrt(SSE));

clear hist_h hist_s k i n bins Image Image_HSV d fname k ; 

