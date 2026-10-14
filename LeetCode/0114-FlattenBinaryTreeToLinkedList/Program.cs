using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════════════════
//  114 · 二叉树展开为链表（原地改指针）
//  脚手架（第十五次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144/94/145/102/104/226/101/110/199/98/700/108/105/106 → 本题）
//
//  ⭐ 与前面 14 站的分界线（本题的"变的是哪里"全在这里，落笔前先读一遍）：
//
//   ① 【动作从「造」变成「改」】
//      105 / 106 是 `new TreeNode` 把树造出来；本题**一个新节点都不许造** ——
//      改的是既有节点的 left / right。
//      ⇒ 判题器因此多出一条前面从没用过的独立裁判：
//         「展开后那批节点，还是不是**原来那批对象**」（引用同一性）。
//
//   ② 【输出不是返回值，而是 root 的内部形态】
//      签名是 `void Flatten(TreeNode root)` —— 没有返回值可比对、也没有新根可拿。
//      ⇒ 判据只能从「比对返回值」换成「检查形态」：
//         所有 left == null ＋ 沿 right 一路读回来 == 原树的前序。
//
//   ③ 【⚠️ 判题器自己要先改一条纪律：每次重新造一棵树】
//      本题的解法是**就地修改** ⇒ 跑一次就把输入树毁了。
//      若像 106 那样复用同一组 `int[]`（值类型、天然可重复用），
//      第二个判题器拿到的会是一棵**已经被展开的树** ⇒ 全是假绿。
//      ⇒ 用例表里存的是**建树函数**（不是实例、也不是层序字符串），每次判题现场造一棵新的。
//      ⇒ 性能区还要多做一件 106 不用做的事：**扣掉"造树"的基线成本**。
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十五次复用） ─────────────────────────

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

// 反序列化：把 Show 出来的字符串重新读回一棵树（尺子自检 + 用例建树要用）
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

// 前序回读 —— 判题器② 的"独立裁判"要用（它是题目定义逐字直译）
// ⚠️ 本题要回读的是**前序**（不是中序/后序 —— 别照抄 106）
List<int> Preorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n) { if (n is null) return; sink.Add(n.val); Walk(n.left); Walk(n.right); }
    Walk(root);
    return sink;
}

// ⭐ 本题新工具①：收集一棵树里的**所有节点对象**
//    HashSet 的默认比较器对引用类型就是**引用相等** ⇒ 正好拿来问"还是原来那个对象吗"
HashSet<TreeNode> CollectRefs(TreeNode? root)
{
    HashSet<TreeNode> set = new HashSet<TreeNode>();
    Stack<TreeNode> stack = new Stack<TreeNode>();
    if (root is not null) stack.Push(root);
    while (stack.Count > 0)
    {
        TreeNode cur = stack.Pop();
        set.Add(cur);
        if (cur.left is not null) stack.Push(cur.left);
        if (cur.right is not null) stack.Push(cur.right);
    }
    return set;
}

// ⭐ 本题新工具②：沿 right 一路读下去（`cap` = 步数上限 ⇒ 顺带把"环"抓出来）
(List<int> vals, bool hasCycle) ChainValues(TreeNode? node, int cap)
{
    List<int> vals = new List<int>();
    TreeNode? cur = node;
    while (cur is not null)
    {
        if (vals.Count > cap) return (vals, true);
        vals.Add(cur.val);
        cur = cur.right;
    }
    return (vals, false);
}

// ⭐ 本题新工具③：整棵树里所有 left 是否都空了
bool AllLeftNull(TreeNode? root)
{
    Stack<TreeNode> stack = new Stack<TreeNode>();
    if (root is not null) stack.Push(root);
    while (stack.Count > 0)
    {
        TreeNode cur = stack.Pop();
        if (cur.left is not null) return false;
        if (cur.right is not null) stack.Push(cur.right);
    }
    return true;
}

string Norm(string s) => s.Replace(" ", "");

