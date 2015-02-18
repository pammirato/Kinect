



jointIndexMap = containers.Map();

jointIndexMap('SpineBase') = 1;
jointIndexMap('SpineMid') = 2;
jointIndexMap('Neck') = 3;
jointIndexMap('Head') = 4;
jointIndexMap('ShoulderLeft') = 5;
jointIndexMap('ElbowLeft') = 6;
jointIndexMap('WristLeft') = 7;
jointIndexMap('HandLeft') = 8;
jointIndexMap('ShoulderRight') = 9;
jointIndexMap('ElbowRight') = 10;
jointIndexMap('WristRight') = 11;
jointIndexMap('HandRight') = 12;
jointIndexMap('HipLeft') = 13;
jointIndexMap('KneeLeft') = 14;
jointIndexMap('AnkleLeft') = 15;
jointIndexMap('FootLeft') = 16;
jointIndexMap('HipRight') = 17;
jointIndexMap('KneeRight') = 18;
jointIndexMap('AnkleRight') = 19;
jointIndexMap('FootRight') = 20;
jointIndexMap('SpineShoulder') = 21;
jointIndexMap('HandTipLeft') = 22;
jointIndexMap('ThumbLeft') = 23;
jointIndexMap('HandTipRight') = 24;
jointIndexMap('ThumbRight') = 25;

%drawSkeleton(jointIndexMap,bodyMatrix,20);
%sit = isSitting(jointIndexMap,bodyMatrix,20);






%is Sitting ??????????????????????????????????????????????????



sitting = zeros(1,length(bodyMatrix)/2);
for j=1:length(bodyMatrix)/2
    yIndex = j*2;%because bodyMatrix has x and y in separate columns
    
    hipToKnee = [bodyMatrix(jointIndexMap('HipRight'),yIndex-1)-bodyMatrix(jointIndexMap('KneeRight'),yIndex-1), ...
            bodyMatrix(jointIndexMap('HipRight'),yIndex)-bodyMatrix(jointIndexMap('KneeRight'),yIndex)];
     
    kneeToAnkle = [bodyMatrix(jointIndexMap('AnkleRight'),yIndex-1)-bodyMatrix(jointIndexMap('KneeRight'),yIndex-1), ...
            bodyMatrix(jointIndexMap('AnkleRight'),yIndex)-bodyMatrix(jointIndexMap('KneeRight'),yIndex)];
    
        
    angle =(180/3.14159)* acos( hipToKnee*kneeToAnkle' / (norm(hipToKnee) * norm(kneeToAnkle)));
    
    title(num2str(angle));
    imshow(colors{j});
    pause;
    
    
    
    sit = 0;
    if(angle< 135)
        sit =1;
    end
    sitting(j) = sit;
end





%??????????????????????????????????????????????????????????????????


temp = zeros(1,sum(sitting));%will hold all frame #'s with sitting dtected
count = 1;
for j=1:length(sitting)
    if(sitting(j)~=0)
        temp(count) = j;
        count = count+1;
    end
end
sitting = temp;

%show about where we thought sitting happenned
for j=1:length(sitting) 
  %imshow(colors{sitting(j)});
  %pause;
end








    


