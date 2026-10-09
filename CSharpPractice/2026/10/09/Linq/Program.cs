// 10-09 · LINQ 第二块实验：链上的【顺序】到底改了语义，还是只改了成本？
//
// 跑法：dotnet run --project d:\Study\Learning\CSharpPractice\2026\10\09\Linq
//
// ⭐ 用法建议：先【猜】再跑 —— 把这四行结果在心里过一遍，再按回车，对上就对上了。

using System.Diagnostics;

static string Show<T>(IEnumerable<T> seq) => "[" + string.Join(", ", seq) + "]";

// ═══════════ 实验①：顺序改变【语义】—— Take 的位置 ═══════════
//   问：两个查询，谁先谁后，结果会一样吗？
Console.WriteLine("═══ 实验① 顺序改变语义 —— Take 的位置 ═══");
int[] v1 = { 1, 2, 3, 4, 5 };

var a1 = v1.Where(n => n > 2).Take(2);   // 甲：先筛，再取前 2 个
var b1 = v1.Take(2).Where(n => n > 2);   // 乙：先取前 2 个，再筛

Console.WriteLine($"  源   : {Show(v1)}");
Console.WriteLine($"  甲 = Where(n>2) 然后 Take(2)  →  {Show(a1)}");
Console.WriteLine($"  乙 = Take(2) 然后 Where(n>2)  →  {Show(b1)}");
Console.WriteLine($"  一样吗？ {(Show(a1) == Show(b1) ? "一样" : "不一样 ← 顺序换了语义，不只是换了性能")}");
Console.WriteLine();

// ═══════════ 实验②：闸门效应 —— OrderBy 的位置 ═══════════
//   问：OrderBy 是"缓冲型"（不看完整条序列不敢下结论）——
//       那它站在链上的哪个位置，会决定它到底"吃进去几个"？
Console.WriteLine("═══ 实验② 闸门效应 —— OrderBy 的位置 ═══");
int[] v2 = { 5, 3, 1, 4, 2 };

var a2 = v2.Take(2).OrderBy(n => n);   // 甲：先拉 2 个出来，再对这 2 个排序
var b2 = v2.OrderBy(n => n).Take(2);   // 乙：先把 5 个全排序，再取前 2 个

Console.WriteLine($"  源   : {Show(v2)}");
Console.WriteLine($"  甲 = Take(2) 然后 OrderBy      →  {Show(a2)}");
Console.WriteLine($"  乙 = OrderBy 然后 Take(2)      →  {Show(b2)}");
Console.WriteLine($"  一样吗？ {(Show(a2) == Show(b2) ? "一样" : "不一样 ← 闸门站在前面还是后面，答案不同")}");
Console.WriteLine();

// ═══════════ 自己动手改（改完 Ctrl+S 再 dotnet run）═══════════
//  ① 实验①把筛选条件换成 `n => n < 4` —— 甲乙还对得上吗？为什么？
//  ② 实验①把 Take(2) 换成 Take(0) / Take(10) —— 边界上会发生什么？
//  ③ 实验②把源换成 { 5, 3, 1, 4, 2 } 的升序版 —— 甲乙还会不同吗？（提示：想想"闸门"有没有起作用的机会）
//  ④ 往实验②的 OrderBy 前面塞一句打印：
//        v2.Where(n => { Console.WriteLine($"筛 {n}"); return true; })
//     看"筛"打印几次 —— 数一数 OrderBy 到底吃进去几个，验证"闸门"这个词用得对不对。

var people = new (string 名字, int 分数)[] { ("A", 2), ("B", 1), ("C", 2), ("D", 1) };

var p1 = people.OrderBy(p => p.分数).OrderBy(p => p.名字);   // 连续两次 OrderBy
var p2 = people.OrderBy(p => p.分数).ThenBy(p => p.名字);    // OrderBy + ThenBy

Console.WriteLine($"p1 = {Show(p1)}");
Console.WriteLine($"p2 = {Show(p2)}");

// ⭐ 第三种写法做对照：还是"连续两次 OrderBy"，只是两次的键对调了
var p3 = people.OrderBy(p => p.名字).OrderBy(p => p.分数);

