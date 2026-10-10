using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════════════════
//  108 · 将有序数组转换为二叉搜索树
//  脚手架（第十二次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144/94/145/102/104/226/101/110/199/98/700 → 本题）
//
//  ⭐⭐ 本题与之前 11 题最大的不同：**答案不唯一**。
//     ⇒ 判题器**不能**照抄 700 那套「期望序列化比对」——
//        同一份输入有【多种】合格答案，硬比序列化会把正确答案判成 ❌。
//     ⇒ 本题改用【性质裁判】：只检查结果「长什么样算合格」（把题目定义逐字直译成
//        几条可机械验证的性质），**完全不管它是怎么建出来的**。
//        代价：查不出「有没有用上前提」（那是性能裁判的活）。
//
//  ⭐ 顺带一条元认知：**「判题器长什么样」由「答案唯不唯一」决定** ——
//     唯一答案 ⇒ 可以比对；多解 ⇒ 只能比性质。
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十二次复用） ─────────────────────────

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

static int CountNodes(TreeNode? root) => root is null ? 0 : 1 + CountNodes(root.left) + CountNodes(root.right);

static int Height(TreeNode? node) => node is null ? 0 : 1 + Math.Max(Height(node.left), Height(node.right));

// ⚠️ 上面两个都是【递归】的 —— 在一条 n = 26 万的「斜链」上它们会**栈溢出**
//    （.NET 的 StackOverflowException 不可捕获，进程当场死）。
//    所以"退化对照"那一段必须用下面两个【迭代】版本。
static int CountNodesIt(TreeNode? root)
{
    if (root is null) return 0;
    int count = 0;
    Stack<TreeNode> st = new Stack<TreeNode>();
    st.Push(root);
    while (st.Count > 0)
    {
        TreeNode cur = st.Pop();
        count++;
        if (cur.left is not null) st.Push(cur.left);
        if (cur.right is not null) st.Push(cur.right);
    }
    return count;
}

static int HeightIt(TreeNode? root)
{
    if (root is null) return 0;
    int best = 0;
    Stack<(TreeNode node, int depth)> st = new Stack<(TreeNode, int)>();
    st.Push((root, 1));
    while (st.Count > 0)
    {
        var (node, depth) = st.Pop();
        if (depth > best) best = depth;
        if (node.left is not null) st.Push((node.left, depth + 1));
        if (node.right is not null) st.Push((node.right, depth + 1));
    }
    return best;
}

// ─────────────────── 判题器⓪：输入前提自检（不碰 Solution） ───────────────────

// 题目原话：「元素已经按【升序】排列」—— 这是全题的立足点。
// ⚠️ 一旦用例表里混进一个非升序数组，题目前提就崩了 ⇒ 我是在【错误的题目】上练手。
// 判据：**严格**递增（BST 要求严格 —— 有相等值就无处安放）。
bool IsStrictlyIncreasing(int[] a)
{
    for (int i = 1; i < a.Length; i++)
        if (a[i] <= a[i - 1]) return false;
    return true;
}

// ───────────────────── 性质裁判：本题的「尺子」 ─────────────────────

// 题目定义逐字直译 ⇒ 三条性质：
//   ① 【中序遍历 == 输入数组】—— 一字不差：个数、顺序、值，全对
//   ② 【高度平衡】—— 每个节点的 |左子树高 − 右子树高| ≤ 1
//   ③ 【合法 BST】—— ① 成立时它自动成立（严格递增的中序 ⟺ BST），
//      但仍**显式再验一遍**：防「中序比较写成了自己跟自己比」这类假绿
// 外加一条防作弊：
//   ④ 【节点数 == 数组长度】—— 不许多造节点、不许漏
List<int> Inorder(TreeNode? root)
{
    List<int> sink = new List<int>();
    void Walk(TreeNode? n)
    {
        if (n is null) return;
        Walk(n.left);
        sink.Add(n.val);
        Walk(n.right);
    }
    Walk(root);
    return sink;
}

// 返回子树高度；一旦发现不平衡，全程短路返回 -1（照 110 的写法）
int BalancedHeight(TreeNode? node)
{
    if (node is null) return 0;
    int l = BalancedHeight(node.left);
    if (l < 0) return -1;
    int r = BalancedHeight(node.right);
    if (r < 0) return -1;
    return Math.Abs(l - r) > 1 ? -1 : 1 + Math.Max(l, r);
}

