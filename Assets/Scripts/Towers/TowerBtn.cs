using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBtn : MonoBehaviour
{
    [Header("Assign the tower you want to spawn")]
    [SerializeField]
    private GameObject towerObject;

    public GameObject TowerObject
    {
        get { return towerObject; }
    }

    
}