Console.WriteLine($"p3 = {Show(p3)}   ← 还是两次 OrderBy，只是键对调");
Console.WriteLine($"p2 和 p3 一样吗？ {(Show(p2) == Show(p3) ? "一样 ← 这就是「侥幸」：写法不同，结果撞上了" : "不一样")}");

var words = new[] { "apple", "bat", "cat", "door", "egg", "fox" };

Console.WriteLine("=== GroupBy：延迟 + 每次重算 ===");
var g = words.GroupBy(w => { Console.WriteLine($"  算 key: {w}"); return w.Length; });
Console.WriteLine("  （配方建好了，还没算）");
Console.WriteLine("  第一遍：");
foreach (var grp in g) Console.WriteLine($"    长度 {grp.Key} → {string.Join(",", grp)}");
Console.WriteLine("  第二遍：");
foreach (var grp in g) Console.WriteLine($"    长度 {grp.Key} → {grp.Count()} 个");

Console.WriteLine("=== ToLookup：立即结算 + 只建一次 ===");
var lk = words.ToLookup(w => { Console.WriteLine($"  [建] 算 key: {w}"); return w.Length; });
Console.WriteLine("  （建完了）");
Console.WriteLine("  第一遍：");
foreach (var grp in lk) Console.WriteLine($"    长度 {grp.Key} → {grp.Count()} 个");
Console.WriteLine("  第二遍：");
foreach (var grp in lk) Console.WriteLine($"    长度 {grp.Key} → {grp.Count()} 个");

// ═══════════ 实验④：分组器到底能不能"提前收手"？ ═══════════
//   问：只想要第一组（Take(1)），是不是就能少读几个元素？
int seen = 0;
var onlyFirst = words.GroupBy(w => { seen++; return w.Length; }).Take(1);

Console.WriteLine("=== 分组器能不能提前收手（只取第一组）===");
foreach (var grp in onlyFirst) Console.WriteLine($"  拿到第一组：长度 {grp.Key}（{string.Join(",", grp)}）");
Console.WriteLine($"  keySelector 被调用 {seen} 次，源一共 {words.Length} 个元素");
Console.WriteLine($"  ⇒ {(seen == words.Length ? "读干了整条源 ⇒ 分组器【不能】提前收手" : "只读了一部分 ⇒ 能提前收手")}");
Console.WriteLine();

// ═══════════ 实验⑤：按 key 取 —— 一个安静给空筐，一个当场炸 ═══════════
var dic = words.ToDictionary(w => w, w => w.Length);

Console.WriteLine("=== 按 key 取：Lookup vs Dictionary ===");
Console.WriteLine($"  lk[99]    → [{string.Join(",", lk[99])}]（{lk[99].Count()} 个）← 空序列，安安静静");
try
{
    _ = dic["zzz"];
}
catch (KeyNotFoundException e)
{
    Console.WriteLine($"  dic[\"zzz\"] → 抛 {e.GetType().Name} ← 当场炸");
}
Console.WriteLine();

// ═══════════ 实验⑥：Select 的性格 —— 流式 / 延迟 / 每次重算 ═══════════
Console.WriteLine("=== Select：延迟 + 流式 + 每次重算 ===");
int[] nums6 = { 1, 2, 3, 4 };
int calls = 0;

var sq = nums6.Select(n => { calls++; Console.WriteLine($"    [投影] {n}"); return n * n; });
Console.WriteLine($"  （配方建好了，投影调用次数 = {calls}）");
Console.WriteLine("  第一遍：");
foreach (var x in sq) Console.WriteLine($"    → 拿到 {x}");
Console.WriteLine($"  calls = {calls}");
Console.WriteLine("  第二遍（只看，不打印值）：");
foreach (var x in sq) { }
Console.WriteLine($"  calls = {calls}   ← 又涨了一轮");
Console.WriteLine();

