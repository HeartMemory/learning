using System.Diagnostics;

// ═══════ 建树与序列化工具（144 / 94 / 145 / 102 / 104 / 226 / 101 同一套，第八次复用）═══════
static TreeNode? Build(int?[] level)
{
    if (level.Length == 0 || level[0] is null) return null;
    TreeNode root = new TreeNode(level[0]!.Value);
    Queue<TreeNode> q = new Queue<TreeNode>();
    q.Enqueue(root);
    int i = 1;
    while (q.Count > 0 && i < level.Length)
    {
        TreeNode cur = q.Dequeue();
        if (i < level.Length && level[i] is int lv) { cur.left = new TreeNode(lv); q.Enqueue(cur.left); }
        i++;
        if (i < level.Length && level[i] is int rv) { cur.right = new TreeNode(rv); q.Enqueue(cur.right); }
        i++;
    }
    return root;
}

static string Show(TreeNode? root)
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

static int CountNodes(TreeNode? root)
{
    if (root is null) return 0;
    return 1 + CountNodes(root.left) + CountNodes(root.right);
}

// ═══════ 计数器：把"重复计算"变成看得见的数字 ═══════
//   ⭐ 今天的题眼是【代价】，不是"能不能算对"。所以裁判不光要答对，还得**数清自己碰了多少个节点**。
int heightCalls = 0;

// ═══════ 独立裁判（朴素直译）：平衡的【定义】= 每一个节点都满足 |h(左) − h(右)| ≤ 1 ═══════
//   ⚠️ 它是【尺子】，不是答案 —— 定义逐字翻译，慢得离谱（斜链上要做 ~n²/2 次高度递归）。
//      它的价值有两处：① 与我的期望值零共享，能替我抓"答案抄错"；② 它自带产量表（heightCalls）。
int Height(TreeNode? node)
{
    heightCalls++;
    if (node is null) return 0;
    return 1 + Math.Max(Height(node.left), Height(node.right));
}

bool BalancedByDefinition(TreeNode? root)
{
    if (root is null) return true;
    if (Math.Abs(Height(root.left) - Height(root.right)) > 1) return false;
    return BalancedByDefinition(root.left) && BalancedByDefinition(root.right);
}

// ═══════ 判题器⓪：裁判自检（**完全不碰 Solution**）═══════
//   ⭐ 先证明"我的尺子本身是准的"，再去量答案。
//      理由：如果我用一把歪尺子量你，你写对了也会显示 ❌ —— 那种"错"是尺子的错。
//      所以本区必须在 Solution 还没写的时候就能跑（也就能单独验证期望值）。
void SelfCheck(string title, int?[] level, bool expected)
{
    heightCalls = 0;
    TreeNode? root = Build(level);
    bool got = BalancedByDefinition(root);
    bool ok = got == expected;
    Console.WriteLine($"{(ok ? "✅" : "❌")} [裁判自检] {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：   {Show(root)}");
        Console.WriteLine($"     裁判答： {got}   我写的期望值：{expected}   ← 两者不一致，先查是谁错");
    }
    else
    {
        Console.WriteLine($"     n = {CountNodes(root)}｜朴素裁判做了 {heightCalls} 次高度递归");
    }
}

// ═══════ 判题器①：期望值比对（量 Solution）═══════
void Check(string title, int?[] level, bool expected)
{
    TreeNode? root = Build(level);
    bool actual = new Solution().IsBalanced(root);
    bool ok = actual == expected;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：{Show(root)}");
        Console.WriteLine($"     期望：{expected}   实际：{actual}");
    }
}

