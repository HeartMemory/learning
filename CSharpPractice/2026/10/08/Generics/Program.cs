// ═══════ 今日主题：泛型（Generics）—— 用「关卡数据」练手 ═══════
//
// 【一句话】泛型 = **把"类型"也变成参数** —— 让容器与方法不必为每种类型各写一遍。
//
// 【为什么要它】承今天开头那 10 分钟（装箱 / 拆箱）：
//   `ArrayList` = 万能纸箱：什么都能塞，取出来必须拆箱 + 转型，错在**运行期**才知道；
//   `List<T>`   = 按用途定制的箱子：塞错东西**编译期**就被拦，取出来直接就是那个类型 ⇒ 0 装箱、0 转型。
//   ⭐ 本质：编译器拿到具体类型后，**为它生成一份专用代码**（不是语法糖，是把运行期检查搬到编译期）。
//
// 【今天四块地基】
//   ① **泛型类 / 接口**：`List<T>` / `Dictionary<K,V>` / `Queue<T>` —— 你其实天天在用，今天看清它长什么样
//   ② **泛型方法**：`T` 出现在**方法签名**上（不只是装在容器里）
//   ③ **泛型约束**（`where T : ...`）—— ⭐ **不是继承，是"资格"**
//   ④ 用「**关卡数据**」（关卡号 / 名字 / 宽度）当例子落地
//
// 【怎么用这个文件】
//   `dotnet run` 先跑一遍 → 会看到几行 `⏳ 还没做` → 去文件底部的类型区把 TODO 1~4 补完 →
//   再 `dotnet run`，对着每行后面的「期望」核 → 最后去「编译错误观察区」做三个实验。
//
// 【今天最值钱的一条判据（先记住）】
//   **在你写的泛型代码里，能对 `T` 做什么，完全取决于编译器【知道】`T` 有什么能力。**
//   ⇒ 编译器的"不知道"，正是约束（`where`）要解决的问题。

using System.Reflection.Metadata.Ecma335;
using Microsoft.VisualBasic;

static void TryRun(string title, Action act)
{
    Console.WriteLine($"—— {title} ——");
    try { act(); }
    catch (NotImplementedException ex) { Console.WriteLine($"      ⏳ 还没做：{ex.Message}"); }
    Console.WriteLine();
}

// ═══════ 测试区（期望值写在每行后面的注释里）═══════

List<LevelData> levels = new List<LevelData>
{
    new LevelData(1, "草地关", 40),
    new LevelData(2, "深渊关", 40),
    new LevelData(3, "高塔关", 60),
    new LevelData(4, "试炼关", 30),
};

Console.WriteLine("═══════ 泛型练习 · 游戏关卡数据表 ═══════\n");

TryRun("① LevelData 实现 IComparable<LevelData>（按关卡号比大小）", () =>
{
    Console.WriteLine($"      #1 与 #2 比 → {levels[0].CompareTo(levels[1])}     ← 期望【负数】（1 号排前面）");
    Console.WriteLine($"      #4 与 #4 比 → {levels[3].CompareTo(levels[3])}     ← 期望【0】");
    Console.WriteLine($"      #3 与 #1 比 → {levels[2].CompareTo(levels[0])}     ← 期望【正数】");
});

TryRun("② 泛型方法 CountWhere<T>（**无**约束：只把 T 喂给 Func）", () =>
{
    Console.WriteLine($"      List<LevelData> 里宽 > 35 的有 {LevelOps.CountWhere(levels, lv => lv.Width > 35)} 个     ← 期望 3");
    Console.WriteLine($"      List<int> 里偶数有 {LevelOps.CountWhere(new List<int> { 1, 2, 3, 4, 5 }, n => n % 2 == 0)} 个     ← 期望 2");
    Console.WriteLine("      ⭐ 同一个方法，两种 T —— 因为方法体对 T 的要求只有「能被喂进 Func」，任何 T 都做得到");
});

TryRun("③ 泛型方法 Largest<T>（**需要**约束：要 T 有「能比较」这个能力）", () =>
{
    Console.WriteLine($"      关卡号最大的 = {LevelOps.Largest(levels)}     ← 期望 #4 试炼关（宽 30）");
    Console.WriteLine($"      最大的数 = {LevelOps.Largest(new List<int> { 3, 9, 4 })}     ← 期望 9");
    Console.WriteLine($"      最大的字符串 = {LevelOps.Largest(new List<string> { "pear", "apple", "fig" })}     ← 期望 pear");
    Console.WriteLine("      ⭐ 三种 T 都能用 —— 它们有一个共同点，你能一句话说出来吗？");
});

TryRun("④ new() 约束：凭空造一个 T", () =>
{
    List<int> fresh = LevelOps.CreateDefault<List<int>>();
    Console.WriteLine($"      CreateDefault<List<int>>() → 得到一个 {fresh.GetType().Name}，Count = {fresh.Count}     ← 全新的空列表");
    fresh.Add(7);
    Console.WriteLine($"      往里面塞一个 → Count = {fresh.Count}     ← 它是真对象（不是 null、不是半成品）");
});
// ✅ 已印证（10-08 亲手撞过）：`CreateDefault<LevelData>()` 报 **CS0310** ——
//    「LevelData 必须是具有公共的无参数构造函数的非抽象类型」。
//    原因：`LevelData` 的【带参】构造把【隐式无参】构造**顶掉了**。
//    ⭐ 结论：`new()` 约束是个【筛选器】—— 它挡住的，正是"凭空造出来没有意义"的类型。
//    `LevelData` 被挡住 = 它的设计是对的（一关必须有编号和名字才成立）⇒ 因此本格改用 `List<int>` 验证。

