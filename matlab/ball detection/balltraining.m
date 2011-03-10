clc
clear
close all

path_training = 'c:/hyggekaffe/new train data/';

cd (path_training);
d = dir(path_training);
d = d(3:length(d));
bins = 100;

for j=1:length(d),
    
    if isdir(d(j).name),
    
    subdir = dir(d(j).name);
    subdirname = d(j).name;
    
    subdir = subdir(3:length(subdir))
    
    histograms_training(j).h = zeros(1,bins);
    histograms_training(j).s = zeros(1,bins);
    histograms_training(j).v = zeros(1,bins);
                
        for k=1:length(subdir),
            file = subdir(k).name
            norm_hist = hwa_hsv([subdirname '/' file],bins);
            
            histograms_training(j).h = histograms_training(j).h + norm_hist.h;
            histograms_training(j).s = histograms_training(j).s + norm_hist.s;
            histograms_training(j).v = histograms_training(j).v + norm_hist.v;
            
        end
        
        histograms_training(j).h  = histograms_training(j).h / k;
        histograms_training(j).s  = histograms_training(j).s / k;
        histograms_training(j).v  = histograms_training(j).v / k;
        
        histograms_training(j).h = histograms_training(j).h / max(histograms_training(j).h);
        histograms_training(j).s = histograms_training(j).s / max(histograms_training(j).s);
        histograms_training(j).v = histograms_training(j).v / max(histograms_training(j).v);
        
        end
        
    end

clear hist_h hist_s k i n bins Image Image_HSV fname k subdir subdirname j file max_h max_s path_training h ; 

