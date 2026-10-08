// ═══════ 今日主题：文件读写 + 异常 + JSON 配置表（10-08）═══════
//
// 【一句话】文件读写 = 在「内存」和「磁盘」之间搬运数据；一切麻烦都来自【外面】。
//
// 【为什么要现在学】"市面上的关卡表"三档，最高一档就是【外部配置表】（JSON / CSV）——
//   国内项目里那些"配置表"就是它。学它需要的正是三件事：读文件 + 解析 + 容错。
//
// 【今天四块地基】
//   ① 读：`File.ReadAllText`（一次性全读 —— 配置表这个量级的首选）/ `File.ReadAllLines`
//   ② 解析：`JsonSerializer.Deserialize<T>(json)` —— ⭐ 它是个【泛型方法】（上午的泛型直接复用）
//   ③ 容错：文件不存在 / 内容坏掉 / 结构不对 —— 各写一条契约
//   ④ 写：`File.WriteAllText` + `JsonSerializer.Serialize`（写完再读回来 = 往返测试）
//
// 【判据链（本课的骨头）】
//   · 文件读写的核心**不是"怎么读"，而是"读砸了怎么办"**
//   · 抛型 vs Try 型：文件缺失是【异常情况】还是【正常情况】？前者抛，后者 Try
//   · ⭐ Try 契约 ≠ 吞掉一切异常：只接你【预期】的那几种（文件层 + 解析层），
//      其他的（拼写错、逻辑错）应该继续冒 —— 那是 bug，别替它捂盖子
//   · ⭐ 相对路径的基准 = **进程工作目录**：`dotnet run` 是项目目录；直接跑 exe 是输出目录
//   · 拼路径一律 `Path.Combine`，不手拼 "/" 或 "\"
//
// 【怎么用这个文件】
//   `dotnet run` 先看骨架 → 补 TODO 1~3 → 再跑，对着每行"期望"核。
//   ⚠️ 两个素材在 `data/` 下：`levels.json`（合法）、`broken.json`（内容坏掉）

using System.Text.Json;   // ⭐ JsonSerializer / JsonException 都在这里

static void TryRun(string title, Action act)
{
    Console.WriteLine($"—— {title} ——");
    try { act(); }
    catch (NotImplementedException ex) { Console.WriteLine($"      ⏳ 还没做：{ex.Message}"); }
    Console.WriteLine();
}

// ═══════ 先看清「相对路径的基准」—— 这是文件读写第一个要问的问题 ═══════
Console.WriteLine($"【基准 ①】当前工作目录   = {Directory.GetCurrentDirectory()}");
Console.WriteLine($"【基准 ②】程序集所在目录 = {AppContext.BaseDirectory}");
Console.WriteLine("   ⭐ 相对路径 `data/levels.json` 是相对【①】解析的：");
Console.WriteLine("      `dotnet run` 把 ① 设成项目目录 ⇒ 下面读得到；");
Console.WriteLine("      若直接跑 bin/Debug/net8.0/FileIO.exe，① 就变成输出目录 ⇒ 找不到 `data/`。");
Console.WriteLine();

Console.WriteLine("═══════ 文件读写 · 关卡配置表 ═══════\n");

TryRun("① 抛型契约：读【存在且合法】的文件", () =>
{
    List<LevelData> levels = FileOps.LoadLevelsOrThrow("data/levels.json");
    Console.WriteLine($"      读到 {levels.Count} 个关卡：");
    foreach (LevelData lv in levels) Console.WriteLine($"        {lv}");
});

TryRun("② 抛型契约：文件不存在 → 炸得响亮（这里当场接住给你看）", () =>
{
    try
    {
        List<LevelData> levels = FileOps.LoadLevelsOrThrow("data/does-not-exist.json");
        Console.WriteLine("      （不该走到这一行）");
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"      ✅ 接住 {ex.GetType().Name}");
        Console.WriteLine($"         Message = {ex.Message}");
        Console.WriteLine("         ← 消息里带着【文件名】，这就是「抛」的价值：类型明确、线索清楚");
    }
});

TryRun("③ 不抛型契约：TryLoadLevels（三种情况各自怎么表现）", () =>
{
    bool ok1 = FileOps.TryLoadLevels("data/levels.json", out List<LevelData>? a);
    Console.WriteLine($"      ① 存在的文件     → ok = {ok1}，读到 {a?.Count ?? 0} 个");

    bool ok2 = FileOps.TryLoadLevels("data/does-not-exist.json", out List<LevelData>? b);
    Console.WriteLine($"      ② 不存在的文件   → ok = {ok2}，结果 = {(b is null ? "null（调用方走默认值）" : "非 null")}");

    bool ok3 = FileOps.TryLoadLevels("data/broken.json", out List<LevelData>? c);
    Console.WriteLine($"      ③ 内容坏掉的文件 → ok = {ok3}，结果 = {(c is null ? "null" : "非 null")}");
});

TryRun("④ 写 + 往返测试（写进系统临时目录，不污染仓库）", () =>
{
    string outPath = Path.Combine(Path.GetTempPath(), "levels-roundtrip.json");
    List<LevelData> original = FileOps.LoadLevelsOrThrow("data/levels.json");
    FileOps.SaveLevels(outPath, original);
    List<LevelData> reloaded = FileOps.LoadLevelsOrThrow(outPath);
    Console.WriteLine($"      写出：{outPath}");
    Console.WriteLine($"      再读回：{reloaded.Count} 个（期望 {original.Count} 个）");
    Console.WriteLine($"      第一个：{reloaded[0]}");
});

