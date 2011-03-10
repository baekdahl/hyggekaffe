clc
clear
close all

path_training = 'c:/hyggekaffe/new train data/';

cd (path_training);
d = dir(path_training);
d = d(3:length(d));
bins = 5;

for j=1:length(d),
    
    if isdir(d(j).name),
    subdir = dir(d(j).name);
    subdirname = d(j).name;
    subdir = subdir(3:length(subdir))
    
    mahalanobis_train(j).data = zeros(1,bins);
                
        for k=1:length(subdir),
            file = subdir(k).name
            norm_hist = hwa_hsv([subdirname '/' file],bins);
            mahalanobis_train(j).data(k,:) = [norm_hist.h];            
        end
    end
    
    mahalanobis_train(j).mean = mean(mahalanobis_train(j).data)';
    mahalanobis_train(j).cov = cov(mahalanobis_train(j).data);
end
    
path_ballid = 'c:/hyggekaffe/new test data';
cd (path_ballid);
d = dir(path_ballid);
d = d(3:length(d));

for j=1:length(d),
    path_ball = [path_ballid '/' d(j).name];
    norm_hist = hwa_hsv(path_ball, bins);
    ball_histogram(j).data = [norm_hist.h]';
end

for j=1:length(d),
    for i=1:length(mahalanobis_train),
        DM_Cov(j,i) = (ball_histogram(j).data - mahalanobis_train(i).mean)'*inv(mahalanobis_train(i).cov)*(ball_histogram(j).data - mahalanobis_train(i).mean);
    end
end
        
clear hist_h hist_s k i n bins Image Image_HSV fname k subdir subdirname j file max_h max_s path_training h norm_hist d ; 

