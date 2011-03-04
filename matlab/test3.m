clc
clear
close all

path_training = 'c:/hyggekaffe/new train data/';

cd (path_training);
d = dir(path_training);
d = d(3:length(d));
bins = 51;

hist_count = 1;

for j=1:length(d),
    
    if isdir(d(j).name),
    subdir = dir(d(j).name);
    subdirname = d(j).name;
    subdir = subdir(3:length(subdir))
    
    single_hist(hist_count).h = zeros(1,bins);
                
        for k=1:length(subdir),
            file = subdir(k).name
            norm_hist = hwa_hsv([subdirname '/' file],bins);
            single_hist(hist_count).h = [norm_hist.h];
            single_hist(hist_count).name = subdir(k).name;
            hist_count = hist_count + 1;
        end
    end

end
    
path_ballid = 'c:/hyggekaffe/new test data';
cd (path_ballid);
d = dir(path_ballid);
d = d(3:length(d));

for j=1:length(d),
    path_ball = [path_ballid '/' d(j).name];
    norm_hist = hwa_hsv(path_ball, bins);
    ball_histogram(j).h = [norm_hist.h]';
end

for j=1:length(ball_histogram);

    for i=1:length(single_hist),
            DM_H(j,i) = L2distance(ball_histogram(j).h,single_hist(i).h);
    end
    
end
    
    

clear hist_h hist_s k i hist_count path_ball path_ballid n bins Image Image_HSV fname k subdir subdirname j file max_h max_s path_training h norm_hist d ; 