Console.WriteLine("（全部绿了之后，试着把 data/levels.json 改坏再跑一遍，看哪几格变成什么样）");

// ═══════ 类型区 ═══════

// ⚠️ 这里用的是【属性】不是字段：`System.Text.Json` 默认**只认 public 属性**。
//    写成 public 字段会【静默】得到默认值（**不报错**！）—— 这类"安静地错"最难查。
// ⚠️ 另一个静默坑：默认**区分大小写** —— JSON 里的 "id" 配不上属性 `Id`。
public class LevelData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string SceneName { get; set; } = "";
    public override string ToString() => $"#{Id} {Name} → 场景 {SceneName}";
}

public static class FileOps
{
    // TODO 1：抛型 —— 读文件 + 解析（两步：文本 → 对象）
    //   · 第 ① 步：`File` 类里哪个方法能"一次读全部文本"？（回忆本课第一块那张速查表）
    //   · 第 ② 步：`JsonSerializer.Deserialize<???>(json)` —— ⭐ 尖括号里填你要的【目标类型】
    //   · 契约：文件不存在时【让它抛】，不要 catch
    //     ⇒ 这正是"抛型契约"：接不接、怎么接，由调用方决定
    // ✅ 答案（10-08）：三步（读 → 解析 → 返回）⇒ 必须用【块体】（`=>` 只能跟一个表达式）
    //    ⭐ 最后那行 `?? throw …` 很漂亮：`throw` 是【表达式】（C# 7 起）⇒ 能挂在 `??` 右边，
    //       一行同时完成"判 null + 抛"，顺手把"解析出 null"这个失败也定成了契约
    //    ⭐ 异常类型挑 `InvalidDataException`（System.IO）：语义 = "内容不符合预期"，比 InvalidOperation 更准
    public static List<LevelData> LoadLevelsOrThrow(string path)
    {
        string json = File.ReadAllText(path);                                          // ① 磁盘 → 字符串
        List<LevelData>? levels = JsonSerializer.Deserialize<List<LevelData>>(json);   // ② 字符串 → 对象
        return levels ?? throw new InvalidDataException($"解析出来是 null，不是合法的关卡表：{path}");
    }

    // TODO 2：不抛型 —— Try 风格（照 `int.TryParse` 的镜子：返回 bool + out 结果）
    //   · 成功 return true 并把结果赋给 out；失败 return false 并把 out 置为 null
    //   · 要接住的失败有三类：① 文件不存在 ② 内容不是合法 JSON（`JsonException`）③ 解析出 null
    //   · ⭐ 这道题真正的考点是"**你接哪些异常**"：
    //       —— Try 的承诺是"我绝不抛"；但 **"绝不抛" ≠ "什么都接"**
    //       —— 只接【预期】的（文件层 + 解析层）；拼写错 / 逻辑错那些应当继续冒（别替 bug 捂盖子）
    //   · 提示：如果只判 `File.Exists` 就万事大吉？想想"文件在、但读不了"（被占用 / 没权限）会怎样
    // ✅ 答案（10-08）：三条 catch + 一个"判 null"，把三档失败全部接管
    //    ⭐ `catch` 里【必须】做的两件事：① `levels = null`（out 必须赋值）② `return false`（补出口）
    public static bool TryLoadLevels(string path, out List<LevelData>? levels)
    {
        try
        {
            string json = File.ReadAllText(path);
            levels = JsonSerializer.Deserialize<List<LevelData>>(json);

            // ⭐ 第 ③ 类失败（"解析出来是 null"）—— 它【不是异常】，只能用表达式判：
            //    null → 返回 false，而此刻 levels 恰好就是 null ⇒ 自动满足"false 必配 null"的契约
            return levels is not null;
        }
        catch (IOException)                     // ② 文件层：不存在 / 目录不存在 / 被占用 —— 父类一句顶三个
        {
            levels = null;
            return false;
        }
        catch (UnauthorizedAccessException)     // ② 权限：⚠️ 不在 IOException 那一支，必须单独接
        {
            levels = null;
            return false;
        }
        catch (JsonException)                   // ③ 解析层：不是合法 JSON、或结构对不上（如"该数组却给了对象"）
        {
            levels = null;
            return false;
        }
        // 📌 三个 catch 里是【同一段代码】= 重复 —— 想去掉它，有两条路（见文件里 TODO 2 的注释）：
        //    · 异常筛选器：`catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)`
        //    · 或抽一个小方法统一收尾
    }

    // TODO 3：写 —— 对象 → 字符串 → 文件（也是两步，方向反过来）
    //   · ⭐ 拼路径用 `Path.Combine(目录, 文件名)`；`Path.GetTempPath()` 给临时目录
    //   · 想一想：写之前要不要 `File.Exists` 先判？
    //     （对比 `Directory.CreateDirectory` 的"已存在也不报错" —— 哪种风格更像"写"？）
    //   · ⚠️ 想一想：这个方法有【返回值】吗？（注意它的签名，和 `File.ReadAllText` 的区别）
    // ✅ 答案（10-08）：和"读"完全对称的两步，只是方向反过来
    //    ⭐ `Serialize(levels)` 【不用】写 `<T>` —— 类型能从参数推出来（类型推断）
    //    ⭐ `WriteAllText` 的语义 = "让它变成这样"：不存在就【创建】、存在就【覆盖】⇒ 不必先判 File.Exists
    public static void SaveLevels(string path, List<LevelData> levels)
    {
        string json = JsonSerializer.Serialize(levels);     // ① 对象 → 字符串
        File.WriteAllText(path, json);                      // ② 字符串 → 文件
    }
}
