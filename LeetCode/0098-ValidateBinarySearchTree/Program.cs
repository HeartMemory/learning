using System.Diagnostics;
// ═══════ 建树与序列化工具（144 / 94 / 145 / 102 / 104 / 226 / 101 / 110 / 199 同一套，第十次复用）═══════
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

// ═══════ 独立裁判（把【定义】逐字直译）═══════
//   定义（照 LeetCode 原话直译）：对【每一个】节点，
//     ① 它的【整棵左子树】里所有节点值都【严格小于】它；
//     ② 它的【整棵右子树】里所有节点值都【严格大于】它；
//     ③ 左右子树各自也必须是 BST；④ 空树是 BST。
//   ⚠️ 它是【尺子】，不是答案 —— 定义怎么说就怎么写，**不做任何优化**：
//      不往下传"允许区间"、不做剪枝 ⇒ **每个节点都会被它的每一个祖先重新扫一遍**。
//      ⇒ 代价 ≈ Σ(子树大小)：**左斜链上最贵 ≈ n²/2**，满树上便宜 ≈ n·log n
//        （与 199 那天同族：同一个词"朴素法"，成本结构由"它到底在重复做什么"决定）。
int naiveVisits = 0;

bool AllLess(TreeNode? node, int bound)
{
    naiveVisits++;
    if (node is null) return true;
    if (node.val >= bound) return false;                      // 严格小于：等于也不算
    return AllLess(node.left, bound) && AllLess(node.right, bound);
}

bool AllGreater(TreeNode? node, int bound)
{
    naiveVisits++;
    if (node is null) return true;
    if (node.val <= bound) return false;                      // 严格大于：等于也不算
    return AllGreater(node.left, bound) && AllGreater(node.right, bound);
}

bool IsBstByDefinition(TreeNode? root)
{
    naiveVisits++;
    if (root is null) return true;
    return AllLess(root.left, root.val) && AllGreater(root.right, root.val)
           && IsBstByDefinition(root.left) && IsBstByDefinition(root.right);
}

// ═══════ 判题器⓪：裁判自检（**完全不碰 Solution**）═══════
//   ⭐ 先证明"尺子本身是准的"，再去量答案 —— 用歪尺子量人，写对了也会显示 ❌。
//      本区必须在 Solution 还没写的时候就能跑通。
void SelfCheck(string title, int?[] level, bool expected)
{
    naiveVisits = 0;
    TreeNode? root = Build(level);
    bool got = IsBstByDefinition(root);
    Console.WriteLine($"{(got == expected ? "✅" : "❌")} [裁判自检] {title}");
    if (got != expected)
        Console.WriteLine($"     输入：{Show(root)}｜裁判答：{got}｜我写的期望值：{expected}   ← 不一致，先查是谁错");
}

// ═══════ 判题器①：期望值比对（量 Solution）═══════
//   ⏳ Solution 还没写时（抛 NotImplementedException）本区自动跳过 —— 于是"尺子校准"能先单独跑通。
bool? TrySolve(TreeNode? root, out bool ready)
{
    ready = true;
    try { return new Solution().IsValidBST(root); }
    catch (NotImplementedException) { ready = false; return null; }
}

void Check(string title, int?[] level, bool expected)
{
    TreeNode? root = Build(level);
    bool? actual = TrySolve(root, out bool ready);
    if (!ready) { Console.WriteLine($"⏳ {title}  → Solution 尚未实现，跳过"); return; }

    bool ok = actual == expected;
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
        Console.WriteLine($"     输入：{Show(root)}｜期望：{expected}｜实际：{actual}");
}

