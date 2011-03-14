for j=1:length(d),
    min_h_vec = DM_H(j,:);
    min_s_vec = DM_S(j,:);
    
    min_hs_vec = min_h_vec - min_s_vec;
    min_hs_vec = min_hs_vec;
    
    for i=1:length(d),
        if(ball_list(i,1)==1),
            min_h_vec(i) = 10000;
        end
        if(ball_list(i,2)==1)
            min_s_vec(i) = 10000;
        end
        if(ball_list(i,3)==1)
            min_hs_vec(i) = 10000;
        end
    end
    
    [disth min_h]   = min(min_h_vec);    
    [dists min_s]   = min(min_s_vec); 
    [disths min_hs] = min(min_hs_vec);
        
    ball_id(j,1) = min_h;
    ball_id(j,2) = min_s;
    ball_id(j,3) = min_hs;
    
    ball_list(min_h,1) = 1;
    ball_list(min_s,2) = 1;
    ball_list(min_hs,3) = 1;
end