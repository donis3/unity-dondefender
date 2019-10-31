using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
//using UnityEngine.UI;

/// <summary>
/// Singleton game manager. Loaded at first scene. Can be loaded at other scenes too. Wont be initialized if thers already an instance
/// </summary>
public class GameManager : MonoBehaviour
{
    //Instance Reference
    public static GameManager instance { get; private set; }

    //Time Tracking
    private float startTime = 0f; //internal tracker
    private float runTime = 0f; //Keep track of game runtime

    //Game State
    public gameState State = gameState.menu;

    //level state
    private int level = 0;
    public int Level { get { return level; } }

    [SerializeField]
    [Range(1f, 10f)]
    private float gameSpeed = 1f;
    public float GameSpeed { get { return gameSpeed; } }
    

    //player progress
    private int playerLastPlayedLevel = 0;//the last level player has played
    private int playerWonLevel = 0;//The highest level player has completed

    //Generate an instance if needed. Do not allow more than one
    private void Awake()
    {
        if( instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else
        {
            Destroy(gameObject);
        }

        
    }

    //Input Controller
    private void Update()
    {
        //Time Tracker
        runTime += Time.deltaTime;

        if (Input.GetKeyDown("1") )
        {
            SceneManager.LoadScene("Menu");
            
        }
        if (Input.GetKeyDown("2"))
        {
            SceneManager.LoadScene("Level");
            
        }

        //speed
        if( Time.timeScale != gameSpeed && State != gameState.pause)
        {
            Time.timeScale = gameSpeed;
        }

        

    }

    public float getGameTime()
    {
        return runTime;
    }



    public void QuitLevel()
    {
        SceneManager.LoadScene("Menu");
    }

    public void RestartLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }


}
