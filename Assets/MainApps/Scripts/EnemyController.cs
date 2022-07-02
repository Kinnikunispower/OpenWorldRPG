using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // �G�̈ړ��C�x���g��`�N���X.
    public class EnemyMoveEvent : UnityEvent<EnemyController> { }
    // �ړI�n�ݒ�C�x���g.
    public EnemyMoveEvent ArrivalEvent = new EnemyMoveEvent();
    // �i�r���b�V��.
    NavMeshAgent navMeshAgent = null;
    // ���ݐݒ肳��Ă���ړI�n.
    Transform currentTarget = null;

    // ----------------------------------------------------------
    /// <summary>
    /// �X�e�[�^�X.
    /// </summary>
    // ----------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // HP.
        public int Hp = 10;
        // �U����.
        public int Power = 1;
    }

    // ��{�X�e�[�^�X.
    [SerializeField] Status DefaultStatus = new Status();
    // ���݂̃X�e�[�^�X.
    public Status CurrentStatus = new Status();
    //! HP�o�[�̃X���C�_�[.
    [SerializeField] public Slider hpBar = null;
    // HP�o�[.
    [SerializeField] GameObject HPBar = null;

    // �A�j���[�^�[.
    public Animator animator = null;

    // ���Ӄ��[�_�[�R���C�_�[�R�[��.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;
    //! �U������p�R���C�_�[�R�[��.
    [SerializeField] ColliderCallReceiver attackHitColliderCall = null;
    //! �ߐڍU������p�R���C�_�[�R�[��.
    [SerializeField] ColliderCallReceiver closeAttackAroundColliderCall = null;
    // �U���Ԋu.
    [SerializeField] float attackInterval = 3f;
    //���S��ԃt���O
    bool die = false;
    // �U�����Ԍv���p.
    float attackTimer = 0f;
    //PlayerController�Ăяo��
    [SerializeField] PlayerController PC = null;

    // �퓬��ԃt���O.
    public bool IsBattle = false;
    //�ߐڃR���C�_�[�N���t���O
    bool isCloseAttack = false;
    //�����ς��Ă�t���O
    bool isRoteting = false;

    // �J�n���ʒu.
    Vector3 startPosition = new Vector3();
    // �J�n���p�x.
    Quaternion startRotation = new Quaternion();
    //��]�O����
    Vector3 beforeForward1;

   //�����������ς���̂ɕK�v
   [SerializeField] float speed = 4;

    // �p�[�e�B�N���v���n�u.
    [SerializeField] GameObject diePrefab = null;
    [SerializeField] GameObject burast = null;

    void Start()
    {
        // �ŏ��Ɍ��݂̃X�e�[�^�X����{�X�e�[�^�X�Ƃ��Đݒ�.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // �X���C�_�[��������.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;

        // Animator���擾���ۊ�.
        animator = GetComponent<Animator>();
        // ���ӃR���C�_�[�C�x���g�o�^.
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);
        // �U���R���C�_�[�C�x���g�o�^.
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);
        attackHitColliderCall.gameObject.SetActive(false);
        // �ߐڍU���R���C�_�[�C�x���g�o�^.
        closeAttackAroundColliderCall.TriggerEnterEvent.AddListener(OnCloseAttackTriggerEnter);
        closeAttackAroundColliderCall.TriggerStayEvent.AddListener(OnCloseAttackTriggerStay);
        closeAttackAroundColliderCall.TriggerExitEvent.AddListener(OnCloseAttackTriggerExit);

        // �J�n���̈ʒu��]��ۊ�.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        animator.SetBool("isAttacking", false);
    }
    void Update()
    {
        // �U���ł����Ԃ̎�.
        if (IsBattle == true)
        {
            attackTimer += Time.deltaTime;
        }
        else
        {
            attackTimer = 0;

            if (currentTarget == null)
            {
                //animator.SetBool("isRun", false);

                ArrivalEvent?.Invoke(this);
            }
            else
            {
                animator.SetBool("isRun", true);

                var sqrDistance = (currentTarget.position - this.transform.position).sqrMagnitude;              
                    if (sqrDistance < 20f)
                    {
                        ArrivalEvent?.Invoke(this);
                    }               
            }
        }

    }

    //�p�[�e�B�N�������֐�
    public void ParticleSystem(GameObject particle, Vector3 position, float ajast)
    {
        var pos = position;
        pos.y += ajast; 
        var obj = Instantiate(particle, pos, Quaternion.identity);
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine(WaitDestroy(par));
    }

    IEnumerator WaitDestroy(ParticleSystem particle)
    {
        yield return new WaitUntil(() => particle.isPlaying == false);
        Destroy(particle.gameObject);
    }

    // ------------------------------------------------------------
    /// <summary>
    /// �U���R���C�_�[�G���^�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAttackTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var player = other.GetComponent<PlayerController>();
            player?.OnEnemyAttackHit(CurrentStatus.Power, attackHitColliderCall.gameObject);
            attackHitColliderCall.gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------
    /// <summary>
    /// ���S���R�[��.
    /// </summary>
    // ----------------------------------------------------------
    public void OnDie()
    {
        animator.SetBool("isDie", true);
        die = true;
        StopAllCoroutines();
        if (PC.particleObjectList.Count > 0)
        {
            foreach (var obj in PC.particleObjectList) Destroy(obj);
            PC.particleObjectList.Clear();
        }
    }

    // ----------------------------------------------------------
    /// <summary>
    /// ���S�A�j���[�V�����I�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_DieEnd()
    {
        ParticleSystem(diePrefab, this.transform.position, 2.0f);
        this.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�G���^�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsBattle = true;
            HPBar.SetActive(true);

            navMeshAgent.SetDestination(PC.transform.position);

        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�X�e�C�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(isCloseAttack == false)
            {
                animator.SetBool("isRun", true);
            }

            if (die == false)
            {
                if (animator.GetBool("isAttacking") == false)
                {
                    Vector3 beforeForward = this.transform.forward;
                    beforeForward1 = beforeForward;
                    Vector3 vector3 = PC.gameObject.transform.position - this.transform.position;
                    vector3.y = 0f;
                    Quaternion quaternion = Quaternion.LookRotation(vector3);
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, quaternion, Time.deltaTime * speed);
                    Debug.Log((this.transform.forward - beforeForward).sqrMagnitude * 10000);
                    if ((this.transform.forward - beforeForward).sqrMagnitude * 10000 > 0.7f)
                    {
                        animator.SetBool("isRun", true);
                    }

                    if(isCloseAttack == false) navMeshAgent.SetDestination(PC.transform.position);
                }
                if(animator.GetBool("isAttacking") == true)
                {
                    navMeshAgent.SetDestination(this.transform.position);
                }
            
            }
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// ���Ӄ��[�_�[�R���C�_�[�I���C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsBattle = false;
            HPBar.SetActive(false);

            currentTarget = null;
        }
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �U��Hit�A�j���[�V�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
        ParticleSystem(burast, attackHitColliderCall.transform.position, 0);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �U���A�j���[�V�����I�����R�[��.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive(false);
    }

    void Anim_AttackFinish()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isRun", true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �v���C���[���g���C���̏���.
    /// </summary>
    // ----------------------------------------------------------
    public void OnRetry()
    {
        // ���݂̃X�e�[�^�X����{�X�e�[�^�X�Ƃ��Đݒ�.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        hpBar.value = CurrentStatus.Hp;
        // �J�n���̈ʒu��]��ۊ�.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        //�G���ēx�\��
        this.gameObject.SetActive(true);
        HPBar.SetActive(false);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// �i�r���b�V���̎��̖ړI�n��ݒ�.
    /// </summary>
    /// <param name="target"> �ړI�n�g�����X�t�H�[��. </param>
    // ----------------------------------------------------------
    public void SetNextTarget(Transform target)
    {
        if (target == null) return;
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.SetDestination(target.position);
        currentTarget = target;
    }

    // ------------------------------------------------------------
    /// <summary>
    /// �ߐڍU���R���C�_�[�C�x���g�R�[��.
    /// </summary>
    /// <param name="other"> �ڋ߃R���C�_�[. </param>
    // ------------------------------------------------------------
    void OnCloseAttackTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            animator.SetBool("isRun", false);
            navMeshAgent.SetDestination(this.transform.position);
            isCloseAttack = true;
        }
    }

    void OnCloseAttackTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (attackTimer >= 7f)
            {
                animator.SetBool("isRun", false);
                animator.SetBool("isAttacking", true);
                navMeshAgent.SetDestination(this.transform.position);

                attackTimer = 0;
            }

            if ((this.transform.forward - beforeForward1).sqrMagnitude * 10000 < 0.05f)
            {
                animator.SetBool("isRun", false);
            }
        }

    }

    void OnCloseAttackTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isCloseAttack = false;
        }
    }
}
