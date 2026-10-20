// ═══════ 建树与判题工具（照 102 / 103 / 107 同一套，第五次复用）═══════

using System.Security;

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

static void Check(string title, int?[] input, int targetSum, bool expected)
{
    TreeNode? root = Build(input);
    bool actual = new Solution().HasPathSum(root, targetSum);
    bool ok = actual == expected;
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     输入树：{Show(root)}   targetSum = {targetSum}");
        Console.WriteLine($"     期望：{expected}     实际：{actual}");
    }
}

// ═══════ 开工前顺手核一眼题面「提示」═══════
//   · 节点数目范围？下限是不是 0（= 空树合法）⇒ 空树配 targetSum = 0 时返回什么？
//   · Node.val 的值域？**含负数** ⇒ 「和超过 target 就剪枝」这条捷径在这里不成立
//   · targetSum 的值域？可能为 0 / 为负
//   · ⚠️ 抄的数不一定准：以网页题面为准（"别信记忆、看题面"是硬纪律）

// ═══════ ★ 今天的题眼（先想清楚，别急着写）═══════
//   112 问的是【存在性】：有没有【一条根到叶子】的路径，和 == targetSum。
//
//   ① 契约三问（先答完再动笔）：
//        我是谁 —— 以 node 为根的子树里，是否存在【从 node 到叶子】的路径、和等于 remain？
//        往下传什么 —— 剩下的额度（或已攒的里程，二选一，别混）
//        往回返什么 —— bool
//
//   ② 今天唯一的新决定 = 「和」的账在哪一步做：
//        A 油表（remain − val，入口就减）  vs  B 里程表（acc + val，叶子才比）
//        两条都对；差别在 targetSum 要不要进递归、边界判定写在哪。
//
//   ③ 两个坑（已经在上一轮的「今日底层原理」里讲过，落笔前再确认一遍）：
//        ⛔ "到叶子" ≠ "到空节点" —— 空节点处判 remain == 0，空树 + target 0 会误判 true
//        ⛔ "remain 归零" ≠ "找到路" —— 中途归零但还有孩子的节点不是终点
//
//   ④ 一个可以主动用上的语言特性：
//        存在性查询用 `||`（短路：左边 true 就不探右边），不要用按位 `|`。
//        想清楚它为什么在这里是"正确的表达"而不是"省事的小聪明"。
//
//   ⑤ 复杂度（写完自己算，别照抄）：时间 O(?)；空间 O(?)。
//        注意空间有两笔账 —— 递归栈深度 + 每层调用里有没有额外分配。

// ═══════ 用例区（形状优先）═══════
Check("★ 官方示例 1：[5,4,8,11,null,13,4,7,2,null,null,null,1], target 22 → 走 5→4→11→2",
      new int?[] { 5, 4, 8, 11, null, 13, 4, 7, 2, null, null, null, 1 }, 22, true);
Check("★ 官方示例 2：[1,2,3], target 5 → 两条路 1+2 / 1+3 都不够",
      new int?[] { 1, 2, 3 }, 5, false);
Check("★★ 官方示例 3：空树 + target 0 —— 回 false（不是 true）",
      new int?[] { }, 0, false);
Check("★ 单节点 [1], target 1 → true",
      new int?[] { 1 }, 1, true);
Check("★ 单节点 [1], target 0 → false",
      new int?[] { 1 }, 0, false);
Check("★ 单节点 [1], target 2 → false",
      new int?[] { 1 }, 2, false);
Check("★★ 满树三层 [1,2,3,4,5,6,7], target 8 → 左路 1+2+5",
      new int?[] { 1, 2, 3, 4, 5, 6, 7 }, 8, true);
Check("★★ 满树三层同上, target 10 → 右路 1+3+6（左边全不通）",
      new int?[] { 1, 2, 3, 4, 5, 6, 7 }, 10, true);
