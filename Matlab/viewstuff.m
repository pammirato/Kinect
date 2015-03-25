SCENE = './scene_09/';
count = 10;
d = dir([SCENE '*.png']);

if(count > length(d))
    count = length(d);
end


colors = cell(1,count/2);
depths = cell(1,count/2);


for i=1:count%length(d)
    if(mod(i,2) == 0)
        depths{(i/2) +1} = imread(strcat(SCENE,d(i).name));
    else
        colors{floor((i/2)) +1} = imread(strcat(SCENE,d(i).name));
    end
end


for i=1:count/2%length(colors)
imagesc(colors{i});
pause(1/30);
end


pause();

for i=1:count/2%length(depths)
%imagesc(depths{i}.depthmat');
imagesc(depths{i});
pause(1/30);
end