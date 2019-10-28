using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class TowerManager : MonoBehaviour
{
    private TowerBtn[] towerButtons;
    private TowerBtn towerBtnPressed;
    private bool towerSelected = false;
    private string buildSiteTag = "BuildSite";
    private SpriteRenderer spriteRenderer;
    private Sprite towerSprite;
    
    
    
    

    private void Awake()
    {
        //Get Button Objects
        towerButtons = FindObjectsOfType<TowerBtn>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if( spriteRenderer == null)
        {
            Debug.LogError("Couldn't find spriteRenderer for TowerManager");
        }
        if( towerButtons.Length == 0 )
        {
            Debug.LogError("Couldn't find tower UI buttons for TowerManager");
        }

    }

    /**
     * Will be called from buttons. OnClick event
     * The btn script will be passed
     */
    public void SelectedTower(TowerBtn btn)
    {
        //Remove Outline from all buttons
        for (int i = 0; i < towerButtons.Length; i++)
        {
            towerButtons[i].GetComponent<Outline>().enabled = false;
        }
        //Remove object if the same object is being pressed again
        if ( btn == towerBtnPressed)
        {
            //Same button is pressed. Cancel
            towerBtnPressed = null;
        } else
        {
            //Select new object and highlight
            towerBtnPressed = btn;
            btn.GetComponent<Outline>().enabled = true;
            towerSelected = true;
        }

        //Select sprite for the tower that we are about to place
        if( towerBtnPressed != null)
        {
            towerSprite = towerBtnPressed.TowerObject.GetComponent<SpriteRenderer>().sprite;
        } else
        {
            towerSprite = null;
        }

    }

    //Remove selected btn
    public void UnselectTower()
    {
        towerSelected = false;
        towerBtnPressed.GetComponent<Outline>().enabled = false;
        towerBtnPressed = null;
        towerSprite = null;
    }

    public void PlaceTower(Vector2 position)
    {
        if (towerSelected == false || towerBtnPressed == null || towerBtnPressed.TowerObject == null || position == null)
        {
            //Nothing to place
            return;
        }

        bool towerPlacedOk = false; //State of the tower

        
        GameObject newTower = Instantiate(towerBtnPressed.TowerObject); //Tower prefab -> scene object
            
         
        
        
        //Calculate where we've hit when we've clicked
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero );
        if (hit)
        {
            if(hit.collider != null && hit.collider.tag != null && hit.collider.tag  == buildSiteTag) //if we've hit a buildSite
            {
                Transform buildSiteTransform = hit.collider.gameObject.GetComponent<Transform>();
                if( buildSiteTransform != null)
                {
                    newTower.transform.position = buildSiteTransform.position;
                    towerPlacedOk = true; //Tower is perfectly placed.
                }
            }
        }
        if( !towerPlacedOk)
        {
            //There was a problem with the tower placement
            Destroy(newTower);
            
        } else
        {
            //Place the tower. We can unselect
            UnselectTower();
        }
        
        


        
    }


    void ControlTowerPlacement()
    {
        //Where to place the tower
        Vector2 worldPoint = new Vector2(0f, 0f);

        if (Input.touchSupported)
        {
            if (Input.touchCount == 1)
            {
                worldPoint = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                PlaceTower(worldPoint);
            }

        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                PlaceTower(worldPoint);

            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PlaceTower(worldPoint);
        }
    }

    void MouseFollow()
    {
        if (Input.touchSupported == false)
        {
            if (towerSprite != null)
            {
                spriteRenderer.sprite = towerSprite;
                transform.position = new Vector2(0f, 0f);

                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = mousePosition;

                //Check if placeable
                RaycastHit2D pos = Physics2D.Raycast(mousePosition, Vector2.zero);
                if(pos.collider != null && pos.collider.tag != null && pos.collider.tag == buildSiteTag)
                {
                    transform.position = pos.collider.gameObject.transform.position;
                    spriteRenderer.color = new Color(0.01f, 0.92f, 0.03f, 0.8f);
                } else
                {
                    spriteRenderer.color = new Color(1f, 0.3f, 0.01f, 0.2f);
                }

            }
            else
            {
                spriteRenderer.sprite = null;
            }
        }
    }
    private void Update()
    {
        ControlTowerPlacement();
        MouseFollow(); //Only works on touch disabled device
        
        
        
        
    }

    private void Start()
    {
        
    }


}
