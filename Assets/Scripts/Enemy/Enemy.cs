using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    //Checkpoint tracking
    private int target = 0;

    //enemy speed
    [Header("Enemy Move Speed")]
    [Range(0.1f, 3f)]
    public float speed = 0.75f;

    //Difficulty coefficient
    [Header("Enemy difficulty [0,1]")]
    [Range(1f, 10f)]
    public float difficulty = 1f;

    //Health
    [Header("Enemy Health [1,10000]")]
    [Range(1, 10000)]
    public int health = 10;

    //Components
    private Animator anim;

    //Internal constants
    private const string exitTag = "Finish"; //Despawner tag
    private const string waypointTag = "Checkpoint"; //Waypoint tag
    private const string projectileTag = "Projectile";
    //Internal Vars
    private int checkpointCount = 0; //Total checkpoint count
    private Vector2 currentHeading;

    private int sortingOrder = 0; // current sprite sorting order
    private int sortingOrderIfGoingUpwards = 0; //if enemy goes up, change sorting layer because background objects go foreground
    private int activeEnemyCount = 0;
    private bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }
    


    //Internal Objects & Components (Will be bound automatically at start() );
    private Transform enemy; //enemy object
    private SpriteRenderer sprite;
    private EnemyManager enemyManager;
    private Transform enemyExit; //Location of exit point
    private GameObject[] waypoints;


    private void Awake()
    {

        //Get animator component
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Could not find animator component for enemy");
        }

    }
    /** INITIALIZE
     * Get required objects, components
     * Verify requirements
     */
    void Start()
    {
        //Prepare Data
        enemy = GetComponent<Transform>(); //Get self transform of enemy
        sprite = GetComponent<SpriteRenderer>();
        enemyManager = FindObjectOfType<EnemyManager>();
        
        //GET WAYPOINTS
        //Will order the objects by their hierarchy order. Top object will be index 0
        waypoints = GameObject.FindGameObjectsWithTag(waypointTag); 
        if( waypoints.Length == 0)
        {
            Debug.LogError("There are 0 waypoints. Please create checkpoint tagged objects on the path");
        }
        checkpointCount = waypoints.Length;

        //GET EXIT POINT
        enemyExit = GameObject.FindGameObjectWithTag(exitTag).transform;
        if( enemyExit == null)
        {
            Debug.LogError("Could not find exit point. Please create object with Finish tag");
        }

        //Get sprite sorting order
        //Reduce order layer
        sortingOrder = sprite.sortingOrder;

        activeEnemyCount = enemyManager.ActiveEnemyCount;
        sprite.sortingOrder -= activeEnemyCount; //send back 1 layer
        sortingOrder = sprite.sortingOrder; //update internal tracker

        sortingOrderIfGoingUpwards = sortingOrder + (activeEnemyCount * 2);//Keep reference for upwards going order 

    }

    /** UPDATE
     * Call movement controller
     */
    void Update()
    {
        enemyMoveController(); //Move enemies across the board
        
    }

    /*====================================| MOVEMENT OF ENEMY THROUGHOUT THE MAP |====================================*/
    /** MOVEMENT
     * Move the enemy towards the next checkpoint
     * Use speed coefficient
     * When last checkpoint is reached, run to exit
     */
    private void enemyMoveController()
    {
        //Check death
        if (isDead) { return; }
        if( EnemyManager.instance.GameLost == true )
        {
            anim.enabled = false; //Stop animation when game is lost
            return;
        }

        //Check waypoints
        if (waypoints.Length == 0) { return; }

        

        if (target < checkpointCount)
        {
            //There are still checkpoints to visit
            enemy.position = Vector2.MoveTowards(enemy.position, waypoints[target].transform.position, speed*Time.deltaTime);

            //Save the current heading
            currentHeading = enemy.position - waypoints[target].transform.position;

            //Call directional controller to decide which way enemy is going
            enemyDirectionController();

        }
        else
        {
            //We have finished the array. GO to exit point
            enemy.position = Vector2.MoveTowards(enemy.position, enemyExit.position, speed * Time.deltaTime);
        }
        


    }


    /** SPRITE DIRECTION
     * Determine which way enemy is going. +x is left -x is right
     * Flip the sprite accordingly
     */
    private void enemyDirectionController()
    {
        //Horizontal
        if(currentHeading.normalized.x > 0.1)
        {
            //Flipping needed
            if (sprite.flipX == false)
            {
                //Switch rotation
                sprite.flipX = true;
            }
            
        } else
        {
            //Flipping Needed
            if (sprite.flipX == true)
            {   
                //Switch rotation
                sprite.flipX = false;
            }
            
        }

        //Vertical
        //If going up, we need to change sorting order. 
        //Lowest sorting order should be highest
        if (currentHeading.normalized.y < 0.1)
        {
            //GOING UP
            if (sprite.sortingOrder != sortingOrderIfGoingUpwards) //Check if already upwards
            {
                sprite.sortingOrder = sortingOrderIfGoingUpwards;
            }
        } else
        {
            //GOING NORMAL
            if( sprite.sortingOrder != sortingOrder ) //check if already normal
            {
                sprite.sortingOrder = sortingOrder;
            }
        }



    }

    /*====================================| ENEMY SPAWN & DESPAWN |====================================*/

    /*====================================| ENEMY DAMAGE |====================================*/
    private void takeDamage(int damage)
    {
        if( damage > 0 && isDead == false)
        {
            if( damage <= health)
            {
                health -= damage;
                //Hurt animation 
                anim.Play("Hurt");
            }else
            {
                health = 0;
                //Death animation
                anim.Play("Death");
                Die();
                EnemyManager.instance.killEnemy(gameObject);
            }
        }
        //Debug.Log("Im hit. My hp is: " + health);
    }

    /*====================================| ENEMY DEATH |====================================*/

    private void Die()
    {
        isDead = true;
    }

    /*====================================| COLLIDERS |====================================*/
    /**TRIGGERS
     * Keep track of triggers
     * When exit point is triggerd, destroy object and inform GameManager
     */
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //CHECK the trigger tag
        switch(collision.tag)
        {
            case waypointTag:
                //Increase target
                target += 1;
                break;

            case exitTag:
                //Despawn enemy
                //Reduce number of active enemies
                enemyManager.despawnEnemy(gameObject);
                break;

            case projectileTag:
                //Enemy is hit with a projectile
                Projectiles projectile = collision.gameObject.GetComponent<Projectiles>();//Projectile
                //Take damage
                takeDamage(projectile.Damage);
                //Destroy the projectile
                projectile.DestroyObject();

                break;
            default:
                //Do nothing
                break;
        }

    }

}
