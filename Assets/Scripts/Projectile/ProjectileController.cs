using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
    [SerializeField]
    [Tooltip("Speed move")]
    float speed;

    [SerializeField] public float damage;

    [SerializeField]
    [Tooltip("가비지 수집")]
    float ttl;
    
    [SerializeField]
    [Tooltip("상태이상 관련")]
    GameObject statusEffect;

    // Settings
    GameObject mTarget;
    float lived = 0;

    void Update()
    {
        Move();
        SelfDestruct();
    }

    // Collider Event Listener
    void OnTriggerEnter(Collider collision) {
        // 적 표적 탐지
        if (collision.gameObject == mTarget) {
            if(statusEffect != null) {
                collision.gameObject.GetComponent<EnemyController>().ApplyStatus(gameObject, statusEffect.GetComponent<Status>());
            } else {
                // base case: 특별한 효과 없음, 대상에 고정 피해
                collision.gameObject.GetComponent<EnemyController>().Damage(damage);
            }
            Destroy(gameObject);
        }
    }

    void Move() {
        transform.position += GetVector() * speed * Time.deltaTime;
    }

    void SelfDestruct() {
        if(!mTarget || lived > ttl) {
            Destroy(gameObject);
        }
    }

    Vector3 GetVector() {
        if (mTarget) {
            return Vector3.Normalize(mTarget.transform.position - transform.position);
        }
        return new Vector3(0,0,0);
    }

    void UpdateTimers() {
        // 타이머 설정
        lived += Time.deltaTime;
    }

    public void SetSpawn(Vector3 _spawn) {
        transform.position = _spawn;
    }

    public void SetTarget(GameObject _target) {
        mTarget = _target;
    }
}
