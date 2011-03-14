function [ string_out, dictionary ] = encode( string_in )
%ENCODE Summary of this function goes here
%   Detailed explanation goes here

dictionary = create_dictionary();
word = '';
string_out = '';

for i=1:length(string_in),
        
    x = string_in(i);
    wordx = [word x];
    
    if(encode_string(wordx,dictionary)~=0),
        word = wordx;
    else
        dic_place = (encode_string(word,dictionary));
        string_out = [string_out ' ' num2str(dic_place)];
        dictionary(end+1) = {wordx};
        word = x;
    end
    
end
dic_place = (encode_string(word,dictionary));
string_out = [string_out ' ' num2str(dic_place)];

string_out = string_out(2:length(string_out));

end

