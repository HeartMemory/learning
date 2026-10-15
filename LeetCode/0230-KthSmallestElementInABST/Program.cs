using System.Diagnostics;

// ══════════════════════════════════════════════════════════════════════════════
//  230 · 二叉搜索树中第 k 小的元素（BST 的性质第一次被"榨着用"）
//  脚手架（第十六次复用同一套「工具 + 前提自检 + 判题器」骨架：
//            144 / 94 / 145 / 102 / 104 / 226 / 101 / 110 / 199 / 98 / 700 /
//            108 / 105 / 106 / 114 → 本题）
//
//  ⭐ 与前面 15 站的分界线（落笔前先读一遍）：
//
//   ① 【答案不是"树"，而是"顺序里的某一个位置"】
//      前 15 站的输出要么是一个数、要么是树/链表的形态；本题的答案是
//      **"中序序列里的第 k 个"** ⇒ 判题器的参照物第一次变成"一条序列"。
//
//   ② 【BST 的性质第一次被"用来省事"，而不只是"用来判定"】
//      98 是**验**它（这棵树是不是 BST）、700 / 108 是**用**它（比大小决定走哪支），
//      本题是**榨**它：中序 == 升序 ⇒ "第 k 小" == "中序里数到第 k 个"。
//      ⭐ 于是本题有两条路（先别写代码，先把这两条想清楚）：
//         (a) 中序遍历 + 计数器 —— 不维护额外信息，最坏要把整棵树读完（O(n)）
//         (b) 每个节点额外存"左子树大小 size" —— 站上根就能一步砍掉一整支（O(h)）
//      ⚠️ (b) 要求**改 TreeNode 的定义**（加字段）⇒ 本题的 TreeNode 是题目定死的
//         ⇒ (b) 只能作为**讨论 / Follow-up**，不是本题的验收项。
//
//   ③ 【本题第一次出现"提前走人"的诱惑】
//      中序读到第 k 个就能停 —— 但"停"分两层：**停止计数**很容易，
//      **停止整棵树的遍历**才麻烦（递归版里"找到之后要不要往上层层退出"）。
//      ⚠️ 这也是本题最容易**假绿**的地方：读完再做第 k 个，全部用例照样过
//         ⇒ 判题器必须用**性能区**把 k=1 与 k=n 的差别照出来。
//
//   ④ 【独立裁判（承 114「每次现场重新造树」的纪律）】
//      KthSmallest 按定义**只读**（不许改树）⇒ 判题器要能证明它真没改：
//        · ②-A 调用前后 `Show(root)` 一字不差
//        · ②-B 同一棵树连问两次，答案必须一致（幂等）
//        · ②-C k 从 1 扫到 n，得到的序列必须 == 中序、且严格递增
//      ⇒ 用例表存的是**建树用的层序串**（不是实例），每次判题现场造一棵新的。
// ══════════════════════════════════════════════════════════════════════════════

// ───────────────────────── 工具（第十六次复用） ─────────────────────────

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

// 中序收集 —— 本题的"另一条路"（把 BST 读成升序数组）；参考实现的底盘
List<int> Inorder(TreeNode? root)
{
    List<int> result = new List<int>();
    void Walk(TreeNode? node)
    {
        if (node is null) return;
        Walk(node.left);
        result.Add(node.val);
        Walk(node.right);
    }
    Walk(root);
    return result;
}

// 参考实现：读完中序再取第 k 个 —— **故意不提前停**，它就是性能区的对照组
int BaselineKth(TreeNode? root, int k) => Inorder(root)[k - 1];

int CountNodes(TreeNode? root) => root is null ? 0 : 1 + CountNodes(root.left) + CountNodes(root.right);

bool IsStrictlyIncreasing(List<int> xs)
{
    for (int i = 1; i < xs.Count; i++) if (xs[i] <= xs[i - 1]) return false;
    return true;
}

// 合法 BST 的"里子"判据：中序严格递增（元素互不相同 = 本题"答案唯一"的前提）
bool IsValidBst(TreeNode? root) => IsStrictlyIncreasing(Inorder(root));

