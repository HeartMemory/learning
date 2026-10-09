using System.Diagnostics;
// ═══════ 建树与序列化工具（144 / 94 / 145 / 102 / 104 / 226 / 101 / 110 / 199 / 98 同一套，第十一次复用）═══════
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

int Height(TreeNode? node) => node is null ? 0 : 1 + Math.Max(Height(node.left), Height(node.right));

// ═══════ 数据前提自检：表里的树必须真的是【合法 BST】═══════
//   700 的题眼（"每比一次砍掉一整支"）完全建立在"输入是一棵 BST"之上。
//   ⇒ 表里一旦混进一棵**非法 BST**，我就是在【错误的题目】上练手，练成了也是假的。
//   判据：中序序列【严格递增】（照 98 的乙法 · 显式栈版逐字直译）。
bool IsReallyBst(TreeNode? root)
{
    if (root is null) return true;
    Stack<TreeNode> stack = new Stack<TreeNode>();
    TreeNode? cur = root;
    int? last = null;
    while (cur is not null || stack.Count > 0)
    {
        while (cur is not null) { stack.Push(cur); cur = cur.left; }
        cur = stack.Pop();
        if (last is int prev && cur.val <= prev) return false;
        last = cur.val;
        cur = cur.right;
    }
    return true;
}

// ═══════ 独立裁判（把【题目定义】逐字直译）═══════
//   定义（照题目原话直译）：找出树中【值等于 val】的那个节点，返回它（以它为根的子树）；没有 ⇒ null。
//   ⚠️ 它是【尺子】，不是答案 —— 定义里**一个字都没提"比较大小"** ⇒ 于是这条裁判**故意不用 BST 性质**，
//      整棵树一个一个翻过去。代价 = O(n)：每个节点都被摸一次。
//      ⇒ 它顺便就是"**不用前提的下下策**"的化身（性能裁判里拿它当参照）。
int naiveVisits = 0;

TreeNode? FindByDefinition(TreeNode? node, int val)
{
    naiveVisits++;
    if (node is null) return null;
    if (node.val == val) return node;
    return FindByDefinition(node.left, val) ?? FindByDefinition(node.right, val);
}

// ═══════ 判题器⓪：前提 + 裁判自检（**完全不碰 Solution**）═══════
//   ⭐ 先证明"题目前提成立 + 尺子本身是准的"，再去量答案 —— 用歪尺子量人，写对了也会显示 ❌。
//      本区必须在 Solution 还没写的时候就能跑通。
string WantOf(int?[]? expected) => expected is null ? "[空树]" : Show(Build(expected));

void SelfCheck(string title, int?[] level, int val, int?[]? expected)
{
    TreeNode? root = Build(level);
    bool bstOk = IsReallyBst(root);
    naiveVisits = 0;
    TreeNode? found = FindByDefinition(root, val);
    string got = Show(found);
    string want = WantOf(expected);
    bool ok = bstOk && got == want;
    Console.WriteLine($"{(ok ? "✅" : "❌")} [前提+裁判自检] {title}");
    if (!bstOk)
        Console.WriteLine("     ⛔ 前提不成立：这棵用例树**不是**合法 BST！先修用例表，别急着看答案。");
    else if (got != want)
        Console.WriteLine($"     输入：{Show(root)}｜找 {val}｜裁判给：{got}｜我写的期望：{want}   ← 不一致，先查是谁错");
}

// ═══════ 判题器①：期望值比对（量 Solution）═══════
//   ⏳ Solution 还没写时（抛 NotImplementedException）本区自动跳过 —— 于是"尺子校准"能先单独跑通。
TreeNode? TrySolve(TreeNode? root, int val, out bool ready)
{
    ready = true;
    try { return new Solution().SearchBST(root, val); }
    catch (NotImplementedException) { ready = false; return null; }
}