// ═══════ 用例表（⭐ 唯一数据源：上面两套判题器读的是同一张表）═══════
//   为什么不写两遍：抄一遍期望值就多一次抄错的机会。表只有一份，两边同时读 ——
//   于是"裁判自检全绿" ⇒ 表里的期望值是对的 ⇒ 判题器①再比对 Solution，链路才是闭合的。
(string Title, int?[] Level, bool Expected)[] cases = new (string, int?[], bool)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：三层，右支比左支深一层（差 1 = 刚好合规）", new int?[] { 3, 9, 20, null, null, 15, 7 }, true),
    ("★ 官方示例 2：最深处只差一个叶子（那个 4,4 那一层）→ 崩", new int?[] { 1, 2, 2, 3, 3, null, null, 4, 4 }, false),

    // ── 边界：空 / 单点 / 只有一支 ──
    ("★ 空树 → 平衡（终止条件的第一道门）", new int?[] { }, true),
    ("★ 单节点 → 天然平衡", new int?[] { 1 }, true),
    ("★ 两个节点，左支深 1（差 1 = 合规的极限）", new int?[] { 1, 2 }, true),
    ("★ 只有右孩子（差 1，同样合规）", new int?[] { 1, null, 2 }, true),

    // ── 左斜链 / 右斜链：既测答案，也测"重复计算"的规模 ──
    ("★★ 左斜链 4 层 → 根上就差 2", new int?[] { 1, 2, null, 3, null, 4 }, false),
    ("★★ 右斜链 4 层 → 方向反过来照样崩", new int?[] { 1, null, 2, null, 3, null, 4 }, false),

    // ── 陷阱组：一眼看过去"挺平衡"，其实不是 ──
    ("⛔ 陷阱 A：根上左右差 1（合规），但**左孩子自己**的左右差 2 → 只看根会假绿",
        new int?[] { 1, 2, 2, 3, null, null, 3, 4 }, false),
    ("⛔ 陷阱 B：两棵子树**各自都平衡**，合到根上差 2 → 「局部都平衡」推不出「整体平衡」",
        new int?[] { 1, 2, 2, 3, 4, null, null, 5, 6 }, false),

    // ── 两支等深、孩子挂的方向相反（承 101 的"交叉成对"：这类形状最容易骗眼睛）──
    ("★★ 左右两支等深、孩子挂的方向相反 → 平衡", new int?[] { 1, 2, 2, 3, null, null, 3 }, true),
    ("★★ 完美满树 4 层 → 处处差 0，平衡", new int?[] { 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4 }, true),
};

Console.WriteLine("═══ 判题器⓪：裁判自检区（证明尺子是准的，不碰 Solution）═══");
foreach (var c in cases) SelfCheck(c.Title, c.Level, c.Expected);

Console.WriteLine("\n═══ 判题器①：期望值比对（量 Solution）═══");
foreach (var c in cases) Check(c.Title, c.Level, c.Expected);

// ═══════ 性能裁判：让"重复计算"的代价变成看得见的数字 ═══════
//   ⚠️ 先拆掉一个【实测会打脸】的直觉：**"斜链 = O(n²)"是错的**。
//      第一版注释里我写"2000 节点链要 200 万次"，实测只有 4,000 次 —— 因为链的根上左右高度
//      就差了 1999，朴素裁判**一眼就崩、立刻返回**，只付了一次全树高度。链不是朴素法的软肋！
//   ⭐ 真判据：朴素法 = "它访问到的**每个**节点，都要重算一次自己子树的高度"
//        代价 ≈ Σ(子树大小)，而**它访问多少节点，取决于"在哪里发现答案"**：
//          · 形状 A 斜链（答案 false）：根上就发现 → 只付一次 → 便宜
//          · 形状 B 完美满树（答案 true）：每个节点都得查 → Σ 子树大小 ≈ n·h → 最亏
//      所以本区**两种形状各测一次**，别只盯着一个数。
//   次数是硬证据（裁判自己会数）；你的 Solution 没法被外部计数 → 只看是否与裁判**同一个量级**。
//   🔧 想亲眼看见"重算版"到底多慢：把 `IsBalanced` 临时改成 `return BalancedByDefinition(root);` 再跑本区。
int chainNodes = 2000;
int?[] skewed = new int?[2 * chainNodes - 1];
skewed[0] = 1;
for (int idx = 1; idx < skewed.Length; idx++) skewed[idx] = (idx % 2 == 1) ? 2 : null;
TreeNode? bigChain = Build(skewed);

int levels = 18;                                        // 完美满树：n = 2^18 − 1 = 262,143
int?[] full = new int?[(1 << levels) - 1];
for (int idx = 0; idx < full.Length; idx++) full[idx] = 1;
TreeNode? bigFull = Build(full);

long lastJudgeCalls = 0, lastJudgeMs = 0, lastSolMs = 0;

void PerfJudge(string title, TreeNode? tree, bool expected)
{
    heightCalls = 0;
    var swJudge = Stopwatch.StartNew();
    bool judgeAnswer = BalancedByDefinition(tree);
    swJudge.Stop();

    var swSolution = Stopwatch.StartNew();
    bool solutionAnswer = new Solution().IsBalanced(tree);
    swSolution.Stop();

    lastJudgeCalls = heightCalls;
    lastJudgeMs = swJudge.ElapsedMilliseconds;
    lastSolMs = swSolution.ElapsedMilliseconds;

    Console.WriteLine($"\n═══ 性能裁判 · {title}（期望 {(expected ? "true" : "false")}）═══");
    Console.WriteLine($"   裁判（朴素直译）：{judgeAnswer}｜高度递归 {lastJudgeCalls:N0} 次｜{lastJudgeMs} ms");
    Console.WriteLine($"   你的 Solution  ：{solutionAnswer}｜{lastSolMs} ms");
}

