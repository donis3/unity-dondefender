using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelect : MonoBehaviour
{
    public static LevelSelect instance { get; private set; }//Instantiate once per level

    [Header("Level Btn Prefab")]
    [SerializeField]
    private GameObject LevelBtn;

    //Required Game Objects
    private GameObject LevelsPanel;

    private Vector2 currentScale;

    public bool levelsLoaded { get; private set; } = false;
    

    


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


    public void SpawnButtons()
    {
        levelsLoaded = true;
        currentScale = GameObject.Find("Canvas").GetComponent<RectTransform>().localScale;
        LevelsPanel = GameObject.Find("Levels");
        GameManager.instance.GetSceneCount();

        
        if (GameManager.instance.LevelCount > 0)
        {
        
            for(int i = 0; i < GameManager.instance.LevelCount; i++)
            {
                
                SpawnLevelButton(i);
            }
        }

        
        

    }



    private void SpawnLevelButton(int LevelNo)
    {
        GameObject LevelButton = Instantiate(LevelBtn);
        LevelButton.transform.localPosition = new Vector2(0f, 0f);
        LevelButton.transform.localScale = currentScale;
        LevelButton.transform.SetParent(LevelsPanel.transform);
        
        //Get Components

        //Level No
        Text LevelText = LevelButton.transform.Find("LvlTxt").GetComponent<Text>();
        //Stars
        Image[] stars = new Image[3];
        for(int i = 0; i < stars.Length; i++)
        {
            stars[i] = LevelButton.transform.Find("stars/Star" + (i+1).ToString()).GetComponent<Image>();
            if( stars[i] == null)
            {
                //Error finding star
                Debug.Log("System Error: Couldnt find Level Button - Star image");
            }
            //Paint gray
            stars[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
        //Lock
        GameObject Lock = LevelButton.transform.Find("LockedImg").gameObject;

        //Print lvl
        LevelText.text = (LevelNo + 1).ToString();

        //Check status
        int Status = GameManager.instance.LevelStats[LevelNo];
        if( Status > 0)
        {
            //Print Stars
            for(int i = 0; i < stars.Length; i++)
            {
                if( i+1 <= Status)
                {
                    stars[i].color = new Color(1f, 1f, 1f);
                }
            }

        }

        if( LevelNo <= GameManager.instance.HighestLevelAvailable)
        {
            ToggleLevelButton(LevelNo, LevelButton, true);
        }else
        {
            ToggleLevelButton(LevelNo, LevelButton, false);
        }

        

        

    }

    void ToggleLevelButton(int LevelNo, GameObject LevelButton, bool isAvailable)
    {
        if( LevelButton == null )
        {
            return;
        }
        GameObject LockImg = LevelButton.transform.Find("LockedImg").gameObject;
        Button LevelBtn = LevelButton.GetComponent<Button>();
        Text LevelTxt = LevelBtn.transform.Find("LvlTxt").GetComponent<Text>();

        
        if (isAvailable == true)
        {
            //Unlocked
            LockImg.SetActive(false);
            LevelBtn.onClick.AddListener(() => GameManager.instance.LoadLevel(LevelNo));
            LevelTxt.color = new Color(1f, 1f, 1f, 1f); //White
        } else
        {
            //Locked
            LockImg.SetActive(true);
            LevelTxt.color = new Color(1f, 1f, 1f, 0f); //alpha 0
            LevelBtn.onClick.RemoveAllListeners();
        }
    }

}
