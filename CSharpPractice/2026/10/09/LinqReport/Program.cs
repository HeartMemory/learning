// ═══════ LINQ 第五块 · 合流练习：一份「关卡通关报表」 ═══════
//
// 【怎么用这个文件】
//   `dotnet run` 先跑一遍 → 会看到五个 ⏳ 还没做 → 去每个任务把 `throw` 换成你的查询 →
//   再 `dotnet run`，对着每行后面的 ✅ / ❌ 核（期望值已经写死在 Task 的第二个参数里）。
//
// 【这一块练什么】把前面四块的操作符【串成一条链】解决一个真实需求：
//   ① 筛（Where） ② 连（Join） ③ 榜（GroupBy + 聚合 + OrderBy + Select）
//   ④ 摊（GroupJoin + SelectMany + DefaultIfEmpty —— 左连接） ⑤ 人（GroupBy + Sum + OrderByDescending + ThenBy）
//
// 【唯一的硬规矩】任务 ④ / ⑤ 的期望值**故意设计成"不写对就一定对不上"** ——
//   ④ 的关卡 5 没人通关；⑤ 有两个玩家总分并列。想蒙是蒙不过去的。

using System.Text.RegularExpressions;

static string Show(object? o)
{
    if (o is null) return "null";
    if (o is string s) return s;
    if (o is System.Collections.IEnumerable seq)
        return "[" + string.Join(", ", seq.Cast<object?>().Select(x => x?.ToString())) + "]";
    return o.ToString()!;
}

void Task(string title, string expect, Func<object?> run)
{
    Console.WriteLine($"—— {title} ——");
    try
    {
        string got = Show(run());
        Console.WriteLine($"      {got}");
        Console.WriteLine($"      {(got == expect ? "✅ 与期望一致" : "❌ 不一致，期望 → " + expect)}");
    }
    catch (NotImplementedException ex)
    {
        Console.WriteLine($"      ⏳ 还没做：{ex.Message}");
    }
    Console.WriteLine();
}

// ═══════ 数据区（内联，不读文件 —— 文件读写留给「存档」那一块）═══════
var levels = new (int Id, string 名称, string 难度)[]
{
    (1, "草地关", "简单"),
    (2, "深渊关", "困难"),
    (3, "高塔关", "普通"),
    (4, "试炼关", "困难"),
    (5, "终章关", "困难"),      // ⭐ 全场没人通关过 —— 报表不许漏掉它
};

var clears = new (string 玩家, int 关卡Id, int 用时, int 金币)[]
{
    ("小明", 1,  42, 12),
    ("小红", 1,  35, 14),
    ("小明", 2,  90,  8),
    ("小刚", 2,  75, 20),
    ("小红", 3,  50, 15),
    ("小明", 3,  60,  9),
    ("小刚", 4, 120, 25),
};

Console.WriteLine("═══════ LINQ 第五块 · 关卡通关报表 ═══════\n");

// ───────────────────────────────────────────────────────────────
Task(
    "任务 ① 筛：用时 ≤ 60 秒的通关记录",
    "[小明/关卡1/42s, 小红/关卡1/35s, 小红/关卡3/50s, 小明/关卡3/60s]",
    () =>
    {
        // 【要什么】一个序列，每个元素形如：小明/关卡1/42s
        // 【用什么】Where（一个就够）
        // 【坑 1】`<=` 还是 `<`？60 秒那条算不算 —— 数一数期望值里有几条。
        // 【坑 2】Where 和 Select 谁在前？（想不起就回想实验⑦：投影跑了 2 次还是 5 次）
        // 【改这里】把这个 throw 换成 `return clears.Where(...).Select(...);`
        return clears.Where(time => time.用时 <= 60).Select(n => $"{n.玩家}/关卡{n.关卡Id}/{n.用时}s");
    });

// ───────────────────────────────────────────────────────────────
Task(
    "任务 ② 连：每条通关记录贴上关卡名",
    "[小明→草地关, 小红→草地关, 小明→深渊关, 小刚→深渊关, 小红→高塔关, 小明→高塔关, 小刚→试炼关]",
    () =>
    {
        // 【要什么】每条通关记录配一个关卡名，格式：玩家→关卡名
        // 【用什么】join（内连接）—— 查询语法 / 方法语法都行，写一个
        // 【坑 1】输出顺序 = 【通关记录】的原顺序，不是按关卡号 —— 顺序这件事由"谁被扫一遍"决定
        // 【坑 2】配不上的会被丢掉 —— 这里恰好每条记录都能配上
        return clears.Join(levels,a => a.关卡Id,b => b.Id,(a,b) => $"{a.玩家}→{b.名称}");
    });

