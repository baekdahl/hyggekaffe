clc
path_ballid = 'c:/hyggekaffe/new train data/11/';

cd (path_ballid);
d = dir(path_ballid);
d = d(3:length(d));
bins = 50;

for j=1:length(d),
    path_ball = [path_ballid '/' d(j).name];
    ball_histogram(j) = hwa_hsv(path_ball, bins);
  
    %ball_histogram(j).h = ball_histogram(j).h / max(ball_histogram(j).h);
    %ball_histogram(j).s = ball_histogram(j).s / max(ball_histogram(j).s);
    %ball_histogram(j).v = ball_histogram(j).v / max(ball_histogram(j).v);
    
    subplot(4,2,j)
    bar(ball_histogram(j).h);
    title(d(j).name);
    
    
end


