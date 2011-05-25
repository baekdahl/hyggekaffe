function [] = plotHists(analtable1, analtable2, analtable3, analtable4)

images = {analtable1, analtable2, analtable3, analtable4};

for i = 1:4
    table = images{i};
    hsv = rgb2hsv(table);
    hue = hsv(:,:,1)*360;
    hue = reshape(hue,1,[]);
    sat = hsv(:,:,2)*255;
    sat = reshape(sat,1,[]);
    val = hsv(:,:,3)*255;
    val = reshape(val,1,[]);
    
    subplot(4,4,i)
    imshow(table)
    set(gca,'ytick',[])
    title(num2str(i));
    axis off

    subplot(4,4,i+4)
    hist(hue,100);
    title('Hue');
    axis([0 360,0 5000])
    %axis off
    box off
    set(gca,'ytick',[])
    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','b','EdgeColor','b');

    subplot(4,4,i+8)
    hist(sat,100);
    title('Saturation')
    axis([0 255,0 10000])
    box off
    set(gca,'ytick',[])
    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','b','EdgeColor','b');

    subplot(4,4,i+12)
    hist(val,100);
    title('Brightness')
    axis([0 255,0 5000])
    box off
    set(gca,'ytick',[])
    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','b','EdgeColor','b');
    
end

end

