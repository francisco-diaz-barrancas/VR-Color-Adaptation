%clear all;

%% Set up the Import Options and import the data
opts = delimitedTextImportOptions("NumVariables", 13);

% Specify range and delimiter
opts.DataLines = [2, Inf];
opts.Delimiter = "\t";

% Specify column names and types
opts.VariableNames = ["Lighting", "Time", "CheckScene", "ChooseOption", "Pos0", "Pos1", "Pos2", "Pos3", "Pos4", "Pos5", "Pos6", "Pos7", "Pos8"];
opts.VariableTypes = ["string", "double", "double", "string", "string", "string", "string", "string", "string", "string", "string", "string", "string"];

% Specify file level properties
opts.ExtraColumnsRule = "ignore";
opts.EmptyLineRule = "read";

% Specify variable properties
opts = setvaropts(opts, ["Lighting", "ChooseOption", "Pos0", "Pos1", "Pos2", "Pos3", "Pos4", "Pos5", "Pos6", "Pos7", "Pos8"], "WhitespaceRule", "preserve");
opts = setvaropts(opts, ["Lighting", "ChooseOption", "Pos0", "Pos1", "Pos2", "Pos3", "Pos4", "Pos5", "Pos6", "Pos7", "Pos8"], "EmptyFieldRule", "auto");


%Variables
contAchromatic=[0,0,0,0,0,0];
percentages=[0,0,0,0,0,0];
participants=1;
times=[20,40,60,80,100,120];

% Import the data
%Prueba1 = readtable("D:\Francisco\ProyectosVRUnity\Color constancy Karl\Assets\Results\Prueba_1.csv", opts);

if(contains(string(table2cell(Prueba1(1,1))),"Red")==1)
    for i=1:6
        if(contains(string(table2cell(Prueba1(i,4))),"S05N")==1)
            contAchromatic(1,i)=contAchromatic(1,i)+1;     
        end
    end
end

if(contains(string(table2cell(Prueba1(1,1))),"Green")==1)
    for i=1:6
        if(contains(string(table2cell(Prueba1(i,4))),"S05N")==1)
            contAchromatic(1,i)=contAchromatic(1,i)+1;     
        end
    end
end

if(contains(string(table2cell(Prueba1(1,1))),"D65")==1)
    for i=1:6
        if(contains(string(table2cell(Prueba1(i,4))),"S05N")==1)
            contAchromatic(1,i)=contAchromatic(1,i)+1;     
        end
    end
end

if(contains(string(table2cell(Prueba1(1,1))),"Blue")==1)
    for i=1:6
        if(contains(string(table2cell(Prueba1(i,4))),"S05N")==1)
            contAchromatic(1,i)=contAchromatic(1,i)+1;     
        end
    end
end
percentages(1,:)=(contAchromatic(1,:)./participants).*100;
plot(times(1,:),percentages(1,:));


%plot(col1, col2)