d= dir('./')

%start at 3 skip . ..
for i=3:length(d)
    name = d(i).name;
    
    hasSubfolders = false;
    parent = strcat('./', name, '/'); 
    subD = dir(parent);
    
    for j=1:length(subD)
        if(strcmp(subD(j).name, name))
            hasSubfolders = true;
            break
        end
    end
    parent = strcat(parent, name,'/');
    if(~hasSubfolders)
        mkdir(parent,'rgb');
        mkdir(parent,'unreg_depth');
        mkdir(parent,'reg_depth');
    end
end