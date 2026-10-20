using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  FailZone —— 掉落哨兵（3D 版 · 2026-10-20）
//  职责只有两件：① 判断"掉下来的是不是玩家" ② 上报裁判
//
//  🪞 复用自 Platformer/FailZone.cs（2D），改了三处：
//    ① 回调换套：3D 是 `OnTriggerEnter(Collider)`，2D 才是 `OnTriggerEnter2D(Collider2D)`
//       ⚠️ 套错不会报错 —— 只是**回调永远不会进来**（"静默不工作"的典型）
//    ② 认人方式换掉：Platformer 用 `LayerMask playerLayer` 按层认，
//       但 **Runner 没有 Player 层**（Coin.cs 已经踩过这一笔）⇒ 照抄层判定
//       的结果是"永远吃不到、且一声不响"
//       ⇒ 改用【组件判定】`GetComponent<PlayerController>()`，与 Obstacle / Coin 同一判据
//    ③ 自检的对象换了：没有层可查，改查 **Is Trigger 有没有勾**
//       （忘勾 = 触发器退化成实体挡板：玩家掉下去会"撞在空气上"停住，永远不触发回调）
//
//  ⭐ 判据（原样保留，这条最值钱）：
//     · 哨兵只管喊：本类不做任何"结束"的事，只调 GameManager.Instance.NotifyPlayerLost()
//     · 「只结算一次」不归哨兵管 —— 由 GameManager 的幂等锁兜底
//       ⇒ 从今天起有【两条致命源】（撞障碍 / 掉出地面），哪怕同一帧全中，也只结算一次
//     · `?.` 容错：场景里忘放 GameManager 也不会炸（承 Coin → ScoreManager 的写法）
//
//  ⚠️ 触发器要成立：**两边至少一方有刚体** —— 玩家是 Dynamic Rigidbody，已满足
//     （所以 FailZone 自己不需要挂刚体；挂了反而会被重力拽下去）
// ══════════════════════════════════════════════════════════════
[RequireComponent(typeof(Collider))]
public class FailZone : MonoBehaviour
{
    private void Awake()
    {
        // ── 装配期自检：响亮地失败 ──
        //   查的两件事都是"不勾就彻底静默"的开关，所以必须提前喊出来
        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            Debug.LogError("FailZone 上没有碰撞体：掉出地面不会有任何反应", this);
            enabled = false;
            return;
        }

        if (!col.isTrigger)
        {
            Debug.LogError("FailZone 的 Is Trigger 没勾：它变成实体挡板了 —— " +
                           "玩家会卡在判定区上、回调永远不进来", this);
            enabled = false;
        }
    }

    // ── 唯一的回调：有东西进到判定区 ──
    private void OnTriggerEnter(Collider other)
    {
        // 门卫：进来的是不是"带 PlayerController 的那个"？
        //   用组件判定而不是 Tag / Layer：Tag 是"全局命名"、组件是"类型"，后者才是真依赖
        if (other.GetComponent<PlayerController>() == null) return;

        // 哨兵只管喊（GameManager 是唯一出口）
        GameManager.Instance?.NotifyPlayerLost();
    }
}
