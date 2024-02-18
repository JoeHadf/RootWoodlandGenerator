using System;
using System.Collections.Generic;
using System.IO;

public static class FileHelper
{
    public static string GetMyWoodlandsFolderPath()
    {
        string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string myWoodlandsPath = System.IO.Path.Combine(myDocumentsPath, "MyWoodlands");
        return myWoodlandsPath;
    }
    public static DirectoryInfo GetMyWoodlandsFolder()
    {
        DirectoryInfo myWoodlandsFolder = Directory.CreateDirectory(GetMyWoodlandsFolderPath());
        return myWoodlandsFolder;
    }

    public static List<string> GetAllSavedWoodlands()
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
