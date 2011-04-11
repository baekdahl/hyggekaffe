hsv_3 = rgb2hsv(analtable3)*360;
hsv_1 = rgb2hsv(analtable1)*360;
hsv_2 = rgb2hsv(analtable2)*360;

subplot(4,3,1)
image(analtable3)
subplot(3,4,4)
hist(hsv_3(:,:,1),360);

subplot(4,3,2)
image(analtable1)
subplot(3,4,5)
hist(hsv_1(:,:,1),360);

subplot(4,3,3)
image(analtable2)
subplot(3,4,6)
hist(hsv_2(:,:,1),360);




