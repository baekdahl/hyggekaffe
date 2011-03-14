function [ dictionary ] = create_dictionary()
%CREATE_DICTIONARY Summary of this function goes here
%   Detailed explanation goes here

dictionary = '';

for i=32:127,
    dictionary = [dictionary,{char(i)}];
end

