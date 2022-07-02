using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    // �W�����v��.
    [SerializeField] float jumpPower = 20f;
    // ����.
    [SerializeField] float rolePower = 40f;
    // ���W�b�h�{�f�B.
    Rigidbody rigid = null;
    // �U������p�I�u�W�F�N�g.
    [SerializeField] GameObject attackHit = null;
    // �U��Hit�I�u�W�F�N�g��ColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;

    [SerializeField] private Vector3 velocity;              // �ړ�����
    [SerializeField] private float moveSpeed = 5.0f;        // �ړ����x
    [SerializeField] private float applySpeed = 0.2f;       // ��]�̓K�p���x


    [System.Serializable]
    public class Status
    {
        // �̗�.
        public int Hp = 10;
        // �U����.
        public int Power = 1;
    }

    // ��{�X�e�[�^�X.
    [SerializeField] Status DefaultStatus = new Status();
    // ���݂̃X�e�[�^�X.
    public Status CurrentStatus = new Status();
    //! HP�o�[�̃X���C�_�[.
    [SerializeField] Slider hpBar = null;
    //! HP�o�[�̃T�u�X���C�_�[.
    [SerializeField] Slider subHpBar = null;

    // �ڒn�t���O.
    public bool isGround = true;
    //! �U���A�j���[�V�������t���O.
    public bool isAttack { get => PAC.isAttackAni; set => PAC.isAttackAni = value; }
    // PC�L�[����������.
    public float horizontalKeyInput = 0;
    // PC�L�[�c��������.
    public float verticalKeyInput = 0;

    PlayerAnimationController PAC = null;
    [SerializeField] GameController GC = null;

    // ���g�̃R���C�_�[.
    [SerializeField] Collider myCollider = null;
    // �U����H������Ƃ��̃p�[�e�B�N���v���n�u.
    [SerializeField] GameObject hitParticlePrefab = null;
    // �p�[�e�B�N���I�u�W�F�N�g�ۊǗp���X�g.
    public List<GameObject> particleObjectList = new List<GameObject>();
    //! �Q�[���I�[�o�[���C�x���g.
    public UnityEvent GameOverEvent = new UnityEvent();
    // �J�n���ʒu.
    Vector3 startPosition = new Vector3();
    // �J�n���p�x.
    Quaternion startRotation = new Quaternion();

    Vector3 cameraMove1;

    [SerializeField] TMPro.TextMeshProUGUI HP = null;


    void Start()
        {

        PAC = GetComponentInChildren<PlayerAnimationController>();
        PAC.ATNow = attackHitCol;
        PAC.ATEnd = attackEndCol;
        // Rigidbody�̎擾.
        rigid = GetComponent<Rigidbody>();
        // �U������p�R���C�_�[�C�x���g�o�^.
        attackHitCall.TriggerEnterEvent.AddListener(OnAttackHitTriggerEnter);


        // ���݂̃X�e�[�^�X�̏�����.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // HP�X���C�_�[��������.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
        subHpBar.maxValue = DefaultStatus.Hp;
        subHpBar.value = CurrentStatus.Hp;
        // �U������p�I�u�W�F�N�g���\����.
        attackHit.SetActive(false);

        // �J�n���̈ʒu��]��ۊ�.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        //�e�L�X�g����
        HP.text = CurrentStatus.Hp.ToString() + "/" + DefaultStatus.Hp.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (GC.allControlStop == false)
        {
             // PC�L�[���͎擾.
            horizontalKeyInput = Input.GetAxis("Horizontal");
            verticalKeyInput = Input.GetAxis("Vertical");


             // �E�{�^���������ꂽ�u�ԂɎ��s
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

            // Shift�{�^���������ꂽ�u�ԂɎ��s
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
                        // ���x�x�N�g���̒�����1�b��moveSpeed�����i�ނ悤�ɒ������܂�
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
        // WASD���͂���AXZ����(�����Ȓn��)���ړ��������(velocity)�𓾂܂�
        velocity = Vector3.zero;
        if (Input.GetButton("Vertical"))
            velocity.z = -Input.GetAxisRaw("Vertical");
        if (Input.GetButton("Horizontal"))
            velocity.x = -Input.GetAxisRaw("Horizontal");

        Vector3 cameraMove = Camera.main.gameObject.transform.rotation * velocity;
        cameraMove.y = 0f; 

        // ���x�x�N�g���̒�����1�b��moveSpeed�����i�ނ悤�ɒ������܂�
        cameraMove = cameraMove.normalized * moveSpeed * Time.deltaTime;

        // �����ꂩ�̕����Ɉړ����Ă���ꍇ
        if (cameraMove.magnitude > 0)
        {
            // �v���C���[�̈ʒu(transform.position)�̍X�V
            // �ړ������x�N�g��(velocity)�𑫂����݂܂�
            transform.position -= cameraMove;
        }

        // �����ꂩ�̕����Ɉړ����Ă���ꍇ
        if (cameraMove.magnitude > 0)
        {
            // �v���C���[�̉�](transform.rotation)�̍X�V
            // ����]��Ԃ̃v���C���[��Z+����(�㓪��)���A�ړ��̔��Ε���(-velocity)�ɉ񂷉�]�ɒi�X�߂Â��܂�
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(-cameraMove),
                                                  applySpeed);
        }
    }

    //�p�[�e�B�N�������֐�
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
    /// �G�̍U�����q�b�g�����Ƃ��̏���.
    /// </summary>
    /// <param name="damage"> �H������_���[�W. </param>
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
    /// ���S������.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnDie()
    {
        GameOverEvent?.Invoke();
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �p�[�e�B�N�����I��������j������.
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
    /// ���g���C����.
    /// </summary>
    // ---------------------------------------------------------------------
    public void Retry()
    {
        // ���݂̃X�e�[�^�X�̏�����.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // �ʒu��]�������ʒu�ɖ߂�.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;
        // ���݂̃X�e�[�^�X�̏�����.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        hpBar.value = CurrentStatus.Hp;

        //�U�������̓r���ł��ꂽ���p
        isAttack = false;
    }
}
