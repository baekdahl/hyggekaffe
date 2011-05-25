clear;
close all;
balls_path = '../../pics/balls/';
radius = 13;
res = [960 720];

for i=1:10

    drawBall(i*[20 20], radius, i);
    axis([0 res(1) 0 res(2)]);
        axis ij;
    hold on;
end