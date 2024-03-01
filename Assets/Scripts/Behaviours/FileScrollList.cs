using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FileScrollList : MonoBehaviour
{
    [SerializeField] private GameObject fileSelectButtonObject;
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private SaveLoadPanel saveLoadPanel;

    private FileGenerator fileGenerator;

    private List<FileSelectButton> buttons = new List<FileSelectButton>();
    private float buttonSize = 30.0f;
    private float listMaxSize = 180.0f;

    private float currentListSize;
    private float scrollSize;

    public void Init(FileGenerator fileGen)
    {
        this.fileGenerator = fileGen;
    }
    
    public void StartScrollList(List<string> fileNames)
    {
        DeleteButtons();
        
        for (int i = 0; i < fileNames.Count; i++)
        {
            AddButton(fileNames[i]);
        }
        
        OnScrollBarUsed();

        currentListSize = buttons.Count * buttonSize;
        scrollSize = Math.Max(0, currentListSize - listMaxSize);
    }

    public void EndScrollList()
    {
        DeleteButtons();
        saveLoadPanel.EnterEscapeMenuState();
    }

    public void SelectFile(string fileName)
    {
        fileGenerator.ReadFileWithName(fileName);
        EndScrollList();
    }

    public void AddButton(string fileName)
    {
        GameObject fileButtonObject = Instantiate(fileSelectButtonObject, Vector3.zero, Quaternion.identity, transform);
        fileButtonObject.transform.localPosition = Vector3.zero;
        FileSelectButton fileSelectButton = fileButtonObject.GetComponent<FileSelectButton>();
        fileSelectButton.Init(this, fileName);
        buttons.Add(fileSelectButton);
    }

    public void DeleteButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i].gameObject);
        }
        
        buttons.Clear();
    }

    public void OnScrollBarUsed()
    {
        float scrollValue = scrollBar.value;
        float listScrollAmount = scrollValue * scrollSize;

        int startIndex =(int)(listScrollAmount / buttonSize);
        int endIndex = startIndex + (int)(listMaxSize / buttonSize) - 1;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i >= startIndex && i <= endIndex)
            {
                Vector3 defaultPosition = i * buttonSize * Vector3.down;
                Vector3 buttonPosition = defaultPosition + listScrollAmount * Vector3.up;
                buttons[i].gameObject.SetActive(true);
                buttons[i].transform.localPosition = buttonPosition;
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }
}
