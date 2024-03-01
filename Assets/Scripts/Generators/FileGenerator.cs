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

        List<AttributeLine> attributeLines = new List<AttributeLine>(clearings.Count * 6);
        Dictionary<int, int> currentClearingIDToSaveID = new Dictionary<int, int>(clearings.Count);

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            
            currentClearingIDToSaveID.Add(currentClearing.clearingID, i);
            
            attributeLines.Add(new NameAttribute(i, currentClearing.clearingName));
            attributeLines.Add(new PositionAttribute(i, currentClearing.GetPosition()));
            attributeLines.Add(new DenizenAttribute(i, currentClearing.majorDenizen));
            attributeLines.Add(new FactionControlAttribute(i, currentClearing.clearingControl));
            attributeLines.Add(new HasBuildingAttribute(i, currentClearing.hasBuilding));

            FactionType[] presentFactions = currentClearing.GetPresentFactions();

            for (int j = 0; j < presentFactions.Length; j++)
            {
                attributeLines.Add(new FactionPresenceAttribute(i, presentFactions[j]));
            }
        }

        List<Path> paths = worldState.paths;
        PathLine[] pathLines = new PathLine[paths.Count];

        for (int i = 0; i < paths.Count; i++)
        {
            Path currentPath = paths[i];
            PathID pathID = currentPath.pathID;
            
            pathLines[i] = new PathLine(currentClearingIDToSaveID[pathID.startID], currentClearingIDToSaveID[pathID.endID]);
        }

        List<Clearing> river = worldState.river.GetRiverClearings();
        int[] riverClearingIDs = new int[river.Count];

        for (int i = 0; i < river.Count; i++)
        {
            riverClearingIDs[i] = currentClearingIDToSaveID[river[i].clearingID];
        }

        bool includeClearings = attributeLines.Count > 0;
        bool includePaths = pathLines.Length > 0;
        bool includeRiver = riverClearingIDs.Length > 0;

        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();

        using (StreamWriter file = new StreamWriter(System.IO.Path.Combine(myWoodlandsPath, fileName + ".wdld")))
        {
            if (includeClearings)
            {
                file.WriteLine((int)FileSectionType.Clearings);
                file.WriteLine(clearings.Count);
                for (int i = 0; i < attributeLines.Count; i++)
                {
                    file.WriteLine(attributeLines[i].GetAttributeLine());
                }
            }

            if (includePaths)
            {
                file.WriteLine("");
                file.WriteLine((int)FileSectionType.Paths);
                for (int i = 0; i < pathLines.Length; i++)
                {
                    file.WriteLine(pathLines[i].GetPathLine());
                }
            }

            if (includeRiver)
            {
                file.WriteLine("");
                file.WriteLine((int)FileSectionType.River);
                for (int i = 0; i < riverClearingIDs.Length; i++)
                {
                    file.WriteLine(riverClearingIDs[i]);
                }
            }
        }
    }
    
    public void ReadFileWithName(string name)
    {
        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();
        string filePath = System.IO.Path.Combine(myWoodlandsPath, name + ".wdld");

        if (File.Exists(filePath))
        {
            ReadFile(File.ReadAllText(filePath));
        }
    }
    
    public void ReadFile(string file)
    {
        worldState.DeleteAllClearings();
        
        string[] splitFile = file.Split("\r\n\r\n");

        Dictionary<int, int> indexToClearingID = new Dictionary<int, int>();

        for (int i = 0; i < splitFile.Length; i++)
        {
            string[] sectionLines = splitFile[i].Split("\r\n");

            if (sectionLines.Length > 1 && Int32.TryParse(sectionLines[0], out int sectionID))
            {
                FileSectionType fileSectionType = (FileSectionType)sectionID;
                switch (fileSectionType)
                {
                    case FileSectionType.Clearings:
                        indexToClearingID = ReadClearingsSection(sectionLines);
                        break;
                    case FileSectionType.Paths:
                        ReadPathsSection(sectionLines, indexToClearingID);
                        break;
                    case FileSectionType.River:
                        ReadRiverSection(sectionLines, indexToClearingID);
                        break;
                }
            }
        }
    }
    
    private Dictionary<int,int> ReadClearingsSection(string[] clearingLines)
    {
        int clearingCount = Int32.Parse(clearingLines[1]);

        Clearing[] clearings = new Clearing[clearingCount];
        Dictionary<int,int> indexToClearingID = new Dictionary<int,int>();

        for (int i = 0; i < clearings.Length; i++)
        {
            Clearing newClearing = worldState.GenerateClearing(Vector3.zero);
            clearings[i] = newClearing;
            indexToClearingID.Add(i, newClearing.clearingID);
        }

        for (int i = 2; i < clearingLines.Length; i++)
        {
            if (AttributeLine.TryReadAttributeLine(clearingLines[i], out AttributeLine attributeLine))
            {
                bool hasFoundClearingID = indexToClearingID.TryGetValue(attributeLine.clearingID, out int clearingID);
                if(hasFoundClearingID && worldState.TryGetClearingWithID(clearingID, out Clearing clearing))
                {
                    attributeLine.ApplyAttributeToClearing(clearing);
                }
            }
        }

        return indexToClearingID;
    }
    
    private void ReadPathsSection(string[] pathLines, Dictionary<int, int> indexToClearingID)
    {
        for (int i = 1; i < pathLines.Length; i++)
        {
            if (PathLine.TryReadAsPathLine(pathLines[i], out PathLine pathLine))
            {
                bool hasFoundStartID = indexToClearingID.TryGetValue(pathLine.startClearingID, out int startID);
                bool hasFoundEndID = indexToClearingID.TryGetValue(pathLine.endClearingID, out int endID);

                if (hasFoundStartID && hasFoundEndID)
                {
                    bool hasFoundStartClearing = worldState.TryGetClearingWithID(startID, out Clearing startClearing);
                    bool hasFoundEndClearing = worldState.TryGetClearingWithID(endID, out Clearing endClearing);
                    if (hasFoundStartClearing && hasFoundEndClearing)
                    {
                        worldState.GeneratePath(startClearing, endClearing);
                    }
                }
            }
        }
    }

    private void ReadRiverSection(string[] riverLines, Dictionary<int, int> indexToClearingID)
    {
        for (int i = 1; i < riverLines.Length; i++)
        {
            if (Int32.TryParse(riverLines[i], out int riverClearingIndex))
            {
                bool hasFoundClearingID = indexToClearingID.TryGetValue(riverClearingIndex, out int riverClearingID);
                if (hasFoundClearingID && worldState.TryGetClearingWithID(riverClearingID, out Clearing clearing))
                {
                    worldState.river.AddClearingToRiver(clearing);
                }
            }
        }
    }
    
    private abstract class AttributeLine
    {
        public int clearingID;
        public AttributeType attributeType;

        protected AttributeLine(int clearingID, AttributeType attributeType)
        {
            this.clearingID = clearingID;
            this.attributeType = attributeType;
        }

        public abstract string GetAttributeLine();

        public abstract void ApplyAttributeToClearing(Clearing clearing);
        
        public static bool TryReadAttributeLine(string line, out AttributeLine attributeLine)
        {
            string[] splitLine = line.Split(",");
            
            attributeLine = null;
            bool hasReadLine = false;

            if (splitLine.Length > 2 && Int32.TryParse(splitLine[1], out int attributeID))
            {
                AttributeType attribute = (AttributeType)attributeID;
                
                switch (attribute)
                {
                    case NameAttribute.attribute:
                        attributeLine = new NameAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                    case PositionAttribute.attribute:
                        attributeLine = new PositionAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                    case DenizenAttribute.attribute:
                        attributeLine = new DenizenAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                    case FactionControlAttribute.attribute:
                        attributeLine = new FactionControlAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                    case HasBuildingAttribute.attribute:
                        attributeLine = new HasBuildingAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                    case FactionPresenceAttribute.attribute:
                        attributeLine = new FactionPresenceAttribute();
                        hasReadLine = attributeLine.TryReadIntoLine(splitLine);
                        break;
                }
            }

            return hasReadLine;
        } 
        
        internal string GetAttributeLinePrefix()
        {
            return $"{clearingID},{(int)attributeType},";
        }

        internal bool EnsureCapacity(string[] line, int capacity)
        {
            return line.Length == 2 + capacity;
        }

        private bool TryReadIntoLine(string[] line)
        {
            bool hasReadLine = false;

            if (TryReadClearingID(line))
            {
                hasReadLine = TryReadAttributeValues(line);
            }

            return hasReadLine;
        }
        
        private bool TryReadClearingID(string[] line)
        {
            if (line.Length >= 1 && Int32.TryParse(line[0], out int id))
            {
                clearingID = id;
                return true;
            }

            return false;
        }

        protected abstract bool TryReadAttributeValues(string[] line);
    }

    private class NameAttribute : AttributeLine
    {
        private string name;
        public const AttributeType attribute = AttributeType.Name;

        public NameAttribute(int clearingID, string name) : base(clearingID, attribute)
        {
            this.name = name;
        }

        public NameAttribute() : base(-1, attribute)
        {
            this.name = "";
        }

        public override string GetAttributeLine()
        {
            return GetAttributeLinePrefix() + $"{name}";
        }

        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetClearingName(name);
        }

        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 1))
            {
                this.name = line[2];
                return true;
            }

            return false;
        }
    }
    
    private class PositionAttribute : AttributeLine
    {
        private Vector3 position;
        public const AttributeType attribute = AttributeType.Position;
        
        public PositionAttribute(int clearingID, Vector3 position) : base(clearingID, attribute)
        {
            this.position = position;
        }

        public PositionAttribute() : base(-1, attribute)
        {
            this.position = Vector3.zero;
        }

        public override string GetAttributeLine()
        {
            return GetAttributeLinePrefix() + $"{position.x},{position.y}";
        }
        
        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetPosition(position);
        }
        
        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 2) && float.TryParse(line[2], out float posX) && float.TryParse(line[3], out float posY))
            {
                this.position = new Vector3(posX, posY, 0);
                return true;
            }

            return false;
        }
    }
    
    private class DenizenAttribute : AttributeLine
    {
        private DenizenType denizen;
        public const AttributeType attribute = AttributeType.Denizen;
        
        public DenizenAttribute(int clearingID, DenizenType denizen) : base(clearingID, attribute)
        {
            this.denizen = denizen;
        }

        public DenizenAttribute() : base(-1, attribute)
        {
            this.denizen = DenizenType.Fox;
        }

        public override string GetAttributeLine()
        {
            return GetAttributeLinePrefix() + $"{(int)denizen}";
        }
        
        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetMajorDenizen(denizen);
        }
        
        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 1) && Int32.TryParse(line[2], out int denizenID))
            {
                this.denizen = (DenizenType)denizenID;
                return true;
            }

            return false;
        }
    }
    
    private class FactionControlAttribute : AttributeLine
    {
        private FactionType faction;
        public const AttributeType attribute = AttributeType.FactionControl;
        
        public FactionControlAttribute(int clearingID, FactionType faction) : base(clearingID, attribute)
        {
            this.faction = faction;
        }

        public FactionControlAttribute() : base(-1, attribute)
        {
            this.faction = FactionType.Denizens;
        }

        public override string GetAttributeLine()
        {
            return GetAttributeLinePrefix() + $"{(int)faction}";
        }
        
        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetClearingControl(faction);
        }
        
        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 1) && Int32.TryParse(line[2], out int factionID))
            {
                this.faction = (FactionType)factionID;
                return true;
            }

            return false;
        }
    }
    
    private class HasBuildingAttribute : AttributeLine
    {
        private bool hasBuilding;
        public const AttributeType attribute = AttributeType.HasBuilding;
        
        public HasBuildingAttribute(int clearingID, bool hasBuilding) : base(clearingID, attribute)
        {
            this.hasBuilding = hasBuilding;
        }

        public HasBuildingAttribute() : base(-1, attribute)
        {
            this.hasBuilding = false;
        }

        public override string GetAttributeLine()
        {
            int hasBuildingValue = (hasBuilding) ? 1 : 0;
            return GetAttributeLinePrefix() + $"{hasBuildingValue}";
        }
        
        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetHasBuilding(hasBuilding);
        }
        
        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 1) && Int32.TryParse(line[2], out int hasBuildingID))
            {
                this.hasBuilding = Convert.ToBoolean(hasBuildingID);
                return true;
            }

            return false;
        }
    }
    
    private class FactionPresenceAttribute : AttributeLine
    {
        private FactionType faction;
        public const AttributeType attribute = AttributeType.FactionPresence;
        
        public FactionPresenceAttribute(int clearingID, FactionType faction) : base(clearingID, attribute)
        {
            this.faction = faction;
        }

        public FactionPresenceAttribute() : base(-1, attribute)
        {
            this.faction = FactionType.Denizens;
        }

        public override string GetAttributeLine()
        {
            return GetAttributeLinePrefix() + $"{(int)faction}";
        }
        
        public override void ApplyAttributeToClearing(Clearing clearing)
        {
            clearing.SetPresence(faction);
        }
        
        protected override bool TryReadAttributeValues(string[] line)
        {
            if (EnsureCapacity(line, 1) && Int32.TryParse(line[2], out int factionID))
            {
                this.faction = (FactionType)factionID;
                return true;
            }

            return false;
        }
    }

    private struct PathLine
    {
        public int startClearingID { get; private set; }
        public int endClearingID { get; private set; }

        public PathLine(int startClearingID, int endClearingID)
        {
            this.startClearingID = startClearingID;
            this.endClearingID = endClearingID;
        }

        public string GetPathLine()
        {
            return $"{startClearingID},{endClearingID}";
        }

        public static bool TryReadAsPathLine(string line, out PathLine pathLine)
        {
            pathLine = new PathLine(-1,-1);
            bool hasReadLine = false;
            
            string[] splitLine = line.Split(",");

            if (splitLine.Length == 2)
            {
                if (Int32.TryParse(splitLine[0], out int startID) && Int32.TryParse(splitLine[1], out int endID))
                {
                    pathLine = new PathLine(startID, endID);
                    hasReadLine = true;
                }
            }

            return hasReadLine;
        }
}
    
    private enum AttributeType
    {
        Name = 0,
        Position = 1,
        Denizen = 2,
        FactionControl = 3,
        HasBuilding = 4,
        FactionPresence = 5,
    }

    private enum FileSectionType
    {
        Clearings = 0,
        Paths = 1,
        River = 2
    }
}
