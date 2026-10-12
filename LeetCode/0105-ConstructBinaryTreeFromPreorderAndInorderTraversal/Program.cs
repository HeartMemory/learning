using System.Diagnostics;
using System.Security.AccessControl;

// ══════════════════════════════════════════════════════════════════════════════
//  105 · 从前序与中序遍历序列构造二叉树
//  脚手架（第十三次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144/94/145/102/104/226/101/110/199/98/700/108 → 本题）
//
//  ⭐ 与上一站 108 的两条对照（今天的分水岭都在这里，落笔前先读一遍）：
//
//   ① 【答案唯不唯一】
//      108 一个输入有多种合格答案 ⇒ 只能"比性质"；
//      本题一个输入**只有一棵树** ⇒ **可以比对期望值**。
//      ⚠️ 但"唯一"是**有前提的**（元素互不相同）—— 前提一破，唯一性立刻崩：
//         若中序里出现了两个相同的值，"根在中序里的哪个位置"就不再唯一 ⇒ 答案不唯一。
//      ⇒ 所以本文件的判题器⓪ 第一件事，就是把这个前提**先验一遍**。
//        （判据：**先证明尺子准，再拿尺子量** —— 承 98 / 199 的裁判自检。）
//
//   ② 【给你的信息是"形状"还是"顺序"】
//      108 给你一段**有序**数组 —— 信息是"谁大谁小"，中点直接就是分界线（一步落位）；
//      本题给你两种**遍历顺序** —— 信息是"谁先出现 + 谁在谁的左边"，
//      必须**先用前序把根钉出来**，再拿根去中序里"切两半"，才得到两个子问题。
//      ⇒ 一条一句话的题眼：**前序负责"谁是根"，中序负责"根左右各有谁"**。
//
//  ⭐ 本文件另埋了一块「性能区」：把"在中序里定位根"这一步的代价**变成看得见的数字**。
//     它故意拿两棵**节点数完全一样、形状互为镜像**的斜链来对照 —— 先别往下翻，写完再看。
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十三次复用） ─────────────────────────

// 层序序列化（尾部 null 一律裁掉）—— 与力扣展示格式一致
string Show(TreeNode? root)
{
    if (root is null) return "[空树]";
    List<string> parts = new List<string>();
    Queue<TreeNode?> q = new Queue<TreeNode?>();
    q.Enqueue(root);
    while (q.Count > 0)
    {
        TreeNode? cur = q.Dequeue();
        if (cur is null) { parts.Add("null"); continue; }
        parts.Add(cur.val.ToString());
        q.Enqueue(cur.left);
        q.Enqueue(cur.right);
    }
    while (parts.Count > 0 && parts[^1] == "null") parts.RemoveAt(parts.Count - 1);
    return "[" + string.Join(", ", parts) + "]";
}

// 反序列化：把 Show 出来的字符串重新读回一棵树（判题器⓪ 自查用例表要用）
TreeNode? Parse(string s)
{
    string t = s.Replace(" ", "");
    if (t == "[空树]" || t == "[]") return null;
    string[] tok = t.Substring(1, t.Length - 2).Split(',', StringSplitOptions.RemoveEmptyEntries);
    if (tok.Length == 0 || tok[0] == "null") return null;

    TreeNode root = new TreeNode(int.Parse(tok[0]));
    Queue<TreeNode> q = new Queue<TreeNode>();
    q.Enqueue(root);
    int i = 1;
    while (q.Count > 0 && i < tok.Length)
    {
        TreeNode cur = q.Dequeue();
        if (i < tok.Length && tok[i] != "null") { cur.left = new TreeNode(int.Parse(tok[i])); q.Enqueue(cur.left); }
        i++;
        if (i < tok.Length && tok[i] != "null") { cur.right = new TreeNode(int.Parse(tok[i])); q.Enqueue(cur.right); }
        i++;
    }
    return root;
}

int CountNodes(TreeNode? root) => root is null ? 0 : 1 + CountNodes(root.left) + CountNodes(root.right);
int Height(TreeNode? node) => node is null ? 0 : 1 + Math.Max(Height(node.left), Height(node.right));

