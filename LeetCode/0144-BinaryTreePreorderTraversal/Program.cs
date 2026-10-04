// ═══════ 建树与判题工具（二叉树题专用 · 94 / 145 / 102 都能直接复用）═══════
// 力扣给二叉树题的输入是一个【层序数组】，null 表示"这个位置没有节点"（不是"节点值为 null"）。
// 例：[1, null, 2, 3]  表示
//         1
//          \
//           2
//          /
//         3
// 建树规则：按层逐个给"非空节点"补孩子，左边先补、右边后补（所以 3 是 2 的左孩子）。
static TreeNode? Build(int?[] level)
{
    if (level.Length == 0 || level[0] is null) return null;   // 空树
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

// 把建好的树按层序打印回来（只在 ❌ 时打，方便你对照"是不是建错了"）
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
    while (parts.Count > 0 && parts[^1] == "null") parts.RemoveAt(parts.Count - 1);  // 去掉尾部多余 null
    return "[" + string.Join(", ", parts) + "]";
}

// 判题：你实现的是 PreorderTraversal，所以【换实现不用换测试】——递归版、迭代版都走同一套
static void Check(string title, int?[] input, int[] expected)
{
    TreeNode? root = Build(input);
    List<int> e = new List<int>(expected);

    List<int> a = new List<int>(new Solution().PreorderTraversal(root));   // 递归版（09-28）

    // 10-04 加：显式栈迭代版 —— 跑同一套用例，两边都得绿（两版互证）
    List<int> b = new List<int>();
    string iterTag = "⬜未实现";
    try
    {
        b = new List<int>(new Solution().PreorderTraversalIterative(root));
        iterTag = b.SequenceEqual(e) ? "✅" : "❌";
    }
    catch (NotImplementedException) { }

    bool ok = a.SequenceEqual(e);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}   〔迭代版：{iterTag}〕");
    if (!ok || iterTag == "❌")
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望：  [{string.Join(", ", e)}]");
        if (!ok) Console.WriteLine($"     递归版：[{string.Join(", ", a)}]");
        if (iterTag == "❌") Console.WriteLine($"     迭代版：[{string.Join(", ", b)}]");
    }
}

// ═══════ 官方提示（开工前顺手核一眼题面「提示」那一栏）═══════
//   节点数目在 [0, 100] 内        ← ⚠️ 下限是 0：**空树合法**，而且官方示例 2 就是它
//   -100 <= Node.val <= 100       ← 会出现负数和 0，别拿 0 当"没填"的标记
//   进阶：递归算法很简单，你可以通过迭代算法完成吗？
// ═══════ 用例区（形状优先，不是数量优先）═══════
Check("★ 官方示例 1：左空、右有子（斜树）", new int?[] { 1, null, 2, 3 }, new int[] { 1, 2, 3 });
Check("★★ 官方示例 2：空树 —— 返回空列表，不是 null！", new int?[] { }, new int[] { });

Check("★ 单节点（最小规模）", new int?[] { 1 }, new int[] { 1 });
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 }, new int[] { 1, 2 });
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 }, new int[] { 1, 2 });
Check("★★ 满二叉树（三层，形状完整）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 1, 2, 4, 5, 3, 6, 7 });
Check("★★ 全左的斜链（深度拉满，考递归）", new int?[] { 1, 2, null, 3, null, 4 }, new int[] { 1, 2, 3, 4 });
Check("★★ 全右的斜链", new int?[] { 1, null, 2, null, 3, null, 4 }, new int[] { 1, 2, 3, 4 });
Check("★ 含 0 和负数（值域边界，别用 0 当哨兵）", new int?[] { 0, -1, -2, -3 }, new int[] { 0, -1, -3, -2 });
Check("★★★ 三层不成满：只有左子树里还有分支", new int?[] { 1, 2, 3, 4, null, null, 5 }, new int[] { 1, 2, 4, 3, 5 });

// ═══════ 反例区（专抓"顺序错 / 漏分支"的写法）═══════
//   前序 = 根 → 左 → 右。写成"根 → 右 → 左"或"先左后根"都能在前几组蒙对，
//   但在这两组的 ❌ 里会当场露馅（叶子层顺序对不上）。
Check("⛔ 抓「根-右-左」：满树叶子层顺序会反", new int?[] { 1, 2, 3, 4, 5 }, new int[] { 1, 2, 4, 5, 3 });
Check("⛔ 抓「按层输出」：把它当成层序遍历输出层序结果", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 1, 2, 4, 5, 3, 6, 7 });

