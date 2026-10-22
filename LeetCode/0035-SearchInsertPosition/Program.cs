// ═══════ 判题工具（承 704 / 700 / 108：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：35 的整个前提 = nums 升序且【无重复】。
// 有重复时"插入位置"本身就不唯一（[1,1,2] 查 1 → 0 和 1 都说得通），题面也就不成立。
static bool PreconditionOk(int[] nums)
{
    for (int i = 1; i < nums.Length; i++)
        if (nums[i - 1] >= nums[i]) return false;
    return true;
}

// 期望值的【独立来源】：线性扫出"第一个 >= target 的下标"（= 下界 lower_bound）。
// ⚠️ 它 O(n)，只用于算期望值，【不进 versions】—— 否则 Bench 就变成自己跟自己比。
static int NaiveLowerBound(int[] nums, int target)
{
    for (int i = 0; i < nums.Length; i++)
        if (nums[i] >= target) return i;
    return nums.Length;
}

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
//    这条是 10-21 的真实教训：写完 Search1 却全绿，因为判题器只认 Search。
(string Name, Func<int[], int, int> Run)[] versions =
{
    ("SearchInsert（你的版本）", (nums, t) => new Solution().SearchInsert(nums, t)),
};

int pass = 0, total = 0;

void Check(string title, int[] nums, int target, int? expected = null)
{
    total++;
    if (!PreconditionOk(nums))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】(非严格升序)，这组不算数");
        return;
    }

    int want = expected ?? NaiveLowerBound(nums, target);
    string src = expected is null ? "（期望值由线性下界现算）" : "";

    List<string> fails = new List<string>();
    foreach ((string name, Func<int[], int, int> run) in versions)
    {
        int actual = run(nums, target);
        if (actual != want) fails.Add($"{name} → 期望 {want}，实际 {actual}");
    }

    if (fails.Count == 0)
    {
        pass++;
        Console.WriteLine($"✅ {title}");
    }
    else
    {
        string head = nums.Length <= 12 ? string.Join(",", nums) : $"(n={nums.Length})";
        Console.WriteLine($"❌ {title}{src}");
        Console.WriteLine($"     nums = [{head}]   target = {target}");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么必须有它：**正确性判题器分不清 O(log n) 和 O(n)** ——
// 随手写个 `for` 从头扫到尾，上面照样全绿。这个探针把差距放大成肉眼可见的数字。
void Bench()
{
    int n = 10_000;                        // 题目允许的上限
    int[] big = new int[n];
    for (int i = 0; i < n; i++) big[i] = i * 2 - 9999;   // 严格升序、互不相同

    int reps = 20_000;
    int[] queries = new int[reps];
    for (int r = 0; r < reps; r++)
    {
        int k = r % n;
        // 一半查【存在】（big 全是奇数）、一半查【不存在】（相邻的偶数，正卡在两个元素之间）
        queries[r] = (r & 1) == 0 ? big[k] : big[k] + 1;
    }

    Solution sol = new Solution();

    // 预热：让 JIT 先编译出来，别把编译时间算进成绩
    for (int i = 0; i < 1_000; i++) sol.SearchInsert(big, big[i % n]);

    long sinkMine = 0;
    Stopwatch sw = Stopwatch.StartNew();
    for (int r = 0; r < reps; r++) sinkMine += sol.SearchInsert(big, queries[r]);
    sw.Stop();
    Console.WriteLine($"⏱ 你的版本   ：n = {n}、查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkMine}）");

    long sinkNaive = 0;
    sw.Restart();
    for (int r = 0; r < reps; r++) sinkNaive += NaiveLowerBound(big, queries[r]);
    sw.Stop();
    Console.WriteLine($"⏱ 线性基线   ：n = {n}、查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkNaive}）");

    Console.WriteLine();
    Console.WriteLine($"   ⭐ 两条 sink 必须相同（{sinkMine} vs {sinkNaive}）：同一批查询给出【同一个下标】，");
    Console.WriteLine("      这比对错更硬 —— 它证明你每一刀都砍在了正确的那半边。");
    Console.WriteLine("   判据：二分 = 每次砍一半 ⇒ 约 20,000 × 14 ≈ 28 万次比较，应在【个位到几十 ms】；");
    Console.WriteLine("         线性扫描 ⇒ 约 20,000 × 5,000 ≈ 1 亿次比较，会到【百 ms 级】（本机实测 ≈ 96 ms）。");
    Console.WriteLine("   ⚠️ 这个方法【不计对错】—— 它只负责把\"你写的是不是二分\"变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 1 <= nums.length <= 10^4        ⇒ 空数组不在约束内
//   · -10^4 <= nums[i], target <= 10^4
//   · nums 为【无重复元素】的升序排列数组 ⇒ 插入位置唯一
//   · 必须使用 O(log n) 的算法          ⇒ 题面直接把线性扫判死，Bench 只是把它照出来
//   ⭐ 一条与 704 不同的地方：**返回值可以等于 nums.Length**（target 比全体都大时），
//      所以"返回值是合法下标"这条假设在这里【不成立】。

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   704 问"在不在"，35 问"该在哪"。中间那座桥叫【收敛点】：
//
//   ① 先把不变量写死（同 704，一个字都不变）：
//        "搜索区间 = [lo, hi]，若答案存在，它必落在区间内"
//      再补一句 35 专属的：**"答案"到底是什么？**
//        —— 答案 = "第一个 >= target 的下标"（下界）。
//      把问题改写成这句之后，命中与不命中就是【同一种情况】了，这是本题最省事的一步。
//
//   ② 一个岔路（自己选，选了就走完）：
//        闭区间 [lo, hi]：初值 lo = 0 / hi = nums.Length - 1，循环条件 ???
//        半开区间 [lo, hi)：初值 lo = 0 / hi = nums.Length，循环条件 ???
//      两条都对；但【初值、循环条件、收缩方式、最后 return 谁】四者必须自洽。
//
//   ③ 循环体只该有三件事：算 mid → 命中就 return → 否则砍掉不含答案的那一半（含 mid）。
//      ⚠️ 如果不命中时不 return，让循环自然结束 —— 那退出时 lo 是什么？（这是本题的全部难点）
//
//   ④ 最容易自欺的一点：`Array.IndexOf` / 一次 for 扫到底，上面照样全绿。
//      ⇒ 判据是 Bench 那两行（以及两条 sink 是否相同）。
//
//   ⑤ 复杂度（写完自己算）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
Check("★ 官方示例 1：[1,3,5,6] 查 5 → 2（命中，同 704）", new int[] { 1, 3, 5, 6 }, 5, 2);
Check("★★ 官方示例 2：[1,3,5,6] 查 2 → 1（不命中 ⇒ 插入位置）", new int[] { 1, 3, 5, 6 }, 2, 1);
Check("★★ 官方示例 3：[1,3,5,6] 查 7 → 4（= nums.Length，返回值越界是合法的）", new int[] { 1, 3, 5, 6 }, 7, 4);
Check("★★ 官方示例 4：[1,3,5,6] 查 0 → 0（插在最前）", new int[] { 1, 3, 5, 6 }, 0, 0);

Check("★★ 单元素命中 [5] 查 5 → 0", new int[] { 5 }, 5, 0);
Check("★★ 单元素插在前面 [5] 查 3 → 0", new int[] { 5 }, 3, 0);
Check("★★★ 单元素插在后面 [5] 查 9 → 1（最容易被写死成 0 的地方）", new int[] { 5 }, 9, 1);

Check("★★ 两元素夹缝 [1,3] 查 2 → 1（相邻区间的收敛 = 本题真门槛）", new int[] { 1, 3 }, 2, 1);
Check("★ 两元素命中左 [1,3] 查 1 → 0", new int[] { 1, 3 }, 1, 0);
Check("★ 两元素命中右 [1,3] 查 3 → 1", new int[] { 1, 3 }, 3, 1);
Check("★★ 两元素插最前 [1,3] 查 0 → 0", new int[] { 1, 3 }, 0, 0);
Check("★★ 两元素插最后 [1,3] 查 4 → 2", new int[] { 1, 3 }, 4, 2);

Check("★ 偶数长度命中正中 [0,1,2,3,4,5] 查 3 → 3", new int[] { 0, 1, 2, 3, 4, 5 }, 3, 3);
Check("★ 奇数长度命中正中 [0,1,2,3,4,5,6] 查 3 → 3", new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3, 3);
Check("★ 奇数长度插正中 [0,1,2,4,5,6,7] 查 3 → 3", new int[] { 0, 1, 2, 4, 5, 6, 7 }, 3, 3);

Check("★ 含负数 [-5,-3,-1,2,4] 查 -3 → 1（命中）", new int[] { -5, -3, -1, 2, 4 }, -3, 1);
Check("★ 含负数夹缝 [-5,-3,-1,2,4] 查 -4 → 1（插在 -5 与 -3 之间）", new int[] { -5, -3, -1, 2, 4 }, -4, 1);
Check("★ 全负数命中末尾 [-9,-7,-5,-3,-1] 查 -1 → 4", new int[] { -9, -7, -5, -3, -1 }, -1, 4);
Check("★ 值域下界 [-9999,0,9999] 查 -9999 → 0", new int[] { -9999, 0, 9999 }, -9999, 0);
Check("★ 值域下界之外 [-9999,0,9999] 查 -10000 → 0", new int[] { -9999, 0, 9999 }, -10000, 0);
Check("★ 值域上界 [-9999,0,9999] 查 9999 → 2", new int[] { -9999, 0, 9999 }, 9999, 2);
Check("★ 值域上界之外 [-9999,0,9999] 查 10000 → 3", new int[] { -9999, 0, 9999 }, 10000, 3);

// 规模用例：题目上限 n = 10^4（期望值交给线性下界现算，见 Check 的第二参）
int[] big = new int[10_000];
for (int i = 0; i < big.Length; i++) big[i] = i * 2 - 9999;   // 严格升序、互不相同
Check("★★★ 规模 n=10000，查首位 → 0", big, big[0], 0);
Check("★★★ 规模 n=10000，查末位 → 9999", big, big[^1], 9999);
Check("★★★ 规模 n=10000，查比全体都大 → 10000", big, 10_000, 10_000);
Check("★★★ 规模 n=10000，查夹缝（不存在）→ 期望值现算", big, 5000);

// ═══════ 反例区（每组点名一种错法）═══════
//   ⛔ A「找不到就 return -1」→ 示例 2 / 3 / 4 全红（这是从 704 直接搬过来的头号惯性）
//   ⛔ B「返回 mid 而不是收敛点 lo」→ 示例 2 抓：最后一轮 mid 落在 0，答案却是 1
//   ⛔ C「while (lo < hi) 混闭区间初值」→ 单元素 [5] 查 9 抓（区间剩 1 个就退出了）
//        ★ 另配一组专抓「lo = mid / hi = mid 忘了 ±1」：[1,3] 查 2 → 卡死，Ctrl+C 停掉
//   ⛔ D「线性扫描一遍过」→ 上面全绿，只有 Bench 能照出来（记得比对两行 sink）

Bench();

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 返回 target 的下标；不存在则返回它应被插入的位置（= 第一个 >= target 的下标）。
    //
    //   ── 落笔顺序（照不变量走，别背模板）：
    //      ① 先写一句注释，说清两件事：你选的区间（"[lo, hi] 闭区间，初值 ?"）
    //         和"循环结束时 lo 代表什么"（提示：这一步决定了本题与 704 的分水岭）
    //      ② 再写初值 + while 条件 —— 一起写，它们是同一句话的两半
    //      ③ 循环体三件事：算 mid → 命中 return → 砍半（含 mid）
    //      ④ 最后落 return —— 想清楚为什么命中与不命中能【共用】同一个返回值
    //
    //   ── 三条硬判据（拿它对，不通过就是错）：
    //      ① [1,3,5,6] 查 2 → 1（不是 -1）
    //      ② [1,3,5,6] 查 7 → 4（允许返回 nums.Length）
    //      ③ [1,3,5,6] 查 0 → 0
    //      并且 ①②③ 都【没有】专门写的 if 分支 —— 全靠同一个收敛点
    //
    //   ── 填完自问三句：
    //      · 我这个循环【一定会结束】吗？凭什么结束？（每轮 lo 或 hi 至少动一个）
    //      · 答案为什么会【正好落在】lo 上，而不是 lo−1 / hi？
    //      · 如果题目改成"第一个 > target 的下标"（upper_bound），我改哪一行？
    //
    //   ── 复杂度（自己填，别空着）：时间 O( )　空间 O( )
    public int SearchInsert(int[] nums, int target)
    {
        int lo = 0;
        int hi = nums.Length - 1;
        while(lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if(nums[mid] == target) return mid;
            if(nums[mid] < target) lo = mid + 1;
            if(nums[mid] > target) hi = mid - 1;
        }
        return lo;
    }
}
