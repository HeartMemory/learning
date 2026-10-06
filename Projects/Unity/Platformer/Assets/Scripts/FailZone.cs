using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  FailZone —— 掉落哨兵（Block 3 · 2026-10-06）
//  职责只有两件：① 判断"掉下来的是不是玩家" ② 上报裁判
//  ⭐ 10-06 调整：不再自定义事件 —— 致命源统一走 GameManager.Instance
//     理由：敌人是【预制体】，裁判拖不了它的引用；单例让任何预制体一行就能上报
// ══════════════════════════════════════════════════════════════
public class FailZone : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;   // ⚠️ 只勾 Player 一层

    private void Awake()
    {
        if (playerLayer.value == 0)
        {
            Debug.LogError("playerLayer 一个层都没勾：判定区认不出玩家，掉下去不会有人处理", this);
            enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // ⭐ `?.` 容错：场景里忘放 GameManager 也不会炸（承 Coin → ScoreManager 的写法）
        GameManager.Instance?.NotifyPlayerLost();
    }
}
