// ═══════ 建树与判题工具（照 102 / 103 同一套，第四次复用）═══════

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
    IList<IList<int>>? actual = new Solution().LevelOrderBottom(root);
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
//   · ⚠️ 抄的数不一定准：以网页题面为准（"别信记忆、看题面"是硬纪律）

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   107 = 102 的【层序】+ 整个结果【上下颠倒】。
//   与昨天 103 的分界线只有一句话：
//     103 翻的是【每层内部】（层内左右换向）；
//     107 翻的是【层与层之间】（每层内部保持左→右）。
//   ⭐ 所以"方向"这把刀今天装的位置又变了：
//     103 装在【往这一层塞】的那端，107 装在【把层塞进结果】的那端。

// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例：[3,9,20,null,null,15,7] → 底层先出", new int?[] { 3, 9, 20, null, null, 15, 7 },
      new int[][] { new[] { 15, 7 }, new[] { 9, 20 }, new[] { 3 } });
Check("★★ 空树 —— 返回【空列表】，不是 null，也不是 [[]]", new int?[] { },
      new int[][] { });
Check("★ 单节点（只有一层，翻不翻无感）", new int?[] { 1 },
      new int[][] { new[] { 1 } });
Check("★ 两节点：只有左孩子", new int?[] { 1, 2 },
      new int[][] { new[] { 2 }, new[] { 1 } });
Check("★★ 左斜链 4 层（每层只有 1 个）", new int?[] { 1, 2, null, 3, null, 4 },
      new int[][] { new[] { 4 }, new[] { 3 }, new[] { 2 }, new[] { 1 } });
Check("★★ 右斜链 4 层（与上组同形）", new int?[] { 1, null, 2, null, 3, null, 4 },
      new int[][] { new[] { 4 }, new[] { 3 }, new[] { 2 }, new[] { 1 } });
Check("★★ 深左链 5 层", new int?[] { 1, 2, null, 3, null, 4, null, 5 },
      new int[][] { new[] { 5 }, new[] { 4 }, new[] { 3 }, new[] { 2 }, new[] { 1 } });