TryRun("⑤ 对照示例（**不用写**，读一读）：泛型类 Dictionary<K, V> 当「关卡数据表」", () =>
{
    Dictionary<int, LevelData> byId = new Dictionary<int, LevelData>();
    foreach (LevelData lv in levels) byId[lv.Id] = lv;      // 覆盖写：同一个 id 再来一次就替掉（幂等）
    Console.WriteLine($"      按编号查 #3 → {byId[3]}");
    Console.WriteLine($"      表里有 {byId.Count} 条");
    Console.WriteLine($"      查不存在的编号：byId[99] 会抛 KeyNotFoundException；TryGetValue 只回 false → {byId.TryGetValue(99, out LevelData? hit)}");
    Console.WriteLine("      ⭐ 注意 `byId` 的完整类型名是 `Dictionary<int, LevelData>` —— 两个类型参数，K 与 V 各填一个");
});

// ═══════ 编译错误观察区（做完 ①②③④ 再来，5 分钟）═══════
//
// 玩法：把某一行前面的 `//` 去掉 → `dotnet build` → **抄下错误码** → 再注释回去。
// 目的：让"约束"这件事从「知道」变成「亲眼见过」。
//
//   A. 顺序实验 —— `new()` 必须写在约束列表【最后】
//      // public class OrderA<T> where T : new(), IComparable<T> { }        ← 期望报 CS0401
//      public class OrderB<T> where T : IComparable<T>, new() { }            ← 对照：这样才合法
//
//   B. 不写约束就用"T 的能力"
//      // static bool SameNoConstraint<T>(T a, T b) => a.CompareTo(b) == 0; ← 期望报 CS1061
//      static bool SameWithConstraint<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) == 0;  ← 对照
//
//   C. `new T()` 却没有 `new()` 约束
//      // static T MakeNoConstraint<T>() => new T();                       ← 期望报 CS0304
//      static T MakeWithConstraint<T>() where T : new() => new T();          ← 对照
//
// ⚠️ 这三组是**故意让编译失败**的 —— 报错就是【通过】。别去"修"，看清错误码就够。

Console.WriteLine("（全部完成后，去「编译错误观察区」做那三个实验）");

// ═══════ 类型区 ═══════

public class LevelData : IComparable<LevelData>
{
    public int Id { get; }
    public string Name { get; }
    public int Width { get; }

    public LevelData(int id, string name, int width)      // ⚠️ 这个【带参】构造会顶掉隐式无参构造（TODO 4 要用到这件事）
    {
        Id = id;
        Name = name;
        Width = width;
    }

    // TODO 1：按 Id 比大小（实现 IComparable<LevelData>）
    //   · 判据：返回【负数】= 我排前面；【0】= 一样大；【正数】= 我排后面（⚠️ 不是 bool！）
    //   · 提示：`int` 自己就实现了 IComparable<int> —— 它能比较两个 int，返回值正好是上面那三个意思
    //   · ⚠️ 参数是可空的：别的节点可能是 null，想一想 null 该排在哪边（先想，再写）
    public int CompareTo(LevelData? other) => other is null ? 1 : Id.CompareTo(other.Id);
    public override string ToString() => $"#{Id} {Name}（宽 {Width}）";
}

public static class LevelOps
{
    // TODO 2：泛型方法（**无约束**）—— 数一数 items 里有多少个满足 predicate
    //   ⭐ 为什么这里不需要约束？先自己答一遍：
    //      "方法体对 T 做的唯一一件事是什么？" —— 如果任何 T 都做得了，编译器就不需要你交证明
    public static int CountWhere<T>(List<T> items, Func<T, bool> predicate)
    {
        int times = 0;
        foreach(T t in items)
        {
            if(predicate(t)) times++;
        }
        return times;
    }

    // TODO 3：泛型方法（**需要约束**）—— 返回 items 里最大的那个
    //   · 写的时候先**不写约束**、直接去比较两个 T → 看编译器报什么错（错误码抄进复盘）
    //   · 然后补上约束。提示：`int` / `string` / 你刚写的 `LevelData` 有一个共同点
    //     —— 它们都实现了一个"以自己为参数"的接口，那个接口正是"能比较"这张证明
    //   · ⚠️ 顺带想：空列表该返回什么？（别急着写，先定"契约"）
    public static T Largest<T>(List<T> items) where T : IComparable<T>
    {
        if(items.Count == 0) throw new InvalidOperationException("items 不能是空列表");
        T result = items[0];
        foreach(T i in items)
        {
            if(i.CompareTo(result) > 0) result = i;
        }
        return result;
    }

    // TODO 4：泛型方法（**new() 约束**）—— 凭空造一个 T 出来
    //   · 先把约束补对，再拿 `CreateDefault<LevelData>()` 试 → ⚠️ 猜猜编译能不能过？（回头看 LevelData 的构造函数）
    //   · 撞上错误后，把"为什么"写进复盘：**为什么"有构造函数"还不够？**
    public static T CreateDefault<T>() where T : new() => new T();
}