// 两种遍历的回读（判题器② 的"独立裁判"要用 —— 它是题目定义逐字直译）
List<int> Preorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n) { if (n is null) return; sink.Add(n.val); Walk(n.left); Walk(n.right); }
    Walk(root);
    return sink;
}
List<int> Inorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n) { if (n is null) return; Walk(n.left); sink.Add(n.val); Walk(n.right); }
    Walk(root);
    return sink;
}

// ───────────────── 判题器⓪：尺子自检 + 输入前提自检（不碰 Solution） ─────────────────

// 前提一：【元素互不相同】—— 这是"答案唯一"的**唯一**依据。
// 前提二：两个数组是【同一组元素的两个排列】（前序与中序描述的是同一棵树）。
List<string> PremiseFaults(int[] pre, int[] inn)
{
    List<string> bad = new List<string>();
    if (pre.Length != inn.Length) bad.Add($"长度不等（前序 {pre.Length} / 中序 {inn.Length}）");
    if (pre.Distinct().Count() != pre.Length) bad.Add("前序里存在重复值 ⇒ 答案不再唯一，判题器①失效");
    if (inn.Distinct().Count() != inn.Length) bad.Add("中序里存在重复值 ⇒ 根的位置不唯一，题目前提已破");
    if (!pre.OrderBy(x => x).SequenceEqual(inn.OrderBy(x => x))) bad.Add("两数组不是同一组元素的排列");
    return bad;
}

// ⭐ 尺子自检：手里这条"期望层序"到底靠不靠得住？
//    判据 = **把它反序列化、回读前序与中序，必须与输入一字不差**。
//    （这一步专门防"我手写的期望值自己就抄错了" —— 期望值错了，后面全白判。）
bool ExpectedSelfCheck(string expected, int[] pre, int[] inn)
{
    TreeNode? exp = Parse(expected);
    return Preorder(exp).SequenceEqual(pre) && Inorder(exp).SequenceEqual(inn);
}

// ───────────────── 判题器①：期望值比对（本题答案唯一 ⇒ 可以比对） ─────────────────

(bool ready, TreeNode? tree, long ms) TryBuild(int[] pre, int[] inn)
{
    try
    {
        Stopwatch sw = Stopwatch.StartNew();
        TreeNode? t = new Solution().BuildTree(pre, inn);
        sw.Stop();
        return (true, t, sw.ElapsedMilliseconds);
    }
    catch (NotImplementedException) { return (false, null, -1); }
}

string Norm(string s) => s.Replace(" ", "");

// 斜链造数组：pre 恒为 [1..n]；in 视方向而定（左斜 = 倒序，右斜 = 正序）
int[] LeftChainPre(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = i + 1; return a; }
int[] LeftChainIn(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = n - i; return a; }
int[] RightChainPre(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = i + 1; return a; }
int[] RightChainIn(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = i + 1; return a; }

