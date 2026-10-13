using System;              // Exception
using System.IO;           // File / Path / IOException
using Newtonsoft.Json;     // JsonConvert（与 GameManager / LevelData 同一套库）
using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  SaveManager —— 存档 / 读档（Block 4 · 2026-10-12）
//  职责：① 启动时【读档】（三态容错）  ② 一局结束时【写档】
//
//  ⭐ 结构（三个判断，每条都带判据）：
//     ① **不用单例** —— 单例只在"别人【主动找它】"时才需要；本组件是【纯订阅者】
//        （它去找事件，没人来找它）⇒ 不开 static 入口。
//        （对照：GameManager / ScoreManager 是"服务"，别人要主动找 ⇒ 它们才是单例）
//     ② **不改 GameManager 一行** —— 直接订阅它【已有】的 `OnGameEnded`
//        （"不管死还是赢"都算一局的成果 ⇒ 这正是那个事件存在的意义）
//     ③ **落点** = `Application.persistentDataPath`
//        —— 控制台练习用的是 `AppContext.BaseDirectory`：**配方同形，只换这一行**
//
//  ⭐ 判据链（承 10-12 控制台练习，逐条搬过来）：
//     · 落点：这个基准会因为我【怎么启动】而变吗？persistentDataPath 不会（跨平台可写目录）
//     · 容错两条底线：文件不存在 ⇒ 默认档起步；文件坏了 ⇒ 也能降级（但要能区分！）
//     · 缺字段：JSON 里没有的键 ⇒ 保持字段初始值 ⇒ 加新字段时【不用写迁移代码】
//     · 只接【预期】的异常（文件层 + 权限 + 解析层）—— 别的继续冒（别替 bug 捂盖子）
// ══════════════════════════════════════════════════════════════

// ⭐ 三态：一个 bool 装不下「第一次玩」与「存档坏了」——
//    前者是【正常情况】（新玩家），后者要【告诉玩家】（他的存档没了）。
public enum SaveLoadState
{
    Loaded,      // 读到了真存档
    NotFound,    // 没有存档（第一次玩）
    Corrupted,   // 存档在、但读不了 / 解析不了（坏档）
}

public class SaveManager : MonoBehaviour
{
    // ⚠️ 只留【文件名】，不暴露完整路径：persistentDataPath 里含【公司名】，
    //    别把它带进日志（仓库红线：0 公司名）
    private const string FileName = "save.json";

