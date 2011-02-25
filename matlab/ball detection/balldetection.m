clc
clear

d = dir('c:/hyggekaffe/pool/balls/');
bins = 255;

for j=3:length(d),
    
    if isdir(d(j).name),
    
    subdir = dir(d(j).name);
    subdirname = d(j).name
    
    histograms(j).h = zeros(1,bins);
    histograms(j).s = zeros(1,bins);
                
        for k=3:length(subdir),

            file = subdir(k).name

            Image = imread([subdirname '/' file]);
            Image_HSV = rgb2hsv(Image);

            hist_h = hist(double(Image_HSV(:,:,1)),bins);
            hist_s = hist(double(Image_HSV(:,:,2)),bins);
       
            for i=1:bins,
               histograms(j).h(i) = histograms(j).h(i) + sum(hist_h(i,:));
               histograms(j).s(i) = histograms(j).s(i) + sum(hist_s(i,:));
            end
        
        end
       
        max_h = max(histograms(j).h);
        max_s = max(histograms(j).s);
        
        histograms(j).h  = histograms(j).h / max_h;
        histograms(j).s  = histograms(j).s / max_s;
        
    end
    
end

for k=3:18,
    subplot(4,4,k-2);
    
    hold on
    bar(histograms(k).h);

    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','g','EdgeColor','g');
    
    plot(histograms(k).s);
    
    %h = findobj(gca,'Type','patch');
    %set(h(2),'FaceColor','g','EdgeColor','g');
    
    hold off
    
    title(d(k).name,'Color','w');
end

% 
% for i=1:bins,
%     distance(i) = histograms(1).h(i) - histograms(3).h(i);
% end
% 
% SSE = distance.^2;
% SSE = sum(sqrt(SSE));

clear hist_h hist_s k i n bins Image Image_HSV fname k subdir subdirname j file d ; 

