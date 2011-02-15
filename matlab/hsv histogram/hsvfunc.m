hcloth = hsv_cloth(:,1);
scloth = hsv_cloth(:,2);
vcloth = hsv_cloth(:,3);

subplot(3,2,1);
hist(hcloth,255);
title('Cloth Hue');

subplot(3,2,3);
hist(scloth,255);
title('Cloth Saturation');

subplot(3,2,5);
hist(vcloth,255);
title('Cloth Value');

h = hsv(:,1);
s = hsv(:,2);
v = hsv(:,3);

subplot(3,2,2);
hist(h,255);
title('Full Hue');

subplot(3,2,4);
hist(s,255);
title('Full Saturation');

subplot(3,2,6);
hist(v,255);
title('Full Value');