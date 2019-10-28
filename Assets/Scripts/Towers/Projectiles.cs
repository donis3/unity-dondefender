using UnityEngine;
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

    public int Damage
    {
        get { return damage; }
    }
    public projectileType DmgType {
        get { return dmgType; }
    }
}
