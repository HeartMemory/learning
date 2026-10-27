// ═══════ 判题工具（承 704 / 35 / 34 / 69 / 367：先自检【前提】，再判答案）═══════

using System.Diagnostics;

// 前提自检：74 能二分，靠的是【两条性质同时成立】——
//   ① 每行升序（非递减）
//   ② 下一行首元素 > 上一行末元素   ← 这条是"能拼成一维"的全部本钱
// 少了 ②，把矩阵当一维数组就是错的（期望值也会跟着错）⇒ 用例自己先验一遍。
static bool PreconditionOk(int[][] matrix)
{
    if (matrix is null || matrix.Length == 0) return false;
    int n = matrix[0].Length;
    if (n == 0) return false;

    for (int i = 0; i < matrix.Length; i++)
    {
        if (matrix[i] is null || matrix[i].Length != n) return false;   // 行长不齐 ⇒ 坐标换算会错
        for (int j = 1; j < n; j++)
            if (matrix[i][j] < matrix[i][j - 1]) return false;          // ① 行内升序
        if (i > 0 && matrix[i][0] <= matrix[i - 1][n - 1]) return false; // ② 行间首尾相接
    }
    return true;
}

// 造矩阵的简写：Mat(new[]{1,3}, new[]{5,7}) ⇒ 2×2
static int[][] Mat(params int[][] rows) => rows;

// 参考实现（只用来当"标准答案"，不参与判分）：暴力扫，O(m·n)
static bool LinearScan(int[][] matrix, int target)
{
    foreach (int[] row in matrix)
        foreach (int v in row)
            if (v == target) return true;
    return false;
}

// ⚠️ 参与判题的版本登记表：**新写一个实现就往这里加一行**（不加 = 它一次都不会被跑到）
(string Name, Func<int[][], int, bool> Run)[] versions =
{
    ("SearchMatrix", (m, t) => new Solution().SearchMatrix(m, t)),
    // 第二版写完，在这里补一行（互证才有意义）：
    // ("SearchMatrix2（???版）", (m, t) => new Solution().SearchMatrix2(m, t)),
};

int pass = 0, total = 0;

