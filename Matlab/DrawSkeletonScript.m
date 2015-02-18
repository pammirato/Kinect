frameNum = 42;

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

line(424-[bodyMatrix(jointIndexMap('FootLeft'),yIndex-1) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex-1)], ...
        [bodyMatrix(jointIndexMap('FootLeft'),yIndex) bodyMatrix(jointIndexMap('AnkleLeft'),yIndex)]);
