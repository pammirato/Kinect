


function [depths,depthTimeStamps,colors,colorTimeStamps,bodyMatrix,bodyTimeStamps] = ReadStuff()
    %possible outouts
    depths = {};
    depthTimeStamps = {};
    bodyMatrix = [];
    boyTimeStamps = {};
    colors = {};
    colorTimeStamps = {};
    sitting = [];


    colorPath = 'C:\Users\Phil\Documents\KinectToMatLab\';
    depthPath = 'C:\Users\Phil\Documents\KinectToMatLab\';
    bodyPath = 'C:\Users\Phil\Documents\BodyData\';

    readDepth = 0;
    readColor= 1;
    readBody =1;



    %b = dir([bodyPath '*.txt']);

    if(readDepth)
        d = dir([depthPath '*.mat']);
        
        depthTimeStamps = zeros(length(c),1);
        depths = cell(length(c),1);
        for i=1:length(d)
           temp = strsplit(c(1).name, {'0','.'});
           depthTimeStamps(i) = temp{2};
           depths{i} = load('-mat', [depthPath d(i).name]);
        end%endfor 
    end%if readDepth

    if(readColor)
        c = dir([colorPath '*.png']);
        
        colorTimeStamps = zeros(length(c),1);
        colors = cell(1,length(c));
        for i=1:length(c)
            temp = strsplit(c(1).name, {'0','.'});
            colorTimeStamps(i) = temp{2};
            colors{i} = imread([colorPath c(i).name]);    
        end%endfor
    end%if read color


    if(readBody)
        [bodyMatrix,bodyTimeStamps] = readBodyDataSavedAtEnd(bodyPath);
    end%end if read body
end %end readStuff




function [bodyMatrix,timestamps] = readBodyDataSavedAtEnd(bodyPath)

    b = dir([bodyPath '*.txt']);

    %maps = [];
    for i=1:length(b)
        fid = fopen([bodyPath strcat('Body',num2str(i),'.txt')]);
       
        %get how many data points have been saved per joint
        first = textscan(fid,'%s %s',1);
        split = strsplit(first{2}{1},'-');
        len = length(split);
        format = ['%s ' repmat('%f,%f-',1,len-1) '%f,%f'];
        frewind(fid);
        
        
        %data is a 1x2N+1 cell array, where N is the number of samples per joint
        %cell 1 has the joint names, cells 2:2N+1 have an x or y value per joint
        bodyMatrix = textscan(fid,format);
        fclose(fid);
        
        
        jointNames = bodyMatrix{1,1};
        
        
       
        %reduce data to just the numbers
        bodyMatrix = bodyMatrix(1,2:length(bodyMatrix));
        
        
        %convert the cell array to a matrix
        bodyMatrix = cell2mat(bodyMatrix);
    
        %get timestamps
        timestamps = bodyMatrix(size(bodyMatrix,1),:);
        
        %get rid of timestamps
        bodyMatrix = bodyMatrix(1:size(bodyMatrix,1)-1,:);
        
        %jointMap = containers.Map();
        %add a vector for each joneName
        %for j=1:(length(jointNames)-1)%-1 cause of the time stamps
        %    jointMap(jointNames{j}) = data(j,:);
        %end
        %maps{i} = jointMap;
    end%for i=1 to length b

    
    
    

end%readBodyDataSavedatEnd















































































































































