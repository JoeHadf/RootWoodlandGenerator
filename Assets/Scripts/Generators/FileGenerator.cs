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
            if (currentClearing.hasSympathy)
            {
                attributeLines.Add(new FactionPresenceAttribute(i, FactionType.WoodlandAlliance));
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

        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();

        using (StreamWriter file = new StreamWriter(System.IO.Path.Combine(myWoodlandsPath, fileName + ".root")))
        {
            file.WriteLine(clearings.Count);
            for (int i = 0; i < attributeLines.Count; i++)
            {
                file.WriteLine(attributeLines[i].GetAttributeLine());
            }
            
            file.WriteLine("");

            for (int i = 0; i < pathLines.Length; i++)
            {
                file.WriteLine(pathLines[i].GetPathLine());
            }
            
            file.WriteLine("");

            for (int i = 0; i < riverClearingIDs.Length; i++)
            {
                file.WriteLine(riverClearingIDs[i]);
            }
        }
    }
    
    public void ReadFileWithName(string name)
    {
        string myWoodlandsPath = FileHelper.GetMyWoodlandsFolderPath();
        string filePath = System.IO.Path.Combine(myWoodlandsPath, name + ".root");

        if (File.Exists(filePath))
        {
            ReadFile(File.ReadAllText(filePath));
        }
    }
    
    public void ReadFile(string file)
    {
        worldState.DeleteAllClearings();
        
        string[] splitFile = file.Split("\r\n\r\n");

        if (splitFile.Length == 3)
        {
            string clearingFile = splitFile[0];
            string pathFile = splitFile[1];
            string riverFile = splitFile[2];

            string[] stringClearingLines = clearingFile.Split("\r\n");
            string[] stringPathLines = pathFile.Split("\r\n");
            string[] stringRiverLines = riverFile.Split("\r\n");

            Dictionary<int, int> indexToClearingID = new Dictionary<int, int>(stringClearingLines.Length);

            int clearingCount = Int32.Parse(stringClearingLines[0]);

            Clearing[] clearings = new Clearing[clearingCount];

            for (int i = 0; i < clearings.Length; i++)
            {
                clearings[i] = worldState.GenerateClearing(Vector3.zero);
            }

            for (int i = 1; i < stringClearingLines.Length; i++)
            {
                if (AttributeLine.TryReadAttributeLine(stringClearingLines[i], out AttributeLine attributeLine))
                {
                    attributeLine.ApplyAttributeToClearing(clearings[attributeLine.clearingID]);
                }
            }
            
            for (int i = 0; i < stringPathLines.Length; i++)
            {
                if (PathLine.TryReadAsPathLine(stringPathLines[i], out PathLine pathLine))
                {
                    worldState.GeneratePath(clearings[pathLine.startClearingID], clearings[pathLine.endClearingID]);
                }
            }

            for (int i = 0; i < stringRiverLines.Length; i++)
            {
                if (Int32.TryParse(stringRiverLines[i], out int riverClearingID))
                {
                    worldState.river.AddClearingToRiver(clearings[riverClearingID]);
                }
            }
        }
    }

    //Any Implementation of AttributeLine must implement a static AttributeType variable and a constructor that takes the splitLine.
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
            clearing.SetHasSympathy(faction == FactionType.WoodlandAlliance);
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
}
