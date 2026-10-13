using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════════════════
//  106 · 从中序与后序遍历序列构造二叉树
//  脚手架（第十四次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144/94/145/102/104/226/101/110/199/98/700/108/105 → 本题）
//
//  ⭐ 与上一站 105 的**三处对照**（本题的"变的是哪里"全在这里，落笔前先读一遍）：
//
//   ① 【根的来源换了一头】
//      105：前序 = [根][左][右] ⇒ 根在**段首**；
//      106：后序 = [左][右][根] ⇒ 根在**段尾**。
//      中序不变：它只负责"根左右各有谁"（用来查 mid）。
//      ⇒ 题眼一句话：**后序告诉你"谁是根"，中序告诉你"根左右各有谁"。**
//
//   ② 【独立裁判跟着换了一条】
//      105 的独立裁判 = 回读**前序 + 中序**；
//      本题的独立裁判 = 回读**中序 + 后序**。
//      ⚠️ 只回读一种序是**判不出唯一解**的 —— 本用例表里专设了一对
//         「后序相同、中序不同」的用例（它们是两棵不同的树），就是拿来盯这一点的。
//
//   ③ 【性能区照旧用"节点数一样、形状互为镜像的两条斜链"】
//      且本题两条斜链的**后序一模一样**（都是 [n..1]）、**只有中序不同** ⇒
//      又是一组"输入长得几乎一样、代价却不同"的对照。
//      （判据：去问"最花时间的那个动作，在两种形状上各要做几次"。）
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十四次复用） ─────────────────────────

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

// 反序列化：把 Show 出来的字符串重新读回一棵树（判题器⓪-A 尺子自检要用）
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
// ⚠️ 本题用的是 **中序 + 后序**（105 用的是前序 + 中序，别照抄错）
List<int> Inorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n) { if (n is null) return; Walk(n.left); sink.Add(n.val); Walk(n.right); }
    Walk(root);
    return sink;
}
List<int> Postorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n) { if (n is null) return; Walk(n.left); Walk(n.right); sink.Add(n.val); }
    Walk(root);
    return sink;
}

// ───────────────── 判题器⓪：尺子自检 + 输入前提自检（不碰 Solution） ─────────────────

// 前提一：【元素互不相同】—— 这是"答案唯一"的**唯一**依据。
// 前提二：两个数组是【同一组元素的两个排列】（中序与后序描述的是同一棵树）。
List<string> PremiseFaults(int[] inn, int[] post)
{
    List<string> bad = new List<string>();
    if (inn.Length != post.Length) bad.Add($"长度不等（中序 {inn.Length} / 后序 {post.Length}）");
    if (inn.Distinct().Count() != inn.Length) bad.Add("中序里存在重复值 ⇒ 根的位置不唯一，题目前提已破");
    if (post.Distinct().Count() != post.Length) bad.Add("后序里存在重复值 ⇒ 答案不再唯一，判题器①失效");
    if (!inn.OrderBy(x => x).SequenceEqual(post.OrderBy(x => x))) bad.Add("两数组不是同一组元素的排列");
    return bad;
}

// ⭐ 尺子自检：手里这条"期望层序"到底靠不靠得住？
//    判据 = **把它反序列化、回读中序与后序，必须与输入一字不差**。
//    （专门防"我手写的期望值自己就抄错了" —— 期望值错了，后面全白判。）
bool ExpectedSelfCheck(string expected, int[] inn, int[] post)
{
    TreeNode? exp = Parse(expected);
    return Inorder(exp).SequenceEqual(inn) && Postorder(exp).SequenceEqual(post);
}

// ───────────────── 判题器①：期望值比对（本题答案唯一 ⇒ 可以比对） ─────────────────

(bool ready, TreeNode? tree, long ms) TryBuild(int[] inn, int[] post)
{
    try
    {
        Stopwatch sw = Stopwatch.StartNew();
        TreeNode? t = new Solution().BuildTree(inn, post);
        sw.Stop();
        return (true, t, sw.ElapsedMilliseconds);
    }
    catch (NotImplementedException) { return (false, null, -1); }
}

string Norm(string s) => s.Replace(" ", "");

// 斜链造数组：
//   后序（两种方向**一样**）= [n, n-1, ..., 1]（一路"先孩子后自己"）
//   中序：左斜链 = 倒序 [n..1]；右斜链 = 正序 [1..n]
int[] ChainPostOrder(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = n - i; return a; }
int[] LeftChainInOrder(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = n - i; return a; }
int[] RightChainInOrder(int n) { int[] a = new int[n]; for (int i = 0; i < n; i++) a[i] = i + 1; return a; }

