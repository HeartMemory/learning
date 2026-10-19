using System;      // [Serializable]

// ══════════════════════════════════════════════════════════════
//  RunnerSaveData —— 「一份跑酷存档」的数据（Runner · 2026-10-19）
//  🪞 照 Platformer/SaveData.cs：纯 C#（不继承 MonoBehaviour、不 using UnityEngine）
//
//  ⚠️ Newtonsoft 认 **public 字段**；每个字段给【初始值】
//     ⇒ 旧存档缺字段时保持初始值，加新字段不用写迁移代码。
// ══════════════════════════════════════════════════════════════
[Serializable]
public class RunnerSaveData
{
    public int SaveVersion = 1;      // 存档版本
    public int HighestScore = 0;     // 历史最高分（只升不降）
    public int TotalRuns = 0;        // 总局数（跨局累加）

    public override string ToString()
        => $"v{SaveVersion}｜最高分 {HighestScore}｜总局数 {TotalRuns}";
}
