// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(Show(sol.Intersection(new int[] { 1, 2, 2, 1 }, new int[] { 2, 2 })));      // 期望 [2]（去重！不是 [2,2]）
Console.WriteLine(Show(sol.Intersection(new int[] { 4, 9, 5 }, new int[] { 9, 4, 9, 8, 4 }))); // 期望 [4,9] 或 [9,4]（顺序不限）
Console.WriteLine(Show(sol.Intersection(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 })));      // 期望 []（没有交集）
Console.WriteLine(Show(sol.Intersection(new int[] { }, new int[] { 1, 2 })));                 // 期望 []（空数组边界）
Console.WriteLine(Show(sol.Intersection(new int[] { 1, 1, 1 }, new int[] { 1 })));            // 期望 [1]（多个重复也只算一次）

Console.WriteLine(Show(sol.AIntersection(new int[] { 1, 2, 2, 1 }, new int[] { 2, 2 })));        // 期望 [2]
Console.WriteLine(Show(sol.AIntersection(new int[] { 4, 9, 5 }, new int[] { 9, 4, 9, 8, 4 }))); // 期望 [4,9] 或 [9,4]

static string Show(int[] a) => "[" + string.Join(",", a) + "]";

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 返回两个数组的交集，元素必须唯一（顺序不限）
    //
    //   HashSet 的三个集合运算武器（新 API）：
    //     set.IntersectWith(other)   → 原地保留「交集」（只留两边都有的）
    //     set.UnionWith(other)       → 原地变成「并集」
    //     set.ExceptWith(other)      → 原地变成「差集」（我有你没有的）
    //   —— 都是"原地修改"（返回值 void），调用后原集合就变了
    //
    //   两条路，建议都写一遍对照（这是本题最大的价值）：
    //     A. 手动版：把 nums1 装进 HashSet 去重 → 遍历 nums2，Contains 命中就收进结果集
    //        （结果也要用 HashSet 装，否则 [1,2,2,1] 这种会重复收集）
    //     B. 集合运算版：两个 HashSet 各装一个数组 → 直接 IntersectWith → 转成数组返回
    //        （体会：一行顶一段循环的爽感）
    //
    //   转换：HashSet → 数组 用 set.ToArray()；也可用 new List<int>(set).ToArray()
    public int[] Intersection(int[] nums1, int[] nums2) {
        if(nums1.Length == 0 || nums2.Length == 0) return new int[0];
        HashSet<int> result = new HashSet<int>(nums1);
        HashSet<int> lookup = new HashSet<int>(nums2);
        result.IntersectWith(lookup);
        return result.ToArray();
    }
    public int[] AIntersection(int[] nums1, int[] nums2)
    {
        if(nums1.Length == 0 || nums2.Length == 0) return new int[0];
        HashSet<int> result = new HashSet<int>();
        HashSet<int> lookup = new HashSet<int>(nums1);
        for(int i = 0;i < nums2.Length; i++)
        {
            if (lookup.Contains(nums2[i]))
            {
                result.Add(nums2[i]);
            }
        }
        return result.ToArray();
    }
}