// BST 独立裁判 = 定义逐字直译：**每个节点背着一个由祖先夹出来的值域区间**（98 甲法）。
// ⚠️ 边界必须用 long —— 用例里有 int.MinValue / int.MaxValue，用 int 夹会溢出误判。
bool IsBstByBounds(TreeNode? node, long low, long high)
{
    if (node is null) return true;
    if (node.val <= low || node.val >= high) return false;
    return IsBstByBounds(node.left, low, node.val) && IsBstByBounds(node.right, node.val, high);
}

bool CheckProperties(TreeNode? root, int[] nums, out string why)
{
    List<string> bad = new List<string>();

    int n = CountNodes(root);
    if (n != nums.Length) bad.Add($"④ 节点数 {n} ≠ 数组长度 {nums.Length}");

    List<int> mid = Inorder(root);
    if (!mid.SequenceEqual(nums))
        bad.Add($"① 中序 ≠ 输入数组（拿到 {mid.Count} 个；前 8 个：{(mid.Count == 0 ? "空" : string.Join(",", mid.Take(8)))}）");

    if (BalancedHeight(root) < 0) bad.Add("② 不是【高度平衡】：某节点的左右子树高差 > 1");

    if (!IsBstByBounds(root, long.MinValue, long.MaxValue)) bad.Add("③ 不是【合法 BST】：值域区间被违反");

    why = bad.Count == 0 ? "四条性质全过" : string.Join("｜", bad);
    return bad.Count == 0;
}

// 「最优高度」= ⌈log2(n+1)⌉ = h 层满树最多装 2^h − 1 个节点，反过来求最小 h。
// ⭐ 这是「取中点、每层对半」能达到的高度；比它高就说明**没有**对半分。
int MinPossibleHeight(int n)
{
    int h = 0, cap = 0;
    while (cap < n) { h++; cap = (1 << h) - 1; }
    return h;
}

// ─────────────────── 判题器①：性质裁判（量 Solution） ───────────────────

(bool ready, TreeNode? tree, long ms) TryBuild(int[] nums)
{
    try
    {
        Stopwatch sw = Stopwatch.StartNew();
        TreeNode? t = new Solution().SortedArrayToBST(nums);
        sw.Stop();
        return (true, t, sw.ElapsedMilliseconds);
    }
    catch (NotImplementedException) { return (false, null, -1); }
}

// ────────────────────────── 用例表（唯一数据源） ──────────────────────────
// ⚠️ 表里**没有**「期望树」这一列 —— 因为答案不唯一，写不出唯一期望（这就是今天的第一课）。
//    每一组只提供**输入**，合格与否交给上面的性质裁判。
(string Title, int[] Nums)[] cases = new (string, int[])[]
{
    // ── 官方示例 ──
    ("★ 官方示例 1：[−10,−3,0,5,9]（n = 5，奇数）", new int[] { -10, -3, 0, 5, 9 }),
    ("★ 官方示例 2：[1,3]（n = 2 ⇒ **答案不唯一**：谁当根都合法）", new int[] { 1, 3 }),

    // ── 边界：空 / 单元素 ──
    ("★ 空数组 [] ⇒ 必须返回 **null**（一个节点都不许建）", Array.Empty<int>()),
    ("★ 单元素 [1] ⇒ 一个光杆根", new int[] { 1 }),

    // ── 逐个加长，看"最小高度"怎么跳 ──
    ("★ 两元素 [1,2]（最小高度 2）", new int[] { 1, 2 }),
    ("★ 三元素 [1,2,3]（n = 3 正好装成满树 ⇒ 最小高度 2）", new int[] { 1, 2, 3 }),
    ("★ 四元素 [1,2,3,4]（n = 4 ⇒ 最小高度 3，左右必然不对称）", new int[] { 1, 2, 3, 4 }),
    ("★ 十六元素 [1..16]（正好满树 ⇒ 最小高度 4）", Enumerable.Range(1, 16).ToArray()),

    // ── 值域：0 / 负数 / 大跨距 ──
    ("★ 值域含 0 与负数：[−2,−1,0,1,2]", new int[] { -2, -1, 0, 1, 2 }),
    ("★ 含零与负数、长度偶数：[−7,−3,0,2,9,11]", new int[] { -7, -3, 0, 2, 9, 11 }),
    ("★ 等距大数：[0, 1000000, 2000000, 3000000]", new int[] { 0, 1000000, 2000000, 3000000 }),

    // ── ⛔ int 两端：专抓"用 int 当值域上下界"的 BST 裁判 ──
    ("⛔ 值域两端：[int.MinValue, int.MaxValue]（用 int 夹区间会溢出）",
        new int[] { int.MinValue, int.MaxValue }),
    ("⛔ 值域两端 + 零：[int.MinValue, 0, int.MaxValue]",
        new int[] { int.MinValue, 0, int.MaxValue }),

    // ── 规模：验证"高度 = log2 n"在长数组上依然成立 ──
    ("★ n = 1000 连续整数（最小高度 10）", Enumerable.Range(1, 1000).ToArray()),
};