void Check(string title, int?[] level, int val, int?[]? expected)
{
    TreeNode? root = Build(level);
    TreeNode? actual = TrySolve(root, val, out bool ready);
    if (!ready) { Console.WriteLine($"⏳ {title}  → Solution 尚未实现，跳过"); return; }

    string got = Show(actual);
    string want = WantOf(expected);
    bool ok = got == want;
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok) Console.WriteLine($"     输入：{Show(root)}｜找 {val}｜期望：{want}｜实际：{got}");
}

// ═══════ 判题器②：与【独立裁判】互证（不看期望值，两种实现对答案）═══════
//   用途：万一表里的期望值被我抄错，①和②会同时绿（读的是同一张表）——
//         但②比的是"两种实现"，所以它多抓一类错：**期望值抄错了、而我的实现"很配合地"也错在同一个地方**。
void CrossCheck(string title, int?[] level, int val)
{
    TreeNode? root = Build(level);
    TreeNode? actual = TrySolve(root, val, out bool ready);
    if (!ready) return;

    TreeNode? judge = FindByDefinition(root, val);
    string mine = Show(actual);
    string judgeStr = Show(judge);
    bool ok = mine == judgeStr;
    Console.WriteLine($"{(ok ? "✅" : "❌")} [与裁判互证] {title}");
    if (!ok) Console.WriteLine($"     输入：{Show(root)}｜找 {val}｜裁判：{judgeStr}｜我的：{mine}");
}

// ═══════ 判题器③：本意观察（**不计分**，只打印）═══════
//   题面说"返回以该节点为根的【子树】" ⇒ 本意是**把树上原本那个节点交出去**，不必新建任何东西。
//   ⚠️ 但力扣官方判题是**按序列化结果比结构**的 ⇒ 返回一个"值一样的新节点"同样能 AC。
//      ⇒ 所以这里**不判错**：🟢 = 原节点（本意）；🟡 = 新节点（能过，但多此一举）。
void Observe(string title, int?[] level, int val)
{
    TreeNode? root = Build(level);
    TreeNode? mine = TrySolve(root, val, out bool ready);
    if (!ready) return;
    TreeNode? judge = FindByDefinition(root, val);
    bool same = ReferenceEquals(mine, judge);
    Console.WriteLine($"   {(same ? "🟢 交出去的是树上原本那个节点" : "🟡 交出去的是新节点（值相同）")} —— {title}");
}

