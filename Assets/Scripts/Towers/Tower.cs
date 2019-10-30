using System;
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

    [Header("Accuracy")]
    [SerializeField]
    [Range(1f, 10f)]
    private float accuracy = 1f;

    [Header("Projectile Prefab")]
    [Space(20f)]
    [SerializeField]
    private Projectiles projectile;

    
    [Header("Other")]
    [Space(20f)]
    private Enemy targetEnemy = null;
    private List<Enemy> enemiesInRange = new List<Enemy>(); 
    private GameObject radiusObj = null;

    [SerializeField]
    [Tooltip("Where the projectiles shoot from. +y will go up, -y will go down")]
    private Vector2 projectileOffset =  Vector2.zero;

    private float lastTargetChange = 0f; //New target picking time tracker
    private bool debug = false; //Will color the target red if enabled


    

    

    private void Awake()
    {
        //Find radius visualizer object under prefab
        Transform children = transform.Find("radius"); //Finds child
        if (children != null)
        {
            radiusObj = children.gameObject;
            //Find the size of radius sprite and calculate scale to match tower radius
            Bounds  Bounds = radiusObj.GetComponent<SpriteRenderer>().sprite.bounds;
            radiusObj.transform.localScale = (Vector2.one*2*radius) / Bounds.size;
        }
        else
        {
            Debug.LogError("Could not find radius visualizer object for tower");
        }

    }


    private void Start()
    {
        

        //Tower starts scanning. (Will pause if enemy manager.isplaying false)
        StartCoroutine("ScanEnemies");
        StartCoroutine("TargetDistanceTracker");
        StartCoroutine("ShootingManager");

    }

    private void Update()
    {
        
    }

    /* =====================================| TOWER OBJECT SELECTABLE |================================= */

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


    /* =====================================| ENEMY SCANNER & TARGET PICKER |================================= */
    /**
     * Clear the list of in range enemies
     * Find all enemies in range and add to list
     */
    private void GetEnemiesInRange()
    {
        //Debug.Log("Scanning a target for " + gameObject.name);
        enemiesInRange.Clear();
        foreach (Enemy e in FindObjectOfType<EnemyManager>().Enemies)
        {
            if( e.IsDead) { continue; } //Skip dead enemies
            if (Vector2.Distance(e.transform.localPosition, transform.localPosition) <= radius)
            {
                
                enemiesInRange.Add(e);
            }
        }

    }
    //Target single closest enemy

    /**
     * Get the first enemy from list and make it target
     */
    private void GetClosestEnemy()
    {
        
        //Get the first enemy in the list (it was the first one that came in to our range)
        //And keep the target till it goes out of range
        if( enemiesInRange.Count <= 0)
        {
            return;//Faild to get enemy
        }
        float minDistance = radius * 10f;//Mega distance to compare to
        Enemy ClosestEnemy = null;
        foreach(Enemy newTarget in enemiesInRange)
        {
            if( newTarget.IsDead) { continue; } //Skip dead enemies
            float distanceToTarget = Vector2.Distance(newTarget.transform.localPosition, transform.localPosition);
            if( distanceToTarget <= minDistance)
            {
                //Found an enemy in the given distance
                minDistance = distanceToTarget;//Update the closest distance we have found. Compare next iteration if thers a closer enemy
                ClosestEnemy = newTarget;
            }
        }//EOL
        if( ClosestEnemy != null)
        {
            //Found the closest enemy to the tower
            NewTarget(ClosestEnemy);
        }
       

    }


    //Lets change target when the enemy is close to moving out of range
    private void VerifyTargetInRange()
    {
        if(targetEnemy != null &&  targetEnemy.IsDead)
        {
            RemoveTarget();
        }
        if (targetEnemy != null && Vector2.Distance(targetEnemy.transform.localPosition, transform.localPosition) > radius)
        {
            //Remove Target. Its too far away
            RemoveTarget();
            //Debug.Log("Target went out of range for " + gameObject.name);

            //Run get enemies in range
        }

        
    }

    //Remove enemy from current target
    private void RemoveTarget()
    {
        if(targetEnemy != null)
        {

            //Debug Coloring
            if (debug == true) { targetEnemy.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f); }
            targetEnemy = null;
        }
    }

    //Will change the enemy target and remove the old one if a new one is given
    void NewTarget(Enemy target)
    {
        
        if( target != null)
        {
            //Remove old one
            if (targetEnemy != null)
            {
                RemoveTarget();
            }

            //Save new one
            targetEnemy = target;
            if (debug == true) { targetEnemy.GetComponent<SpriteRenderer>().color = new Color(1f, 0.2f, 0.2f); }//Debug coloring
            //Keep time
            lastTargetChange = GameManager.instance.getGameTime(); //Keep track of time
        }
    }

    /* =====================================| ENEMY SCAN & TRACK COROUTINES |================================= */
    /** 
     * IF target = null -> Scan and get a target. If nothing in range, do it again in .5 seconds
     * IF target = enemy -> Wait 2 seconds and scan again. Get closest
     */
    IEnumerator ScanEnemies()
    {
        while(true)
        {
            //Game Paused
            if(FindObjectOfType<EnemyManager>().gameStarted == false)
            {
                yield return new WaitUntil(() => FindObjectOfType<EnemyManager>().gameStarted == true);
            }
            //Game Running
            yield return new WaitForSeconds(0.1f);

            //Remove target if out of range
            

            //Change if 2 seconds have passed
            if (GameManager.instance.getGameTime() - lastTargetChange > 2f) //x seconds
            {
                //Debug.Log("2 seconds passed. Trying to get new target");
                RemoveTarget();
            }

            //If target is null, Scan enemies
            if ( targetEnemy == null)
            {
                GetEnemiesInRange();//Scan and make a list
                //Get first element as target if any exists
                if(enemiesInRange.Count > 0)
                {
                    GetClosestEnemy();
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }

        
    }

    /**When we have a target
     * Run distance tracker continusly
     */
    IEnumerator TargetDistanceTracker()
    {
        while(true)
        {
            //Game Paused
            if (FindObjectOfType<EnemyManager>().gameStarted == false)
            {
                yield return new WaitUntil(() => FindObjectOfType<EnemyManager>().gameStarted == true);
            }

            if (targetEnemy != null)
            {
                VerifyTargetInRange();
            }
            yield return null;
        }
        
    }





    /* =====================================| ATTACK OPERATIONS |========================= */

    /**
     * If there is a target enemy present
     * Shoot 1 projectile. 
     * Projectile follows enemy till collision and destroys itself
     */
    void ShootProjectile()
    {
        //Nothing to shoot
        if( targetEnemy == null) { return; }

        //Instantiate projectile
        Projectiles newProjectile = GameObject.Instantiate(projectile);
        newProjectile.transform.position = new Vector2(transform.localPosition.x + projectileOffset.x, transform.localPosition.y + projectileOffset.y);

        StartCoroutine(MoveProjectile(newProjectile, targetEnemy));
        


    }
    IEnumerator MoveProjectile(Projectiles projectile, Enemy target)
    {
        //Debug.Log("Moving a projectile towards enemy...");

        //Distance Tracker (Accuracy)
        float maxDistance = 0.5f / accuracy; //if accuracy is 1, arrows will stop at 0.5f // If accuracy is 10, it will go to 0.05f
        float minDistance = 0.01f;
        //Randomize accuracy
        float randomStoppingDistance = Rhinotap.Tools.RandomFloat(minDistance, maxDistance);
        //Debug.Log("Accuracy distance: " + randomStoppingDistance.ToString("n4"));

        //Time Tracker
        float step = 0.1f; //limit calculations to 10 times per second
        float lastMovement = 0f;
        while (target != null && projectile != null) {

            //Game Paused
            if (FindObjectOfType<EnemyManager>().gameStarted == false)
            {
                yield return new WaitUntil(() => FindObjectOfType<EnemyManager>().gameStarted == true);
            }
            lastMovement += Time.deltaTime;
            if( lastMovement < step)
            {
                continue;
            }else if( lastMovement >= step)
            {
                lastMovement = 0f;
            }


            
            //Remaining distance
            float distance = Mathf.Abs(Vector2.Distance(projectile.transform.localPosition, target.transform.localPosition));
            if( distance <= randomStoppingDistance)
            {
                yield return new WaitForSeconds(0.1f);
                if(projectile != null && projectile.gameObject != null)
                {
                    projectile.Remove();
                }
                yield break; //Reached Target
            }
            //Other checks (enemy is dead?)


            //Dİrectional vector
            Vector2 direction = target.transform.localPosition - projectile.transform.localPosition;

            //Find the angle of the vector. projectile -------> enemy
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            //Apply rotation
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            projectile.transform.localRotation = Quaternion.Slerp(projectile.transform.localRotation, rotation, projectile.Speed);

            //Apply movement
            projectile.transform.localPosition = Vector2.MoveTowards(projectile.transform.localPosition, target.transform.localPosition, projectile.Speed * Time.deltaTime);

            //Continue till we hit the target OR target disappears
            yield return null;
        }
        if( projectile != null)
        {
            Destroy(projectile.gameObject);
            projectile = null;
        }
        yield break;
    }

    IEnumerator ShootingManager()
    {
        while( true )
        {
            //Game Paused
            if (FindObjectOfType<EnemyManager>().gameStarted == false)
            {
                yield return new WaitUntil(() => FindObjectOfType<EnemyManager>().gameStarted == true);
            }

            //Stop if no target
            if(targetEnemy == null)
            {
                yield return new WaitUntil(() => targetEnemy != null);
            }
            //Shoot a projectile
            ShootProjectile();

            //Wait for shooting speed
            yield return new WaitForSeconds(shootingSpeed);

        }
    }



}
