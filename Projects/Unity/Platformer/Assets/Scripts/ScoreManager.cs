using UnityEngine;
using TMPro;   // ★ TMP 在 TMPro 命名空间

// ══════════════════════════════════════════════════════════════
//  ScoreManager —— 只管"把分数画出来"（承 Breakout 的同一套职责）
//  🆕 今天的新东西：**单例（Singleton）**
//     —— "全场只有一个"的组件，让任何地方都能直接找到它，
//        不必层层传引用、也不必到处 FindObjectOfType
// ══════════════════════════════════════════════════════════════
public class ScoreManager : MonoBehaviour
{
    // ⭐ 单例入口：static = 挂在【类型】上（不是某个实例上）
    //   · private set = 外部只能读，不能改（否则谁都能把它指走）
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int scorePerCoin = 10;   // ⭐ 分值归计分员管（"一枚值几分"是规则，不是金币的属性）

    private int _coins;            // 状态：已收集数
    private int _score;            // 状态：总分
    private int _lastShown = -1;   // 脏检查用的"上一次画的是什么"

    void Awake()
    {
        // ⭐ 单例的"响亮地失败"：重复挂载 = 逻辑会分叉（两个实例抢着当）
        if (Instance != null && Instance != this)
        {
            Debug.LogError("场景里有第二个 ScoreManager：单例必须全场唯一", this);
            enabled = false;
            return;
        }
        Instance = this;

        if (scoreText == null)
        {
            Debug.LogError("scoreText 槽位是空的：分数没地方画", this);
            enabled = false;
            return;
        }

        Redraw();   // 开局先画一次（0 分）
    }

    void OnDestroy()
    {
        // ⭐ 单例的账：自己死掉时必须把"类型级入口"清掉，否则会留一个【悬空引用】
        if (Instance == this) Instance = null;
    }

    // ── 唯一入口：收到"吃到了几枚" ──
    //   ⭐ 内部仍是"状态 + 重画"：改分只在这里，画只在 Redraw 里（承 Breakout 的"唯一出口"）
    public void AddCoin(int amount = 1)
    {
        if (amount == 0) return;
        _coins += amount;
        _score += amount * scorePerCoin;
        Redraw();
    }

    // ── 重开一局时清零（10-06 会用到）──
    public void ResetScore()
    {
        _coins = 0;
        _score = 0;
        Redraw();
    }

    private void Redraw()
    {
        // 脏检查：分数没变就不写（承 09-23：`ToString()` 造垃圾 + 触发 Canvas 重建）
        if (_score == _lastShown) return;
        _lastShown = _score;
        scoreText.text = "COINS:" + _coins + "  SCORE:" + _score;   // ⚠️ 只用英文/数字
    }
}
