using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveLoadPanel : MonoBehaviour
{
    [SerializeField] private GameObject fileScrollListObject;
    [SerializeField] private FileScrollList fileScrollList;

    [SerializeField] private GameObject fileSaveMenuObject;

    private WorldState worldState;
    public bool loadingWoodland { get; private set; }
    public bool savingWoodland { get; private set; }

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
        
        loadingWoodland = false;
        savingWoodland = false;
    }
    

    public bool IsDoingAction()
    {
        return loadingWoodland || savingWoodland;
    }

    public void OpenScrollList()
    {
        if (worldState.editMode != EditMode.River)
        {
            fileScrollListObject.SetActive(true);
            List<string> savedWoodlands = FileHelper.GetAllSavedWoodlands();
            fileScrollList.StartScrollList(savedWoodlands);

            loadingWoodland = true;
        }
    }

    public void CloseScrollList()
    {
        fileScrollListObject.SetActive(false);
        loadingWoodland = false;
    }

    public void OpenSaveMenu()
    {
        if (worldState.editMode != EditMode.River)
        {
            fileSaveMenuObject.SetActive(true);
            savingWoodland = true;
        }
    }
    
    public void CloseSaveMenu()
    {
        fileSaveMenuObject.SetActive(false);
        savingWoodland = false;
    }
}
