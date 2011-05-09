
maskarea = 369861.5;
maskper = 3153.8;

for i=1:15,
   
    hold on
    if(data(i,3)==0),
        marker='sg';
    end
    if(data(i,3)==1),
        marker='dr';
    end
    
    scatter(data(i,1)/maskarea,data(i,2)/maskper,marker,'filled');
        
end

xlabel('\lambda : Area factor')
ylabel('\kappa : Perimeter factor')
title('Occlusion plot')


hold off