// ═══════ 判题器②：与【独立裁判】互证（不看期望值，直接和"定义直译版"对答案）═══════
//   用途：万一表里的期望值被我抄错，①和②会同时绿（抄的是同一张表）——
//         但②比的是"两种实现"，所以它多抓一类错：**期望值抄错了、而我的实现"很配合地"也错了同一个地方**。
void CrossCheck(string title, int?[] level)
{
    TreeNode? root = Build(level);
    bool? actual = TrySolve(root, out bool ready);
    if (!ready) return;

    bool judge = IsBstByDefinition(root);
    bool ok = actual == judge;
    Console.WriteLine($"{(ok ? "✅" : "❌")} [与裁判互证] {title}");
    if (!ok)
        Console.WriteLine($"     输入：{Show(root)}｜裁判：{judge}｜我的：{actual}");
}

// ═══════ 用例表（⭐ 唯一数据源：上面三套判题器读的是同一张表）═══════
//   表只有一份、三边同时读：于是"裁判自检全绿" ⇒ 表里的期望值是对的 ⇒ ① 比对 Solution、② 两种实现互证，链路才闭合。
(string Title, int?[] Level, bool Expected)[] cases = new (string, int?[], bool)[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：`[2,1,3]` → 合法", new int?[] { 2, 1, 3 }, true),
    ("⛔ 官方示例 2：`[5,1,4,null,null,3,6]` → **非法**！每一对【父子】都成立，但 3 落在 5 的右子树里",
        new int?[] { 5, 1, 4, null, null, 3, 6 }, false),

    // ── 边界：空树 / 单点 / 只有一边 ──
    ("★ 空树 → **true**（空树是合法 BST，不是 null、不是 false）", new int?[] { }, true),
    ("★ 单节点 `[1]`", new int?[] { 1 }, true),
    ("★ 只有左孩子且更小 `[2,1]`", new int?[] { 2, 1 }, true),
    ("★ 只有右孩子且更大 `[1,null,2]`", new int?[] { 1, null, 2 }, true),

    // ── 严格性：相等也非法（判据词是"严格小于 / 严格大于"）──
    ("⛔ 重复值 `[2,2,2]` → false（左孩子必须**严格**小于根）", new int?[] { 2, 2, 2 }, false),
    ("⛔ 重复值 `[1,null,1]` → false（右孩子必须**严格**大于根）", new int?[] { 1, null, 1 }, false),

    // ── 陷阱组：⭐ 违反发生在【远处】—— "只比每一对父子"的写法在这里必露馅 ──
    ("⛔ 陷阱 A（右子树里塞了个小值）：`[10,5,15,null,null,6,20]` → false", new int?[] { 10, 5, 15, null, null, 6, 20 }, false),
    ("⛔ 陷阱 B（隔两层才违反）：`[5,4,6,null,null,3,7]` → false", new int?[] { 5, 4, 6, null, null, 3, 7 }, false),
    ("⛔ 陷阱 C（**左**子树里藏了个大值，且它和爸爸比是合法的）：`[10,5,15,3,11,null,20]` → false",
        new int?[] { 10, 5, 15, 3, 11, null, 20 }, false),

    // ── 合法对照（"看起来乱"但其实是 BST）──
    ("★★ 合法对照（四层、右支更深）：`[10,5,15,3,7,13,18]` → true", new int?[] { 10, 5, 15, 3, 7, 13, 18 }, true),
    ("★★ 合法对照（中序正好 0..6）：`[3,1,5,0,2,4,6]` → true", new int?[] { 3, 1, 5, 0, 2, 4, 6 }, true),

    // ── 两条斜链 ──
    ("★ 左斜链递增 `[3,2,null,1]` → true（每层只有一个节点）", new int?[] { 3, 2, null, 1 }, true),
    ("★ 右斜链递增 `[1,null,2,null,3]` → true", new int?[] { 1, null, 2, null, 3 }, true),
    ("⛔ 右斜链递减 `[3,null,2,null,1]` → false（右子树必须更大）", new int?[] { 3, null, 2, null, 1 }, false),

    // ── int 边界：⭐ 抓"拿 int 当无限大 / 无限小"的写法 ──
    ("⛔ 边界①：`[-2147483648]`（`int.MinValue` 单节点）→ true —— 用 `int.MinValue` 当中序【前驱哨兵】的写法会误判",
        new int?[] { -2147483648 }, true),
    ("⛔ 边界②：`[-2147483648,null,2147483647]` → true —— 用 `int` 当上下界递归的写法会误判",
        new int?[] { -2147483648, null, 2147483647 }, true),
    ("★ 边界③：`[2147483647]`（`int.MaxValue` 单节点）→ true", new int?[] { 2147483647 }, true),
};

