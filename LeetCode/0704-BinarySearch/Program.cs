// ═══════ 判题工具（承 700 / 108：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：这道题的整个前提就一句话 —— nums 严格升序。
// 前提破了，后面所有"期望值"都是在错误的题目上算出来的。
static bool PreconditionOk(int[] nums)
{
    for (int i = 1; i < nums.Length; i++)
        if (nums[i - 1] >= nums[i]) return false;
    return true;
}

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
//    这是 10-21 踩到的真实缺口：写完 Search1 却全绿，因为判题器只认 Search。
(string Name, Func<int[], int, int> Run)[] versions =
{
    ("Search（递归版）",  (nums, t) => new Solution().Search(nums, t)),
    ("Search1（迭代版）", (nums, t) => new Solution().Search1(nums, t)),
};

int pass = 0, total = 0;

// 一组用例要给【所有版本】都判一遍：任何一版错 = 这组红（= 两版互证）
void Check(string title, int[] nums, int target, int expected)
{
    total++;
    if (!PreconditionOk(nums))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】(非严格升序)，这组不算数");
        return;
    }

    List<string> fails = new List<string>();
    foreach ((string name, Func<int[], int, int> run) in versions)
    {
        int actual = run(nums, target);
        if (actual != expected) fails.Add($"{name} → 期望 {expected}，实际 {actual}");
    }

    if (fails.Count == 0)
    {
        pass++;
        Console.WriteLine($"✅ {title}");
    }
    else
    {
        string head = nums.Length <= 12 ? string.Join(",", nums) : $"(n={nums.Length})";
        Console.WriteLine($"❌ {title}");
        Console.WriteLine($"     nums = [{head}]   target = {target}");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么要它：**正确性判题器永远分不清 O(log n) 和 O(n)** —— `Array.IndexOf` 能让上面全部变绿。
// 这个探针把差距放大成肉眼可见的数字。
void Bench()
{
    int n = 10_000;                        // 题目允许的上限
    int[] big = new int[n];
    for (int i = 0; i < n; i++) big[i] = i * 2 - 9999;   // 严格升序、互不相同、值域内

    int reps = 20_000;
    int[] queries = new int[reps];
    for (int r = 0; r < reps; r++)
    {
        int k = r % n;
        // 一半查【存在】（big 全是奇数）、一半查【不存在】（相邻的偶数，插在两个元素中间）
        queries[r] = (r & 1) == 0 ? big[k] : (k == n - 1 ? big[k] - 1 : big[k] + 1);
    }

    foreach ((string name, Func<int[], int, int> run) in versions)
    {
        // 预热：让 JIT 先把方法编译出来，别把编译时间算进成绩
        for (int i = 0; i < 1_000; i++) run(big, big[i % n]);

        long sink = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++) sink += run(big, queries[r]);
        sw.Stop();

        Console.WriteLine($"⏱ {name}：n = {n}、查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms（sink={sink}）");
    }

    Console.WriteLine();
    Console.WriteLine("   判据：二分 = 每次砍一半 ⇒ 约 20,000 × 14 ≈ 28 万次比较，应在【个位到几十 ms】；");
    Console.WriteLine("         线性扫描 ⇒ 约 20,000 × 5,000 ≈ 1 亿次比较，会到【几百 ms 甚至秒级】。");
    Console.WriteLine("   ⚠️ 这个方法【不计对错】—— 它只负责把\"你写的是不是二分\"这件事变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（我刚从网页取的，仍以网页为准）═══════
//   · 1 <= nums.length <= 10^4        ⇒ 空数组不用管（不在约束内）
//   · -10^4 < nums[i], target < 10^4  ⇒ 值域内
//   · 元素【互不相同】                ⇒ 答案唯一 ⇒ 判题器可以直接比对下标（不用像 108 那样比性质）
//   · nums 升序
//   ⭐ 一条反常识（今天第一课）：**这题不会整数溢出**。
//      "`mid = (lo + hi) / 2` 会溢出" 那句八股来自 `lo + hi > int.MaxValue` 的场合；
//      这里 n ≤ 10^4 ⇒ lo + hi ≤ 20,000，离 21 亿差着十万八千里。
//      ⇒ 别把别处的药方背到这里来吃（69 题才会真溢出，那天再谈）。

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   704 是二分线的第一题，它练的不是"找到"，而是【区间】这个概念本身。
//
//   ① 落笔前先写死一件事 —— 你的【不变量】：
//        "搜索区间 = [lo, hi]，target 若存在，下标必在此区间内"
//        整个二分只有这一句话；下面所有代码都是它的翻译，不是模板。
//
//   ② 一个岔路（自己选，选了就走完）：
//        闭区间 [lo, hi]：初值 lo = 0 / hi = nums.Length - 1，循环条件 ??? 
//        半开区间 [lo, hi)：初值 lo = 0 / hi = nums.Length，循环条件 ???
//        两条都对；但【初值、循环条件、收缩方式】三者必须自洽 ——
//        混搭 = 漏答案 或 越界，且症状不一样（下面反例区分别派了用例）。
//
//   ③ 循环体只该有三件事（想清楚再写，不要多写）：
//        算 mid → 命中就 return → 否则砍掉【不含 target 的那一半】，
//        ⚠️ 砍的时候【必须把 mid 自己也砍掉】，否则区间原地不动 = 死循环。
//
//   ④ 这道题最容易自欺的一点（今天最贵）：
//        `Array.IndexOf(nums, target)` 一行就能让上面全部变绿 —— 结果全对，复杂度 O(n)。
//        ⇒ 判题器只能证明"答案对"，证明不了"你在二分"。
//        真判据是 Bench：同样 20,000 次查询，二分与线性差两个数量级。
//
//   ⑤ 复杂度（写完自己算）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
Check("★ 官方示例 1：[-1,0,3,5,9,12] 查 9 → 下标 4", new int[] { -1, 0, 3, 5, 9, 12 }, 9, 4);
Check("★ 官方示例 2：[-1,0,3,5,9,12] 查 2 → -1（不存在）", new int[] { -1, 0, 3, 5, 9, 12 }, 2, -1);

Check("★★ 单元素命中 [5] 查 5 → 0（循环写 lo<hi 的会在这里露馅）", new int[] { 5 }, 5, 0);
Check("★ 单元素不命中 [5] 查 3（target 小于唯一元素）→ -1", new int[] { 5 }, 3, -1);
Check("★ 单元素不命中 [5] 查 9（target 大于唯一元素）→ -1", new int[] { 5 }, 9, -1);

Check("★★ 两元素命中左 [1,3] 查 1 → 0", new int[] { 1, 3 }, 1, 0);
Check("★★ 两元素命中右 [1,3] 查 3 → 1", new int[] { 1, 3 }, 3, 1);
Check("★★★ 两元素夹缝 [1,3] 查 2 → -1（相邻区间的收敛 = 这道题的真门槛）",
      new int[] { 1, 3 }, 2, -1);

Check("★ 偶数长度 6 个，命中首个 [0,1,2,3,4,5] 查 0 → 0", new int[] { 0, 1, 2, 3, 4, 5 }, 0, 0);
Check("★ 偶数长度 6 个，命中末尾 [0,1,2,3,4,5] 查 5 → 5", new int[] { 0, 1, 2, 3, 4, 5 }, 5, 5);
Check("★ 奇数长度 7 个，命中正中 [0,1,2,3,4,5,6] 查 3 → 3", new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3, 3);

Check("★★ target 大于全体 [1,2,3] 查 4 → -1（右边界必须能收）", new int[] { 1, 2, 3 }, 4, -1);
Check("★★ target 小于全体 [1,2,3] 查 0 → -1（左边界必须能收）", new int[] { 1, 2, 3 }, 0, -1);

Check("★ 含负数 [-5,-3,-1,2,4] 查 -3 → 1", new int[] { -5, -3, -1, 2, 4 }, -3, 1);
Check("★ 全负数 [-9,-7,-5,-3,-1] 查 -1 → 4", new int[] { -9, -7, -5, -3, -1 }, -1, 4);
Check("★ 值域下界 [-9999,0,9999] 查 -9999 → 0", new int[] { -9999, 0, 9999 }, -9999, 0);
Check("★ 值域上界 [-9999,0,9999] 查 9999 → 2", new int[] { -9999, 0, 9999 }, 9999, 2);

// 规模用例：题目上限 n = 10^4（步数才看得出手上没有循环遍历）
int[] big = new int[10_000];
for (int i = 0; i < big.Length; i++) big[i] = i * 2 - 9999;   // 严格升序、互不相同
Check("★★★ 规模 n=10000，查最后一位 → 9999", big, big[^1], 9999);
Check("★★★ 规模 n=10000，查首位 -9999 → 0", big, big[0], 0);
Check("★★★ 规模 n=10000，查一个【不存在】的偶数 5000 → -1", big, 5000, -1);

// ═══════ 反例区（每组点名一种错法）═══════
//   ⛔ A「循环写 while (lo < hi)，出去后不再比一次」→ 单元素、命中末尾都会漏（上面 [5] 查 5 / [1,3] 查 3 抓）
//   ⛔ B「收缩写成 lo = mid / hi = mid（忘了 ±1）」→ 相邻区间原地打转 = 【死循环】
//        ★ 下面这组就是专派给它的：跑起来【卡住不停】就是这条病，Ctrl+C 停掉；正确版本秒过。
Check("⛔ 抓「lo=mid / hi=mid 不 ±1」：[1,3] 查 2 → -1（错版本会卡死在这里）",
      new int[] { 1, 3 }, 2, -1);
//   ⛔ C「hi 取了 nums.Length（半开区间）却仍写 while (lo <= hi) 闭区间逻辑」→ 越界异常
Check("⛔ 抓「hi = Length 混闭区间」：[1,3] 查 9 → -1（错版本会抛 IndexOutOfRange）",
      new int[] { 1, 3 }, 9, -1);
//   ⛔ D「不等就 return mid」→ 官方示例 2 会返回一个下标而不是 -1（上面示例 2 抓）
//   ⛔ E「用 Array.IndexOf 一遍过」→ 上面全绿，只有 Bench 能照出来（记得看 ⏱ 那行）

Bench();

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 返回 target 在 nums 中的下标；不存在则返回 -1。
    //
    //   ── 落笔顺序（照不变量走，别背模板）：
    //      ① 先写一句注释，把你选的区间说清楚（"[lo, hi] 闭区间，初值 ?"）
    //      ② 再写初值 + while 条件 —— 这两个必须一起写，它们是同一句话的两半
    //      ③ 最后写循环体三件事：算 mid → 命中 return → 砍半（含 mid）
    //
    //   ── 三条硬判据（拿它对，不通过就是错）：
    //      ① 单元素 [5] 查 5 得到 0（不是靠循环外补一刀，而是区间定义本身就允许它命中）
    //      ② [1,3] 查 2 得到 -1 且【自己停下来】
    //      ③ 每轮循环 lo 或 hi 至少有一个真的动了（否则就是反例 B）
    //
    //   ── 填完自问一句：我这个循环【一定会结束】吗？凭什么结束？
    //   ── 复杂度（自己填，别空着）：时间 O( )　空间 O( )
    public int Search(int[] nums, int target)
    {
        return tofound(nums, target, 0, nums.Length - 1);
    }
    int tofound(int[] nums, int target, int lo, int hi)
    {
        if(lo > hi) return -1;
        int mid = (lo + hi) / 2;
        if(nums[mid] == target) return mid;
        if(nums[mid] < target) return tofound(nums, target, mid + 1, hi);
        return tofound(nums, target, lo, mid - 1);
    }
    public int Search1(int[] nums, int target)
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
        return -1;
    }
}
