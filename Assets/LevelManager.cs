using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance { get; private set; }//Instantiate once per level

    /*=============| Current Active Level (Scene)|================*/
    //Current Level
    [Header("Level Identifier")]
    [SerializeField]
    [Range(0,1000)]
    private int level = 0;
    //Getters
    public int LevelDisplay { get { return level + 1; } }//Level number starts from 1. 0->1 (For GUI)
    public int Level { get { return level; } }

    private bool wavesPaused = true;
    private bool gamePaused = false;
    public bool WavesPaused { get { return wavesPaused; } }
    public bool GamePaused { get { return gamePaused; } }


    /*=============| Player Data For This Level |================*/
    //Player current money amount
    private int money = 10;
    private int previousMoney = 0;//Anti Cheat
    public int Money { get { return money; } }



    /*==============|Data Trackers|=============*/


    private int shownWaveEndScreenFor = -1;


    /*============|UI OBJECTS|==============*/
    private GameObject playToggle;
    private GameObject uiCurrentMoney;
    private GameObject uiCurrentWave;
    private GameObject uiTotalWave;
    private GameObject uiCurrentEscaped;
    private GameObject uiMaxEscaped;
    private GameObject uiCurrentLevel;
    private GameObject btnWaveStart;
    private GameObject textWaveStart;
    private CanvasGroup waveComplete;
    private GameObject pauseMenu;
    private GameObject pauseMenuResumeBtn;
    private GameObject pauseMenuRestartBtn;
    private GameObject pauseMenuQuitBtn;
    private GameObject feedbackSystem;
    private Text feedback;


    //Sprites
    public Sprite btnPlayNormal;
    public Sprite btnPlayPaused;


    private void Awake()
    {
        //Instantiate singleton(for level)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Change state to level
        GameManager.instance.State = gameState.level;

        //Load required data & Objects
        level = GameManager.instance.Level;
        playToggle = GameObject.Find("btnPlayToggle");

        uiCurrentMoney   = GameObject.Find("uiCurrentMoney");
        uiCurrentWave    = GameObject.Find("uiCurrentWave");
        uiTotalWave      = GameObject.Find("uiTotalWave");
        uiCurrentEscaped = GameObject.Find("uiCurrentEscaped");
        uiMaxEscaped     = GameObject.Find("uiMaxEscaped");
        uiCurrentLevel   = GameObject.Find("uiCurrentLevel");
        btnWaveStart     = GameObject.Find("btnWaveStart");
        textWaveStart    = GameObject.Find("textWaveStart");
        feedback         = GameObject.Find("feedback").GetComponent<Text>();
        feedbackSystem = GameObject.Find("feedbackSystem");

        //Pause menu items
        pauseMenu        = GameObject.Find("PauseMenu");
        pauseMenuQuitBtn = GameObject.Find("btnQuit");
        pauseMenuRestartBtn = GameObject.Find("btnRestart");
        pauseMenuResumeBtn = GameObject.Find("btnResume");
        pauseMenu.SetActive(false);

        //End of wave splash
        waveComplete     = GameObject.Find("waveComplete").GetComponent<CanvasGroup>();
        waveComplete.alpha = 0f;//initial alpha

        //Event Listeners
        Button btnPlayToggle = playToggle.GetComponent<Button>();
        btnPlayToggle.onClick.AddListener(PlayPause);

        Button btnWavePlay = btnWaveStart.GetComponent<Button>();
        btnWavePlay.onClick.AddListener(SendNextWave);

        Button btnResume = pauseMenuResumeBtn.GetComponent<Button>();
        btnResume.onClick.AddListener(PlayPause);

        Button btnRestart = pauseMenuRestartBtn.GetComponent<Button>();
        btnRestart.onClick.AddListener(GameManager.instance.RestartLevel);

        Button btnQuit = pauseMenuQuitBtn.GetComponent<Button>();
        btnQuit.onClick.AddListener(GameManager.instance.QuitLevel);

        //Print current level
        uiCurrentLevel.GetComponent<Text>().text = (level+1).ToString();

        //print total waves
        uiTotalWave.GetComponent<Text>().text = EnemyManager.instance.TotalWaves.ToString();

        //Print allowed escape
        uiMaxEscaped.GetComponent<Text>().text = EnemyManager.instance.EnemyEscapeAllowed.ToString();
        uiCurrentEscaped.GetComponent<Text>().text = "0";


        StartCoroutine("UpdateUiElements");
        StartCoroutine("ShowFeedback");

        Feedback("Welcome");
        Feedback("HAHAHA");
        Feedback("Shut up");
    }


    /*|==========================| Wave Management ===========================| */
    public void PauseWaves()
    {
        wavesPaused = true;        
    }
    private void SetWaveBtnText()
    {
        if (EnemyManager.instance.LastCompletelySpawnedWave < 1)
        {
            textWaveStart.GetComponent<Text>().text = "START";
        }
        else if (EnemyManager.instance.LastCompletelySpawnedWave < EnemyManager.instance.TotalWaves) 
        {
            textWaveStart.GetComponent<Text>().text = "NEXT WAVE";
        }

    }
    private void ShowWaveBtn()
    {
        btnWaveStart.SetActive(true);
    }

    public void SendNextWave()
    {
        EnemyManager.instance.IncrementWave();
        wavesPaused = false;
        btnWaveStart.SetActive(false);
    }

    IEnumerator ShowWaveEnd()
    {
        shownWaveEndScreenFor = EnemyManager.instance.CurrentWave;
        for(int i = 0; i < 10; i++)
        {
            if( waveComplete.alpha >= 1f) { continue; }
            waveComplete.alpha += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);

        //Remove after 2 sec
        for (int i = 0; i < 10; i++)
        {
            if (waveComplete.alpha <= 0f) { continue; }
            waveComplete.alpha -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }


    /*|==========================| ENEMY Management ===========================| */
    

    /*|==========================| Game Management ===========================| */

    public void PlayPause()
    {
        //Resume Game
        if(gamePaused == true )
        {
            gamePaused = false;
            playToggle.GetComponent<Image>().sprite = btnPlayPaused;
            playToggle.SetActive(true);
            GameManager.instance.State = gameState.level;
            Time.timeScale = GameManager.instance.GameSpeed;
            pauseMenu.SetActive(false);
            return;
        }

        //Pause Game
        if( gamePaused == false )
        {
            playToggle.SetActive(false);
            Time.timeScale = 0;
            gamePaused = true;
            playToggle.GetComponent<Image>().sprite = btnPlayNormal;
            
            GameManager.instance.State = gameState.pause;
            pauseMenu.SetActive(true);
            return;
        }
        
        

    }


    public void ShowGameOver()
    {
        pauseMenu.SetActive(true);
        //Change bg color
        Transform pausedBg = pauseMenu.transform.Find("pausedBg");
        pausedBg.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f, 0.4f);//Color red

        //Change text
        Text pauseText = GameObject.Find("pauseTitle").GetComponent<Text>();
        pauseText.text = "GAME OVER";

        //Remove resume button
        pauseMenuResumeBtn.SetActive(false);

        pauseMenuQuitBtn.transform.position = new Vector3(pauseMenuQuitBtn.transform.position.x, pauseMenuQuitBtn.transform.position.y + 30f);

    }


    /*|==========================| Money Management ===========================| */
    public void GetMoney(int amount)
    {
        if( amount <= GameSettings.MaxMoneyIncrement)
            money += Math.Abs(amount);
    }

    public void SpendMoney(int amount)
    {
        if( amount >= money)
        {
            money = 0;
        } else
        {
            money -= Math.Abs(amount);
        }
    }

    //Check the difference amount each cycle. If its higher than max allowed. Stop it
    private void MoneyAnticheat()
    {
        if( money - previousMoney > GameSettings.MaxMoneyIncrement)
        {
            //Caught cheating
            money = previousMoney;
        }else
        {
            previousMoney = money;
        }
    }

    public void DisplayMoney()
    {
        MoneyAnticheat();
        uiCurrentMoney.GetComponent<Text>().text = money.ToString();
    }


    IEnumerator UpdateUiElements()
    {
        while(true)
        {
            if( GameManager.instance.State == gameState.pause)
            {
                yield return new WaitUntil(() => GameManager.instance.State == gameState.level);
            }
            yield return new WaitForSeconds(0.1f); // 10 times per second

            //Update player money
            DisplayMoney();

            //Update current wave
            uiCurrentWave.GetComponent<Text>().text = EnemyManager.instance.CurrentWave.ToString();

            //Update escaped
            uiCurrentEscaped.GetComponent<Text>().text = EnemyManager.instance.TotalEscaped.ToString();

            //Update wave btn
            SetWaveBtnText();

            //Check game lost
            if(EnemyManager.instance.GameLost)
            {
                //Youve lost. Show end screen
                Debug.Log("UI -> GAME OVER");
                ShowGameOver();
                yield break;
            }

            //Check wave end
            if( EnemyManager.instance.WaveManagerRunning == true && 
                EnemyManager.instance.LastCompletelySpawnedWave == EnemyManager.instance.CurrentWave &&
                EnemyManager.instance.LastCompletelySpawnedWave != shownWaveEndScreenFor)
            {
                if( EnemyManager.instance.LastCompletelySpawnedWave == EnemyManager.instance.TotalWaves)
                {
                    //Game has ended
                    //Show win screen
                }else
                {
                    //Current wave have spawned fully. Check active count
                    if (EnemyManager.instance.ActiveEnemyCount == 0)
                    {
                        ShowWaveBtn();
                        StartCoroutine("ShowWaveEnd");
                    }
                }
                
            }

        }
    }




    public void Feedback(string message)
    {
        if( !feedbacks.Contains(message))
            feedbacks.Add(message);
    }
    private List<string> feedbacks = new List<string>();
    IEnumerator ShowFeedback()
    {
        float waitPerMessage = 5f;
        CanvasGroup canvas = feedbackSystem.GetComponent<CanvasGroup>();
        canvas.alpha = 0f;

        
        while (true)
        {
            if (GameManager.instance.State == gameState.pause)
            {
                yield return new WaitUntil(() => GameManager.instance.State == gameState.level);
            }
            if (feedbacks.Count == 0)
            {
                yield return new WaitUntil(() => feedbacks.Count > 0);
            }
            //Copy feedbacks to new list
            List<string> feedbacksThisCycle = new List<string>(feedbacks);
            feedbacks.Clear();
            float currentWaitPerMessage = waitPerMessage / (float)feedbacksThisCycle.Count;
            //Debug.Log(feedbacksThisCycle.Count.ToString() + " messages -> wait per msg: " + currentWaitPerMessage.ToString());

            for(int i = 0; i < feedbacksThisCycle.Count; i++)
            {
                canvas.alpha = 0.7f;
                feedback.text = feedbacksThisCycle[i];
                yield return new WaitForSeconds(currentWaitPerMessage); //Customized waiting time
            }
            //After loop, fade out
            for (int i = 0; i < 10; i++)
            {
                canvas.alpha -= 0.1f;
                yield return new WaitForSeconds(0.05f);
            }
            //End of routine
            feedback.text = "";
            canvas.alpha = 0f;

            //Go back to start
        }//EOL
    }
}
