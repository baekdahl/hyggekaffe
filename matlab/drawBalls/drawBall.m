function [ ] = drawBall(pos, radius, number)
    boundingRect = [pos-radius radius*2 radius*2]
    rectangle('position', boundingRect,'curvature',[1,1]);
    text(pos(1), pos(2), num2str(number), 'FontSize', 14, 'HorizontalAlignment', 'center');
end

