using System;
using UnityEngine;
using System.Collections;
public enum projectileType
{
    rock, arrow, fireball
};
public class Projectiles : MonoBehaviour
{
    [Header("Damage of each shot")]
    [SerializeField]
    private int damage;
    [Header("Damage Type")]
    [SerializeField]
    private projectileType dmgType;

    [Header("Speed")]
    [SerializeField]
    [Range(3f, 8f)]
    private float speed = 5f;

    [Header("Screen Removal Time")]
    [SerializeField]
    [Range(0f, 8f)]
    [Space(20)]
    private float removeAfter = 0f;

    public int Damage
    {
        get { return damage; }
    }
    public projectileType DmgType {
        get { return dmgType; }
    }

    public float Speed
    {
        get { return speed; }
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    public void Remove()
    {
        if( gameObject != null)
        {
            StartCoroutine("RemoveProjectile");
        }
    }

    public void multiplyDamage(float multiplier)
    {
        damage = (int)Math.Round(multiplier * (float)damage);
        if(damage < 10) { damage = 10; }
    }

    IEnumerator RemoveProjectile()
    {
        if( removeAfter > 0f)
        {
            yield return new WaitForSeconds(removeAfter);
        }   
        DestroyObject();
        yield return null;
    }
}
