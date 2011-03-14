function [ output_string ] = decode( input_string )
%UNTITLED2 Summary of this function goes here
%   Detailed explanation goes here

dictionary = create_dictionary();
output_string = '';
element = '';

[token, remain] = strtok(input_string);
input_string = remain;

element = decode_string(token,dictionary);

output_string = [output_string element];
word = element;

while(1),

[token, remain] = strtok(input_string);
element = decode_string(token,dictionary);

if( length(dictionary) < str2num(token) ),
   element = [word word(1)];
end

output_string = [output_string element];
dictionary(end+1) = {[word element(1)]};

word = element;

if(isempty(remain)),
    break;
end
input_string = remain;
end

