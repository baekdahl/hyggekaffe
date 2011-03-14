clc
path_ballid = 'c:/hyggekaffe/new test data/';

cd (path_ballid);
d = dir(path_ballid);
d = d(3:length(d));
bins = 100;

for j=1:length(d),
    path_ball = [path_ballid '/' d(j).name];
    ball_histogram(j) = hwa_hsv(path_ball, bins);
    ball_histogram(j).h = ball_histogram(j).h / max(ball_histogram(j).h);
    ball_histogram(j).s = ball_histogram(j).s / max(ball_histogram(j).s);
    ball_histogram(j).v = ball_histogram(j).v / max(ball_histogram(j).v);
end

for j=1:length(d),
    for i=1:length(histograms_training),
        DM_H(j,i) = L2distance(ball_histogram(j).h,histograms_training(i).h);
        DM_S(j,i) = L2distance(ball_histogram(j).s,histograms_training(i).s);
        DM_V(j,i) = L2distance(ball_histogram(j).v,histograms_training(i).v);
        
        %fa = (0:(1/(bins-1)):1)';
        %w1 = ball_histogram(j).s / sum(ball_histogram(j).s);
        %w2 = histograms_training(i).s;
        %DM_H(j,i) = emd(fa,fa,w1,w2,@gdf);        
    end
end

ball_id = zeros(length(d),1);
ball_list = zeros(length(d),1);

%DM_H = DM_H + DM_S + 0.3*DM_V;
DM_H_save = DM_H;

for j=1:length(d)
     [mini pos] = min(DM_H);
     [mini2 pos2] = min(mini);
     
     ball_id(pos2) = pos(pos2);
    
     DM_H(:,pos(pos2)) = 100;
     DM_H(pos(pos2),:) = 100;
end

clear bins j path_ball path_ballid bins j ball_list disth dists i d min_h min_s min_s_vec;