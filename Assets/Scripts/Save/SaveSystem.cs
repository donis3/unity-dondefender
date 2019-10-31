using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string SaveFile = Application.persistentDataPath + "/playerprogress.rhn";

    public static void SaveGameData(int[] levelStats)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        
        FileStream fh = new FileStream(SaveFile, FileMode.Create);

        SaveData data = new SaveData(levelStats);

        formatter.Serialize(fh, data);
        fh.Close();
    }

    public static int[] LoadGameData()
    {
        if( File.Exists(SaveFile))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fh = new FileStream(SaveFile, FileMode.Open);
            SaveData data = formatter.Deserialize(fh) as SaveData;
            fh.Close();
            return data.levelStats;
        }else
        {
            return null;
        }
    }

    public static void DeleteSaveData()
    {
        if (File.Exists(SaveFile))
        {
            Debug.Log("deleted save");
            File.Delete(SaveFile);
        }
    }
   
}
