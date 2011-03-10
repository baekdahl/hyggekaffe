for i=1:112,

    DM_H(:,i) = DM_H(:,i) - min(DM_H(:,i))
    %DM_Cov(:,i) = DM_Cov(:,i) - min(DM_Cov(:,i))

end
