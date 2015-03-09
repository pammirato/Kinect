

maxNumToRead = 40000;

objectName = 'Test3';

savePath = strcat('C:/Users/ammirato/Documents/KinectData/' , objectName, '/');


colorPath = savePath; %'C:\Users\Phil\Documents\KinectToMatLab\';
depthPath = savePath; %'C:\Users\Phil\Documents\KinectToMatLab\';
bodyPath = savePath; %'C:\Users\Phil\Documents\BodyData\';

readDepth = 0;
readColor= 1;
readBody =0;


%possible outputs
depths = {};
regDepths = {};
depthTimeStamps = {};
bodyMatrix = [];
bodyTimeStamps = {};
colors = {};
colorTimeStamps = {};

%b = dir([bodyPath '*.txt']);

if(readDepth)
    d = dir([depthPath '*.mat']);

    depthTimeStamps = zeros(length(d),1);
    %depths = cell(length(d),1);
    
    for i=1:length(d)
        if(i>maxNumToRead)
            break;
        end
       %temp = strsplit(d(i).name, {objectName,'.'});
       %depthTimeStamps(i) = str2num(temp{2});
       
       if(strfind(d(i).name, 'NEW'))
            regDepths{length(regDepths)+1} = load('-mat', [depthPath d(i).name]);
       else
            depths{length(depths)+1} = load('-mat', [depthPath d(i).name]);
       end
    end%endfor 
end%if readDepth

if(readColor)
    c = dir([colorPath '*.png']);

    colorTimeStamps = zeros(length(c),1);
    colors = cell(1,length(c));
    for i=1:length(c)
        if(i>maxNumToRead)
            break;
        end
        temp = strsplit(c(i).name, {objectName,'.'});
        %colorTimeStamps(i) = str2num(temp{2});
        colors{i} = imread([colorPath c(i).name]);    
    end%endfor
end%if read color


if(readBody)
   
    b = dir([bodyPath '*.txt']);

    for i=1:length(b)
        fid = fopen([bodyPath strcat('Body',num2str(i),'.txt')]);
       
        %get how many data points have been saved per joint
        first = textscan(fid,'%s %s',1);
        split = strsplit(first{2}{1},'-');
        len = length(split);
        format = ['%s ' repmat('%f,%f-',1,len-1) '%f,%f'];
        frewind(fid);
        
        
        %bodyMatrix is a 1x2N+2 cell array, where N is the number of samples per joint
        %cell 1 has the joint names, cells 2:2N+1 have an x or y value per joint
        bodyMatrix = textscan(fid,format);
        fclose(fid);
        
        
        jointNames = bodyMatrix{1,1};
        
        
       
        %reduce data to just the numbers
        bodyMatrix = bodyMatrix(1,2:length(bodyMatrix));
        
        
        %convert the cell array to a matrix
        bodyMatrix = cell2mat(bodyMatrix);
    
        %get timestamps
        bodyTimeStamps = bodyMatrix(size(bodyMatrix,1),:);
        %get rid of -1's that were needed for formatting
        bodyTimeStamps = bodyTimeStamps(1:2:end)'; %get odd rows, and make it a column vecotr 
        
        %get rid of timestamps
        bodyMatrix = bodyMatrix(1:size(bodyMatrix,1)-1,:);
    end%for i=1 to length b
end%end if read body




%just so we have it
jointIndexMap = containers.Map();
if(readBody)
    for i=1:length(jointNames)-1%-1 because of TimeStamp
            jointIndexMap(jointNames{i}) = i;
    end
end