Check("★★ 满树三层同上, target 11 → 右路 1+3+7（四条路径和 = 7 / 8 / 10 / 11）",
      new int?[] { 1, 2, 3, 4, 5, 6, 7 }, 11, true);
Check("★★★ 满树三层同上, target 9 → 【哪条都不通】（9 不在 7 / 8 / 10 / 11 里）",
      new int?[] { 1, 2, 3, 4, 5, 6, 7 }, 9, false);
Check("★★ 左斜链 4 层 [1,2,null,3,null,4], target 10 → 全链相加",
      new int?[] { 1, 2, null, 3, null, 4 }, 10, true);
Check("★★★ 左斜链同上, target 6 → 中途 1+2+3 正好归零，但 3 还有孩子、不是叶子 ⇒ false",
      new int?[] { 1, 2, null, 3, null, 4 }, 6, false);
Check("★★ 含负数 [1,-2,-3], target -1 → 走左",
      new int?[] { 1, -2, -3 }, -1, true);
Check("★★ 含负数同上, target -2 → 走右（左边 -1 不通）",
      new int?[] { 1, -2, -3 }, -2, true);
Check("★★ 含负数同上, target 0 → 两条都不通",
      new int?[] { 1, -2, -3 }, 0, false);
Check("★★ 全负深链 [-1,-2,null,-3], target -6 → -1-2-3",
      new int?[] { -1, -2, null, -3 }, -6, true);

// ═══════ 反例区（专抓"存在性 + 叶子"这件事的错法）═══════
//   ⛔ A 错法「node == null 时 return remain == 0」→ 空树 + target 0 就 true（上面第 3 组抓）
//   ⛔ B 错法「任意节点 remain 归零就 true，不判是不是叶子」→ 下面第 1 组抓
//   ⛔ C 错法「只探左子/只探右子，漏了另一侧」→ 下面第 2 组抓（答案只在右边）
//   ⛔ D 错法「往下传 remain − val 但忘了减自己」→ 官方示例 1 就会歪（看实际值对不对得上）
Check("⛔ 抓「remain 归零就 return true（不判叶子）」：[1,-1,2,5], target 0 → 期望 false",
      new int?[] { 1, -1, 2, 5 }, 0, false);
Check("⛔ 抓「漏探一侧」：[1,2,3], target 4 → 只有 1+3 成立 ⇒ true",
      new int?[] { 1, 2, 3 }, 4, true);
Check("⛔ 抓「把两棵子树的和相加当成路径和」：[1,2,3], target 6 → 1+2+3，但这不是一条路径 ⇒ false",
      new int?[] { 1, 2, 3 }, 6, false);
Check("⛔ 抓「只在叶子比较、却把中途节点当叶子提前返回」：[1,2,null,3], target 3 → 1+2 归零但 2 有孩子 ⇒ false",
      new int?[] { 1, 2, null, 3 }, 3, false);

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
    // TODO: 判断是否存在【根到叶子】的路径，路径上节点值之和 == targetSum。
    //
    //   ── 落笔顺序建议（照契约三问，别跳步）：
    //      ① 先写下递归函数的签名 + 一句话注释（"我是谁"），再写函数体
    //      ② 先写【两个基准情形】：空节点 / 叶子 —— 想清楚各自该返回什么、为什么
    //      ③ 最后写【递归调用】：左右各探一次，怎么合成
    //
    //   ── 三条硬判据（写完拿它对，不通过就是错）：
    //      ① 空树 + targetSum = 0 → false
    //      ② 中途 remain 归零但节点还有孩子 → 不算找到
    //      ③ 只探一侧就返回 → 上面反例第 2 组会红
    //
    //   ── 复杂度填在这里（自己想，别空着）：
    //      时间 O(n)　空间 O(h)
    public bool HasPathSum(TreeNode? root, int targetSum)
    {
        if(root == null) return false;
        if(root.left == null && root.right == null && targetSum == root.val) return true;
        return (HasPathSum(root.left,targetSum - root.val) || HasPathSum(root.right,targetSum - root.val));
    }
}
