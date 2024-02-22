using TMPro;
using UnityEngine;

public class FileSaveMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField fileNameInputField;

    private FileGenerator fileGenerator;

    public void Init(FileGenerator fileGen)
    {
        this.fileGenerator = fileGen;
    }

    public void GenerateFileWithName()
    {
        fileGenerator.GenerateFile(fileNameInputField.text);
    }
}