// 平衡 BST（值 = 中序下标 1..n）—— 性能区的大树；这里递归深度只有 log2(n)，安全
TreeNode BuildBalanced(int lo, int hi)
{
    if (lo > hi) return null!;
    int mid = lo + (hi - lo) / 2;
    return new TreeNode(mid, BuildBalanced(lo, mid - 1), BuildBalanced(mid + 1, hi));
}

// 左斜链：root = n，一路 .left 走到 1 ⇒ 中序 = 1..n，**高度 = n**（递归深度的尺子）
TreeNode BuildLeftChain(int n)
{
    TreeNode root = new TreeNode(n);
    TreeNode cur = root;
    for (int v = n - 1; v >= 1; v--) { cur.left = new TreeNode(v); cur = cur.left; }
    return root;
}

// 统一调用口：把"没实现 / 抛异常 / 真的算错"分成三笔账
(int value, string status) TryKth(Solution sol, TreeNode? root, int k)
{
    try { return (sol.KthSmallest(root, k), "OK"); }
    catch (NotImplementedException) { return (0, "尚未实现"); }
    catch (Exception e) { return (0, "抛异常:" + e.GetType().Name); }
}

// 性能测量：把"造树 + 调用"一起测，再用 buildOnly = true 单独测造树，两者相减 = 净值
//   ⭐ 参数是「函数」而不是 Solution ⇒ 同一个测量器既能量你的实现，也能量对照组 Baseline
(double ms, long sink) Measure(Func<TreeNode, int, int> act, int n, int k, int repeat, bool buildOnly)
{
    long sink = 0;
    Stopwatch sw = Stopwatch.StartNew();
    for (int r = 0; r < repeat; r++)
    {
        TreeNode root = BuildBalanced(1, n);
        if (!buildOnly) sink += act(root, k);
    }
    sw.Stop();
    return (sw.Elapsed.TotalMilliseconds, sink);
}

// ══════════════════════════════ 顶层流程 ══════════════════════════════

// ── 用例表：存的是「建树串」，不是实例（承 114 的纪律）──
//    期望值来自：官方示例 2 组 + 边界手推（单节点 / 左斜链 / 右斜链 / 满树中段）
string[] trees =
{
    "[3, 1, 4, null, 2]",                 // 官方示例 1
    "[3, 1, 4, null, 2]",                 // 同树，取最大
    "[5, 3, 6, 2, 4, null, null, 1]",     // 官方示例 2
    "[5, 3, 6, 2, 4, null, null, 1]",     // 同树，取最小
    "[1]",                                // 单节点边界
    "[3, 2, null, 1]",                    // 左斜链（高度 3）
    "[1, null, 2, null, 3]",              // 右斜链（高度 3）
    "[4, 2, 6, 1, 3, 5, 7]",              // 满树 → 中段
    "[4, 2, 6, 1, 3, 5, 7]",              // 满树 → 最大
    "[40, 20, 60, 10, 30, 50, 70]",       // 满树 → 中段（值不是 1..7，防"答案碰巧等于下标"）
    "[10, 5, 15, 3, 7, 12, 17]",          // 满树 → 中段
};
int[] ks = { 1, 4, 3, 1, 1, 3, 1, 4, 7, 5, 6 };
int[] expected = { 1, 4, 3, 1, 1, 3, 1, 4, 7, 50, 15 };

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

Console.WriteLine("════ ⓪-B 前提自检（用例是不是合法 BST + k 是否在 [1, n]）════");
int premisePass = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    int n = CountNodes(t);
    bool bstOk = IsValidBst(t);
    bool kOk = ks[i] >= 1 && ks[i] <= n;
    if (bstOk && kOk) premisePass++;
    Console.WriteLine($"   组 {i + 1,2}   合法BST={(bstOk ? "✅" : "❌")}   节点数={n,2}   k={ks[i],2}   k在范围内={(kOk ? "✅" : "❌")}");
}
Console.WriteLine($"   ⇒ {premisePass}/{trees.Length}"
    + (premisePass == trees.Length
        ? " 全绿（「中序第 k 个」这条命就吊在「合法 BST + 元素互不相同」上）"
        : " ⚠️ 有非法用例，先修用例"));