// 斜链造树：值 1..n 一条链，toLeft = true 是左斜链、false 是右斜链
// ⚠️ 这是个**造树函数**（不是数组）：每调一次造一棵新树 —— 因为本题的解法会把它改掉
TreeNode? BuildChain(int n, bool toLeft)
{
    TreeNode? head = null;
    for (int i = n; i >= 1; i--)
    {
        TreeNode node = new TreeNode(i);
        if (toLeft) node.left = head; else node.right = head;
        head = node;
    }
    return head;
}

// ───────────────── 判题器⓪-A：尺子自检（期望值必须先证明它对 —— 不碰 Solution） ─────────────────

// 判据（两条都要满足，期望值才配当尺子）：
//   ① 把期望层序【反序列化】之后，它必须**本身就是一条右链**（left 全空）；
//   ② 沿它的 right 读出来，必须与「输入树的前序」一字不差。
bool ExpectedSelfCheck(string expected, Func<TreeNode?> make)
{
    TreeNode? exp = Parse(expected);
    if (!AllLeftNull(exp)) return false;
    var (vals, cycle) = ChainValues(exp, CountNodes(exp) + 5);
    if (cycle) return false;
    return vals.SequenceEqual(Preorder(make()));
}

// ───────────────── 判题器⓪-B：判题器前提自检（查的是"判题器自己"） ─────────────────

// 本题特有：前 14 站要查的是"用例对不对"，本题还要多查一条"判题器能不能被信任"：
//   ① **建树函数每次必须返回新对象** —— 就地修改的解法不能被喂同一棵树两次（否则第二个判题器拿到的是已展开的树 ⇒ 假绿）
//   ② 用例若给了期望链：链长必须等于该用例的节点数（尺子与输入对齐）
//   ③ 期望链必须自身是条右链（left 全空）
List<string> PremiseFaults(string title, Func<TreeNode?> make, string? expected)
{
    List<string> bad = new List<string>();
    TreeNode? a = make();
    TreeNode? b = make();
    // ⚠️ 空树用例（`[]`）两次都返回 null ⇒ 这不算"同一棵树"，跳过这一条
    if (a is not null && ReferenceEquals(a, b)) bad.Add("建树函数两次返回**同一个对象** ⇒ 就地修改的判题器不能被重复用");
    if (expected is null) return bad;
    TreeNode? exp = Parse(expected);
    if (!AllLeftNull(exp)) bad.Add("期望值自身不是一条右链（left 没清空）");
    if (CountNodes(exp) != CountNodes(a)) bad.Add($"期望链长 {CountNodes(exp)} ≠ 输入节点数 {CountNodes(a)}");
    return bad;
}

// ───────────────── 判题器①：期望值比对（本题展开目标唯一 ⇒ 可以比对） ─────────────────

(bool ready, TreeNode? after, HashSet<TreeNode>? before, List<int>? pre, string? crash) TryFlatten(Func<TreeNode?> make)
{
    TreeNode? root = make();
    HashSet<TreeNode> before = CollectRefs(root);   // ⚠️ 快照必须在展开**之前**取
    List<int> pre = Preorder(root);
    try
    {
        new Solution().Flatten(root);
        return (true, root, before, pre, null);
    }
    catch (NotImplementedException) { return (false, null, null, null, null); }
    catch (Exception ex) { return (false, root, before, pre, $"{ex.GetType().Name}: {ex.Message}"); }
}

