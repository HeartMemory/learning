using UnityEngine;
using TMPro;        // ★ TMP 在 TMPro 命名空间

// ═══════════════════════════════════════════════════════════════════
//  ScoreManager —— 只负责"把分数画出来"（方案甲：它【谁都不认识】）
//
//  数据流单向：BrickSpawner（状态）→ GameManager（读+判定）→ ScoreManager（只画）
//  箭头到 ScoreManager 就【断了】——这就是方案甲比乙干净的地方。
// ═══════════════════════════════════════════════════════════════════
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    // ★ 新增：分值归计分员管（"一块砖值几分"是【计分规则】，不是砖块的属性）
    [SerializeField] private int scorePerBrick = 10;

    private int _lastDestroyed = -1;    // 脏检查用的"上一次画的是什么"

    // ★ 为什么初始值给 -1 而不是 0？
    //   因为 0 是一个【合法】的"打掉 0 块"——用 0 当哨兵就分不清"还没画过"和"画了 0"。
    //   ⚠️ 这和 GameManager 那道"还没初始化"防线是【同一个病】，只是它在字段层面。

    // ❌ 已删除 09-23 预留的 brickSpawner 字段：分数不需要反向去问砖块

    // ─────────────────────────────────────────────────────────────
    // TODO 1：唯一入口 —— 收到"打掉了几块"，重算并显示分数
    //   public void SetDestroyedCount(int destroyed)
    //     ① 脏检查：if (destroyed == _lastDestroyed) return;
    //        ★ 为什么必须有？（09-23 那两条账：ToString() 造字符串垃圾 + 触发 Canvas 重建）
    //     ② 更新 _lastDestroyed
    //     ③ 写文本：scoreText.text = ... (destroyed * scorePerBrick)
    //
    //   ★★ 对比它和原来的 AddScore(int delta) —— 这就是方案甲的收益 ★★
    //     旧（事件累加）：_score += delta → 多喊一次就多加一次分，靠【调用方只喊一次】撑着
    //     新（状态重算）：显示 = f(destroyed) → 喊一次和喊一百次一样，【幂等由结构保证】
    //
    //   ★ 顺带想：如果 GameManager 忘了做脏检查、每帧都调这个方法，
    //     正确性还成立吗？代价是什么？
    // ─────────────────────────────────────────────────────────────
    public void SetDestroyedCount(int destroyed)
    {
        if (destroyed == _lastDestroyed) return;  // 脏检查
        _lastDestroyed = destroyed;
        scoreText.text = "SCORE:" + (destroyed * scorePerBrick).ToString();
    }
}
