using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.AI;

public class PlayerAnimationController : PlayerController
{
    // �A�j���[�^�[.
    public Animator animator = null;
    //! �U���A�j���[�V�������t���O.
    public bool isAttackAni = false;
    //! ����A�j���[�V�������t���O.
    public bool isKaihi = false;
    public Action ATNow;
    public Action ATEnd;

    bool isJumping = false;
    bool isCombo = true;
    int  comboLevel = 0;
    public bool isLying = false;




    // Start is called before the first frame update
    void Start()
    {
        // Animator���擾���ۊ�.
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // �E�{�^���������ꂽ�u�ԂɎ��s
        if (Input.GetMouseButtonDown(0))
        {
           if(isCombo == true)
            {
                // Animation��isAttack�g���K�[���N��.
                animator.SetTrigger("isAttack");
                // �U���J�n.
                isAttackAni = true;
                isCombo = false;
                comboLevel += 1;
            }
            
        }

        // Shift�{�^���������ꂽ�u�ԂɎ��s
        if (Input.GetButtonDown("roling"))
        {
            if (isKaihi == false && isLying == false)
            {
                if(isKaihi == false)
                animator.SetTrigger("isKaihi");
                // ����J�n.

            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (animator.GetBool("isGround") == true)
            {
                animator.SetBool("isGround", false);
            }
        }
        if(isLying == true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("rise");
            }
        }

    }

    void FixedUpdate()
    {
        // PC�L�[���͎擾.
        horizontalKeyInput = Input.GetAxisRaw("Horizontal");
        verticalKeyInput = Input.GetAxisRaw("Vertical");


        bool isKeyInput = horizontalKeyInput > 0.1f || verticalKeyInput > 0.1f || horizontalKeyInput < -0.1f || verticalKeyInput < -0.1f; ;
        if (isKeyInput == true && isAttackAni == false)
        {
            animator.SetBool("isRun", true);
            animator.SetBool("isIdle", false);
        }
        else
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isIdle", true);
        }
    }
    //�A�^�b�N�A�j���[�V�����V�X�e��

    public void Anim_AttackAnimStart()
    {
        isKaihi = true;
        isAttackAni = true;

    }
    public void Anim_AttackHit()
    {
        ATNow.Invoke();
    }

    public void Anim_AttackEnd()
    {

        ATEnd.Invoke();
    }

    public void Anim_AttackAnimEnd()
    {
        Debug.Log("end");
        // �U���I��.
        isAttackAni = false;
        StartCoroutine(DelayCoroutine(0.2f));
        isCombo = true;
        comboLevel = 0;
    }

    public void Anim_JumpEnd()
    {
        // �W�����v�I��.
        animator.SetBool("isGround", true);
    }

    public void Anim_RoleStart()
    {
        // ����J�n.
        isKaihi = true;
        isAttackAni = false;
    }
    public void Anim_RoleEnd()
    {
        // ����I��.
        isKaihi = false;
        comboLevel = 0;
        isCombo = true;
    }



    public void Anim_ComboStart()
    {
        isCombo = true;
        isKaihi = false;
    }

    public void Anim_ComboEnd()
    {
        isCombo = false;
    }

    // �҂���
    private IEnumerator DelayCoroutine(float waittime)
    {
        // waittime�ԑ҂�
        yield return new WaitForSeconds(waittime);
    }

    public void hitAttack(GameObject attackScale)
    {
        if(attackScale.gameObject.tag == "BigAttack")
        {
            animator.SetTrigger("isBigAttack");
        }
    }

    public void Anim_Lying()
    {
        isLying = true;
        isCombo = false;
    }

    public void Anim_RiseEnd()
    {
        isLying = false;
        isCombo = true;
        isKaihi = false;
        isAttackAni = false;
    }
}