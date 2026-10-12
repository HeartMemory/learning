using System;      // [Serializable]

// ══════════════════════════════════════════════════════════════
//  SaveData —— 「一份存档」的数据（Block 4 · 2026-10-12）
//  用途：写 / 读 `Application.persistentDataPath/save.json`
//
//  ⭐ 为什么写成【纯 C#】（不继承 MonoBehaviour、不 using UnityEngine）：
//     承 LevelData 的分层判据 —— 数据就是数据，"能离开 Unity 的代码" = 能单测、能秒验。
//
//  ⚠️ Newtonsoft 认 【public 字段】（与 LevelData 一致）；
//     若换成 System.Text.Json 就必须改成【属性】—— 那是控制台练习里踩过的坑。
//
//  ⭐ 每个字段都给【初始值】—— 这就是「旧存档缺字段不崩」的依托：
//     JSON 里没有那个键 ⇒ 反序列化后【保持字段初始值】（不崩、不抛、也不是 null）
//     ⇒ 加新字段时，一行迁移代码都不用写。
//     （10-19 会加 `UnlockedLevel`：那时旧存档会自动兼容 —— 本条就是实战验证）
// ══════════════════════════════════════════════════════════════
[Serializable]
public class SaveData
{
    public int SaveVersion = 1;      // 存档版本（将来真要"迁移"时用它分流）
    public int HighestScore = 0;     // 历史最高分（只升不降）
    public int TotalCoins = 0;       // 累计金币（跨局累加）

    public override string ToString()
        => $"v{SaveVersion}｜最高分 {HighestScore}｜累计金币 {TotalCoins}";
}
