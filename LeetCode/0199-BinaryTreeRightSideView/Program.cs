using System.Diagnostics;
// ═══════ 建树与序列化工具（144 / 94 / 145 / 102 / 104 / 226 / 101 / 110 同一套，第九次复用）═══════
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

// 把"右视图结果"显示成 "[1, 3, 4]" —— 与 Show 的树形写法刻意长得不一样，免得两张表看混
static string ShowList(IReadOnlyList<int> xs) => "[" + string.Join(", ", xs) + "]";

static bool Same(IReadOnlyList<int> a, IReadOnlyList<int> b)
{
    if (a.Count != b.Count) return false;
    for (int k = 0; k < a.Count; k++) if (a[k] != b[k]) return false;
    return true;
}

// ═══════ 独立裁判（把【定义】逐字直译）：右视图 =「对每一个深度，取该深度【最右】的那个节点的值」═══════
//   ⚠️ 它是【尺子】，不是答案 —— 定义怎么说就怎么写，**不做任何优化**：
//      每求一层，就把整棵树从头再走一遍（明知可以剪枝也不剪）→ 代价 ≈ h × n，慢得没道理。
//      价值有两处：① 与"我的实现"零共享（不用队列分层、也不靠"先右后左"的直觉）→ 能替我抓"答案抄错 / 想歪"；
//                 ② 它自带【产量表】（naiveVisits），把"重走一遍"这件事变成看得见的数字。
int naiveVisits = 0;

int Height(TreeNode? node) => node is null ? 0 : 1 + Math.Max(Height(node.left), Height(node.right));

// 把"深度 == target"的节点按【从左到右】全收进 bag → bag 的【最后一个】就是该层最右
void CollectAtDepth(TreeNode? node, int target, int depth, List<int> bag)
{
    naiveVisits++;
    if (node is null) return;
    if (depth == target) bag.Add(node.val);
    CollectAtDepth(node.left, target, depth + 1, bag);   // 先左
    CollectAtDepth(node.right, target, depth + 1, bag);  // 后右 → 末尾才是"最右"
}

List<int> RightViewByDefinition(TreeNode? root)
{
    List<int> res = new List<int>();
    if (root is null) return res;              // 空树 → 空列表（不是 null、不是 [0]、不是 [[]]）
    int h = Height(root);
    for (int d = 0; d < h; d++)
    {
        List<int> bag = new List<int>();
        CollectAtDepth(root, d, 0, bag);
        res.Add(bag[^1]);
    }
    return res;
}

// ═══════ 判题器⓪：裁判自检（**完全不碰 Solution**）═══════
//   ⭐ 先证明"尺子本身是准的"，再去量答案。
//      理由：用一把歪尺子量人，写对了也会显示 ❌ —— 那种"错"是尺子的错。
//      本区必须在 Solution 还没写的时候就能跑（所以它与判题器①读同一张表，但只跑裁判）。
void SelfCheck(string title, int?[] level, int[] expected)
{
    naiveVisits = 0;
    TreeNode? root = Build(level);
    List<int> got = RightViewByDefinition(root);
    bool ok = Same(got, expected);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [裁判自检] {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：  {Show(root)}");
        Console.WriteLine($"     裁判答：{ShowList(got)}   我写的期望值：{ShowList(expected)}   ← 不一致，先查是谁错");
    }
    else
    {
        Console.WriteLine($"     n = {CountNodes(root)}｜裁判重走了 {naiveVisits:N0} 个节点");
    }
}

static int CountNodes(TreeNode? root)
{
    if (root is null) return 0;
    return 1 + CountNodes(root.left) + CountNodes(root.right);
}

// ═══════ 判题器①：期望值比对（量 Solution）═══════
//   ⏳ Solution 还没写时（抛 NotImplementedException）本区自动跳过 —— 于是"尺子校准"能先单独跑通。
IList<int>? TrySolve(TreeNode? root, out bool ready)
{
    ready = true;
    try { return new Solution().RightSideView(root); }
    catch (NotImplementedException) { ready = false; return null; }
}