// ═══════ 类型与工具区 ═══════
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
    // TODO: 返回这棵树的【前序遍历】结果（顺序：根 → 左 → 右），用【递归】实现。
    //
    //   ── 先抓题眼：什么是"前序"？
    //      就是把"访问一个节点"和"处理它的子树"排个先后：**我的事我先做完，再管左右**。
    //      中序（后天的 94）/ 后序（145 的 145）只是把"干根的事"挪到中间 / 最后 —— 一行代码的位置之差。
    //
    //   ── 递归的三件套（今天计基 10 分钟讲的就是它）：
    //      ① 最小情况（终止条件）：节点为空 → 什么都不做（**没有它 = NullReferenceException**）
    //      ② 干自己这层该干的事：把 root.val 收进结果
    //      ③ 把同样的任务交给左右子树（**信任它们能自己搞定**，别在脑子里跟着递归跑）
    //
    //   ── 落笔前想清楚这两处（都会踩）：
    //      ① 结果集怎么跨多次递归调用累积？
    //         （提示：让递归函数【拿一个 List 进来往里装】，别每次调用都 new 一个新的）
    //      ② 递归函数要不要返回值？
    //         · 返回 void + 把 List 当参数传  → 简单直接
    //         · 返回 List 并在上一层拼接      → 每层都在造新 List，想想代价
    //         先写前者，跑通之后自己回答：后者差在哪？
    //
    //   ── 复杂度（写完自己算一遍，明天讲递归树时要对答案）：
    //      每个节点恰好被访问一次 → 时间 O(n)；递归深度 = 树高 h，最坏（斜链）h = n。
    //
    //   ── ★ 进阶（AC 之后做，题面明确要求的那条）：
    //      不用递归，用【显式栈】自己模拟"系统帮我做的事"。
    //      ⚠️ 写之前先想清楚一个问题：栈是**后进先出**，而前序要先访问左 ——
    //         那么左孩子和右孩子，谁该【先压栈】？（想清了再写，写完直接跑上面同一套判题）
    //
    //   ── 验收标准：
    //      ① 12 组全绿（含空树、斜链、含 0/负数）；② 反例区那 2 组的含义你能说出来；
    //      ③ 迭代版也全绿，且能讲出"为什么压栈顺序要反过来"。
    public IList<int> PreorderTraversal(TreeNode? root) {
        List<int> result = new List<int>();
        Walk(root, result);
        return result;
    }

    private void Walk(TreeNode? node, List<int> result) {
        if(node == null) return;
        result.Add(node.val);
        Walk(node.left, result);
        Walk(node.right, result);
    }

    // ═══════ 10-04（复盘日）· 盲写靶子：显式栈迭代版 ═══════
    // TODO: 不递归，用【显式栈】模拟"系统在背后替我做的事"。
    //
    //   ⚠️ 落笔前先用一句话回答（README 清单点名的那句）：
    //       **左右孩子，谁先压栈？为什么？**（栈是后进先出，而前序要**先**访问左）
    //      —— 答不出来就先别写代码，把这一句想明白，代码只剩 5 行。
    //
    //   ── 三个想清楚再写的点：
    //     ① "出栈顺序"就等于"访问顺序"吗？若等于，节点该在**压栈时**还是**出栈时**装进 result？
    //     ② 栈里能装 null 吗？不能 → 压之前得先做什么？（对照：递归版的终止条件写在哪一行）
    //     ③ 空树那组用例（期望空列表）—— 你现在的写法还接得住吗？
    //
    //   ── 纸笔自查（别在脑子里空转）：满树 `[1,2,3,4,5,6,7]` 期望 `1,2,4,5,3,6,7`。
    //      手动画栈的进出（每步记下栈里剩谁、吐出谁），对不上就回去改。
    //
    //   ── 复杂度（写完自己算）：每个节点入栈一次 + 出栈一次 → 时间 O(n)；栈最大容量 = 树高 h。
    //
    //   ── AC 之后回来看这句：递归把"栈"藏在**系统的调用栈**里；这里是把**同一个东西**
    //      搬到台面上自己管 —— 今天计基整理 30min 讲的就是它（见当日复盘）。
    public IList<int> PreorderTraversalIterative(TreeNode? root) {
        List<int> ints = new List<int>();
        if(root == null) return ints;
        Stack<TreeNode> tree = new Stack<TreeNode>();
        tree.Push(root);
        while(tree.Count > 0)
        {
            TreeNode cur = tree.Pop();
            ints.Add(cur.val);
            if(cur.right != null) tree.Push(cur.right);
            if(cur.left != null) tree.Push(cur.left);
        }
        return ints;
    }
}