// ────────────────────────── 用例表（唯一数据源） ──────────────────────────
// ⚠️ Expected == null 表示"这组不给期望值"（规模太大，手写不划算）⇒ 只走判题器②。
(string Title, int[] Pre, int[] Inn, string? Expected)[] cases = new (string, int[], int[], string?)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：pre [3,9,20,15,7] / in [9,3,15,20,7]", new int[] { 3, 9, 20, 15, 7 }, new int[] { 9, 3, 15, 20, 7 }, "[3,9,20,null,null,15,7]"),
    ("★ 官方示例 2：单节点 pre [-1] / in [-1]", new int[] { -1 }, new int[] { -1 }, "[-1]"),

    // ── 边界 ──
    ("★ 空输入（题面约束 ≥ 1 个元素，但边界值得试）", Array.Empty<int>(), Array.Empty<int>(), "[空树]"),
    ("★ 单节点 [5]", new int[] { 5 }, new int[] { 5 }, "[5]"),

    // ── ⛔ 镜像组：节点数一样、形状互为镜像 ──
    //    专抓两类错：① 左右切分搞反 ② 子区间长度算错（少算/多算一个）
    ("⛔ 右斜链 2 节点：pre [1,2] / in [1,2]", new int[] { 1, 2 }, new int[] { 1, 2 }, "[1,null,2]"),
    ("⛔ 左斜链 2 节点：pre [1,2] / in [2,1]", new int[] { 1, 2 }, new int[] { 2, 1 }, "[1,2]"),
    ("⛔ 右斜链 3 节点：pre [1,2,3] / in [1,2,3]", new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, "[1,null,2,null,3]"),
    ("⛔ 左斜链 3 节点：pre [1,2,3] / in [3,2,1]", new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 }, "[1,2,null,3]"),

    // ── 满树 / 偏斜 ──
    ("★ 完美满树 3 层：pre [1,2,4,5,3,6,7] / in [4,2,5,1,6,3,7]", new int[] { 1, 2, 4, 5, 3, 6, 7 }, new int[] { 4, 2, 5, 1, 6, 3, 7 }, "[1,2,3,4,5,6,7]"),
    ("★ 左支更深（4 是 2 的**右**孩子）：pre [1,2,4,3] / in [2,4,1,3]", new int[] { 1, 2, 4, 3 }, new int[] { 2, 4, 1, 3 }, "[1,2,3,null,4]"),
    ("★ 左支更深（4 是 2 的**左**孩子）：pre [1,2,4,3] / in [4,2,1,3]", new int[] { 1, 2, 4, 3 }, new int[] { 4, 2, 1, 3 }, "[1,2,3,4]"),

    // ── 值域 ──
    ("★ 值域含 0 与负数：pre [0,-3,-5,9] / in [-5,-3,0,9]", new int[] { 0, -3, -5, 9 }, new int[] { -5, -3, 0, 9 }, "[0,-3,9,-5]"),
    ("⛔ int 两端：pre [Min,Max] / in [Min,Max]", new int[] { int.MinValue, int.MaxValue }, new int[] { int.MinValue, int.MaxValue }, "[-2147483648,null,2147483647]"),

    // ── 规模（**不给期望值** ⇒ 只走判题器② 的独立裁判）──
    ("★ n = 1000 左斜链（不给期望值，只查性质）", LeftChainPre(1000), LeftChainIn(1000), null),
    ("★ n = 1000 右斜链（不给期望值，只查性质）", RightChainPre(1000), RightChainIn(1000), null),
};

// ══════════════════════════════ 开 跑 ══════════════════════════════

Console.WriteLine("═══ 判题器⓪-A：尺子自检（期望值必须先证明它对 —— 不碰 Solution）═══");
Console.WriteLine("   判据：把期望层序【反序列化】后回读前序/中序，必须与输入一字不差。");
foreach (var c in cases)
{
    if (c.Expected is null) { Console.WriteLine($"➖ [尺子] {c.Title}  → 本组未给期望值，跳过"); continue; }
    bool ok = ExpectedSelfCheck(c.Expected, c.Pre, c.Inn);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [尺子] {c.Title}");
    if (!ok) Console.WriteLine($"     ⛔ 期望值 {c.Expected} 与输入不自洽 ⇒ **先改用例表，别急着看你的解**。");
}

Console.WriteLine("\n═══ 判题器⓪-B：题目前提自检（元素互不相同 + 互为排列）═══");
Console.WriteLine("   ⭐ 为什么查这个：「答案唯一」这条命，全靠「元素互不相同」吊着。");
foreach (var c in cases)
{
    List<string> bad = PremiseFaults(c.Pre, c.Inn);
    Console.WriteLine($"{(bad.Count == 0 ? "✅" : "❌")} [前提] {c.Title}");
    if (bad.Count > 0) Console.WriteLine($"     ⛔ {string.Join("｜", bad)} ⇒ 题目前提不成立，先修用例表。");
}

Console.WriteLine("\n═══ 判题器①：期望值比对（本题答案唯一 ⇒ 敢比对）═══");
foreach (var c in cases)
{
    var r = TryBuild(c.Pre, c.Inn);
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    string got = Show(r.tree);
    if (c.Expected is null)
    {
        Console.WriteLine($"➖ {c.Title}  〔无期望值 ⇒ 交给判题器②〕  拿到：{(CountNodes(r.tree) <= 16 ? got : $"[{CountNodes(r.tree)} 个节点]")}");
        continue;
    }
    bool ok = Norm(got) == Norm(c.Expected);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {c.Title}");
    if (!ok) Console.WriteLine($"     期望：{c.Expected}\n     拿到：{got}");
}