void Check(string title, int?[] level, int[] expected)
{
    TreeNode? root = Build(level);
    IList<int>? actual = TrySolve(root, out bool ready);
    if (!ready) { Console.WriteLine($"⏳ {title}  → Solution 尚未实现，跳过"); return; }

    int[] a = new int[actual!.Count];
    for (int k = 0; k < a.Length; k++) a[k] = actual[k];

    bool ok = Same(a, expected);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：{Show(root)}");
        Console.WriteLine($"     期望：{ShowList(expected)}   实际：{ShowList(a)}");
    }
}

// ═══════ 判题器②：与【独立裁判】互证（不看期望值，直接和"定义直译版"对答案）═══════
//   用途：万一表里的期望值本身被我抄错，①和②会同时绿（因为抄的是同一张表）——
//         但②比的是"两种实现"，所以它多抓一类错：**我的期望值抄错了、但我的实现"很配合地"也错了同一个地方**。
void CrossCheck(string title, int?[] level)
{
    TreeNode? root = Build(level);
    IList<int>? actual = TrySolve(root, out bool ready);
    if (!ready) return;

    List<int> judge = RightViewByDefinition(root);
    int[] a = new int[actual!.Count];
    for (int k = 0; k < a.Length; k++) a[k] = actual[k];

    bool ok = Same(a, judge);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [与裁判互证] {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：{Show(root)}");
        Console.WriteLine($"     裁判：{ShowList(judge)}   我的：{ShowList(a)}");
    }
}

// ═══════ 用例表（⭐ 唯一数据源：上面三套判题器读的是同一张表）═══════
//   为什么不写三遍：抄一遍期望值就多一次抄错的机会。表只有一份，三边同时读 ——
//   于是"裁判自检全绿" ⇒ 表里的期望值是对的 ⇒ ① 比对 Solution、② 两种实现互证，链路才闭合。
(string Title, int?[] Level, int[] Expected)[] cases = new (string, int?[], int[])[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：`[1,2,3,null,5,null,4]`（左支右孩子 5 与右支右孩子 4 同层）", new int?[] { 1, 2, 3, null, 5, null, 4 }, new int[] { 1, 3, 4 }),
    ("★ 官方示例 2：`[1,null,3]`（一路向右）", new int?[] { 1, null, 3 }, new int[] { 1, 3 }),
    ("★ 官方示例 3：空树 → **空列表**（不是 null、不是 [0]）", new int?[] { }, new int[] { }),

    // ── 边界：单点 / 只有一个孩子 ──
    ("★ 单节点：`[1]`", new int?[] { 1 }, new int[] { 1 }),
    ("★ 只有左孩子：`[1,2]`", new int?[] { 1, 2 }, new int[] { 1, 2 }),
    ("★ 只有右孩子：`[1,null,2]`", new int?[] { 1, null, 2 }, new int[] { 1, 2 }),

    // ── 两条斜链：每层只剩一个节点 ──
    ("★★ 左斜链 3 层：`[1,2,null,3]` → 每层就是它自己", new int?[] { 1, 2, null, 3 }, new int[] { 1, 2, 3 }),
    ("★★ 右斜链 3 层：`[1,null,2,null,3]` → 方向反过来答案一样", new int?[] { 1, null, 2, null, 3 }, new int[] { 1, 2, 3 }),
    ("★★ 完美满树 3 层：`[1,2,3,4,5,6,7]` → 每层最右 = 2^k − 1", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 1, 3, 7 }),

    // ── 陷阱组：⭐ 右视图的答案【可能来自左子树】—— 一路向右收集的写法会在这里露馅 ──
    ("⛔ 陷阱 A（最深一层只有 **左支** 有节点）：`[1,2,3,4]` → 末位是 **左支** 的 4",
        new int?[] { 1, 2, 3, 4 }, new int[] { 1, 3, 4 }),
    ("⛔ 陷阱 B（左支比右支深两层，第 3、4 层答案都从 **左支** 出）：`[1,2,3,4,null,null,null,5]`",
        new int?[] { 1, 2, 3, 4, null, null, null, 5 }, new int[] { 1, 3, 4, 5 }),
    ("⛔ 陷阱 C（答案左右交替出：第 2 层归 **右支**、第 3 层又归 **左支**）：`[1,2,3,4,5,null,null,6]`",
        new int?[] { 1, 2, 3, 4, 5, null, null, 6 }, new int[] { 1, 3, 5, 6 }),
    ("★★ 答案全在右支（右支独走两层）：`[1,2,3,null,null,null,4]`",
        new int?[] { 1, 2, 3, null, null, null, 4 }, new int[] { 1, 3, 4 }),

    // ── 值域：含 0 与负数（防止误用"0 = 没有 / 空"当哨兵）──
    ("★★ 含 0 与负数：`[0,-1,1]` → 0 是合法节点值，别拿它当空", new int?[] { 0, -1, 1 }, new int[] { 0, 1 }),
};

