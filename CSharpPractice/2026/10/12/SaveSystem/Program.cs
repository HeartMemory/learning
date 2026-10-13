// ═══════ 今日主题：存档 / 读档（10-12 · Block 4 第 1 课）═══════
//
// 【一句话】10-08 学的是「配置表」，今天学它的**反面** ——「存档」。
//   配置表：只读、跟游戏一起发布、没人改它 ⇒ 坏了是我的错，改回来就行。
//   存 档：① 运行时**写**它 ② 会被玩家机器上的意外弄坏 ③ 会随版本**演化**
//          ⇒ 坏了不能只会崩，得有**救**的办法。
//
// 【为什么现在学】Phase 0 达标项之一 =「能独立写带文件存取 + 异常的控制台项目」；
//   而且 10-13 起的 3D 跑酷（第 8 号成品）要记分 / 最高分 ⇒ 这套配方要**原样复用**。
//
// 【今天在 10-08 地基上要补的三件事】
//   ① **落点**：不能用相对路径（它的基准 =「进程工作目录」= 会随**启动方式**变）
//      ⇒ 换一个「确定的基准」拼出**绝对路径**：`AppContext.BaseDirectory`
//      （Unity 里换成 `Application.persistentDataPath` —— 配方同形，只换那一行）
//   ② **原子写**：直接覆盖写，一旦写到一半崩 / 断电 ⇒ 存档变半截，**旧档也没了**
//      ⇒ 先写**临时文件**，成功之后再**整体替换**
//   ③ **缺字段**：旧存档里没有新加的那个字段 —— 会崩吗？
//      ⭐ 不会。JSON 里没有的字段，反序列化后**保持属性的初始值**
//      ⇒ 这就是「存档演化」的**免费方案**：新字段给个默认值，旧档自动兼容
//
// 【判据链（本课的骨头）】
//   · 落点判据：这个基准会因为我**怎么启动**而变吗？会变就不能用
//   · 原子写判据：这个写操作有没有「中间状态」？有 ⇒ 就要临时文件
//   · 缺字段判据：JSON 缺的字段 → 取**属性初始值**（不崩、不抛、也不是 null）
//   · 容错两条底线（承 10-08）：① 不存在 ⇒ 默认值起步 ② 坏了 ⇒ 也能降级
//   · ⭐ 今天多问一句：这两件事**该不该在返回值上区分**？
//     「第一次玩」和「存档坏了」对玩家的意义完全不同 ⇒ **一个 bool 装不下三态**
//   · 还是 10-08 那条：**Try 契约 ≠ 吞掉一切** —— 只接你**预期**的（文件层 + 解析层）
//
// 【怎么用这个文件】
//   ① `dotnet run` 先看骨架（未实现的 TODO 会打 ⏳）
//   ② 从 TODO 1 开始补，**每补一个就再跑一次**，对着判据核
//   ③ 判题器是**只报告、不抛**的（✅/❌ 一路打完），所以一次能看到所有格子

using System.Text.Json;   // JsonSerializer / JsonException

// ───────────────────────── 工具 ─────────────────────────

// 让"还没做"和"做错了"分开：NotImplemented ⇒ ⏳；别的异常继续冒（那是 bug，别捂盖子）
static void TryRun(string title, Action act)
{
    Console.WriteLine($"—— {title} ——");
    try { act(); }
    catch (NotImplementedException ex) { Console.WriteLine($"      ⏳ 还没做：{ex.Message}"); }
    Console.WriteLine();
}

// 判题器要造「坏档 / 旧档」—— 临时写一个文件（别往仓库里加素材）
static void WriteFixture(string path, string content)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content);
}

// 打成 ✅/❌ 而不抛 —— 让判题器一次跑完所有格子
static void Check(string what, bool ok, string detail = "")
    => Console.WriteLine($"   {(ok ? "✅" : "❌")} {what}{(detail.Length == 0 ? "" : "   " + detail)}");

// ═══════════ 第一件事：把「落点」看清楚（这是今天第一个要问的问题）═══════════

