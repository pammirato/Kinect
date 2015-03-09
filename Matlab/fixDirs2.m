d= dir('./')

%start at 3 skip . ..
for i=3:length(d)
    
    if(~d(i).isdir)
        continue;
    end
    name = d(i).name
    
    hasSubfolders = false;
    parent = strcat('./', name, '/'); 
    subD = dir(parent);
    
    for j=1:length(subD)
        if(strcmp(subD(j).name, name))
            hasSubfolders = true;
            break
        end
    end
    parent2 = strcat(parent, name,'/');
    if(~hasSubfolders)
        mkdir(parent2,'rgb');
        mkdir(parent2,'unreg_depth');
        mkdir(parent2,'reg_depth');
        
        mkdir(strcat(parent,'rgb'),'selects');
        
    else
        
        if(length(dir([strcat(parent2,'reg_depth/') '*.png'])) > 2)
            continue;
        end
        %get selectedf timestamps
       selects = dir([strcat(parent,'rgb/selects/') '*.png']);
       selectsTimestamps = zeros(1,length(selects));
       for j=1:length(selects)
          temp = strsplit(selects(j).name, {name,'_'});
          selectsTimestamps(j) = str2num(temp{2});
       end
       
       
       %get all timestamps
       rgb = dir([strcat(parent,'rgb/') '*.png']);
       rgbTimestamps = zeros(1,length(rgb));
       for j=1:length(rgb)
          temp = strsplit(rgb(j).name, {name,'_'});
          rgbTimestamps(j) = str2num(temp{2});
       end
       
       
       
       
       
       unreg_depths = dir([strcat(parent,'unreg_depth/') '*.png']);
       unregTimestamps = zeros(1,length(unreg_depths));
       for j=1:length(unreg_depths)
          temp = strsplit(unreg_depths(j).name, {name,'_'});
          unregTimestamps(j) = str2num(temp{2});
       end
       
       reg_depths = dir([strcat(parent,'reg_depth/') '*.png']);
       regTimestamps = zeros(1,length(reg_depths));
       for j=1:length(reg_depths)
          temp = strsplit(reg_depths(j).name, {name,'_'});
          regTimestamps(j) = str2num(temp{2});
       end
       
       rgbCounter =1;
       regCounter =1;
       unregCounter=1;
       for j=1:length(selectsTimestamps)
          curTime = selectsTimestamps(j);
          
          
          
          
          if(rgbCounter > length(rgbTimestamps))
              disp('error rgb');
              break;
          end
          rgbTime = rgbTimestamps(rgbCounter);
          while(abs(rgbTime-curTime) > 0)
              rgbCounter = rgbCounter+1;
              rgbTime = rgbTimestamps(rgbCounter);           
          end
          
          
          
          
          if(regCounter > length(regTimestamps))
              disp('error reg');
              break;
          end
          regTime = regTimestamps(regCounter);
          while(abs(regTime-curTime) > 10)
              regCounter = regCounter+1;
              regTime = regTimestamps(regCounter);           
          end
          
          
          
          if(unregCounter > length(unregTimestamps))
              disp('error unreg');
              break;
          end
          unregTime = unregTimestamps(unregCounter);
          while(abs(unregTime-curTime) > 10)
              unregCounter = unregCounter+1;
              unregTime = unregTimestamps(unregCounter);           
          end
          
          %copy the files over
          copyfile(strcat(parent,'rgb/',rgb(unregCounter).name), ...
                    strcat(parent2,'rgb/',rgb(unregCounter).name));
          
          copyfile(strcat(parent,'unreg_depth/',unreg_depths(unregCounter).name), ...
                    strcat(parent2,'unreg_depth/',unreg_depths(unregCounter).name));
          
                
          copyfile(strcat(parent,'reg_depth/',reg_depths(regCounter).name), ...
                    strcat(parent2,'reg_depth/',reg_depths(regCounter).name));

       end
       
       
    end%if has subfolders or not
    
end