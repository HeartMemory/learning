using UnityEngine;

// ═══════════════════════════════════════════════════════════════════
//  FailZone —— 屏幕下方那条"漏球判定线"
//
//  挂在层级根节点 FailZone 上（空物体 + BoxCollider2D 勾【是触发器】）。
//  依赖方向：FailZone ─→ GameManager（单向向上报信，不构成环）
// ═══════════════════════════════════════════════════════════════════
public class FailZone : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    // ─────────────────────────────────────────────────────────────
    // TODO 1：漏球判定
    //   void OnTriggerEnter2D(Collider2D other)
    //     ① 门卫：不是球就滚蛋
    //        if (!other.gameObject.CompareTag("Ball")) return;
    //        ★ 必须用 CompareTag，不要写 other.gameObject.tag == "Ball"
    //          —— 09-22 讲过：.tag 每次调用都新建 string → 堆分配 → 你在给 GC 添柴，
    //             而这个回调可能每帧触发多次
    //     ② 是球 → gameManager.OnBallLost();
    //     ③ 打一条日志，证明"是这条线自己判的"（漏球属于意外事件，值得留痕）
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
        gameManager.OnBallLost();
        Debug.Log("球掉进了 FailZone");
    }
}
