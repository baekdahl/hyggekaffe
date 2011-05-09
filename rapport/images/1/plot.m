
maskarea = 369861.5;
maskper = 3153.8;

for i=1:16,
   
    hold on
    if(data(i,3)==0),
        marker='sg';
    end
    if(data(i,3)==1),
        marker='dr';
    end
    if(data(i,3)==2),
        marker='+';
    end
    
    scatter(data(i,1)/maskarea,data(i,2)/maskper,marker,'filled');
        
end

hold off