Console.WriteLine("\n═══ 判题器②：独立裁判 = 题目定义逐字直译（**不看期望值**）═══");
Console.WriteLine("   题目定义只有两句：「前序遍历 == preorder」且「中序遍历 == inorder」。");
Console.WriteLine("   ⭐ 它和判题器①是**两条独立的路**：①抄期望值、②回读定义 —— 一条错了另一条能抓。");
foreach (var c in cases)
{
    var r = TryBuild(c.Pre, c.Inn);
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    bool okPre = Preorder(r.tree).SequenceEqual(c.Pre);
    bool okIn = Inorder(r.tree).SequenceEqual(c.Inn);
    bool okShape = CountNodes(r.tree) == c.Pre.Length;
    Console.WriteLine($"{(okPre && okIn && okShape ? "✅" : "❌")} {c.Title}   〔前序回读 {(okPre ? "✅" : "❌")}｜中序回读 {(okIn ? "✅" : "❌")}｜节点数对 {(okShape ? "✅" : "❌")}〕");
}

Console.WriteLine("\n═══ 判题器③：形状观察（**不计错**，只打印）═══");
foreach (var c in cases)
{
    var r = TryBuild(c.Pre, c.Inn);
    if (!r.ready) break;
    int n = c.Pre.Length;
    Console.WriteLine($"   · n = {n,4}｜树高 = {Height(r.tree),4}｜层序：{(n <= 16 ? Show(r.tree) : $"[{n} 个节点，略]")}   —— {c.Title}");
}

// ═══════════════ 性能区：把「定位根」的代价变成看得见的数字 ═══════════════
//  ⚠️ 斜链的**递归深度 = n**（不像 108 那样是 log n）⇒ 规模这里只上到 2000，
//     再大有可能撞栈溢出（.NET 的 StackOverflowException 不可捕获，进程当场死）。
double Measure(int[] pre, int[] inn, int repeat)
{
    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < repeat; i++) _ = new Solution().BuildTree(pre, inn);
    sw.Stop();
    return sw.Elapsed.TotalMilliseconds;
}

string ScaleVerdict(double small, double big)
{
    if (small < 1.0 && big < 2.0) return "🔵 两次都太小（< 2 ms）⇒ 看不出斜率，但至少说明这一步**没有**在反复扫长数组";
    double ratio = big / Math.Max(small, 0.0001);
    if (ratio < 2.6) return $"🟢 约 **{ratio:F1} 倍** ⇒ 输入翻倍、代价跟着翻倍 ⇒ **线性**";
    if (ratio > 3.4) return $"🟡 约 **{ratio:F1} 倍** ⇒ 输入翻倍、代价翻 **4** 倍 ⇒ **平方**";
    return $"🟠 约 **{ratio:F1} 倍** ⇒ 落在两者之间（小规模计时噪声大，多跑几次看趋势）";
}

