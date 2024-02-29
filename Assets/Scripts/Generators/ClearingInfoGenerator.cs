using System;
using System.Collections.Generic;
using Extensions;
using Random = UnityEngine.Random;

public class ClearingInfoGenerator
{
    private WorldState worldState;

    private static string[] defaultNames = new string[]
    {
        "Patchwood",
        "Clutcher's Creek",
        "Rooston",
        "Limberly",
        "Flathome",
        "Opensky Haven",
        "Underleaf",
        "Pinehorn",
        "Milltown",
        "Allaburrow",
        "Tonnery",
        "Icetrap",
        "Ironvein",
        "Sundell",
        "Oakenhold",
        "Blackpaw's Dam",
        "Firehollow",
        "Windgap Refuge",
    };

    public ClearingInfoGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }
    
    //assigns denizens to each clearing ensuring as even a spread as possible
    public void GenerateDenizens()
    {
        List<Clearing> shuffledClearings = new List<Clearing>(worldState.clearings);
        shuffledClearings.Shuffle();

        int baseDenizenCount = shuffledClearings.Count / 3;
        int clearingsToAssign = shuffledClearings.Count % 3;

        for (int i = 0; i < 3; i++)
        {
            DenizenType denizen = (DenizenType)i;
            for (int j = 0; j < baseDenizenCount; j++)
            {
                shuffledClearings[i * baseDenizenCount + j].SetMajorDenizen(denizen);
            }
        }

        switch (clearingsToAssign)
        {
            case 1:
                shuffledClearings[^1].SetMajorDenizen((DenizenType)Random.Range(0, 3));
                break;
            case 2:
                int firstDenizenID = Random.Range(0, 3);
                shuffledClearings[^1].SetMajorDenizen((DenizenType)firstDenizenID);
                bool goUp = Convert.ToBoolean(Random.Range(0, 2));
                int secondDenizenID = (goUp) ? (firstDenizenID + 1) % 3: (firstDenizenID + 2) % 3;
                shuffledClearings[^2].SetMajorDenizen((DenizenType)secondDenizenID);
                break;
        }
    }

    public void GenerateClearingNames()
    {
        string[] names = (string[]) defaultNames.Clone();
        List<Clearing> clearings = worldState.clearings;

        int nameCount = names.Length;

        for (int i = 0; i < clearings.Count; i++)
        {
            int nameIndex = Random.Range(0, nameCount);
            string name = names[nameIndex];
            clearings[i].SetClearingName(name);
            
            (names[nameIndex], names[nameCount - 1]) = (names[nameCount - 1], names[nameIndex]);
            nameCount--;

            if (nameCount == 0)
            {
                nameCount = names.Length;
            }
        }
    }

    private DenizenType ChooseRandomDenizen()
    {
        int denizenID = Random.Range(0, 3);
        return (DenizenType)denizenID;
    }
}