Console.WriteLine("═══ 判题器⓪：裁判自检区（证明尺子是准的，不碰 Solution）═══");
foreach (var c in cases) SelfCheck(c.Title, c.Level, c.Expected);

Console.WriteLine("\n═══ 判题器①：期望值比对（量 Solution）═══");
foreach (var c in cases) Check(c.Title, c.Level, c.Expected);

Console.WriteLine("\n═══ 判题器②：与独立裁判互证（不看期望值，两种实现对答案）═══");
foreach (var c in cases) CrossCheck(c.Title, c.Level);

// ═══════ 性能裁判：让"重走一遍"的代价变成看得见的数字 ═══════
//   ⚠️ 先拆一个【从 110 顺手带过来的直觉】：那天朴素裁判在【斜链上反而便宜】（根上一眼就崩）。
//      本题的裁判不一样 —— 它**每一层都要走遍全树**，代价 ≈ **h × n** ⇒ **h 大的形状（斜链）最贵**。
//      ⭐ 判据：**同一个词（"朴素法"）在两个题里成本结构可以完全相反**，得看它到底在重复做什么。
int chainNodes = 2000;
int?[] chain = new int?[2 * chainNodes - 1];      // 右斜链：h = n，最吃亏的形状
chain[0] = 1;
for (int idx = 1; idx < chain.Length; idx++) chain[idx] = (idx % 2 == 1) ? null : 2;
TreeNode? bigChain = Build(chain);

int levels = 16;                                  // 完美满树：n = 2^16 − 1 = 65,535，h = 16（矮胖）
int?[] full = new int?[(1 << levels) - 1];
for (int idx = 0; idx < full.Length; idx++) full[idx] = 1;
TreeNode? bigFull = Build(full);

void PerfJudge(string title, TreeNode? tree)
{
    naiveVisits = 0;
    Stopwatch swJudge = Stopwatch.StartNew();
    List<int> judgeAnswer = RightViewByDefinition(tree);
    swJudge.Stop();

    IList<int>? solAnswer = TrySolve(tree, out bool ready);
    long solMs = -1;
    if (ready)
    {
        Stopwatch swSol = Stopwatch.StartNew();
        _ = new Solution().RightSideView(tree);
        swSol.Stop();
        solMs = swSol.ElapsedMilliseconds;
    }

    Console.WriteLine($"\n═══ 性能裁判 · {title} ═══");
    Console.WriteLine($"   树高 h = {Height(tree)}｜节点数 ≈ {CountNodes(tree):N0}｜裁判重走 {naiveVisits:N0} 个节点｜{swJudge.ElapsedMilliseconds} ms");
    Console.WriteLine($"   裁判答案尾部：…{string.Join(", ", judgeAnswer.Skip(Math.Max(0, judgeAnswer.Count - 3)))}");
    Console.WriteLine(ready
        ? $"   你的 Solution  ：{solMs} ms"
        : "   你的 Solution  ：⏳ 尚未实现");
}

PerfJudge($"形状 A：{chainNodes} 节点右斜链（h = n → 裁判每层都要走遍全树，最亏）", bigChain);
PerfJudge($"形状 B：完美满树 {levels} 层 / {full.Length:N0} 节点（h 只有 {levels} → 同题里最省）", bigFull);

