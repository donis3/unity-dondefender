using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSite : MonoBehaviour
{
    //Keep track of build site status
    private GameObject tower = null;
    private SpriteRenderer towerSprite = null;
    
    public GameObject Tower
    {
        get { return tower; }
    }
    public bool isBuilt = false;
    public int siteIndex = 0;

    public void PlaceTower(GameObject towerObject)
    {
        tower = Instantiate(towerObject);
        tower.transform.position = transform.position;
        towerSprite = tower.GetComponent<SpriteRenderer>();
        isBuilt = true;


        //Find close objects
        List<GameObject> otherTowers = new List<GameObject>();
        Tower[] others = GameObject.FindObjectsOfType<Tower>();

        float minDistance = 1f;
        foreach(Tower other in others)
        {
            if( other.gameObject == tower) { continue; }
            if( Vector2.Distance(transform.position, other.transform.position) < minDistance)
            {
                //Debug.Log("Found another close tower");
                //Found a close object
                if(other.transform.position.y >= transform.position.y)
                {
                    //the close tower is on higer Y (farther away, must be on the background). So increase this towers sorting order
                    towerSprite.sortingOrder += 1;

                } else
                {
                    towerSprite.sortingOrder -= 1;
                }
            }

        }


    }

    public void RemoveTower()
    {
        Destroy(tower.gameObject);
        tower = null;
        towerSprite = null;
        isBuilt = false;
    }


}
