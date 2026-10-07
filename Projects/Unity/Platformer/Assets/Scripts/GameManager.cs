using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// ══════════════════════════════════════════════════════════════
//  GameManager —— 全场唯一裁判（Block 3 · 2026-10-06）
//  职责：接收"致命源"的上报 → 决定死 → 上锁 → 改状态 → 广播（唯一出口）
//
//  ⭐ 结构（10-06 定）：
//     致命源（FailZone / 敌人 / 将来的陷阱） ──直接调用──▶ GameManager.Instance.NotifyPlayerLost()
//     裁判 ──发事件──▶ 订阅者（DeathPanel 淡入；将来：音效 / 统计）
//  ⭐ 判据分层：
//     · 唯一的【服务】用单例 —— 听众只有 1 个，且敌人是【预制体】，裁判拖不了它的引用
//     · 一条【消息】用事件 —— 听众未知 / 会变
// ══════════════════════════════════════════════════════════════
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }   // ⭐ 唯一服务：让预制体一行就能找到裁判

    public event Action OnGameOver;      // 一条消息：**只有"死"** —— 需要区分死/赢的订阅者订它（如结算面板）
    public event Action OnGameWin;       // 一条消息：**只有"赢"**
    public event Action OnGameEnded;     // ⭐ 一条消息：**不管怎么结束** —— 不关心原因的订阅者（如"玩家停手"）只订它

    private bool _isGameOver;            // ⭐ 幂等门卫：全场唯一的上锁处

    private void Awake()
    {
        // ⭐ 单例自检：静态槽位不随场景卸载自动清 → 重开场景时可能残留旧值
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"场景里有不止一个 GameManager（{name}）：本实例会被停用", this);
            enabled = false;
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        // ⭐ 两笔账之一：静态字段【不随场景卸载自动清】→ 自己清入口（与 ScoreManager 同族）
        if (Instance == this) Instance = null;
    }

    // ⭐ 所有致命源的【唯一入口】：哨兵只管喊，裁判只管判
    public void NotifyPlayerLost()
    {
        if (_isGameOver) return;         // ① 先上锁（第二声、第三声都在这儿被吃掉）
        _isGameOver = true;

        Time.timeScale = 0f;             // ② 再改状态：世界冻结
        Debug.Log("[GameManager] 玩家死亡 → 世界冻结 + 广播 OnGameOver");

        OnGameOver?.Invoke();            // ③ 最后广播：谁关心谁反应（裁判不认识 UI）

        // TODO ①：紧接着再广播那条"不管怎么结束"的消息
        //   照镜子：上面那行怎么写，它就怎么写（只换事件名）
        //   ⚠️ 漏了它 → "死"之后【不关心原因】的订阅者也收不到通知
        OnGameEnded?.Invoke();
    }

    // ⭐ 所有【取胜源】的唯一入口（与 NotifyPlayerLost 逐行对称）
    public void NotifyPlayerWon()
    {
        // TODO ⑤：三件事，顺序与 NotifyPlayerLost 完全一致（上锁 → 改状态 → 广播）
        //   自检三问（写之前先答，别先抄）：
        //     ① 幂等门卫：用【同一个】_isGameOver，还是【另起一个】bool？
        //        两种做法各自会在什么情形下出错？
        //        提示：玩家最后一步可能【同时】踩中 Goal 与 FailZone —— 那一刻谁该赢？
        //     ② timeScale = 0 为什么夹在"上锁"和"广播"之间，而不是放最后？
        //     ③ Debug.Log 那行怎么写，才能在控制台里和"死"那行一眼分开？
        if (_isGameOver) return;         // ① 先上锁（第二声、第三声都在这儿被吃掉）
        _isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] 玩家通关 → 世界冻结 + 广播 OnGameWin");
        OnGameWin?.Invoke();

        // TODO ②：与 TODO ①【完全对应】—— 这里也必须广播那条"不管怎么结束"的消息
        //   ⚠️ 两处缺一不可：只写一处 = bug 只修了一半
        OnGameEnded?.Invoke();
    }

    public void Restart()
    {
        Time.timeScale = 1f;             // ⚠️ 必须先还 timeScale，否则新场景一开场就是冻结的
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
