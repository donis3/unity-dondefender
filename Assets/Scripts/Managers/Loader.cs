using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Loader : MonoBehaviour
{
    public GameObject gameManager;
    private void Awake()
    {
        //Verify gameManager binding
        Assert.IsNotNull(gameManager, "GameManager is not on loader script @ camera");

        //Load gameManager instance
        if(GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }

}
