using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    //Checkpoint tracking
    private int target = 0;

    //enemy speed
    public float speed = 1.5f;

    //Internal constants
    private const string exitTag = "Finish"; //Despawner tag
    private const string waypointTag = "Checkpoint"; //Waypoint tag
    //Internal Vars
    private int checkpointCount = 0; //Total checkpoint count
    private Vector2 currentHeading;
    private int sortingOrder = 0; // current sprite sorting order
    private int sortingOrderIfGoingUpwards = 0; //if enemy goes up, change sorting layer because background objects go foreground
    private int activeEnemyCount = 0;
    


    //Internal Objects & Components (Will be bound automatically at start() );
    private Transform enemy; //enemy object
    private SpriteRenderer sprite;
    public Transform enemyExit; //Location of exit point
    public GameObject[] waypoints;


    /** INITIALIZE
     * Get required objects, components
     * Verify requirements
     */
    void Start()
    {
        //Prepare Data
        enemy = GetComponent<Transform>(); //Get self transform of enemy
        sprite = GetComponent<SpriteRenderer>();
        
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

        activeEnemyCount = GameManager.instance.ActiveEnemyCount;
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

    /** MOVEMENT
     * Move the enemy towards the next checkpoint
     * Use speed coefficient
     * When last checkpoint is reached, run to exit
     */
    private void enemyMoveController()
    {
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
                GameManager.instance.enemyReachedExit();
                Destroy(gameObject);
                break;
            default:
                //Do nothing
                break;
        }

    }

}
/* //OLD movement controller with fixed step (Might be better for low hardware)
 * 
    //Seconds needed to call movement again. (Iterations per second)
    private float navigationTime = 0f;
    private float navigationUpdateTime = 0.02f; //OBSELETE (movefunc1)
    private void enemyMoveControllerOLD()
    {
        //Check waypoints
        if( waypoints.Length == 0) { return; }

        //Keep track of navigation time by adding time passed to it.
        //If navigationTime is still less than navigationUpdateTime, do not perform Movement
        navigationTime += Time.deltaTime;
        if( navigationTime < navigationUpdateTime)
        {
            //enough time has not yet passed to call another move
            return;
        }
        if( target < checkpointCount)
        {
            //There are still checkpoints to visit
            enemy.position = Vector2.MoveTowards(enemy.position, waypoints[target].transform.position, navigationTime);

            //Save the current heading
            currentHeading = enemy.position - waypoints[target].transform.position;

            //Call directional controller to decide which way enemy is going
            enemyDirectionController();

        } else
        {
            //We have finished the array. GO to exit point
            enemy.position = Vector2.MoveTowards(enemy.position, enemyExit.position, navigationTime);
        }
        //Reset Timer
        navigationTime = 0f;


    }
*/
