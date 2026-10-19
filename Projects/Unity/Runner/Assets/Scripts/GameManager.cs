using System;
using UnityEngine;
using UnityEngine.SceneManagement;   // SceneManager（重开要用）

// ══════════════════════════════════════════════════════════════
//  GameManager —— 跑酷裁判（唯一出口）
//
//  ⭐ 模式（照 Platformer 镜子）：哨兵（Obstacle）→ 裁判（本类）→ 订阅者（停手）
//
//  ⭐ 判据（落笔前先读）：
//     ① 【单例】全局只有一个裁判，Instance 的装配照 Platformer 镜子
//        （重复实例要"响亮地失败"，别静默）
//     ② 【唯一出口】NotifyPlayerLost 是"结束"的唯一入口 ——
//        所有想让游戏结束的（障碍物、以后的掉落物 / 深渊）都调它，不各自改状态。
//     ③ 【幂等锁】_isGameOver 一锁，后到的上报直接 return。
//        ⭐ 承 Platformer："死"和"赢"共用同一把锁，最后一步同时压中两个哨兵也只结算一次。
//     ④ 【先锁 → 改状态 → 广播】顺序不能换：副作用（广播）放在最后。
//     ⑤ 【停手怎么实现】两种，今天先选一种跑通：
//        · 简单：`Time.timeScale = 0`（冻结整个世界）
//        · 精细：发 OnGameOver 事件，PlayerController 订阅 → 停手
//        （最小闭环先选一种；结算面板 / 重开按钮后续再加）
// ══════════════════════════════════════════════════════════════
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnGameOver;

    private bool _isGameOver;
    public bool IsGameOver => _isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"场景里有不止一个 GameManager（{name}）：本实例会被停用", this);
            enabled = false;
            return;
        }
        Instance = this;
        _isGameOver = false;
    }

    public void NotifyPlayerLost()
    {
        if(IsGameOver)
        {
            return; // 幂等锁：已经结束的游戏不再重复结算
        }
        _isGameOver = true;          // ① 先上锁（顺序不能换）

        // ② 再改状态：冻结世界
        //    只加"读状态停手"（10-17 的做法）治不了"结束后还在滑/滚/掉"：
        //    停手 = 不再写速度，但**物理引擎照跑**、刚体速度还在 ⇒ 惯性把人推走。
        //    `timeScale = 0` 才是"冻结世界"那一刀；两把刀互补，缺一不可。
        Time.timeScale = 0f;

        // ③ 最后播副作用：存档（RunnerSaveManager）与结算面板（RunnerDeathPanel）都订这里
        //    ⚠️ 10-19 之前【这一行是缺的】—— 事件声明了、谁都没 Invoke ⇒ 订阅者永远收不到
        OnGameOver?.Invoke();
    }

    // ── 重开一局（结算面板的 RESTART 按钮调它）──
    //    🪞 照 Platformer/GameManager.cs 的 Restart()：两行、顺序不能反
    public void Restart()
    {
        // ⚠️ 必须先还 timeScale = 1f，再 LoadScene ——
        //    否则新场景一开场就是【冻结】的（Breakout 09-26 踩过）
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
