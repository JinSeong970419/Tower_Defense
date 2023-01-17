using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator anim;

    // 초기화
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Run Forward", true);
    }
}