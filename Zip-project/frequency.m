function [ p, symbols ] = frequency( input_string )
%FREQUENCY Summary of this function goes here
%   Detailed explanation goes here

input_string = str2num(input_string);
input_string_hist = hist(input_string,max(input_string));
bar(input_string_hist)
count = 1;
p = 0;
symbols = 0;


for i=1:max(input_string),
    
    if(input_string_hist(i)~=0),
        p(count,1) = input_string_hist(i);
        symbols(count,1) = i;
        count = count + 1;
    end
end

p = p / sum(p);