Console.WriteLine($"\n判据：本题裁判代价 ≈ h × n —— 与 110 那天（斜链反而便宜）**结论相反**。");
Console.WriteLine($"      所以「与裁判同量级」在这里不是验收标准（你的解该是每节点只碰一次的 O(n)）；");
Console.WriteLine($"      验收看图的是：**两组形状下你的解都远快于裁判**，且答案与裁判逐项一致。");

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
    // TODO: 返回二叉树的【右视图】—— 从右侧看过去，每一层能看见的那个节点值（自上而下）。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      "右视图"翻译成一句话 = **每一层最右边那个节点的值**。
    //      ⭐ 而"一层一层地处理"这件事，你昨天（102）刚做过 —— 所以第一个问题不是"怎么写"，
    //         而是"**102 的那套装备，本题能直接用吗？要改哪一处？**"
    //
    //   ── 落笔前必须回答的四个问题：
    //      ① 怎么知道"这一层走完了"？（回想 102：`int levelSize = q.Count` 要取在 for 的**哪里**？）
    //      ② 每层该收【第一个】出队的，还是【最后一个】出队的？为什么？
    //      ③ 空树返回什么？（看官方示例 3：是 null？是 `[[]]`？还是空列表？）
    //      ④ 若改用 DFS 走（先右后左），"最右"这个信息藏在哪？—— 我该在**什么时候**记下当前节点？
    //
    //   ── 自问自答（写完再回来看）：
    //      · ⛔ 陷阱：答案**不一定来自右子树**！左支更深时，最深一层的答案就在左支里（看陷阱 A / B / C）。
    //        用哪一组用例能一眼抓出"只会一路向右走"的写法？
    //      · ⭐ 若走 DFS：为什么"**先右后左** + **每层只记第一次到达的**"拿到的就是最右？
    //        —— 换成"先左后左"，同一套记账还成立吗？为什么？
    //      · 队列峰值（BFS）与递归栈深（DFS）分别是多少？哪种形状下 BFS 更费、哪种形状下 DFS 更费？
    //        （承 104 / 101 那族"形状决定谁更省"的结论；今天的形状 A / B 正好是两极。）
    //      · 值域里有 0 和负数 —— 所以**不能用 0 当"这里没人"的哨兵**，为什么？
    //
    //   ── 复杂度（自己算一遍）：
    //      BFS 版：时间 O(?)、空间 O(?)（w = 最宽层）
    //      DFS 版：时间 O(?)、空间 O(?)（h = 树高）
    //      ⭐ 上面的「性能裁判」两种形状各跑一次 —— 算完拿它验一验你的判断。
    //
    //   ── 验收标准：
    //      ① 用例表全绿（含空树 / 两条斜链 / **答案来自左子树**的三组陷阱 / 含 0 与负数）；
    //      ② 裁判自检区全绿（尺子准 ⇒ 表里的期望值可信）；
    //      ③ 判题器②「与裁判互证」逐项一致（证明是"两种实现"对上了，不是抄的同一张表）；
    //      ④ 能一句话说清"为什么每层的最后一个就是右视图那个"。
    //
    //   ── 进阶（做完 BFS 版再说，别今天硬塞）：
    //      ① DFS 版 `RightSideViewDfs`：先右后左 + 记当前深度，**每层只收第一次到达的** →
    //         写完在下面加一区"双版互证"：同一份 `cases`，两版都必须等于期望值。
    //      ② 两版对比空间的实测：造一棵"斜链"和一棵"满树"，比 BFS 队列峰值 vs DFS 递归深度。
    public IList<int> RightSideView(TreeNode? root)
    {
        List<int> result = new List<int>();
        if(root == null) return result;
        Queue<TreeNode> q = new Queue<TreeNode>();
        q.Enqueue(root);
        while(q.Count > 0)
        {
            int times = q.Count;
            for(int i = 0;i < times; i++)
            {
                TreeNode cur = q.Dequeue();
                if(cur.left != null) q.Enqueue(cur.left);
                if(cur.right != null) q.Enqueue(cur.right);
                if(i == times - 1) result.Add(cur.val);
            }
        }
        return result;
    }
}
