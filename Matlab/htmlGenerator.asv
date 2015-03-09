%script to save data to an html file for visualization

%how many figures we have saved so far
current_figure_num = 1;


%as an example generate 10 sets of 100 x,y pairs
x = rand(100,3);
y = rand(100,3);




image_dir = './images/';

mkdir('./', image_dir);



fid = fopen('./index.html','w');

%some headers for html
fprintf(fid, '<!DOCTYPE html>\n');
    fprintf(fid, '<html>\n');
        fprintf(fid, '<head>\n');
            fprintf(fid, '<title> Results Vis </title>\n');
        fprintf(fid, '</head>\n');
        
        
        %what is visible on the page
        fprintf(fid, '<body>\n');
            fprintf(fid, '<h1>Results Vis</h1>\n');

                % a table to hold results, (this one is N x 2)
                fprintf(fid, '<table style="width:100%">\n');
                
                    %for copy/pasting  ( a blank row in the beginning)
                    fprintf(fid, '<tr>\n');
                        fprintf(fid, '<td>\n');
                        fprintf(fid, '</td>\n');
                    fprintf(fid, '</tr>\n');
                    
                    %loop over all data sets,(there will be one figure
                    %per data set)
                    for i=1:size(x,2)
                        
                        %make the figure(but don't display it)
                        h=figure('visible', 'off');
                        %h=figure(current_figure_num);
                        plot(x,y);%this is just the example
                        
                        
                        %make a filename to save the figure
                        file_name=strcat(image_dir, 'Result', ... 
                                         num2str(current_figure_num));
                                          
                        saveas(h,file_name,'jpg');
                        
                        %put the image in the html file
                        fprintf(fid, '<tr>\n'); %row in table
                            fprintf(fid, '<td>\n'); %column in table
                                fprintf(fid, 'Caption\n');
                            fprintf(fid, '</td>\n');
                            
                            fprintf(fid, '<td>\n'); %another column in table
                               %put in the jpg image
                               fprintf(fid, ...
                                       strcat(' <img src="',  ...
                                       strcat(file_name, '.jpg'), ...
                                       '" style="width:304px;height:228px">'));
                            fprintf(fid, '</td>\n');
                        fprintf(fid, '</tr>\n');
                        
                        current_figure_num = current_figure_num +1;
                    end%for i
                    
                 %close all tags   
                fprintf(fid, '</table>\n');
        fprintf(fid, '</body>\n');
    fprintf(fid, '</html>\n');
    
    

fclose(fid);