Console.WriteLine("═══ 判题器⓪：裁判自检区（证明尺子是准的，不碰 Solution）═══");
foreach (var c in cases) SelfCheck(c.Title, c.Level, c.Expected);

Console.WriteLine("\n═══ 判题器①：期望值比对（量 Solution）═══");
foreach (var c in cases) Check(c.Title, c.Level, c.Expected);

Console.WriteLine("\n═══ 判题器②：与独立裁判互证（不看期望值，两种实现对答案）═══");
foreach (var c in cases) CrossCheck(c.Title, c.Level);

// ═══════ 性能裁判：让"每个节点被每个祖先重扫一遍"变成看得见的数字 ═══════
//   ⚠️ 必须用【合法】的 BST —— 否则裁判里的 `&&` 会短路提前停手，数字就不再代表"完整成本"了。
int chainN = 2000;
int?[] leftChain = new int?[2 * chainN - 1];                 // 左斜链的层序写法
leftChain[0] = chainN;                                       // 递减：n → n−1 → … → 1
for (int idx = 1; idx < leftChain.Length; idx++)
    leftChain[idx] = (idx % 2 == 1) ? chainN - (idx + 1) / 2 : null;
TreeNode? bigChain = Build(leftChain);

int fullDepth = 15;
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

void PerfJudge(string title, TreeNode? tree)
{
    naiveVisits = 0;
    Stopwatch swJudge = Stopwatch.StartNew();
    bool judgeAnswer = IsBstByDefinition(tree);
    swJudge.Stop();

    bool? solAnswer = null;
    long solMs = -1;
    try
    {
        Stopwatch swSol = Stopwatch.StartNew();
        solAnswer = new Solution().IsValidBST(tree);
        swSol.Stop();
        solMs = swSol.ElapsedMilliseconds;
    }
    catch (NotImplementedException) { }

    Console.WriteLine($"\n═══ 性能裁判 · {title} ═══");
    Console.WriteLine($"   h = {Height(tree)}｜n = {CountNodes(tree):N0}｜裁判重走 {naiveVisits:N0} 个节点｜{swJudge.ElapsedMilliseconds} ms｜裁判答 {judgeAnswer}");
    Console.WriteLine(solAnswer is null ? "   你的 Solution：⏳ 尚未实现" : $"   你的 Solution：{solMs} ms（答 {solAnswer}）");
}

PerfJudge($"形状 A：{chainN} 节点【左斜链】（h = n ⇒ 每个节点都被祖先重扫，最亏）", bigChain);
PerfJudge($"形状 B：完美满树 {fullDepth} 层（h 只有 {fullDepth} ⇒ 同题里最省）", bigFull);

