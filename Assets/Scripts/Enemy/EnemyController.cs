using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour {

    [SerializeField]
    [Tooltip("ExplosionPrefab")]
    public GameObject explosion;

    [SerializeField]
    [Tooltip("Start Health")]
    float health;

    [SerializeField]
    [Tooltip("Speed")]
    public float speed = 5.0f;

    [SerializeField]
    [Tooltip("Rotates Speed")]
    public float angularSpeed = 450.0f;

    [SerializeField]
    [Tooltip("Next position")]
    Transform targetWaypoint;

    // raw
    [SerializeField]
    [Tooltip("Raw Resistance Damag [0-1]")]
    public float RawMitigation = 0;

    // fire
    [SerializeField]
    [Tooltip("Fire Resist [0-1]")]
    public float FireResistance = 0;

    [SerializeField]
    [Tooltip("Mitigation Fire [0-1]")]
    public float FireMitigation = 0;

    // ice
    [SerializeField]
    [Tooltip("Ice Resist [0-1]")]
    public float IceResistance = 0;

    [SerializeField]
    [Tooltip("Mitigation Ice")]
    public float IceMitigation = 0;

    // nature 
    [SerializeField]
    [Tooltip("Resist Nature [0-1]")]
    public float NatureResistance = 0;

    [SerializeField]
    [Tooltip("Mitigation Nature [0-1]")]
    public float NatureMitigation = 0;

    [SerializeField]
    [Tooltip("enemy type")]
    public string enemyType;

    [SerializeField]
    [Tooltip("gold per kill")]
    public int reward = 10;

    //private List<Status> statusEffects = new List<Status>();

    private int waypointIndex = 0;

    //Health Bar UI
    [SerializeField]
    public Image healthBar;
    private float initialHealth;

    void Start()
    {
        // compensation
        reward = RevenueTemplate.revenueDictMonsters[enemyType];

        // target waypoint
        targetWaypoint = FindObjectOfType<Waypoints>().GetNextWaypoint(waypointIndex);
        waypointIndex++;

        // 초기 체력 bar
        healthBar.fillAmount = 1;
        initialHealth = health;
    }

    void Update()
    {
        Die();
        Move();
    }
    
    void Move() 
    {
        //transform.position += Vector3.left * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, targetWaypoint.position) <= 0.7f)
        {
            targetWaypoint = FindObjectOfType<Waypoints>().GetNextWaypoint(waypointIndex);
            if (targetWaypoint == null)
            {
                GetComponentInParent<EnemySpawnerController>().ReachedEnd();
                Kill();
                return;
            }
            waypointIndex++;
        }

        Vector3 direction = targetWaypoint.position - transform.position;
        transform.position += direction.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), angularSpeed * Time.deltaTime);
    }

    public void Damage(float damage) {
        health -= damage;
        float healthCalc = health / initialHealth;
        healthBar.fillAmount = healthCalc;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void ApplyStatus(GameObject projectile, Status status) {
        Type type = status.GetType();
        Status alreadyApplied = gameObject.GetComponent(type) as Status;

        // New Status
        if (!alreadyApplied) {
            Status appliedStatus = gameObject.AddComponent(type) as Status;
            appliedStatus.Copy(status);
            appliedStatus.Apply(gameObject, projectile);
        }

        // 현재 적용
        else {
            alreadyApplied.Refresh();
            alreadyApplied.Apply(gameObject, projectile);
        }
    }

    void MakeExplode() 
    {
        // Explosions make me happy =D
        if (explosion != null)
            Instantiate(explosion, transform.position, transform.rotation);
    }

    public void Kill()
    {
        MakeExplode();
        Destroy(gameObject, 0);
    }

    /**
     * monster 사망 확인
     */
    void Die() {
        if(health <= 0) {
            GameController.EnemyKilled(reward);
            Kill();
        }
    }
}