// ═══════════ 实验⑦：Select 站在 Where 前面还是后面 ═══════════
//   问：结果一样吗？—— 代价一样吗？
Console.WriteLine("=== Select 与 Where 的先后 ===");
int[] nums7 = { 1, 2, 3, 4, 5 };
int c1 = 0, c2 = 0;

var r1 = nums7.Where(n => n % 2 == 0).Select(n => { c1++; return n * n; });   // 先筛，再投影
var r2 = nums7.Select(n => { c2++; return n * n; }).Where(n => n % 2 == 0);   // 先投影，再筛

Console.WriteLine($"  先筛再投影 → {Show(r1)}   投影跑了 {c1} 次");
Console.WriteLine($"  先投影再筛 → {Show(r2)}   投影跑了 {c2} 次");
Console.WriteLine($"  ⇒ 结果{(Show(r1) == Show(r2) ? "一样" : "不一样")}，代价{(c1 == c2 ? "也一样" : "却不一样")}");
Console.WriteLine();

// ═══════════ 实验⑧：投影能不能"换形状"？ ═══════════
Console.WriteLine("=== 投影换形状（Select 不是「取字段」）===");
var words8 = new[] { "apple", "bat" };

IEnumerable<string> upper = words8.Select(w => w.ToUpper());
IEnumerable<(string 词, int 长度)> pairs = words8.Select(w => (w, w.Length));

Console.WriteLine($"  Select(w => w.ToUpper())       → {Show(upper)}");
Console.WriteLine($"  Select(w => (w, w.Length))     → {Show(pairs)}");
Console.WriteLine();

// ═══════════ 实验⑦b：把投影换成 n + 1（+1 会翻奇偶）═══════════
Console.WriteLine("=== 同一条对照，投影换成 n + 1 ===");
int t1calls = 0, t2calls = 0;

var t1 = nums7.Where(n => n % 2 == 0).Select(n => { t1calls++; return n + 1; });
var t2 = nums7.Select(n => { t2calls++; return n + 1; }).Where(n => n % 2 == 0);

Console.WriteLine($"  先筛再投影 → {Show(t1)}   投影跑了 {t1calls} 次");
Console.WriteLine($"  先投影再筛 → {Show(t2)}   投影跑了 {t2calls} 次");
Console.WriteLine($"  ⇒ 结果{(Show(t1) == Show(t2) ? "一样（侥幸还成立）" : "不一样 ← 侥幸破功")}");
Console.WriteLine();

// ═══════════ 实验⑨：null 的"晚炸" ═══════════
Console.WriteLine("=== Select 遇到 null：炸在【遍历途中】，不是开头 ===");
string?[] raw = { "aa", "bbb", null, "dddd" };

Console.WriteLine("  只要投影（不挡）：");
try
{
    foreach (var len in raw.Select(s => s!.Length))
        Console.WriteLine($"    拿到 {len}");
}
catch (NullReferenceException)
{
    Console.WriteLine("    💥 炸了 —— 但上面【已经吐出的值】早被用掉了");
}

Console.WriteLine($"  Where 先挡一道 → {Show(raw.Where(s => s != null).Select(s => s!.Length))}  （不再炸）");
Console.WriteLine();

// ═══════════ 实验⑩：Join 是"哈希连接"，不是"两两比一遍" ═══════════
Console.WriteLine("=== Join 的代价：嵌套循环 vs 哈希连接 ===");
var custs = Enumerable.Range(1, 2000).Select(i => (Id: i, 名字: $"客户{i}")).ToArray();
var ords = Enumerable.Range(1, 20000).Select(i => (Id: i, CustomerId: (i % 2000) + 1)).ToArray();

int compares = 0, paired1 = 0;
var sw = Stopwatch.StartNew();
foreach (var o in ords)
{
    foreach (var c in custs)
    {
        compares++;
        if (o.CustomerId == c.Id) { paired1++; break; }
    }
}
sw.Stop();
Console.WriteLine($"  手工嵌套循环：比较 {compares:N0} 次 / {sw.ElapsedMilliseconds} ms / 配对 {paired1} 条");

