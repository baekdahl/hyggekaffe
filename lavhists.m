hsv_1 = rgb2hsv(analtable1)*250;
hsv_2 = rgb2hsv(analtable2)*250;
hsv_3 = rgb2hsv(analtable3)*250;
hsv_4 = rgb2hsv(analtable4)*250;

hue_1 = hsv_1(:,:,1);
hue_2 = hsv_2(:,:,1);
hue_3 = hsv_3(:,:,1);
hue_4 = hsv_4(:,:,1);

hue_1 = reshape(hue_1,1,[]);
hue_2 = reshape(hue_2,1,[]);
hue_3 = reshape(hue_3,1,[]);
hue_4 = reshape(hue_4,1,[]);

sat_1 = hsv_1(:,:,2);
sat_2 = hsv_2(:,:,2);
sat_3 = hsv_3(:,:,2);
sat_4 = hsv_4(:,:,2);

sat_1 = reshape(sat_1,1,[]);
sat_2 = reshape(sat_2,1,[]);
sat_3 = reshape(sat_3,1,[]);
sat_4 = reshape(sat_4,1,[]);

val_1 = hsv_1(:,:,3);
val_2 = hsv_2(:,:,3);
val_3 = hsv_3(:,:,3);
val_4 = hsv_4(:,:,3);

val_1 = reshape(val_1,1,[]);
val_2 = reshape(val_2,1,[]);
val_3 = reshape(val_3,1,[]);
val_4 = reshape(val_4,1,[]);

subplot(4,4,2)
image(analtable1)
title('2) Image');
subplot(4,4,6)
hist(hue_1,100);
title('2) Hue');
subplot(4,4,10)
hist(sat_1,100);
title('2) Saturation')
subplot(4,4,14)
hist(val_1,100);
title('2) Value')

subplot(4,4,3)
image(analtable2)
title('3) Image');
subplot(4,4,7)
hist(hue_2,100);
title('3) Hue');
subplot(4,4,11)
hist(sat_2,100);
title('3) Saturation')
subplot(4,4,15)
hist(val_2,100);
title('3) Value')

subplot(4,4,1)
image(analtable3)
title('1) Image');
subplot(4,4,5)
hist(hue_3,100);
title('1) Hue');
subplot(4,4,9)
hist(sat_3,100);
title('1) Saturation')
subplot(4,4,13)
hist(val_3,100);
title('1) Value')

subplot(4,4,4)
image(analtable4)
title('4) Image');
subplot(4,4,8)
hist(hue_4(:,:,1),100);
title('4) Hue');
subplot(4,4,12)
hist(sat_4,100);
title('4) Saturation')
subplot(4,4,16)
hist(val_4,100);
title('4) Value')