Console.WriteLine();

Solution sol = new Solution();

Console.WriteLine("════ 判题器① 期望值比对（每次现场重新造树）════");
int pass1 = 0, notImpl = 0, mism = 0, exCount = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    (int value, string status) r = TryKth(sol, t, ks[i]);
    bool ok = r.status == "OK" && r.value == expected[i];
    if (ok) pass1++;
    else if (r.status == "尚未实现") notImpl++;
    else if (r.status == "OK") mism++;
    else exCount++;
    string got = r.status == "OK" ? r.value.ToString() : r.status;
    Console.WriteLine($"   组 {i + 1,2}   k={ks[i],2}   期望 {expected[i],4}   实测 {got,8}   {(ok ? "✅" : "❌")}");
}
Console.WriteLine($"   ⇒ {pass1}/{trees.Length}    （值不符 {mism} 组、尚未实现 {notImpl} 组、抛异常 {exCount} 组）");
Console.WriteLine();

if (notImpl > 0)
{
    Console.WriteLine("   ⛔ 还有用例没实现 ⇒ 判题器② / 性能区 / 阶梯区先不跑。");
    Console.WriteLine("      （先把 `KthSmallest` 写出来，回来看这条流水线怎么抓你。）");
    return;
}

Console.WriteLine("════ 判题器② 独立裁判（没改树 / 幂等 / k 扫描 == 中序）════");
int intactPass = 0, idemPass = 0, scanPass = 0;
for (int i = 0; i < trees.Length; i++)
{
    TreeNode? t = Parse(trees[i]);
    string before = Show(t);

    (int v1, string s1) = TryKth(sol, t, ks[i]);
    string after = Show(t);
    (int v2, string s2) = TryKth(sol, t, ks[i]);

    bool intact = before == after;
    bool idem = s1 == "OK" && s2 == "OK" && v1 == v2;
    if (intact) intactPass++;
    if (idem) idemPass++;

    int n = CountNodes(t);
    List<int> inorderRef = Inorder(t);
    List<int> byK = new List<int>();
    bool scanOk = true;
    for (int k = 1; k <= n; k++)
    {
        (int v, string s) = TryKth(sol, t, k);
        if (s != "OK") { scanOk = false; break; }
        byK.Add(v);
    }
    scanOk = scanOk && byK.Count == n && IsStrictlyIncreasing(byK) && byK.SequenceEqual(inorderRef);
    if (scanOk) scanPass++;

    Console.WriteLine($"   组 {i + 1,2}   没改树 {(intact ? "✅" : "❌")}   幂等 {(idem ? "✅" : "❌")}   k=1..{n} 扫描 == 中序 {(scanOk ? "✅" : "❌")}");
    if (!scanOk)
    {
        for (int k2 = 0; k2 < byK.Count && k2 < inorderRef.Count; k2++)
        {
            if (byK[k2] != inorderRef[k2])
            {
                Console.WriteLine($"        ↳ 第一个不一致：k = {k2 + 1}，期望 {inorderRef[k2]}，实测 {byK[k2]}");
                break;
            }
        }
    }
}
Console.WriteLine($"   ⇒ 没改树 {intactPass}/{trees.Length}　幂等 {idemPass}/{trees.Length}　k 扫描 {scanPass}/{trees.Length}");
Console.WriteLine();

Console.WriteLine("════ 性能区（n = 100,000 平衡 BST；扣掉「建树」基线取净值）════");
const int N = 100_000;
const int Repeat = 20;

double baseMs = Measure((r, kk) => 0, N, 0, Repeat, true).ms;
(double ms1, long sink1) = Measure((r, kk) => sol.KthSmallest(r, kk), N, 1, Repeat, false);
(double msH, long sinkH) = Measure((r, kk) => sol.KthSmallest(r, kk), N, N / 2, Repeat, false);
(double msN, long sinkN) = Measure((r, kk) => sol.KthSmallest(r, kk), N, N, Repeat, false);

// 对照组：Baseline = 中序**全读完**再取第 k 个（故意不提前停）
(double msB1, long sinkB1) = Measure(BaselineKth, N, 1, Repeat, false);
(double msBN, long sinkBN) = Measure(BaselineKth, N, N, Repeat, false);

