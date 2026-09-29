using System;                            // ★ Action 在 System 命名空间里
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════
//  FailZone —— 屏幕下方那条"漏球判定线"（09-29 起：它成了一个【发布者】）
//
//  挂在层级根节点 FailZone 上（空物体 + BoxCollider2D 勾【是触发器】）。
//
//  依赖方向（09-29 改造后）：
//      FailZone ─→ 谁都不认识：它只负责喊一声 BallLost
//      GameManager ─→ 订阅它（GameManager 认识 FailZone；FailZone 不认识 GameManager）
//  ★ 为什么方向必须是这个：`event` 只能由【声明它的类】内部 raise
//    （外部 raise 或赋值 = CS0070）→ 谁要广播，事件就必须声明在谁身上 → 发布者 = 声明者
// ═══════════════════════════════════════════════════════════════════
public class FailZone : MonoBehaviour
{
    // ★ 事件 = "带门禁的委托字段"：订阅方只能 += / -=，不能赋值、不能调用
    //   ⚠️ 这里不能写 `Action?`：Unity 的编译上下文没开 nullable → 会报 CS8632 警告
    //   （注解只是给编译器的提示，与 `?.` 这个运行时检查是两件事）
    public event Action BallLost;

    // ─────────────────────────────────────────────────────────────
    // TODO 1：漏球判定
    //   void OnTriggerEnter2D(Collider2D other)
    //     ① 门卫：不是球就滚蛋
    //        if (!other.gameObject.CompareTag("Ball")) return;
    //        ★ 必须用 CompareTag，不要写 other.gameObject.tag == "Ball"
    //          —— 09-22 讲过：.tag 每次调用都新建 string → 堆分配 → 你在给 GC 添柴，
    //             而这个回调可能每帧触发多次
    //     ② 是球 → 留一条日志（漏球是意外事件，值得留痕）→ 广播 BallLost
    //        ★ 09-29 起这里【不再】直连 GameManager：
    //          没人订阅是合法的（通知是可选的旁路）→ 由 `?.` 兜住
    //          判据：**发布者的防线是 `?.`；"必须有对接方"的防线属于订阅者那一侧**
    //     ③ 日志放在广播【之前】—— 09-24 那条纪律：日志写在会抛异常的行之后 = 等于没放
    //
    //   ★★ 注意签名和 Brick.cs 的差别 ★★
    //     Brick.cs ：OnCollisionEnter2D(Collision2D other)   ← 碰撞回调
    //     这里      ：OnTriggerEnter2D (Collider2D  other)   ← 触发回调
    //     参数类型【不同】，别顺手抄错；抄错了编译器会报错（这次是好事，不是静默失效）
    //
    //   ★★ Trigger 生效的三个条件（少一个 = 静默失效，错误 12 的"四连"同款）★★
    //     ① 两者【至少一方】有 Rigidbody2D   → 球有（Dynamic）✓
    //     ② 两者【都】有 Collider2D          → 球有 CircleCollider2D ✓，FailZone 要挂 BoxCollider2D
    //     ③ 【至少一方】勾了"是触发器"        → FailZone 的 BoxCollider2D 勾上
    //     三条全满足才会回调；不满足时【不报错、不执行、控制台干干净净】——
    //     所以配完之后第一件事是【打日志验证它真的会响】，别靠"看起来像"。
    // ─────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;

        Debug.Log("球掉进了 FailZone");   // ★ 日志放在广播之前
        BallLost?.Invoke();               // ★ 广播：没人订阅就安静跳过（`?.` = 发布者的防线）
    }
}
