using System;
using UnityEngine;

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
        _isGameOver = true;
    }
}
