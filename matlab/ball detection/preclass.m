clc
clear
path_ballid = 'c:/hyggekaffe/new test data/';

cd (path_ballid);
d = dir(path_ballid);
d = d(3:length(d));
bins = 10;

for j=1:length(d),
    path_ball = [path_ballid '/' d(j).name];
    [Image,map,alpha] = imread(path_ball);
    
    sizeimg = size(Image);
    width   = sizeimg(2);
    height  = sizeimg(1);
    
    white = 0;
    
    for u=1:width,
        for v=1:height,
            if( alpha(v,u) > 0 ),
                if(Image(v,u,1)+Image(v,u,2)+Image(v,u,3) > 3*81),
                    white = white + 1;
                end
            end
        end
    end
    str = sprintf('Kugle %d er %d hvid',j,white);
    disp(str);
    
    
end