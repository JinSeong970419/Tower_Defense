using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureStatus : Status {
    [SerializeField]
    [Tooltip("지속 시간")]
    public float duration = 12; // how long the effect lasts

    [SerializeField]
    [Tooltip("공격 속도")]
    public float ticTime = 3;

    [SerializeField]
    public float ticDamage;

    //Settings
    float lastTic = 0;
    float appliedTime = 0;
    GameObject mTarget;

    public override void Copy(Status copyingGeneric) {
        NatureStatus copyingStatus = copyingGeneric as NatureStatus;
        duration = copyingStatus.duration;
        ticTime = copyingStatus.ticTime;
        ticDamage = copyingStatus.ticDamage;
    }

    void Start() {
        appliedTime = Time.deltaTime;
    }

    void Update() {
        Die();
        appliedTime += Time.deltaTime;
        lastTic += Time.deltaTime;
        ApplyTic();
    }

    void ApplyTic() {
        if (lastTic >= ticTime) {
            lastTic = 0;
            float mitigation = mTarget.GetComponent<EnemyController>().NatureMitigation;
            float damageTaken = ticDamage * (1 - mitigation);
            mTarget.GetComponent<EnemyController>().Damage(damageTaken);
        }
    }

    public override void Apply(GameObject target, GameObject projectile) {
        mTarget = target;
        float initialDamage = projectile.GetComponent<ProjectileController>().damage;
        float resistance = target.GetComponent<EnemyController>().NatureResistance;
        float mitigation = target.GetComponent<EnemyController>().NatureMitigation;

        duration *= 1 - resistance;

        float damageTaken = initialDamage * (1 - mitigation);
        target.GetComponent<EnemyController>().Damage(damageTaken);
    }

    public override void Refresh() {
        appliedTime = 0;
    }

    void Die() {
        if (appliedTime >= duration || !mTarget) {
            Destroy(this);
        }
    }
}