// ────────────────────────── 用例表（唯一数据源） ──────────────────────────
// ⚠️ Expected == null 表示"这组不给期望值"（规模太大，手写不划算）⇒ 只走判题器②。
(string Title, int[] In, int[] Post, string? Expected)[] cases = new (string, int[], int[], string?)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：in [9,3,15,20,7] / post [9,15,7,20,3]", new int[] { 9, 3, 15, 20, 7 }, new int[] { 9, 15, 7, 20, 3 }, "[3,9,20,null,null,15,7]"),
    ("★ 官方示例 2：单节点 in [-1] / post [-1]", new int[] { -1 }, new int[] { -1 }, "[-1]"),

    // ── 边界 ──
    ("★ 空输入（题面约束 ≥ 1 个元素，但边界值得试）", Array.Empty<int>(), Array.Empty<int>(), "[空树]"),
    ("★ 单节点 [5]", new int[] { 5 }, new int[] { 5 }, "[5]"),

    // ── ⛔ 镜像组：节点数一样、形状互为镜像（后序**完全相同**，只有中序不同）──
    //    专抓两类错：① 左右切分搞反 ② 子区间长度算错（少算/多算一个）
    ("⛔ 右斜链 2 节点：in [1,2] / post [2,1]", new int[] { 1, 2 }, new int[] { 2, 1 }, "[1,null,2]"),
    ("⛔ 左斜链 2 节点：in [2,1] / post [2,1]", new int[] { 2, 1 }, new int[] { 2, 1 }, "[1,2]"),
    ("⛔ 右斜链 3 节点：in [1,2,3] / post [3,2,1]", new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 }, "[1,null,2,null,3]"),
    ("⛔ 左斜链 3 节点：in [3,2,1] / post [3,2,1]", new int[] { 3, 2, 1 }, new int[] { 3, 2, 1 }, "[1,2,null,3]"),

    // ── 满树 / 偏斜 ──
    ("★ 完美满树 3 层：in [4,2,5,1,6,3,7] / post [4,5,2,6,7,3,1]", new int[] { 4, 2, 5, 1, 6, 3, 7 }, new int[] { 4, 5, 2, 6, 7, 3, 1 }, "[1,2,3,4,5,6,7]"),
    // ⭐ 这一对是本题特有的：**后序一模一样，中序不同 ⇒ 两棵不同的树**
    ("⛔ 后序相同 · 4 是 2 的**右**孩子：in [2,4,1,3] / post [4,2,3,1]", new int[] { 2, 4, 1, 3 }, new int[] { 4, 2, 3, 1 }, "[1,2,3,null,4]"),
    ("⛔ 后序相同 · 4 是 2 的**左**孩子：in [4,2,1,3] / post [4,2,3,1]", new int[] { 4, 2, 1, 3 }, new int[] { 4, 2, 3, 1 }, "[1,2,3,4]"),

    // ── 值域 ──
    ("★ 值域含 0 与负数：in [-5,-3,0,9] / post [-5,-3,9,0]", new int[] { -5, -3, 0, 9 }, new int[] { -5, -3, 9, 0 }, "[0,-3,9,-5]"),
    ("⛔ int 两端：in [Min,Max] / post [Min,Max]", new int[] { int.MinValue, int.MaxValue }, new int[] { int.MinValue, int.MaxValue }, "[2147483647,-2147483648]"),

    // ── 规模（**不给期望值** ⇒ 只走判题器② 的独立裁判）──
    ("★ n = 1000 左斜链（不给期望值，只查性质）", LeftChainInOrder(1000), ChainPostOrder(1000), null),
    ("★ n = 1000 右斜链（不给期望值，只查性质）", RightChainInOrder(1000), ChainPostOrder(1000), null),
};

// ══════════════════════════════ 开 跑 ══════════════════════════════

Console.WriteLine("═══ 判题器⓪-A：尺子自检（期望值必须先证明它对 —— 不碰 Solution）═══");
Console.WriteLine("   判据：把期望层序【反序列化】后回读中序/后序，必须与输入一字不差。");
foreach (var c in cases)
{
    if (c.Expected is null) { Console.WriteLine($"➖ [尺子] {c.Title}  → 本组未给期望值，跳过"); continue; }
    bool ok = ExpectedSelfCheck(c.Expected, c.In, c.Post);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [尺子] {c.Title}");
    if (!ok) Console.WriteLine($"     ⛔ 期望值 {c.Expected} 与输入不自洽 ⇒ **先改用例表，别急着看你的解**。");
}

