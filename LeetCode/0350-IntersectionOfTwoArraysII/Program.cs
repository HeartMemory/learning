// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(string.Join(",", sol.Intersect(new[] { 1, 2, 2, 1 }, new[] { 2, 2 })));        // 期望 2,2
Console.WriteLine(string.Join(",", sol.Intersect(new[] { 4, 9, 5 }, new[] { 9, 4, 9, 8, 4 })));  // 期望 4,9 或 9,4（顺序随意）
Console.WriteLine(string.Join(",", sol.Intersect(new[] { 1, 1, 1 }, new[] { 1, 1 })));          // 期望 1,1 ★ 次数取较小的那个
Console.WriteLine(string.Join(",", sol.Intersect(new[] { 1, 2, 3 }, new[] { 4, 5, 6 })));        // 期望（空）
Console.WriteLine(string.Join(",", sol.Intersect(new[] { 1 }, new[] { 1 })));                    // 期望 1
Console.WriteLine(string.Join(",", sol.Intersect(new int[0], new[] { 1, 2 })));                  // 期望（空）★ 边界
// 提示：空结果用 string.Join 打出来就是一行空白，属正常；想看得更清楚可以两边各加一对中括号

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 返回两个数组的交集 —— 每个元素的【出现次数 = 它在两个数组中次数的较小值】。
    //
    //   ★ 先想清楚它和 349（两个数组的交集）差在哪：
    //     349 要的是"去重后的交集" → HashSet 一句话解决；
    //     350 要的是"带次数的交集"：nums1 = [1,1,1]、nums2 = [1,1] → 结果 [1,1]（不是 [1]）。
    //     而 HashSet 会【去重】，恰好把这里需要的信息丢掉了 → 需要一张【计数表】。
    //
    //   思路提示（"库存"模型，和 09-15 那题 383 赎金信是同一套路，只是反过来用）：
    //     第 1 步：给其中一个数组建库存表 Dictionary<int,int>（数字 → 还剩几件）
    //     第 2 步：遍历另一个数组，每命中一次就【取走一件】（计数 −1）
    //     第 3 步：库存减到 0 的东西不能再取 —— "次数取较小值"就被这一步自动实现了
    //
    //   动手顺序建议：
    //     ① 先写"建表"（注意：同一个数字第二次出现要【累加】，不是覆盖！）
    //     ② 再写"取货 + 收进结果"（结果长度未知 → 用你 09-01 学过的 List）
    //     ③ 最后 List 转 int[]（API：ToArray()）
    //
    //   复杂度：O(m+n) 时间、O(m) 空间。
    //   ★ 进阶（有余力再想）：让【较短的那个数组】当库存表 → 空间 O(min(m,n))；
    //     或者走完全不同的路：两数组排序 + 双指针 O(n log n)，面试常被追问"数据已经有序怎么办"。
    public int[] Intersect(int[] nums1, int[] nums2) {
        if(nums1.Length == 0 || nums2.Length == 0) return new int[0];
        List<int> result = new List<int>();
        Dictionary<int,int> dict = new Dictionary<int, int>();
        foreach(int n1 in nums1)
        {
            dict.TryGetValue(n1,out int times);
            dict[n1] = times + 1;
        }
        foreach(int n2 in nums2)
        {
            if(dict.TryGetValue(n2,out int times) && times > 0) 
            {
                result.Add(n2);
                dict[n2] = times - 1;
            }
        }
        return result.ToArray();
    }
}
