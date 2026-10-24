// ═══════ 判题工具（承 704 / 35 / 34：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：69 的前提只有一条 —— x 非负（题面：0 <= x <= 2^31 - 1）。
// ⚠️ 注意【没有】"数组已排序"这一条：本题的"有序数据"不是别人给的，是你自己造出来的
//    （候选答案 0,1,2,… 天生升序），所以这道题真正要自检的前提只剩定义域。
static bool PreconditionOk(int x) => x >= 0;

// 期望值的【独立来源】：浮点开方 + 双侧校正。
//   Math.Sqrt 的 53 位尾数在 int 范围内其实够用，但"够用"这件事必须由校正兜住 ——
//   把定义域换到 long（> 2^53）它就不再成立，所以别把浮点开方当定理用。
// ⚠️ 它只用来算期望值，【不进 versions】—— 否则 Bench 就变成自己跟自己比。
static int NaiveSqrt(int x)
{
    long r = (long)Math.Sqrt(x);
    while ((r + 1) * (r + 1) <= x) r++;   // 用 long 乘法，避免"校正"这一步自己先溢出
    while (r * r > x) r--;
    return (int)r;
}

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
//    这是 10-21 的真实教训：写完新函数却全绿，因为判题器只认表里登记过的那个名字。
(string Name, Func<int, int> Run)[] versions =
{
    ("MySqrt（你的版本）", x => new Solution().MySqrt(x)),
};

int pass = 0, total = 0;