    // ⭐ 落点 = 一个【确定的基准】+ 固定文件名
    //    不手拼 "/" 或 "\" —— 一律 Path.Combine（跨平台分隔符它自己管）
    //    ⚠️ persistentDataPath 这个目录 Unity【保证已存在】，不必自己 CreateDirectory
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);

    private SaveData _data = new SaveData();   // 内存里的当前存档（读档失败时它就是一份全新默认档）

    private void Start()
    {
        // ① 先读档
        SaveLoadState state = LoadFromDisk();
        Debug.Log($"[SaveManager] 读档结果 = {state} ⇒ {_data}");

        // ② 再订阅：⚠️ 必须放 Start，不能放 Awake ——
        //    `GameManager.Instance` 是它在【自己的 Awake】里赋值的，
        //    而两个 Awake 的先后顺序 Unity【不保证】⇒ 放 Awake 有几率拿到 null
        if (GameManager.Instance == null)
        {
            Debug.LogError("[SaveManager] 场景里没有 GameManager：本局结束不会被存档", this);
            return;
        }
        GameManager.Instance.OnGameEnded += HandleGameEnded;
    }

    private void OnDestroy()
    {
        // ⭐ 退订（承 09-29 事件那课）：发布者比订阅者活得久时会留【悬空订阅】。
        //    场景卸载会连发布者一起销毁，但"先退订"永远是安全写法。
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
    }

    // ───────────────── 一局结束：合并成果 → 落盘 ─────────────────
    private void HandleGameEnded()
    {
        // 取数：谁的状态谁提供（分数只有 ScoreManager 知道 —— 承"裁判不认识 UI"的同族分工）
        //   ⚠️ 空引用防线：单例可能被停用、也可能场景里压根没挂 ⇒ 读不到就当 0，
        //      但**不能**因此把存档也带崩（存档是最后一道工序，它不该成为新的崩溃源）
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        int coins = ScoreManager.Instance != null ? ScoreManager.Instance.Coins : 0;

        // 合并：只往"更好"的方向走
        if (score > _data.HighestScore) _data.HighestScore = score;   // 最高分只升不降
        _data.TotalCoins += coins;                                     // 累计金币（跨局累加）

        SaveToDisk();
    }

    // ⭐ 临时文件的落点：必须与正式档【同目录】—— "原子"只保证在**同一文件系统内**
    //    派生链只有一个方向：FileName → SaveFilePath → TempFilePath
    //    用【计算属性】而不是字段：每次现算 ⇒ 永远和源头一致
    private static string TempFilePath => SaveFilePath + ".tmp";

    // ───────────────── 写档：原子写（10-13）─────────────────
    private void SaveToDisk()
    {
        // ⭐ 原子写三步（与控制台练习 `SaveSystem.Save` 是【同一套配方】）：
        //    ① 写临时文件 ② 整体替换 ③ 走完不留残渣
        //
        //    ⚠️ 为什么不能直接 `File.WriteAllText(SaveFilePath, json)`：
        //       它是【先截断、再写】⇒ 中间任何一刻出事（断电 / 被杀进程 / 磁盘满），
        //       存档就成半截 —— 而且【旧的也没了】。
        //
        //    ⭐ 为什么"替换"是原子的：同卷内它是 **rename**（文件级换指向），
        //       不存在"写了一半"的中间态 ⇒ 要么旧档、要么新档。
        //
        //    ⚠️ 与控制台版的【唯一差别】（同一配方、两地 API 不同）：
        //       控制台（net8.0）有 `File.Move(tmp, dest, overwrite: true)`，一行搞定；
        //       本工程 `.NET Standard 2.0` 档位 ⇒ `File.Move` **只有两参重载**
        //       （三参要 .NET Standard 2.1）⇒ 只能自己分支。
        //       ⭐ 结论来自 `unity_reflect` 实测，不是凭记忆。
        string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
        File.WriteAllText(TempFilePath, json);                     // ① 写临时文件（同目录）

        if (File.Exists(SaveFilePath))                             // ② 整体替换（两分支）
            File.Replace(TempFilePath, SaveFilePath, null);        //    目标在 ⇒ 替换（backup 传 null = 不留备份）
        else
            File.Move(TempFilePath, SaveFilePath);                 //    第一次存档 ⇒ 直接搬过去

        // ③ Move / Replace 都会把临时文件【消耗掉】⇒ 目录里不留残渣
        Debug.Log($"[SaveManager] 已写档 {FileName}（原子）⇒ {_data}");
    }

    // ───────────────── 读档：三态 + 两条底线 ─────────────────
    private SaveLoadState LoadFromDisk()
    {
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData parsed = JsonConvert.DeserializeObject<SaveData>(json);

            // ⭐ "读出来了，但内容不成立" —— 它【不会抛异常】，只能靠判返回值。
            //    判据链：文件在 ⇒ 不可能是 NotFound；内容不能用 ⇒ 只剩 Corrupted
            if (parsed is null)
            {
                Debug.LogWarning($"[SaveManager] {FileName} 内容是 null ⇒ 按坏档处理，用全新档起步");
                _data = new SaveData();
                return SaveLoadState.Corrupted;
            }

            _data = parsed;
            return SaveLoadState.Loaded;
        }
        catch (FileNotFoundException)
        {
            // 文件不存在 = 【第一次玩】= 正常情况（不是错）
            _data = new SaveData();
            return SaveLoadState.NotFound;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            // ⚠️ 只接【预期】的三类（文件层 + 权限 + 解析层）—— 别的继续冒（别替 bug 捂盖子）
            //   · FileNotFoundException 是 IOException 的【子类】⇒ 它必须排在前面（就是上一条）
            //   · UnauthorizedAccessException【不在】IOException 那一支 ⇒ 必须单独列出来
            //   · JsonException = 文本不是合法 JSON / 结构对不上
            Debug.LogWarning($"[SaveManager] {FileName} 读不了 / 解析不了（{ex.GetType().Name}）⇒ 按坏档处理，用全新档起步");
            _data = new SaveData();
            return SaveLoadState.Corrupted;
        }
    }
}
