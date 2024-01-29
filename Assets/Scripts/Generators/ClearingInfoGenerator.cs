using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class ClearingInfoGenerator
{
    private WorldState worldState;

    public ClearingInfoGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }
    
    public void GenerateDenizens()
    {
        List<Clearing> clearings = worldState.clearings;

        for (int i = 0; i < clearings.Count; i++)
        {
            DenizenType denizen = ChooseRandomDenizen();
            clearings[i].SetMajorDenizen(denizen);
        }
    }

    public void GenerateClearingNames()
    {
        string[] names = System.IO.File.ReadAllLines("Assets/DefaultClearingNames.txt");
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