Console.WriteLine("\n═══ 判题器⓪-B：题目前提自检（元素互不相同 + 互为排列）═══");
Console.WriteLine("   ⭐ 为什么查这个：「答案唯一」这条命，全靠「元素互不相同」吊着。");
foreach (var c in cases)
{
    List<string> bad = PremiseFaults(c.In, c.Post);
    Console.WriteLine($"{(bad.Count == 0 ? "✅" : "❌")} [前提] {c.Title}");
    if (bad.Count > 0) Console.WriteLine($"     ⛔ {string.Join("｜", bad)} ⇒ 题目前提不成立，先修用例表。");
}

Console.WriteLine("\n═══ 判题器①：期望值比对（本题答案唯一 ⇒ 敢比对）═══");
foreach (var c in cases)
{
    var r = TryBuild(c.In, c.Post);
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
Console.WriteLine("   题目定义只有两句：「中序遍历 == inorder」且「后序遍历 == postorder」。");
Console.WriteLine("   ⭐ 它和判题器①是**两条独立的路**：①抄期望值、②回读定义 —— 一条错了另一条能抓。");
Console.WriteLine("   ⚠️ 两条**都要**回读 —— 只回读一条判不出唯一解（用例表里那对『后序相同』就是证据）。");
foreach (var c in cases)
{
    var r = TryBuild(c.In, c.Post);
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    bool okIn = Inorder(r.tree).SequenceEqual(c.In);
    bool okPost = Postorder(r.tree).SequenceEqual(c.Post);
    bool okShape = CountNodes(r.tree) == c.In.Length;
    Console.WriteLine($"{(okIn && okPost && okShape ? "✅" : "❌")} {c.Title}   〔中序回读 {(okIn ? "✅" : "❌")}｜后序回读 {(okPost ? "✅" : "❌")}｜节点数对 {(okShape ? "✅" : "❌")}〕");
}

Console.WriteLine("\n═══ 判题器③：形状观察（**不计错**，只打印）═══");
foreach (var c in cases)
{
    var r = TryBuild(c.In, c.Post);
    if (!r.ready) break;
    int n = c.In.Length;
    Console.WriteLine($"   · n = {n,4}｜树高 = {Height(r.tree),4}｜层序：{(n <= 16 ? Show(r.tree) : $"[{n} 个节点，略]")}   —— {c.Title}");
}

// ═══════════════ 性能区：把「定位根」的代价变成看得见的数字 ═══════════════
//  ⚠️ 斜链的**递归深度 = n**（不像 108 那样是 log n）⇒ 规模这里只上到 2000，
//     再大有可能撞栈溢出（.NET 的 StackOverflowException 不可捕获，进程当场死）。
double Measure(int[] inn, int[] post, int repeat)
{
    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < repeat; i++) _ = new Solution().BuildTree(inn, post);
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

Console.WriteLine("\n═══ 性能区：节点数一样、形状互为镜像的两条斜链（本题后序完全相同！）═══");
if (!TryBuild(Array.Empty<int>(), Array.Empty<int>()).ready)
{
    Console.WriteLine("   ⏳ Solution 尚未实现 ⇒ 性能区跳过。（下面这段判据可以留着写完再看。）");
}
else
{
    const int repeat = 20;
    int[] li1 = LeftChainInOrder(1000), lp1 = ChainPostOrder(1000);
    int[] li2 = LeftChainInOrder(2000), lp2 = ChainPostOrder(2000);
    int[] ri2 = RightChainInOrder(2000), rp2 = ChainPostOrder(2000);

    double tL1 = Measure(li1, lp1, repeat);
    double tL2 = Measure(li2, lp2, repeat);
    double tR2 = Measure(ri2, rp2, repeat);

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
    Console.WriteLine("      ① 两条斜链的**后序一模一样**，为什么代价可能差很多？");
    Console.WriteLine("          （提示：去问「每一步里、那个最花时间的动作，在两种形状上各要做多少次」。）");
    Console.WriteLine("      ② 判据 A 的斜率是线性的还是平方的？它取决于**哪一步**？");
    Console.WriteLine("      ③ 如果你的斜率已经是线性的：那一步被换成了什么？（拿空间换的，还是换了别的？）");
}

// ══════════════════════════════ 类型与答题区 ══════════════════════════════

public class TreeNode
{
    public int val;
    public TreeNode? left;
    public TreeNode? right;
    public TreeNode(int val = 0, TreeNode? left = null, TreeNode? right = null)
    {
        this.val = val;
        this.left = left;
        this.right = right;
    }
}

public class Solution
{
    // TODO: 用【中序】与【后序】两个序列，还原出唯一的那棵二叉树，返回根节点。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      ① 后序的**最后一个**元素 = 整棵树的根（后序的定义：左 → 右 → 根）。
    //      ② 拿着这个根的值去**中序**里找它 —— 左边那堆 = 左子树全部，右边那堆 = 右子树全部。
    //      ③ 后序"去掉最后一个根"之后：**前 L 格**是左子树（L = mid - inLo），**剩下的**是右子树。
    //      ⭐ 一句话：**后序告诉你"谁是根"，中序告诉你"根左右各有谁"。**
    //
    //   ── 与 105 的分界线（只差一头，但区间要跟着翻面）：
    //      105：根在**前序段首** ⇒ 左子树接在"根后面"，右子树顶到"段尾"；
    //      本题：根在**后序段尾** ⇒ 左子树顶在"段首"，右子树接在"根前面"。
    //
    //   ── 落笔前必须回答的六个问题：
    //      ① 递归函数要握住**哪两段区间**？4 个下标够不够？（够 —— 两段都是闭区间）
    //      ② 为什么不用 `Array.Copy` 切新数组？空间差在哪？
    //      ③ 左子树的**后序区间**是哪两格？右子树的呢？
    //         （提示：后序里根占掉的是**最后一格**，不是第一格 —— 这是本题最容易错的一步）
    //      ④ 终止条件是什么？哪一段会出现"起点 > 终点"？
    //      ⑤ 找根那一步：`IndexOf` 直接扫，还是先建表？先跑绿再谈优化。
    //      ⑥ ⭐ `mid` 的初值为什么不能是 `0`？（105 里它是"栈溢出的放大器" —— 会怎样放大？）
    //
    //   ── 写完自己回答（这才是今天的账）：
    //      · 用例表里那对「后序相同、中序不同」为什么会是两棵树？
    //        它俩一起证明了什么？（提示：只给一种序够不够）
    //      · ⭐ 性能区判据 B：两条斜链**后序完全一样**，代价差在哪一步、差多少倍？
    //      · ⭐ 判据 A 的斜率是线性还是平方？取决于哪一步？（写完再拿数字验）
    //      · ⛔ 只给 `postorder` 能不能建树？拿 `[1,2]` 试一下它对应几棵不同的树。
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      时间 O(?)、空间 O(?)      ← 算完拿性能区的数字验一验
    //      ⚠️ 空间别忘了算**递归深度**（斜链上有多深？）
    //
    //   ── 验收标准：
    //      ① 判题器⓪-A 全绿 且 ⓪-B 全绿（尺子准 + 前提成立）；
    //      ② 判题器① 全绿（有期望值的那些组，层序一字不差）；
    //      ③ 判题器② 全绿（中序回读 / 后序回读 / 节点数，三项全 ✅）；
    //      ④ 性能区判据 A 的斜率说得出来、判据 B 的差异**能一句话解释清楚**；
    //      ⑤ 能一句话回答："**为什么只要元素互不相同，这棵树就是唯一的？**"
    //
    //   ── 进阶（做完一条再说）：
    //      ① 把"找根"换成一张 `值 → 中序下标` 的表，再跑一次性能区，看判据 A 怎么变。
    //      ② 不用递归 → 用**显式栈 + 区间**写一版（照 104 / 145 那条线）。
    //      ③ 变体（面试常追问）：只给**前序 + 后序**行不行？什么时候唯一、什么时候不唯一？
    public TreeNode? BuildTree(int[] inorder, int[] postorder)
    {
        Dictionary<int,int> pos = new Dictionary<int, int>();
        for(int i = 0;i < inorder.Length; i++)
        {
            pos[inorder[i]] = i;
        }
        return BuildSubtree(inorder,postorder,0,inorder.Length - 1,0,postorder.Length - 1,pos);
    }
    TreeNode? BuildSubtree(int[] inorder, int[] postorder, int inLo,  int inHi, int posLo, int posHi, Dictionary<int,int> pos)
    {
        if(inHi < inLo || posHi < posLo) return null;
        TreeNode result = new TreeNode();
        int temp = postorder[posHi];
        if(!pos.TryGetValue(temp, out int mid)) throw new InvalidOperationException($"inorder 里找不到 {temp} ⇒ 切分已经错了，别让它悄悄产出一棵假树");
        result.val = temp;
        int leftsize = mid - inLo;
        int rightsize = inHi - mid;
        result.left = BuildSubtree(inorder,postorder,inLo,inLo + leftsize - 1,posLo,posLo + leftsize - 1,pos);
        result.right = BuildSubtree(inorder,postorder,inHi - rightsize + 1,inHi,posHi - rightsize,posHi - 1,pos);
        return result;
    }
}