// ───────────────────────────────────────────────────────────────
Task(
    "任务 ③ 榜：关卡排行（按平均用时升序）",
    "[关卡1: 2 次 / 平均 38.5s / 最高 14, 关卡3: 2 次 / 平均 55s / 最高 15, 关卡2: 2 次 / 平均 82.5s / 最高 20, 关卡4: 1 次 / 平均 120s / 最高 25]",
    () =>
    {
        // 【要什么】每关一行，格式：关卡{关卡号}: {次数} 次 / 平均 {平均用时}s / 最高 {最高金币}
        // 【用什么】GroupBy → 聚合（Count / Average / Max）→ OrderBy → Select（投影成字符串）
        // 【坑 1】Average 出来是 double：38.5 显示成 "38.5"、55 显示成 "55"（别自己四舍五入）
        // 【坑 2】这一版【只包含有人玩过的关卡】—— 关卡 5 该不该出现在报表里？看任务 ④
        // ✅ 成品（主语 = 关卡 ⇒ 按 关卡Id 分组；⭐ "组"里是记录，聚合必须给"选哪个字段"的选择器）
        return clears
            .GroupBy(c => c.关卡Id)
            .OrderBy(g => g.Average(x => x.用时))
            .Select(g => $"关卡{g.Key}: {g.Count()} 次 / 平均 {g.Average(x => x.用时)}s / 最高 {g.Max(x => x.金币)}");
    });

// ───────────────────────────────────────────────────────────────
Task(
    "任务 ④ 摊：关卡 × 通关者 明细（没人通关的关卡也要留一行）",
    "[草地关 → 小明, 草地关 → 小红, 深渊关 → 小明, 深渊关 → 小刚, 高塔关 → 小红, 高塔关 → 小明, 试炼关 → 小刚, 终章关 → （没人通关）]",
    () =>
    {
        // 【要什么】每关×每通关者一行，格式：{关卡名} → {玩家}；没人通关的关卡留一行 "（没人通关）"；
        //           ⭐ 按【关卡号】升序（不是按平均用时）
        // 【用什么】左连接的固定配方：join … on … equals … into 筐 → from x in 筐.DefaultIfEmpty() → select
        // 【坑 1】`.DefaultIfEmpty()` 给的是 `default` —— 值元组 (string,int,int,int) 的 default 是 (null,0,0,0)
        //         ⇒ 只能靠"玩家那个字段是 null"判出来（⚠️ 这是【侥幸】：恰好有个 string 字段）
        // 【坑 2】投影里要同时引用【关卡】和【通关记录】—— 想一想，被 `into` 收走之后，
        //         第二个 from 里的那个变量是【新声明的】还是旧的？
        // ✅ 成品（左连接固定配方：join … into 筐 → orderby → from x in 筐.DefaultIfEmpty() → select）
        return from lv in levels
               join rc in clears on lv.Id equals rc.关卡Id into bucket
               orderby lv.Id
               from x in bucket.DefaultIfEmpty()
               select $"{lv.名称} → {(x.玩家 is null ? "（没人通关）" : x.玩家)}";
    });

// ───────────────────────────────────────────────────────────────
Task(
    "任务 ⑤ 人：玩家榜（总金币降序，并列时按最高金币降序）",
    "[小刚: 45 金币 / 最高 25, 小红: 29 金币 / 最高 15, 小明: 29 金币 / 最高 12]",
    () =>
    {
        // 【要什么】每人一行，格式：{玩家}: {总金币} 金币 / 最高 {最高金币}；总金币降序，并列时按最高金币降序
        // 【用什么】GroupBy → Sum / Max → OrderByDescending → ThenByDescending → Select
        // 【坑】⭐ 这里 ThenBy **不是可选的**：小红和小明都是 29 分，
        //        - 只用一次 OrderByDescending：并列时保留的是【分组出现的顺序】（小明先出现）⇒ 小明在小红前面
        //        - 加 ThenByDescending(最高金币)：小红(15) 才会排到小明(12) 前面
        //        ⇒ 这就是今天那条判据的正面用法：**并列时的顺序，也得由你说清，不能靠"碰巧"**
        // ✅ 成品（主语 = 玩家 ⇒ 按 玩家 分组；⭐ ThenByDescending 不是可选的：并列时由它决定顺序）
        return clears
            .GroupBy(c => c.玩家)
            .OrderByDescending(g => g.Sum(x => x.金币))
            .ThenByDescending(g => g.Max(x => x.金币))
            .Select(g => $"{g.Key}: {g.Sum(x => x.金币)} 金币 / 最高 {g.Max(x => x.金币)}");
    });

// ═══════ 进阶（做完五个任务再说）═══════
//  ① 把任务 ③ 的排序键从"平均用时"换成"最高金币"，观察榜怎么变
//  ② 任务 ⑤ 加一列"通关了几【关】"—— 同一人同一关通两次只算一关（提示：`Distinct()` 能做什么）
//  ③ ⭐ 把任务 ④ 的 `DefaultIfEmpty()` 拿掉再跑 —— 你会拿到"内连接"的结果，关卡 5 直接消失
//  ④ 用 `ToLookup` 重写任务 ④，比较两种写法谁更可读
//  ⑤ ⭐⭐ 抬头看看今天全部实验里出现过几次"结果对但写法错" —— 然后回答：
//     **为什么"跑一遍对了"是最弱的证据？**
