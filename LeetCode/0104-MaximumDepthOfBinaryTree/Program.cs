// ═══════ 建树与判题工具（144 / 94 / 145 / 102 同一套，第五次复用）═══════
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

// 判题器：这次期望值是一个【整数】——深度的错法通常只差 1 格，所以失败时必须把树打印出来，
// 否则"差一格"你根本看不出是哪一支算错了。
static void Check(string title, int?[] input, int expected)
{
    TreeNode? root = Build(input);
    int actual = new Solution().MaxDepth(root);
    bool ok = actual == expected;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望深度：{expected}   实际：{actual}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？**下限是不是 0**（= 空树合法）——直接决定"空树返回 0 还是 1"
//   · 深度怎么定义？**节点数**还是**边数**？（本题是节点数：只有根 = 1）
//   · ⚠️ 我抄的数不一定准：**以网页题面为准**（"别信记忆、看题面"是硬纪律）
//
// ═══════ ★ 今天要把 145 的伏笔收掉：什么叫"自底向上" ═══════
//   145（后序）那天记过一句：**后序天生适合"自底向上"的问题**（求树高 / 算子树大小 / 释放资源）。
//   今天就是那类问题的第一题 —— 你落笔前先自己回答：
//     · "以我为根的那棵子树的深度" = 我孩子的答案 + ？→ **谁必须先算完，我才能算**？
//     · 这个依赖顺序，正好是哪一种遍历的访问顺序？（前序 / 中序 / 后序）
//   ⭐ 判据：**当"我的答案需要孩子的答案"时，你必须用后序的那种顺序思考** ——
//      递归调用写在前面，自己的合并动作写在后面。
//
// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：不对称，最深层在右支的延伸上", new int?[] { 3, 9, 20, null, null, 15, 7 }, 3);
Check("★★ 官方示例 2：两条腿长度不等（右比左长）", new int?[] { 1, null, 2 }, 2);

Check("★ 空树 —— 返回 0（递归终止条件的第一道门）", new int?[] { }, 0);
Check("★ 单节点（最小规模，也是「深度定义」的锚点）", new int?[] { 1 }, 1);
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 }, 2);
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 }, 2);
Check("★★ 满二叉树（三层）", new int?[] { 1, 2, 3, 4, 5, 6, 7 }, 3);
Check("★★ 满二叉树（四层 15 节点，深度 = log₂(n+1) 的对照点）",
      new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4);
Check("★★ 全左的斜链（深度 = 节点数，递归栈最费）", new int?[] { 1, 2, null, 3, null, 4 }, 4);
Check("★★ 全右的斜链（与上组同形、方向相反）", new int?[] { 1, null, 2, null, 3, null, 4 }, 4);
Check("★★★ 最宽层与最深层不在同一条支上（不能只看「层数最多的那层」）",
      new int?[] { 1, 2, 3, null, 4, null, null, 5 }, 4);
Check("★★★ 答案是【右支】给的：左支只有 2 层、右支一路到底（抓「只顾左边」）",
      new int?[] { 1, 2, 3, null, null, null, 4, null, 5 }, 4);

// ═══════ 反例区（专抓三种错法）═══════
//   ⚠️ 下面三条**症状全部是实测出来的**（临时副本各跑一遍 15 组），别凭直觉猜：
//     · 错法 A：只算一条腿（`return 1 + MaxDepth(root.left);`）
//              → 满树 / 单节点**照样绿**（左右一样长时看不出差别）→ 只有"右支更深"的形状才露馅
//     · 错法 B：漏掉 `+1`（`return Math.Max(左, 右);`）
//              → ❗实测：**整棵树恒为 0**，**不是"全局少 1"**！
//                机制：终止条件给 0，而此后**没有任何一层加过东西** → 零 + 零 = 零，树多高都一样
//              → ❗更阴的是**空树那组会假绿**（期望 0、结果也 0）→ 实测结果是 **13 红 2 绿**
//     · 错法 C：终止条件写错（`return 1;` 或边界值取 `-1`）
//              → ① `return 1;`：空树返回 1 → 只有空树那组戳穿
//              → ② `null → -1`：❗实测这才是真正的**"整棵树统一少 1"**（每个节点的答案平移一格），
//                 且空树返回 -1 → 实测 **15 组全红**
//   ⭐ 由此得到两条判据：
//      ① **"漏 +1"和"边界值选错"不是同一种病** —— 前者让答案**恒为 0**（断崖），
//         后者让答案**整体平移**（加性）；能分清这两种，才说明真的懂"返回值是怎么一层层累积起来的"
//      ② **假绿比红更危险**：错法 B 有 2 组是绿的 —— 用例里必须同时有"深度 1"（单节点）
//         和"深度 4"（深树），才能把"恒为 0"和"平移一格"分开，也才不会误以为"至少对了一部分"
Check("⛔ 抓「只算左支」（漏右支）→ 左支只 2 层、右支 4 层（只算左会得到 2）",
      new int?[] { 1, 2, 3, null, null, null, 4, null, 5 }, 4);