Console.WriteLine("═══ 判题器⓪：输入前提自检（数组必须严格递增 —— 完全不碰 Solution）═══");
foreach (var c in cases)
{
    bool ok = IsStrictlyIncreasing(c.Nums);
    Console.WriteLine($"{(ok ? "✅" : "❌")} [前提] {c.Title}");
    if (!ok) Console.WriteLine("     ⛔ 这组用例不是升序 ⇒ 题目前提不成立，先修用例表，别急着看答案。");
}

Console.WriteLine("\n═══ 判题器①：性质裁判（本题的「对错」只能这么判 —— 答案不唯一）═══");
foreach (var c in cases)
{
    var r = TryBuild(c.Nums);
    if (!r.ready) { Console.WriteLine($"⏳ {c.Title}  → Solution 尚未实现，跳过"); continue; }

    bool ok = CheckProperties(r.tree, c.Nums, out string why);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {c.Title}");
    if (!ok) Console.WriteLine($"     ⛔ {why}");
}

Console.WriteLine("\n═══ 判题器②：形状与最优性（**不计错**，只打印判据）═══");
Console.WriteLine("   判据：最优高度 = ⌈log2(n+1)⌉（每层对半分能达到的最小高度）");
foreach (var c in cases)
{
    var r = TryBuild(c.Nums);
    if (!r.ready) break;

    int n = c.Nums.Length;
    int h = Height(r.tree);
    int hMin = MinPossibleHeight(n);
    string dump = n <= 16 ? Show(r.tree) : $"[{n} 个节点，略]";
    Console.WriteLine($"   {(h == hMin ? "🟢" : "🟡")} {c.Title}");
    Console.WriteLine($"        n = {n}｜树高 = {h}｜最优 = {hMin}"
        + (h == hMin ? " ⇒ 🟢 已是最优（对半分了）" : $" ⇒ 🟡 比最优高 {h - hMin} 层"));
    Console.WriteLine($"        层序：{dump}");
}

Console.WriteLine("\n═══ 判题器③：节点数平衡观察（**不计错**，只打印）═══");
Console.WriteLine("   ⭐ 知识点：「高度平衡」的定义是 |左高 − 右高| ≤ 1 —— 它**不要求**「节点数平衡」");
Console.WriteLine("      （|左子树大小 − 右子树大小| ≤ 1）。两件事不是一回事。");
int NodeBalance(TreeNode? node, ref bool bad)
{
    if (node is null) return 0;
    int l = NodeBalance(node.left, ref bad);
    int r = NodeBalance(node.right, ref bad);
    if (Math.Abs(l - r) > 1) bad = true;
    return l + r + 1;
}
foreach (var c in cases)
{
    var r = TryBuild(c.Nums);
    if (!r.ready) break;

    bool bad = false;
    NodeBalance(r.tree, ref bad);
    Console.WriteLine($"   {(bad ? "🟡 节点数有差（>1）—— 仍然合格（定义不要求它）" : "🟢 连节点数都对半分")} —— {c.Title}");
}

// ═══════════════ 形状区：把「取中点 ⇒ 平衡」变成看得见的数字 ═══════════════
//   ⚠️ 108 在【时间】上没什么悬念（谁都得把每个元素碰一遍 = O(n)）；
//      有意思的维度是【树高 / 递归深度】—— 它决定「BST 查询还能不能 O(log n)」。
//
//   ⭐ 先小后大：先拿小规模探一眼形状，**形状健康**才上大输入。
//      理由很实在 —— 若你的解是「逐个插入」式的**递归**写法，直接喂 262,143 会**栈溢出**，
//      .NET 里 StackOverflowException **不可捕获**，进程当场死掉（后面什么都看不到）。
int probeN = 4095;      // 2^12 − 1
int bigN = 262143;      // 2^18 − 1（与 700 同一尺寸，便于横向对照）