Console.WriteLine("═══════ 存档要落在哪儿？先看清两个基准 ═══════\n");
Console.WriteLine($"【基准 ①】进程当前工作目录 = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"【基准 ②】程序集所在目录   = {AppContext.BaseDirectory}");
Console.WriteLine();
Console.WriteLine("   ⭐ ① 会随【怎么启动】而变：`dotnet run` 是项目目录，双击 exe 是输出目录");
Console.WriteLine("      ⇒ 存档挂在 ① 上，换个启动方式就「找不到自己的存档」");
Console.WriteLine("   ⭐ 存档要挂在一个【确定的基准】上 —— 今天用 ②，Unity 里换成");
Console.WriteLine("      `Application.persistentDataPath`（配方同形，只换那一行）");
Console.WriteLine();

// ══════════════════════════ 判题器 ══════════════════════════

TryRun("⓪ 落点自检", () =>
{
    string p = SaveSystem.SavePath;
    Console.WriteLine($"      SavePath = {p}");
    Check("是【绝对路径】（不依赖工作目录）", Path.IsPathFullyQualified(p));
    Check("落在 AppContext.BaseDirectory 之下", p.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase));
    Check("所在目录已存在（不存在要自己 CreateDirectory —— 想一想 Unity 那边要不要）", Directory.Exists(Path.GetDirectoryName(p)!));
});

TryRun("① 往返测试：写 → 读（承 10-08 那条）", () =>
{
    SaveData src = new SaveData { HighestScore = 1234, UnlockedLevel = 3, TotalCoins = 88, PlayerName = "阿猫" };
    SaveSystem.Save(src);
    LoadState st = SaveSystem.Load(out SaveData got);

    Check("Load 返回 Loaded", st == LoadState.Loaded, $"实到 {st}");
    Check("HighestScore 往返一致", got.HighestScore == src.HighestScore, $"{src.HighestScore} → {got.HighestScore}");
    Check("UnlockedLevel 往返一致", got.UnlockedLevel == src.UnlockedLevel, $"{src.UnlockedLevel} → {got.UnlockedLevel}");
    Check("TotalCoins 往返一致", got.TotalCoins == src.TotalCoins, $"{src.TotalCoins} → {got.TotalCoins}");
    Check("PlayerName 往返一致（中文别乱码）", got.PlayerName == src.PlayerName, $"{src.PlayerName} → {got.PlayerName}");
});

TryRun("② 没有存档 = 第一次玩", () =>
{
    if (File.Exists(SaveSystem.SavePath)) File.Delete(SaveSystem.SavePath);
    LoadState st = SaveSystem.Load(out SaveData d);
    if (d is null) { Console.WriteLine("   ❌ data 是 null —— 违反了「三种情况都交回可用对象」的契约"); return; }
    Check("返回 NotFound（**不是** Corrupted！这两种别混）", st == LoadState.NotFound, $"实到 {st}");
    Check("默认档 = 全新开始（UnlockedLevel == 1）", d.UnlockedLevel == 1, $"实到 {d.UnlockedLevel}");
});

TryRun("③ 存档坏掉（JSON 被写到一半）", () =>
{
    WriteFixture(SaveSystem.SavePath, "{\"HighestScore\": 99,");
    LoadState st = SaveSystem.Load(out SaveData d);
    Check("返回 Corrupted（不是 NotFound、更不是崩）", st == LoadState.Corrupted, $"实到 {st}");
    Check("仍然给出一份【可用】的默认档（这叫降级）", d is not null);
});

TryRun("④ 旧版存档：合法 JSON 但缺字段  ★ 今天最关键的一格", () =>
{
    WriteFixture(SaveSystem.SavePath, "{\"HighestScore\": 500}");
    LoadState st = SaveSystem.Load(out SaveData d);
    if (d is null) { Console.WriteLine("   ❌ data 是 null —— 违反了「三种情况都交回可用对象」的契约"); return; }
    Check("返回 Loaded（**缺字段不算坏档**）", st == LoadState.Loaded, $"实到 {st}");
    Check("存过的字段读出来了（500）", d.HighestScore == 500, $"实到 {d.HighestScore}");
    Check("【没存过】的字段 = 属性初始值（UnlockedLevel == 1）", d.UnlockedLevel == 1, $"实到 {d.UnlockedLevel}");
    Check("字符串字段也没被写成 null", d.PlayerName is not null, $"实到 {d.PlayerName ?? "null"}");
});

