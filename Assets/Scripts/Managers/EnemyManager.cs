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
    [Range(0.1f, 1f)] [SerializeField] public float levelDifficulty = 0.1f; //How many enemies to spawn at once
    [Tooltip("How many enemy types to include. Do not pass the amount of enemy prefabs you have included. ")]
    [Range(1, 10)] [SerializeField] public int enemyVariety = 3; //How many enemy types to spawn at once
    [Tooltip("Spawn chances below this number will be ignored")]
    [Range(0f,49f)] [SerializeField] private float ignoreBelow = 10f;

    [Header("Wave Timer")]
    [Space(20)]
    [Tooltip("Wait time in seconds between enemy waves")]
    [Range(1f, 600f)]
    [SerializeField] private float enemyWaveDelay = 25f;

    [Tooltip("Wait time in seconds between each enemy during a single wave")]
    [Range(0.1f, 5f)]
    [SerializeField] private float enemySpawnDelay = 0.1f;


    //Internal Vars
    private int enemySpawnedCount = 0; //Keep track of how many enemies spawned so far
    private int enemyActiveCount = 0; //Keep track of how many enemies are alive at this time
    
    private int enemyMax = 0; //Calculate total enemies that will be spawned throughout the level
    private int currentWave = 1; //Will start from 1 to enemyWaveMax

    
    private List<float> enemyDifficulties = new List<float>(); //Keep track of enemy difficulties

    //internal var getters for other scripts
    public int ActiveEnemyCount{get { return enemyActiveCount; }} //Get active enemy count public
    public int CurrentWave { get { return currentWave; } } //Current wave that is being played
    public int TotalWaves { get { return enemyWaveMax; } }
    public int TotalEscaped { get { return enemyEscapedCounter; } }
    public int TotalKilled { get { return enemyKilledCounter; } }
    


    //Game state toggle
    [Header("Game state toggle (Play/Pause)")]
    [Space(20)]
    [Tooltip("Game is paused/active toggle")]
    public bool gameStarted = false; //toggle for game runtime
    private bool waveManagerActive = false;

    private bool levelFinished = false;
    private int enemyKilledCounter = 0;
    private int enemyEscapedCounter = 0;
    private int currentWaveSpawned = 0; // Spawn wavesize-spawned times for this wave when resumed. (Pausing during a wave spawn)
    private float currentWaveDelay = 0f;

    

    /** Awake : Dependency Control
     * Do assertions for game objects 
     */
    private void Awake()
    {
        
        Assert.IsNotNull(objSpawn, "Spawn point game object");
        Assert.IsTrue(objEnemies.Length > 0, "Enemies object array");

        enemyMax = 0;
        //Calculate total enemies this level
        if(enemyWaveMax > 0 && enemyPerSpawn > 0)
        {
            //Apply gauss formula to calculate total number of enemies
            if(enemyWaveMax > 1 && enemyWaveIncrease > 0)
            {
                enemyMax = (enemyWaveMax / 2) * (enemyPerSpawn + (enemyPerSpawn + ((enemyWaveMax-1) * enemyWaveIncrease)));
                
            }else
            {
                enemyMax = (enemyWaveMax * enemyPerSpawn);
            }
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

        //Calculate minimum delay
        if( enemyWaveMax > 1 &&  enemySpawnDelay*(float)CalculateWaveSize(enemyWaveMax-1) > enemyWaveDelay)
        {
            Debug.LogError("Wave timer was not enough to compensate max needed spawn time. Fixin...");
            enemyWaveDelay = (enemySpawnDelay * (float)CalculateWaveSize(enemyWaveMax - 1)) + 1f;
        }

        //Time scale
        Time.timeScale = 1;
        
    }


    private void Start()
    {

        StartCoroutine("WaveManager");
    }
    private void Update()
    {
        GameStateEngine();
        
        
    }

    /**
     * Handle Game runtime
     */
    private void GameStateEngine()
    {
        if( gameStarted )
        {
            Time.timeScale = 1;
        } else
        {
            Time.timeScale = 0;
        }
            

    }



    /** Wave Composition // RETURN int[] (enemyIndex => How many to spawn)
     * Create a wave composition considering
     * current wave no,
     * difficulty
     * and calculate chance to spawn each type
     */
    public int[] createWaveComposition(int waveNo)
    {
        if( enemyDifficulties.Count == 0)
        {
            Debug.LogError("Enemy difficulties are not available. Can't provide wave composition");
            throw new System.Exception("Could not generate enemy wave composition. Enemy difficulties are not available");
        }
        List<KeyValuePair<int, float>> finalizedSpawnChances = new List<KeyValuePair<int, float>>();

        //Calculate wave difficulty (first wave easiest, last wave hardest)(Last wave will have 1 coefficient)
        float waveDifficulty = levelDifficulty * ((float)waveNo / enemyWaveMax);
        //Debug.Log("Level Difficulty: " + levelDifficulty.ToString("n3") + " Current Wave: " + waveNo.ToString() + " Total Waves: " + enemyWaveMax.ToString() + " Wave Difficulty: " + waveDifficulty.ToString("n3") );

        //Divide the wave difficulty by enemy difficulty to find how close it is to the wave. The higer the more chanse to spawn
        float[] enemyDifficultyProbability = new float[enemyDifficulties.Count];
        for(int i = 0; i < enemyDifficulties.Count; i++)
        {
            enemyDifficultyProbability[i] = enemyDifficulties[i] / waveDifficulty;
            //Debug.Log("Enemy " + i.ToString() + " division: " + enemyDifficultyProbability[i].ToString("n2"));
        }
        //Find total probabilities
        float totalProb = enemyDifficultyProbability.Sum();

        int totalIgnored = 0; // keep track of ignored enemies. If total reaches max; dont let the last enemy to be ignored too.
        //Convert the probabilities to percentage
        for (int i = 0; i < enemyDifficultyProbability.Length; i++)
        {
            enemyDifficultyProbability[i] = (enemyDifficultyProbability[i] / totalProb) * 100f;

            //Ignore if below tolerance
            if( enemyDifficultyProbability[i] >= ignoreBelow )
            {
                
                //Debug.Log("+Enemy " + i + " Spawn Chanse: " + enemyDifficultyProbability[i].ToString("n3"));
                finalizedSpawnChances.Add(new KeyValuePair<int, float>(i, enemyDifficultyProbability[i]));//Add the enemy index & spawn chanse
            } else
            {
                totalIgnored++;
                if( totalIgnored == enemyDifficultyProbability.Length)
                {
                    //Max ignore reached
                    //Debug.Log("Max enemy ignore is reached. Keeping the last enemy in the spawn table.");
                    finalizedSpawnChances.Add(new KeyValuePair<int, float>(i, enemyDifficultyProbability[i]));//Add the enemy index & spawn chanse
                }
                //Debug.Log("Enemy " + i + " Spawn Chanse: " + enemyDifficultyProbability[i].ToString("n3") + " -- IGNORED");
            }

        }

        finalizedSpawnChances = Rhinotap.Tools.SortPair(finalizedSpawnChances, false); //Descending order. Big to small
        
        //Apply enemy variety if we have more than 1 enemy and enemy count is more than variety
        if (finalizedSpawnChances.Count >  enemyVariety )
        {
            //Debug.Log("===================Total chanses: " + finalizedSpawnChances.Count.ToString() + " But variety is limited to " + enemyVariety.ToString());
            for( int i = 0; i < finalizedSpawnChances.Count; i++)
            {
                //Remove elements after variety limit is reached. If chanses list is 3 of size. Variety : 2, remove the last element
                if( i >= enemyVariety)
                {
                    //Debug.Log("Enemy Variety is reached, Removing Enemy Index of " + finalizedSpawnChances[i].Key.ToString());
                    finalizedSpawnChances.RemoveAt(i);
                }
            }
        }

        //Generate array for return
        int[] result = new int[objEnemies.Length];

        //Calculate wave size
        int waveSize = enemyPerSpawn + ( (waveNo-1) * enemyWaveIncrease); //at wave 0, no increase
        if( waveSize <= 0)
        {
            throw new System.Exception("========Could not calculate wave size. Its either 0 or negative");
        }
        //Create Percentages
        int[] percentages = new int[objEnemies.Length];

        for(int i = 0; i < finalizedSpawnChances.Count; i++)
        {
            if(finalizedSpawnChances[i].Key < objEnemies.Length)
            percentages[finalizedSpawnChances[i].Key] = (int)Mathf.Round(finalizedSpawnChances[i].Value);
        }

        Rhinotap.Choose chooseEnemy = new Rhinotap.Choose(percentages);

        //Generate Results
        for(int i = 0; i < waveSize; i++)
        {
            int enemyIndex = chooseEnemy.NextValue();
            result[enemyIndex] += 1;
        }

        return result;
    }


    IEnumerator WaveManager()
    {
        
        while(true )
        {
            //Paused State
            if (!gameStarted)
            {
                yield return new WaitUntil(() => gameStarted == true);
            }

            //Wave Delay 
            yield return new WaitForSeconds(enemyWaveDelay);

            int currentWaveSize = enemyPerSpawn + ((currentWave - 1) * enemyWaveIncrease); //Enemy number to spawn this wave
            int availableActiveEnemySlots = enemyMaxActive - enemyActiveCount;

            if (enemySpawnDelay < 0.1f) { enemySpawnDelay = 0.1f; } //Min spawn delay

            //If there are enough slots
            if (currentWaveSize <= availableActiveEnemySlots)
            {
                //SPAWN ENEMY WAVE
                /* ====================================================== */

                Debug.Log("====Spawning wave: " + currentWave.ToString());

                //Get wave composition
                int[] waveComposition = createWaveComposition(currentWave);
                if (waveComposition.Length == 0) { yield break; } //Exit if no enemy


                int waveSpawnCount = 0;
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
                            waveSpawnCount++;
                            yield return new WaitForSeconds(enemySpawnDelay); //Wait for given delay

                        }//Eol enemy spawn amount
                    }
                }//Eol enemy types


                //Wave Spawner complete
                if (waveSpawnCount == CalculateWaveSize(currentWave))
                {
                    Debug.Log("Wave " + currentWave.ToString() + " has spawned completely (" + CalculateWaveSize(currentWave).ToString() + " enemies)");
                    currentWave++;
                }
                else
                {
                    Debug.LogError("Wave " + currentWave.ToString() + " has NOT spawned completely (" + waveSpawnCount.ToString() + " / " + CalculateWaveSize(currentWave).ToString() + " enemies)");
                }

                /* ====================================================== */

            }
            else
            {
                Debug.Log("@WaveManager Scene was filled, waiting for next iteration");
            }

        } //EOL while coroutine true
    }






    /**
     * Calculate enemy size of a wave
     */
    private int CalculateWaveSize(int waveNo)
    {
        int result = enemyPerSpawn + ((currentWave - 1) * enemyWaveIncrease);
        return result;
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

        enemy.transform.position = objSpawn.transform.position; //Move it to spawner

        //Increment counters
        enemyActiveCount++;
        enemySpawnedCount++;
    }


    /**
     * Remove a single enemy object from the scene
     * Called when enemy escapes
     */
    public void despawnEnemy(GameObject enemy)
    {

        //Despawn this enemy
        Destroy(enemy);
        if (enemyActiveCount > 0)
        {
            enemyActiveCount--;
        }
        enemyEscapedCounter++;
    }
}
