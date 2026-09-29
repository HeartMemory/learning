// ═══════ 建树与判题工具（与 144 同一套，直接复用）═══════
// 力扣给二叉树题的输入是【层序数组】，null = "这个位置没有节点"（不是"节点值为 null"）。
// 建树规则：按层给非空节点补孩子，左先右后。
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

static void Check(string title, int?[] input, int[] expected)
{
    TreeNode? root = Build(input);
    IList<int> actual = new Solution().InorderTraversal(root);
    List<int> a = new List<int>(actual);
    List<int> e = new List<int>(expected);
    bool ok = a.SequenceEqual(e);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望：[{string.Join(", ", e)}]");
        Console.WriteLine($"     实际：[{string.Join(", ", a)}]");
    }
}

// ═══════ 官方提示（开工前顺手核一眼题面「提示」）═══════
//   节点数目在 [0, 100] 内        ← 下限 0：**空树合法**（官方示例 2 就是它）
//   -100 <= Node.val <= 100       ← 有 0 和负数，别拿 0 当"没填"的标记
//   进阶：递归算法很简单，你可以通过迭代算法完成吗？
// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：左空、右有子（斜树）", new int?[] { 1, null, 2, 3 }, new int[] { 1, 3, 2 });
Check("★★ 官方示例 2：空树 —— 返回空列表，不是 null", new int?[] { }, new int[] { });

Check("★ 单节点（最小规模）", new int?[] { 1 }, new int[] { 1 });
Check("★ 两节点：只有左孩子（中序里左先出）", new int?[] { 1, 2 }, new int[] { 2, 1 });
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 }, new int[] { 1, 2 });
Check("★★ 满二叉树（三层，形状完整）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 4, 2, 5, 1, 6, 3, 7 });
Check("★★ 全左的斜链（深度拉满，中序 = 倒序）", new int?[] { 1, 2, null, 3, null, 4 }, new int[] { 4, 3, 2, 1 });
Check("★★ 全右的斜链（中序 = 正序）", new int?[] { 1, null, 2, null, 3, null, 4 }, new int[] { 1, 2, 3, 4 });
Check("★ 含 0 和负数（值域边界）", new int?[] { 0, -1, -2, -3 }, new int[] { -3, -1, 0, -2 });
Check("★★★ 三层不成满：只有左子树里还有分支", new int?[] { 1, 2, 3, 4, null, null, 5 }, new int[] { 4, 2, 1, 3, 5 });

// ═══════ 反例区（专抓"顺序放错位置"）═══════
//   中序 = 左 → 根 → 右。只要"装自己"那一行放错位置就会露馅：
//     · 放在最前 → 输出的是【前序】（144 的答案）
//     · 放在最后 → 输出的是【后序】（明天的 145）
Check("⛔ 抓「前序」错版：装自己在最前，会输出 1,2,4,5,3", new int?[] { 1, 2, 3, 4, 5 }, new int[] { 4, 2, 5, 1, 3 });
Check("⛔ 抓「后序」错版：装自己在最后，满树会输出 4,5,2,6,7,3,1", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 4, 2, 5, 1, 6, 3, 7 });

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
    // TODO: 返回这棵树的【中序遍历】结果（顺序：左 → 根 → 右），用【递归】实现。
    //
    //   ── 先抓题眼：和昨天的 144 差在哪？
    //      差【一行代码的位置】。144 是"先干自己的事，再管左右"；
    //      中序是"先把左子树交出去，**回来之后**再干自己的事，最后管右"。
    //      ⭐ 所以真正的考点是：你能不能想清楚"我这一行的前后，发生了什么"（= 调用栈的进出）。
    //
    //   ── 递归三件套（同上）：
    //      ① 最小情况：节点为空 → 直接 return（**没它必 NRE**，一处盖三入口）
    //      ② 交出去：左子树
    //      ③ 干自己的事：把 root.val 装进结果（★ 就是这一行要挪位置）
    //      ④ 交出去：右子树
    //
    //   ── 落笔前想清楚：
    //      ① 结果集怎么传？（同 144：外层备容器，内层 void + 参数）
    //      ② 为什么"先左后自己"能让整棵树按从小到大吐出来？
    //         （见下面的 ★ 不变量）
    //
    //   ── 复杂度：每个节点访问一次 → O(n) 时间；递归深度 = 树高 h（斜链最坏 h = n）。
    //
    //   ── ★ 中序的"超能力"（今天必须记住，为 10-08/09/10 的 BST 三题埋伏笔）：
    //      对【二叉搜索树 BST】做中序遍历，输出【必然升序】。
    //      因为 BST 的定义就是"左子树全比我小、右子树全比我大"——
    //      而中序恰好按"小 → 我 → 大"的顺序访问。以后看到"验证 BST"，先想这条。
    //      ⚠️ 反过来不成立：普通二叉树中序升序 ≠ 它是 BST（等 98 题再细究）
    //
    //   ── ★ 进阶（题面明确要求，比 144 的迭代版更难）：
    //      用【显式栈】做中序。难点：不能像前序那样"压进去就访问"——
    //      必须**一路向左压到底**，压不动了（null）才弹一个出来访问，然后转向它的右子树。
    //      写成自然语言就是："能往左就往左，不能往左就吐一个，再往右。"
    //
    //   ── 验收标准：
    //      ① 12 组全绿；② 反例区那两组的含义你能说出来（前序/后序错版分别长什么样）；
    //      ③ 迭代版也全绿，并能用"能左就往左，不能左就吐一个，再往右"复述它。
    public IList<int> InorderTraversal(TreeNode? root) {
        List<int> result = new List<int>();
        Walk(root, result);
        return result;
    }

    private void Walk(TreeNode? node, List<int> result) {
        if(node == null) return;
        Walk(node.left, result);
        result.Add(node.val);
        Walk(node.right, result);
    }
}