PerfJudge($"形状 A：{chainNodes} 节点左斜链（根上一眼就崩 → 朴素反而便宜）", bigChain, false);
PerfJudge($"形状 B：完美满树 {levels} 层 / {full.Length:N0} 节点（处处平衡 → 朴素必须访问所有节点）", bigFull, true);

Console.WriteLine($"\n判据：形状 B 上朴素法 = 「每个节点 × 自己子树里的全部节点」→ 实测 {lastJudgeCalls:N0} 次；");
Console.WriteLine($"      ≈ 2·n·h（n = {full.Length:N0}，h = {levels}；每个子树的空孩子也要算一次）。");
Console.WriteLine($"      而「返回值带高度」每节点只碰一次 ≈ 2n ≈ {2L * full.Length:N0}；实测 {lastJudgeMs} ms vs {lastSolMs} ms。");
Console.WriteLine($"      ⚠️ 形状 A 上朴素反而快（只付了一次全树高度）—— 快慢取决于「它在哪里发现答案」，不是背复杂度标签。");

// ═══════ 类型与答题区 ═══════
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
    // TODO: 判断二叉树是否【每一个节点】都平衡 —— 平衡返回 true，否则 false。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      平衡的定义是"**每一个节点**都满足 |h(左) − h(右)| ≤ 1"，不是只看根。
    //      ⭐ 所以第一个问题不是"怎么写"，而是"**递归要往上递什么**"：
    //          · 只递 bool（平衡 / 不平衡）→ 不够：判"我这一层"还需要两个子树的高度，
    //            而高度只能靠**再递归问一次** —— 这就是裁判那套笨办法：同一棵子树的高度被反复重算。
    //          · 递"高度 + 平衡"两样东西 → 每个节点只被访问一次。
    //
    //   ── 落笔前必须回答的三个问题：
    //      ① 返回值装几样东西？两样（如 (bool ok, int h) 元组）？还是一样 + 哨兵（int，-1 = 已崩）？
    //      ② 用 -1 当哨兵时，凭什么保证"真高度"永远不会等于 -1？
    //         （想不清这条，哨兵就会把"合法的空子树"和"已崩的子树"混成一个信号。）
    //      ③ 计算顺序是"孩子先、父后"（后序）还是别的？为什么必须排成这个顺序？
    //
    //   ── 自问自答（写完再回来看）：
    //      · 为什么"对每个节点都调用一次高度函数"会退化到 O(n²)？画一条斜链，数一数根那一次要碰几个节点。
    //      · 提前发现不平衡时，结果能不能**一路短路**传回根？哨兵法是否天然自带这个能力？
    //      · ⭐ 与 104 的血缘：如果只写"返回高度"，和 104 的 MaxDepth 差几行？（今天多出来的是"哪里该喊停"）
    //      · 如果题目改成"返回**最深**的那个不平衡节点"，返回值要再换吗？
    //
    //   ── 复杂度（自己算一遍）：
    //      时间 O(?)、空间 O(?)（空间 = 递归栈深 = 树高 → 最坏是哪种形状？）
    //      ⭐ 用下面的「性能裁判」实测：2000 节点斜链，裁判要 ~200 万次高度递归，你的实现该是几次？
    //
    //   ── 验收标准：
    //      ① 用例表全绿（含空树 / 左右斜链 / "根平衡但深处崩" / "两棵子树各自平衡但合起来差 2"）；
    //      ② 裁判自检区全绿（尺子准 ⇒ 表里的期望值可信）；
    //      ③ 性能裁判：与裁判不是一个量级（若耗时同量级 → 你写的还是单节点重算高度那套）；
    //      ④ 能一句话说清"为什么返回值必须同时携带高度"。
    //
    //   ── 进阶（做完递归版再说，别今天硬塞）：
    //      ① 迭代版 `IsBalanced1`：显式栈后序 + 一张"节点 → 高度"的表（承 10-04 的便条法 / 145 路线 A）。
    //         写完在下面加一区"双版互证"：同一份 `cases`，两版都必须等于期望值。
    //      ② 自顶向下 + 记忆化（Dictionary 缓存高度）—— 它也是 O(n)。说说它与自底向上是两种写法还是同一件事。
    public bool IsBalanced(TreeNode? root)
    {
        return CheckHeight(root).ok;
    }
    (bool ok, int high) CheckHeight(TreeNode? root)
    {
        if(root == null) return (true, 0);
        var left  = CheckHeight(root.left);
        var right = CheckHeight(root.right);
        if(Math.Abs(left.high - right.high) <= 1 && left.ok && right.ok) return (true, 1 + Math.Max(left.high,right.high));
        return (false, 0);
    }
}
