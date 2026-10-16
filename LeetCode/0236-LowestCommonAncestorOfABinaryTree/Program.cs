using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════════════════
//  236 · 二叉树的最近公共祖先（LCA）（分治的第三张脸）
//  脚手架（第十七次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144 / 94 / 145 / 102 / 104 / 226 / 101 / 110 / 199 / 98 / 700 /
//            108 / 105 / 106 / 114 / 230 → 本题）
//
//  ⭐ 与前面 16 站的分界线（落笔前先读一遍）：
//
//   ① 【答案第一次是"树上的一个节点"，而不是"一个数 / 一种形态"】
//      104 回一个数（深度）、110 回两样（(bool, int)）、114 就地改指针、
//      230 回"中序里的第 k 个"。本题的返回值是 **TreeNode** ⇒ 于是本题最大的
//      字号是：**"我这棵子树里没找到"该怎么表达？**
//      （🪞 镜子：98 的 `int?` 哨兵、230 的 `bool isfound` —— **"什么都没有"要靠类型说**）
//
//   ② 【"分治"从"算出一个数"变成"带回来一个节点"】
//      105 / 106 是"从序列把树长回来"、108 是"取中点"，都是**造**；
//      本题是**找**，而且要找两样东西（p 和 q）⇒ 关键在于：
//      **从孩子那里带回来的，是"找到了 p / 找到了 q / 两样都找到了"这件事本身**
//      —— 而不是"p 在哪条路上"这种细节。这就是今天的题眼：
//      **"两棵子树的答案怎么合起来"**。
//
//   ③ 【判题器第一次要问"你返回的是不是原来那个对象"】
//      LCA 的答案必然**是树里已有的节点** ⇒ 判题器②的「引用同一性」这一条
//      会拦下"new 一个同值节点交差"的写法（承 114 的"就地改指针"）。
//      ⚠️ 同时它也解释了：**本题为什么能用"期望值比对"** —— 因为元素互不相同
//      （"答案唯一"这条命吊在它上面；承 105 的 ⓪-B）。
//
//   ④ 【独立裁判 = LCA 定义的逐字直译】
//      "最近公共祖先"的定义 = **同时是 p 和 q 的祖先、且深度最大（离它们最近）的那个节点**。
//      直译版 = 把全树铺平、逐个节点问"你是不是 p 和 q 的祖先？"、记下**最深**的那个 ——
//      **明知可以剪枝也不剪**（这就是它的价值：它是尺子，不是解法）。
//      ⇒ 代价 ≈ 每个节点 × 全树（O(n·h) ~ O(n²)）⇒ 只能在**小用例**上跑。
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十七次复用） ─────────────────────────

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

// 反序列化：把层序串读回一棵树（尺子自检 + 用例建树要用）
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

bool ValuesUnique(TreeNode? root)
{
    HashSet<int> seen = new HashSet<int>();
    bool Walk(TreeNode? node)
    {
        if (node is null) return true;
        if (!seen.Add(node.val)) return false;
        return Walk(node.left) && Walk(node.right);
    }
    return Walk(root);
}

// 按值找节点引用 —— 判题器要用它把"题面里的 p、q"变成"树上的哪个对象"
//   ⚠️ 值唯一是本题的前提 ⇒ 这个查找才是有意义的（否则找到谁都一样）
TreeNode? FindByVal(TreeNode? root, int val)
{
    if (root is null) return null;
    if (root.val == val) return root;
    return FindByVal(root.left, val) ?? FindByVal(root.right, val);
}

// "n 是不是 target 的祖先"（**含自身** —— LCA 定义允许答案是 p 或 q 本人）
//   ⚠️ 这是判题器自己的独立实现（不是 Solution 的）⇒ 它必须能独立回答问题
bool counting = false;   // 记数开关：判题器②借它时不计入裁判的次数账
long ancestorCalls = 0;  // 裁判自己数的"访问次数"

bool IsAncestorOf(TreeNode? node, TreeNode target)
{
    if (counting) ancestorCalls++;
    if (node is null) return false;
    if (ReferenceEquals(node, target)) return true;
    return IsAncestorOf(node.left, target) || IsAncestorOf(node.right, target);
}