sw.Restart();
var hashJoined = ords.Join(custs, o => o.CustomerId, c => c.Id, (o, c) => (o, c)).ToArray();
sw.Stop();
Console.WriteLine($"  Join（哈希连接）：{sw.ElapsedMilliseconds} ms / 配对 {hashJoined.Length} 条");
Console.WriteLine();

// ═══════════ 实验⑪：内连接会丢人，左连接不会 ═══════════
Console.WriteLine("=== 内连接 vs 左连接 ===");
var cs = new (string 名字, int Id)[] { ("C1", 1), ("C2", 2), ("C3", 3), ("C4", 4) };
var os = new (string 商品, int CustomerId)[] { ("苹果", 1), ("面包", 1), ("牛奶", 3) };

var inner = from c in cs
            join o in os on c.Id equals o.CustomerId
            select $"{c.名字} → {o.商品}";

var left = from c in cs
           join o in os on c.Id equals o.CustomerId into bucket
           from o in bucket.DefaultIfEmpty()
           select $"{c.名字} → {(o.商品 is null ? "（没下过单）" : o.商品)}";

Console.WriteLine($"  内连接（Join）→ {Show(inner)}");
Console.WriteLine($"  左连接（三步）→ {Show(left)}");
Console.WriteLine();

// ═══════════ 实验⑫：DefaultIfEmpty 到底管的是哪一件事 ═══════════
Console.WriteLine("=== DefaultIfEmpty：只有【整个序列空】才出手 ===");
int[] emptySeq = { };
int[] notEmpty = { 5, 6 };
string?[] hasNullInside = { "aa", null, "bb" };

Console.WriteLine($"  空序列   .DefaultIfEmpty()    → {Show(emptySeq.DefaultIfEmpty())}      ← 给一个 default(int) = 0");
Console.WriteLine($"  空序列   .DefaultIfEmpty(-1)  → {Show(emptySeq.DefaultIfEmpty(-1))}      ← 自己指定占位值");
Console.WriteLine($"  非空序列 .DefaultIfEmpty()    → {Show(notEmpty.DefaultIfEmpty())}      ← 原样放行，一个不多一个不少");
Console.WriteLine($"  序列里有 null 元素           → {Show(hasNullInside.DefaultIfEmpty().Select(x => x ?? "(空元素)"))}");
Console.WriteLine("     ↑ 里面的 null【没有】被替换 —— 它管的是「整个序列空」，不是「元素是 null」");
Console.WriteLine();

// ═══════════ 实验⑭：where 摆在 group 的哪一边，就换了"筛什么" ═══════════
Console.WriteLine("=== where 在 group 之前 / 在 into 之后 / 在组内 ===");
var rec = new (string 玩家, int 关卡Id, int 用时)[]
{
    ("小明", 1, 42), ("小红", 1, 35), ("小明", 2, 90), ("小刚", 2, 75), ("小红", 3, 50),
};

// A：where 在 group【之前】⇒ 筛的是【通关记录】（达标了才有资格进组）
var qA = from r in rec
         where r.用时 <= 60
         group r by r.关卡Id into grp
         orderby grp.Key
         select $"关卡{grp.Key}: {grp.Count()} 条";

// B：where 在 into【之后】⇒ 筛的是【组】（整关都被扔掉）
var qB = from r in rec
         group r by r.关卡Id into grp
         where grp.Count() >= 2
         orderby grp.Key
         select $"关卡{grp.Key}: {grp.Count()} 条";

// C：作用在【组内元素】上的 where（先分组，再数每组里达标的）
var qC = rec.GroupBy(r => r.关卡Id)
            .OrderBy(grp => grp.Key)
            .Select(grp => $"关卡{grp.Key}: 达标 {grp.Where(x => x.用时 <= 60).Count()} 条");

// D：⭐ 用【方法语法】把 B 原样写一遍
var qD = rec.GroupBy(r => r.关卡Id)
            .Where(grp => grp.Count() >= 2)
            .OrderBy(grp => grp.Key)
            .Select(grp => $"关卡{grp.Key}: {grp.Count()} 条");

