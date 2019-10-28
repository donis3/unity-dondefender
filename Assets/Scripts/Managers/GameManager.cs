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
        if (Input.GetKeyDown("1") )
        {
            SceneManager.LoadScene("Menu");
            
        }
        if (Input.GetKeyDown("2"))
        {
            SceneManager.LoadScene("Game");
            
        }
        

    }

}
