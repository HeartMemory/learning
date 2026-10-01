// ═══════ 建树与判题工具（144 / 145 同一套，第四次复用）═══════
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

// 格式化"层序结果"，用于失败时打印（期望 / 实际 都用它，形状一眼可比）
static string Fmt(IEnumerable<IEnumerable<int>>? levels)
{
    if (levels is null) return "null（⚠️ 空树该返回【空列表】，不是 null）";
    List<string> parts = new List<string>();
    foreach (var lv in levels) parts.Add("[" + string.Join(",", lv) + "]");
    return "[" + string.Join(", ", parts) + "]";
}

// 判题器：这次期望值是"列表的列表"（层序天然是二维），所以要逐层逐元素比
static void Check(string title, int?[] input, int[][] expected)
{
    TreeNode? root = Build(input);
    IList<IList<int>>? actual = new Solution().LevelOrder(root);

    bool ok = actual is not null && actual.Count == expected.Length;
    if (ok)
    {
        for (int i = 0; i < expected.Length && ok; i++)
        {
            if (actual![i] is null || actual[i].Count != expected[i].Length) { ok = false; break; }
            for (int j = 0; j < expected[i].Length; j++)
                if (actual[i][j] != expected[i][j]) { ok = false; break; }
        }
    }

    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入树：{Show(root)}");
        Console.WriteLine($"     期望：{Fmt(expected)}");
        Console.WriteLine($"     实际：{Fmt(actual)}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？**下限是不是 0**（= 空树合法）——决定空树返回 [] 还是 [[]]
//   · Node.val 的值域？（决定用不用考虑溢出）
//   · ⚠️ 我抄的数不一定准：**以网页题面为准**（"别信记忆、看题面"是硬纪律）
//
// ═══════ ★ 今天换脑子：从"深度优先"切到"广度优先" ═══════
//   前三天（144 / 94 / 145）都是【递归 DFS】：一条路走到黑，靠系统栈回退。
//   今天要【一层一层往外扩散】——而"先进先出"这件事，天生属于【队列】。
//   ⭐ 一句话记住这个分水岭：**DFS 的隐形工具是栈，BFS 的显式工具是队列。**

// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：完全二叉树（每层长度都不同）", new int?[] { 3, 9, 20, null, null, 15, 7 },
      new int[][] { new[] { 3 }, new[] { 9, 20 }, new[] { 15, 7 } });
Check("★★ 官方示例 2：空树 —— 返回【空列表】，不是 null，也不是 [[]]", new int?[] { },
      new int[][] { });

Check("★ 单节点（最小规模）", new int?[] { 1 },
      new int[][] { new[] { 1 } });
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 },
      new int[][] { new[] { 1 }, new[] { 2 } });
Check("★ 两节点：只有右孩子", new int?[] { 1, null, 2 },
      new int[][] { new[] { 1 }, new[] { 2 } });
Check("★★ 满二叉树（三层，最后一层最长）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4, 5, 6, 7 } });
Check("★★ 全左的斜链（层数 = 节点数，队列里永远只有 1 个）", new int?[] { 1, 2, null, 3, null, 4 },
      new int[][] { new[] { 1 }, new[] { 2 }, new[] { 3 }, new[] { 4 } });
Check("★★ 全右的斜链（与上组同形，但方向相反）", new int?[] { 1, null, 2, null, 3, null, 4 },
      new int[][] { new[] { 1 }, new[] { 2 }, new[] { 3 }, new[] { 4 } });
Check("★ 含 0 和负数（值域边界）", new int?[] { 0, -1, -2, -3 },
      new int[][] { new[] { 0 }, new[] { -1, -2 }, new[] { -3 } });
Check("★★★ 最宽层与最深层不在同一条支上", new int?[] { 1, 2, 3, null, 4, null, null, 5 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4 }, new[] { 5 } });
Check("★★ 三层不成满（最宽层不是最后一层）", new int?[] { 1, 2, 3, 4, null, null, 5 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4, 5 } });
Check("★★ 左支早停、右支继续（null 不能造出假层）", new int?[] { 1, 2, 3, 4, 5, 6, null },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4, 5, 6 } });

