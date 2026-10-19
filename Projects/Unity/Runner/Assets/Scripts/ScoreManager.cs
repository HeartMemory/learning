using UnityEngine;
using TMPro;   // ★ TMP 在 TMPro 命名空间

// ══════════════════════════════════════════════════════════════
//  ScoreManager —— Runner 计分（2026-10-19）
//  🪞 整份照 Platformer/ScoreManager.cs 抄过来，只加了两样：
//     ① `HighestScore` 出口（由存档推入）  ② 显示串多一个 BEST
//
//  ⭐ 判据（照镜子，一条没改）：
//     · 单例：全场唯一，重复实例要"响亮地失败"，`OnDestroy` 里清掉入口（防悬空引用）
//     · **唯一入口**改分：`AddCoin` / `SetHighestScore` / `ResetScore`，别处一律只读
//     · **脏检查**：值没变就不写 text（`ToString` 造垃圾 + 触发 Canvas 重建）
//     · 文本**只用英文/数字**（TMP 默认字体没有中文字形）
// ══════════════════════════════════════════════════════════════
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int scorePerCoin = 10;   // ⭐ 分值归计分员管（"一枚几分"是规则，不是金币的属性）

    private int _coins;
    private int _score;
    private int _highest;          // 历史最高（来自存档，只升不降）
    private int _lastScore = int.MinValue;
    private int _lastBest = int.MinValue;

    // ── 只读出口（给存档 / 给面板）──
    public int Score => _score;
    public int Coins => _coins;
    public int HighestScore => _highest;
    public int DisplayHighest => Mathf.Max(_highest, _score);   // 本局破纪录时立刻反映

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"场景里有不止一个 ScoreManager（{name}）：本实例会被停用", this);
            enabled = false;
            return;
        }
        Instance = this;

        if (scoreText == null)
        {
            Debug.LogError("ScoreManager 的 scoreText 槽是空的：分数没地方画", this);
            enabled = false;
            return;
        }
        Redraw();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── 唯一入口①：吃到金币 ──
    public void AddCoin(int amount = 1)
    {
        if (amount == 0) return;
        _coins += amount;
        _score += amount * scorePerCoin;
        Redraw();
    }

    // ── 唯一入口②：存档读完档后把最高分推进来（只升不降，幂等）──
    public void SetHighestScore(int value)
    {
        if (value <= _highest) return;
        _highest = value;
        Redraw();
    }

    // ── 重开一局时清零（⚠️ 本工程靠 LoadScene 重开，这个入口留给"原地重开"的将来）──
    public void ResetScore()
    {
        _coins = 0;
        _score = 0;
        Redraw();
    }

    private void Redraw()
    {
        int best = DisplayHighest;
        if (_score == _lastScore && best == _lastBest) return;   // 脏检查
        _lastScore = _score;
        _lastBest = best;
        scoreText.text = "COINS:" + _coins + "  SCORE:" + _score + "  BEST:" + best;
    }
}