Check("⛔ 抓「漏掉 +1」→ 实测整棵树恒为 0（单节点应得 1、漏了得 0）",
      new int?[] { 1 }, 1);
Check("⛔ 抓「终止条件写错」（`return 1` / `null → -1`）→ 空树应得 0",
      new int?[] { }, 0);

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
    // TODO: 返回这棵二叉树的【最大深度】（根到最远叶子路径上的节点数；空树 = 0）。
    //
    //   ── 抓题眼：这题和 144 / 94 / 145 的"遍历"有什么不一样？
    //      前三题是"把每个节点按顺序走一遍"（**过程本身就是答案**）。
    //      今天是"每个节点要交一个**数值结论**给它的父亲"（**答案靠孩子反推**）——
    //      ⭐ 关键句：**谁必须先算完，我才能算？** 想清楚这句，遍历顺序就定了。
    //
    //   ── 三个必须先回答的问题（落笔前想完，别边写边想）：
    //      ① 一个**空节点**（`null`）的"深度"该返回什么？
    //         —— 这个返回值一旦选对，**叶子节点就不需要任何特判**（"递归的优雅来自边界返回值"）。
    //      ② 左右两棵子树深度不同时，**谁说了算**？
    //      ③ 拿到孩子的答案之后，还差哪一步才等于"以我为根的子树的深度"？（想想"我"算不算一层）
    //
    //   ── 自问自答（写完再回来看，别提前抄）：
    //      · 如果把 `+1` 写在**递归调用之前**（`return 1 + ...`），和写在**之后**，
    //        对结果有影响吗？对"代码可读性 + 自底向上的思路"有影响吗？
    //      · 漏 `+1` 时为什么答案会**恒为 0**（而不是"少 1"）？—— 想想整个递归里**唯一**
    //        能产生非零数字的地方在哪；再对比"把边界值取成 -1"为什么才是"整体少 1"
    //
    //   ── 复杂度（自己算一遍）：
    //      时间 O(?)：每个节点被访问几次？
    //      空间：递归栈的层数 = 树高 h → O(?)；⚠️ 斜链时 h = n（和 145 那天量出来的结论对一下）
    //
    //   ── 进阶（102 的回收，做完递归版再写）：
    //      用**层序 BFS** 再写一版 `MaxDepth`：深度 = **层数** → 只要数清"一共处理了几层"。
    //      ⭐ 两版必须给出完全一致的结果（15 组核对）—— 这就是"同一问题两种机制的对照"。
    //      再想一句：BFS 版的空间是 O(?)（提示：和 102 一起量过 —— 答案是**形状决定**的）。
    //
    //   ── 验收标准：
    //      ① 15 组全绿（含空树、两条斜链、满树四层、三组反例）；
    //      ② 反例区三组各自戳穿什么错法，能说出来；
    //      ③ 能用一句话说清"为什么后序遍历天生适合自底向上的问题"（= 145 的伏笔回收）；
    //      ④ 进阶：BFS 版与递归版结果全一致。
    public int MaxDepth1(TreeNode? root)
    {
        int high = 0;
        if(root == null) return high;
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
            }
            high++;
        }
        return high;
    }
    public int MaxDepth(TreeNode? root)
    {
        if(root == null) return 0;
        return Math.Max(MaxDepth(root.left), MaxDepth(root.right)) + 1;
    }
}
