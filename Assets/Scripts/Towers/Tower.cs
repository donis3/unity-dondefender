using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]

    [SerializeField]
    [Tooltip("The distance from which the tower can shoot")]
    [Range(0.1f, 50f)]
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

    private void Start()
    {
        //FindObjectOfType<EnemyManager>()
    }


}
