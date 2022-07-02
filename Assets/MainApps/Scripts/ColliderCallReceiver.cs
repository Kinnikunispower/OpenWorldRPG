using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// -------------------------------------------------------------------------
/// <summary>
/// �R���C�_�[�R�[���o�b�N�̎�M���[�e�B���e�B�N���X.
/// </summary>
// -------------------------------------------------------------------------
public class ColliderCallReceiver : MonoBehaviour
{
    // �g���K�[�C�x���g��`�N���X.
    public class TriggerEvent : UnityEvent<Collider> { }
    // �g���K�[�G���^�[�C�x���g.
    public TriggerEvent TriggerEnterEvent = new TriggerEvent();
    // �g���K�[�X�e�C�C�x���g.
    public TriggerEvent TriggerStayEvent = new TriggerEvent();
    // �g���K�[�C�O�W�b�g�C�x���g.
    public TriggerEvent TriggerExitEvent = new TriggerEvent();

    void Start()
    {

    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// �g���K�[�G���^�[�R�[���o�b�N.
    /// </summary>
    /// <param name="other"> �ڐG�����R���C�_�[. </param>
    // -------------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        TriggerEnterEvent?.Invoke(other);
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// �g���K�[�X�e�C�R�[���o�b�N.
    /// </summary>
    /// <param name="other"> �ڐG�����R���C�_�[. </param>
    // -------------------------------------------------------------------------
    void OnTriggerStay(Collider other)
    {
        TriggerStayEvent?.Invoke(other);
    }

    // -------------------------------------------------------------------------
    /// <summary>
    /// �g���K�[�C�O�W�b�g�R�[���o�b�N.
    /// </summary>
    /// <param name="other"> �ڐG�����R���C�_�[. </param>
    // -------------------------------------------------------------------------
    void OnTriggerExit(Collider other)
    {
        TriggerExitEvent?.Invoke(other);
    }
}
