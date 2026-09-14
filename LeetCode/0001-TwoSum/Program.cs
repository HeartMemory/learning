// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();

// ── 暴力版（08-28 原作，O(n²)）——保留对照，不删 ──
int[] r1 = sol.TwoSum(new int[] { 2, 7, 11, 15 }, 9);
Console.WriteLine($"{r1[0]}, {r1[1]}");   // 期望 0, 1

// ── 哈希版（09-14 重写，O(n)）——今天的任务 ──
Console.WriteLine(Show(sol.TwoSumHash(new int[] { 2, 7, 11, 15 }, 9)));    // 期望 0, 1
Console.WriteLine(Show(sol.TwoSumHash(new int[] { 3, 2, 4 }, 6)));         // 期望 1, 2（不能返回 0, 0——同一元素不能用两次）
Console.WriteLine(Show(sol.TwoSumHash(new int[] { 3, 3 }, 6)));            // 期望 0, 1（同值但不同下标，合法）
Console.WriteLine(Show(sol.TwoSumHash(new int[] { -1, -2, -3, -4 }, -7))); // 期望 2, 3（负数也能算）
Console.WriteLine(Show(sol.TwoSumHash(new int[] { 0, 4, 3, 0 }, 0)));      // 期望 0, 3（0 + 0）

static string Show(int[] r) => r.Length == 2 ? $"{r[0]}, {r[1]}" : "无解";

// ═══════ 类型区 ═══════
public class Solution {
    // 暴力版（08-28）：双层循环枚举所有配对 —— O(n²)
    public int[] TwoSum(int[] nums, int target) {
        for (int i = 0; i < nums.Length - 1; i++)
        {
            for(int j = i + 1; j < nums.Length; j++)
            {
                if (nums[i] + nums[j] == target)
                {
                    return new int[]{i,j};
                }
            }
        }
        return new int[0];
    }

    // TODO（09-14 哈希重写）：一趟遍历 O(n)，用 Dictionary 换空间
    //
    //   Dictionary<int, int> 记什么？—— key = 见过的数字，value = 它出现的下标
    //   遍历到 nums[i] 时问自己一句：我要配对的数是 target - nums[i]，它之前出现过吗？
    //       ① 出现过 → 字典里存着它的下标 → 直接返回 [那个下标, i]
    //       ② 没出现 → 把当前数字和下标登记进字典，继续往后走
    //
    //   ⚠️ 顺序陷阱（自己想清楚为什么）：查字典 和 存字典，哪个在前？
    //      试想 {3, 3}, target = 6：如果先存后查，i=0 时会发生什么？（会配到自己 → 返回 [0,0] ❌）
    //
    //   API 备忘：dict.ContainsKey(x) / dict[x] 取值 / dict[x] = i 存值 / dict.TryGetValue(x, out int idx)
    public int[] TwoSumHash(int[] nums, int target) {
        Dictionary<int,int> dict = new Dictionary<int,int>();
        for(int i = 0;i < nums.Length; i++)
        {
            if(dict.TryGetValue(target - nums[i], out int idx)) return new int[]{idx,i};
            dict[nums[i]] = i;
        }
        return new int[0];
    }
}
