using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using UnityEngine;


public class FileManager
{
    public DirectoryInfo GetMyWoodlandsFolder()
    {
        string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DirectoryInfo myWoodlandsFolder = Directory.CreateDirectory(System.IO.Path.Combine(myDocumentsPath, "MyWoodlands"));
        return myWoodlandsFolder;
    }

    public List<string> GetAllSavedWoodlands()
    {
        DirectoryInfo myWoodlandsFolder = GetMyWoodlandsFolder();
        FileInfo[] woodlandFiles = myWoodlandsFolder.GetFiles();
        
        List<string> savedWoodlands = new List<string>(woodlandFiles.Length);

        for (int i = 0; i < woodlandFiles.Length; i++)
        {
            FileInfo currentFile = woodlandFiles[i];
            string nameWithExtension = currentFile.Name;
            int indexOfLastDot = nameWithExtension.LastIndexOf(".", StringComparison.Ordinal);
            string name = nameWithExtension.Substring(0, indexOfLastDot);
            string extension = nameWithExtension.Substring(indexOfLastDot + 1);

            if (extension == "root")
            {
                savedWoodlands.Add(name);
            }
        }

        return savedWoodlands;
    }
}
