function [ output_string ] = decode_string( input_string, dictionary )
%UNTITLED4 Summary of this function goes here
%   Detailed explanation goes here

output_string = dictionary(str2num(input_string));
output_string = char(output_string);
end

