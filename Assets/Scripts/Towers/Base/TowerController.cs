using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour //ISelectHandler, IDeselectHandler
{
    [SerializeField]
    [Tooltip("target Layer")]
    LayerMask targetableLayer;

    [SerializeField]
    [Tooltip("Cooldown shots")]
    float cooldown;

    [SerializeField]
    [Tooltip("타우 발사체")]
    GameObject gProjectile;

    [SerializeField]
    [Tooltip("발사체 생성 위치")]
    GameObject projectileSpawn;

    [SerializeField]
    [Tooltip("string tower type")]
    string towerType;


    [SerializeField] bool rotateTowardEnemy = false;
    [SerializeField] float rotationSpeed = 1000F;
    [SerializeField] GameObject towerWepon = null;
    [SerializeField] public string formattedName;
    [SerializeField] public string nextTowerCost;

    //[SerializeField] GameObject _GM;

    // tower settings
    private GameObject mTarget; // current target
    float shotCooldown = 0;

    // shooting sfx
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        //_GM = GameObject.FindGameObjectWithTag("_GM");
    }

    void Update()
    {
        UpdateTimers();
        UpdateWeponRotation();
        Attack();
    }

    void UpdateWeponRotation()
    {
        if (rotateTowardEnemy && towerWepon != null)
        {
            if (mTarget != null)
            {
                Vector3 lookAt = mTarget.transform.position - towerWepon.transform.position;
                lookAt.y = 0;
                towerWepon.transform.rotation = Quaternion.RotateTowards(towerWepon.transform.rotation, Quaternion.LookRotation(lookAt), rotationSpeed * Time.deltaTime);
            }
        }
    }

    void SetTarget(GameObject newTarget) {
        mTarget = newTarget;
    }

     // Collider Event Listener
    void OnTriggerEnter(Collider collision) {
        // currently no target and valid target enters detection
        // set colider as the new target
        if((targetableLayer & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer && !mTarget) {
            SetTarget(collision.gameObject);
        }
    }

    void OnTriggerExit(Collider collision) {
        // 표적 탐지
        if (collision.gameObject == mTarget) {
            mTarget = null;
        }
    }

    void OnTriggerStay(Collider collision) {
        if ((targetableLayer & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer && !mTarget) {
            SetTarget(collision.gameObject);
        }
    }

    void UpdateTimers() {
        if(shotCooldown >= 0) {
            shotCooldown -= Time.deltaTime;
        }
    }

    void Attack() {
        if (mTarget && shotCooldown <= 0) {
            shotCooldown = cooldown;
            SpawnProjectile();
            audioSource.Play();
        }
    }

    void SpawnProjectile() {
        GameObject shot = Instantiate(gProjectile) as GameObject;
        shot.GetComponent<ProjectileController>().SetSpawn(projectileSpawn.transform.position);
        shot.GetComponent<ProjectileController>().SetTarget(mTarget);
    }

    public string GetTowerType() { return towerType; }
    public void SetTowerType(string type) { towerType = type; }

    //public void OnSelect(BaseEventData eventData)
    //{
    //    transform.GetComponent<Outline>().enabled = true;

    //    if (_GM != null)
    //    {
    //        _GM.GetComponent<TowerBuildingManager>().SpawnMenu(transform.gameObject);
    //    }
    //}

    //public void OnDeselect(BaseEventData eventData)
    //{
    //    transform.GetComponent<Outline>().enabled = false;

    //    if(_GM != null)
    //    {
    //        _GM.GetComponent<TowerBuildingManager>().DeSpawnMenu();
    //    }
    //}
}