int[] MakeSorted(int n)
{
    int[] a = new int[n];
    for (int i = 0; i < n; i++) a[i] = i - n / 2;   // 含负数、居中
    return a;
}

Console.WriteLine("\n═══ 形状区 A：小规模探针（决定要不要上大输入）═══");
var probe = TryBuild(MakeSorted(probeN));
bool probeHealthy = false;
if (!probe.ready)
{
    Console.WriteLine("   ⏳ Solution 尚未实现 ⇒ 形状区跳过（下面的「退化对照」与你的解无关，照常看）");
}
else
{
    int h = Height(probe.tree);
    int hMin = MinPossibleHeight(probeN);
    probeHealthy = h == hMin && CountNodes(probe.tree) == probeN;
    Console.WriteLine($"   n = {probeN:N0}｜你的解：树高 = {h}｜最优 = {hMin} ⇒ {(probeHealthy ? "🟢 形状健康，可以上大输入" : "🟡 形状已退化，**大输入跳过**（免得进程被栈溢出杀掉）")}");
}

if (probe.ready && probeHealthy)
{
    Console.WriteLine($"\n═══ 形状区 B：大输入 n = {bigN:N0}（= 2^18 − 1）═══");
    var big = TryBuild(MakeSorted(bigN));
    int h = Height(big.tree);
    int hMin = MinPossibleHeight(bigN);
    Console.WriteLine($"   你的解：树高 = {h}｜节点数 = {CountNodes(big.tree):N0}｜{big.ms} ms｜最优 = {hMin}"
        + (h == hMin ? " ⇒ 🟢" : $" ⇒ 🟡 高出 {h - hMin} 层"));
    Console.WriteLine($"   性质裁判：{(CheckProperties(big.tree, MakeSorted(bigN), out _) ? "✅ 过" : "❌ 不过")}");
}

// ── 退化对照：同样是 n 个节点的【合法 BST】，只是「每次都取端点当根」──
//    ⚠️ 这条链**不经过你的 Solution**，是脚手架用【循环】自己搭的 ——
//       用递归搭它 / 量它都会栈溢出，这本身就是「取端点不可行」的实感。
TreeNode? BuildRightChain(int n)
{
    if (n <= 0) return null;
    TreeNode root = new TreeNode(0);
    TreeNode tail = root;
    for (int i = 1; i < n; i++) { tail.right = new TreeNode(i); tail = tail.right; }
    return root;
}