void Check(string title, int x, int? expected = null)
{
    total++;
    if (!PreconditionOk(x))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】(x 必须 >= 0)，这组不算数");
        return;
    }

    int want = expected ?? NaiveSqrt(x);
    string src = expected is null ? "（期望值由浮点开方 + 校正当场算出）" : "";

    List<string> fails = new List<string>();
    foreach ((string name, Func<int, int> run) in versions)
    {
        try
        {
            int actual = run(x);
            if (actual != want) fails.Add($"{name} → 期望 {want}，实际 {actual}");
        }
        catch (Exception ex)
        {
            fails.Add($"{name} → 抛异常 {ex.GetType().Name}: {ex.Message}");
        }
    }

    if (fails.Count == 0)
    {
        pass++;
        Console.WriteLine($"✅ {title}");
    }
    else
    {
        Console.WriteLine($"❌ {title}{src}");
        Console.WriteLine($"     x = {x}   期望 {want}");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 探针（不计对错 · 只打印）═══════
// ⚠️ 本题【故意不做性能探针】，理由是可验证的、不是"我觉得"：
//    x <= 2^31 - 1 ⇒ 答案上界 = floor(sqrt(2147483647)) = 46340
//    ⇒ 就算线性枚举 0…46340 也是微秒级 ⇒ 造不出有意义的差距，硬比只会得到一个假比拼。
//    69 练的是【判定式与边界】，速度留给 704 / 34 那两题。
//    所以这里换成【全量对账】：手写几十个用例总有盲区，全量扫一遍没有。
void VerifyAll()
{
    Console.WriteLine("── 探针 A：全量对账 0 ~ 300000（每个 x 都与独立来源比对）──");
    {
        Solution sol = new Solution();
        long sinkMine = 0, sinkNaive = 0;
        int bad = 0;
        string firstBad = "";

        Stopwatch sw = Stopwatch.StartNew();
        for (int x = 0; x <= 300_000; x++)
        {
            int mine = sol.MySqrt(x);
            int want = NaiveSqrt(x);
            sinkMine += mine;
            sinkNaive += want;
            if (mine != want)
            {
                if (bad == 0) firstBad = $"x = {x}：期望 {want}，实际 {mine}";
                bad++;
            }
        }
        sw.Stop();

        Console.WriteLine($"⏱ 你的版本 ：300001 个 x、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkMine}）");
        Console.WriteLine($"⏱ 独立来源 ：同一批 x、期望值之和 sink={sinkNaive}");
        Console.WriteLine(bad == 0
            ? "   ⭐ 两条 sink 相同，全量 0 错"
            : $"   ⛔ 不一致 {bad} 处；第一处 = {firstBad}");
    }

    Console.WriteLine();
    Console.WriteLine("── 探针 B：端点与完全平方数附近（溢出 / 差一 的集中区）──");
    {
        Solution sol = new Solution();
        // 31 位数上界、以及 46340^2 这个"刚好是 / 刚好不是完全平方数"的分水岭
        int[] probes = { 1, 2, 3, 4, 46339, 46340, 46341, 2147395599, 2147395600, int.MaxValue };
        long sinkMine = 0, sinkNaive = 0;

        foreach (int x in probes)
        {
            int mine = sol.MySqrt(x);
            int want = NaiveSqrt(x);
            sinkMine += mine;
            sinkNaive += want;
            Console.WriteLine($"   x = {x,12} → 你 {mine,6}｜期望 {want,6}{(mine == want ? "" : "   ⛔")}");
        }

        Console.WriteLine($"   ⭐ 两条 sink 必须相同（{sinkMine} vs {sinkNaive}）");
        Console.WriteLine("   判据：46340² = 2147395600 本身是【完全平方数】⇒ 输入 2147395600 应得 46340，");
        Console.WriteLine("         输入 2147395599 应得 46339 —— 这一对是「<= 还是 <」写反时唯一照得出来的地方。");
    }

    Console.WriteLine();
    Console.WriteLine("   ⚠️ 本方法【不计对错】—— 它只负责把「有没有盲区」变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 0 <= x <= 2^31 - 1 = 2147483647   ⇒ 【0 是合法输入】，答案 0
//   · 返回 int，是 sqrt(x) 的【整数部分】= floor(sqrt(x))（向下取整，不是四舍五入）
//   · 不允许使用任何内置指数函数 / 开方函数（pow、sqrt 之类）—— 这是本题的"闭卷"条件
//   · 官方示例：x = 4 → 2；x = 8 → 2（8 的平方根 2.828 取整为 2，不是 3）

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   34 教了你一件事：二分的返回值是一条【边界】，不是"某个元素"。
//   69 把这句话换了个场地 —— 你要找的仍然是一条边界，只不过数组是你自己造的：
//
//   ① 先把"我在哪个数组上二分"说清楚（这一步想清楚，代码就是 34 的复印件）：
//        候选数组 = 0, 1, 2, 3, …
//        第 k 个元素的值 = k * k   （单调不减，天生有序）
//        要找的是：**最后一个满足 k*k <= x 的 k**（= 第一个 k*k > x 的位置，再减一）
//
//   ② 判定式是本题唯一的"新东西"，也是唯一会静默出错的地方：
//        mid * mid <= x   ← 用 int 算会【溢出成负数】，判据瞬间翻转，而且【不报错】
//      三种躲法（选一种，别混）：
//        a) (long)mid * mid <= x
//        b) mid <= x / mid        ← 推荐：不用宽类型，但 mid = 0 要单独想清楚
//        c) 把 hi 压到 x / 2 + 1  ← 能缓解，压不干净（x = 2 还得特判）
//
//   ③ 两种收缩风格（选一条走完，别混）：
//        风格 α「闭区间、排除 mid」：while (lo <= hi)
//            if (满足) lo = mid + 1; else hi = mid - 1;      mid = (lo + hi) / 2（下取整）
//            退出后 hi = 最后一个满足、lo = 第一个不满足（两者差 1），取 hi 或 lo - 1 都行
//        风格 β「半开区间、只缩不排」：while (lo < hi)
//            if (满足) lo = mid; else hi = mid - 1;          mid = (lo + hi + 1) / 2（【必须】上取整）
//            退出后 lo == hi == 答案
//      ⚠️ 判据：哪条分支写了 lo = mid / hi = mid，取整方向就必须让 mid 不等于那一端，
//         否则区间一格没缩 ⇒ 死循环（x = 8 时 β 用下取整，走到 lo=2,hi=3 就出不来）。
//
//   ④ 收口判据（写完必须自查）：**x = 0 时你的 lo / hi 初值还能不能进循环？答案对不对？**
//      （hi 取 x 时 hi = 0，循环会跑一轮；hi 取 x/2 时也是 0 ⇒ 两条路都别依赖"循环至少跑一次"）
//
//   ⑤ 复杂度（写完自己算）：时间 O( )　空间 O(1)

// ═══════ 防线：方法体还没写时，整套用例与探针直接跳过（别给你 30 行红字）═══════
bool implemented;
try
{
    _ = new Solution().MySqrt(1);
    implemented = true;
}
catch (NotImplementedException)
{
    implemented = false;
}

if (!implemented)
{
    Console.WriteLine("⏭ MySqrt 还是 throw NotImplementedException —— 用例与探针全部跳过。");
    Console.WriteLine("   先把方法体写出来（路 A 或路 B 选一条），再 dotnet run。");
}
else
{
    // ═══════ 用例区（形状优先：先边界，再规模）═══════
    Check("★ 官方示例 1：x = 4 → 2（完全平方数）", 4, 2);
    Check("★★ 官方示例 2：x = 8 → 2（floor(2.828)，不是四舍五入到 3）", 8, 2);

    Check("★★★ x = 0 → 0（合法输入，也是「循环可能一轮都不跑」的唯一入口）", 0, 0);
    Check("★★ x = 1 → 1", 1, 1);
    Check("★★ x = 2 → 1（最小的非完全平方数）", 2, 1);
    Check("★★ x = 3 → 1", 3, 1);

    Check("★ 完全平方数本身：9 → 3（「刚好等于」这一支）", 9, 3);
    Check("★★ 完全平方数 − 1：15 → 3（16 的前一格）", 15, 3);
    Check("★★ 完全平方数：16 → 4", 16, 4);
    Check("★★ 完全平方数 + 1：17 → 4", 17, 4);

    Check("★ x = 26 → 5", 26, 5);
    Check("★ x = 99 → 9", 99, 9);
    Check("★ x = 100 → 10", 100, 10);
    Check("★★ x = 1000000 → 1000", 1_000_000, 1000);
    Check("★★ x = 999999 → 999", 999_999, 999);

    // ★★★ 上界区：这一片是 int 溢出与「<= / < 写反」的唯一照妖镜
    Check("★★★ 46340² − 1 = 2147395599 → 46339", 2_147_395_599, 46339);
    Check("★★★ 46340²     = 2147395600 → 46340", 2_147_395_600, 46340);
    Check("★★★ 46340² + 1 = 2147395601 → 46340", 2_147_395_601, 46340);
    Check("★★★ int.MaxValue = 2147483647 → 46340（题面上界）", 2_147_483_647, 46340);

    // ═══════ 反例区（每组点名一种错法）═══════
    //   ⛔ A「判定式用 int 乘法 mid * mid」→ int.MaxValue 系列抓：mid 一旦过 46340，乘积溢出成负
    //      症状取决于写法：可能返回 46341、可能死循环、可能静默给 0 —— 都不报错，只有端点用例照得出
    //   ⛔ B「返回 mid 而不是收缩后的那一格」→ x = 8 抓（第一次 mid 可能就命中 2 或 3，取决于 lo/hi 初值）
    //   ⛔ C「风格 β 写了 lo = mid，却用 (lo+hi)/2」→ 死循环，dotnet run 会一直挂着，Ctrl+C 停
    //   ⛔ D「循环条件写成 lo < hi 且最后不校验」→ 完全平方数附近差一（16 / 17 / 2147395600 一组抓）
    //   ⛔ E「hi 初值取 x」→ 本身没错，但 x = 2^31-1 时只是多跑几轮；若同时用 int 乘法，溢出更早发生
    //   ⛔ F「直接用 Math.Sqrt 强转」→ int 范围内结果【是对的】（浮点 53 位够），所以这套用例抓不到；
    //      它不是"会错"，而是"绕过了要练的东西"—— 定义域扩到 long（> 2^53）才会真错
    //   ⛔ G「x = 0 特判漏了 / mid = 0 时 x / mid 除零」→ 用除法判定式（写法 b）时的专属坑

    VerifyAll();

    // ═══════ 结果汇总 ═══════
    Console.WriteLine();
    Console.WriteLine($"—— 正确性 {pass} / {total} ——");
}

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 返回 floor(sqrt(x))，即最大的 k 使 k * k <= x。
    //
    //   ── 落笔顺序（照"边界"的思路走，别背模板）：
    //      ① 先写一句注释，说清你要找的是哪条边界：
    //         「最后一个满足 k*k <= x 的 k」 或者 「第一个满足 k*k > x 的 k，再减一」—— 二选一，写死它
    //      ② 定区间：lo / hi 各是什么含义？（候选答案的范围，不是数组下标）
    //      ③ 定判定式：用 (long) 乘法还是除法？（选定之后，全方法只准用这一种）
    //      ④ 定收缩方向：满足时该往哪边走？不满足时呢？（自己拿 x = 8 手推三轮验证）
    //      ⑤ 定返回值：答案落在 lo 还是 hi 还是 lo - 1？（拿 x = 0 和 x = 2 各验一遍）
    //
    //   ── 三条硬判据（拿它对，不通过就是错）：
    //      ① x = 8 → 2（不是 3）
    //      ② x = 0 → 0（不崩、不越界）
    //      ③ x = 2147483647 → 46340（判定式没溢出）
    //      并且【没有】从 mid 向左右线性试探的循环（那是 O(sqrt x)，不是二分）
    //
    //   ── 填完自问三句：
    //      · 我的 mid 是怎么取的？如果答案是"最后一个满足"，mid 取 (lo+hi)/2 会怎样（拿 x = 8 手推）
    //      · 判定式为什么不能直接写 mid * mid <= x？拿 x = int.MaxValue、mid = 46341 手算一遍
    //      · 如果改用除法 mid <= x / mid，mid = 0 时这一行会不会炸？为什么（提示：0 作除数）
    //
    //   ── 复杂度（自己填，别空着）：时间 O(logn)　空间 O(1)
    public int MySqrt(int x)
    {
        int lo = 0;
        int hi = x;
        while(lo < hi)
        {
            int mid = (lo + hi + 1) / 2;
            if(mid > x / mid) hi = mid - 1;
            else if(mid < x / mid) lo = mid;
            else return mid;
        }
        return lo;
    }
}
