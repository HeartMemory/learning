using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  Goal —— 通关哨兵（Block 3 · 2026-10-07 · 第 7 号成品「2D 平台跳跃」）
//  职责只有两件：① 判断"碰到我的是不是玩家" ② 上报【裁判】
//
//  ⭐ 同族：FailZone.cs —— 它不是"参考资料"，是【镜子】。
//     结构逐行照抄，【唯一的不同】= 喊哪一句。
//     判据：哨兵只管喊，裁判只管判；哨兵不认识 UI、不认识面板。
// ══════════════════════════════════════════════════════════════
public class Goal : MonoBehaviour
{
    [Header("门卫")]
    [SerializeField] private LayerMask playerLayer;   // ⚠️ 只勾 Player 一层

    private void Awake()
    {
        // TODO ①【响亮地失败】：槽位一个层都没勾 → 喊一声 + 别让脚本继续装死
        //   照镜子：看 FailZone.Awake() —— 它用【哪个属性】判断"一个都没勾"？报错怎么写的？然后把自己怎么了？
        if(playerLayer.value == 0)
        {
            Debug.LogError("playerLayer 一个层都没勾：终点认不出玩家，碰到不会有人处理", this);
            enabled = false;
        }
    }

    // ⭐ 触发器回调：碰撞体勾了「是触发器」→ 只报"碰到了"，【不产生物理阻挡】
    //    判据（承 Coin）：两个碰撞体里【至少一个带刚体】消息才会发出 —— 玩家是 Dynamic 刚体，已满足
    void OnTriggerEnter2D(Collider2D other)
    {
        // TODO ②【门卫】：用【层】判定"撞进来的是不是玩家"，不是玩家就 return
        //   照镜子：FailZone 那一行的位运算怎么写？为什么是 `(mask.value & (1 << layer)) == 0` 这个形状？
        if((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // TODO ③【上报】：喊给谁、喊哪一句？
        //   ⭐ 与 FailZone 的【唯一差别】就在这一行 —— 想清楚"赢"该走哪条通道
        GameManager.Instance?.NotifyPlayerWon();
    }
}
