using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSite : MonoBehaviour
{
    //Keep track of build site status
    private GameObject tower = null;
    
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
        isBuilt = true;
    }

    public void RemoveTower()
    {
        Destroy(gameObject);
        tower = null;
        isBuilt = false;
    }


}
