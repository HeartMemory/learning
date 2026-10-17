using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  Obstacle —— 跑酷障碍物（自治）
//  职责：撞到玩家 → 上报裁判（GameManager）
//
//  ⭐ 今天的模式（照 Platformer 镜子）：哨兵 → 裁判 → 停手
//     · 哨兵 = 本类（Obstacle）：只负责"喊一嗓子"，不负责"怎么判、怎么结束"
//     · 裁判 = GameManager.Instance：唯一出口
//     · 停手 = 玩家 / 相机每帧读 GameManager.Instance.IsGameOver（读状态，非订阅）
//
//  ⭐ 判据（落笔前先读）：
//     ① 【为什么是 OnCollisionEnter 而不是 OnTriggerEnter】
//        玩家是 dynamic Rigidbody（PlayerController 已挂），障碍物是 static Collider
//        ⇒ 物理上"真撞"，双方都会收到 OnCollisionEnter。
//        Trigger 是"穿过不挡"（判定式）；Collision 是"撞上被挡"（物理式）。
//        跑酷"撞障碍 → 结束"要的是"撞"这个物理事实。
//     ② 【怎么认出"来的是玩家"】
//        用 `collision.gameObject.GetComponent<PlayerController>() != null` 判，
//        不依赖 Tag（省一次接线；Tag 是"全局命名"、组件是"类型"）。
//     ③ 【哨兵只管喊】本类不做任何"结束"的事，只调 GameManager.Instance.NotifyPlayerLost()。
//     ④ 【撞一次喊一次？不必自己防重复】
//        游戏结束的"只结算一次"由 GameManager 的幂等锁兜底 ——
//        ⭐ 承 Platformer："死"和"赢"共用同一把锁，最后一步同时压中两个哨兵也只结算一次。
//        这里同理：多个障碍同时撞、或同一个障碍每帧 OnCollisionEnter 刷屏，裁判只结算第一次。
//
//  🪞 镜子：Platformer 的 `FailZone`（漏球判定线）就是这种"哨兵"——
//     碰撞回调里"门卫（认出来的是谁）" → 上报 GameManager 唯一出口，自己不做别的。
// ══════════════════════════════════════════════════════════════
public class Obstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() != null)
        {
            // 撞到玩家 → 上报裁判（GameManager）
            GameManager.Instance.NotifyPlayerLost();
        }
    }
}
