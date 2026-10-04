// ═══════ 建树与判题工具（与 144 / 94 同一套，第三次复用）═══════
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
    List<int> e = new List<int>(expected);

    List<int> a = new List<int>(new Solution().PostorderTraversal(root));   // 递归版（09-30）

    // 10-04 加：两条迭代路线各跑一遍，和递归版**三版互证**
    List<int> b = new List<int>();
    string tagB = "⬜未实现";
    try
    {
        b = new List<int>(new Solution().PostorderTraversalIterativeB(root));
        tagB = b.SequenceEqual(e) ? "✅" : "❌";
    }
    catch (NotImplementedException) { }

    List<int> c = new List<int>();
    string tagA = "⬜未实现";
    try
    {
        c = new List<int>(new Solution().PostorderTraversalIterativeA(root));
        tagA = c.SequenceEqual(e) ? "✅" : "❌";
    }
    catch (NotImplementedException) { }

    bool ok = a.SequenceEqual(e);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}   〔路线B 逆序：{tagB}｜路线A 标记：{tagA}〕");
    if (!ok || tagB == "❌" || tagA == "❌")
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望：  [{string.Join(", ", e)}]");
        if (!ok) Console.WriteLine($"     递归版：[{string.Join(", ", a)}]");
        if (tagB == "❌") Console.WriteLine($"     路线B ：[{string.Join(", ", b)}]");
        if (tagA == "❌") Console.WriteLine($"     路线A ：[{string.Join(", ", c)}]");
    }
}

// ═══════ 官方提示（开工前顺手核一眼题面「提示」）═══════
//   节点数目在 [0, 100] 内        ← 下限 0：**空树合法**（官方示例 2）
//   -100 <= Node.val <= 100
//   进阶：递归算法很简单，你可以通过迭代算法完成吗？
//
// ═══════ ★ 遍历三连收官：同一棵树，三种顺序 ═══════
//   以满树 [1,2,3,4,5,6,7] 为例（这三行建议抄进笔记，一眼看清区别）：
//     前序（144）根→左→右： 1, 2, 4, 5, 3, 6, 7     ← "先干自己的事"
//     中序（94） 左→根→右： 4, 2, 5, 1, 6, 3, 7     ← "两棵子树中间插自己"
//     后序（145）左→右→根： 4, 5, 2, 6, 7, 3, 1     ← "收尾才轮到自己"
//   ⭐ 三者的代码只差"装自己"那一行的**位置**——今天你就该能一眼看出来了。
// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：左空、右有子（斜树）", new int?[] { 1, null, 2, 3 }, new int[] { 3, 2, 1 });
Check("★★ 官方示例 2：空树 —— 返回空列表，不是 null", new int?[] { }, new int[] { });

