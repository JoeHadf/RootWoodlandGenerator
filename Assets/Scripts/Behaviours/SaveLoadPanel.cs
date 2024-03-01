using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveLoadPanel : MonoBehaviour
{
    [SerializeField] private GameObject escapeMenuObject;
    
    [SerializeField] private GameObject fileScrollListObject;
    [SerializeField] private FileScrollList fileScrollList;

    [SerializeField] private GameObject fileSaveMenuObject;

    private WorldState worldState;

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;

        this.worldState.OnEnterLoadMenuState += OpenLoadMenu;
        this.worldState.OnExitLoadMenuState += CloseLoadMenu;

        this.worldState.OnEnterSaveMenuState += OpenSaveMenu;
        this.worldState.OnExitSaveMenuState += CloseSaveMenu;

        this.worldState.OnEnterEscapeMenuState += OpenEscapeMenu;
        this.worldState.OnExitEscapeMenuState += CloseEscapeMenu;
    }

    public void EnterLoadMenuState()
    {
        worldState.TryEnterMenuState(MenuState.Load);
    }

    public void EnterEscapeMenuState()
    {
        worldState.TryEnterMenuState(MenuState.Escape);
    }

    public void EnterSaveMenuState()
    {
        worldState.TryEnterMenuState(MenuState.Save);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OpenLoadMenu()
    {
        fileScrollListObject.SetActive(true);
        List<string> savedWoodlands = FileHelper.GetAllSavedWoodlands();
        fileScrollList.StartScrollList(savedWoodlands);
    }

    private void CloseLoadMenu()
    {
        fileScrollListObject.SetActive(false);
    }

    private void OpenSaveMenu()
    {
        fileSaveMenuObject.SetActive(true);
    }
    
    private void CloseSaveMenu()
    {
        fileSaveMenuObject.SetActive(false);
    }

    public void OpenEscapeMenu()
    {
        escapeMenuObject.SetActive(true);
    }

    public void CloseEscapeMenu()
    {
        escapeMenuObject.SetActive(false);
    }
}