// ────────────────────────── 用例表（唯一数据源） ──────────────────────────
// ⚠️ Expected == null 表示"这组不给期望值"（规模太大，手写不划算）⇒ 只走判题器②。
// ⚠️ 第二列是**建树函数**（不是字符串、不是实例）：因为解法会**就地**改掉它。
(string Title, Func<TreeNode?> Make, string? Expected)[] cases = new (string, Func<TreeNode?>, string?)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：[1,2,5,3,4,null,6]", () => Parse("[1,2,5,3,4,null,6]"), "[1,null,2,null,3,null,4,null,5,null,6]"),
    ("★ 官方示例 2：空树 []", () => Parse("[]"), "[空树]"),

    // ── 边界 ──
    ("★ 单节点 [7]", () => Parse("[7]"), "[7]"),

    // ── ⛔ 本题核心陷阱组：左边有货 ⇒ 要"搬" ──
    //    专抓两类错：① 先动指针再存引用（丢半棵树）② 搬完之后忘了清 left
    ("⛔ 只有左孩子 3 节点：[1,2,null,3]", () => Parse("[1,2,null,3]"), "[1,null,2,null,3]"),
    ("⛔ 只有左子树、深 5：[5,4,null,3,null,2,null,1]", () => Parse("[5,4,null,3,null,2,null,1]"), "[5,null,4,null,3,null,2,null,1]"),
    ("⛔ 左子树深、右子树也深（满树 3 层）", () => Parse("[1,2,3,4,5,6,7]"), "[1,null,2,null,4,null,5,null,3,null,6,null,7]"),

    // ── 形态：本来就"不用动"的树（改了反而错） ──
    ("★ 右斜链 3 节点：[1,null,2,null,3]（展开 = 原样）", () => Parse("[1,null,2,null,3]"), "[1,null,2,null,3]"),
    ("★ 右链 4 节点：[1,null,2,null,3,null,4]", () => Parse("[1,null,2,null,3,null,4]"), "[1,null,2,null,3,null,4]"),

    // ── 值域（⚠️ 本题题面**不要求元素互不相同** ⇒ 判据全靠【结构】，不靠值） ──
    ("★ 值域含 0 与负数：[0,-3,9]", () => Parse("[0,-3,9]"), "[0,null,-3,null,9]"),
    ("★ 值重复：三节点全 2", () => Parse("[2,2,2]"), "[2,null,2,null,2]"),
    ("★ 值重复 + 只有左孩子：[3,3,null,3]", () => Parse("[3,3,null,3]"), "[3,null,3,null,3]"),

    // ── 规模（**不给期望值** ⇒ 只走判题器② 的独立裁判）──
    ("★ n = 1000 左斜链（不给期望值，只查性质）", () => BuildChain(1000, true), null),
    ("★ n = 1000 右斜链（不给期望值，只查性质）", () => BuildChain(1000, false), null),
};

// ══════════════════════════════ 开 跑 ══════════════════════════════

Console.WriteLine("═══ 判题器⓪-A：尺子自检（期望值必须先证明它对 —— 不碰 Solution）═══");
Console.WriteLine("   判据：期望层序【反序列化】后 ① 必须本身就是一条右链 ② 沿 right 读出 == 输入树的前序。");
foreach (var c in cases)
{
    if (c.Expected is null) { Console.WriteLine($"➖ [尺子] {c.Title}  → 本组未给期望值，跳过"); continue; }
    bool ok = ExpectedSelfCheck(c.Expected, c.Make);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [尺子] {c.Title}");
    if (!ok) Console.WriteLine($"     ⛔ 期望值 {c.Expected} 与输入不自洽 ⇒ **先改用例表，别急着看你的解**。");
}

Console.WriteLine("\n═══ 判题器⓪-B：判题器前提自检（建树函数每次给新对象 + 尺子与输入对齐）═══");
Console.WriteLine("   ⭐ 为什么查这个：本题的解法会**就地**改掉输入 ⇒ 判题器若能拿到\"同一棵树两次\"，后面全是假绿。");
foreach (var c in cases)
{
    List<string> bad = PremiseFaults(c.Title, c.Make, c.Expected);
    Console.WriteLine($"{(bad.Count == 0 ? "✅" : "❌")} [前提] {c.Title}");
    if (bad.Count > 0) Console.WriteLine($"     ⛔ {string.Join("｜", bad)} ⇒ 先修判题器/用例表。");
}

Console.WriteLine("\n═══ 判题器①：期望值比对（本题展开目标唯一 ⇒ 敢比对）═══");
foreach (var c in cases)
{
    var r = TryFlatten(c.Make);
    // ⚠️ 顺序不能反：crash 也走 ready=false 那条路 ⇒ 先问 crash，否则"抛异常"会被误报成"尚未实现"
    if (r.crash is not null) { Console.WriteLine($"❌ {c.Title}\n     展开**抛异常**：{r.crash}"); continue; }
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    string got = Show(r.after);
    if (c.Expected is null)
    {
        Console.WriteLine($"➖ {c.Title}  〔无期望值 ⇒ 交给判题器②〕  链长：{ChainValues(r.after, CountNodes(r.after) + 5).vals.Count}");
        continue;
    }
    bool ok = Norm(got) == Norm(c.Expected);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {c.Title}");
    if (!ok) Console.WriteLine($"     期望：{c.Expected}\n     拿到：{got}");
}

