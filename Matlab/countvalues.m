

%have depthmap
counter = 0;
for i=1:size(depthmap,1)
    for j=1:size(depthmap,2)
        if(depthmap(i,j) ~= 0)
            counter = counter +1;
        end
    end
end

counter