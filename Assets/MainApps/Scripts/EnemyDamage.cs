using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    // 攻撃HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    //! 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    //! 攻撃ヒット時エフェクトプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;

    EnemyController EC = null;
    [SerializeField] PlayerController PC = null;

    // Start is called before the first frame update
    void Start()
    {
        // 攻撃判定用コライダーイベント登録.
        attackHitCall.TriggerEnterEvent.AddListener(OnPlayerAttackHitTriggerEnter);

        EC = GetComponentInParent<EnemyController>();
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃ヒット時コール.
    /// </summary>
    /// <param name="damage"> 食らったダメージ. </param>
    // ----------------------------------------------------------
    public int OnAttackHit(int damage, int currentHp, Vector3 attackPosition)
    {
        Debug.Log(currentHp);
        currentHp -= damage;
        EC.ParticleSystem(hitParticlePrefab, myCollider.ClosestPoint(attackPosition),0);
        return  currentHp;
    }

    void OnPlayerAttackHitTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "PlayerAttack")
        {
            int currentEnemyHp = OnAttackHit(PC.CurrentStatus.Power, EC.CurrentStatus.Hp, this.transform.position);
            EC.CurrentStatus.Hp = currentEnemyHp;
            EC.hpBar.value = EC.CurrentStatus.Hp;

            if (EC.CurrentStatus.Hp <= 0)
            {
                EC.OnDie();
            }
            else if(EC.animator.GetBool("isAttacking") == false)
            {
                EC.animator.SetTrigger("isHit");
            }

        }
    }
}
