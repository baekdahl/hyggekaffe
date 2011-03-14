function [ L2dist ] = L2distance( histogram_training, histogram_ball )
%L2DISTANCE Summary of this function goes here
%   Detailed explanation goes here

L2dist = 0;

for i=1:length(histogram_training),

    L2dist = L2dist + (histogram_training(i) - histogram_ball(i))^2;
    
end

L2dist = sqrt(L2dist);

end

