using UnityEngine;

public class Brick : MonoBehaviour
{
    private BrickSpawner _pool;
    public void Init(BrickSpawner pool)
    {
        _pool = pool;
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        // TODO 1：先把"不是球"的碰撞挡在门外
        //   Tag 路线：other.gameObject.CompareTag("Ball")
        //   ★ 必须用 CompareTag，不要写 other.gameObject.tag == "Ball"
        //     （.tag 属性每次调用都会新建一个 string → 堆分配 → 你在给 GC 添柴，
        //      正好是今天计基随行讲的那件事，而这个回调可能每帧触发多次）
        //   写法建议：提前 return 的"门卫式"——if (!...) return;
        //     比嵌套 if 更平（回想 232 那次"提前 return 只用于终止分支"的教训）

        if (!other.gameObject.CompareTag("Ball")) return;

        // TODO 2：是球 → 让砖块消失（最小版）
        //   gameObject.SetActive(false);
        //   ★ 想清楚：为什么是 gameObject.SetActive 而不是 this.enabled = false？
        //     关掉【脚本组件】之后，这块砖的碰撞体还在不在？球会不会照样撞上来？
        //     （09-16 学过"组件禁用 vs 物体禁用"，今天正好用它来解释一个真实后果）

        _pool.Return(gameObject);

        // TODO 3：打一条日志，证明"是砖块自己决定消失的"
        //   建议带上自己的名字和对方是谁，方便验收

        Debug.Log($"砖块 {gameObject.name} 被 {other.gameObject.name} 撞掉了");
    }
}
