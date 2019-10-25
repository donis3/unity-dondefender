using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Enemy Manager Game Object
 * Manages Enemy spawns and despawns
 * Talks to Enemy ( for despawn )
 */
public class EnemyManager : MonoBehaviour
{
    //Game Objects (Bind required prefabs here on unity gui)
    [SerializeField] private GameObject objSpawn;
    [SerializeField] private GameObject[] objEnemies;
    

    [Space(20)]
    //Level Settings
    [Range(0,500)][SerializeField] public int enemyMax; //Max number of enemies to spawn before level end
    [Range(0, 60)][SerializeField] public int enemyMaxActive; //Max number of active enemies on screen
    [Range(0, 20)][SerializeField] public int enemyPerSpawn; //How many enemies to spawn at once

    

    //Internal Vars
    private int enemySpawnedCount = 0; //Keep track of how many enemies spawned so far
    private int enemyActiveCount = 0; //Keep track of how many enemies are alive at this time
    public int ActiveEnemyCount{get { return enemyActiveCount; }} //Get active enemy count public
    public bool gameStarted = false; //toggle for game runtime

    /** Awake : Dependency Control
     * Do assertions for game objects 
     */
    private void Awake()
    {
        Assert.IsNotNull(objSpawn, "Spawn point game object");
        Assert.IsTrue(objEnemies.Length > 0, "Enemies object array");
        
    }

    private void Update()
    {
        if(gameStarted)
        {
            spawnEnemyWave();
        }
    }

    /**
     * SPawn enemies by wave
     */
    void spawnEnemyWave()
    {
        //If spawned enemy count is less than total allowed enemies
        //AND currently active enemy count is less than allowed active enemies on screen
        if (enemySpawnedCount < enemyMax && enemyActiveCount < enemyMaxActive)
        {

            for (int i = 0; i < enemyPerSpawn; i++)
            {
                //Check the condition again IN CASE enemy per spawn is > 1
                if (enemySpawnedCount < enemyMax && enemyActiveCount < enemyMaxActive)
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
     * Call this to despawn an enemy from enemy script
     */
    public void despawnEnemy(GameObject enemy)
    {

        //Despawn this enemy
        Destroy(enemy);
        if (enemyActiveCount > 0) { 
            enemyActiveCount--;
        }
    }

}