// 全树铺平（带深度）—— 直译裁判要看"谁最深"
List<(TreeNode node, int depth)> Flatten(TreeNode? root)
{
    List<(TreeNode, int)> all = new List<(TreeNode, int)>();
    void Walk(TreeNode? node, int depth)
    {
        if (node is null) return;
        all.Add((node, depth));
        Walk(node.left, depth + 1);
        Walk(node.right, depth + 1);
    }
    Walk(root, 0);
    return all;
}

// ⭐ 独立裁判（LCA 定义逐字直译）：同时是 p、q 的祖先，且**最深**的那个
//   **故意不剪枝**：每个节点都要挨个问一遍"你俩的孩子里有没有 p / q"
TreeNode? JudgeLca(TreeNode? root, TreeNode p, TreeNode q)
{
    TreeNode? best = null;
    int bestDepth = -1;
    foreach ((TreeNode node, int depth) in Flatten(root))
    {
        if (IsAncestorOf(node, p) && IsAncestorOf(node, q) && depth > bestDepth)
        {
            best = node;
            bestDepth = depth;
        }
    }
    return best;
}

// 平衡 BST（值 = 1..n）—— 性能区的大树；递归深度只有 log2(n)，安全
TreeNode BuildBalanced(int lo, int hi)
{
    if (lo > hi) return null!;
    int mid = lo + (hi - lo) / 2;
    return new TreeNode(mid, BuildBalanced(lo, mid - 1), BuildBalanced(mid + 1, hi));
}

// 左斜链：root = n，一路 .left 走到 1 ⇒ **高度 = n**（递归深度的尺子）
TreeNode BuildLeftChain(int n)
{
    TreeNode root = new TreeNode(n);
    TreeNode cur = root;
    for (int v = n - 1; v >= 1; v--) { cur.left = new TreeNode(v); cur = cur.left; }
    return root;
}

// 统一调用口：把"没实现 / 抛异常 / 真的算错"分成三笔账
(TreeNode? node, string status) TryLca(Solution sol, TreeNode? root, TreeNode? p, TreeNode? q)
{
    try { return (sol.LowestCommonAncestor(root, p, q), "OK"); }
    catch (NotImplementedException) { return (null, "尚未实现"); }
    catch (Exception e) { return (null, "抛异常:" + e.GetType().Name); }
}

// 性能测量：造树 + 找 p/q（+ 调用）一起测；buildOnly = true 时只测前面那一段 ⇒ 相减 = 净值
(double ms, long sink) Measure(Func<TreeNode, TreeNode, TreeNode, TreeNode?> act, int n, int repeat, bool buildOnly)
{
    long sink = 0;
    Stopwatch sw = Stopwatch.StartNew();
    for (int r = 0; r < repeat; r++)
    {
        TreeNode root = BuildBalanced(1, n);
        TreeNode p = FindByVal(root, 1)!;      // 最左最深叶（值最小）
        TreeNode q = FindByVal(root, n)!;      // 最右最深叶（值最大）
        if (!buildOnly)
        {
            TreeNode? a = act(root, p, q);
            sink += a?.val ?? -1;
        }
    }
    sw.Stop();
    return (sw.Elapsed.TotalMilliseconds, sink);
}

// 裁判对照测量（小规模专用）：只测"直译裁判"本身，看它啃掉了多少次数与时间
(double ms, long calls) MeasureJudge(int n, int repeat)
{
    Stopwatch sw = Stopwatch.StartNew();
    long calls = 0;
    for (int r = 0; r < repeat; r++)
    {
        TreeNode root = BuildLeftChain(n);
        TreeNode p = FindByVal(root, 1)!;
        TreeNode q = FindByVal(root, 2)!;
        ancestorCalls = 0;
        counting = true;
        TreeNode? r2 = JudgeLca(root, p, q);
        counting = false;
        calls += ancestorCalls;
        if (r2 is null) calls = -1;
    }
    sw.Stop();
    return (sw.Elapsed.TotalMilliseconds, calls);
}

