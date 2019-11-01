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
    [Range(1,1000)]
    private int level = 0;
    //Getters
    public int LevelDisplay { get { return level; } }//Level number starts from 1. 0->1 (For GUI)
    public int Level { get { return level; } }

    private bool wavesPaused = true;
    private bool gamePaused = false;
    public bool WavesPaused { get { return wavesPaused; } }
    public bool GamePaused { get { return gamePaused; } }


    /*=============| Player Data For This Level |================*/
    //Player current money amount
    private int money = 0;
    private int previousMoney = 0;//Anti Cheat
    public int Money { get { return money; } }
    private int levelStars = 0;// 0 means not completed. 



    /*==============|Tools|=============*/
    public System.Random random = new System.Random();

    /*==============|Data Trackers|=============*/


    private int shownWaveEndScreenFor = -1;

    private int totalMoneyGain = 0;
    private int levelScore = 0;
    private int levelMaxScore = 0;
    private int levelSuccessPercentage = 0;

    private bool autoWaveActive = false;


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

    //WinScreen
    private CanvasGroup winScreen;
    private Button btnNextLevel;
    private GameObject lvlCompleteTxt;
    private GameObject[] winStars;


    //Tower Menu
    private RectTransform towerMenuTransform;
    private CanvasGroup towerMenu;
    private Text towerNameTxt;
    private Text towerLvlTxt;
    private Button towerUpgradeBtn;
    private Text towerUpgradeBtnTxt;
    private Button towerSellBtn;
    private Text towerSellBtnTxt;

    //Auto Wave Toggle
    private Button autoWave;

    //Speed Toggle
    private Button speedToggle;
    private Text speedToggleTxt;

    [Header("Play/Pause button sprites")]
    [Space(20)]
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
        if(level <= 1) { level = 1; }
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

        //Win menu items
        Transform winBanner = pauseMenu.transform.Find("winScreen");
        winScreen = winBanner.GetComponent<CanvasGroup>();
        btnNextLevel = winBanner.Find("btnNext").GetComponent<Button>();
        lvlCompleteTxt = pauseMenu.transform.Find("lvlCompleteTxt").gameObject;
        lvlCompleteTxt.SetActive(false);

        //Score
        winStars = new GameObject[3];
        winStars[0] = winBanner.Find("Stars/Star1").gameObject;
        winStars[1] = winBanner.Find("Stars/Star2").gameObject;
        winStars[2] = winBanner.Find("Stars/Star3").gameObject;
        winStars[0].SetActive(false);
        winStars[1].SetActive(false);
        winStars[2].SetActive(false);

        //Auto Wave
        autoWave = GameObject.Find("btnAutoWave").GetComponent<Button>();
        autoWave.onClick.AddListener(ToggleAutoWave);

        //Speed toggle
        speedToggle = GameObject.Find("speedToggle/btnSpeedToggle").GetComponent<Button>();
        speedToggleTxt = speedToggle.transform.Find("Text").GetComponent<Text>();

        speedToggle.onClick.AddListener(GameManager.instance.ToggleSpeed2x);


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

        //Load next level(Win Screen)
        btnNextLevel.onClick.AddListener(LoadNext);

        //Print current level
        uiCurrentLevel.GetComponent<Text>().text = (level).ToString();

        //print total waves
        uiTotalWave.GetComponent<Text>().text = EnemyManager.instance.TotalWaves.ToString();

        //Print allowed escape
        uiMaxEscaped.GetComponent<Text>().text = EnemyManager.instance.EnemyEscapeAllowed.ToString();
        uiCurrentEscaped.GetComponent<Text>().text = "0";


        //Get tower menu objects
        towerMenu = GameObject.Find("towerMenu").GetComponent<CanvasGroup>();
        towerMenuTransform = towerMenu.GetComponent<RectTransform>();
        towerNameTxt = towerMenu.transform.Find("towerNameTxt").gameObject.GetComponent<Text>();
        towerLvlTxt = towerMenu.transform.Find("towerLvlTxt").gameObject.GetComponent<Text>();
        towerUpgradeBtn = towerMenu.transform.Find("towerMenuBtnGrp/towerUpgradeBtn").gameObject.GetComponent<Button>();
        towerUpgradeBtnTxt = towerUpgradeBtn.transform.GetComponentInChildren<Text>();
        towerSellBtn = towerMenu.transform.Find("towerMenuBtnGrp/towerSellBtn").gameObject.GetComponent<Button>();
        towerSellBtnTxt = towerSellBtn.transform.GetComponentInChildren<Text>();
        
        //initial menu state
        towerMenu.alpha = 0f;
        towerMenu.blocksRaycasts = false;
        towerMenu.interactable = false;



        //Add money
        AddStartingMoney();

        StartCoroutine("UpdateUiElements");
        StartCoroutine("ShowFeedback");


        
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
        if( autoWaveActive == true)
        {
            return;
        }
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

    private void ToggleAutoWave()
    {
        autoWaveActive = !autoWaveActive;//toggle

        //Set color
        if(autoWaveActive)
        {
            autoWave.GetComponent<Image>().color = new Color(0.25f, 1f, 0.1f, 1f);
        } else
        {
            autoWave.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    /*|==========================| ENEMY Management ===========================| */
    
    public bool CheckWinStatus()
    {
        //If the last wave have not spawned yet, level cant complete
        if(EnemyManager.instance.LastCompletelySpawnedWave != EnemyManager.instance.TotalWaves)
        {
            return false;
        }
        if(EnemyManager.instance.ActiveEnemyCount > 0)
        {
            return false;
        }
        if(EnemyManager.instance.Enemies.Count > 0)
        {
            return false;
        }
        if( EnemyManager.instance.GameLost == true )
        {
            return false;
        }
        if( EnemyManager.instance.TotalEscaped > EnemyManager.instance.EnemyEscapeAllowed )
        {
            return false;
        }

        return true;

    }

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


    //Lose Screen
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

    //Win Screen /Level complete
    public void ShowWinScreen()
    {
        winScreen.alpha = 1f;
        winScreen.interactable = true;
        winScreen.blocksRaycasts = true;
        pauseMenuResumeBtn.SetActive(false);//disable pause btn
        pauseMenu.SetActive(true);

        int score = CalculateScore();
        int stars = 0;
        if (score >= 0 && score <= 33) { stars = 1; }
        else if (score > 33 && score < GameSettings.MinScoreForPerfect) { stars = 2; }
        else if (score >= GameSettings.MinScoreForPerfect) { stars = 3; }
        //Save stars
        levelStars = stars;

        //Save Progress if this one is higher
        GameManager.instance.SaveLevelProgress(level, stars);
        //Show win screen
        StartCoroutine(WinScreenAnimation(stars));
    }

    private  int CalculateScore()
    {
        if( CheckWinStatus() == false )
        {
            return 0;
        }
        int totalAllowed = EnemyManager.instance.EnemyEscapeAllowed;
        int totalEscaped = EnemyManager.instance.TotalEscaped;
        float unescaped = (float)totalAllowed - (float)totalEscaped;
        if( unescaped <= 0f)
        {
            unescaped = 0.1f;
        }
        if( totalAllowed <= 0)
        {
            return 100;
        }
        float percentage = (unescaped / (float)totalAllowed) * 100;
        int score = (int)Math.Round(percentage);


        return score;
        
    }

    //Win screen stars animator
    IEnumerator WinScreenAnimation(int stars)
    {
        
        lvlCompleteTxt.SetActive(true);
        lvlCompleteTxt.transform.localScale = Vector2.one * 0.1f;
        for (int i = 0; i < 17; i++ )
        {
            double pow = Math.Pow((double)i, 2d);
            if ( i <= 8)
            {
                
                yield return new WaitForSecondsRealtime(0.05f);
                
                if( i == 8)
                {
                    lvlCompleteTxt.transform.localScale = new Vector2(1.8f, 1.8f);
                }else
                {
                    lvlCompleteTxt.transform.localScale = Vector2.one * 0.05f * (float)pow;
                }
            } else
            {
                yield return new WaitForSecondsRealtime(0.05f);
                lvlCompleteTxt.transform.localScale = new Vector2(lvlCompleteTxt.transform.localScale.x-0.1f, lvlCompleteTxt.transform.localScale.y - 0.1f);
            }
            
            
        }
        lvlCompleteTxt.transform.localScale = Vector2.one;

        yield return new WaitForSecondsRealtime(0.5f);
        //Show Stars
        if ( stars > 0)
        {
            for(int i = 0; i < stars; i++)
            {
                winStars[i].SetActive(true);
                if(i != 2) { 
                    yield return new WaitForSecondsRealtime(0.5f);
                }else
                {
                    for(int j = 0; j < 10; j++)
                    {
                        yield return new WaitForSecondsRealtime(0.05f);
                        if( winStars[i].transform.localScale.x < 1.5f && j < 5)
                        {
                            winStars[i].transform.localScale = new Vector2(winStars[i].transform.localScale.x + 0.1f, winStars[i].transform.localScale.y + 0.1f);
                        }else if(winStars[i].transform.localScale.x > 1f)
                        {
                            winStars[i].transform.localScale = new Vector2(winStars[i].transform.localScale.x - 0.1f, winStars[i].transform.localScale.y - 0.1f);
                        }else if(winStars[i].transform.localScale.x <= 1f)
                        {
                            break;
                        }
                    }
                    winStars[i].transform.localScale = Vector2.one;
                    continue;
                }

            }
        }
    }


    //Ask game manager to load next level
    private void LoadNext()
    {
        GameManager.instance.LoadNextLevel(level);
    }


    /*|==========================| Money Management ===========================| */
    public void GetMoney(int amount)
    {
        //Include level multiplyer
        float levelMultiplier = 1f;
        if( level > 0)
        {
            levelMultiplier = 1f + ((float)level / 30f); //after 30th level money gain will be nearly 2x
        }
        if( amount <= GameSettings.MaxMoneyIncrement)
        {
            Debug.Log("Get money: " + amount);
            amount = Math.Abs(amount);
            amount = (int)Math.Round((float)amount * levelMultiplier);
            Debug.Log("Multiplied money: " + amount);
            money += amount;
            totalMoneyGain += Math.Abs(amount);
        }
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

    public void WaveEndMoneyCheck()
    {
        if(money < GameSettings.MinMoney)
        {
            money = GameSettings.MinMoney;
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

    public void AddStartingMoney()
    {
        int startingMoney = GameSettings.StartingMoney;
        float bonus = 0f;
        if( level > 0)
        {
            bonus = ((float)startingMoney * (float)level) / 10f; //Bonus is %10 of level*starting money. if lvl 100, starting is 40 u get 400
        }
        GetMoney((int)Math.Round((double)bonus) + startingMoney);
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

            //Check game speed
            if(GameManager.instance.GameSpeed > 1f)
            {
                speedToggle.GetComponent<Image>().color = new Color(0.2f, 0.95f, 0.1f);
                speedToggleTxt.color = new Color(1f, 1f, 1f);
            }else
            {
                speedToggle.GetComponent<Image>().color = new Color(1f, 1f, 1f);
                speedToggleTxt.color = new Color(0.7f, 0.7f, 0.7f);
            }

            //Check game lost
            if(EnemyManager.instance.GameLost)
            {
                //Youve lost. Show end screen
                ShowGameOver();
                yield break;
            }

            //Check game won
            if( CheckWinStatus() == true )
            {
                Feedback("Level Complete!");
                ShowWinScreen();
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
                    //Do nothing. CheckWinStatus will take over
                }else
                {
                    //Current wave have spawned fully. Check active count
                    if (EnemyManager.instance.ActiveEnemyCount == 0)
                    {
                        //Wave cleared
                        
                        StartCoroutine("ShowWaveEnd");

                        yield return new WaitForSeconds(1f);
                        ShowWaveBtn();

                        //Press the button if auto is active
                        if(autoWaveActive)
                        {
                            yield return new WaitForSecondsRealtime(1f);
                            btnWaveStart.GetComponent<Button>().onClick.Invoke();//Auto Click
                        }

                    }
                }
                
            }

        }
    }


    /*|==========================| Tower Management |===========================| */
    private bool towerMenuActive = false;
    public void HideTowerMenu()
    {
        towerMenuActive = false;
        //Remove Listeners
        towerUpgradeBtn.onClick.RemoveAllListeners();
        towerSellBtn.onClick.RemoveAllListeners();


        towerMenu.alpha = 0f;
        towerMenu.interactable = false;
        towerMenu.blocksRaycasts = false;
        towerMenuTransform.position = Vector2.zero;
    }

    public void ShowTowerMenu(Vector2 moveTo, Tower tower)
    {
        if(towerMenuActive == true) { return; }
        towerMenuActive = true;

        
        towerMenu.alpha = 1f;
        towerMenu.interactable = true;
        towerMenu.blocksRaycasts = true;

        //moveTo.x += 0.5f;
        moveTo.y -= 0.9f;

        Vector2 towerPosition = Camera.main.WorldToScreenPoint(moveTo);
        
        towerMenuTransform.position = towerPosition;

        //Update Tower Menu Labels
        towerNameTxt.text = tower.TowerName;
        towerLvlTxt.text = tower.TowerLevel;
        
        //Upgrade Button
        if( tower.UpgradePrice > 0)
        {
            towerUpgradeBtn.gameObject.SetActive(true);
            towerUpgradeBtnTxt.text = tower.UpgradePrice.ToString();
            if ( tower.UpgradePrice > money)
            {
                towerUpgradeBtn.enabled = false;
                //Cant Afford
                towerUpgradeBtnTxt.color = new Color(1f, 0.2f, 0.2f);
            } else
            {
                towerUpgradeBtn.enabled = true;
                towerUpgradeBtnTxt.color = new Color(1f, 1f, 1f);
                towerUpgradeBtn.onClick.AddListener(tower.UpgradeTower);
            }
            
            
            
        } else
        {
            towerUpgradeBtnTxt.text = "0";
            towerUpgradeBtn.gameObject.SetActive(false);
        }

        //Sell Button
        if(tower.SellPrice > 0)
        {
            towerSellBtn.gameObject.SetActive(true);
            towerSellBtnTxt.text = tower.SellPrice.ToString();
            towerSellBtn.onClick.AddListener(tower.SellTower);
        } else
        {
            towerSellBtnTxt.text = "0";
            towerSellBtn.gameObject.SetActive(false);
        }


    }


    /*====================| FEEDBACK SYSTEM |===========================*/
    public void Feedback(string message, float wait = 5f)
    {
        waitPerMessage = wait;
        if( !feedbacks.Contains(message))
            feedbacks.Add(message);
    }
    private List<string> feedbacks = new List<string>();
    private float waitPerMessage = 5f;
    IEnumerator ShowFeedback()
    {
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
