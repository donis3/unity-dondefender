using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

/**
 * Enemy Manager Game Object
 * Manages Enemy spawns and despawns
 * Talks to Enemy ( for despawn )
 */
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance { get; private set; } //Instantiate once per level

    //Game Objects (Bind required prefabs here on unity gui)
    [Header("Spawn Point Prefab")]
    [SerializeField] private GameObject objSpawn;
   
    [Header("Enemy Prefabs")]
    [Tooltip("Enemy prefabs. Lower index values will be considered easer. First enemy will be considered easiest")]
    [SerializeField] private GameObject[] objEnemies;


    [Header("Level Settings")]
    [Space(20)]
    //Level Settings
    [Tooltip("Total number of waves for the level")]
    [Range(1,50)][SerializeField] public int enemyWaveMax = 5; //Total wave number
    [Tooltip("Max number of enemies active on the map at once")]
    [Range(0, 60)][SerializeField] public int enemyMaxActive = 100; //Max number of active enemies on screen
    [Tooltip("Initial number of enemies per wave")]
    [Range(0, 20)][SerializeField] public int enemyPerSpawn = 5; //How many enemies to spawn at once
    [Tooltip("Enemy number to increment per wave. Each waves enemy count will be increased by this much compared to the previous wave")]
    [Range(0, 20)] [SerializeField] public int enemyWaveIncrease = 1; //How many enemies to spawn at once

    
    [Header("Difficulty Settings")]
    [Space(20)]
    [Tooltip("The harder the level, the stronger enemies will be choosen per level")]
    [Range(1f, 50f)] [SerializeField] public float levelDifficulty = 0.1f; //Level difficulty
    [Tooltip("How many enemy types to include. Do not pass the amount of enemy prefabs you have included. ")]
    [Range(1, 10)] [SerializeField] public int enemyVariety = 3; //How many enemy types to spawn at once
    [Tooltip("Spawn chances below this number will be ignored")]
    [Range(0f,49f)] [SerializeField] private float ignoreBelow = 10f;

    [Tooltip("Allowed enemy escapes before gameover")]
    [Range(0, 1000)] [SerializeField] private int enemyEscapeAllowed = 20; //game over if 20+ escapes

    [Header("Wave Timer")]
    [Space(20)]
    [Tooltip("Wait time in seconds between enemy waves")]
    [Range(1f, 600f)]
    [SerializeField] private float enemyWaveDelay = 25f;

    [Tooltip("Wait time in seconds between each enemy during a single wave")]
    [Range(0.1f, 5f)]
    [SerializeField] private float enemySpawnDelay = 0.05f;


    //Internal Vars
    private int enemySpawnedCount = 0; //Keep track of how many enemies spawned so far
    private int enemyActiveCount = 0; //Keep track of how many enemies are alive at this time
    
    private int enemyMax = 0; //Calculate total enemies that will be spawned throughout the level
    private int currentWave = 0; //Will start from 1 to enemyWaveMax
    

    //Active enemies list
    private List<Enemy> enemies = new List<Enemy>();

    private List<Enemy> deadEnemies = new List<Enemy>();//Keep dead enemies for a while
    public List<Enemy> Enemies
    {
        get { return enemies; }
    }

    
    private List<float> enemyDifficulties = new List<float>(); //Keep track of enemy difficulties

    //internal var getters for other scripts
    public int ActiveEnemyCount{get { return enemyActiveCount; }} //Get active enemy count public
    public int CurrentWave { get { return currentWave; } } //Current wave that is being played
    public int TotalWaves { get { return enemyWaveMax; } }
    public int TotalEscaped { get { return enemyEscapedCounter; } }
    public int TotalKilled { get { return enemyKilledCounter; } }
    public int EnemyEscapeAllowed { get { return enemyEscapeAllowed; } }



    private bool lastWaveSpawned = false;
    
    private int enemyKilledCounter = 0;
    private int enemyEscapedCounter = 0;

    private bool waveManagerRunning = false;
    private int lastCompletelySpawnedWave = -1;
    public bool WaveManagerRunning { get { return waveManagerRunning; } }
    public int LastCompletelySpawnedWave { get { return lastCompletelySpawnedWave; } }

    private bool gameLost = false;
    public bool GameLost { get { return gameLost; } }


    private int bonusHp = 0;

    /** Awake : Dependency Control
     * Do assertions for game objects 
     */
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Assert.IsNotNull(objSpawn, "Spawn point game object");
        Assert.IsTrue(objEnemies.Length > 0, "Enemies object array");

        enemyMax = 0;
        //Calculate total enemies this level
        if(enemyWaveMax > 0 && enemyPerSpawn > 0)
        {
            enemyMax = CalculateTotalSpawnSize();
        }
        
        

        //Get enemy difficulties and store them in list
        if( objEnemies.Length > 0)
        {
            for(int i = 0; i < objEnemies.Length; i++)
            {
                float diff = objEnemies[i].GetComponent<Enemy>().difficulty;
                enemyDifficulties.Add(diff);
            }
        }


        //CALCULATE level vars and fix inconsistencies
        int maxWaveSize = enemyPerSpawn + ((enemyWaveMax - 1) * enemyWaveMax); //Bigges wave size
        if( enemyMaxActive < enemyWaveMax)
        {
            Debug.LogError("Max enemy wave size is bigger than max active enemy size! Fixing..");
            enemyMaxActive = maxWaveSize;
        }
        
        //Evaluate delays
        if(enemySpawnDelay <= 0.1f)
        {
            enemySpawnDelay = 0.1f;
        }


        //min spawn delay
        if (enemySpawnDelay < 0.1f) { enemySpawnDelay = 0.1f; } //Min spawn delay

        //Change game state


    }


    private void Start()
    {
        EnemyHealthCalculatorForLevel();

        StartCoroutine("WaveManager");





    }
    private void Update()
    {
        
        
        
    }

    //Bonus hp for enemies. Increased with level and difficulty. Each wave, huge hp increase
    private void EnemyHealthCalculatorForLevel()
    {
        
         bonusHp = (int)Math.Round((float)LevelManager.instance.Level * Rhinotap.Tools.RandomFloat(0.1f, 0.5f) * levelDifficulty);

        //include wave
        float waveDifficulty = CalculateWaveDifficulty(currentWave);
        if( waveDifficulty <= 0f) { waveDifficulty = 0.1f; }

        bonusHp += (int)Math.Round(waveDifficulty * GameSettings.WaveHpIncrementMultiplier);
        
        //Multiply by level if level is 1, do *1, if level is 10, do 1.05 if 
        float levelMultiplier = (float)(LevelManager.instance.Level+10) / 20f; //100 -> 5 10-> 0.5
        bonusHp = (int)Math.Round((float)bonusHp * (1f+levelMultiplier));

        
        
    }

    /** Wave Composition // RETURN int[] (enemyIndex => How many to spawn)
     * Create a wave composition considering
     * current wave no,
     * difficulty
     * and calculate chance to spawn each type
     * 
     * Math:
     * LevelDifficulty - enemy difficulty = DeltaDif
     * the smaller the delta, the higher we want the spawn chance. To convert it:
     * 1000/deltaDif -> smaller numbers will become much higer
     * SPECIAL CASE: if deltaDif = 0 -> MathError. convert to 0.1
     */
    public int[] createWaveComposition(int waveNo)
    {
        if( enemyDifficulties.Count == 0)
        {
            Debug.LogError("Enemy difficulties are not available. Can't provide wave composition");
            throw new System.Exception("Could not generate enemy wave composition. Enemy difficulties are not available");
        }

        //Generate array for return
        int[] result = new int[objEnemies.Length];

        //Calculate wave size
        int waveSize = CalculateWaveSize(waveNo);

        //Calculate wave difficulty
        float waveDifficulty = levelDifficulty * ((float)waveNo / enemyWaveMax);

        //Generate spawn chances
        
        Rhinotap.SpawnChance newChance = new Rhinotap.SpawnChance(waveDifficulty, enemyDifficulties.ToArray(), ignoreBelow);
        newChance.Variety = enemyVariety;
        int[] spawnChances = newChance.Result();

        newChance.Report(spawnChances);
        
        

        //Create Percentages
        Rhinotap.Choose chooseEnemy = new Rhinotap.Choose(spawnChances);

        //Generate Results
        for(int i = 0; i < waveSize; i++)
        {
            int enemyIndex = chooseEnemy.NextValue(); //Choose an enemy based on spawn chance
            result[enemyIndex] += 1; //Increment this enemy type by 1 (how many times it will spawn)
        }

        return result;
    }


    IEnumerator WaveManager()
    {
        
        while(lastWaveSpawned == false && gameLost == false)
        {
            waveManagerRunning = true;

            //Paused State
            if (LevelManager.instance.GamePaused == true)
            {
                yield return new WaitUntil(() => LevelManager.instance.GamePaused == false);
            }

            //Waves Paused
            if(LevelManager.instance.WavesPaused == true)
            {
                yield return new WaitUntil(() => LevelManager.instance.WavesPaused == false);
            }

            //End Game
            if (currentWave > enemyWaveMax)
            {
                lastWaveSpawned = true;
                waveManagerRunning = false;
                LevelManager.instance.Feedback("Last wave have spawned");
                yield break;
            }

            //Remove corpses before new wave
            if(deadEnemies.Count > 0 )
            {
                for(int i = 0; i < deadEnemies.Count; i++)
                {
                    Destroy(deadEnemies[i].gameObject);
                }
                deadEnemies.Clear();
            }

            //Wave Delay 
            if (lastCompletelySpawnedWave < 1)
            {
                LevelManager.instance.Feedback("First wave is coming in " + Math.Round((double)enemyWaveDelay) + " seconds..", 1f);
            }
            yield return new WaitForSeconds(enemyWaveDelay);

            int currentWaveSize = CalculateWaveSize(currentWave);
            int availableActiveEnemySlots = enemyMaxActive - enemyActiveCount;

            

            //If there are enough slots
            if (currentWaveSize <= availableActiveEnemySlots)
            {
                //SPAWN ENEMY WAVE
                /* ====================================================== */

                //Debug.Log("====Spawning wave: " + currentWave.ToString());

                //Get wave composition
                int[] waveComposition = createWaveComposition(currentWave);
                if (waveComposition.Length == 0) {
                    //Could not calculate wave composition
                    Debug.LogError("Wave no " + currentWave + " did not yield any spawns. Exiting Coroutine");
                    waveManagerRunning = false;
                    yield break;
                }


                
                //i: enemy j: spawn this times
                for (int i = 0; i < waveComposition.Length; i++)
                {
                    //Spawn each enemy x times
                    if (waveComposition[i] > 0)
                    {
                        //Spawn the enemy at the i index, x times
                        for (int j = 0; j < waveComposition[i]; j++)
                        {
                            
                            spawnEnemy(i);
                            
                            yield return new WaitForSeconds(enemySpawnDelay + Rhinotap.Tools.RandomFloat(0.1f,0.4f)); //Wait for given delay

                        }//Eol enemy spawn amount
                    }
                }//Eol enemy types


                

                //Pause waves if we still havent finished
                LevelManager.instance.PauseWaves(); //Pause waves and wait for next wave

                

                /* ====================================================== */

            }
            else
            {
                Debug.Log("@WaveManager Scene was filled, waiting for next iteration");
            }

            waveManagerRunning = false;
            lastCompletelySpawnedWave = currentWave;
        } //EOL while coroutine true
    }

    private float CalculateWaveDifficulty(int waveNo)
    {
        float waveDifficulty = levelDifficulty * ((float)waveNo / enemyWaveMax);
        return waveDifficulty;
    }


    public void IncrementWave()
    {
        EnemyHealthCalculatorForLevel();
        currentWave++;
    }

    /**
     * Calculate enemy size of a wave
     */
    public int CalculateWaveSize(int waveNo)
    {
        int result = enemyPerSpawn + ((waveNo - 1) * enemyWaveIncrease);
        return result;
    }

    private int CalculateTotalSpawnSize()
    {
        int total = 0;
        for(int i = 1; i <= enemyWaveMax; i++)
        {
            total += CalculateWaveSize(i);
        }
        return total;
    }

    /**
     * Sawn a single enemy of the given index
     */
    void spawnEnemy(int enemyIndex)
    {
        if (objEnemies.Length == 0 || objEnemies.Length <= enemyIndex)
        {
            throw new System.Exception("Enemies Obj failed to serve " + "\n ObjSize: " + objEnemies.Length.ToString() + " Request: " + enemyIndex.ToString());
        }

        //Instantiate an enemy prefab from enemies array as a gameobject 
        GameObject enemy = Instantiate(objEnemies[enemyIndex]) as GameObject;
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        
        
        enemyComponent.health += bonusHp;


        Enemies.Add(enemyComponent);

        enemy.transform.position = objSpawn.transform.position; //Move it to spawner

        //Increment counters
        enemyActiveCount++;
        enemySpawnedCount++;
    }


    //Level Finished
    public void GameEnded()
    {
        //Debug.Log("You won");
        
    }

    public void Lose()
    {
        
        StopCoroutine("WaveManager");
        gameLost = true;
    }

    private int lastDeadEnemySort = -20;
    public void killEnemy(GameObject enemy)
    {
        deadEnemies.Add(enemy.GetComponent<Enemy>());
        Enemies.Remove(enemy.GetComponent<Enemy>());

        //Gain money
        enemy.GetComponent<Enemy>().rewardMoney();

        //Dead enemy sorting order between -20 and -1. Oldear dead ones will be further behind
        lastDeadEnemySort++;
        if( lastDeadEnemySort >= -1)
        {
            lastDeadEnemySort = -20;
        }
        enemy.GetComponent<SpriteRenderer>().sortingOrder = lastDeadEnemySort;

        if (enemyActiveCount > 0)
        {
            enemyActiveCount--;
            enemyKilledCounter++;
        }
        //Get location
        Vector2 originalPos = enemy.transform.localPosition;
        enemy.transform.localPosition = new Vector2(
            originalPos.x + Rhinotap.Tools.RandomFloat(0.01f, 0.1f),
            originalPos.y + Rhinotap.Tools.RandomFloat(0.01f, 0.1f)
            );
        
    }

    /**
     * Remove a single enemy object from the scene
     * Called when enemy escapes
     */
    public void despawnEnemy(GameObject enemy)
    {
        Enemies.Remove(enemy.GetComponent<Enemy>());

        //Despawn this enemy
        Destroy(enemy);
        if (enemyActiveCount > 0)
        {
            enemyActiveCount--;
        }

        enemyEscapedCounter++;
        if ( TotalEscaped >= enemyEscapeAllowed)
        {
            Lose();
            return;
        }
        
    }
}
