using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerBtn : MonoBehaviour
{
    [Header("Assign the tower you want to spawn")]
    [SerializeField]
    private GameObject towerObject;

    private Text priceLabel; //Price label for the button
    private int price = 0;
    public int Price { get { return price; } }

    private bool affordable = false;
    public bool Affordable { get { return affordable; } }


    private Button btn;

    public GameObject TowerObject
    {
        get { return towerObject; }
    }

    private void Awake()
    {
        priceLabel = GetComponentInChildren<Text>();
        price = towerObject.GetComponent<Tower>().Price;

        priceLabel.text = price.ToString();
        btn = GetComponent<Button>();
    }

    private void Start()
    {

        StartCoroutine("CheckAffordable");
    }

    //Check if the tower is affordable
    IEnumerator CheckAffordable()
    {
        //Endless loop
        while(true)
        {
            //Pause
            if (GameManager.instance.State == gameState.pause)
            {
                yield return new WaitUntil(() => GameManager.instance.State == gameState.level);
            }

            //Iteration Limiter
            yield return new WaitForSeconds(0.2f);

            if( LevelManager.instance.Money < Price)
            {
                //Debug.Log("cant afford");
                btn.interactable = false;
                affordable = false;
            }else
            {
                btn.interactable = true;
                affordable = true;
            }


            yield return null;
        }
    }

    


}
