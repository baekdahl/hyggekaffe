function [f, fval] = test(varargin)

% Images
    B = imread('C:\hyggekaffe\pool\balls\01\1.jpg');
    B = rgb2hsv(B);
    B = B(:,:,1)
    
    A = imread('C:\hyggekaffe\pool\balls\01\1.jpg');
    A = rgb2hsv(A);
    A = A(:,:,1);
    
% Histograms
nbins = 20;
[ca ha] = imhist(A, nbins);
[cb hb] = imhist(B, nbins);

% Features
f1 = ha;
f2 = hb;

% Weights
w1 = ca / sum(ca);
w2 = cb / sum(cb);

% Earth Mover's Distance
[fval] = emd(f1, f2, w1, w2, @gdf);


% Results
wtext = sprintf('fval = %f', fval);
figure('Name', wtext);
subplot(121);imshow(A);title('first image');
subplot(122);imshow(B);title('second image');

end