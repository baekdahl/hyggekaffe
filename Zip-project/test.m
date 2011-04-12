clc
input_string = input{(1)};

profile on

[encoded_string dictionary] = encode(input_string);

%[p symbols] = frequency(encoded_string);
%huff_key = huffmandict(symbols,p);

%huff_encoded = huffmanenco(str2num(encoded_string),huff_key)';

%Compression_ration = (length(input_string)*8)/length(huff_encoded)

%huff_decoded = huffmandeco(huff_encoded, huff_key);

%huff_string = '';
%for i=1:length(huff_decoded),
%   huff_string = [huff_string ' ' num2str(huff_decoded(i))];
%end
%   huff_string = huff_string(2:length(huff_string));


decoded_string = decode(encoded_string);

profview