// 一组用例要给【所有版本】都判一遍：任何一版错 = 这组红（= 互证）
void Check(string title, int[][] matrix, int target, bool expected)
{
    total++;
    if (!PreconditionOk(matrix))
    {
        Console.WriteLine($"⚠️ {title} —— 【用例自身前提不成立】（行内升序 / 行间首尾相接 / 行长齐），这组不算数");
        return;
    }

    List<string> fails = new List<string>();
    foreach ((string name, Func<int[][], int, bool> run) in versions)
    {
        try
        {
            bool actual = run(matrix, target);
            if (actual != expected) fails.Add($"{name} → 期望 {expected}，实际 {actual}");
        }
        catch (Exception e)
        {
            // 越界 / 空引用 / 除零会被抓在这里，而不是把整个程序炸掉
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
        Console.WriteLine($"❌ {title}   (target = {target})");
        foreach (string f in fails) Console.WriteLine($"     {f}");
    }
}

// ═══════ 全量对账（随机造合法矩阵 → 区间里【逐个整数】比）═══════
// 为什么要有它：手写用例只覆盖"我想得到的边界"。造 m×n 的随机矩阵，
// 把 [最小−1, 最大+1] 里每个整数都判一遍 ⇒ 差一 / 越界 / 行列写反，全部会露出来。
static int[][] BuildRandomMatrix(int m, int n, Random rng)
{
    int[] flat = new int[m * n];
    int cur = rng.Next(-50, 0);
    for (int i = 0; i < flat.Length; i++)
    {
        cur += rng.Next(1, 4);              // 严格递增 ⇒ ①② 天然同时成立
        flat[i] = cur;
    }

    int[][] mat = new int[m][];
    for (int i = 0; i < m; i++)
    {
        mat[i] = new int[n];
        Array.Copy(flat, i * n, mat[i], 0, n);
    }
    return mat;
}

void CrossCheck()
{
    Random rng = new Random(20261027);
    (int m, int n)[] shapes = { (1, 1), (1, 7), (7, 1), (2, 3), (3, 2), (5, 5), (13, 17) };

    int mismatches = 0, probes = 0;
    foreach ((int m, int n) in shapes)
    {
        for (int trial = 0; trial < 20; trial++)
        {
            int[][] mat = BuildRandomMatrix(m, n, rng);
            int lo = mat[0][0], hi = mat[m - 1][n - 1];

            for (int t = lo - 1; t <= hi + 1; t++)
            {
                bool expected = LinearScan(mat, t);
                foreach ((string name, Func<int[][], int, bool> run) in versions)
                {
                    probes++;
                    bool actual;
                    try { actual = run(mat, t); }
                    catch (Exception e)
                    {
                        mismatches++;
                        if (mismatches <= 8)
                            Console.WriteLine($"❌ 对账崩了：{name} / {m}×{n} / target={t} → {e.GetType().Name}");
                        continue;
                    }
                    if (actual != expected)
                    {
                        mismatches++;
                        if (mismatches <= 8)
                            Console.WriteLine($"❌ 对账不符：{name} / {m}×{n} / target={t} → 期望 {expected}，实际 {actual}");
                    }
                }
            }
        }
    }

    Console.WriteLine(mismatches == 0
        ? $"✅ 全量对账：{probes} 次查询 / 0 处不符（{shapes.Length} 种形状 × 每种 20 张随机矩阵）"
        : $"❌ 全量对账：{probes} 次查询 / {mismatches} 处不符");
}

// ═══════ 性能探针（不计对错 · 只打印）═══════
// 为什么要有它：正确性判题器分不清 O(log(m·n)) 与 O(m·n) —— 两版都能全绿。
void Bench()
{
    int[][] mat = BuildRandomMatrix(100, 100, new Random(74));
    int lo = mat[0][0], hi = mat[99][99];

    int reps = 20_000;
    int[] queries = new int[reps];
    Random rng = new Random(7);
    for (int r = 0; r < reps; r++) queries[r] = rng.Next(lo - 1, hi + 2);

    foreach ((string name, Func<int[][], int, bool> run) in versions)
    {
        try
        {
            for (int i = 0; i < 500; i++) run(mat, queries[i]);   // 预热：别把 JIT 时间算进成绩

            int sink = 0;
            Stopwatch sw = Stopwatch.StartNew();
            for (int r = 0; r < reps; r++) sink += run(mat, queries[r]) ? 1 : 0;
            sw.Stop();

            Console.WriteLine($"⏱ {name}：100×100 矩阵查 {reps} 次、总耗时 {sw.Elapsed.TotalMilliseconds:F1} ms（sink={sink}）");
        }
        catch (Exception e)
        {
            Console.WriteLine($"⏱ {name}：跳过（{e.GetType().Name}）");
        }
    }

    Console.WriteLine();
    Console.WriteLine("   判据：二分 ⇒ 每查 ≤ 14 步 ⇒ 应在【个位到几十 ms】；");
    Console.WriteLine("         逐行二分 ⇒ 每查 ~700 步 ⇒ 慢一个数量级；");
    Console.WriteLine("         整表扫描 ⇒ 每查 1 万步 ⇒ 秒级。");
    Console.WriteLine("   ⚠️ 这个方法【不计对错】—— 它只把\"你写的是哪一档\"变成数字。");
}

// ═══════ 开工前顺手核一眼题面「提示」（以网页为准）═══════
//   · 1 <= m, n <= 100                  ⇒ m·n ≤ 10000，【不溢出】（与 69 / 367 不同，这题没有溢出戏）
//   · -10^4 <= matrix[i][j], target <= 10^4
//   · 性质①：每行非递减   性质②：下一行首元素 > 上一行末元素（【严格】跨接，不是"≥"）
//   · 要求 O(log(m·n))
//   ⭐ 一句话抓住这题：**两条性质合起来 = 矩阵按行读出来是一条升序数组**。

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   ① 先回答"凭什么能二分"：
//        单调性 = f(k) = 按行展开后的第 k 个数，在 k 上严格递增。
//        ⚠️ 只有①不行（行内有序但行间乱序 ⇒ 展开后不是一条升序），必须①②同时成立。
//        这一句写进注释 —— 判据是"能说清为什么"，不是"模板背得熟"。
//
//   ② 坐标换算（今天唯一的机械功）：
//        一维下标 k  ↔  二维 (r, c)，列数 = n：
//            r = k / n      c = k % n          （k → 坐标）
//            k = r * n + c                     （坐标 → k，用来核对你自己）
//        ⚠️ 分母是【列数 n】不是行数 m。方阵会让写反的人全绿，非方阵立刻现形
//           ⇒ 用例里专门放了 2×3 与 3×2。
//
//   ③ 两条路线（任选一条写完，都满足 O(log(m·n))）：
//        路线 A · 一次二分：区间 [0, m·n − 1]，每次比较 matrix[mid / n][mid % n]
//        路线 B · 两次二分：先"定行"再"行内二分"
//            · 定行 = 找【最后一个 行首 ≤ target 的行】← 这是 34 / 69 那种边界问题，不是等值查找
//            · ⚠️ 定行可能"一行都定不出来"（target 比所有行首都小）⇒ 这个出口必须显式处理
//            · 行内二分 = 704 原样搬过来
//        ⭐ 判据：路线 B 的定行段，用"第一个 ≥"还是"最后一个 ≤"，差一行 —— 用「target 恰为某行首」的用例抓。
//
//   ④ 区间三件套必须自洽（与 704 同一套）：初值 · 循环条件 · 收缩是否排除 mid。
//        自问：区间被砍到只剩一个元素时，它被比较过吗？
//
//   ⑤ 复杂度（写完自己填，别空着）：时间 O( )　空间 O( )

// ═══════ 用例区（形状优先：先边界，再规模）═══════
int[][] official = Mat(
    new[] { 1, 3, 5, 7 },
    new[] { 10, 11, 16, 20 },
    new[] { 23, 30, 34, 60 });

Check("★ 官方示例 1：target = 3 → true", official, 3, true);
Check("★ 官方示例 2：target = 13 → false（5 < 13 < 16，行内空隙）", official, 13, false);

// —— 形状边界：1×1 / 1×n / n×1 ——
Check("★★ 1×1 命中：[[1]] t=1 → true", Mat(new[] { 1 }), 1, true);
Check("★★ 1×1 落空：[[1]] t=0 → false", Mat(new[] { 1 }), 0, false);
Check("★ 1×n 行尾：[[1,3,5,7]] t=7 → true", Mat(new[] { 1, 3, 5, 7 }), 7, true);
Check("★ 1×n 小于全部：[[1,3,5,7]] t=0 → false", Mat(new[] { 1, 3, 5, 7 }), 0, false);
Check("★ n×1 末行：[[1],[3],[5]] t=5 → true", Mat(new[] { 1 }, new[] { 3 }, new[] { 5 }), 5, true);
Check("★ n×1 超出：[[1],[3],[5]] t=6 → false", Mat(new[] { 1 }, new[] { 3 }, new[] { 5 }), 6, false);

// —— 跨行接缝（② 这条性质的主战场）——
Check("★★ 上一行末：[[1,3,5],[10,11,16]] t=5 → true", Mat(new[] { 1, 3, 5 }, new[] { 10, 11, 16 }), 5, true);
Check("★★ 下一行首：[[1,3,5],[10,11,16]] t=10 → true（定行差一行会在这里错）",
    Mat(new[] { 1, 3, 5 }, new[] { 10, 11, 16 }), 10, true);
Check("★★ 行间空隙：[[1,3,5],[10,11,16]] t=7 → false", Mat(new[] { 1, 3, 5 }, new[] { 10, 11, 16 }), 7, false);
Check("★ 2×2 四终点：[[1,2],[3,4]] t=2 → true", Mat(new[] { 1, 2 }, new[] { 3, 4 }), 2, true);
Check("★ 2×2 四终点：[[1,2],[3,4]] t=3 → true", Mat(new[] { 1, 2 }, new[] { 3, 4 }), 3, true);
Check("★ 比最小还小：[[1,2],[3,4]] t=0 → false", Mat(new[] { 1, 2 }, new[] { 3, 4 }), 0, false);

// —— 非方阵（行列写反必红）——
Check("★★★ 2×3：[[1,3,5],[7,9,11]] t=9 → true（r = k/3 才对，k/2 会错）",
    Mat(new[] { 1, 3, 5 }, new[] { 7, 9, 11 }), 9, true);
Check("★★★ 3×2：[[1,3],[5,7],[9,11]] t=7 → true（同一份代码两种形状都要对）",
    Mat(new[] { 1, 3 }, new[] { 5, 7 }, new[] { 9, 11 }), 7, true);

// —— 负数（约束下界 −10^4）——
Check("★ 含负数：[[-10,-5,-1],[0,3,9]] t=-5 → true", Mat(new[] { -10, -5, -1 }, new[] { 0, 3, 9 }), -5, true);
Check("★ 含负数落空：[[-10,-5,-1],[0,3,9]] t=-4 → false", Mat(new[] { -10, -5, -1 }, new[] { 0, 3, 9 }), -4, false);
Check("★ 全负：[[-5,-3],[-2,-1]] t=-5 → true", Mat(new[] { -5, -3 }, new[] { -2, -1 }), -5, true);

// —— 规模上限 100×100（约束边界）——
int[][] big = BuildRandomMatrix(100, 100, new Random(74));
Check($"★★★ 100×100 首元素 → true", big, big[0][0], true);
Check($"★★★ 100×100 末元素 → true", big, big[99][99], true);
Check($"★★★ 100×100 比最小还小 → false", big, big[0][0] - 1, false);
Check($"★★★ 100×100 比最大还大 → false", big, big[99][99] + 1, false);

// ═══════ 反例区（每组点名一种错法）═══════
//   ⛔ A「行列用反：r = mid % m, c = mid / m」→ 2×3 与 3×2 必红，方阵反而全绿
//        ★ 症状指纹：只有非方阵错 ⇒ 不用怀疑二分逻辑，去查分母
//   ⛔ B「定行用『第一个行首 ≥ target』」→ target = 10（恰为第 2 行首）会定到第 1 行 ⇒ 漏答案
//   ⛔ C「定行返回 0 兜底」→ target 比所有行首都小时会误查第 1 行（本例恰好 false，别指望它红，用对账抓）
//   ⛔ D「展平 hi 写 m*n」→ 下标越界（IndexOutOfRange）
//   ⛔ E「每行都做一次二分」→ 结果对，但 Bench 慢一个数量级 ⇒ 只有 Bench 能照出来
//   ⛔ F「只比值不缩区间（while 里忘了改 lo/hi）」→ 死循环（跑的时候卡住就是它）
//   ⛔ G「`if` 写成 `while`，或循环条件写 `lo < hi` 却不补最后一步」→ 单元素区间漏比较
//        ★ 判据：区间只剩一个元素时，它被比较过吗？

CrossCheck();

Bench();

// ═══════ 结果汇总 ═══════
Console.WriteLine();
Console.WriteLine($"—— 正确性 {pass} / {total} ——");

// ═══════ 作答区（方法体留给你自己写）═══════
public class Solution
{
    // TODO: 在满足「每行升序 + 行间首尾相接」的 matrix 里找 target。
    //
    //   ── 落笔顺序（照不变量走，别背模板）：
    //      ① 先写一句注释：单调性是什么、为什么本题能二分
    //      ② 选路线（A 一次二分 / B 两次二分），写清区间与"列数 n"从哪来
    //      ③ 写循环：算 mid → 换算坐标 → 比较 → 砍半
    //
    //   ── 三条硬判据：
    //      ① 2×3 与 3×2 两种形状都对（坐标换算站得住）
    //      ② target 恰为某行首元素 → true（路线 B 的定行不差一行）
    //      ③ Bench 落在【几十 ms】档（不是秒级）
    //
    //   ── 填完自问：这个循环【一定停】吗？靠什么停？
    //   ── 复杂度（自己填，别空着）：时间 O(log m + log n)　空间 O(1)
    public bool SearchMatrix(int[][] matrix, int target)
    {
        if(matrix.Length == 0 || matrix[0].Length == 0) return false;
        int rows = matrix.Length - 1;
        int cols = matrix[0].Length - 1;
        int row = -1;
        int col = -1;
        int lo = 0;
        int hi = rows;
        while(lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if(target < matrix[mid][0]) hi = mid - 1;
            else if(target > matrix[mid][cols]) lo = mid + 1;
            else{row = mid; break;}
        }
        if(row == -1) return false;
        lo = 0;
        hi = cols;
        while(lo <= hi)
        {
            int mid = (lo + hi) / 2;
            int judge = matrix[row][mid];
            if(target < judge) hi = mid - 1;
            else if(target > judge) lo = mid + 1;
            else{col = mid; break;}
        }
        if(col == -1) return false;
        return true;
    }
}