// ══════════════════════════════ 顶层流程 ══════════════════════════════

// ── 用例表：存的是「建树串 + p、q 的值」，不是实例（承 114 的纪律）──
//    期望值来自：官方示例 3 组 + 边界手推（父子 / 链两端 / 兄弟 / 深层 / 跨两支）
string[] trees =
{
    "[3, 5, 1, 6, 2, 0, 8, null, null, 7, 4]",                                  // 官方示例 1
    "[3, 5, 1, 6, 2, 0, 8, null, null, 7, 4]",                                  // 官方示例 2（p 自己就是答案）
    "[1, 2]",                                                                   // 官方示例 3（p 是根）
    "[1, 2, 3]",                                                                // 兄弟 → 答案是根
    "[1, 2, 3]",                                                                // p 是根、q 在右支
    "[3, 2, null, 1]",                                                          // 父子相邻：父是答案
    "[3, 2, null, 1]",                                                          // 左斜链两端 → 答案是根
    "[1, null, 2, null, 3]",                                                    // 右斜链两端 → 答案是根
    "[5, 3, 8, 1, 4, 7, 9, null, 2]",                                           // 答案在中层（既不是根也不是叶子）
    "[1, 2, 3, 4, 5, null, 6]",                                                 // 横跨两支 → 答案是根
    "[40, 20, 60, 10, 30, 50, 70]",                                             // 防"答案碰巧等于层序下标"
    "[10, 5, 15, 3, 7, 12, 17, 1, null, 6, null, 11, null, null, 18]",          // 深层：答案在两支分岔处
    "[10, 5, 15, 3, 7, 12, 17, 1, null, 6, null, 11, null, null, 18]",          // 同树，另取一对（答案更靠上）
};
int[] pv = { 5, 5, 1, 2, 3, 1, 1, 3, 2, 4, 10, 1, 18 };
int[] qv = { 1, 4, 2, 3, 1, 2, 3, 1, 4, 6, 30, 6, 11 };
int[] expected = { 3, 5, 1, 1, 1, 2, 3, 1, 3, 1, 20, 5, 15 };

Console.WriteLine("════ ⓪-A 尺子自检（Parse ⇄ Show 往返一致）════");
int rulerPass = 0;
foreach (string s in trees)
{
    TreeNode? t = Parse(s);
    bool ok = Show(t) == s;
    if (ok) rulerPass++;
    Console.WriteLine($"   {(ok ? "✅" : "❌")}  {s}");
}
Console.WriteLine($"   ⇒ {rulerPass}/{trees.Length}"
    + (rulerPass == trees.Length ? " 全绿（尺子可信）" : " ⚠️ 尺子本身有问题，先修尺子"));
Console.WriteLine();

Console.WriteLine("════ ⓪-B 前提自检（值互不相同 + p、q 都在树中 + p ≠ q + 期望答案在树中）════");
int premisePass = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    bool uniq = ValuesUnique(t);
    bool pOk = FindByVal(t, pv[i]) is not null;
    bool qOk = FindByVal(t, qv[i]) is not null;
    bool notSame = pv[i] != qv[i];
    bool eOk = FindByVal(t, expected[i]) is not null;
    bool ok = uniq && pOk && qOk && notSame && eOk;
    if (ok) premisePass++;
    Console.WriteLine($"   组 {i + 1,2}   值唯一={(uniq ? "✅" : "❌")}   p={pv[i],4} 在树中={(pOk ? "✅" : "❌")}"
        + $"   q={qv[i],4} 在树中={(qOk ? "✅" : "❌")}   p≠q={(notSame ? "✅" : "❌")}   期望答案在树中={(eOk ? "✅" : "❌")}");
}
Console.WriteLine($"   ⇒ {premisePass}/{trees.Length}"
    + (premisePass == trees.Length
        ? " 全绿（「答案唯一、且必是树里已存在的节点」这两条命就吊在这些前提上）"
        : " ⚠️ 有非法用例，先修用例"));
Console.WriteLine();

Solution sol = new Solution();

