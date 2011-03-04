k2 = 1;
for k=1:length(histograms_training),
    subplot(4,8,k2);
    hold on
    bar(histograms_training(k).h);
    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','g','EdgeColor','g');
    plot(histograms_training(k).s);
    hold off
    title(k,'Color','w');
    
    k2 = k2 + 1;
    
    subplot(4,8,k2);
    hold on
    bar(ball_histogram(k).h);
    h = findobj(gca,'Type','patch');
    set(h,'FaceColor','g','EdgeColor','g');
    plot(ball_histogram(k).s);
    hold off
    
    
    k2 = k2 + 1;
end
