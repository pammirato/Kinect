
for i=1:length(colors)
imagesc(colors{i});
pause(1/30);
end


pause();

for i=1:length(depths)
imagesc(depths{i}.depthmat');
pause(1/30);
end