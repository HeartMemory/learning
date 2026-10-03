// ═══════ 建树与判题工具（144 / 94 / 145 / 102 / 104 同一套，第六次复用）═══════
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

// 判题器：这次的「期望值」是【另一棵树】—— 所以让同一台序列化器（Show）吐出两份层序文本再比：
//   ⭐ 形状和数值一次比完，比"逐字段核对"既省事又更严（对角链那种"只差一个孩子"的形状最敏感）
static void Check(string title, int?[] input, int?[] expected)
{
    TreeNode? root = Build(input);
    TreeNode? actualRoot = new Solution().InvertTree(root);
    string expectedText = Show(Build(expected));
    string actualText = Show(actualRoot);
    bool ok = actualText == expectedText;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入：      {Show(Build(input))}");
        Console.WriteLine($"     期望翻转后：{expectedText}");
        Console.WriteLine($"     实际翻转后：{actualText}");
    }
}

// ⭐ 对合性检查（involution）：**翻两次必须还原**。
//   这是"翻转"这个操作的【定义性质】—— 不依赖任何抄来的期望值就能判对错，
//   哪怕我把题面示例抄错了，这条也照样抓得住"根本不是翻转"的实现。
static void CheckInvolution(string title, int?[] input)
{
    TreeNode? root = Build(input);
    TreeNode? twice = new Solution().InvertTree(new Solution().InvertTree(root));
    string original = Show(Build(input));
    string back = Show(twice);
    bool ok = back == original;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}（翻两次 = 原树）");
    if (!ok)
    {
        Console.WriteLine($"     原树：    {original}");
        Console.WriteLine($"     翻两次后：{back}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？**下限是不是 0**（= 空树合法）—— 决定"空树要不要特判"
//   · "翻转"到底翻几层？**只翻根的一层**，还是**每一层都翻**？（画三层的树，手工翻一遍再落笔）
//   · ⚠️ 我抄的数不一定准：**以网页题面为准**（"别信记忆、看题面"是硬纪律）
//
// ═══════ ★ 今天要和 104 划一条分界线 ═══════
//   104（昨天）的结论是：**"我的答案要用孩子的答案" → 必须后序**。
//   今天先自己回答一个问题再落笔：
//     · 226 里，"以我为根的那棵子树"需不需要**孩子的结果**才能动手？
//         → 我做的动作是【就地改结构】（把两个孩子的指针互换），
//           交换这个动作**不依赖孩子算出了什么** —— 两个孩子交不交结论，跟我换不换没关系。
//   ⭐ 判据：**"要不要用孩子的结果"决定遍历顺序** ——
//        要（104 求高度 / 算子树大小）→ 后序被钉死；
//        不要（226 就地换指针）→ **前序 / 后序都能做**，差别只剩"读起来顺不顺"。
//   ⚠️ 思考题（先猜，写完再看反例区）：那**中序**行不行？中序是"左 → 我 → 右"，
//      可我一旦交换，右孩子就变成原来的左孩子了 —— 那第三步递归的到底是谁？
//      这一问会决定你"能不能用中序"，写下你的猜想。
//
// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：满树三层（左右两支各自还要再翻）", new int?[] { 4, 2, 7, 1, 3, 6, 9 }, new int?[] { 4, 7, 2, 9, 6, 3, 1 });
Check("★ 官方示例 2：最小的「两个孩子」交换", new int?[] { 2, 1, 3 }, new int?[] { 2, 3, 1 });

Check("★ 空树 → 还是空树（终止条件的第一道门）", new int?[] { }, new int?[] { });
Check("★ 单节点 → 没有孩子可换，原地不动", new int?[] { 1 }, new int?[] { 1 });
Check("★ 只有左孩子 → 翻完变成【只有右孩子】（形状要跟着变，不只是数值）", new int?[] { 1, 2 }, new int?[] { 1, null, 2 });
Check("★ 只有右孩子 → 翻完变成【只有左孩子】", new int?[] { 1, null, 2 }, new int?[] { 1, 2 });
Check("★★ 左斜链（全长 4）→ 翻成右斜链（答案的最右支一路到底）",
      new int?[] { 1, 2, null, 3, null, 4 }, new int?[] { 1, null, 2, null, 3, null, 4 });
Check("★★ 右斜链（全长 4）→ 翻成左斜链",
      new int?[] { 1, null, 2, null, 3, null, 4 }, new int?[] { 1, 2, null, 3, null, 4 });
Check("★★ 两个孩子的深度不一样（左支 3 层、右支 2 层）：交换后深度也要对调过来",
      new int?[] { 1, 2, 3, 4, null, null, null }, new int?[] { 1, 3, 2, null, null, null, 4 });
Check("★★ 满树四层：翻转 = 每一层的左右两半对调",
      new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
      new int?[] { 1, 3, 2, 7, 6, 5, 4, 15, 14, 13, 12, 11, 10, 9, 8 });

CheckInvolution("★★ 对合性：满树四层", new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
CheckInvolution("★★ 对合性：左斜链（最歪的形状）", new int?[] { 1, 2, null, 3, null, 4 });
CheckInvolution("★ 对合性：单节点", new int?[] { 1 });

// ═══════ 迭代版对照区（承 104：同一问题、两种推进机制）═══════
//   ⭐ 接线口径：迭代版方法名固定 `InvertTree1`、返回 `TreeNode?`（与 104 的 `MaxDepth1` 同风格）。
//      这一区会【同时跑两版】，每组里"递归版 + 迭代版"都必须双双等于期望值 ——
//      ⭐ 两版互相印证才是重点：同一份用例上，两套机制独立写错还能同时全绿的概率极低。
//      这就是验收标准③（迭代版与递归版一致）的自动化，不用靠肉眼比对。
static void CheckBoth(string title, int?[] input, int?[] expected)
{
    string wanted = Show(Build(expected));
    string rec = Show(new Solution().InvertTree(Build(input)));
    string iter = Show(new Solution().InvertTree1(Build(input)));
    bool ok = rec == wanted && iter == wanted;

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     期望：   {wanted}");
        Console.WriteLine($"     递归版： {rec}{(rec == wanted ? "" : "   ← ❌ 不一致")}");
        Console.WriteLine($"     迭代版： {iter}{(iter == wanted ? "" : "   ← ❌ 不一致")}");
    }
}

CheckBoth("★ 官方示例 1：满树三层", new int?[] { 4, 2, 7, 1, 3, 6, 9 }, new int?[] { 4, 7, 2, 9, 6, 3, 1 });
CheckBoth("★ 空树（两版都要原样交回 null）", new int?[] { }, new int?[] { });
CheckBoth("★ 单节点", new int?[] { 1 }, new int?[] { 1 });
CheckBoth("★★ 左斜链（BFS 里队列一路不回头）",
      new int?[] { 1, 2, null, 3, null, 4 }, new int?[] { 1, null, 2, null, 3, null, 4 });
CheckBoth("★★ 两支深度不等（左 3 层 / 右 2 层）—— 队列的「分层记账」最容易在这里露馅",
      new int?[] { 1, 2, 3, 4, null, null, null }, new int?[] { 1, 3, 2, null, null, null, 4 });
CheckBoth("★★ 满树四层",
      new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
      new int?[] { 1, 3, 2, 7, 6, 5, 4, 15, 14, 13, 12, 11, 10, 9, 8 });
CheckBoth("⚠️ 左右同值（值层序的盲区：两版「互相」一致才有意义，不是各自看着像对）",
      new int?[] { 1, 2, 2 }, new int?[] { 1, 2, 2 });

// ═══════ 反例区（专抓四种错法 + 一个假绿陷阱）═══════
//   ⚠️ 下面每条都标了"该错法会在哪一组露馅 / 哪一组会假绿"——先照着读，再自己写错一次验证。
//   ⭐ 前三条症状已用临时副本**实测**（满树三层 `[1,2,3,4,5,6,7]`，期望 `[1,3,2,7,6,5,4]`）——
//      三条的"指纹"完全不同，正是它们最值钱的地方：
//     · 错法 A：**只递归不交换**（`InvertTree(root.left); InvertTree(root.right);` 却没换指针）
//              → ❗实测：**树原样返回** `[1,2,3,4,5,6,7]`（每层都白翻，指针一次没动过）
//              → ❗但 **空树 / 单节点 / 左右同值** 这几组会**假绿**：它们本来就不需要变
//                （实测左右同值的 `[1,2,2]` 用 A 也照样"通过"——**值层序有盲区**）
//     · 错法 B：**只交换不递归**（只翻根那一层）
//              → ❗实测：期望 `[1,3,2,7,6,5,4]` → 实际 `[1,3,2,6,7,4,5]`
//                → 第一层对了，**第三层只做了"左右两半对调"、每半内部没翻**
//                  （4/5 该互换成 5/4，却只是各自留在对面那一半里）
//                → ⭐ 最阴的一条：**开头两个数完全正确**，只看开头会以为"思路对了"
//     · 错法 C：**交换时没留临时变量**（`root.left = root.right; root.right = root.left;`）
//              → ❗实测：期望 `[1,3,2,7,6,5,4]` → 实际 `[1,3,3,7,7,7,7]`
//                → **不是"翻了两次"，是"同一支被塞给了两边"**：两个孩子都指向原右子树 →
//                  **原左支整支消失**（4/5 没了），并且 3 出现两次、7 出现四次（**重复引用**）
//                → ⭐ 指纹 = **值重复** —— 和 A（原样不动）、B（开头对）一眼可分
//     · 错法 D：**返回值写错**（例：`return root.left;`）
//              → 递归里面翻得好好的，但**交给调用者的根不对** → 交给判题的那棵树直接错位
//   ⭐ 一句话判据：**"错法长什么样"必须跑出来看** —— 三条错法的现象各不相同，
//      凭直觉猜"反正都是错的、都会红"的话，就丢掉了"从症状反推病因"这条能力。
Check("⛔ 抓 A「只递归不交换」→ 非对称形状会原样返回（期望 [2, 3, 1]）", new int?[] { 2, 1, 3 }, new int?[] { 2, 3, 1 });
Check("⛔ 抓 B「只交换不递归 / 只翻一层」→ 第三层没翻（4/5/6/7 那一层露馅）",
      new int?[] { 1, 2, 3, 4, 5, 6, 7 }, new int?[] { 1, 3, 2, 7, 6, 5, 4 });
Check("⛔ 抓 C「交换没留临时变量」→ 原左支丢失（左孩子该是 3，会拿到 2）", new int?[] { 1, 2, 3 }, new int?[] { 1, 3, 2 });
Check("⛔ 抓 D「返回值写错」→ 交给判题的根错位（该是 1，会拿到 2 / 3）", new int?[] { 1, 2, 3 }, new int?[] { 1, 3, 2 });
Check("⚠️ 假绿陷阱：左右同值的树 —— 值层序**看不出**翻没翻（形状不是镜像的也照样绿）", new int?[] { 1, 2, 2 }, new int?[] { 1, 2, 2 });

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
    // TODO: 翻转二叉树 —— 把**每个**节点的左右子树互换，返回翻转后的根。
    //
    //   ── 抓题眼：和 104 比，这题为什么**不被遍历顺序绑死**？
    //      104 是"我的答案 = 孩子的答案 + 1" → 孩子必须先算完（后序）。
    //      226 是"就地改结构"：我做的动作（换两个指针）**不需要孩子先交出什么**。
    //      ⭐ 判据：**"要不要用孩子的结果"决定顺序** —— 要 → 后序；只是就地改自己 → 任意顺序。
    //
    //   ── 三个必须先回答的问题（落笔前想完）：
    //      ① 交换这一步，写在**递归调用之前**还是**之后**？两种都行吗？为什么？
    //      ② 交换两个孩子，**必须**借一个临时变量吗？C# 有没有一行搞定的写法？
    //         （提示：C# 的**元组赋值**可以一次交换两个值 —— `(a, b) = (b, a);`）
    //      ③ 终止条件：遇到 `null` 该做什么、返回什么？（想象叶子节点会怎么被调用）
    //
    //   ── 自问自答（写完再回来看）：
    //      · 如果"交换"写在递归之后，和写在之前，**结果**有区别吗？（对比 104 那题必须后序）
    //      · 错法 C（`root.left = root.right; root.right = root.left;`）为什么是"丢一半"而不是"翻两次"？
    //      · 中序（左 → 交换 → 右）到底能不能做？（写完去反例区对答案）
    //
    //   ── 复杂度（自己算一遍）：
    //      时间 O(?)：每个节点被碰到几次？
    //      空间：递归栈深 = 树高 h → O(?)；斜链时 h = n（承 10-02 第 34 章）
    //
    //   ── 进阶（今天的主菜，做完递归版再写）：
    //      ① 用**显式栈 `Stack<TreeNode>`**（DFS 迭代）再写一版；
    //      ② 用 **BFS 队列**再写一版（骨架复用 104 的层序版 —— 但那题的队列是"按层消费"的，
    //         这题的队列是"路过一个、就换一个"→ ⭐ 同一套队列，两种用法，写的时候对比一下）；
    //      ③ 三版（前序 / 后序 / 迭代）结果必须完全一致。
    //
    //   ── 验收标准：
    //      ① 全绿（含空树 / 两条斜链 / 满树四层 / 对合性三组 / 一组假绿陷阱）；
    //      ② 能一句话说清"为什么 226 不需要后序，而 104 必须后序"；
    //      ③ 迭代版与递归版结果一致；
    //      ④ 反例区 A/B/C/D 各自戳穿什么错法，能说出来。
    public TreeNode? InvertTree(TreeNode? root)
    {
        if(root == null) return root;
        (root.left,root.right) = (root.right,root.left);
        InvertTree(root.left);
        InvertTree(root.right);
        return root;
    }
    public TreeNode? InvertTree1(TreeNode? root)
    {
        if(root == null) return root;
        Queue<TreeNode> q = new Queue<TreeNode>();
        q.Enqueue(root);
        while(q.Count > 0)
        {
            TreeNode cur = q.Dequeue();
            (cur.left,cur.right) = (cur.right,cur.left);
            if(cur.left != null) q.Enqueue(cur.left);
            if(cur.right != null) q.Enqueue(cur.right);
        }
        return root;
    }
}
