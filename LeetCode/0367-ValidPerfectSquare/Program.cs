// ═══════ 判题工具（承 704 / 69：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：这题的前提一句话 —— num ≥ 1（题目约束 1 ≤ num ≤ 2^31 − 1）。
// 破了前提，"期望值"就是在一道不存在的题上算出来的（负数不是完全平方 ⇒ 别为它设计分支）。
static bool PreconditionOk(int num) => num >= 1;

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
//    10-21 的真实缺口：写完 Search1 却全绿，因为判题器只认 Search —— 判题器要能失败。
(string Name, Func<int, bool> Run)[] versions =
{
    ("IsPerfectSquare", n => new Solution().IsPerfectSquare(n)),
    // 第二版写完，在这里补一行（互证才有意义）：
    // ("IsPerfectSquare2（???版）", n => new Solution().IsPerfectSquare2(n)),
};

int pass = 0, total = 0;

// 一组用例要给【所有版本】都判一遍：任何一版错 = 这组红（= 互证）
void Check(string title, int num, bool expected)
{
    total++;
    if (!PreconditionOk(num))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】(num < 1)，这组不算数");
        return;
    }

    List<string> fails = new List<string>();
    foreach ((string name, Func<int, bool> run) in versions)
    {
        try
        {
            bool actual = run(num);
            if (actual != expected) fails.Add($"{name} → 期望 {expected}，实际 {actual}");
        }
        catch (Exception e)
        {
            // 这一行很关键：真实现里的越界 / 溢出异常会被抓在这里，而不是把整个程序炸掉
            fails.Add($"{name} → 抛异常 {e.GetType().Name}：{e.Message}");
        }
    }

    if (fails.Count == 0)
    {
        pass++;
        Console.WriteLine($"✅ {title}");
    }
    else
    {
        Console.WriteLine($"❌ {title}   (num = {num})");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么要它：正确性判题器分不清 O(1) / O(log n) / O(√n) —— 三版都能全绿。
// 这个探针把差距放大成肉眼可见的数字。
void Bench()
{
    // 取 num 的上限附近一批值：二分 ≈ 31 次比较、奇数累加 ≈ 46340 次加法、Math.Sqrt ≈ 1 次
    int reps = 20_000;
    int[] queries = new int[reps];
    for (int r = 0; r < reps; r++) queries[r] = int.MaxValue - r;   // 全是 2^31-1 附近的大数

    foreach ((string name, Func<int, bool> run) in versions)
    {
        try
        {
            for (int i = 0; i < 1_000; i++) run(queries[i]);   // 预热：别把 JIT 编译时间算进成绩

            int sink = 0;
            Stopwatch sw = Stopwatch.StartNew();
            for (int r = 0; r < reps; r++) sink += run(queries[r]) ? 1 : 0;
            sw.Stop();

            Console.WriteLine($"⏱ {name}：查询 {reps} 次（num ≈ 2^31）、总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms（sink={sink}）");
        }
        catch (Exception e)
        {
            Console.WriteLine($"⏱ {name}：跳过（{e.GetType().Name}）");
        }
    }

    Console.WriteLine();
    Console.WriteLine("   判据：二分 ⇒ 每查 31 步、应在【个位到几十 ms】；");
    Console.WriteLine("         奇数累加 / 逐个试 ⇒ 每查 ~46000 步 ⇒ 约 9 亿步，会到【秒级】；");
    Console.WriteLine("         Math.Sqrt ⇒ 常数级，但【边界正确性】只能靠上面的用例证明，不靠速度。");
    Console.WriteLine("   ⚠️ 这个方法【不计对错】—— 它只负责把\"你写的到底是哪一档\"变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 1 <= num <= 2^31 - 1              ⇒ 上界 = int.MaxValue = 2147483647
//   · 返回 true / false（是否完全平方数）
//   ⭐ 两条反常识（今天的地基）：
//      ① 【这题会溢出，704 不会】。46340² = 2,147,395,600 还在 int 里；
//         46341² = 2,147,488,281 已经 > int.MaxValue ⇒ 在 int 里算 mid * mid 会【翻成负数】。
//         判据：**看到"乘法"先问一句"最大能到多大"；看到"加法"同理（691 那种才轮到 /2 溢出）**。
//      ② 【1 是完全平方数】（1 = 1²）⇒ 最小用例直接卡掉"从 2 开始"的写法。

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   367 是 704 / 69 的同族：它练的是【把"是不是"翻译成"找得到吗"】——
//
//   ① 先找到那条【单调性】：
//        f(k) = k² 在 k ≥ 0 上严格递增 ⇒「有没有 k 使 k² = num」是一个【在有序序列里找值】的问题。
//        没有单调性就不能二分 —— 这句话比任何模板都重要。
//
//   ② 二分的区间要自己定，且【上界不是 num】：
//        k ∈ [1, num] 能对，但白跑了多余的 31 步里的一大半；
//        k ∈ [1, 46340] 是紧的（⌊√(2^31−1)⌋ = 46340）—— 想清楚为什么，再落笔。
//        初值 / 循环条件 / 收缩方式【三者必须自洽】（704 那套判据原封不动搬过来）。
//
//   ③ 比较怎么写，直接决定你会不会踩溢出：
//        写法 A：`mid * mid == num`      ⇒ 必须先把 mid 转 long：`(long)mid * mid`
//        写法 B：`mid == num / mid`      ⇒ 全 int 也不溢出，但要处理整除的截断（num / mid 是向下取整）
//        两条都对，选一条走完，并在注释里写清【你为什么选它】。
//
//   ④ 另有两条"曲线"（写法留在你的脑子，不要求实现）：
//        奇数累加：1 + 3 + 5 + … + (2k−1) = k²  ⇒ 一路减到 ≤ 0，看是否恰好落在 0
//        Math.Sqrt：`(int)Math.Sqrt(num)` 再回验 `k * k == num`
//        ⚠️ 后者看着最省事，但它是【浮点】—— 46000 这个量级 double 精确，可一旦换成 long 版就会翻车；
//           而且"回验"这一步删不得（截断 ↓ 会让 15 → 3，3² = 9 ≠ 15 ⇒ 回验就是判据本身）。
//
//   ⑤ 复杂度（写完自己算）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
Check("★ 官方示例 1：16 → true", 16, true);
Check("★ 官方示例 2：14 → false", 14, false);
Check("★ 官方示例 3：1 → true（1 = 1²，最小用例）", 1, true);

Check("★★ 2 → false（最小非平方数；写成 k 从 2 开始会在这里错）", 2, false);
Check("★ 3 → false", 3, false);
Check("★ 4 → true（2²）", 4, true);
Check("★ 9 → true（3²）", 9, true);
Check("★ 15 → false（夹在 3²=9 与 4²=16 之间的典型坑）", 15, false);
Check("★ 25 → true（5²）", 25, true);

Check("★★ 808201 → true（899²，中等规模真平方数）", 808201, true);
Check("★★ 808200 → false（差 1，专治\"±1 边界写反\"）", 808200, false);
Check("★★ 808202 → false（多 1）", 808202, false);

// —— 溢出区（今天的主战场，全在 46340 附近）——
Check("★★★ 2147395600 → true（46340²，int 范围内最大的完全平方数）", 2147395600, true);
Check("★★★ 2147395599 → false（差 1）", 2147395599, false);
Check("★★★ 2147395601 → false（多 1）", 2147395601, false);
Check("★★★ 2147483647 → false（int.MaxValue，√ ≈ 46340.95，无整数解）", int.MaxValue, false);
Check("★★★ 2147483646 → false（int.MaxValue − 1）", int.MaxValue - 1, false);

// ═══════ 反例区（每组点名一种错法）═══════
//   ⛔ A「mid * mid 全用 int」→ 2147395601 / int.MaxValue 那几组会把乘积翻成负数 ⇒ 误判 true
//        ★ 症状指纹：只有 46340² 以上的用例红，小数值全绿
//   ⛔ B「k 的上界取 num 且 mid 算 (lo+hi)/2」→ 这里 lo+hi ≤ 2·2^31 会溢出 ⇒ lo 变负、死循环或异常
//   ⛔ C「用 Math.Sqrt 但删掉回验」→ 15 会变 true（sqrt 截断后 3² ≠ 15）
//   ⛔ D「循环写 while (lo < hi) 且不补最后比一次」→ 808201（899²）这类会漏
//        ★ 判据：**区间被砍到只剩一个元素时，它被比较过吗？**

Bench();

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 判断 num 是否为完全平方数。
    //
    //   ── 落笔顺序（照不变量走，别背模板）：
    //      ① 先写一句注释：你的单调性是什么、k 的区间是什么（含上界为什么取它）
    //      ② 再写初值 + while 条件 —— 两者是同一句话的两半
    //      ③ 最后写循环体：算 mid → 比较（想清溢出）→ 砍半（含 mid）
    //
    //   ── 三条硬判据：
    //      ① 1 → true、2 → false（上界与下界都收得住）
    //      ② 2147395600 → true 且 2147483647 → false（溢出这条线站得住）
    //      ③ Bench 里耗时落在【几十 ms】档（不是秒级）
    //
    //   ── 填完自问：我这个循环【一定停】吗？靠什么停？
    //   ── 复杂度（自己填，别空着）：时间 O(logn)　空间 O(1)
    public bool IsPerfectSquare(int num)
    {
        int lo = 1;
        int hi = num / 2 + 1;
        while (lo <= hi)
        {
            int mid = (hi + lo) / 2;
            int q = num / mid;
            if (q == mid && num % mid == 0) return true;
            else if (q < mid) hi = mid - 1;
            else lo = mid + 1;
        }
        return false;
    }
}