Console.WriteLine("\n═══ 判题器②：独立裁判 = 题目定义逐字直译（**不看期望值**）═══");
Console.WriteLine("   四条判据：① 就地改（还是原来那批**对象**）② 前序回读 ③ left 全空 ④ 链长 == 节点数（无遗漏/无环）");
Console.WriteLine("   ⭐ 与判题器①是**两条独立的路**：①抄期望值、②回读定义 —— 一条错了另一条能抓。");
foreach (var c in cases)
{
    var r = TryFlatten(c.Make);
    if (r.crash is not null) { Console.WriteLine($"❌ {c.Title}\n     展开**抛异常**：{r.crash}"); continue; }
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    HashSet<TreeNode> afterSet = CollectRefs(r.after);
    bool okSame = afterSet.Count == r.before!.Count && afterSet.IsSubsetOf(r.before!);
    var (vals, cycle) = ChainValues(r.after, r.before!.Count + 5);
    bool okPre = !cycle && vals.SequenceEqual(r.pre!);
    bool okLeft = AllLeftNull(r.after);
    bool okCount = !cycle && vals.Count == r.before!.Count;

    bool all = okSame && okPre && okLeft && okCount;
    Console.WriteLine($"{(all ? "✅" : "❌")} {c.Title}");
    Console.WriteLine($"     就地改(同一批对象) {(okSame ? "✅" : "❌")}｜前序回读 {(okPre ? "✅" : "❌")}"
        + $"｜left 全空 {(okLeft ? "✅" : "❌")}｜链长对 {(okCount ? "✅" : "❌")}{(cycle ? "（⛔ 检测到环！）" : "")}");
}

Console.WriteLine("\n═══ 判题器③：形状观察（**不计错**，只打印）═══");
foreach (var c in cases)
{
    var r = TryFlatten(c.Make);
    if (!r.ready) break;
    if (r.crash is not null) continue;
    int n = r.before!.Count;
    int h = Height(c.Make());          // ⚠️ 高度必须拿**展开前**的树算（展开后 left 全空 ⇒ 那条链的"高度"就是链长，没有意义）
    var (vals, cycle) = ChainValues(r.after, n + 5);
    string head = vals.Count <= 8 ? string.Join(",", vals) : string.Join(",", vals.Take(8)) + ",…";
    Console.WriteLine($"   · n = {n,4}｜原树高 = {h,4}｜链长 = {vals.Count,4}{(cycle ? "（⛔ 环）" : "")}｜链头：[{head}]   —— {c.Title}");
}

// ═══════════ 性能区：把「搬左子树」的代价变成看得见的数字 ═══════════
//  ⚠️ 本题性能区与 106 有个**关键差别**：解法会**就地毁掉输入** ⇒
//     计时里必须含"重新造一棵树"的成本 ⇒ 所以要**同时量两笔**、相减得净值。
//  ⚠️ 左斜链的**递归深度 = n** ⇒ 规模只上到 2000（.NET 的 StackOverflowException 不可捕获，进程当场死）。
double Measure(Func<TreeNode?> make, int repeat)
{
    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < repeat; i++)
    {
        TreeNode? root = make();
        new Solution().Flatten(root);
    }
    sw.Stop();
    return sw.Elapsed.TotalMilliseconds;
}

double Baseline(Func<TreeNode?> make, int repeat)
{
    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < repeat; i++) _ = make();
    sw.Stop();
    return sw.Elapsed.TotalMilliseconds;
}

