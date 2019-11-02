using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelect : MonoBehaviour
{
    [Header("Level Btn Prefab")]
    [SerializeField]
    private GameObject LevelBtn;

    //Required Game Objects
    private GameObject LevelsPanel;
    

    private void Start()
    {
        LevelsPanel = GameObject.Find("Levels");
        if(GameManager.instance.LevelCount > 0)
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
        LevelButton.transform.localScale = Vector2.one;
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

        //Check availability
        if( LevelNo <= GameManager.instance.HighestLevelAvailable)
        {
            //LEVEL AVAILABLE
            Debug.Log("Level " + (LevelNo+1).ToString() + " is available");
            Lock.SetActive(false);
            //Bind Event
            LevelButton.GetComponent<Button>().onClick.AddListener(() => GameManager.instance.LoadLevel(LevelNo));
        } else
        {
            Debug.Log("Level " + (LevelNo + 1).ToString() + " is disabled");
            Lock.SetActive(true);
            LevelButton.GetComponent<Button>().enabled = false;
            //Hide text
            LevelText.color = new Color(1f, 1f, 1f, 0f);
            
        }

        

    }

}