Console.WriteLine("════ 判题器① 期望值比对（每次现场重新造树）════");
int pass1 = 0, notImpl = 0, mism = 0, exCount = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    TreeNode? p = FindByVal(t, pv[i]);
    TreeNode? q = FindByVal(t, qv[i]);
    (TreeNode? node, string status) r = TryLca(sol, t, p, q);
    bool ok = r.status == "OK" && r.node is not null && r.node.val == expected[i];
    if (ok) pass1++;
    else if (r.status == "尚未实现") notImpl++;
    else if (r.status == "OK") mism++;
    else exCount++;
    string got = r.status == "OK" ? (r.node is null ? "null" : r.node.val.ToString()) : r.status;
    Console.WriteLine($"   组 {i + 1,2}   p={pv[i],4}  q={qv[i],4}   期望 {expected[i],4}   实测 {got,8}   {(ok ? "✅" : "❌")}");
}
Console.WriteLine($"   ⇒ {pass1}/{trees.Length}    （值不符 {mism} 组、尚未实现 {notImpl} 组、抛异常 {exCount} 组）");
Console.WriteLine();

if (notImpl > 0)
{
    Console.WriteLine("   ⛔ 还有用例没实现 ⇒ 判题器② / 性能区 / 阶梯区先不跑。");
    Console.WriteLine("      （先把 `LowestCommonAncestor` 写出来，回来看这条流水线怎么抓你。）");
    return;
}

Console.WriteLine("════ 判题器② 独立裁判（同一性 / 是祖先 / 最近 / 没改树 / 幂等 / 直译裁判）════");
int refPass = 0, ancPass = 0, nearPass = 0, intactPass = 0, idemPass = 0, judgePass = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    TreeNode? p = FindByVal(t, pv[i]);
    TreeNode? q = FindByVal(t, qv[i]);
    string before = Show(t);

    (TreeNode? a1, string s1) = TryLca(sol, t, p, q);
    string after = Show(t);
    (TreeNode? a2, string s2) = TryLca(sol, t, p, q);

    bool intact = before == after;
    bool idem = s1 == "OK" && s2 == "OK" && ReferenceEquals(a1, a2);
    if (intact) intactPass++;
    if (idem) idemPass++;

    // ⭐ 引用同一性：返回的必须是**树里那个对象**（不是一个同值的新节点）
    bool refOk = a1 is not null && ReferenceEquals(a1, FindByVal(t, a1.val));
    if (refOk) refPass++;

    // "是祖先"：a1 必须同时是 p 和 q 的祖先（**含自身**）
    bool ancOk = a1 is not null && p is not null && q is not null && IsAncestorOf(a1, p) && IsAncestorOf(a1, q);
    if (ancOk) ancPass++;

    // "最近"：a1 的两个孩子**都不能**是共同祖先 —— 这条就是"最近"的定义
    bool nearOk = false;
    if (a1 is not null && p is not null && q is not null)
    {
        bool leftCommon = a1.left is not null && IsAncestorOf(a1.left, p) && IsAncestorOf(a1.left, q);
        bool rightCommon = a1.right is not null && IsAncestorOf(a1.right, p) && IsAncestorOf(a1.right, q);
        nearOk = !leftCommon && !rightCommon;
    }
    if (nearOk) nearPass++;

    // 独立裁判（定义逐字直译）
    counting = false;
    TreeNode? judge = JudgeLca(t, p!, q!);
    bool judgeOk = a1 is not null && ReferenceEquals(a1, judge);
    if (judgeOk) judgePass++;

    Console.WriteLine($"   组 {i + 1,2}   同一性 {(refOk ? "✅" : "❌")}   是祖先 {(ancOk ? "✅" : "❌")}"
        + $"   最近 {(nearOk ? "✅" : "❌")}   没改树 {(intact ? "✅" : "❌")}   幂等 {(idem ? "✅" : "❌")}"
        + $"   直译裁判 {(judgeOk ? "✅" : "❌")}");
    if (!judgeOk && a1 is not null && judge is not null)
        Console.WriteLine($"        ↳ 直译裁判答：{judge.val}，你的答案：{a1.val}（不一致 ⇒ 先查是谁错）");
}
Console.WriteLine($"   ⇒ 同一性 {refPass}/{trees.Length}　是祖先 {ancPass}/{trees.Length}　最近 {nearPass}/{trees.Length}"
    + $"　没改树 {intactPass}/{trees.Length}　幂等 {idemPass}/{trees.Length}　直译裁判 {judgePass}/{trees.Length}");
