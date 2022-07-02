using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    // ジャンプ力.
    [SerializeField] float jumpPower = 20f;
    // 回避力.
    [SerializeField] float rolePower = 40f;
    // リジッドボディ.
    Rigidbody rigid = null;
    // 攻撃判定用オブジェクト.
    [SerializeField] GameObject attackHit = null;
    // 攻撃HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;

    [SerializeField] private Vector3 velocity;              // 移動方向
    [SerializeField] private float moveSpeed = 5.0f;        // 移動速度
    [SerializeField] private float applySpeed = 0.2f;       // 回転の適用速度


    [System.Serializable]
    public class Status
    {
        // 体力.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }

    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();
    //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;
    //! HPバーのサブスライダー.
    [SerializeField] Slider subHpBar = null;

    // 接地フラグ.
    public bool isGround = true;
    //! 攻撃アニメーション中フラグ.
    public bool isAttack { get => PAC.isAttackAni; set => PAC.isAttackAni = value; }
    // PCキー横方向入力.
    public float horizontalKeyInput = 0;
    // PCキー縦方向入力.
    public float verticalKeyInput = 0;

    PlayerAnimationController PAC = null;
    [SerializeField] GameController GC = null;

    // 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    // 攻撃を食らったときのパーティクルプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;
    // パーティクルオブジェクト保管用リスト.
    public List<GameObject> particleObjectList = new List<GameObject>();
    //! ゲームオーバー時イベント.
    public UnityEvent GameOverEvent = new UnityEvent();
    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();

    Vector3 cameraMove1;

    [SerializeField] TMPro.TextMeshProUGUI HP = null;


    void Start()
        {

        PAC = GetComponentInChildren<PlayerAnimationController>();
        PAC.ATNow = attackHitCol;
        PAC.ATEnd = attackEndCol;
        // Rigidbodyの取得.
        rigid = GetComponent<Rigidbody>();
        // 攻撃判定用コライダーイベント登録.
        attackHitCall.TriggerEnterEvent.AddListener(OnAttackHitTriggerEnter);


        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // HPスライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
        subHpBar.maxValue = DefaultStatus.Hp;
        subHpBar.value = CurrentStatus.Hp;
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive(false);

        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        //テキスト処理
        HP.text = CurrentStatus.Hp.ToString() + "/" + DefaultStatus.Hp.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (GC.allControlStop == false)
        {
             // PCキー入力取得.
            horizontalKeyInput = Input.GetAxis("Horizontal");
            verticalKeyInput = Input.GetAxis("Vertical");


             // 右ボタンが押された瞬間に実行
            if (Input.GetMouseButtonDown(0))
            {
                if (isAttack == false)
                {
                }

                if (isAttack == true)
                {
                }
            }

            if (Input.GetButtonDown("Jump"))
            {
                if (PAC.animator.GetBool("isGround") == true)
                {
                    rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                }
            }

            // Shiftボタンが押された瞬間に実行
            if (Input.GetButtonDown("roling"))
            {
                    if (PAC.isKaihi == false && PAC.isLying == false)
                    {
              
                        velocity = Vector3.zero;
                        if (Input.GetButton("Vertical"))
                            velocity.z = -Input.GetAxisRaw("Vertical");
                        if (Input.GetButton("Horizontal"))
                            velocity.x = -Input.GetAxisRaw("Horizontal");

                        Vector3 cameraMove = Camera.main.gameObject.transform.rotation * velocity;
                        // 速度ベクトルの長さを1秒でmoveSpeedだけ進むように調整します
                        cameraMove = cameraMove.normalized * 200 * Time.deltaTime;
                        transform.position -= cameraMove;
                        rigid.AddForce(cameraMove, ForceMode.Acceleration);

                    }
            }


        }

    }

    void FixedUpdate()
    {
        bool isKeyInput = horizontalKeyInput > 0.1f || verticalKeyInput > 0.1f || horizontalKeyInput < -0.1f || verticalKeyInput < -0.1f; ;
        if (isKeyInput == true && isAttack == false && PAC.isLying == false)
        {
            PlayerMove();
        }
    }

    void PlayerMove()
    {
        // WASD入力から、XZ平面(水平な地面)を移動する方向(velocity)を得ます
        velocity = Vector3.zero;
        if (Input.GetButton("Vertical"))
            velocity.z = -Input.GetAxisRaw("Vertical");
        if (Input.GetButton("Horizontal"))
            velocity.x = -Input.GetAxisRaw("Horizontal");

        Vector3 cameraMove = Camera.main.gameObject.transform.rotation * velocity;
        cameraMove.y = 0f; 

        // 速度ベクトルの長さを1秒でmoveSpeedだけ進むように調整します
        cameraMove = cameraMove.normalized * moveSpeed * Time.deltaTime;

        // いずれかの方向に移動している場合
        if (cameraMove.magnitude > 0)
        {
            // プレイヤーの位置(transform.position)の更新
            // 移動方向ベクトル(velocity)を足し込みます
            transform.position -= cameraMove;
        }

        // いずれかの方向に移動している場合
        if (cameraMove.magnitude > 0)
        {
            // プレイヤーの回転(transform.rotation)の更新
            // 無回転状態のプレイヤーのZ+方向(後頭部)を、移動の反対方向(-velocity)に回す回転に段々近づけます
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(-cameraMove),
                                                  applySpeed);
        }
    }

    //パーティクル生成関数
    public void ParticleSystem(GameObject particle, Vector3 position)
    {
        var pos = position;
        var obj = Instantiate(particle, pos, Quaternion.identity);
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine(WaitDestroy(par));
    }

    void OnAttackHitTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            attackHit.SetActive(false);
        }
    }
    void attackHitCol()
    {
        attackHit.SetActive(true);
    }

    void attackEndCol()
    {
        attackHit.SetActive(false);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 敵の攻撃がヒットしたときの処理.
    /// </summary>
    /// <param name="damage"> 食らったダメージ. </param>
    // ---------------------------------------------------------------------
    public void OnEnemyAttackHit(int damage, GameObject attackCol)
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;
        subHpBar.DOValue(CurrentStatus.Hp, 2.0f);
        ParticleSystem(hitParticlePrefab, this.transform.position);
        HP.text = CurrentStatus.Hp.ToString() + "/" + DefaultStatus.Hp.ToString(); 

        PAC.hitAttack(attackCol);
        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// 死亡時処理.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnDie()
    {
        GameOverEvent?.Invoke();
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// パーティクルが終了したら破棄する.
    /// </summary>
    /// <param name="particle"></param>
    // ---------------------------------------------------------------------
    IEnumerator WaitDestroy(ParticleSystem particle)
    {
        yield return new WaitUntil(() => particle.isPlaying == false);
        Destroy(particle.gameObject);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// リトライ処理.
    /// </summary>
    // ---------------------------------------------------------------------
    public void Retry()
    {
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // 位置回転を初期位置に戻す.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        hpBar.value = CurrentStatus.Hp;

        //攻撃処理の途中でやられた時用
        isAttack = false;
    }
}