Console.WriteLine("\n═══ 退化对照：「每次都取端点当根」建出来的是什么 ═══");
TreeNode? chain = BuildRightChain(bigN);
Console.WriteLine($"   节点数 = {CountNodesIt(chain):N0}｜高度（**迭代**量，递归量会栈溢出）= {HeightIt(chain):N0}");
Console.WriteLine($"   ⇒ 同一批 {bigN:N0} 个元素：对半分 = {MinPossibleHeight(bigN)} 层，取端点 = {bigN:N0} 层。");
Console.WriteLine($"      差 {bigN - MinPossibleHeight(bigN):N0} 倍 —— **BST 的 O(log n) 全靠「高度」这一件事撑着**");
Console.WriteLine("      （这就是 700 结尾那句「平衡树存在的理由」，今天从「查」换到了「造」）。");

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
    // TODO: 把【升序】数组转换成一棵【高度平衡】的二叉搜索树，返回根节点。
    //       （本题**答案不唯一** —— 多个合格答案都算对，所以判题器只查性质，不查形状）
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      有序数组的**中点**天生就是"分界线"：比它小的全在左边、比它大的全在右边
    //      ⇒ 把它放在根上，BST 的性质**不用算就成立**（有序数组的下标正好把这件事变成"切一刀"）。
    //      而"取中点"还顺手满足了另一条要求 —— 左右两半长度差 ≤ 1 ⇒ 每层尽量对半分
    //      ⇒ 高度 = ⌈log2(n+1)⌉ = **最小可能高度**（这才是"高度平衡"想达到的东西）。
    //
    //   ── 与前面三站的关系（BST 一条线）：
    //      98 = **查**（这棵树合不合法）→ 700 = **找**（在里面搜一个值）→ 108 = **造**（按规则长出一棵）
    //      前两题在"读"一棵已存在的树，本题是"写"。
    //      与 104 / 110 同源的地方：**返回值携带的是"建好的子树"** ⇒ 孩子先建好、父才能挂
    //      ⇒ 又是一个**自底向上（后序）**的形状。
    //
    //   ── 落笔前必须回答的六个问题：
    //      ① 递归函数该返回什么？`void` 往参数里塞，还是**返回建好的子树根**？
    //         （问自己：哪一个能让"父节点挂孩子"这一步自然写出来？）
    //      ② 区间怎么表示？用 `(lo, hi)` 两个下标，还是每次 `Array.Copy` 切出新数组？
    //         两者的额外开销差在哪（提示：空间复杂度会不一样）？
    //      ③ 终止条件写哪个？`lo > hi` 还是 `lo == hi`？
    //         想清楚"空区间"和"单元素区间"分别该返回什么 —— 这决定空数组那组用例过不过。
    //      ④ 中点怎么算？`(lo + hi) / 2` 会不会溢出？（⚠️ 小心：这题里 `lo/hi` 是**下标**不是**值**，
    //         别把"值域可能到 int.MinValue"这件事张冠李戴到下标上 —— 但把这条想明白本身就很值钱）
    //      ⑤ "高度最优"是**取中点**换来的，还是**递归**换来的？
    //         （拿形状区的数字回答；如果觉得是递归换来的，试着换个位置取根，再看形状区）
    //      ⑥ 本题**答案不唯一** —— 那"判题器该怎么写"？
    //         （这是今天真正的元认知收获，见文件头的说明）
    //
    //   ── 自问自答（写完再回来看）：
    //      · 为什么 `mid` 放在根上就一定是 BST？把"每个节点的左子树全小于它、右子树全大于它"
    //        翻译成下标语言，答案就只有一句话。
    //      · ⭐ 为什么"取中点"能同时给出【BST】和【最优高度】两样东西？
    //      · ⚠️ 「高度平衡」（|左高 − 右高| ≤ 1）**不要求**「节点数平衡」
    //        （|左大小 − 右大小| ≤ 1）—— 判题器③会替你看这两件事到底一不一样；
    //        顺便想一想：能不能构造一棵"高度平衡、但节点数差很多"的树？（提示：12 个节点）
    //      · ⛔ 有条路"看起来聪明"：**逐个插入**一个普通 BST。
    //        它建出来的确实是合法 BST、中序也确实是升序 —— 但它在形状区里会**退化成一条链**
    //        （高度 = n）。想想：为什么？它错在哪？（提示：普通 BST 的插入不看"全局还剩多少"）
    //
    //   ── 复杂度（自己算一遍，别背）：
    //      时间 O(?)、空间 O(?)     ← 算完拿形状区的数字验一验
    //      ⭐ 若你用了 `Array.Copy` 切数组，空间会变成多少？（这是路径①与②的真正区别）
    //
    //   ── 验收标准：
    //      ① 判题器⓪ 全绿（前提成立）；
    //      ② 判题器① 全绿（中序 == 输入 / 高度平衡 / 合法 BST / 节点数对 —— 四条缺一不可）；
    //      ③ 形状区 A 🟢 且 B 🟢（高度 == ⌈log2(n+1)⌉）；
    //      ④ 能一句话回答："**为什么把中点放在根上，就同时满足了 BST 和高度平衡？**"
    //      ⑤ 能一句话回答："**这题为什么不能用 700 那种判题器？**"
    //
    //   ── 进阶（做完一条再说，别今天硬塞）：
    //      ① 换成「偶数长度时取**靠右**那个中点」再写一版 → 判题器① 照样全绿
    //         （这就是"答案不唯一"的实证），但层序输出会不一样。
    //      ② 不用递归 → 用**显式栈 + 区间**写一版（照 104 / 145 那条"同一顺序、换工具"的线）。
    //      ③ 变体（面试常追问）：输入换成**有序链表** → 数组能 O(1) 随机访问取中点，链表不能，
    //         那怎么办？（这题是 109，留个印象）
    public TreeNode? SortedArrayToBST(int[] nums)
    {
        return Build(nums,0,nums.Length - 1);
    }
    TreeNode? Build(int[] nums, int lo, int hi)
    {
        if(hi < lo) return null;
        TreeNode result = new TreeNode();
        int mid = (hi + lo) / 2;
        result.val = nums[mid];
        result.left = Build(nums,lo,mid - 1);
        result.right = Build(nums,mid + 1,hi);
        return result;
    }
}