// ═══════ 用例表（⭐ 唯一数据源：上面几套判题器读的是同一张表）═══════
//   表只有一份、多边同读：于是"裁判自检全绿" ⇒ 前提成立 + 期望值可信 ⇒ ① 比对 Solution、
//   ② 两种实现互证，链路才闭合。（Expected 为 null = 期望返回 null，即"树里没这个值"）
(string Title, int?[] Level, int Val, int?[]? Expected)[] cases = new (string, int?[], int, int?[]?)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：`[4,2,7,1,3]` 找 **2** → 返回以 2 为根的那棵子树", new int?[] { 4, 2, 7, 1, 3 }, 2, new int?[] { 2, 1, 3 }),
    ("★ 官方示例 2：`[4,2,7,1,3]` 找 **5** → 树里没这个值 ⇒ **null**", new int?[] { 4, 2, 7, 1, 3 }, 5, null),

    // ── 命中根 / 命中浅层 ──
    ("★ 命中根：找 4 → 原样交出整棵树", new int?[] { 4, 2, 7, 1, 3 }, 4, new int?[] { 4, 2, 7, 1, 3 }),
    ("★ 命中根的右孩子：找 7 → `[7]`（它是个光杆）", new int?[] { 4, 2, 7, 1, 3 }, 7, new int?[] { 7 }),

    // ── 命中叶子（交出去的就是一个"光杆节点"）──
    ("★ 命中左支最深的叶子：找 1", new int?[] { 4, 2, 7, 1, 3 }, 1, new int?[] { 1 }),
    ("★ 命中右支叶子：找 3", new int?[] { 4, 2, 7, 1, 3 }, 3, new int?[] { 3 }),

    // ── 边界：空树 / 单节点 ──
    ("★ 空树找 1 → null（连根都没有）", new int?[] { }, 1, null),
    ("★ 单节点命中：`[1]` 找 1", new int?[] { 1 }, 1, new int?[] { 1 }),
    ("★ 单节点未命中：`[1]` 找 2 → null", new int?[] { 1 }, 2, null),

    // ── ⛔ 方向陷阱：把"小于该走左"写反 ⇒ 往【反支】走，一路扑空 ──
    ("⛔ 方向陷阱：`[10,5,15,3,7,13,20]` 找 **3** —— 它一路都在【左边】，方向写反 ⇒ 返回 null",
        new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 3, new int?[] { 3 }),
    ("⛔ 方向陷阱：同一棵树找 **20** —— 它一路都在【右边】", new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 20, new int?[] { 20 }),
    ("★ 命中中间层（交出去的子树有三层）：同一棵树找 **15**", new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 15, new int?[] { 15, 13, 20 }),

    // ── ⛔ "走到尽头才发现没有"：值落在值域两端之外 / 夹在空档里 ──
    ("⛔ 比全场都小：找 1 → null（一路往左走到头）", new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 1, null),
    ("⛔ 比全场都大：找 100 → null（一路往右走到头）", new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 100, null),
    ("⛔ 夹在空档里：找 6 → null（5 的右孩子是空的）", new int?[] { 10, 5, 15, 3, 7, 13, 20 }, 6, null),

    // ── 满树 15 节点（性能裁判里那条 18 层满树的小兄弟）──
    ("★ 满树找最深左值 1", new int?[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 }, 1, new int?[] { 1 }),
    ("★ 满树找最深右值 15", new int?[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 }, 15, new int?[] { 15 }),
    ("★ 满树找根 8 → 整棵树原样", new int?[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 }, 8,
        new int?[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 }),
    ("⛔ 满树找 16 → null（比全场都大）", new int?[] { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 }, 16, null),

    // ── 两条斜链（⚠️ 这里正解【并不快】—— 见性能裁判形状 B）──
    ("★ 左斜链 `[3,2,null,1]` 找 1（最深）", new int?[] { 3, 2, null, 1 }, 1, new int?[] { 1 }),
    ("★ 左斜链找 2 → `[2,1]`（顺手把左孩子一起交出去）", new int?[] { 3, 2, null, 1 }, 2, new int?[] { 2, 1 }),
    ("⛔ 左斜链找 4 → null（比根还大 ⇒ 根上没有右孩子）", new int?[] { 3, 2, null, 1 }, 4, null),
    ("★ 右斜链 `[1,null,2,null,3]` 找 3（最深）", new int?[] { 1, null, 2, null, 3 }, 3, new int?[] { 3 }),

    // ── int 两端 + 值域含 0（⭐ 抓"拿 0 / int.MinValue 当'没找到'哨兵"的写法）──
    ("★ 找 `int.MinValue`（它当根）→ 单节点", new int?[] { int.MinValue }, int.MinValue, new int?[] { int.MinValue }),
    ("★ `[-2147483648,null,2147483647]` 找最大值 → `[2147483647]`",
        new int?[] { int.MinValue, null, int.MaxValue }, int.MaxValue, new int?[] { int.MaxValue }),
    ("★ 同上找最小值 → 交出整棵树（它带着右孩子）",
        new int?[] { int.MinValue, null, int.MaxValue }, int.MinValue, new int?[] { int.MinValue, null, int.MaxValue }),
    ("★ 值域含 **0**：`[0,-3,9]` 找 0 → 交出整棵树",
        new int?[] { 0, -3, 9 }, 0, new int?[] { 0, -3, 9 }),
    ("⛔ 值域含 0 但找的是 5：`[0,-3,9]` 找 5 → null（0 是【真值】，不是「没人」）",
        new int?[] { 0, -3, 9 }, 5, null),
};

Console.WriteLine("═══ 判题器⓪：前提 + 裁判自检区（证明树真是 BST、尺子真是准的，不碰 Solution）═══");
foreach (var c in cases) SelfCheck(c.Title, c.Level, c.Val, c.Expected);

