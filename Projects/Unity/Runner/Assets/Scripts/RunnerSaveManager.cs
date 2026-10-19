using System;              // Exception
using System.IO;           // File / Path / IOException
using Newtonsoft.Json;     // JsonConvert（本工程已装 com.unity.nuget.newtonsoft-json）
using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  RunnerSaveManager —— 存档 / 读档（Runner · 2026-10-19）
//  🪞 整份照 Platformer/SaveManager.cs 抄过来，**只换落点文件名**：
//        "save.json" → "runner-save.json"
//     （两个工程各有一个 persistentDataPath 子目录，本来也不会撞，但名字分开更好认）
//
//  ⭐ 判据（一条没改）：
//     · **不用单例**：本组件是【纯订阅者】（它去找事件，没人来找它）
//     · 订阅放 **Start 不放 Awake**（`GameManager.Instance` 在它的 Awake 里赋值，先后不保证）
//     · 落点 = `Application.persistentDataPath`（跨平台可写目录，不随启动方式变）
//     · **原子写**：写临时文件 → 整体替换 → 不留残渣
//     · **三态**：Loaded / NotFound（第一次玩，正常）/ Corrupted（坏档，要区分）
//     · 只接【预期】的异常（文件层 + 权限 + 解析层）
// ══════════════════════════════════════════════════════════════
public enum SaveLoadState
{
    Loaded,
    NotFound,
    Corrupted,
}

public class RunnerSaveManager : MonoBehaviour
{
    // ⚠️ 只留文件名，不暴露完整路径（persistentDataPath 含本机用户名）
    private const string FileName = "runner-save.json";

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);
    private static string TempFilePath => SaveFilePath + ".tmp";   // 同目录：原子只保证在同一文件系统内

    private RunnerSaveData _data = new RunnerSaveData();

    private void Start()
    {
        SaveLoadState state = LoadFromDisk();
        Debug.Log($"[RunnerSaveManager] 读档结果 = {state} ⇒ {_data}");

        // 把最高分推给计分员（计分员是"服务"，由本类去找它；本类自己不开单例）
        if (ScoreManager.Instance == null)
            Debug.LogWarning("[RunnerSaveManager] 场景里没有 ScoreManager：最高分无法显示（存档本身不受影响）", this);
        else
            ScoreManager.Instance.SetHighestScore(_data.HighestScore);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[RunnerSaveManager] 场景里没有 GameManager：本局结束不会被存档", this);
            return;
        }
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        // 空引用防线：读不到就当 0，但**不能**因此把存档带崩
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;

        if (score > _data.HighestScore) _data.HighestScore = score;   // 只升不降
        _data.TotalRuns++;

        SaveToDisk();
    }

    private void SaveToDisk()
    {
        string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
        File.WriteAllText(TempFilePath, json);                     // ① 写临时文件

        if (File.Exists(SaveFilePath))
            File.Replace(TempFilePath, SaveFilePath, null);        // ② 替换（backup = null）
        else
            File.Move(TempFilePath, SaveFilePath);                 //    第一次存档 ⇒ 直接搬

        Debug.Log($"[RunnerSaveManager] 已写档 {FileName}（原子）⇒ {_data}");
    }

    private SaveLoadState LoadFromDisk()
    {
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            RunnerSaveData parsed = JsonConvert.DeserializeObject<RunnerSaveData>(json);

            if (parsed is null)     // "读出来了但内容不成立" —— 不会抛异常，只能判返回值
            {
                Debug.LogWarning($"[RunnerSaveManager] {FileName} 内容是 null ⇒ 按坏档处理");
                _data = new RunnerSaveData();
                return SaveLoadState.Corrupted;
            }

            _data = parsed;
            return SaveLoadState.Loaded;
        }
        catch (FileNotFoundException)
        {
            _data = new RunnerSaveData();      // 第一次玩 = 正常情况
            return SaveLoadState.NotFound;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Debug.LogWarning($"[RunnerSaveManager] {FileName} 读不了 / 解析不了（{ex.GetType().Name}）⇒ 按坏档处理");
            _data = new RunnerSaveData();
            return SaveLoadState.Corrupted;
        }
    }
}
