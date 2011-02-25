function [f, fval] = test(varargin)

% Images
if nargin == 0
    B = imread('C:\hyggekaffe\pool\balls\8\8-3.jpg');
    B = rgb2hsv(B);
    B = B(:,:,1)
    
    A = imread('C:\hyggekaffe\pool\balls\8\8-4.jpg');
    A = rgb2hsv(A);
    A = A(:,:,1);
    
elseif nargin == 2
    A = varargin{1};
    B = varargin{2};
    if ischar(A)
        A = imread(A);
    end;
    if ischar(B)
        B = imread(B);
    end;
end;

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
[f, fval] = emd(f1, f2, w1, w2, @gdf);


% Results
wtext = sprintf('fval = %f', fval);
figure('Name', wtext);
subplot(121);imshow(A);title('first image');
subplot(122);imshow(B);title('second image');

end