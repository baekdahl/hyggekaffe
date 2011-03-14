function [ place_string ] = encode_string( input_string, dictionary )
%FIND_STRING Summary of this function goes here
%   Detailed explanation goes here

place_string = 0;

for i=1:length(dictionary),
   
    if(strcmp(input_string,dictionary(i))),
        place_string = i;
        break
    end

end