Console.WriteLine("\n═══ 判题器①：期望值比对（量 Solution）═══");
foreach (var c in cases) Check(c.Title, c.Level, c.Val, c.Expected);

Console.WriteLine("\n═══ 判题器②：与独立裁判互证（不看期望值，两种实现对答案）═══");
foreach (var c in cases) CrossCheck(c.Title, c.Level, c.Val);

Console.WriteLine("\n═══ 判题器③：本意观察（不计分）═══");
Observe("官方示例 1（找 2）", new int?[] { 4, 2, 7, 1, 3 }, 2);

// ═══════ 性能裁判：把"每比一次砍一支"变成看得见的数字 ═══════
//   判据：正解只该走【一条路】= h + 1 个节点；暴力裁判要走【全树】= n 个节点。
int fullDepth = 18;
int filled = 0;
TreeNode? BuildFullBst(int depth)                            // 中序递增填充 ⇒ 建出来的一定是合法 BST
{
    if (depth == 0) return null;
    TreeNode node = new TreeNode();
    node.left = BuildFullBst(depth - 1);
    node.val = ++filled;
    node.right = BuildFullBst(depth - 1);
    return node;
}
TreeNode? bigFull = BuildFullBst(fullDepth);
int fullN = CountNodes(bigFull);                             // 2^18 − 1 = 262,143

int chainN = 2000;
int?[] leftChain = new int?[2 * chainN - 1];                 // 左斜链的层序写法（递减：n → … → 1）
leftChain[0] = chainN;
for (int idx = 1; idx < leftChain.Length; idx++)
    leftChain[idx] = (idx % 2 == 1) ? chainN - (idx + 1) / 2 : null;
TreeNode? bigChain = Build(leftChain);

void PerfJudge(string title, TreeNode? tree, int val)
{
    naiveVisits = 0;
    Stopwatch swJudge = Stopwatch.StartNew();
    TreeNode? judgeAnswer = FindByDefinition(tree, val);
    swJudge.Stop();

    TreeNode? solAnswer = null;
    bool ready = false;
    long solMs = -1;
    try
    {
        Stopwatch swSol = Stopwatch.StartNew();
        solAnswer = new Solution().SearchBST(tree, val);
        swSol.Stop();
        solMs = swSol.ElapsedMilliseconds;
        ready = true;
    }
    catch (NotImplementedException) { }

    Console.WriteLine($"\n═══ 性能裁判 · {title} ═══");
    Console.WriteLine($"   h = {Height(tree)}｜n = {CountNodes(tree):N0}｜裁判被调用 {naiveVisits:N0} 次（含空孩子槽）｜{swJudge.ElapsedMilliseconds} ms");
    if (!ready)
        Console.WriteLine("   你的 Solution：⏳ 尚未实现");
    else
        Console.WriteLine($"   你的 Solution：{solMs} ms（答案与裁判一致：{(Show(solAnswer) == Show(judgeAnswer) ? "✅" : "❌")}）");
}

PerfJudge($"形状 A：完美满树 {fullDepth} 层（n = {fullN:N0}，找【最大值】—— 它排在最后才被摸到）", bigFull, fullN);
PerfJudge($"形状 B：{chainN} 节点【左斜链】（找【最深处】的值 1 —— 树高 h = n）", bigChain, 1);

