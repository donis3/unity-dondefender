using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class TowerManager : MonoBehaviour
{
    public static TowerManager instance { get; private set; }

    private TowerBtn[] towerButtons;
    private TowerBtn towerBtnPressed;
    private bool towerSelected = false;
    private string buildSiteTag = "BuildSite";
    private string towerTag = "Tower";
    private SpriteRenderer spriteRenderer;
    private Sprite towerSprite;
    private BuildSite[] buildSites;

    private GameObject selectedTowerObject;
    private int towerLayerIndex;
    private int buildSiteLayerIndex;
    private bool mouseFollowActive = false; //Disable other operations while placing tower
    private float lastTowerPlaced;
    
    

    
    
    
    
    

    private void Awake()
    {
        //Instantiate singleton(for level)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //Get Button Objects
        towerButtons = FindObjectsOfType<TowerBtn>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //Get BuildSites
        GameObject[] buildSiteObjects = GameObject.FindGameObjectsWithTag(buildSiteTag);
        
        buildSites = new BuildSite[buildSiteObjects.Length];
        if (buildSites.Length > 0)
        {
            for (int i = 0; i < buildSiteObjects.Length; i++)
            {
                buildSites[i] = buildSiteObjects[i].GetComponent<BuildSite>();
            }
        }

        if( spriteRenderer == null)
        {
            Debug.LogError("Couldn't find spriteRenderer for TowerManager");
        }
        if( towerButtons.Length == 0 )
        {
            Debug.LogError("Couldn't find tower UI buttons for TowerManager");
        }
        if(buildSiteObjects.Length == 0)
        {
            Debug.LogError("Couldn't find any tower-build sites.");
        }

        //Tower layer for selecting tower
        towerLayerIndex = LayerMask.NameToLayer("TowerObjects");
        buildSiteLayerIndex = LayerMask.NameToLayer("Ground");

    }

    private void Update()
    {
        ControlTowerPlacement();
        MouseFollow(); //Only works on touch disabled device

        TowerObjectSelectController();




    }

    private void Start()
    {
        
    }



    /* ======================================= TOWER PLACEMENT BUTTONS AND CONTROLS =============================== */

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
        if (btn == towerBtnPressed)
        {
            //Same button is pressed. Cancel
            towerBtnPressed = null;
        }
        else
        {
            //Check Price
            if(btn.Affordable == false )
            {
                LevelManager.instance.Feedback("Not enough money..");
                return;
            }
            //Select new object and highlight
            towerBtnPressed = btn;
            btn.GetComponent<Outline>().enabled = true;
            towerSelected = true;
        }

        //Select sprite for the tower that we are about to place
        if (towerBtnPressed != null)
        {
            towerSprite = towerBtnPressed.TowerObject.GetComponent<SpriteRenderer>().sprite;
        }
        else
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

    //Create the tower object on the plot
    public void PlaceTower(Vector2 position)
    {
        if (towerSelected == false || towerBtnPressed == null || towerBtnPressed.TowerObject == null || position == null)
        {
            //Nothing to place
            return;
        }

        if( LevelManager.instance.Money < towerBtnPressed.Price)
        {
            //Cant afford
            UnselectTower();
            LevelManager.instance.Feedback("Can't afford this tower");
            return;
        }

        //Calculate where we've hit when we've clicked
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 100000f, 1 << buildSiteLayerIndex);
        if (hit)
        {
            //Find the buildsite component
            BuildSite buildSite = hit.collider.gameObject.GetComponent<BuildSite>();

            if (hit.collider != null && hit.collider.tag != null && hit.collider.tag == buildSiteTag) //if we've hit a buildSite
            {
                if (buildSite.isBuilt == false)
                {
                    //Send buildsite the prefab to place on itself.
                    buildSite.PlaceTower(towerBtnPressed.TowerObject);

                    //Remove the price
                    LevelManager.instance.SpendMoney(towerBtnPressed.Price);
                    lastTowerPlaced = Time.time;
                    UnselectTower(); //unselect the tower button
                }
            }
        }

    }



    //Input listening
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

        
    }

    //Visualize the tower at cursor
    void MouseFollow()
    {
        if (Input.touchSupported == false)
        {
            if (towerSprite != null)
            {
                mouseFollowActive = true;
                spriteRenderer.sprite = towerSprite;
                transform.position = new Vector2(0f, 0f);

                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = mousePosition;

                //Check if placeable
                RaycastHit2D pos = Physics2D.Raycast(mousePosition, Vector2.zero, 100000f, 1 << buildSiteLayerIndex);
                if (pos.collider != null && pos.collider.tag != null && pos.collider.tag == buildSiteTag)
                {
                    BuildSite collidedSite = pos.collider.gameObject.GetComponent<BuildSite>();

                    if (collidedSite.isBuilt == false)
                    {
                        transform.position = pos.collider.gameObject.transform.position;
                        spriteRenderer.color = new Color(0.01f, 0.92f, 0.03f, 0.8f);
                    }
                }
                else
                {
                    spriteRenderer.color = new Color(1f, 0.3f, 0.01f, 0.2f);
                }

            }
            else
            {

                mouseFollowActive = false;
                spriteRenderer.sprite = null;
            }
        }
    }


    /* ======================================= Selectable Tower Objects On Map =============================== */

    private void TowerObjectSelectController()
    {
        if( mouseFollowActive == true ) { return; }

        //Wait before selecting if a tower was just placed
        if(Time.time - lastTowerPlaced < 1f)
        {
            return;
        }


        Vector2 worldPoint;
        if (Input.touchSupported == true && Input.touchCount > 0)
        {
            worldPoint = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            
        } else if( Input.GetMouseButtonDown(0) )
        {
            worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            

        } else
        {   
            //No input
            return;
        }
        //Select tower
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 1000000000000f,1 << towerLayerIndex);
        if(hit.collider != null && hit.collider.tag != null)
        {
            //Debug.Log("Hit: " + hit.collider.gameObject.tag);
            if (hit.collider.tag == towerTag)
            {
                SelectTowerObject(hit.collider.gameObject);
            }else
            {
                DeselectTowerObject();
            }
        }else
        {

            DeselectTowerObject();
        }
        
    }

    /** Select a tower on the map
     * Save the object reference
     * deselect the last selected tower if exists
     */
    private void SelectTowerObject(GameObject Tower)
    {
        //Deselect old tower if exists
        if( selectedTowerObject != null)
        {
            selectedTowerObject.GetComponent<Tower>().DeselectThisTower();
        }
        if (Tower != null)
        {
            selectedTowerObject = Tower;
        }
        selectedTowerObject.GetComponent<Tower>().SelectThisTower();

    }

    /** Deselect a tower on the map (Hide radius visualizer)
     * Call Deselecttower for that tower
     * remove currently selected tower object reference
     */
    private void DeselectTowerObject()
    {
        if(selectedTowerObject != null)
        {
            selectedTowerObject.GetComponent<Tower>().DeselectThisTower();
        }

        selectedTowerObject = null;
    }


}