Console.WriteLine($"\n判据：本裁判 = BST 定义直译 ⇒ 代价 ≈ Σ(子树大小)：斜链 ≈ n²/2，满树 ≈ n·log n。");
Console.WriteLine($"      正解两版（上下界递归 / 中序遍历）都是 **O(n)、每节点只碰一次** ⇒ 两组形状下都该远快于裁判。");

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
    // TODO: 判断这棵树是不是【有效的二叉搜索树（BST）】。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      ⛔ **别只比"父子"**。看官方示例 2：`[5,1,4,null,null,3,6]` —— 5>1、4<5、3<4、6>4，
    //         每一对父子都成立，可它**不是** BST（3 落在 5 的右子树里）。
    //      ⇒ 真正要管的是"**整棵子树的值域区间**"，不是"和爸爸比一次"。
    //
    //   ── 两条正路（先走通一条，别同时写）：
    //      甲、**上下界递归**：往下走的时候，把"这个节点允许的区间"传给孩子 ——
    //          往左走把上界收紧、往右走把下界收紧，一旦越界立刻返回 false。
    //      乙、**中序遍历**：BST 的中序序列必须【严格递增】——
    //          于是问题变成"遍历时，我能不能记住【上一个访问到的值】"。
    //
    //   ── 落笔前必须回答的四个问题：
    //      ① 甲法：往下传的区间，初值该是多少？
    //         (A) 直接写 `int.MinValue` / `int.MaxValue` → 拿用例 `[-2147483648]` 和
    //             `[-2147483648,null,2147483647]` 试一下会发生什么？该怎么修？
    //      ② 乙法：这个"上一个值"该放【参数】还是【字段 / 闭包变量】？为什么？（回想 110 那天
    //         "把结论放进参数往下传"的后果）
    //      ③ 乙法：哨兵初值能不能用 `int.MinValue`？（看"边界①"那条用例）
    //      ④ "相等"算不算合法？看用例 `[2,2,2]` 和 `[1,null,1]` —— 判据词是"**严格**小于 / 大于"。
    //
    //   ── 自问自答（写完再回来看）：
    //      · 甲法的区间是【开】还是【闭】？用 `int` 还是 `long` / 可空？为什么必须和"严格性"一起定？
    //      · 乙法为什么"只比前驱"就够？—— **中序有序**和**BST** 之间是【充要】关系吗？
    //      · 一句话对照两版：甲法 = "**把约束往下传**"；乙法 = "**把树拉成一条线**"。
    //        哪一版"发现得早"？代价差在哪（提示：甲法能剪枝，乙法要走到"当前节点"才知道前驱是谁）？
    //      · ⛔ 有个偷懒版本："每层只判 `left.val < root.val < right.val` 然后递归" ——
    //        它在哪条用例上翻车？为什么（用"值域区间"的话说一遍）？
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      甲：时间 O(?)、空间 O(?)（说清空间是"谁的深度"）
    //      乙：时间 O(?)、空间 O(?)
    //      ⭐ 下面的「性能裁判」两种形状各跑一次 —— 算完拿它验一验你的判断。
    //
    //   ── 验收标准：
    //      ① 用例表全绿（含 空树 / 重复值 / **远处违反**的陷阱 A·B·C / int 边界两端 / 两条斜链）；
    //      ② 裁判自检区全绿（尺子准 ⇒ 表里的期望值可信）；
    //      ③ 判题器②「与裁判互证」逐项一致（证明是"两种实现"对上了，不是抄的同一张表）；
    //      ④ 能一句话答出："**为什么只比较每一对父子是不够的？**"
    //
    //   ── 进阶（做完一条再说，别今天硬塞）：
    //      ① 另一条路也写一版（`IsValidBSTByInorder`）→ 在下面加一区"双版互证"：
    //         同一份 `cases`，两版都必须等于期望值（照 101 / 110 那套"两种实现互证"）。
    //      ② 乙法再写一个**迭代版**（借 94 那套显式栈，承本册「显式栈 = 把返回地址搬进栈里」）。
    public bool IsValidBST(TreeNode? root)
    {
        if(root == null) return true;
        Stack<TreeNode> trees = new Stack<TreeNode>();
        TreeNode cur = root;
        int? last = null;
        while(cur != null || trees.Count > 0)
        {
            while(cur != null)
            {
                trees.Push(cur);
                cur = cur.left;
            }
            cur = trees.Pop();
            if(cur.val <= last) return false;
            last = cur.val;
            cur = cur.right;
        }
        return true;
    }
}
