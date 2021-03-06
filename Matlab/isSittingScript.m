





%drawSkeleton(jointIndexMap,bodyMatrix,20);
%sit = isSitting(jointIndexMap,bodyMatrix,20);






%is Sitting ??????????????????????????????????????????????????

bodyFrameWidth = 424;
bodyFrameHeight = 512;

colorFrameWidth = 1920;
colorFrameHeight = 1080;



sitting = zeros(1,length(bodyMatrix)/2);
for j=1:length(bodyMatrix)/2
    yIndex = j*2;%because bodyMatrix has x and y in separate columns
    
    
    
    %get right knee joint
    rightHipToKnee = [bodyMatrix(jointIndexMap('HipRight'),yIndex-1)-bodyMatrix(jointIndexMap('KneeRight'),yIndex-1), ...
            bodyMatrix(jointIndexMap('HipRight'),yIndex)-bodyMatrix(jointIndexMap('KneeRight'),yIndex)];
     
    rightKneeToAnkle = [bodyMatrix(jointIndexMap('AnkleRight'),yIndex-1)-bodyMatrix(jointIndexMap('KneeRight'),yIndex-1), ...
            bodyMatrix(jointIndexMap('AnkleRight'),yIndex)-bodyMatrix(jointIndexMap('KneeRight'),yIndex)];
    
        

    %ge lefft knee joint
    leftHipToKnee = [bodyMatrix(jointIndexMap('HipLeft'),yIndex-1)-bodyMatrix(jointIndexMap('KneeLeft'),yIndex-1), ...
            bodyMatrix(jointIndexMap('HipLeft'),yIndex)-bodyMatrix(jointIndexMap('KneeLeft'),yIndex)];
     
    leftKneeToAnkle = [bodyMatrix(jointIndexMap('AnkleLeft'),yIndex-1)-bodyMatrix(jointIndexMap('KneeLeft'),yIndex-1), ...
            bodyMatrix(jointIndexMap('AnkleLeft'),yIndex)-bodyMatrix(jointIndexMap('KneeLeft'),yIndex)];
    
        
   %get the angles at the knee joint
    rightAngle =(180/3.14159)* acos( rightHipToKnee*rightKneeToAnkle' / (norm(rightHipToKnee) * norm(rightKneeToAnkle)));

    
    leftAngle =(180/3.14159)* acos( leftHipToKnee*leftKneeToAnkle' / (norm(leftHipToKnee) * norm(leftKneeToAnkle)));
    
   
    if(rightAngle< 135  && leftAngle < 135)
        sitting(j)  =1;
    end
end





%??????????????????????????????????????????????????????????????????


temp = zeros(1,sum(sitting));%will hold all frame #'s with sitting detected
count = 1;
for j=1:length(sitting)
    if(sitting(j)~=0)
        temp(count) = j;
        count = count+1;
    end
end
sitting = temp;


yellow = uint8([255 255 0]); % [R G B];
shapeInserter = vision.ShapeInserter('Shape','Circles','BorderColor','Custom','CustomBorderColor',yellow);
%circles = int32([30 30 20; 80 80 25]); %  [x1 y1 radius1;x2 y2 radius2]
 
circleRadius = 10;

%2 circles per sitting pose, one for each knee
circles = zeros(length(sitting)*2,3);


heightRatio = (colorFrameHeight/bodyFrameHeight);
widthRatio = (colorFrameWidth/bodyFrameWidth);
%show about where we thought sitting happenned
for j=1:length(sitting)
             %frame number
    yIndex = sitting(j)*2;
    
    
    y1 =( bodyFrameHeight - bodyMatrix(jointIndexMap('KneeRight'),yIndex) )*heightRatio;
    x1 =( bodyFrameWidth -bodyMatrix(jointIndexMap('KneeRight'),yIndex-1) )* widthRatio;
    y2 =( bodyFrameHeight - bodyMatrix(jointIndexMap('KneeLeft'),yIndex) )* heightRatio;
    x2 =( bodyFrameWidth -bodyMatrix(jointIndexMap('KneeLeft'),yIndex) )* widthRatio;
    circles(2*j-1,:) = [x1 y1 circleRadius];
    circles(2*j,:) = [x2 y2 circleRadius];
end

circles = int32(circles);

J = step(shapeInserter, colors{1}, circles);
imshow(J);






    


