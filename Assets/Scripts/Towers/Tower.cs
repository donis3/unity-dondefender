using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]

    [SerializeField]
    [Tooltip("The distance from which the tower can shoot")]
    [Range(0.1f, 10f)]
    private float radius = 1f;

    [SerializeField]
    [Tooltip("How many seconds between each attack")]
    [Range(0.1f, 5f)]
    private float shootingSpeed = 1f;

    [SerializeField]
    [Tooltip("Type of projectile")]
    private projectileType dmgType = projectileType.arrow;

    [Header("Projectile Prefab")]
    [Space(20f)]
    [SerializeField]
    private Projectiles projectile;

    [Header("Other")]
    [Space(20f)]
    private Enemy targetEnemy = null;
    private List<Enemy> enemiesInRange = new List<Enemy>(); 
    private GameObject radiusObj = null;


    

    private void Awake()
    {
        //Find radius visualizer object under prefab
        Transform children = transform.Find("radius"); //Finds child
        if (children != null)
        {
            radiusObj = children.gameObject;
            //Radius sprite must be 1unit x 1unit (Adjust pixels per unit)
            radiusObj.transform.localScale = Vector2.one * (radius*2);
        }
        else
        {
            Debug.LogError("Could not find radius visualizer object for tower");
        }

    }


    private void Start()
    {
        


        StartCoroutine("ScanEnemies");
    }

    private void Update()
    {
        
    }

    //SELECT TOWER (Show Radius)
    public void SelectThisTower()
    {
        
        //Debug.Log("Selecting Tower");
        if(radiusObj == null) { return; }
        radiusObj.SetActive(true);
        
        
    }

    //DESELECT TOWER (Hide Radius)
    public void DeselectThisTower()
    {
        //Debug.Log("De-Selecting Tower");
        if (radiusObj == null) { return; }
        radiusObj.SetActive(false);
    }


    /**
     * Get all enemies
     */
    public void GetEnemiesInRange()
    {
        enemiesInRange.Clear();
        foreach (Enemy e in FindObjectOfType<EnemyManager>().Enemies)
        {
            if (Vector2.Distance(e.transform.position, transform.position) <= radius)
            {
                enemiesInRange.Add(e);
            }
        }

    }
    //Target single closest enemy
    public void GetClosestEnemy()
    {
        if( enemiesInRange.Count == 0)
        {
            targetEnemy = null;
            return;
        }
        //Loop all enemies and compare their distance. Find closest one
        foreach( Enemy e in enemiesInRange)
        {
            float closestDistance = radius * 2;
            float distance = Vector2.Distance(e.transform.position, transform.position);
            if ( distance < closestDistance)
            {
                closestDistance = distance;
                Debug.Log("Found an enemy in range (" + dmgType.ToString() + ")");
                targetEnemy = e;
                
            }
        }
    }

    //Enemy scanner
    IEnumerator ScanEnemies()
    {
        while(true)
        {
            
            if(FindObjectOfType<EnemyManager>().gameStarted == false)
            {
                yield return new WaitUntil(() => FindObjectOfType<EnemyManager>().gameStarted == true);
            }
            yield return 0.2f;
            GetEnemiesInRange();
            GetClosestEnemy();
            yield return 0.3f;
        }
        
        
    }

}
