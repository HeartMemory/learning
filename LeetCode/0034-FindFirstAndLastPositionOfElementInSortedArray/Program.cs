// ═══════ 判题工具（承 704 / 35：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：34 的前提 = nums 【非降序】（⚠️ 与 35 的"严格升序、无重复"不同 ——
// 本题的整个难点就来自"允许重复"，所以这里判的是 nums[i-1] > nums[i] 才算坏）。
static bool PreconditionOk(int[] nums)
{
    for (int i = 1; i < nums.Length; i++)
        if (nums[i - 1] > nums[i]) return false;
    return true;
}

// 期望值的【独立来源】：一趟线性扫，同时记下 target 第一次与最后一次出现的位置。
// ⚠️ 它 O(n)，只用于算期望值，【不进 versions】—— 否则 Bench 就变成自己跟自己比。
static int[] NaiveRange(int[] nums, int target)
{
    int first = -1, last = -1;
    for (int i = 0; i < nums.Length; i++)
    {
        if (nums[i] == target)
        {
            if (first < 0) first = i;
            last = i;
        }
    }
    return new int[] { first, last };
}

static string Show(int[] a) => a is null ? "null" : "[" + string.Join(",", a) + "]";

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
//    这条是 10-21 的真实教训：写完 Search1 却全绿，因为判题器只认 Search。
(string Name, Func<int[], int, int[]> Run)[] versions =
{
    ("SearchRange（你的版本）", (nums, t) => new Solution().SearchRange(nums, t)),
};

int pass = 0, total = 0;