TryRun("⑤ 两种边角：\"{}\" 与字面 null", () =>
{
    WriteFixture(SaveSystem.SavePath, "{}");
    LoadState s1 = SaveSystem.Load(out SaveData a);
    if (a is null) { Console.WriteLine("   ❌ data 是 null —— 违反了「三种情况都交回可用对象」的契约"); return; }
    Check("\"{}\" ⇒ Loaded + 全默认值", s1 == LoadState.Loaded && a.HighestScore == 0, $"实到 {s1}｜最高分 {a.HighestScore}");

    WriteFixture(SaveSystem.SavePath, "null");
    LoadState s2 = SaveSystem.Load(out SaveData b);
    Console.WriteLine($"      字面 `null` ⇒ {s2}   ⚠️ 你把它算 Loaded 还是 Corrupted？先说理由再定");
    Check("无论算哪一种，都【不能崩】且必须给可用对象", b is not null);
});

TryRun("⑥ 原子写：走完之后不留临时文件", () =>
{
    string dir = Path.GetDirectoryName(SaveSystem.SavePath)!;
    SaveSystem.Save(new SaveData { HighestScore = 7 });
    string[] files = Directory.GetFiles(dir);
    Console.WriteLine($"      目录里现有 {files.Length} 个文件：");
    foreach (string f in files) Console.WriteLine($"        {Path.GetFileName(f)}");
    Check("没有 .tmp / .temp / .new 之类残留",
        !files.Any(f => f.EndsWith(".tmp") || f.EndsWith(".temp") || f.EndsWith(".new")));
    LoadState st = SaveSystem.Load(out SaveData d);
    Check("目标存档确实是最新的值", st == LoadState.Loaded && d is not null && d.HighestScore == 7);
});

TryRun("⑦ 覆盖写：连写两次，第二次赢", () =>
{
    SaveSystem.Save(new SaveData { HighestScore = 100 });
    SaveSystem.Save(new SaveData { HighestScore = 200 });
    LoadState st = SaveSystem.Load(out SaveData d);
    if (d is null) { Console.WriteLine("   ❌ data 是 null —— 违反了「三种情况都交回可用对象」的契约"); return; }
    Check("读到的是 200（不是 100，也不是两份数据打架）", st == LoadState.Loaded && d.HighestScore == 200, $"实到 {st}｜最高分 {d.HighestScore}");
});

// ═══════════ 进阶（做完 TODO 1~3 再说，别今天硬塞）═══════════
//
// ① 加一个 `.bak` 备份：写之前先把旧档复制成 `save.json.bak`；
//    读遇到 Corrupted 时**先去试试 bak** ⇒ 这才叫"能救"（今天只做到了"能降级"）
// ② 实测「为什么不能用工作目录」：把当前目录切到别处再跑 exe，看相对路径还能不能找到
//    （`Directory.GetCurrentDirectory()` 与 `AppContext.BaseDirectory` 两个基准各跑一次对照）
// ③ Unity 版：把 `AppContext.BaseDirectory` 换成 `Application.persistentDataPath`，其余一字不改
//    —— 顺便想一想：`persistentDataPath` 那个目录**要不要**自己 CreateDirectory？
// ④ 存档"防手改"：加一个校验和（把字段拼起来做个 hash 存进文件），读的时候对一遍
//    ⭐ 诚实一点：它防得住"手抖改坏"，但防不住**有意的**玩家（图什么？）

// ══════════════════════════════ 类型与答题区 ══════════════════════════════

// 存档里存什么：对着 Platformer 的真实需求（进度 + 最高分）
public class SaveData
{
    // ⭐ 每个字段都给【初始值】—— 这就是"旧存档缺字段不崩"的依托
    //    判据：JSON 里没有的字段，反序列化后**保持属性初始值**
    public int HighestScore { get; set; } = 0;
    public int UnlockedLevel { get; set; } = 1;      // 默认从第 1 关开始
    public int TotalCoins { get; set; } = 0;
    public string PlayerName { get; set; } = "Player";
    public int SaveVersion { get; set; } = 1;

    // 🆕 假装这是"下个版本才加的新字段" —— 判题器④ 会拿一份**没有它**的旧存档喂给你
    public int PlayTimeSeconds { get; set; } = 0;

