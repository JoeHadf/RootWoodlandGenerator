using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class FileGenerator
{
    private WorldState worldState;

    public FileGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void GenerateFile(string fileName)
    {
        List<Clearing> clearings = worldState.clearings;
        Dictionary<int, int> clearingIDToIndex = new Dictionary<int, int>(clearings.Count);
        Line[] clearingLines = new Line[clearings.Count];
        
        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            ClearingLine currentLine = new ClearingLine(currentClearing.clearingName, currentClearing.GetPosition(), currentClearing.majorDenizen
                ,currentClearing.clearingControl, currentClearing.hasBuilding, currentClearing.hasSympathy);
            clearingIDToIndex.Add(currentClearing.clearingID, i);
            clearingLines[i] = currentLine;
        }

        List<Path> paths = worldState.paths;
        Line[] pathLines = new Line[paths.Count];

        for (int i = 0; i < paths.Count; i++)
        {
            Path currentPath = paths[i];
            PathID currentPathID = currentPath.pathID;
            PathLine currentLine = new PathLine(clearingIDToIndex[currentPathID.startID],
                clearingIDToIndex[currentPathID.endID]);
            pathLines[i] = currentLine;
        }

        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();

        using (StreamWriter file = new StreamWriter(System.IO.Path.Combine(myWoodlandsPath, fileName + ".root")))
        {
            for (int i = 0; i < clearingLines.Length; i++)
            {
                file.WriteLine(clearingLines[i].GetLineString());
            }
            
            file.WriteLine("");

            for (int i = 0; i < pathLines.Length; i++)
            {
                file.WriteLine(pathLines[i].GetLineString());
            }
        }
    }

    public void ReadFileWithName(string name)
    {
        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();
        string filePath = System.IO.Path.Combine(myWoodlandsPath, name + ".root");

        if (File.Exists(filePath))
        {
            worldState.DeleteAllClearings();
            ReadFile(File.ReadAllText(filePath));
        }
    }

    public void ReadFile(string file)
    {
        string[] splitFile = file.Split("\r\n\r\n");

        if (splitFile.Length == 2)
        {
            string clearingFile = splitFile[0];
            string pathFile = splitFile[1];

            string[] stringClearingLines = clearingFile.Split("\r\n");
            string[] stringPathLines = pathFile.Split("\r\n");

            Dictionary<int, int> indexToClearingID = new Dictionary<int, int>(stringClearingLines.Length);

            for (int i = 0; i < stringClearingLines.Length; i++)
            {
                if (TryReadAsClearingLine(stringClearingLines[i], out ClearingLine clearingLine))
                {
                    Clearing currentClearing = worldState.GenerateClearing(new Vector3(clearingLine.posX, clearingLine.posY, 0));
                    
                    currentClearing.SetClearingName(clearingLine.name);
                    currentClearing.SetMajorDenizen((DenizenType)clearingLine.denizenID);
                    currentClearing.SetClearingControl((FactionType)clearingLine.factionID);
                    currentClearing.SetHasBuilding(Convert.ToBoolean(clearingLine.hasBuilding));
                    currentClearing.SetHasSympathy(Convert.ToBoolean(clearingLine.hasWoodlandAlliancePresence));
                    
                    indexToClearingID.Add(i, currentClearing.clearingID);
                }
            }
            
            for (int i = 0; i < stringPathLines.Length; i++)
            {
                if (TryReadAsPathLine(stringPathLines[i], out PathLine pathLine))
                {
                    if (indexToClearingID.TryGetValue(pathLine.startClearing, out int startClearingID) &&
                        indexToClearingID.TryGetValue(pathLine.endClearing, out int endClearingID))
                    {
                        worldState.GeneratePath(startClearingID, endClearingID);
                    }
                }
            }
        }
    }

    private bool TryReadAsClearingLine(string stringClearingLine, out ClearingLine clearingLine)
    {
        bool failedRead = false;
        
        string[] splitLine = stringClearingLine.Split(",");

        if (splitLine.Length == 7)
        {
            string name = splitLine[0];
            
            if (!float.TryParse(splitLine[1], out float posX))
            {
                failedRead = true;
            }

            if (!float.TryParse(splitLine[2], out float posY))
            {
                failedRead = true;
            }
            
            if (!Int32.TryParse(splitLine[3], out int denizenID) || !Enum.IsDefined(typeof(DenizenType), denizenID))
            {
                failedRead = true;
            }

            if (!Int32.TryParse(splitLine[4], out int factionID) || !Enum.IsDefined(typeof(FactionType), factionID))
            {
                failedRead = true;
            }

            if (!Int32.TryParse(splitLine[5], out int hasBuilding))
            {
                failedRead = true;
            }
            
            if (!Int32.TryParse(splitLine[6], out int hasWoodlandAlliancePresence))
            {
                failedRead = true;
            }

            if (!failedRead)
            {
                clearingLine = new ClearingLine(name, posX, posY, denizenID, factionID, hasBuilding, hasWoodlandAlliancePresence);
            }
            else
            {
                clearingLine = new ClearingLine();
            }

        }
        else
        {
            failedRead = true;
            clearingLine = new ClearingLine();
        }

        return !failedRead;
    }
    
    private bool TryReadAsPathLine(string stringPathLine, out PathLine pathLine)
    {
        bool failedRead = false;
        
        string[] splitLine = stringPathLine.Split(",");

        if (splitLine.Length == 2)
        {
            if (!Int32.TryParse(splitLine[0], out int startClearing))
            {
                failedRead = true;
            }
            
            if (!Int32.TryParse(splitLine[1], out int endClearing))
            {
                failedRead = true;
            }
            
            if (!failedRead)
            {
                pathLine = new PathLine(startClearing, endClearing);
            }
            else
            {
                pathLine = new PathLine(0,0);
            }

        }
        else
        {
            failedRead = true;
            pathLine = new PathLine(0,0);
        }

        return !failedRead;
    }

    private abstract class Line
    {
        public abstract string GetLineString();
    }

    private class PathLine : Line
    {
        public int startClearing { get; private set; }
        public int endClearing { get; private set; }

        public PathLine(int startClearing, int endClearing)
        {
            this.startClearing = startClearing;
            this.endClearing = endClearing;
        }
        
        public override string GetLineString()
        {
            return $"{startClearing},{endClearing}";
        }
    }
    
    private class ClearingLine : Line
    {
        public string name { get; private set; }
        public float posX { get; private set; }
        public float posY { get; private set; }
        public int denizenID { get; private set; }
        public int factionID { get; private set; }
        public int hasBuilding { get; private set; }
        public int hasWoodlandAlliancePresence { get; private set; }

        public ClearingLine(string name, Vector3 position, DenizenType denizen, FactionType faction,bool hasBuilding,
            bool hasWoodlandAlliancePresence)
        {
            this.name = name;
            this.posX = position.x;
            this.posY = position.y;
            this.denizenID = (int)denizen;
            this.factionID = (int)faction;
            this.hasBuilding = (hasBuilding) ? 1 : 0;
            this.hasWoodlandAlliancePresence = (hasWoodlandAlliancePresence) ? 1 : 0;
        }
        
        public ClearingLine(string name, float posX, float posY, int denizen, int faction, int hasBuilding,
            int hasWoodlandAlliancePresence)
        {
            this.name = name;
            this.posX = posX;
            this.posY = posY;
            this.denizenID = denizen;
            this.factionID = faction;
            this.hasBuilding = hasBuilding;
            this.hasWoodlandAlliancePresence = hasWoodlandAlliancePresence;
        }

        public ClearingLine()
        {
            this.name = "";
            this.posX = 0;
            this.posY = 0;
            this.denizenID = 0;
            this.factionID = 0;
            this.hasBuilding = 0;
            this.hasWoodlandAlliancePresence = 0;
        }

        public override string GetLineString()
        {
            return $"{name},{posX},{posY},{denizenID},{factionID},{hasBuilding},{hasWoodlandAlliancePresence}";
        }
    }
}