// ═══════ 反例区（专抓"分层"这件事的三种错法）═══════
//   层序 = 一层一层、每层从左到右。下面三组输入不同，各自能戳穿一种错法：
//     · 错法 A：孩子进队顺序写反（先 right 后 left）→ 每层变成"从右到左"
//     · 错法 B：把"本层个数"写成【活的】`q.Count`（`for (int i = 0; i < q.Count; i++)`）
//              → 计数器被"出队腾位 + 入队新增"**续命**（一边抽干一边补货）→ **每层被切碎错位**
//              ⚠️ 实测满树得到 [[1,2,3,4],[5,6],[7]] —— **不是**"压成一层"（别想当然，跑一遍）
//     · 错法 C：空树那组若返回 null 或 [[]]，会被"空树"那组直接戳穿（见用例区第 2 组）
//   ⭐ 由此得到一条**用例设计判据**：错法 B 在"每层只有 1 个节点"的形状上（单节点 / 两节点 / 两条斜链）
//      **完全看不出来** —— 那时"活的 Count"恰好也等于 1。**bug 的可见性取决于用例的形状**，
//      所以用例区必须有"每层长度不同"的树（官方示例 1 / 满树 / 层长不齐那几组）。
Check("⛔ 抓「每层从右到左」（满树会输出 [[1],[3,2],[7,6,5,4]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4, 5, 6, 7 } });
Check("⛔ 抓「用 q.Count 当循环上限」→ 每层切碎错位（满树实测 [[1,2,3,4],[5,6],[7]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4, 5, 6, 7 } });
Check("⛔ 抓「给不存在的孩子补 null 当分隔符」→ 会多出空层（常见于「哨兵法」）", new int?[] { 1, 2, 3, null, 4 },
      new int[][] { new[] { 1 }, new[] { 2, 3 }, new[] { 4 } });

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
    // TODO: 返回这棵树的【层序遍历】（自顶向下、逐层从左到右）。
    //
    //   ── 抓题眼：和 144 / 94 / 145 的根本区别是什么？
    //      前三题是【深度优先 DFS】：一条路走到黑，靠**系统栈**回退。
    //      层序是【广度优先 BFS】：一圈一圈往外扩散 —— 靠**你自己 new 的队列**。
    //      ⭐ 所以今天不是"再加一题遍历"，而是**换一套推进机制**。
    //
    //   ── ⭐ 今天唯一的真难点（全部难度都压在这一句上）：
    //      **你怎么知道"这一层结束了"？**
    //      队列里会同时躺着"本层还没处理的"和"下一层刚进队的"——它们混在一起，
    //      队列自己【不会】告诉你边界在哪。所以落笔前先把这三个问题想清楚：
    //        · 想让"层"显形，你得在哪一刻去看队列的**什么信息**？
    //        · 那个信息必须落在哪儿，才不会被后续的进出队操作改掉？
    //        · 自问自答：如果图省事写成 `for (int i = 0; i < q.Count; i++)`，输出会变成什么形状？
    //
    //   ── 复杂度（写完自己算一遍，别照抄我的）：
    //      时间 O(?)：每个节点进出队列各几次？
    //      空间：**算两次** —— 一条斜链（h = n）和一棵满树，队列里最多各同时躺多少个节点？
    //      ⚠️ 算完再回答这一句：为什么递归版（144 讲过是 O(h)）和层序的答案**形状正好相反**？
    //
    //   ── 验收标准：
    //      ① 15 组全绿（含空树、两条斜链、含 0/负数、三组反例）；
    //      ② 反例区三组的含义能说出来（从右到左 / 压成一层 / 补空层）；
    //      ③ 能用一句话说清"为什么层序用队列、前中后序用递归（系统栈）"。
      public IList<IList<int>> LevelOrder(TreeNode? root)
      {
            IList<IList<int>> list = new List<IList<int>>();
            if(root == null) return list;
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(root);
            while(queue.Count > 0)
            {
                  int times = queue.Count;
                  List<int> ints = new List<int>();
                  for(int i = 0;i < times;i++)
                  {
                        TreeNode node = queue.Dequeue();
                        ints.Add(node.val);
                        if(node.left != null) queue.Enqueue(node.left);
                        if(node.right != null) queue.Enqueue(node.right);
                  }
                  list.Add(ints);
            }
            return list;
      }
}