    public override string ToString()
        => $"最高分 {HighestScore}｜解锁到第 {UnlockedLevel} 关｜金币 {TotalCoins}｜玩家 {PlayerName}｜版本 {SaveVersion}｜时长 {PlayTimeSeconds}s";
}

// ⭐ 三态：一个 bool 装不下「第一次玩」和「存档坏了」——
//    前者是**正常情况**（新玩家），后者要**告诉玩家**（他的存档没了）。
//    承 10-08 的 Try 契约，但返回值从 bool 升级成枚举。
public enum LoadState
{
    Loaded,      // 读到了真存档
    NotFound,    // 没有存档（第一次玩）
    Corrupted,   // 存档在、但读不了 / 解析不了（坏档）
}

public static class SaveSystem
{
    // ───────────────────────── TODO 1：落点 ─────────────────────────
    //   · 用哪个**基准** + 哪个**文件名**？
    //     判据：换个启动方式（`dotnet run` ↔ 双击 exe），它还指向**同一个文件**吗？
    //   · ⚠️ 别手拼 "/" 或 "\"（承 10-08：一律 `Path.Combine`）
    public static string SavePath => Path.Combine(AppContext.BaseDirectory, "save.json");
    private static string TempPath => SavePath + ".tmp";

    // ─────────────────── TODO 2：读（三态 + 两条底线）───────────────────
    //   契约：
    //     · 文件**不存在**          ⇒ NotFound  + data = 全新默认档
    //     · 文件在、但读/解析不了   ⇒ Corrupted + data = 全新默认档
    //     · 成功                    ⇒ Loaded    + data = 真存档
    //   ⚠️ 三种情况下 `data` 都必须是【可直接用】的对象（调用方不该再判 null）
    //   ⚠️ 只接你【预期】的那几类（文件层 + 解析层）—— 别的继续冒（承 10-08）
    //   ⚠️ 边角：JSON 内容正好是 `null` 时，`Deserialize` 会返回 null
    //      ⇒ 它算哪一种？**先说出理由，再写**（这一段判题器⑤ 会问你）
    public static LoadState Load(out SaveData data)
    {
        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData? parsed = JsonSerializer.Deserialize<SaveData>(json);   // ← 可空变量接住（CS8601 消失）
            if (parsed is null)                                              // ← "读出来了，但内容不成立"
            {
                data = new SaveData();
                return LoadState.Corrupted;                                  // ← 出口 ①
            }
            data = parsed;                                                   // ← 此处已被收窄成非 null
            return LoadState.Loaded;
        }
        catch (JsonException)
        {
            data = new SaveData();
            return LoadState.Corrupted;
        }
        catch (FileNotFoundException)
        {
            data = new SaveData();
            return LoadState.NotFound;
        }
        catch (IOException)
        {
            data = new SaveData();
            return LoadState.Corrupted;
        }
    }

    // ──────────────── TODO 3：写（原子写）· ✅ 10-13 完成 ────────────────
    //   📌 TODO 1 / 2 / 3 **全部完成**（⓪②③④⑤⑥⑦ 全绿；`dotnet build` = 0 警告 0 错误）
    //   ✅ 实测结论：`File.Replace` **要求目标已存在**（不存在抛 `FileNotFoundException`），
    //      而 `File.Move(s, d, overwrite: true)` 两种都行 ⇒ 选 **Move 一行搞定、不用分支**。
    //   ⭐ 三步：① 写临时文件 ② 整体替换 ③ 不留残渣（Move 会把 source 消耗掉）
    //   ⭐ 判据③（中途失败旧档不被破坏）**判题器测不了** ⇒ 用手工探针验过：
    //      在替换前插 `throw` → **读回** `save.json` → 内容一字未变。
    //   ⚠️ 复用到 Unity 时要改：那边 `.NET Standard 2.0` 档位 **没有三参 `File.Move`**
    //      ⇒ 必须自己分支（`Replace` / `Move`），详见 Unity 笔记第 26 章
    public static void Save(SaveData data)
    {
        File.WriteAllText(TempPath,JsonSerializer.Serialize<SaveData>(data));
        File.Move(TempPath,SavePath,true);
    }
}
