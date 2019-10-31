using System;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public  class SaveData
{
    public int[] levelStats;
    public  SaveData(int[] LevelStats)
    {
        levelStats = LevelStats;
    }
}
