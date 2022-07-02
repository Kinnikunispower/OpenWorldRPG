using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // ゲームオーバーオブジェクト.
    [SerializeField] GameObject gameOver = null;
    // プレイヤー.
    [SerializeField] PlayerController PC = null;
    // 敵リスト.
    [SerializeField] List<EnemyController> enemys = new List<EnemyController>();
    public bool allControlStop;
    // 敵の移動ターゲットリスト.
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
    /// リストからランダムにターゲットを取得.
    /// </summary>
    /// <returns> ターゲット. </returns>
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
    /// 敵に次の目的地を設定.
    /// </summary>
    /// <param name="enemy"> 敵. </param>
    // ---------------------------------------------------------------------
    void EnemyMove(EnemyController enemy)
    {
        var target = GetEnemyMoveTarget();
        if (target != null) enemy.SetNextTarget(target);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ゲームオーバー時にプレイヤーから呼ばれる.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnGameOver()
    {
        // ゲームオーバーを表示.
        gameOver.SetActive(true);
        // プレイヤーを非表示.
        PC.gameObject.SetActive(false);
        //すべての操作を止めるフラグ
        allControlStop = true;
        //カメラの移動を止める

        // 敵の攻撃フラグを解除.
        foreach (EnemyController enemy in enemys) enemy.IsBattle = false;
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// リトライボタンクリックコールバック.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        // プレイヤーリトライ処理.
        PC.Retry();
        // 敵のリトライ処理.
        foreach (EnemyController enemy in enemys) enemy.OnRetry();
        // プレイヤーを表示.
        PC.gameObject.SetActive(true);
        // ゲームオーバーを非表示.
        gameOver.SetActive(false);
        allControlStop = false;
    }
}