Console.WriteLine();

Console.WriteLine("════ 性能区（平衡树 n = 100,000；p、q = 最左叶 / 最右叶 ⇒ 答案是根）════");
const int N = 100_000;
const int Repeat = 20;

double baseMs = Measure((r, pp, qq) => null, N, Repeat, true).ms;
(double msUser, long sinkUser) = Measure((r, pp, qq) => sol.LowestCommonAncestor(r, pp, qq), N, Repeat, false);
double netUser = Math.Max(0.0, msUser - baseMs) / Repeat;

Console.WriteLine($"   造树 + 找 p/q 基线：{baseMs,8:F2} ms（{Repeat} 次）；单次 ≈ {baseMs / Repeat:F3} ms");
Console.WriteLine($"   你的实现：总 {msUser,8:F2} ms   ⇒ 单次净值 {netUser:F4} ms");
Console.WriteLine();

Console.WriteLine("   【对照组】直译裁判（每个节点都要问一遍「你俩的孩子里有没有 p、q」）");
Console.WriteLine("   —— 拿 O(h) 的「链」放在 O(n) 个节点上问 ⇒ Σ(子树大小)，量级自己会说话：");
foreach (int small in new[] { 500, 1000, 2000 })
{
    (double msJ, long callsJ) = MeasureJudge(small, 1);
    Console.WriteLine($"      左斜链 n = {small,5}：裁判访问 {callsJ,12:N0} 次，{msJ,8:F2} ms"
        + $"   （≈ Σ(子树大小) ≈ n²/2 = {small * (small - 1L) / 2:N0} —— 差距就是「它漏了多少次重叠的活」）");
}
Console.WriteLine();
Console.WriteLine("   ⭐ 三笔账要回答的三个问题：");
Console.WriteLine("      ① 你的实现：n 从 100,000 往下砍一半，净值该怎么变？（提示：你每个节点碰了几次？）");
Console.WriteLine("      ② 直译裁判的次数是 n 的几阶？（拿 500 / 1000 / 2000 三档比一比）");
Console.WriteLine("      ③ ⭐ 若你的实现是「对每一层都判一次『p、q 在不在这棵子树里』」，它会落在哪一栏？");
Console.WriteLine("         （这正是本题最常见的「能过用例、但量级难看」的写法；判题器②那条「最近」在拦什么？）");
Console.WriteLine();

