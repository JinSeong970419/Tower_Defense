using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceStatus : Status {
    [SerializeField]
    [Tooltip("지속 시간")]
    public float duration = 12;

    [SerializeField]
    [Tooltip("Slow")]
    public float slowPercent = 10;

    //Settings
    float appliedTime = 0;
    GameObject mTarget;
    float originalSpeed = 0;

    public override void Copy(Status copyingGeneric) {
        IceStatus copyingStatus = copyingGeneric as IceStatus;
        duration = copyingStatus.duration;
        slowPercent = copyingStatus.slowPercent;
    }

    void Start() {
        appliedTime = Time.deltaTime;
    }

    void Update() {
        Die();
        appliedTime += Time.deltaTime;
    }

    public override void Apply(GameObject target, GameObject projectile) {
        mTarget = target;
        float initialDamage = projectile.GetComponent<ProjectileController>().damage;
        float resistance = target.GetComponent<EnemyController>().IceResistance;
        float mitigation = target.GetComponent<EnemyController>().IceMitigation;

        // 슬로우 중첩 방지
        if (originalSpeed == 0) {
            originalSpeed = target.GetComponent<EnemyController>().speed;
            target.GetComponent<EnemyController>().speed -= originalSpeed * ((slowPercent - mitigation) / 100);
        }

        duration *= 1 - resistance;

        float damageTaken = initialDamage * (1 - mitigation);
        target.GetComponent<EnemyController>().Damage(damageTaken);
    }

    public override void Refresh() {
        appliedTime = 0;
    }

    void Die() {
        if (appliedTime >= duration || !mTarget) {
            mTarget.GetComponent<EnemyController>().speed = originalSpeed;
            Destroy(this);
        }
    }
}
