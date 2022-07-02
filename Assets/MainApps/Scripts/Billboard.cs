using UnityEngine;

public class Billboard : MonoBehaviour
{
    //! �^�[�Q�b�g�J����.
    [SerializeField] Camera lookCamera = null;
    //! Y���݂̂̉�]�ɂ���t���O.
    [SerializeField] bool isY = false;


    void Start()
    {
        if (lookCamera == null) lookCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (lookCamera == null) return;

        // Y����]�̂�.
        if (isY == true)
        {
            var cameraPos = lookCamera.transform.position;
            cameraPos.y = this.transform.position.y;
            var look = this.transform.position - cameraPos;

            this.transform.forward = look;
        }
        // ���S�ɐ��ʂ��J�����Ɍ�����.
        else
        {
            var cameraPos = lookCamera.transform.position;
            var look = this.transform.position - cameraPos;

            this.transform.forward = look;
        }
    }
}
