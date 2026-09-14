// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.ContainsDuplicate(new int[] { 1, 2, 3, 1 }));                    // 期望 True
Console.WriteLine(sol.ContainsDuplicate(new int[] { 1, 2, 3, 4 }));                    // 期望 False
Console.WriteLine(sol.ContainsDuplicate(new int[] { 1, 1, 1, 3, 3, 4, 3, 2, 4, 2 })); // 期望 True
Console.WriteLine(sol.ContainsDuplicate(new int[] { 1 }));                            // 期望 False（单元素）
Console.WriteLine(sol.ContainsDuplicate(new int[] { }));                              // 期望 False（空数组边界）

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 数组中存在重复元素 → 返回 true；全部互不相同 → false
    //   要求：一趟遍历搞定（O(n)），不要用嵌套循环的 O(n²) 暴力解
    //
    //   API 备忘：
    //     HashSet<int> seen = new HashSet<int>();
    //     seen.Add(x)       → 添加成功返回 true；【已存在返回 false】（这个返回值是解题钥匙）
    //     seen.Contains(x)  → 判断存在（O(1)）
    //     seen.Count        → 元素个数
    //
    //   思路方向（自己想细节）：边遍历边"登记"，登记时如果发现"这个我登记过"——答案就出来了
    public bool ContainsDuplicate(int[] nums) {
        HashSet<int> seen = new HashSet<int>();
        for(int i = 0;i < nums.Length; i++)
        {
            if(!seen.Add(nums[i])) return true;
        }
        return false;
    }
}
