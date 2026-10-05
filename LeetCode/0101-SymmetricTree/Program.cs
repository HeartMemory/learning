// ═══════ 建树与序列化工具（144 / 94 / 145 / 102 / 104 / 226 同一套，第七次复用）═══════
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

// 镜像工具：把整棵树**复制着**翻一遍（不动原树）。
//   ⭐ 这是我的「独立裁判」——它和 Solution 是两套各写各的代码。
//      用「树 == 自己翻过来的那棵」这条【定义性质】给答案兜底：
//      哪怕我把期望值抄错了，它也照样抓得住"根本不是对称"的实现。
static TreeNode? Mirror(TreeNode? node)
{
    if (node is null) return null;
    TreeNode copy = new TreeNode(node.val);
    copy.left = Mirror(node.right);
    copy.right = Mirror(node.left);
    return copy;
}

// ═══════ 判题器①：拿期望值比（题面示例用）═══════
static void Check(string title, int?[] level, bool expected)
{
    TreeNode? root = Build(level);
    bool actual = new Solution().IsSymmetric(root);
    bool ok = actual == expected;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：{Show(root)}");
        Console.WriteLine($"     期望：{expected}   实际：{actual}");
    }
}

// ═══════ 判题器②：性质裁判（不靠抄来的期望值）═══════
//   ⭐「轴对称」的定义 = **这棵树和它自己的镜像长得一模一样**。
//      所以：Solution 的答案 必须 等于 (Show(root) == Show(Mirror(root)))，两种形状都要核到。
static void CheckByMirror(string title, int?[] level)
{
    TreeNode? root = Build(level);
    bool bySolution = new Solution().IsSymmetric(root);
    bool byDefinition = Show(root) == Show(Mirror(root));
    bool ok = bySolution == byDefinition;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}（Solution 的答案 ==「树等于自己的镜像」）");
    if (!ok)
    {
        Console.WriteLine($"     输入：   {Show(root)}");
        Console.WriteLine($"     Solution：{bySolution}");
        Console.WriteLine($"     定义（树 == 镜像）：{byDefinition}");
    }
}

