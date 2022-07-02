using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    // �U��Hit�I�u�W�F�N�g��ColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    //! ���g�̃R���C�_�[.
    [SerializeField] Collider myCollider = null;
    //! �U���q�b�g���G�t�F�N�g�v���n�u.
    [SerializeField] GameObject hitParticlePrefab = null;

    EnemyController EC = null;
    [SerializeField] PlayerController PC = null;

    // Start is called before the first frame update
    void Start()
    {
        // �U������p�R���C�_�[�C�x���g�o�^.
        attackHitCall.TriggerEnterEvent.AddListener(OnPlayerAttackHitTriggerEnter);

        EC = GetComponentInParent<EnemyController>();
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �U���q�b�g���R�[��.
    /// </summary>
    /// <param name="damage"> �H������_���[�W. </param>
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
