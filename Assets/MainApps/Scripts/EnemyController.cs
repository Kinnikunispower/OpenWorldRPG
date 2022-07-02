using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // 敵の移動イベント定義クラス.
    public class EnemyMoveEvent : UnityEvent<EnemyController> { }
    // 目的地設定イベント.
    public EnemyMoveEvent ArrivalEvent = new EnemyMoveEvent();
    // ナビメッシュ.
    NavMeshAgent navMeshAgent = null;
    // 現在設定されている目的地.
    Transform currentTarget = null;

    // ----------------------------------------------------------
    /// <summary>
    /// ステータス.
    /// </summary>
    // ----------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // HP.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }

    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();
    //! HPバーのスライダー.
    [SerializeField] public Slider hpBar = null;
    // HPバー.
    [SerializeField] GameObject HPBar = null;

    // アニメーター.
    public Animator animator = null;

    // 周辺レーダーコライダーコール.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;
    //! 攻撃判定用コライダーコール.
    [SerializeField] ColliderCallReceiver attackHitColliderCall = null;
    //! 近接攻撃判定用コライダーコール.
    [SerializeField] ColliderCallReceiver closeAttackAroundColliderCall = null;
    // 攻撃間隔.
    [SerializeField] float attackInterval = 3f;
    //死亡状態フラグ
    bool die = false;
    // 攻撃時間計測用.
    float attackTimer = 0f;
    //PlayerController呼び出し
    [SerializeField] PlayerController PC = null;

    // 戦闘状態フラグ.
    public bool IsBattle = false;
    //近接コライダー侵入フラグ
    bool isCloseAttack = false;
    //向き変えてるフラグ
    bool isRoteting = false;

    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();
    //回転前向き
    Vector3 beforeForward1;

   //ゆっくり向け変えるのに必要
   [SerializeField] float speed = 4;

    // パーティクルプレハブ.
    [SerializeField] GameObject diePrefab = null;
    [SerializeField] GameObject burast = null;

    void Start()
    {
        // 最初に現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;

        // Animatorを取得し保管.
        animator = GetComponent<Animator>();
        // 周辺コライダーイベント登録.
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);
        // 攻撃コライダーイベント登録.
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);
        attackHitColliderCall.gameObject.SetActive(false);
        // 近接攻撃コライダーイベント登録.
        closeAttackAroundColliderCall.TriggerEnterEvent.AddListener(OnCloseAttackTriggerEnter);
        closeAttackAroundColliderCall.TriggerStayEvent.AddListener(OnCloseAttackTriggerStay);
        closeAttackAroundColliderCall.TriggerExitEvent.AddListener(OnCloseAttackTriggerExit);

        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        animator.SetBool("isAttacking", false);
    }
    void Update()
    {
        // 攻撃できる状態の時.
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

    //パーティクル生成関数
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
    /// 攻撃コライダーエンターイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
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
    /// 死亡時コール.
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
    /// 死亡アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_DieEnd()
    {
        ParticleSystem(diePrefab, this.transform.position, 2.0f);
        this.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダーエンターイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
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
    /// 周辺レーダーコライダーステイイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
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
    /// 周辺レーダーコライダー終了イベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
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
    /// 攻撃Hitアニメーションコール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
        ParticleSystem(burast, attackHitColliderCall.transform.position, 0);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーション終了時コール.
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
    /// プレイヤーリトライ時の処理.
    /// </summary>
    // ----------------------------------------------------------
    public void OnRetry()
    {
        // 現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        hpBar.value = CurrentStatus.Hp;
        // 開始時の位置回転を保管.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        //敵を再度表示
        this.gameObject.SetActive(true);
        HPBar.SetActive(false);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// ナビメッシュの次の目的地を設定.
    /// </summary>
    /// <param name="target"> 目的地トランスフォーム. </param>
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
    /// 近接攻撃コライダーイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
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