Console.WriteLine("\n判据：① 满树上正解只走 h + 1 ≈ 19 步，暴力裁判要摸遍整棵树（n 个节点 + 空槽 ≈ 52 万次调用）⇒ 差上万倍：\"砍一支\"是真的在省。");
Console.WriteLine("      ② 斜链上正解也要走 h = n 步 ⇒ **和暴力一样慢** —— O(h) 里的 h 是【树高】不是节点数；");
Console.WriteLine("         树一旦歪成一条链，BST 查找就退化成线性 ⇒ 这就是\"平衡树\"存在的理由（记一笔，后面会回来）。");

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
    // TODO: 在 BST 里找【值等于 val】的节点，返回以它为根的子树；找不到返回 null。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      BST 的全部价值就是"**每比一次，砍掉一整支**" —— 每往下一层，待查范围就少一半。
    //      ⇒ 所以正解只该走【一条】从根往下的路径，长度 = 树高 h，别的节点【一眼都不看】。
    //
    //   ── 两条正路（先走通一条，别同时写）：
    //      甲、**迭代 while**：一个游标顺着走，边走边砍掉一半 —— 空间 O(1)。
    //      乙、**递归**：把"该往哪边走"写成递归调用 —— 空间 O(h)（每层一个栈帧）。
    //          ⭐ 判据（承 101 / 98）：**子问题的形状，主函数签名自己就问得出来**吗？
    //             问得出来 ⇒ **不必开 helper**。
    //
    //   ── 落笔前必须回答的五个问题：
    //      ① 走到 `cur.val == val` 时，返回的到底是什么？（`val`？新造的节点？还是 `cur` 本身？）
    //      ② `val < cur.val` 该走哪边？写反了会怎样 —— 用「方向陷阱」那两条用例（找 3 / 找 20）验一下。
    //      ③ 游标变成 `null` 说明什么？那时该返回什么？
    //      ④ 终止条件一共几个？能不能像 104 那样"一行盖住多个入口"？
    //      ⑤ 甲、乙两版，谁的空间是 O(1)？为什么？（提示：C# 不保证尾调用优化）
    //
    //   ── 自问自答（写完再回来看）：
    //      · 为什么"每比一次就能砍掉一整支"？用 98 那天的说法讲：**每个节点都背着一个值域区间**——
    //        你站在 `cur` 上，`val` 和 `cur.val` 一比，就知道它落在哪半边。
    //      · ⛔ 有个"不用 BST 性质、整棵树都翻"的写法 —— 它**能过上面全部用例**（结果一样），
    //        但它在性能裁判里慢得离谱。这说明什么？（⭐ 判据：**用例表只能查"对不对"，
    //        查不出"有没有用上前提"** —— 后者要靠性能裁判。）
    //      · ⭐ 700 的 O(h) 里 h 是【树高】不是节点数 n —— 两条斜链上 h = n ⇒ 正解退化成 O(n)，
    //        和暴力一样慢。⇒ 这就是"平衡树"存在的理由（记一笔，后面回来）。
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      甲：时间 O(?)、空间 O(?)      乙：时间 O(?)、空间 O(?)
    //      ⭐ 下面的「性能裁判」两种形状各跑一次 —— 算完拿它验一验你的判断。
    //
    //   ── 验收标准：
    //      ① 用例表全绿（含 空树 / 命中根 / 命中叶 / **方向陷阱** / 值域两端之外 / 空档 / int 两端 / 值域含 0 / 两条斜链）；
    //      ② 前提自检 + 裁判自检全绿（证明表里的树真是 BST、期望值可信）；
    //      ③ 判题器②「与裁判互证」逐项一致；
    //      ④ 能一句话答出："**为什么每比一次就能砍掉一整支？**"（答不上来 = 没抓住题眼）
    //
    //   ── 进阶（做完一条再说，别今天硬塞）：
    //      ① 另一条路也写一版 → 在下面加一区"双版互证"：同一份 `cases`，两版都必须等于期望值
    //         （照 101 / 110 / 98 那套"两种实现互证"）。
    //      ② ⭐ 变体（面试常追问）：不返回子树，改成返回【从根到该节点的整条路径】——
    //         思路同源，但要把"走过的路"记下来。想想记在哪、记什么。
    public TreeNode? SearchBST(TreeNode? root, int val)
    {
        if(root == null) return null;
        Queue<TreeNode> trees = new Queue<TreeNode>();
        trees.Enqueue(root);
        while(trees.Count > 0)
        {
            TreeNode cur = trees.Dequeue();
            int temp = cur.val;
            if(temp == val) return cur;
            if(temp > val && cur.left != null) trees.Enqueue(cur.left);
            if(temp < val && cur.right != null) trees.Enqueue(cur.right);
        }
        return null;
    }
}
