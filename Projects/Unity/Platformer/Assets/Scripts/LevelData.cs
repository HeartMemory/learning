using System;      // [Serializable]

// ══════════════════════════════════════════════════════════════
//  LevelData —— 「一关」的数据（Block 3 · 2026-10-08 · 多关卡）
//  用途：从 Assets/Data/levels.json 读进来，组成"关卡表"
//
//  ⭐ 为什么写成【纯 C#】（不继承 MonoBehaviour、不 using UnityEngine）：
//     · 数据就是数据 —— 它不需要"挂"在任何物体上，也不需要 Update
//     · "能离开 Unity 的代码" = 能单测、能复用、能在控制台秒验（分层判据）
//
//  ⚠️ Newtonsoft 认 【public 字段】；
//     若换成 System.Text.Json 就必须改成【属性】——那是今天在练习里踩过的坑。
// ══════════════════════════════════════════════════════════════
[Serializable]
public class LevelData
{
    public int Id;              // 关卡编号（1 起）
    public string Name;         // 显示名（"草地关"）
    public string SceneName;    // ⭐ 必须与 Build Settings 里注册的【场景名】逐字一致，否则 LoadScene 会失败

    public override string ToString() => $"#{Id} {Name} → {SceneName}";
}
