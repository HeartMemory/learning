// ═══════ 判题工具（承 704 / 35 / 34 / 69 / 367 / 74：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：153 说的是"由【严格升序】数组旋转 k 次得到"。能拿来当前提的有三条：
//   ① 元素互不相同          ← 网页版 153 保证 unique（带重复的是 154，判断会完全不同）
//   ② 至多【一次】下降       ← nums[i] > nums[i+1] 的位置至多一处
//   ③ 有下降时，末元素必须【小于】首元素（环形也只剩一次下降）
//       ⚠️ ② 单独不够：[−3,−1,−2] 线性只下降一次，但环形里下降两次（−1→−2、−2 绕回 −3 之后）
//          ⇒ 它根本不是旋转来的，期望值也就没有意义（这正是"用例自己要先验"的理由）
static bool PreconditionOk(int[] a)
{
    if (a is null || a.Length == 0) return false;

    HashSet<int> seen = new HashSet<int>();
    foreach (int v in a)
        if (!seen.Add(v)) return false;                 // ① 互不相同

    int drops = 0;
    for (int i = 1; i < a.Length; i++)
        if (a[i] < a[i - 1]) drops++;                   // ② 至多一处下降
    if (drops > 1) return false;
    if (drops == 1 && a[a.Length - 1] >= a[0]) return false;   // ③ 环形也只剩一次下降
    return true;
}

// 参考实现（只用来当"标准答案"，不参与判分）：全扫一遍，O(n)
static int LinearMin(int[] a)
{
    int m = a[0];
    foreach (int v in a) if (v < m) m = v;
    return m;
}

// 打印数组的简写：长数组只显示头尾（否则 5000 个元素会把终端刷爆 ⇒ 看起来像"卡死"）
static string Fmt(int[] a)
{
    const int head = 8, tail = 4;
    if (a.Length <= head + tail + 1) return "[" + string.Join(",", a) + "]";
    return "[" + string.Join(",", a.Take(head)) + $", …… 共 {a.Length} 个 …… ,"
         + string.Join(",", a.Skip(a.Length - tail)) + "]";
}

// 造"旋转数组"的简写：把严格升序的 lo.. 按 k 切开重拼
static int[] Rotated(int k, params int[] ascending)
{
    int n = ascending.Length;
    k %= n;
    int[] r = new int[n];
    for (int i = 0; i < n; i++) r[i] = ascending[(i + k) % n];
    return r;
}

// ⚠️ 参与判题的版本登记表：新写一个实现就往这里加一行（不加 = 它一次都不会被跑到）
(string Name, Func<int[], int> Run)[] versions =
{
    ("FindMin", a => new Solution().FindMin(a)),
    // 第二版写完，在这里补一行（互证才有意义）：
    // ("FindMin2（???版）", a => new Solution().FindMin2(a)),
};

// ═══════ 作答区是否就绪（没写时用"尺子校准"先跑，别一片红）═══════
static bool SolutionReady()
{
    try
    {
        new Solution().FindMin(new[] { 1 });
        return true;
    }
    catch (NotImplementedException)
    {
        return false;
    }
    catch
    {
        return true;   // 能抛别的异常 ⇒ 代码已经写了（只是有 bug），照常判题
    }
}

int pass = 0, total = 0;