Console.WriteLine("════ 阶梯区：左斜链的递归深度（⚠️ 这一区可能让进程**直接消失**）════");
Console.WriteLine("   递归版在这里的深度 = 树高 = n ⇒ 每翻一倍，栈就多啃一倍。");
Console.WriteLine("   ⚠️ 若下面某一档之后**再无输出**，那不是报错，是栈溢出（.NET 抓不住，进程当场死）。");
Console.WriteLine();
int[] steps = { 1000, 2000, 4000, 8000, 16000, 32000 };
foreach (int n in steps)
{
    Console.WriteLine($"   → 左斜链 n = {n,6}，p = 链尾（值 1）、q = 它的父（值 2）…");
    Console.Out.Flush();
    TreeNode chain = BuildLeftChain(n);
    TreeNode? p = FindByVal(chain, 1);
    TreeNode? q = FindByVal(chain, 2);
    (TreeNode? node, string status) r = TryLca(sol, chain, p, q);
    Console.WriteLine($"     结果 = {(r.node is null ? r.status : r.node.val.ToString())}（期望 2）"
        + $"{(r.node is not null && r.node.val == 2 ? " ✅" : " ❌")}");
    Console.Out.Flush();
}
Console.WriteLine("   ⭐ 尺子怎么读：**哪一档之后没有下一行输出，那一档就是栈顶**（默认线程栈 ≈ 1MB）。");
Console.WriteLine("      （承 108 / 230：一棵**完全合法的树**会先于算法要你的命 —— 除非你的写法不递归。）");
Console.WriteLine();

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
    // TODO: 返回 p 和 q 的**最近公共祖先**（题目保证 p、q 都在树中、节点值互不相同、p ≠ q）。
    //
    //   ── 抓题眼（先回答这三问，再落笔）：
    //      ① 把"最近公共祖先"用**一句话**说准 ——
    //         （提示：把 root→p 的路和 root→q 的路都画出来，这两条路的关系是什么？）
    //      ② ⭐ 站在一个节点上，你怎么知道"答案在我左子树 / 右子树 / 就是我自己"？
    //         ⇒ 你**必须从孩子那里带回来什么信息**，才够做这个判断？
    //         （🪞 镜子：110 的 `(bool ok, int height)` —— 返回值要装几样东西，由"上层要拿它做什么"决定）
    //      ③ ⭐ 返回值类型是 `TreeNode?` —— "我这棵子树里**没有** p 也没有 q" 怎么表达？
    //         "只找到 p" 又怎么表达？（承 98 的 `int?` 哨兵、230 的 `bool isfound`）
    //         ⚠️ 想清楚：**同一个返回值，可以同时表示"找到了谁"和"什么都没找到"吗？**
    //
    //   ── 落笔前必须回答的五个问题：
    //      ① 同一棵树值互不相同 ⇒ 你能靠 `val` 认人吗？判题器②的「引用同一性」在拦什么写法？
    //      ② 示例 2（答案是 p 自己）时，你的写法要不要特判？为什么？
    //      ③ p 是 q 的祖先 / q 是 p 的祖先，这两种情况下你的递归在第几层就返回了？
    //      ④ ⚠️ "最近"这两个字，在你的代码里**由哪一行保证**？
    //         （如果一行都没有 —— 那么它是被你"顺带"满足的，还是被你"运气"满足的？）
    //      ⑤ 左斜链 n = 32000 时，你的递归深度是多少？（阶梯区会替你量）
    //
    //   ── 写完自己回答（这才是今天的账）：
    //      · 复杂度：时间 O(?)、空间 O(?)；为什么本题**一次遍历**就够？
    //      · 你的返回值一共有几种取值？每种取值对上层意味着什么？（画一张小表）
    //      · 性能区第 ③ 问：什么写法会退化成 O(n·h)？它在哪一栏露馅？
    //
    //   ── 验收标准：
    //      ① ⓪-A / ⓪-B 全绿（尺子可信 + 用例合法）；
    //      ② 判题器① 13/13 全绿；
    //      ③ 判题器② 六项全绿（同一性 / 是祖先 / 最近 / 没改树 / 幂等 / 直译裁判）；
    //      ④ 性能区能一句话解释你的量级；阶梯区能说出自己的栈顶在哪一档（或"我不递归"）。
    //
    //   ── 进阶（做完一条再说）：
    //      ① 不递归：先记录 root→p 与 root→q 两条路径，再找**最后一个公共节点**
    //         （🪞 镜子：82 的"两条链一起走"、141 的快慢指针；空间换成 O(h)）
    //      ② 父指针 + 深度对齐（"两个链表找交点"，承 206 / 82 那套比较思路）
    //      ③ 同族变体：BST 的 LCA（承 98 / 700 / 108 那条线 —— 有 BST 性质就能 O(h) 直接走，
    //         一句话说清"性质换来了什么"）
    public TreeNode? LowestCommonAncestor(TreeNode? root, TreeNode? p, TreeNode? q)
    {
        return Walk(root,p,q);
    }
    TreeNode? Walk(TreeNode? root, TreeNode? p, TreeNode? q)
    {
        if(root == null) return null;
        if(root == p || root == q) return root;
        var left = Walk(root.left,p,q);
        var right = Walk(root.right,p,q);
        if(left != null && right != null) return root;
        if(left != null) return left;
        if(right != null) return right;
        return null;
    }
}
