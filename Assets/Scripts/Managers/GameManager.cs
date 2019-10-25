using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

/// <summary>
/// Singleton game manager. Loaded by Main Camera (loader.cs)
/// </summary>
public class GameManager : MonoBehaviour
{

    //Instance 
    public static GameManager instance = null; //singleton

    //Game Objects
    public GameObject objSpawn;
    public GameObject[] objEnemies;

    //Game Settings
    public int enemyMax; //Max number of enemies to spawn before level end
    public int enemyMaxActive; //Max number of active enemies on screen
    public int enemyPerSpawn; //How many enemies to spawn at once


    //Member Vars
    private int enemySpawnedCount = 0; //Keep track of how many enemies spawned so far
    private int enemyActiveCount = 0; //Keep track of how many enemies are alive at this time
    public int ActiveEnemyCount
    {
        get { return enemyActiveCount; }
    }

    

    /**
     * Instantiate Game Manager once
     */
    private void Awake()
    {
        ///Singleton Engine
        if( instance == null)
        {
            //Instantiate first run
            instance = this;
            //Do not destroy instance
            DontDestroyOnLoad(gameObject);
        } else if ( instance != this)
        {
            Destroy(gameObject);
            
        }
        

        //Assert requirements
        Assert.IsNotNull(objSpawn, "Spawn point game object");
        Assert.IsTrue(objEnemies.Length > 0, "Enemies object array");

        
        

    }

    private void Start()
    {
        Debug.Log("Started game manager");
    }

    private void Update()
    {
        if (Input.GetKeyDown("1") )
        {
            SceneManager.LoadScene("Menu");
            enemyActiveCount = 0;
            enemySpawnedCount = 0;
        }
        if (Input.GetKeyDown("2"))
        {
            SceneManager.LoadScene("Game");
            enemyActiveCount = 0;
            enemySpawnedCount = 0;
        }
        if( Input.GetKeyDown(KeyCode.A))
        {
            spawnEnemyWave();
        }

    }

    /**
     * Spawn enemies only on "Game" scene
     */
    void spawnEnemyWave()
    {
        if( SceneManager.GetActiveScene().name != "Game")
        {
            return;
        }
        //If spawned enemy count is less than total allowed enemies
        //AND currently active enemy count is less than allowed active enemies on screen
        if( enemySpawnedCount < enemyMax && enemyActiveCount < enemyMaxActive)
        {
            
            for (int i = 0; i < enemyPerSpawn; i++)
            {
                //Check the condition again IN CASE enemy per spawn is > 1
                if( enemySpawnedCount < enemyMax && enemyActiveCount < enemyMaxActive)
                {
                    //Instantiate an enemy prefab from enemies array as a gameobject 
                    GameObject enemy = Instantiate(objEnemies[0]) as GameObject;
                    enemy.transform.position = objSpawn.transform.position; //Move it to spawner

                    
                    
                }
                //Increase active and spawned counters
                enemySpawnedCount++;
                enemyActiveCount++;
            }
        }
    }

    /**
     * An enemy has reached despawn point
     * Decrease active enemy count
     */
    public void enemyReachedExit()
    {
        if( enemyActiveCount > 0)
        {
            enemyActiveCount -= 1;
        }
    }
}