Console.WriteLine("\n═══ 性能区：节点数一样、形状互为镜像的两条斜链 ═══");
if (!TryBuild(Array.Empty<int>(), Array.Empty<int>()).ready)
{
    Console.WriteLine("   ⏳ Solution 尚未实现 ⇒ 性能区跳过。（下面这段判据可以留着写完再看。）");
}
else
{
    const int repeat = 20;
    int[] lp1 = LeftChainPre(1000), li1 = LeftChainIn(1000);
    int[] lp2 = LeftChainPre(2000), li2 = LeftChainIn(2000);
    int[] rp2 = RightChainPre(2000), ri2 = RightChainIn(2000);

    double tL1 = Measure(lp1, li1, repeat);
    double tL2 = Measure(lp2, li2, repeat);
    double tR2 = Measure(rp2, ri2, repeat);

    Console.WriteLine($"   形状      n      跑 {repeat} 次总耗时      单次");
    Console.WriteLine($"   左斜链   1000   {tL1,10:F2} ms      {tL1 / repeat,8:F3} ms");
    Console.WriteLine($"   左斜链   2000   {tL2,10:F2} ms      {tL2 / repeat,8:F3} ms");
    Console.WriteLine($"   右斜链   2000   {tR2,10:F2} ms      {tR2 / repeat,8:F3} ms");
    Console.WriteLine();
    Console.WriteLine($"   判据 A（同形状、规模翻倍）：左斜链 1000 → 2000 = {tL1:F2} → {tL2:F2} ms");
    Console.WriteLine($"        {ScaleVerdict(tL1, tL2)}");
    Console.WriteLine($"   判据 B（同规模、形状相反）：n = 2000 上，左斜 {tL2:F2} ms vs 右斜 {tR2:F2} ms"
        + (tR2 > 0.01 ? $" ⇒ 相差 {tL2 / Math.Max(tR2, 0.0001):F1} 倍" : ""));
    Console.WriteLine();
    Console.WriteLine("   ⭐ 先别急着改代码，先回答三个问题（这才是这块数字的用处）：");
    Console.WriteLine("      ① 两棵树**节点数完全一样**，为什么代价可能差很多？");
    Console.WriteLine("          （提示：去问「每一步里、那个最花时间的动作，在两种形状上各要做多少次」。）");
    Console.WriteLine("      ② 判据 A 的斜率是线性的还是平方的？它取决于**哪一步**？");
    Console.WriteLine("      ③ 如果你的斜率已经是线性的：那一步被换成了什么？（数据换空间，还是换了别的？）");
}

// ══════════════════════════════ 类型与答题区 ══════════════════════════════

public class TreeNode {
    public int val;
    public TreeNode? left;
    public TreeNode? right;
    public TreeNode(int val = 0, TreeNode? left = null, TreeNode? right = null) {
        this.val = val;
        this.left = left;
        this.right = right;
    }
}