function [sit] = isSitting(jointIndexMap,points,frameNumber)

    yIndex = frameNumber*2;%because points has x and y in separate columns
    
    hipToKnee = [points(jointIndexMap('HipRight'),yIndex-1)-points(jointIndexMap('KneeRight'),yIndex-1), ...
            points(jointIndexMap('HipRight'),yIndex)-points(jointIndexMap('KneeRight'),yIndex)];
     
    kneeToAnkle = [points(jointIndexMap('AnkleRight'),yIndex-1)-points(jointIndexMap('KneeRight'),yIndex-1), ...
            points(jointIndexMap('AnkleRight'),yIndex)-points(jointIndexMap('KneeRight'),yIndex)];
    
        
    angle =(180/3.14159)* acos( hipToKnee*kneeToAnkle' / (norm(hipToKnee) * norm(kneeToAnkle)));
    
    
    sit = 0;
    if(angle< 150)
        sit =1;
    end

end



%returns the angle between the segments ab and cd
function [angle] = getAngle(a,b,c,d)

    angle = acos([a b] * [c d] / (norm([a b])+norm([c d]))); 

end




function[maps] =  readBodyDataSavedOneAtATime(bodyPath)


end







function [] = drawSkeleton(jointIndexMap,bodyMatrix, frameNum)
    
    yIndex = 2*frameNum;%because points has x and y in separate columns
    
    figure
    p =plot(bodyMatrix(:,yIndex-1),bodyMatrix(:,yIndex),'y.','MarkerSize',15);%plot joints
   % hold on
    line([bodyMatrix(jointIndexMap('Head'),yIndex-1) bodyMatrix(jointIndexMap('Neck'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('Head'),yIndex) bodyMatrix(jointIndexMap('Neck'),yIndex)]);
        
    line([bodyMatrix(jointIndexMap('SpineShoulder'),yIndex-1) bodyMatrix(jointIndexMap('Neck'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineShoulder'),yIndex) bodyMatrix(jointIndexMap('Neck'),yIndex)]);
    
    line([bodyMatrix(jointIndexMap('SpineShoulder'),yIndex-1) bodyMatrix(jointIndexMap('SpineMid'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineShoulder'),yIndex) bodyMatrix(jointIndexMap('SpineMid'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('SpineBase'),yIndex-1) bodyMatrix(jointIndexMap('SpineMid'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineBase'),yIndex) bodyMatrix(jointIndexMap('SpineMid'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('SpineBase'),yIndex-1) bodyMatrix(jointIndexMap('HipRight'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineBase'),yIndex) bodyMatrix(jointIndexMap('HipRight'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('SpineBase'),yIndex-1) bodyMatrix(jointIndexMap('HipLeft'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineBase'),yIndex) bodyMatrix(jointIndexMap('HipLeft'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('SpineShoulder'),yIndex-1) bodyMatrix(jointIndexMap('ShoulderRight'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineShoulder'),yIndex) bodyMatrix(jointIndexMap('ShoulderRight'),yIndex)]);
    
    line([bodyMatrix(jointIndexMap('SpineShoulder'),yIndex-1) bodyMatrix(jointIndexMap('ShoulderLeft'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('SpineShoulder'),yIndex) bodyMatrix(jointIndexMap('ShoulderLeft'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('KneeRight'),yIndex-1) bodyMatrix(jointIndexMap('HipRight'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('KneeRight'),yIndex) bodyMatrix(jointIndexMap('HipRight'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('KneeRight'),yIndex-1) bodyMatrix(jointIndexMap('AnkleRight'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('KneeRight'),yIndex) bodyMatrix(jointIndexMap('AnkleRight'),yIndex)]);
    
    line([bodyMatrix(jointIndexMap('FootRight'),yIndex-1) bodyMatrix(jointIndexMap('AnkleRight'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('FootRight'),yIndex) bodyMatrix(jointIndexMap('AnkleRight'),yIndex)]);
    
    
    line([bodyMatrix(jointIndexMap('KneeLeft'),yIndex-1) bodyMatrix(jointIndexMap('HipLeft'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('KneeLeft'),yIndex) bodyMatrix(jointIndexMap('HipLeft'),yIndex)]);
     
    line([bodyMatrix(jointIndexMap('KneeLeft'),yIndex-1) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('KneeLeft'),yIndex) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex)]);
    
    line([bodyMatrix(jointIndexMap('FootLeft'),yIndex-1) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex-1)], ...
            [bodyMatrix(jointIndexMap('FootLeft'),yIndex) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex)]);
    
    %rotate(p,[0,0],180);
    
end%draw skeleton



%bad, erros,  just here to da
function [] = doIsStitting()



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

        sitting = zeros(1,length(bodyMatrix)/2);
        for j=1:length(bodyMatrix)/2
            sitting(j) = isSitting(jointIndexMap,bodyMatrix,j);
        end

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
        if(readColor)
           for j=1:length(sitting) 
              %imshow(colors{sitting(j)});
              %pause;
           end
        end
end



%joint names

%'SpineBase'
 %   'SpineMid'
  %  'Neck'
   % 'Head'
   % 'ShoulderLeft'
    %'ElbowLeft'
    %'WristLeft'
    %'HandLeft'
    %'ShoulderRight'
    %'ElbowRight'
    %'WristRight'
    %'HandRight'
    %'HipLeft'
    %'KneeLeft'
    %'AnkleLeft'
    %'FootLeft'
    %'HipRight'
    %'KneeRight'
    %'AnkleRight'
    %'FootRight'
    %'SpineShoulder'
    %'HandTipLeft'
    %'ThumbLeft'
    %'HandTipRight'
    %'ThumbRight'