double net1 = Math.Max(0.0, ms1 - baseMs) / Repeat;
double netH = Math.Max(0.0, msH - baseMs) / Repeat;
double netN = Math.Max(0.0, msN - baseMs) / Repeat;
double netB1 = Math.Max(0.0, msB1 - baseMs) / Repeat;
double netBN = Math.Max(0.0, msBN - baseMs) / Repeat;

Console.WriteLine($"   建树基线：{baseMs,8:F2} ms（{Repeat} 次）；单次造树 ≈ {baseMs / Repeat:F3} ms");
Console.WriteLine($"   你的实现     k = 1     总 {ms1,8:F2} ms   ⇒ 单次净值 {net1:F4} ms");
Console.WriteLine($"   你的实现     k = n/2   总 {msH,8:F2} ms   ⇒ 单次净值 {netH:F4} ms");
Console.WriteLine($"   你的实现     k = n     总 {msN,8:F2} ms   ⇒ 单次净值 {netN:F4} ms");
Console.WriteLine($"   【对照组】Baseline（读完中序再取，不提前停）：");
Console.WriteLine($"                k = 1     净值 {netB1:F4} ms");
Console.WriteLine($"                k = n     净值 {netBN:F4} ms   ⇒ 两笔应当**几乎一样**（它压根不提前停）");
Console.WriteLine($"   （sink = {sink1 + sinkH + sinkN + sinkB1 + sinkBN}；打印它只为防 JIT 把调用优化掉）");
Console.WriteLine();
Console.WriteLine("   ⭐ 三笔账要回答的三个问题：");
Console.WriteLine("      ① k=1 与 k=n 的净值差几倍？**只有差很多**才说明「提前走人」真的生效了；");
Console.WriteLine("         若三笔几乎一样 ⇒ 你写的是 Baseline（读完才取第 k 个）。");
Console.WriteLine("      ② 差的那部分时间，是被哪一步吃掉的？（提示：问「k 变小时，树上的哪一段被省掉了」）");
Console.WriteLine("      ③ 净值随 k 增长是线性的吗？（k = n/2 → n，规模翻倍，净值该翻几倍）");
Console.WriteLine();