// ═══════ 判题器③：递归版 / 迭代版 双版互证 ═══════
//   ⭐ 两套机制独立写错、还能在同一份用例上同时全绿的概率极低 ——
//      这就是验收标准③（迭代版与递归版一致）的自动化。
static void CheckBoth(string title, int?[] level, bool expected)
{
    TreeNode? root = Build(level);
    bool rec = new Solution().IsSymmetric(root);
    bool iter = new Solution().IsSymmetric1(Build(level));
    bool ok = rec == expected && iter == expected;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：   {Show(root)}");
        Console.WriteLine($"     期望：   {expected}");
        Console.WriteLine($"     递归版： {rec}");
        Console.WriteLine($"     迭代版： {iter}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？**下限是不是 0**（= 空树合法）—— 决定空树要不要特判。
//   · 值域有没有负数？（有的话"拿 0 当哨兵"之类的写法会出问题。）
//   · ⚠️ 我抄的数不一定准：**以网页题面为准**（"别信记忆、看题面"是硬纪律）。
//
// ═══════ ★ 今天的题眼：对称 ≠ 每一层数值回文 ═══════
//   下面两个"陷阱组"会把你脑子里最省事的那条路(逐层收集 + 判回文)当场戳穿：
//     · 陷阱 A `[1,2,2,2,null,2]` ：第 2 层读作 [2, null, 2]，肉眼完全对称 —— 但**结构不对称**。
//     · 陷阱 B `[1,2,2,3,null,3,null]`：第 2 层读作 [3, null, 3]，同样肉眼对称 —— **结构不对称**。
//   两者真实答案都是 false。所以：
//     ⭐ 对称要**成对地比**：把"左半棵树"和"右半棵树的镜像"对齐，
//        要么用**两只手同时往下走**（两个参数），要么用**成对的容器**。
//   ⚠️ 先自己在纸上画一遍陷阱 A，把"肉眼为什么被骗"写下来，再落笔。
//
// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：满树三层（左支 3,4 ／ 右支 4,3）", new int?[] { 1, 2, 2, 3, 4, 4, 3 }, true);
Check("★ 最小对称：根 + 两个同值孩子", new int?[] { 1, 2, 2 }, true);
Check("★ 单节点 → 天然对称（连「两只手」都比不出东西）", new int?[] { 1 }, true);
Check("★ 空树 → 对称（终止条件的第一道门）", new int?[] { }, true);
Check("★★ 内外侧空档：值 3 都挂在内侧（null-3 / 3-null 才叫镜像）", new int?[] { 1, 2, 2, null, 3, 3, null }, true);
Check("★★ 两支深度不等：左支深一层（3 挂左边）、右支浅但方向相反（3 挂右边）",
      new int?[] { 1, 2, 2, 3, null, null, 3 }, true);

Check("★★ 官方示例 2：null-3 / null-3 —— **同一侧**都有孩子 → 不是镜像", new int?[] { 1, 2, 2, null, 3, null, 3 }, false);
Check("★ 值不等：1 的左右是 2 和 3", new int?[] { 1, 2, 3 }, false);
Check("★ 只有左孩子 → 缺了右边那一半", new int?[] { 1, 2 }, false);
Check("★ 只有右孩子", new int?[] { 1, null, 2 }, false);
Check("★★ 深层值不等（第三层 4 / 5 对不上）", new int?[] { 1, 2, 2, 3, 4, 4, 5 }, false);
Check("⛔ 陷阱 A：每层肉眼像回文，但结构不对称 → 必须 false（逐层回文法在这里假绿）",
      new int?[] { 1, 2, 2, 2, null, 2 }, false);
Check("⛔ 陷阱 B：3 全挂在同侧（[3,null,3] 是假对称）→ 必须 false",
      new int?[] { 1, 2, 2, 3, null, 3, null }, false);

// ═══════ 性质裁判区：用"树 == 自己的镜像"这个定义再交叉验一遍 ═══════
//   ⭐ 这一区不抄任何期望值，纯靠定义兜底 —— 和上面的"抄示例"是两条独立的腿。
CheckByMirror("★ 官方示例 1", new int?[] { 1, 2, 2, 3, 4, 4, 3 });
CheckByMirror("★ 官方示例 2", new int?[] { 1, 2, 2, null, 3, null, 3 });
CheckByMirror("★ 空树", new int?[] { });
CheckByMirror("★ 单节点", new int?[] { 1 });
CheckByMirror("★ 只有左孩子", new int?[] { 1, 2 });
CheckByMirror("★★ 左斜链 [1,2,null,3,null,4]（永远不对称）", new int?[] { 1, 2, null, 3, null, 4 });
CheckByMirror("⛔ 陷阱 A", new int?[] { 1, 2, 2, 2, null, 2 });
CheckByMirror("⛔ 陷阱 B", new int?[] { 1, 2, 2, 3, null, 3, null });
CheckByMirror("★★ 满树四层（每层都对称）",
      new int?[] { 1, 2, 2, 3, 4, 4, 3, 5, 6, 7, 8, 8, 7, 6, 5 });
CheckByMirror("★★ 满树四层但最底层两两对调（只差一个叶子就崩）",
      new int?[] { 1, 2, 2, 3, 4, 4, 3, 5, 6, 7, 8, 8, 7, 6, 6 });

// ═══════ 迭代版对照区（承 102：同一问题、换推进机制）═══════
//   ⭐ 接线口径：迭代版方法名固定 `IsSymmetric1`、返回 `bool`（与 226 的 `InvertTree1` 同风格）。
//      这一区会**同时跑两版**，每组里两版都必须等于期望值。
CheckBoth("★ 官方示例 1", new int?[] { 1, 2, 2, 3, 4, 4, 3 }, true);
CheckBoth("★ 官方示例 2", new int?[] { 1, 2, 2, null, 3, null, 3 }, false);
CheckBoth("★ 空树（两版都要原样答 true）", new int?[] { }, true);
CheckBoth("★ 单节点", new int?[] { 1 }, true);
CheckBoth("★★ 两支深度不等", new int?[] { 1, 2, 2, 3, null, null, 3 }, true);
CheckBoth("⛔ 陷阱 A（迭代时最容易「忘了成对地对齐」）", new int?[] { 1, 2, 2, 2, null, 2 }, false);
CheckBoth("⛔ 陷阱 B", new int?[] { 1, 2, 2, 3, null, 3, null }, false);

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
    // TODO: 判断二叉树是否轴对称 —— 对称返回 true，否则 false。
    //
    //   ── 抓题眼（先想清楚再落笔）：
    //      对称**不是**"每一层的数值读起来是回文"（陷阱 A / B 会戳穿）。
    //      真正的定义是：**左半棵树 == 右半棵树的镜像**。
    //      ⭐ 所以比较必须**成对**进行，而且这一对是**交叉**的：
    //          左孩子的【左】  ↔  右孩子的【右】
    //          左孩子的【右】  ↔  右孩子的【左】
    //      （一旦记成"左的左 ↔ 右的左"，陷阱 B 那种同侧挂孩子就会被你放过。）
    //
    //   ── 落笔前必须回答的三个问题：
    //      ① 你的"两只手"分别是哪两个节点？（它们一开始是谁和谁？）
    //      ② 终止条件有**几种**情况？想象"一只手是 null、另一只手不是"会怎样 ——
    //         是两个都 null 才算通过，还是任意一个 null 都通过？
    //      ③ 每次递归要往下发**几对**比较？发的是哪两对？
    //
    //   ── 自问自答（写完再回来看）：
    //      · 为什么"逐层收集 + 判回文"会在陷阱 A / B 上假绿？（把两题的形状画出来对）
    //      · 如果题目改成"判断两棵树是否互为镜像"，代码要改几行？（⭐ 这题的 helper 就是这个版本）
    //      · 递归的两处调用，能不能交换顺序？结果一样吗？（对照 226：什么时候顺序被钉死）
    //
    //   ── 复杂度（自己算一遍）：
    //      时间 O(?)：最坏要碰到几个节点？提前 false 的情况能省多少？
    //      空间：递归栈深 = ? → O(?)
    //
    //   ── 进阶（做完递归版再写）：
    //      ① 用**队列**写迭代版 `IsSymmetric1`：⭐ 关键差异 —— 入队时**成对入队**、出队时**成对出队**，
    //         判断完再把"下一对"的两个孩子**交叉着**排进去（承 102 的队列，但用法完全不同）。
    //      ② 或者用 `Stack<TreeNode?>`（DFS 迭代）—— 队和栈哪个更自然？说说理由。
    //      ③ 两版结果必须完全一致（判题器③会自动核对）。
    //
    //   ── 验收标准：
    //      ① 全绿（含空树 / 两条斜链 / 两个陷阱组 / 两种深度不等 / 一组"只差一个叶子"）；
    //      ② 性质裁判区（树 == 自己的镜像）全绿；
    //      ③ 迭代版与递归版结果一致；
    //      ④ 能一句话说清"为什么不能只做一次层序遍历判回文"。
    public bool IsSymmetric(TreeNode? root)
    {
        if(root == null) return true;
        return IsMirror(root.left,root.right);
    }

    private bool IsMirror(TreeNode? a, TreeNode? b)
    {
        if(a == null && b == null) return true;
        if(a == null || b == null) return false;
        if(a.val != b.val) return false;
        return IsMirror(a.left,b.right) && IsMirror(a.right,b.left);
    }

    // 进阶：迭代版（队列成对处理）。骨架先留空，写完递归版再来。
    public bool IsSymmetric1(TreeNode? root)
    {
        if(root == null) return true;
        if(root.left == null && root.right == null) return true;
        if(root.left == null || root.right == null) return false;
        if(root.left.val != root.right.val) return false;
        Queue<TreeNode> trees = new Queue<TreeNode>();
        trees.Enqueue(root.left);
        trees.Enqueue(root.right);
        while (trees.Count > 0)
        {
            int times = trees.Count();
            List<TreeNode> list = new List<TreeNode>();
            for(int i = 0;i < times; i++)
            {
                list.Add(trees.Dequeue());
            }
            for(int i = 0;i < times; i += 2)
            {
                TreeNode p = list[i];
                TreeNode q = list[i + 1];
                if (p.left != null && q.right != null)
                {
                    if(p.left.val != q.right.val) return false;
                    trees.Enqueue(p.left);
                    trees.Enqueue(q.right);
                }else if(p.left != q.right) return false;
                    
                if (p.right != null && q.left != null)
                {
                    if(p.right.val != q.left.val) return false;
                    trees.Enqueue(p.right);
                    trees.Enqueue(q.left);
                }else if(p.right != q.left) return false;
            }
        }
        return true;
    }
}
