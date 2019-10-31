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

    public GameObject TowerObject
    {
        get { return towerObject; }
    }

    private void Awake()
    {
        priceLabel = GetComponentInChildren<Text>();
        price = towerObject.GetComponent<Tower>().Price;

        priceLabel.text = price.ToString();
    }

    


}