Console.WriteLine("════ 阶梯区：左斜链的递归深度（⚠️ 这一区可能让进程**直接消失**）════");
Console.WriteLine("   递归版在这里的深度 = 树高 = n ⇒ 每翻一倍，栈就多啃一倍。");
Console.WriteLine("   ⚠️ 若下面某一档之后**再无输出**，那不是报错，是栈溢出（.NET 抓不住，进程当场死）。");
Console.WriteLine();
int[] steps = { 1000, 2000, 4000, 8000, 16000, 32000 };
foreach (int n in steps)
{
    Console.WriteLine($"   → 左斜链 n = {n,6}，求第 {n} 小（必须走到底，最坏情况）…");
    Console.Out.Flush();
    TreeNode chain = BuildLeftChain(n);
    int got = sol.KthSmallest(chain, n);
    Console.WriteLine($"     结果 = {got}（期望 {n}）{(got == n ? " ✅" : " ❌")}");
    Console.Out.Flush();
}
Console.WriteLine("   ⭐ 尺子怎么读：**哪一档之后没有下一行输出，那一档就是栈顶**（默认线程栈 ≈ 1MB）。");
Console.WriteLine("      递归版在这里的深度 = 树高 = n ⇒ 这棵**完全合法**的 BST 会先于算法要你的命。");
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
    // TODO: 返回这棵 BST 中**第 k 小**的元素（题目保证 1 ≤ k ≤ 节点数）。
    //
    //   ── 抓题眼（先回答这三问，再落笔）：
    //      ① BST 身上只有一条性质跟"顺序"有关：**中序遍历 = 升序**
    //         （承 98："中序 = 升序"就是验 BST 的**里子**）⇒ 第 k 小 == 中序里数到第 k 个。
    //      ② ⭐ "能砍掉哪一支？" —— 站在一个节点上，你要先拿到**什么信息**，
    //         才能连碰都不碰地跳过一整棵子树？
    //         （提示：若知道"左子树里有几个节点"，这个节点自己是第几小？）
    //         ⚠️ 再追一步：这个信息**现在拿得到吗**？（TreeNode 只有 val / left / right）
    //            拿不到 ⇒ 你手上**没有资格**跳支，只能数着走。跳支要付什么代价？
    //            （这就是题目的 Follow-up：给节点加 size 字段 —— 只能讨论，别改本题的 TreeNode）
    //      ③ 既然只能数着走：要不要把整棵树读完？**什么情况下可以提前走人**？
    //
    //   ── 落笔前必须回答的六个问题：
    //      ① 递归版里，计数器该放在中序的**哪一步**（进左 / 读 val / 进右）？
    //      ② 数到第 k 个之后，答案怎么**带出递归**？（签名返回 int、不是 int? ——
    //         "还没找到"这个状态用什么表达，才不会跟合法答案撞车？）
    //      ③ ⚠️ `return` 只能跳出**当前这一层** ⇒ 找到之后，上面那几层还继续遍历吗？
    //         若还在走，你的"提前停"是不是只停了计数、根本没省时间？（性能区就是照这个的）
    //      ④ 空树 / 单节点：题目保证 1 ≤ k ≤ n ⇒ 你到底要不要写 `root == null` 的特判？
    //         （先想清"合法输入里有没有空树"，再决定写不写。）
    //      ⑤ 左斜链 n = 8000 时，递归版的**深度**是多少？（承 108 的三轮栈溢出 —— 深度 = 树高）
    //         ⚠️ 本题在最坏形状上，树高 = n ⇒ 一棵**完全合法**的 BST 会直接要你的命。
    //      ⑥ ⭐ 迭代版（显式栈）：空间怎么做到 O(h)？
    //         （🪞 镜子：你自己 10-11 写的 104 显式栈版；"用栈模拟递归的展开过程"是同一套手法）
    //
    //   ── 写完自己回答（这才是今天的账）：
    //      · 复杂度：时间 O(?)、空间 O(?)；"最好情况"和"最坏情况"分别是哪种 k、哪种树形？
    //      · ⭐ 性能区给的三笔账差几倍？差的那部分时间被哪一步吃掉了？
    //      · 为什么"跳支"（Follow-up）能把定位降到 O(h)？它买来的速度，花的是什么代价？
    //      · 为什么判题器②要检查"调用前后树的层序一字不差"？
    //        （提示：第 k 小跟你**改树**本来是两件事 —— 但有人会想用"就地展开成链"来取第 k 个。）
    //
    //   ── 验收标准：
    //      ① ⓪-A / ⓪-B 全绿（尺子可信 + 用例合法）；
    //      ② 判题器① 11/11 全绿；
    //      ③ 判题器② 三项全绿（没改树 / 幂等 / k 扫描 == 中序）；
    //      ④ 性能区能一句话解释 k=1 与 k=n 的倍数；
    //      ⑤ 能一句话回答："**在不额外存东西的前提下，本题为什么拿不到 O(h) 时间？**"
    //
    //   ── 进阶（做完一条再说）：
    //      ① 迭代 + 显式栈的中序：空间 O(h)、不递归（对照阶梯区的表现）
    //      ② 莫里斯中序（O(1) 空间）：拿 left 指针当前驱的"临时回头线"，用完还原
    //      ③ Follow-up（面试常问）：节点带 size ⇒ O(h) 定位；再问"频繁改 + 频繁查"怎么权衡
    //      ④ 同族变体：第 k **大**（中序倒过来数）、普通二叉树的中序第 k 个（没有 BST 性质 ⇒ 只能全读）
    private int times;
    private int result;
    private bool isfound;
    public int KthSmallest(TreeNode? root, int k)
    {
        times = 0;
        isfound = false;
        Walk(root,k);
        if(isfound) return result;
        return 0;
    }
    void Walk(TreeNode? root, int k)
    {
        if(root == null || isfound) return;
        Walk(root.left,k);
        times++;
        if(times == k)
        {
            isfound = true;
            result = root.val;
            return;
        }
        Walk(root.right,k);
        return;
    }
}
