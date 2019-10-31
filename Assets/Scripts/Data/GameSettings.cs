using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum gameState { menu, level, pause};
public static class GameSettings
{
    [Header("% Price increase per level")]
    [SerializeField]
    [Range(0,200)]
    private static int towerPricePercentageIncrementPerLevel = 80;

    [Header("Starting Money")]
    [SerializeField]
    [Range(0, 1000)]
    private static int startingMoney = 20;

    

    [Header("Highest amount of money gainable per action (To stop cheating)")]
    [Space(30)]
    [SerializeField]
    [Range(0, 1000)]
    private static int maxMoneyIncrement = 500;

    //Getters
    public static int TowerPricePercentageIncrementPerLevel { get { return towerPricePercentageIncrementPerLevel; }}
    public static int StartingMoney { get { return startingMoney; } }
    public static int MaxMoneyIncrement { get { return maxMoneyIncrement; } }
}