public class Solution {
    // TODO: 用【前序】与【中序】两个序列，还原出唯一的那棵二叉树，返回根节点。
    //
    //   ── 抓题眼（先想清楚再落笔，别先写代码）：
    //      ① 前序的第一个元素 = **整棵树的根**（前序的定义：根 → 左 → 右）。
    //      ② 拿着这个根的值，去**中序**里找到它 —— 它左边那一堆 = 左子树的全部节点，
    //         右边那一堆 = 右子树的全部节点（中序的定义：左 → 根 → 右）。
    //      ③ 于是原问题被**切成两个同型的小问题**：左子树、右子树。
    //         ⇒ 这是一个"分治/递归"的形状 —— 和 108 是同一族（都是"造树"），
    //            但**切法完全不同**：108 靠"有序 + 取中点"直接落位，本题靠"找根 + 切两半"。
    //      ⭐ 一句话概括：**前序告诉你"谁是根"，中序告诉你"根左右各有谁"。**
    //
    //   ── 与前面几站的关系（二叉树线一路走到这里）：
    //      遍历（144/94/145）→ DFS 变体（104/226/101/110）→ BFS（102/199）
    //      → BST（98 查 / 700 找 / 108 造）→ 本题：**第一次"从序列反推结构"**
    //      注意方向：前面都是"给我一棵树，我读它"；本题是"给我两个序列，我把它长回来"。
    //
    //   ── 落笔前必须回答的六个问题：
    //      ① 递归函数要处理什么？—— 你手里必须同时握住**两段区间**（前序一段、中序一段）。
    //         问自己：这两段的**起点/终点**各是什么？能不能只传 4 个下标？
    //      ② 为什么不直接 `Array.Copy` 切出新数组？两种做法的空间复杂度差在哪？
    //         （承 108 的"路径①与②"）
    //      ③ 前序那一段里，"根的左孩子那棵子树"占了几格？这个格数从哪儿来？
    //         （提示：它 **不来自前序**，而来自**中序**里根左边那堆的数量 —— 这是本题最容易错的一步）
    //      ④ 终止条件写什么？"空区间"和"只剩一个元素"分别该返回什么？
    //         想清楚：空输入那组用例要过，靠的是哪一行。
    //      ⑤ 找根那一步怎么找？`IndexOf` 直接扫，还是先建一张表？
    //         先别优化 —— 把①~④写对、跑绿，再拿**性能区**的数字来决定要不要换。
    //         换之前先回答：换了之后，**哪一步**变便宜了？代价又付在哪儿？
    //      ⑥ ⭐ 为什么可以用**前序的第一个**去中序里找，而不是反过来用中序的某个值去前序里找？
    //         （提示：前序的"第一个"是**全局**信息；中序的第一个是"最左边那个"，语义不同。）
    //
    //   ── 写完自己回答（这才是今天的账）：
    //      · 本题与 108 的判题器为什么一个能比对、一个只能比性质？"唯一性"是谁给的？
    //      · ⭐ 性能区判据 B：节点数一样的两条斜链，代价差在哪一步、差多少倍？
    //        如果差很多，说明什么？（承 700 那句"O(h) 里的 h 是树高"的味道 ——
    //        这次变的是"**同一个动作在两种形状上要做几次**"）
    //      · 判据 A 的斜率若从平方变线性，是靠**哪一步**的替换换来的？新方案多付了什么？
    //      · ⛔ 有个"看起来更聪明"的坑：既然前序第一个是根，那**只拿前序**能不能建树？
    //        用 `[1,2,3]` 试一下 —— 它对应几棵不同的树？（这就是中序不可省的证据）
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      时间 O(?)、空间 O(?)      ← 算完拿性能区的数字验一验
    //      ⚠️ 空间别忘了算**递归深度**（斜链上有多深？）
    //
    //   ── 验收标准：
    //      ① 判题器⓪-A 全绿 且 ⓪-B 全绿（尺子准 + 前提成立）；
    //      ② 判题器① 全绿（有期望值的那些组，层序一字不差）；
    //      ③ 判题器② 全绿（前序回读 / 中序回读 / 节点数，三项全 ✅）；
    //      ④ 性能区判据 A 的斜率说得出来，判据 B 的差异**能一句话解释清楚**；
    //      ⑤ 能一句话回答："**为什么只要元素互不相同，这棵树就是唯一的？**"
    //
    //   ── 进阶（做完一条再说，别今天硬塞）：
    //      ① 把"找根"换成一张 `值 → 中序下标` 的表，再跑一次性能区 —— 看判据 A 的斜率怎么变。
    //      ② 不用递归 → 用**显式栈 + 区间**写一版（照 104 / 145 那条"同一顺序、换工具"的线）。
    //      ③ 变体（面试常追问）：只给**前序 + 后序**行不行？（拿 `[1,2]` 想一想：
    //         前序 `[1,2]` + 后序 `[2,1]` 能定出一棵树；但 `[1,2]` / `[2,1]` 只给前序就不行。
    //         结论留给你写：**什么时候唯一、什么时候不唯一**。）
    //      ④ 变体（这题是 106）：把前序换成**后序** —— 后序的"根"在**最后一个**，
    //         那切分时哪一段要跟着变？（先别写，先说出"变的是哪里"）
    public TreeNode? BuildTree(int[] preorder, int[] inorder)
    {
        Dictionary<int,int> pos = new Dictionary<int, int>();
        for(int i = 0;i < inorder.Length; i++)
        {
            pos[inorder[i]] = i;
        }
        return BuildSubtree(preorder,inorder,0,preorder.Length - 1,0,inorder.Length - 1,pos);
    }
    TreeNode? BuildSubtree(int[] preorder, int[] inorder, int preLo, int preHi, int inLo,  int inHi, Dictionary<int,int> pos)
    {
        if(preHi < preLo || inHi < inLo) return null;
        TreeNode result = new TreeNode();
        int temp = preorder[preLo];
        if(!pos.TryGetValue(temp, out int mid)) throw new InvalidOperationException($"inorder 里找不到 {temp} ⇒ 切分已经错了，别让它悄悄产出一棵假树");
        result.val = temp;
        int leftsize = mid - inLo;
        int rightsize = inHi - mid;
        result.left = BuildSubtree(preorder, inorder, preLo + 1, preLo + leftsize, inLo, inLo + leftsize - 1, pos);
        result.right = BuildSubtree(preorder, inorder, preHi - rightsize + 1, preHi, inHi - rightsize + 1, inHi, pos);
        return result;
    }
}
