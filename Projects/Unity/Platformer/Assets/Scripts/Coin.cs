using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  Coin —— 金币自治（Block 3 · 2026-10-05 · 第 7 号成品）
//  职责只有两件：① 判断"吃到我的是不是玩家" ② 上报 + 自己消失
//  为什么金币自治（承 Breakout 的 Brick）：全场 1 个玩家、N 个金币
//    → 让 N 个各管自己一份逻辑，玩家代码不需要认识任何金币
// ══════════════════════════════════════════════════════════════
public class Coin : MonoBehaviour
{
    [Header("门卫")]
    [SerializeField] private LayerMask playerLayer;   // ⚠️ 只勾 Player 一层

    void Awake()
    {
        if (playerLayer.value == 0)
        {
            Debug.LogError("playerLayer 一个层都没勾：金币认不出玩家，永远不会被吃", this);
            enabled = false;
        }
    }

    // ⭐ 触发器回调：金币碰撞体勾了「是触发器」→ 只报"碰到了"，【不产生物理阻挡】
    //    判据：两个碰撞体里【至少一个带刚体】消息才会发出
    //          —— 玩家是 Dynamic 刚体 → 已满足，金币自己【不需要】刚体
    void OnTriggerEnter2D(Collider2D other)
    {
        // 门卫：用【层】判定（Player 层已存在；用标签还得先去 TagManager 加）
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // 上报计分：⭐ 金币只认识"计分员这个类型"，不认识任何一个具体对象
        //   ⭐ 用 `?.` 容错：场景里忘放 ScoreManager 时，金币照样能被吃（只是不加分）
        ScoreManager.Instance?.AddCoin(1);

        gameObject.SetActive(false);

        // ⭐ 用 SetActive 而不是 Destroy：销毁昂贵、禁用便宜（承 09-22 对象池的思想）
    }
}