Check("★ 单节点（最小规模）", new int?[] { 1 }, new int[] { 1 });
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 }, new int[] { 2, 1 });
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 }, new int[] { 2, 1 });
Check("★★ 满二叉树（三层，形状完整）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 4, 5, 2, 6, 7, 3, 1 });
Check("★★ 全左的斜链（后序 = 倒序）", new int?[] { 1, 2, null, 3, null, 4 }, new int[] { 4, 3, 2, 1 });
Check("★★★ 全右的斜链（⚠️ 后序【也是】倒序，不像中序那样是「正序」）", new int?[] { 1, null, 2, null, 3, null, 4 }, new int[] { 4, 3, 2, 1 });
Check("★ 含 0 和负数（值域边界）", new int?[] { 0, -1, -2, -3 }, new int[] { -3, -1, -2, 0 });
Check("★★★ 三层不成满：只有左子树里还有分支", new int?[] { 1, 2, 3, 4, null, null, 5 }, new int[] { 4, 2, 5, 3, 1 });

// ═══════ 反例区（专抓"顺序放错位置"）═══════
//   后序 = 左 → 右 → 根。把"装自己"放在别处就会露馅：
//     · 放最前 → 输出【前序】；放中间 → 输出【中序】
Check("⛔ 抓「前序」错版（满树会输出 1,2,4,5,3,6,7）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 4, 5, 2, 6, 7, 3, 1 });
Check("⛔ 抓「中序」错版（满树会输出 4,2,5,1,6,3,7）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int[] { 4, 5, 2, 6, 7, 3, 1 });

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
    // TODO: 返回这棵树的【后序遍历】结果（顺序：左 → 右 → 根），用【递归】实现。
    //
    //   ── 抓题眼：和 144 / 94 差在哪？
    //      只差"装自己"那一行的位置：后序要**等两棵子树都回来之后**才装自己。
    //      ⭐ 所以今天真正要体会的是：**"我这一行放在哪"决定了三种遍历**——
    //         这也解释了后序为什么天生适合"自底向上"的问题（求树高、算子树大小、释放资源…）。
    //
    //   ── 递归三件套（第三次了，应该已经变成肌肉记忆）：
    //      ① 最小情况：节点为空 → 直接 return（**没它必 NRE**；一处盖三个入口）
    //      ② 交出去：左子树
    //      ③ 交出去：右子树
    //      ④ 干自己的事：装 root.val（★ 今天这行要放最后）
    //
    //   ── 复杂度：O(n) 时间；递归深度 = 树高 h（斜链最坏 h = n）。
    //
    //   ── ★ 进阶（题面要求）：
    //      用【显式栈】做后序 —— 这是三种遍历里迭代版**最难**的一个。
    //      原因：后序要"左、右都处理完才处理自己"，而栈只有一个"回退指针"，
    //      你得额外记住"我是从左子树上来的、还是从右子树上来的"。
    //      两条常见路线（先自己想，别急着抄）：
    //        · 路线 A：模拟系统的做法 —— 压栈时带上"标记"，回退时判断该不该输出自己
    //        · 路线 B：**偷懒版** —— 前序是"根左右"，后序是"左右根"；
    //                  如果写成"根右左"，再**整体逆序**，是不是就得到后序了？
    //                  （这条路只需要在 144 的迭代代码上改两个地方 + 一次反转，非常划算）
    //
    //   ── 验收标准：
    //      ① 12 组全绿；② 反例区两组的含义能说出来；③ 能只凭"装自己那一行的位置"复述三种遍历；
    //      ④ （加分）迭代版跑通，并说清你选了路线 A 还是 B、为什么
    public IList<int> PostorderTraversal(TreeNode? root) {
        List<int> result = new List<int>();
        Walk(root, result);
        return result;
    }

    private void Walk(TreeNode? node, List<int> result) {
        if(node == null) return;
        Walk(node.left, result);
        Walk(node.right, result);
        result.Add(node.val);
    }

    // ═══════ 10-04（复盘日）· 盲写靶子①：路线 B —— "偷懒版"（先写这个）═══════
    // TODO: 用【前序的迭代写法】改两个地方 + 一次反转，就得到后序。
    //
    //   ── 推理链（自己想通，别背）：
    //      前序 = 根 → 左 → 右 ； 后序 = 左 → 右 → 根。
    //      问自己两个问题：
    //       ① 把前序里"先左后右"的压栈顺序**对调**，得到的是哪种顺序？（写在纸上：根 ? ?）
    //       ② 把 ① 的结果**整体倒过来**，又是哪种顺序？
    //      ⇒ 所以：抄 144 的循环，改**压栈顺序**，最后 `result.Reverse()`。
    //
    //   ── 一个必须答的细节：**反转应该在哪一步做**——循环里每轮反一次，还是循环外一次？
    //      （从复杂度上论证，不要凭感觉）
    //
    //   ── 纸笔自查：满树 `[1,2,3,4,5,6,7]` 期望 `4,5,2,6,7,3,1`。
    public IList<int> PostorderTraversalIterativeB(TreeNode? root) {
        Stack<int> ints = new Stack<int>();
        List<int> result = new List<int>();
        if(root == null) return result;
        Stack<TreeNode> tree = new Stack<TreeNode>();
        tree.Push(root);
        while(tree.Count > 0)
        {
            TreeNode cur = tree.Pop();
            ints.Push(cur.val);
            if(cur.left != null) tree.Push(cur.left);
            if(cur.right != null) tree.Push(cur.right);
        }
        while(ints.Count > 0)
        {
            result.Add(ints.Pop());
        }
        return result;
    }

    // ═══════ 10-04（复盘日）· 盲写靶子②：路线 A —— "老实版"（压栈带标记）═══════
    // TODO: 真正模拟系统调用栈：**回退时判断"我是从左子树上来的，还是从右子树上来的"**。
    //
    //   ── 核心难点：栈里只有"节点"，可"我回来了"这件事怎么表达？
    //       ⇒ 提示方向：让栈里装的**不只是一个节点**（可以是"节点 + 一个状态"），
    //         或者**再借一个容器**记住"谁已经访问过了"。两条路都行，选一条写。
    //
    //   ── 三个必须答的点（写在注释里，别只在脑子里过）：
    //     ① 第一次来到某个节点 vs **第三次**回到它（左回来了 / 右回来了），
    //        这两种"回来"分别该做什么？（第三次才是"装自己"的时刻 —— 为什么?）
    //     ② 空节点还要不要压栈？（对照 144：那里的判空在哪）
    //     ③ 循环结束时栈里还剩什么？
    //
    //   ── 写完和路线 B 对照：两条路线谁更短？谁更接近"系统真正做的事"？
    //      面试里被追问"不用反转怎么写"，答的就是路线 A。
    public IList<int> PostorderTraversalIterativeA(TreeNode? root) {
        List<int> ints = new List<int>();
        if(root == null) return ints;
        Stack<(TreeNode node, int stage)> tree = new Stack<(TreeNode node, int stage)>();
        tree.Push((root, 0));
        while(tree.Count > 0)
        {
            (TreeNode cur, int times) = tree.Pop();
            if(times == 0)
            {
                tree.Push((cur, 1));
                if(cur.left != null) tree.Push((cur.left, 0)); 
            }else if(times == 1)
            {
                tree.Push((cur, 2));
                if(cur.right != null) tree.Push((cur.right, 0));
            }else if(times == 2)
            {
                ints.Add(cur.val);
            }
        }
        return ints;
    }
}
