using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

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


    //Scene count
    private int TotalLevels = 0;
    
    public int LevelCount
    {
        get { GetSceneCount(); return TotalLevels; }
    }

    //player progress
    private int playerLastPlayedLevel = 0;//the last level player has played
    private int playerWonLevel = 0;//The highest level player has completed

    private int[] levelStats;
    public int[] LevelStats { get { return levelStats; } }

    private int highestLevelAvailable = 0;
    public int HighestLevelAvailable { get { return highestLevelAvailable; } }


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

        if( State == gameState.menu && LevelSelect.instance.levelsLoaded == false)
        {
            LevelSelect.instance.SpawnButtons();
        }
        
        

    }

    private void Start()
    {
        
        GetSceneCount();
        LoadPlayerProgress();
        

        
    }

    //Save a level progress in levelStats. hint: level is normal not array index. Level 1 : 1
    public void SaveLevelProgress(int level, int star)
    {
        if(level <= 0 || star <= 0)
        {
            return;
        }
        if( level > TotalLevels) {  return; }
        if( star > 3) { star = 3; }

        if(levelStats.Length < level-1)
        {
            //index out of reach
            Debug.LogError("Couldn't save player progress. level number is out of reach");
            return;
        }
        //Check if this score is better
        int oldScore = levelStats[(level - 1)];
        if( star > oldScore)
        {
            levelStats[(level - 1)] = star; //level index is 1 less
            //Save file
            SavePlayerProgress();
        }else
        {
            //you did a better score before
        }
        if( level > highestLevelAvailable)
        {
            highestLevelAvailable = level;
        }
        
        return;
    }

    //Get 0-3 integer for a levels progress
    public int getLevelProgress(int level)
    {
        if( level <= levelStats.Length && level > 0)
        {
            return levelStats[(level-1)];
        }
        return 0;
    }

    public void LoadPlayerProgress()
    {
        int[] loadedLevelStats = SaveSystem.LoadGameData();
        levelStats = new int[LevelCount];

        if (loadedLevelStats != null)
        {
            if(loadedLevelStats.Length > 0)
            {
                for(int i = 0; i < levelStats.Length; i++)
                {
                    //Does the loaded stats have this index?
                    if( loadedLevelStats.Length > i)
                    {
                        levelStats[i] = loadedLevelStats[i];

                        //is level passed. If so, make the next one available
                        if( LevelStats[i] > 0)
                        {
                            //Level has at least 1 star
                            highestLevelAvailable = i+1;
                        }
                    }
                }
            }
            
        }
    }

    public void SavePlayerProgress()
    {
        SaveSystem.SaveGameData(levelStats);
    }

    



    public float getGameTime()
    {
        return runTime;
    }



    public void QuitLevel()
    {
        SceneManager.LoadScene("Menu");

        State = gameState.menu;
        
    }

    public void RestartLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void LoadNextLevel(int currentLevel)
    {
        if( currentLevel >= TotalLevels)
        {
            //Game Complete
            SceneManager.LoadScene("Menu");
            level = 0;
        }else if( currentLevel < TotalLevels)
        {
            level = currentLevel + 1;
            string newSceneName = "Level " + level.ToString();
            SceneManager.LoadScene(newSceneName);
        }else
        {
            //Some error
            Debug.LogError("Scene manager error. Loading menu");
            SceneManager.LoadScene("Menu");
            level = 0;
        }
    }

    public void ToggleSpeed2x()
    {
        if(gameSpeed == 1f)
        {
            gameSpeed = 2f;
        } else if( gameSpeed == 2f)
        {
            gameSpeed = 1f;
        } else
        {
            gameSpeed = 1f;
        }
    }


    public void LoadLevel(int levelNo)
    {
        if(levelNo < TotalLevels)
        {
            string levelName = "Level " + (levelNo + 1).ToString();
            SceneManager.LoadScene(levelName);
        }
    }

    public void GetSceneCount()
    {
        TotalLevels = SceneManager.sceneCountInBuildSettings;
        if( TotalLevels > 1)
        {
            TotalLevels -= 1; //Remove menu
        }
    }

}
