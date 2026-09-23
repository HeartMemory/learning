using UnityEngine;
using TMPro;        // ★ TMP 在 TMPro 命名空间（昨天讲的"跨命名空间要 using"，今天实战）

public class ScoreManager : MonoBehaviour
{
    // ★ 字段类型用【基类】TMP_Text —— 它是 TextMeshProUGUI（UI 版）和 TextMeshPro（3D 版）的共同父类
    //   用基类 = 以后想挂在 3D 物体上也不用改代码（这就是你 09-15 学的抽象类的价值）
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private BrickSpawner brickSpawner;
    private int _score = 0;

    // TODO 1：对外提供"加分"入口
    //   public void AddScore(int delta)
    //     ① _score += delta
    //     ② 更新文本：scoreText.text = _score.ToString()
    //     ★ 只在这里写 text，绝不在 Update 里每帧写（计基那两条：字符串垃圾 + 重建）

    public void AddScore(int delta)
    {
        _score += delta;
        scoreText.text = "SCORE:" + _score.ToString();
    }

    // TODO 2（有课就走降档，这项可选）：剩余砖块数兜底
    //   public int RemainingBricks —— 想清楚这个数从哪儿来
    //   （提示：分数应能"从状态重算"，不只靠事件累加）
}