string ScaleVerdict(double small, double big)
{
    if (small < 1.0 && big < 2.0) return "🔵 两次都太小（< 2 ms）⇒ 看不出斜率，但至少说明这一步**没有**在反复扫长链";
    double ratio = big / Math.Max(small, 0.0001);
    if (ratio < 2.6) return $"🟢 约 **{ratio:F1} 倍** ⇒ 规模翻倍、代价跟着翻倍 ⇒ **线性**";
    if (ratio > 3.4) return $"🟡 约 **{ratio:F1} 倍** ⇒ 规模翻倍、代价翻 **4** 倍 ⇒ **平方**";
    return $"🟠 约 **{ratio:F1} 倍** ⇒ 落在两者之间（小规模计时噪声大，多跑几次看趋势）";
}

Console.WriteLine("\n═══ 性能区：同为 n 的两条斜链（左斜链要\"搬\"、右斜链本来就在右边）═══");
if (!TryFlatten(() => Parse("[1]")).ready)
{
    Console.WriteLine("   ⏳ Solution 尚未实现 ⇒ 性能区跳过。（下面这段判据可以留着写完再看。）");
}
else
{
    const int repeat = 20;
    Func<TreeNode?> L1 = () => BuildChain(1000, true);
    Func<TreeNode?> L2 = () => BuildChain(2000, true);
    Func<TreeNode?> R2 = () => BuildChain(2000, false);

    double bL1 = Baseline(L1, repeat), bL2 = Baseline(L2, repeat), bR2 = Baseline(R2, repeat);
    double fL1 = Measure(L1, repeat), fL2 = Measure(L2, repeat), fR2 = Measure(R2, repeat);
    double nL1 = fL1 - bL1, nL2 = fL2 - bL2, nR2 = fR2 - bR2;

    Console.WriteLine($"   形状      n      只造树(基线)     造树+展开        展开净值（跑 {repeat} 次）");
    Console.WriteLine($"   左斜链   1000   {bL1,10:F2} ms   {fL1,10:F2} ms   {nL1,10:F2} ms   （单次 {(nL1 / repeat),7:F3} ms）");
    Console.WriteLine($"   左斜链   2000   {bL2,10:F2} ms   {fL2,10:F2} ms   {nL2,10:F2} ms   （单次 {(nL2 / repeat),7:F3} ms）");
    Console.WriteLine($"   右斜链   2000   {bR2,10:F2} ms   {fR2,10:F2} ms   {nR2,10:F2} ms   （单次 {(nR2 / repeat),7:F3} ms）");
    Console.WriteLine();
    Console.WriteLine($"   判据 A（同形状、规模翻倍）：左斜链 1000 → 2000 净值 = {nL1:F2} → {nL2:F2} ms");
    Console.WriteLine($"        {ScaleVerdict(nL1, nL2)}");
    Console.WriteLine($"   判据 B（同规模、形状相反）：n = 2000 上，左斜 {nL2:F2} ms vs 右斜 {nR2:F2} ms"
        + (nR2 > 0.01 ? $" ⇒ 相差 {nL2 / Math.Max(nR2, 0.0001):F1} 倍" : ""));
    Console.WriteLine();
    Console.WriteLine("   ⭐ 先别急着改代码，先回答三个问题（这才是这块数字的用处）：");
    Console.WriteLine("      ① 判据 A 的斜率是线性的还是平方的？**取决于哪一步**？");
    Console.WriteLine("          （提示：问「每一层里、那个最花时间的动作，在这条形状上各要做多少次」。）");
    Console.WriteLine("      ② 判据 B：两条斜链**节点数一模一样**，为什么代价可能差很多？差在哪一步？");
    Console.WriteLine("      ③ ⚠️ 如果你写的是「逆序递归 + prev」那一版，预期 **①≈2 倍、②≈1 倍** ——");
    Console.WriteLine("          那就拿 ① 的 2 倍去说明它是线性，而不是去等一个永远不会出现的 4 倍。");
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
    // TODO: 把 root 展开成一条「只有 right 的链」，顺序 == 原树的前序遍历。
    //       就地修改：**不 new 任何节点、也不换根**（root 引用必须原样）。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      ① 目标形态 `[1,null,2,null,3,...]` 一句话：**left 全空、right 串成前序**。
    //      ② 前序的顺序是「根 → 左 → 右」⇒ **站在根上时，"下一个该是谁"永远是"左子树的最左端"**。
    //      ③ ⇒ 于是本题唯一的新动作就一句话：
    //         **把「右子树」整根挂到「左子树」去**，然后把左子树提上来当右子树、left 清空。
    //
    //   ── 与前面 14 站的分界线（一个字：改 vs 造）：
    //      105/106 是 `new TreeNode` 造新树；本题**一个新节点都不许造** ——
    //      改的是既有节点的 left/right ⇒ 判题器② 的第一条独立裁判就是
    //      「**展开后那批节点还是不是原来那批对象**」（引用同一性）。
    //      ⚠️ 推论：既然不换根、又是 void ⇒ "改完了没有"只能靠**形态**判（left 全空 + 沿 right 读回前序）。
    //
    //   ── 落笔前必须回答的六个问题：
    //      ① 站在 `root` 上，做完哪两件事之后，"剩下的活"就只剩对**两个子节点**重复同样的事？
    //         ⚠️ 顺序问题：**先把 right 存进局部变量**再动指针 —— 为什么？（少了这一步会丢半棵树）
    //      ② 把右子树挂到"左子树那条链的**末尾**"：那个末尾怎么找？
    //         （这一步是本题第一个复杂度陷阱 —— 先在纸上数一下它要花多少步。）
    //      ③ 展开后的链上，**最后一个节点**的 right 是什么？不置 null 会怎样？（判题器② 的"环"就是抓它）
    //      ④ 空树 / 单节点：你的代码靠哪一行"天然"处理掉？（别写特判，先看循环/递归自己怎么说）
    //      ⑤ 递归版：左斜链 2000 节点时**递归深度**是多少？拿它跟 106 比一比。
    //      ⑥ ⭐ 有没有一个**既不递归、也不用找末尾**的写法？
    //         （提示：把"前序"倒着想 —— 右 → 左 → 根，一个 `prev` 指针就串起来了。）
    //
    //   ── 写完自己回答（这才是今天的账）：
    //      · 判据 A 的斜率是什么？**取决于哪一步**？把那个动作换成什么才会变线性？
    //      · 判据 B：两条斜链节点数一样，代价差在哪一步、差几倍？
    //      · ⭐ 为什么"先递归左、再递归右、最后接链"能是对的？（拿一棵 3 层满树手推一遍，写下每一步的形态）
    //      · ⛔ 既然不许 new 节点，那"展开"这件事一共改了几个指针？（拿[官方示例 1]数一数，报个数）
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      时间 O(?)、空间 O(?)      ← 算完拿性能区验
    //      ⚠️ 空间别忘了算**递归深度**（左斜链上有多深？）
    //
    //   ── 验收标准：
    //      ① 判题器⓪-A 全绿 且 ⓪-B 全绿（尺子准 + 判题器可信）；
    //      ② 判题器① 全绿（有期望值的那些组，层序一字不差）；
    //      ③ 判题器② 全绿（就地改 / 前序回读 / left 全空 / 链长对，四项全 ✅）；
    //      ④ 性能区判据 A、判据 B 的差异**能一句话解释清楚**；
    //      ⑤ 能一句话回答："**为什么本题的判题器必须每次重新造一棵树？**"
    //
    //   ── 进阶（做完一条再说）：
    //      ① 「逆序递归 + `prev`」版：右 → 左 → 根，一遍 O(n)、不用找末尾（对照判据 A/B 的预期变化）。
    //      ② 莫里斯式写法：**每步把右子树挂到左子树的最右端，然后 `root = root.left`** ⇒ O(1) 空间、不递归。
    //      ③ 变体（面试常追问）：展开成**中序**顺序；展开成**双向链表（left 也用上）**。
    public void Flatten(TreeNode? root)
    {
        if(root == null) return;
        TreeNode? oldRight = root.right;
        Flatten(root.left);
        Flatten(root.right);
        root.right = root.left;
        root.left  = null;
        TreeNode? tail = root;
        while(tail.right != null)
            {
                tail = tail.right;
            }
        tail.right = oldRight;
    }

}