Console.WriteLine($"  A where 在 group 前（筛记录）→ {Show(qA)}");
Console.WriteLine($"  B where 在 into 后（筛组）  → {Show(qB)}");
Console.WriteLine($"  C where 作用在组内元素      → {Show(qC)}");
Console.WriteLine($"  D 方法语法写 B              → {Show(qD)}   （和 B 一样吗？{(Show(qD) == Show(qB) ? "一样 ✅" : "不一样 ❌")}）");
Console.WriteLine();

// ═══════════ 实验⑮：聚合 —— 参数是「选择器」，不是「值」═══════════
Console.WriteLine("=== 聚合 · 数字序列（不带选择器）===");
var 金币表 = new[] { 12, 14, 8, 20, 15, 9, 25 };
Console.WriteLine($"  原始序列        → {Show(金币表)}");
Console.WriteLine($"  Count()         → {金币表.Count()}");
Console.WriteLine($"  Sum()           → {金币表.Sum()}");
Console.WriteLine($"  Average()       → {金币表.Average()}      ← ⭐ 永远返回 double");
Console.WriteLine($"  Max() / Min()   → {金币表.Max()} / {金币表.Min()}");
Console.WriteLine();

Console.WriteLine("=== 聚合 · 带选择器（从一条记录里挑字段）===");
var 记录表 = new (string 玩家, int 关卡Id, int 用时, int 金币)[]
{
    ("小明", 1, 42, 12), ("小红", 1, 35, 14), ("小明", 2, 90, 8),
    ("小刚", 2, 75, 20), ("小红", 3, 50, 15), ("小明", 3, 60, 9), ("小刚", 4, 120, 25),
};
Console.WriteLine($"  最高金币        → {记录表.Max(c => c.金币)}");
Console.WriteLine($"  用时合计        → {记录表.Sum(c => c.用时)}");
Console.WriteLine($"  Count 带条件    → {记录表.Count(c => c.用时 <= 60)}      ← Count 也能带条件");
Console.WriteLine($"  平均用时        → {记录表.Average(c => c.用时)}");
Console.WriteLine();

Console.WriteLine("=== 聚合 · 空序列上的陷阱 ===");
var 空表 = Array.Empty<int>();
Console.WriteLine($"  空表.Sum()      → {空表.Sum()}      ← 0（有单位元，不炸）");
Console.WriteLine($"  空表.Count()    → {空表.Count()}      ← 0");
try { Console.WriteLine($"  空表.Average()  → {空表.Average()}"); }
catch (InvalidOperationException e) { Console.WriteLine($"  空表.Average()  → 抛 {e.GetType().Name}   ← ⚠️ 空集合没有「平均」"); }
Console.WriteLine($"  空表.Any()      → {空表.Any()}     ← false");
Console.WriteLine($"  空表.All(x => x > 999) → {空表.All(x => x > 999)}   ← ⚠️ true！这叫「空真」");



Console.WriteLine();

// ═══════════ 实验⑬：把左连接拆开看 —— 「筐」长什么样、DefaultIfEmpty 管的是哪一步 ═══════════
Console.WriteLine("=== ① GroupJoin 给的「筐」长什么样 ===");
var buckets = cs.GroupJoin(
    os,
    c => c.Id,
    o => o.CustomerId,
    (c, bucket) => $"{c.名字}: [{string.Join(",", bucket.Select(b => b.商品))}]");

Console.WriteLine($"  {Show(buckets)}");
Console.WriteLine("  ↑ C2 / C4 拿到的是【空筐】—— 不是缺行、也不是 null");
Console.WriteLine();

Console.WriteLine("=== ② 把 DefaultIfEmpty 拿掉，客户就没了 ===");
var noDefault = from c in cs
                join o in os on c.Id equals o.CustomerId into bucket
                from o in bucket
                select $"{c.名字} → {o.商品}";

Console.WriteLine($"  GroupJoin + SelectMany（不补空）→ {Show(noDefault)}");
Console.WriteLine($"  ⇒ 和内连接{(Show(noDefault) == Show(inner) ? "一模一样" : "不一样")} ⇒ Join 其实就是「GroupJoin 之后把空筐丢掉」");



