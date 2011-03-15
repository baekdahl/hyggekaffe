clc
%clear

%input_string = 'There have been some hair-raising goings-on outside the castle at Elsinore. As the terrified Horatio and Marcellus look on, the ghost of the recently deceased king appears to Prince Hamlet. The spirit beckons Hamlet offstage, and the frenzied prince follows after, ordering the witnesses to stay put.'
input_string = 'sir sid eastman easily teases sea sick seals';

profile on;

%input_string = import{(1)};

[encoded_string dictionary] = encode(input_string);
[p symbols] = frequency(encoded_string);
huff_key = huffmandict(symbols,p);

huff_encoded = huffmanenco(str2num(encoded_string),huff_key)';

Compression_ration = (length(input_string)*8)/length(huff_encoded)

huff_decoded = huffmandeco(huff_encoded, huff_key);

huff_string = '';
for i=1:length(huff_decoded),
   huff_string = [huff_string ' ' num2str(huff_decoded(i))];
end
   huff_string = huff_string(2:length(huff_string));


decoded_string = decode(huff_string);