Check("★★ 满树三层（层序 [ [1],[2,3],[4,5,6,7] ] 的表层翻转）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 4, 5, 6, 7 }, new[] { 2, 3 }, new[] { 1 } });
Check("★★★ 满树四层（8 个元素那层要到最前面）", new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
      new int[][] { new[] { 8, 9, 10, 11, 12, 13, 14, 15 }, new[] { 4, 5, 6, 7 }, new[] { 2, 3 }, new[] { 1 } });
Check("★ 含 0 和负数（值域边界）", new int?[] { 0, -1, -2, -3 },
      new int[][] { new[] { -3 }, new[] { -1, -2 }, new[] { 0 } });
Check("★★ 最宽层不是最深的那层（层数 4、宽度 2/1 交错）", new int?[] { 1, 2, 3, null, 4, null, null, 5 },
      new int[][] { new[] { 5 }, new[] { 4 }, new[] { 2, 3 }, new[] { 1 } });
Check("★★ 三层不成满（最宽层不在最后）", new int?[] { 1, 2, 3, 4, null, null, 5 },
      new int[][] { new[] { 4, 5 }, new[] { 2, 3 }, new[] { 1 } });
Check("★★★ 不对称四层树（每层宽度 1 / 2 / 3 / 1）", new int?[] { 1, 2, 3, 4, 5, 6, null, 7 },
      new int[][] { new[] { 7 }, new[] { 4, 5, 6 }, new[] { 2, 3 }, new[] { 1 } });

// ═══════ 反例区（专抓"翻转"这件事的错法）═══════
//   107 只有一把刀（层与层颠倒），所以反例要能分别戳穿这两种"翻错"：
//     · 错法 A：根本忘了翻 → 输出与 102 一模一样
//     · 错法 B：把 107 当成 103 → 层内也反了
//   ⭐ 判据（两种错法的"最小可抓形状"不一样，别混成一句）：
//     · 错法 A（忘了翻）→ 只要【层数 ≥ 2】就露馅 —— 斜链即可抓到。
//     · 错法 B（层内也反）→ 必须【某层 ≥ 2 个节点】才露馅 —— 斜链上永远隐身
//       （长度 1 的列表怎么翻都是它自己）。
//     · 两种都抓不到的形状 = 空树 / 单节点（只有一层、且宽度为 1）。
Check("⛔ 抓「忘了翻转 ⇒ 退化成 102」（错解给你 [[1],[2,3],[4,5,6,7]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 4, 5, 6, 7 }, new[] { 2, 3 }, new[] { 1 } });
Check("⛔ 抓「层内也反了 ⇒ 当成 103」（错解给你 [[7,6,5,4],[3,2],[1]]）", new int?[] { 1, 2, 3, 4, 5, 6, 7 },
      new int[][] { new[] { 4, 5, 6, 7 }, new[] { 2, 3 }, new[] { 1 } });
Check("⛔ 抓「只颠倒了一层 / 下标写错」（不对称树会给 [[7],[6,5,4],[3,2],[1]] 之类）", new int?[] { 1, 2, 3, 4, 5, 6, null, 7 },
      new int[][] { new[] { 7 }, new[] { 4, 5, 6 }, new[] { 2, 3 }, new[] { 1 } });

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
    // TODO: 返回【自底向上】的层序遍历 —— 最底层在最前，根在最后；【每层内部仍是左→右】。
    //
    //   ── 起点：102 那套 BFS（队列 + 先取 `times = q.Count` 再 for 一圈）
    //      你已经写过三遍（102 / 103，其中 103 是 102 + 每层方向开关）。
    //      今天**不需要新装备**，只多一个决定：
    //
    //   ── 唯一的新决定（想清楚再落笔）：
    //      "层与层颠倒"这件事，在哪一步做？三条路都对，但代价不同 ——
    //        ① 每收集完一层就 `result.Insert(0, level)`      （塞进头部）
    //        ② 一路 `Add` 到底，最后 `result.Reverse()` 一次  （攒完再翻）
    //        ③ 用 `Stack<List<int>>` / `LinkedList.AddFirst`  （换容器）
    //      ⭐ 问题：①每次 `Insert(0, …)` 要搬动多少个"引用"？
    //         假设一共 L 层，总共搬了多少次？与 ② 的一次性 Reverse 比，谁贵、贵在哪个量级？
    //         （这题 L 最大也就 n，但"每层都搬一遍"和"最后搬一遍"在常数上差一个因子 L）
    //
    //   ── 顺带想一个"别踩"的点：
    //      `List<T>.Reverse()` 是【就地】改，返回 void —— 它翻的是"这个列表自己"。
    //      所以你要确认：你翻的是【装层的那个大列表】，不是【某一层的元素列表】。
    //      （翻错对象 = 直接变成 103 的锯齿形，第 2 组反例会当场抓住你）
    //
    //   ── 复杂度（写完自己算，别照抄）：
    //      时间 O(?)；空间 O(?) —— 照 102 算两次：一条斜链（h=n）和一棵满树，
    //      队列里最多同时躺多少个节点？结果列表里无论如何都躺着几个元素？
    //
    //   ── 验收标准：
    //      ① 16 组全绿（含空树、三条斜链、含 0/负数、两棵满树、不对称四层）；
    //      ② 反例区三组各戳穿什么错法，能一句话说出来；
    //      ③ 能说清"103 翻层内、107 翻层间"这个分界，为什么斜链永远区分不出这两题。
      public IList<IList<int>> LevelOrderBottom(TreeNode? root)
      {
            List<IList<int>> ints = new List<IList<int>>();
            if(root == null) return ints;
            Queue<TreeNode> trees = new Queue<TreeNode>();
            trees.Enqueue(root);
            while(trees.Count > 0)
            {
                  int times = trees.Count;
                  List<int> list = new List<int>();
                  for(int i = 0;i < times;i++)
                  {
                        TreeNode node = trees.Dequeue();
                        list.Add(node.val);
                        if(node.left != null) trees.Enqueue(node.left);
                        if(node.right != null) trees.Enqueue(node.right);
                  }
                  ints.Add(list);
            }
            ints.Reverse();
            return ints;
      }
}
