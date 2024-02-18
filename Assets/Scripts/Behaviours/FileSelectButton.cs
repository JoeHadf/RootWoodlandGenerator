using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FileSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    private FileScrollList fileScrollList;
    private string fileName;

    public void Init(FileScrollList scrollList, string file)
    {
        this.fileScrollList = scrollList;
        this.fileName = file;
        buttonText.text = this.fileName;
    }

    public void SelectThisFile()
    {
        fileScrollList.SelectFile(fileName);
    }
}