void Check(string title, int[] nums, int target, int[]? expected = null)
{
    total++;
    if (!PreconditionOk(nums))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】(非非降序)，这组不算数");
        return;
    }

    int[] want = expected ?? NaiveRange(nums, target);
    string src = expected is null ? "（期望值由线性扫现算）" : "";

    List<string> fails = new List<string>();
    foreach ((string name, Func<int[], int, int[]> run) in versions)
    {
        try
        {
            int[] actual = run(nums, target);
            if (!actual.SequenceEqual(want))
                fails.Add($"{name} → 期望 {Show(want)}，实际 {Show(actual)}");
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
        string head = nums.Length <= 12 ? string.Join(",", nums) : $"(n={nums.Length})";
        Console.WriteLine($"❌ {title}{src}");
        Console.WriteLine($"     nums = [{head}]   target = {target}");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么必须有它：**正确性判题器分不清 O(log n) 和 O(n)**。
// 尤其是"先二分命中，再向左右线性扩"这种写法 —— 普通用例全绿，只有这个探针能照出来。
void Bench()
{
    // 防线：方法体还没写时，探针不该把整个程序带崩（只提示跳过）
    try { _ = new Solution().SearchRange(new int[] { 1 }, 1); }
    catch (NotImplementedException)
    {
        Console.WriteLine("⏭ 探针跳过：SearchRange 还没实现（先写方法体，再回来看这两组数字）");
        return;
    }

    Console.WriteLine("── 探针 A：随机升序、区间很短（二分真正的用武之地）──");
    {
        int n = 100_000;                       // 题目允许的上限
        int[] big = new int[n];
        for (int i = 0; i < n; i++) big[i] = i * 2;        // 全偶数、互不相同

        int reps = 5_000;
        int[] queries = new int[reps];
        for (int r = 0; r < reps; r++) queries[r] = (r % 10) * 20_000;   // 命中的多，分布散

        Solution sol = new Solution();
        for (int i = 0; i < 1_000; i++) sol.SearchRange(big, queries[i % reps]);   // 预热

        long sinkMine = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++)
        {
            int[] res = sol.SearchRange(big, queries[r]);
            sinkMine += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 你的版本 ：n = {n}、查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkMine}）");

        long sinkNaive = 0;
        sw.Restart();
        for (int r = 0; r < reps; r++)
        {
            int[] res = NaiveRange(big, queries[r]);
            sinkNaive += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 线性基线 ：n = {n}、查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkNaive}）");

        Console.WriteLine($"   ⭐ 两条 sink 必须相同（{sinkMine} vs {sinkNaive}）：同一批查询给出【同一对下标】。");
    }

    Console.WriteLine();
    Console.WriteLine("── 探针 B：全相同数组（「命中后向两边扩」的最坏情况）──");
    {
        int n = 100_000;
        int[] flat = new int[n];
        Array.Fill(flat, 7);                   // 全 7

        int reps = 2_000;
        Solution sol = new Solution();
        for (int i = 0; i < 100; i++) sol.SearchRange(flat, 7);   // 预热

        long sinkMine = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++)
        {
            int[] res = sol.SearchRange(flat, 7);
            sinkMine += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 你的版本 ：查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkMine}）");

        long sinkNaive = 0;
        sw.Restart();
        for (int r = 0; r < reps; r++)
        {
            int[] res = NaiveRange(flat, 7);
            sinkNaive += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 线性基线 ：查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkNaive}）");

        Console.WriteLine($"   ⭐ sink 仍需相同（{sinkMine} vs {sinkNaive}）；本题正确答案恒为 [0,{n - 1}]。");
    }

    Console.WriteLine();
    Console.WriteLine("── 探针 C：目标段在【左半】、更大的值在【右半】（「命中后往回扫」的真最坏情况）──");
    Console.WriteLine("   ⚠️ 这一组是后补的：探针 B 的「全相同」其实【照不出】往回扫的代价 ——");
    Console.WriteLine("      lo 和 hi 一伸手就撞上 target，于是每步都立刻 break。真正的对手是这一组。");
    {
        int n = 100_000;
        int[] skew = new int[n];
        for (int i = 0; i < n; i++) skew[i] = i < n / 2 ? 1 : 2;   // 左半全 1，右半全 2

        int reps = 2_000;
        Solution sol = new Solution();
        for (int i = 0; i < 100; i++) sol.SearchRange(skew, 1);    // 预热

        long sinkMine = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int r = 0; r < reps; r++)
        {
            int[] res = sol.SearchRange(skew, 1);
            sinkMine += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 你的版本 ：查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkMine}）");

        long sinkNaive = 0;
        sw.Restart();
        for (int r = 0; r < reps; r++)
        {
            int[] res = NaiveRange(skew, 1);
            sinkNaive += res[0] * 1_000_000L + res[1];
        }
        sw.Stop();
        Console.WriteLine($"⏱ 线性基线 ：查询 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds,7:F1} ms（sink={sinkNaive}）");

        Console.WriteLine($"   ⭐ sink 仍需相同（{sinkMine} vs {sinkNaive}）；本题正确答案恒为 [0,{n / 2 - 1}]。");
        Console.WriteLine("   判据：真二分的耗时【与数据形状无关】—— 探针 A / B / C 三组应全是亚毫秒级；");
        Console.WriteLine("         若某一组突然涨到几十/几百 ms ⇒ 那组形状正好是你实现里的线性段。");
    }

    Console.WriteLine();
    Console.WriteLine("   判据：真二分（两遍）= 每次砍一半 ⇒ 探针 A 应在【个位 ms】、探针 B 与 n 无关；");
    Console.WriteLine("         若探针 B 也慢成百 ms 级 ⇒ 你写的是「命中后向两边线性扩」，最坏 O(n)，题面不允许。");
    Console.WriteLine("   ⚠️ 本方法【不计对错】—— 它只负责把「你写的是不是 O(log n)」变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 0 <= nums.length <= 10^5   ⇒ ⚠️ 【空数组是合法输入】，返回 [-1,-1]
//   · -10^9 <= nums[i], target <= 10^9
//   · nums 按【非降序】排列 ⇒ 允许重复，本题难点全在这里
//   · 必须使用 O(log n) 的算法 ⇒ 题面直接把"线性扩"判死
//   · 返回值是长度 2 的数组 [first, last]；**找不到时必须是 [-1, -1]**（不是 [0,0]，不是 [0,-1]）

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   35 教了你一件事：二分的返回值不是"某个元素"，而是【一条边界】。
//   34 就是那句话的直接后果 —— 一条边界不够，这里要【两条边界】。
//
//   ① 先给两个边界各起一个名字（这一步想清楚，代码就只有一行区别）：
//        left  = 第一个  >= target 的下标   （就是 35 的下界 lower_bound）
//        right = 最后一个 <= target 的下标
//      而 right 还有一个更省事的等价说法：**"第一个 > target 的下标，再减一"**（上界 upper_bound − 1）。
//      ⇒ ⭐ 到这一步你会发现：**两遍调用的其实是同一个函数，只差一个比较符**。
//
//   ② 于是有两条路（选一条走完，别混）：
//        路 A（推荐先走）：写一个 LowerBound(nums, value) 复用两遍 ——
//                          left = LowerBound(target)；right = LowerBound(target + 1) - 1
//        路 B（更"本能"）：写两个循环，一个收缩找左边界，一个收缩找右边界；
//                          ⚠️ 路 B 的右半边 mid 要取 (lo + hi + 1) / 2，否则 lo = mid 时会【死循环】
//
//   ③ 找到之后【不要】向两边线性扩 —— 那就是 O(n)，探针 B 会当场把差距照出来。
//
//   ④ 收口判据（写完必须自查）：**找不到时两个下标怎么变成 -1？**
//      别专门写 if —— 想想 left > right 时 [left, right] 长什么样（例如 left = 2 / right = 1）。
//
//   ⑤ 复杂度（写完自己算）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
Check("★ 官方示例 1：[5,7,7,8,8,10] 查 8 → [3,4]", new int[] { 5, 7, 7, 8, 8, 10 }, 8, new int[] { 3, 4 });
Check("★★ 官方示例 2：[5,7,7,8,8,10] 查 6 → [-1,-1]（找不到）", new int[] { 5, 7, 7, 8, 8, 10 }, 6, new int[] { -1, -1 });
Check("★★ 官方示例 3：[] 查 0 → [-1,-1]（空数组是合法输入）", Array.Empty<int>(), 0, new int[] { -1, -1 });

Check("★★ 单元素命中 [1] 查 1 → [0,0]（最容易写成 [0,-1] 的地方）", new int[] { 1 }, 1, new int[] { 0, 0 });
Check("★★ 单元素不命中 [1] 查 0 → [-1,-1]", new int[] { 1 }, 0, new int[] { -1, -1 });
Check("★★ 单元素不命中 [1] 查 2 → [-1,-1]", new int[] { 1 }, 2, new int[] { -1, -1 });

Check("★★★ 全相同 [2,2,2] 查 2 → [0,2]（两个边界都在最远端）", new int[] { 2, 2, 2 }, 2, new int[] { 0, 2 });
Check("★★ 全相同 [2,2,2] 查 3 → [-1,-1]", new int[] { 2, 2, 2 }, 3, new int[] { -1, -1 });
Check("★★ 两元素命中 [1,1] 查 1 → [0,1]", new int[] { 1, 1 }, 1, new int[] { 0, 1 });

Check("★★ 唯一段在开头 [8,8,9,9,9] 查 8 → [0,1]", new int[] { 8, 8, 9, 9, 9 }, 8, new int[] { 0, 1 });
Check("★★ 唯一段在末尾 [1,2,3,8,8] 查 8 → [3,4]", new int[] { 1, 2, 3, 8, 8 }, 8, new int[] { 3, 4 });
Check("★★ 唯一段在正中 [1,2,5,5,5,9] 查 5 → [2,4]", new int[] { 1, 2, 5, 5, 5, 9 }, 5, new int[] { 2, 4 });
Check("★★★ 比全体都小/都大 [5,7,7,8] 查 1 → [-1,-1]", new int[] { 5, 7, 7, 8 }, 1, new int[] { -1, -1 });
Check("★★★ 比全体都大 [5,7,7,8] 查 99 → [-1,-1]", new int[] { 5, 7, 7, 8 }, 99, new int[] { -1, -1 });

Check("★ 含负数与 0 [-9,-9,0,0,3] 查 0 → [2,3]", new int[] { -9, -9, 0, 0, 3 }, 0, new int[] { 2, 3 });
Check("★ 含负数 [-9,-9,0,3] 查 -9 → [0,1]", new int[] { -9, -9, 0, 3 }, -9, new int[] { 0, 1 });
Check("★ 值域下界 [-1000000000,0,1000000000] 查 -1000000000 → [0,0]", new int[] { -1000000000, 0, 1000000000 }, -1000000000, new int[] { 0, 0 });
Check("★★ 值域上界 [0,1000000000] 查 1000000000 → [1,1]", new int[] { 0, 1000000000 }, 1000000000, new int[] { 1, 1 });

Check("★★ 偶数长度尾段 [0,1,2,3,3,3] 查 3 → [3,5]", new int[] { 0, 1, 2, 3, 3, 3 }, 3, new int[] { 3, 5 });
Check("★ 偶数长度无重复 [0,1,2,3] 查 2 → [2,2]", new int[] { 0, 1, 2, 3 }, 2, new int[] { 2, 2 });

// ⚠️ 长连续段专项（10-23 后补）：这是「先把区间二分到命中、再在子区间里找」这类写法的唯一盲区 ——
//    命中只说明"区间里存在 target"，不说明"这就是边界"；target 段一长，随便一次命中都会落在段中间。
Check("★★★ 长连续段贴前 [1,8,8,8,8,8,8,8,8,9] 查 8 → [1,8]", new int[] { 1, 8, 8, 8, 8, 8, 8, 8, 8, 9 }, 8, new int[] { 1, 8 });
Check("★★★ 长连续段在正中 [0,0,5,5,5,5,5,9] 查 5 → [2,6]", new int[] { 0, 0, 5, 5, 5, 5, 5, 9 }, 5, new int[] { 2, 6 });
Check("★★★ 全部就是一段 [8,8,8,8] 查 8 → [0,3]", new int[] { 8, 8, 8, 8 }, 8, new int[] { 0, 3 });

// 规模用例：题目上限 n = 10^5（期望值交给线性扫现算，见 Check 的第二参）
int[] big = new int[100_000];
for (int i = 0; i < big.Length; i++) big[i] = i / 2;          // 每个值出现两次：0,0,1,1,2,2,…
Check("★★★ 规模 n=100000（每值两遍），查 0 → [0,1]", big, 0, new int[] { 0, 1 });
Check("★★★ 规模 n=100000，查末值 → 期望值现算", big, 49_999);
Check("★★★ 规模 n=100000，查不存在（负数）→ [-1,-1]", big, -1, new int[] { -1, -1 });

int[] flat = new int[100_000];
Array.Fill(flat, 7);
Check("★★★ 规模 n=100000 全相同，查 7 → [0,99999]", flat, 7, new int[] { 0, 99_999 });

// ═══════ 反例区（每组点名一种错法）═══════
//   ⛔ A「命中就返回 [i,i]」（把 34 当 704 做）→ 示例 1 抓：查 8 应得 [3,4]，会得到 [3,3] 或 [4,4]
//   ⛔ B「找不到返回 [0,0] / [0,-1]」→ 示例 2 / 3 抓（-1 是【两边都】要写的）
//   ⛔ C「只算左边界，右边界抄成 left + 计数」→ 全相同用例 [2,2,2] 抓（计数得 3，右边界却是 2）
//   ⛔ D「二分命中后向两边线性扩」→ 上面【全绿】，只有探针 B（全相同 10^5）能照出来
//   ⛔ E「右半边 mid 用 (lo+hi)/2 且 lo = mid」→ 死循环，Ctrl+C 停掉（路 B 的专属坑）
//   ⛔ F「空数组没判 → 下标越界」→ 示例 3 直接抛异常（判题器会打出异常名）

Bench();

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 返回 target 在 nums 中的 [第一次出现, 最后一次出现]；不存在返回 [-1,-1]。
    //
    //   ── 落笔顺序（照"边界"的思路走，别背模板）：
    //      ① 先写一句注释，说清两个名字：left = 第一个 >= target 的下标；
    //         right = 第一个 > target 的下标再【减一】
    //      ② 把"找第一个 >= value 的下标"抽成一个私有方法（本题会调它两遍）
    //      ③ 在外面做三件事：调两遍 → 右边界减一 → 处理"找不到"
    //      ④ "找不到"那一句想清楚：left 停在哪、right 算出来是多少、两者什么关系才该返回 [-1,-1]
    //
    //   ── 三条硬判据（拿它对，不通过就是错）：
    //      ① [5,7,7,8,8,10] 查 8 → [3,4]（不是 [3,3]）
    //      ② [5,7,7,8,8,10] 查 6 → [-1,-1]（两个 -1）
    //      ③ [] 查 0 → [-1,-1]（不越界）
    //      并且【没有】任何"命中后向左右线性扩"的循环
    //
    //   ── 填完自问三句：
    //      · 我这个方法调了两遍，第二遍传的值是 target 还是 target + 1？为什么？
    //      · 为什么 right 要减一？不减一会怎样（拿 [5,7,7,8] 查 8 手推一遍）
    //      · 全相同数组 [2,2,2] 查 2，我的两遍二分各跑几轮？和线性扩比差多少？
    //
    //   ── 复杂度（自己填，别空着）：时间 O(logn)　空间 O(1)
    public int[] SearchRange(int[] nums, int target)
    {
        int a = -1;
        int b = -1;
        int lo = 0;
        int hi = nums.Length - 1;
        while(lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if(nums[mid] == target)
            {
                a = lowerBound(nums,target,lo,mid);
                b = lowerBound(nums,target + 1,mid,hi) - 1;
                break;
            }
            if(nums[mid] < target) lo = mid + 1;
            if(nums[mid] > target) hi = mid - 1;
        }
        return new int[]{a,b};
    }
    // 下界：返回【第一个 >= value 的下标】；全小于 value 时返回 lo 越界值（= 右端 + 1 那一格）。
    // ⚠️ 命中时不 return —— 命中只说明"这里有 value"，不说明"这里就是边界"。
    int lowerBound(int[] nums, int value, int lo, int hi)
    {
        while(lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if(nums[mid] < value) lo = mid + 1;
            else hi = mid - 1;
        }
        return lo;
    }
}
