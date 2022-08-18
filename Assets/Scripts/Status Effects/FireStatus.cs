using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStatus : Status
{
    [SerializeField]
    [Tooltip("지속 시간")]
    public float duration = 12; // 화상 효과 지속 시간

    [SerializeField]
    [Tooltip("화상 틱 간격")]
    public float ticTime = 3;

    [SerializeField]
    [Tooltip("매 틱마다 화상 피해 적용")]
    public float ticDamage;

    //Settings
    float lastTic = 0; // 마지막 틱 이후 시간 체크
    float appliedTime = 0; // 화상 지속 시간 체크
    GameObject mTarget; // 화상 영향받는 시간

    public override void Copy(Status copyingGeneric) {
        FireStatus copyingStatus = copyingGeneric as FireStatus;
        duration = copyingStatus.duration;
        ticTime = copyingStatus.ticTime;
        ticDamage = copyingStatus.ticDamage;
    }

    void Start()
    {
        appliedTime = Time.deltaTime;
    }

    void Update()
    {
        Die();
        appliedTime += Time.deltaTime;
        lastTic += Time.deltaTime;
        ApplyTic();
    }

    void ApplyTic() {
        if (lastTic >= ticTime) {
            lastTic = 0;
            float mitigation = mTarget.GetComponent<EnemyController>().FireMitigation;
            float damageTaken = ticDamage * (1 - mitigation);
            mTarget.GetComponent<EnemyController>().Damage(damageTaken);
        }
    }

    public override void Apply(GameObject target, GameObject projectile) {
        mTarget = target;
        float initialDamage = projectile.GetComponent<ProjectileController>().damage;
        float resistance = target.GetComponent<EnemyController>().FireResistance;
        float mitigation = target.GetComponent<EnemyController>().FireMitigation;

        duration *= 1 - resistance;

        float damageTaken = initialDamage * (1 - mitigation);
        target.GetComponent<EnemyController>().Damage(damageTaken);
    }

    /**
     * 효과가 다시 적용될때 호출
     */
    public override void Refresh() { appliedTime = 0; }

    void Die() {
        if (appliedTime >= duration || !mTarget) {
            Destroy(this);
        }
    }
}
