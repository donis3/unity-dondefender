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

    [Header("Minimum Money Each Wave")]
    [SerializeField]
    [Range(0, 1000)]
    private static int minMoney = 50;

    [Header("Minimum Money Per Kill")]
    [SerializeField]
    [Range(0, 1000)]
    private static int minDeathMoney = 3;

    [Header("3 Star min score")]
    [SerializeField]
    [Range(0, 100)]
    private static int minScorePerfect = 80;

    [Header("What % of money you get back for selling")]
    [SerializeField]
    [Range(0, 100)]
    private static int towerSellRefundPercentage = 50;



    [Header("Highest amount of money gainable per action (To stop cheating)")]
    [Space(30)]
    [SerializeField]
    [Range(0, 1000)]
    private static int maxMoneyIncrement = 500;

    [Header("Difficulty")]
    [Space(30)]
    [SerializeField]
    [Range(0f, 100f)]
    private static float waveHpIncrementMultiplier = 12f;

    //Getters
    public static int TowerPricePercentageIncrementPerLevel { get { return towerPricePercentageIncrementPerLevel; }}
    public static int StartingMoney { get { return startingMoney; } }
    public static int MaxMoneyIncrement { get { return maxMoneyIncrement; } }
    public static int MinMoney { get { return minMoney; } }
    public static int MinDeathMoney { get { return minDeathMoney; } }
    public static int MinScoreForPerfect { get { return minScorePerfect; } }
    public static int TowerSellRefundPercentage { get { return towerSellRefundPercentage; } }
    public static float WaveHpIncrementMultiplier { get { return waveHpIncrementMultiplier; } }
}
