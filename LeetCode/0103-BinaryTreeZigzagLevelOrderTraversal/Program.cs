// ═══════ 建树与判题工具（照 102 / 199 同一套，第三次复用）═══════

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

// 格式化"层序结果"，失败时打印（期望 / 实际 都用它，形状一眼可比）
static string Fmt(IEnumerable<IEnumerable<int>>? levels)
{
    if (levels is null) return "null（⚠️ 空树该返回【空列表】，不是 null）";
    List<string> parts = new List<string>();
    foreach (var lv in levels) parts.Add("[" + string.Join(",", lv) + "]");
    return "[" + string.Join(", ", parts) + "]";
}

// 逐层逐元素比两个"层序结果"（期望 `int[][]` 与结果 `IList<IList<int>>` 都能塞进来）
static bool Same(IEnumerable<IEnumerable<int>>? x, IEnumerable<IEnumerable<int>>? y)
{
    if (x is null || y is null) return false;
    List<IEnumerable<int>> a = x.ToList(), b = y.ToList();
    if (a.Count != b.Count) return false;
    for (int i = 0; i < a.Count; i++)
        if (!a[i].SequenceEqual(b[i])) return false;
    return true;
}

static void Check(string title, int?[] input, int[][] expected)
{
    TreeNode? root = Build(input);
    IList<IList<int>>? actual = new Solution().ZigzagLevelOrder(root);
    bool ok = Same(actual, expected);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望：{Fmt(expected)}");
        Console.WriteLine($"     实际：{Fmt(actual)}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？下限是不是 0（= 空树合法）——决定空树返回 [] 还是 [[]]
//   · Node.val 的值域？（决定用不用考虑溢出）
//   · ⚠️ 我抄的数不一定准：以网页题面为准（"别信记忆、看题面"是硬纪律）
//
// ═══════ ★ 今天的题眼 ═══════
//   锯齿形 = 102 的【层序】+ 每层翻一次【方向】。
//   ⭐ 你已判断对：`Dequeue` 只能从头取"下一个"，没有"取第几个"——
//     所以"方向"这把刀装不到"出队"那端，只能装在【往结果里塞】这端。

// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：完全二叉树（第 1 层要反过来）", new int?[] { 3, 9, 20, null, null, 15, 7 },
      new int[][] { new[] { 3 }, new[] { 20, 9 }, new[] { 15, 7 } });
Check("★★ 空树 —— 返回【空列表】，不是 null，也不是 [[]]", new int?[] { },
      new int[][] { });

Check("★ 单节点（最小规模，方向翻不翻都无感）", new int?[] { 1 },
      new int[][] { new[] { 1 } });
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 },
      new int[][] { new[] { 1 }, new[] { 2 } });
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 },
      new int[][] { new[] { 1 }, new[] { 2 } });
Check("★★ 满树三层（第 1 层反、第 2 层正）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5, 6, 7 } });
Check("★★ 满树四层（第 3 层最长、且要反过来）", new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5, 6, 7 }, new[] { 15, 14, 13, 12, 11, 10, 9, 8 } });
Check("★★ 全左的斜链（每层只有 1 个，锯齿无感）", new int?[] { 1, 2, null, 3, null, 4 },
      new int[][] { new[] { 1 }, new[] { 2 }, new[] { 3 }, new[] { 4 } });
Check("★★ 全右的斜链（与上组同形，方向相反）", new int?[] { 1, null, 2, null, 3, null, 4 },
      new int[][] { new[] { 1 }, new[] { 2 }, new[] { 3 }, new[] { 4 } });
Check("★ 含 0 和负数（值域边界）", new int?[] { 0, -1, -2, -3 },
      new int[][] { new[] { 0 }, new[] { -2, -1 }, new[] { -3 } });
Check("★★★ 最宽层与最深层不在同一条支上（反的层里元素个数不齐）", new int?[] { 1, 2, 3, null, 4, null, null, 5 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4 }, new[] { 5 } });
Check("★★ 三层不成满（最宽层不是最后一层）", new int?[] { 1, 2, 3, 4, null, null, 5 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5 } });

// ═══════ 反例区（专抓"方向"这件事的两种错法）═══════
//   锯齿 = 只在【偶数层】翻。下面两组专戳"方向翻错"：
//     · 错法 A：根本忘了翻 → 退化成普通层序
//     · 错法 B：每层都翻 / 方向标志忘了每层翻一次 → 第 2 层也被反了
//   ⭐ 判据：这两组错法在"每层只有 1 个"的形状（斜链）上【完全看不出来】——
//     只有"某层有 ≥2 个节点"的树才抓得到（所以满树用例是必考）。
Check("⛔ 抓「忘了翻方向 = 普通层序」（满树会输出 [[1],[2,3],[4,5,6,7]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5, 6, 7 } });
Check("⛔ 抓「每层都反 / 方向标志没翻」（满树会输出 [[1],[3,2],[7,6,5,4]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5, 6, 7 } });
Check("⛔ 抓「反了引用、连累上一层」（结果里已有层被二次反转）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 3, 2 }, new[] { 4, 5, 6, 7 } });

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
    // TODO: 返回【锯齿形层序遍历】—— 第 0 层左→右、第 1 层右→左、第 2 层左→右……逐层交替。
    //
    //   ── 你已经踩中题眼（写之前默念一遍）：
    //      「遍历顺序」和「输出方向」是两把独立的刀。
    //      BFS 队列永远从左到右出队（`Dequeue` 只能取"下一个"，没有"取第几个"），
    //      ⇒ 方向这把刀只能装在【往这一层结果里塞】这端，出队那端动不了。
    //
    //   ── 最后一块拼图（想清楚再落笔，别直接敲）：
    //      要"每层翻一次"，你得先知道【现在是第几层】。
    //        · 这个"层号 / 方向"状态从哪来？每层在哪一刻翻一次？
    //        · 翻的"对象"是谁——是"这一层收集完再 Reverse"，还是"收集的时候就从另一头插"？
    //          （两条路都对，但把"该翻哪一步"想清楚，别把方向翻到【已经塞进结果的那一层】上）
    //
    //   ── 复杂度（写完自己算，别照抄）：
    //      时间 O(?)；空间照 102 算两次 —— 一条斜链（h=n）和一棵满树，队列里最多各同时躺多少？
    //
    //   ── 验收标准：
    //      ① 15 组全绿（含空树、两条斜链、含 0/负数、两组满树、三组反例）；
    //      ② 反例区三组各戳穿什么错法，能一句话说出来；
    //      ③ 能说清"方向开关为什么只能装在塞的那端、不能装在出队那端"。
    private int layer;
    public IList<IList<int>> ZigzagLevelOrder(TreeNode? root)
    {
        List<IList<int>> result = new List<IList<int>>();
        if(root == null) return result;
        layer = 0;
        Queue<TreeNode> trees = new Queue<TreeNode>();
        trees.Enqueue(root);
        while(trees.Count > 0)
        {
            int times = trees.Count;
            layer++;
            List<int> temp = new List<int>();
            for(int i = 0;i < times; i++)
            {
                TreeNode cur = trees.Dequeue();
                if(cur.left != null) trees.Enqueue(cur.left);
                if(cur.right != null) trees.Enqueue(cur.right);
                temp.Add(cur.val);
            }
            if(layer % 2 == 0) temp.Reverse();
            result.Add(temp);
        }
        return result;
    }
}
