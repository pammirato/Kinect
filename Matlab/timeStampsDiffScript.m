

colorToDepth = zeros(length(colorTimeStamps),1);
diffs = -ones(length(colorTimeStamps),1);

for i=1:length(colorTimeStamps)
    minIndex = 0;
    minDiff = 1000000;

    for j=1:length(depthTimeStamps)
        curDiff = abs(colorTimeStamps(i) - depthTimeStamps(j));
        if(curDiff < minDiff)
            minDiff = curDiff;
            minIndex = j;
        end
    end 
    
    diffs(i) = minDiff;
end


colorDiffs =  [];
for i=1:length(colorTimeStamps)-1
    colorDiffs(i) = colorTimeStamps(i+1) - colorTimeStamps(i);
end
