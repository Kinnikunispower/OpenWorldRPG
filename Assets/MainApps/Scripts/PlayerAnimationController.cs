using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.AI;

public class PlayerAnimationController : PlayerController
{
    // アニメーター.
    public Animator animator = null;
    //! 攻撃アニメーション中フラグ.
    public bool isAttackAni = false;
    //! 回避アニメーション中フラグ.
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
        // Animatorを取得し保管.
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 右ボタンが押された瞬間に実行
        if (Input.GetMouseButtonDown(0))
        {
           if(isCombo == true)
            {
                // AnimationのisAttackトリガーを起動.
                animator.SetTrigger("isAttack");
                // 攻撃開始.
                isAttackAni = true;
                isCombo = false;
                comboLevel += 1;
            }
            
        }

        // Shiftボタンが押された瞬間に実行
        if (Input.GetButtonDown("roling"))
        {
            if (isKaihi == false && isLying == false)
            {
                if(isKaihi == false)
                animator.SetTrigger("isKaihi");
                // 回避開始.

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
        // PCキー入力取得.
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
    //アタックアニメーションシステム

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
        // 攻撃終了.
        isAttackAni = false;
        StartCoroutine(DelayCoroutine(0.2f));
        isCombo = true;
        comboLevel = 0;
    }

    public void Anim_JumpEnd()
    {
        // ジャンプ終了.
        animator.SetBool("isGround", true);
    }

    public void Anim_RoleStart()
    {
        // 回避開始.
        isKaihi = true;
        isAttackAni = false;
    }
    public void Anim_RoleEnd()
    {
        // 回避終了.
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

    // 待つ処理
    private IEnumerator DelayCoroutine(float waittime)
    {
        // waittime間待つ
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