void Check(string title, int[] nums, int expected)
{
    total++;
    if (!PreconditionOk(nums))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】（互不相同 / 至多一次下降），这组不算数");
        return;
    }

    List<string> fails = new List<string>();
    foreach ((string name, Func<int[], int> run) in versions)
    {
        try
        {
            int actual = run(nums);
            if (actual != expected) fails.Add($"{name} → 期望 {expected}，实际 {actual}");
        }
        catch (Exception e)
        {
            // 越界 / 空引用会被抓在这里，而不是把整个程序炸掉
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
        Console.WriteLine($"❌ {title}   (nums = {Fmt(nums)})");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 全量对账：把"每一种长度 × 每一个旋转点"都枚举一遍 ═══════
// 为什么要有它：手写用例只覆盖"我想得到的边界"。而这题的形状空间很小（长度 × 旋转点），
// 完全枚举得起 —— 长度 1..12 的每个旋转点 + 长度 20 的 20 个旋转点，随机值再刷 20 轮。
// 这样"未旋转 / 最小值在末位 / 最小值在次末位 / 单元素 / 两元素"全都在里面。
void CrossCheck()
{
    Random rng = new Random(20261028);
    int probes = 0, mismatches = 0;

    for (int trial = 0; trial < 20; trial++)
    {
        foreach (int n in new[] { 1, 2, 3, 4, 5, 8, 20 })
        {
            int[] asc = new int[n];
            int cur = rng.Next(-50, 0);
            for (int i = 0; i < n; i++) { cur += rng.Next(1, 5); asc[i] = cur; }   // 严格递增

            for (int k = 0; k < n; k++)
            {
                int[] nums = Rotated(k, asc);
                int want = LinearMin(nums);

                foreach ((string name, Func<int[], int> run) in versions)
                {
                    probes++;
                    int actual;
                    try { actual = run(nums); }
                    catch (Exception e)
                    {
                        mismatches++;
                        if (mismatches <= 8) Console.WriteLine($"❌ 对账崩了：{name} / n={n} k={k} → {e.GetType().Name}");
                        continue;
                    }
                    if (actual != want)
                    {
                        mismatches++;
                        if (mismatches <= 8)
                            Console.WriteLine($"❌ 对账不符：{name} / n={n} k={k} nums={Fmt(nums)} → 期望 {want}，实际 {actual}");
                    }
                }
            }
        }
    }

    Console.WriteLine(mismatches == 0
        ? $"✅ 全量对账：{probes} 次 / 0 处不符（长度 1/2/3/4/5/8/20 × 每个旋转点 × 20 轮随机值）"
        : $"❌ 全量对账：{probes} 次 / {mismatches} 处不符");
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么要有它：正确性判题器分不清 O(log n) 与 O(n) —— 两版都能全绿。
void Bench()
{
    const int n = 5_000;                     // 题面上限量级
    int[] asc = new int[n];
    for (int i = 0; i < n; i++) asc[i] = i - 2500;
    int[] nums = Rotated(n / 3, asc);        // 故意让最小值落在中间

    const int reps = 5_000;

    foreach ((string name, Func<int[], int> run) in versions)
    {
        try
        {
            for (int i = 0; i < 200; i++) run(nums);   // 预热：别把 JIT 时间算进来

            int sink = 0;
            Stopwatch sw = Stopwatch.StartNew();
            for (int r = 0; r < reps; r++) sink += run(nums);
            sw.Stop();

            Console.WriteLine($"⏱ {name}：n={n} 查 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms（sink={sink}）");
        }
        catch (Exception e)
        {
            Console.WriteLine($"⏱ {name}：跳过（{e.GetType().Name}）");
        }
    }

    {
        int sink = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++) sink += LinearMin(nums);
        sw.Stop();
        Console.WriteLine($"⏱ 线性基线（参考实现）：n={n} 查 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms（sink={sink}）");
    }

    Console.WriteLine();
    Console.WriteLine("   判据：二分 ⇒ 每查 ≤ 13 步（log₂5000 ≈ 12.3）⇒ 应在【个位 ms】档；");
    Console.WriteLine("         线性 ⇒ 每查 5000 步 ⇒ 比它慢两个数量级。");
    Console.WriteLine("   ⚠️ 这个方法【不计对错】—— 它只把\"你写的是哪一档\"变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 1 <= nums.length <= 5000
//   · -5000 <= nums[i] <= 5000
//   · nums 里的元素【互不相同】（⇒ 带重复的版本是 154，判段方式会变）
//   · nums 是【严格升序数组旋转 1..n 次】得到的（k = 0 也允许，即"没转过"）
//   · 要求 O(log n)
//   ⭐ 一句话抓住这题：**旋转只干了一件事 —— 把一条升序切成两段，并把第一段搬到后面。**

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   ① 先回答"我要找的东西，在数组里长什么样"：
//        最小值 = 断点（那唯一一次下降）【右侧】的第一个元素。
//        断点不存在（k = 0）时，最小值就是 nums[0] —— 这不是特殊情况，是同一个描述的退化情形。
//        这一句写进注释 —— 判据是"能说清为什么"，不是"模板背得熟"。
//
//   ② 比较对象怎么选（本题唯一的新东西）：
//        二分每步只能问一个"是/否"。问法必须满足：**答完就能整段砍掉**。
//        候选是 nums[mid] 跟谁比 —— 想想这两问：
//          · 跟【右端】比，能不能一刀判断"mid 落在那一段"？为什么？
//          · 跟【左端】比，会在哪种数组上失灵？举一个具体数组反驳它。
//        ⚠️ 能用右端的理由，本质是"右端一定在第二段（或者等于最小值）"—— 自己把这句话补完。
//
//   ③ 与 35 的下界同构（同一套模板，只换比较对象）：
//        35 找的是"第一个 ≥ target 的位置"，本题找的是"第一个 ≤ ??? 的位置"。
//        ⭐ 关键：这里的 ??? 不是外面传进来的参数，而是**从数组自身取到的一个量**。
//        想清楚"取谁、为什么它一定存在、为什么它在旋转/未旋转两种情况下都成立"。
//
//   ④ 区间三件套必须自洽（与 704 / 34 同一套）：初值 · 循环条件 · 收缩是否排除 mid。
//        `while (lo < hi)` 与 `while (lo <= hi)` 都能写对，但**配的收缩方式不同**：
//        自问 —— 循环停下来时，[lo, hi] 里还剩几个元素？答案一定是那个元素吗？
//
//   ⑤ 复杂度（写完自己填，别空着）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
void RunCases()
{
// —— 官方示例 ——
Check("★ 官方示例 1：[3,4,5,1,2] → 1", new[] { 3, 4, 5, 1, 2 }, 1);
Check("★ 官方示例 2：[4,5,6,7,0,1,2] → 0", new[] { 4, 5, 6, 7, 0, 1, 2 }, 0);
Check("★ 官方示例 3：[11,13,15,17] → 11（k = 0，没转过）", new[] { 11, 13, 15, 17 }, 11);

// —— 长度边界 ——
Check("★★ 单元素：[1] → 1", new[] { 1 }, 1);
Check("★★ 两元素·转过：[2,1] → 1", new[] { 2, 1 }, 1);
Check("★★ 两元素·没转：[1,2] → 1", new[] { 1, 2 }, 1);

// —— 断点位置（"最小值在哪"的三种主战场）——
Check("★★ 最小值在【末位】：[2,3,4,5,1] → 1", new[] { 2, 3, 4, 5, 1 }, 1);
Check("★★ 最小值在【次末位】：[3,4,5,1,2] → 1", new[] { 3, 4, 5, 1, 2 }, 1);
Check("★★ 最小值在【中间】：[5,1,2,3,4] → 1", new[] { 5, 1, 2, 3, 4 }, 1);
Check("★ 两元素·最小值在末位：[5,4] → 4", new[] { 5, 4 }, 4);

// —— 负数（约束下界 −5000）——
Check("★ 含负数：[3,4,-2,0,1] → -2", new[] { 3, 4, -2, 0, 1 }, -2);
Check("★ 全负·转过：[-2,-1,-3] → -3", new[] { -2, -1, -3 }, -3);
Check("★ 全负·没转：[-9,-7,-5] → -9", new[] { -9, -7, -5 }, -9);

// —— 规模上限 5000（两个极端旋转点）——
int[] bigAsc = new int[5000];
for (int i = 0; i < bigAsc.Length; i++) bigAsc[i] = i * 2 - 5000;
Check("★★★ 5000 长 · k = 0（未旋转）→ 首元素", Rotated(0, bigAsc), bigAsc[0]);
Check("★★★ 5000 长 · k = 4999（最小值在末位）→ 末元素", Rotated(4999, bigAsc), bigAsc[0]);
Check("★★★ 5000 长 · k = 2500（最小值在正中间）→ 中间元素", Rotated(2500, bigAsc), bigAsc[0]);
}

// ═══════ 尺子校准（Solution 没写时先跑它）═══════
// 验的是【判题器自己】：① 造的数组都真的合法（前提自检全过）
//                     ② 暴力参考实现算出来的最小值 == 构造时埋进去的最小值（asc[0]）
void CalibrateRuler()
{
    Random rng = new Random(20261028);
    int built = 0, bad = 0;

    for (int trial = 0; trial < 20; trial++)
    {
        foreach (int n in new[] { 1, 2, 3, 4, 5, 8, 20 })
        {
            int[] asc = new int[n];
            int cur = rng.Next(-50, 0);
            for (int i = 0; i < n; i++) { cur += rng.Next(1, 5); asc[i] = cur; }

            for (int k = 0; k < n; k++)
            {
                int[] nums = Rotated(k, asc);
                built++;
                if (!PreconditionOk(nums)) bad++;                  // 造出来的必须合法
                if (LinearMin(nums) != asc[0]) bad++;              // 暴力最小值必须 == 埋进去的最小值
            }
        }
    }

    Console.WriteLine(bad == 0
        ? $"✅ 尺子校准：{built} 个随机旋转数组，前提自检全过、暴力最小值 == 构造的最小值"
        : $"❌ 尺子校准：{bad} 处不对（判题器自己的问题，先修它）");
}

Console.WriteLine();
if (!SolutionReady())
{
    Console.WriteLine("⏭ Solution.FindMin 还是 throw NotImplementedException —— 用例的判分与对账全部跳过。");
    CalibrateRuler();
}
else
{
    CalibrateRuler();
    RunCases();
    CrossCheck();
    Bench();
}

// ═══════ 反例区（每组点名一种错法，写的时候自己对照）═══════
//   ⛔ A「拿 nums[lo] 当判断基准」→ 在"最小值恰在末位"的数组上判反（[2,3,4,5,1] 是主战场）
//   ⛔ B「忘了 k = 0」→ [11,13,15,17] 会返回 nums[1]（只有"没转过"的用例能照出来）
//   ⛔ C「收缩时把 mid 一起砍掉」→ 当 nums[mid] 恰好是最小值时被跳过 ⇒ 答案变成次小
//        ★ 判据：区间只剩一个元素时，它被比较过吗？
//   ⛔ D「`while (lo <= hi)` 配 `hi = mid`」→ 死循环（跑起来卡住就是它，两元素时最明显）
//   ⛔ E「循环条件写 `lo < hi` 却在里面 `lo = mid + 1`」→ 可能跳过答案（用 [2,1] 抓）
//   ⛔ F「用 Math.Min 全扫一遍」→ 结果对，但 Bench 慢两个数量级 ⇒ 只有 Bench 能照出来
//   ⛔ G「先找最大值再 +1 取模」→ 结果多半对，但边界写成 nums[(maxIdx + 1) % n] 时，
//        k = 0 会绕回 nums[0]（恰好对）⇒ 这类"看起来对"的写法只能靠【对账】抓

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 在"严格升序数组旋转 k 次"得到的 nums 里，找最小值。
    //
    //   ── 落笔顺序（照不变量走，别背模板）：
    //      ① 先写一句注释：最小值与断点的关系是什么（含 k = 0 的退化情形）
    //      ② 选比较对象：nums[mid] 跟谁比、为什么这样比能整段砍掉
    //      ③ 写初值 + while 条件 —— 两者是同一句话的两半
    //      ④ 写循环体：算 mid → 比较 → 收缩（想清 mid 要不要留在区间里）
    //
    //   ── 三条硬判据：
    //      ① [11,13,15,17]（没转过）与 [2,3,4,5,1]（最小值在末位）都对
    //      ② [1] / [2,1] 不出界、不死循环
    //      ③ Bench 落在【个位数 ms】档（不是线性档）
    //
    //   ── 填完自问：这个循环【一定停】吗？靠什么停？停下来时 lo 指向的那个元素，
    //        为什么一定是答案（而不是"只是没被排除"）？
    //   ── 复杂度（自己填，别空着）：时间 O( )　空间 O( )
    public int FindMin(int[] nums)
    {
        int low  = 0;
        int high = nums.Length - 1;
        while(low <= high)
        {
            if(nums[high] < nums[low])
            {
                int mid = (low + high) / 2;
                if(nums[mid] > nums[high]) low = mid + 1;
                else high = mid;
            }else return nums[low];
        }
        return nums[low];
    }
}
