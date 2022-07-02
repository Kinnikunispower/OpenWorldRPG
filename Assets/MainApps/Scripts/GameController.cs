using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // �Q�[���I�[�o�[�I�u�W�F�N�g.
    [SerializeField] GameObject gameOver = null;
    // �v���C���[.
    [SerializeField] PlayerController PC = null;
    // �G���X�g.
    [SerializeField] List<EnemyController> enemys = new List<EnemyController>();
    public bool allControlStop;
    // �G�̈ړ��^�[�Q�b�g���X�g.
    [SerializeField] List<Transform> enemyTargets = new List<Transform>();


    void Start()
    {
        PC.GameOverEvent.AddListener(OnGameOver);

        gameOver.SetActive(false);

        foreach (var enemy in enemys)
        {
            enemy.ArrivalEvent.AddListener(EnemyMove);
        }
    }

    public void ShowMessageWindow(string message)
    {
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ���X�g���烉���_���Ƀ^�[�Q�b�g���擾.
    /// </summary>
    /// <returns> �^�[�Q�b�g. </returns>
    // ---------------------------------------------------------------------
    Transform GetEnemyMoveTarget()
    {
        if (enemyTargets == null || enemyTargets.Count == 0) return null;
        else if (enemyTargets.Count == 1) return enemyTargets[0];

        int num = Random.Range(0, enemyTargets.Count);
        return enemyTargets[num];
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �G�Ɏ��̖ړI�n��ݒ�.
    /// </summary>
    /// <param name="enemy"> �G. </param>
    // ---------------------------------------------------------------------
    void EnemyMove(EnemyController enemy)
    {
        var target = GetEnemyMoveTarget();
        if (target != null) enemy.SetNextTarget(target);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// �Q�[���I�[�o�[���Ƀv���C���[����Ă΂��.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnGameOver()
    {
        // �Q�[���I�[�o�[��\��.
        gameOver.SetActive(true);
        // �v���C���[���\��.
        PC.gameObject.SetActive(false);
        //���ׂĂ̑�����~�߂�t���O
        allControlStop = true;
        //�J�����̈ړ����~�߂�

        // �G�̍U���t���O������.
        foreach (EnemyController enemy in enemys) enemy.IsBattle = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ���g���C�{�^���N���b�N�R�[���o�b�N.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        // �v���C���[���g���C����.
        PC.Retry();
        // �G�̃��g���C����.
        foreach (EnemyController enemy in enemys) enemy.OnRetry();
        // �v���C���[��\��.
        PC.gameObject.SetActive(true);
        // �Q�[���I�[�o�[���\��.
        gameOver.SetActive(false);
        allControlStop = false;
    }
}
