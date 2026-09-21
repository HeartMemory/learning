// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1, 2, 3, 1 }, 3));        // 期望 True
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1, 0, 1, 1 }, 1));        // 期望 True
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1, 2, 3, 1, 2, 3 }, 2));  // 期望 False
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 99, 99 }, 0));            // 期望 False ★ 差值必须 ≤ k，k=0 时连相邻同值都不算
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1, 2, 1 }, 0));           // 期望 False ★ 同上，防止写成 "同值就 true"
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1 }, 1));                 // 期望 False ★ 单元素边界（别越界/别崩）
Console.WriteLine(sol.ContainsNearbyDuplicate(new int[0], 5));                  // 期望 False ★ 空数组边界
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 1, 1 }, 5));              // 期望 True ★★ 反向用例：长度 < k 时答案照样可能为 true
Console.WriteLine(sol.ContainsNearbyDuplicate(new[] { 5, 5, 5 }, 100));         // 期望 True ★★ 同上，把"数组短"当"没答案"会漏
// 提示：想看得更清楚可以打成 $"用例1: {结果}" 的形式；这里先保持和之前几题一样的裸输出

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 判断是否存在两个【不同下标】i、j，使得 nums[i] == nums[j] 且 |i - j| <= k。
    //
    //   ★ 先想清楚它和 217（存在重复元素）差在哪：
    //     217 只问"有没有重复过" → HashSet 一句话解决（用到 Add 返回值那招）；
    //     219 问的是"重复的那两次【离得够不够近】" → HashSet 只存了"存在性"，
    //         【位置信息】被丢掉了 → 所以今天要换一个能存"键 → 值"的结构。
    //
    //   思路提示（"最近一次"模型）：
    //     遍历数组，对当前数字 nums[i]：
    //       ① 查：这个数字【上次】出现在哪？（查不到 = 第一次见）
    //       ② 判：i - 上次位置 <= k 吗？是 → 直接返回 true（找到就跑，别恋战）
    //       ③ 登记：把这个数字的位置【更新】为当前 i，继续往后走
    //     遍历结束都没命中 → false。
    //
    //   ★★ 今天的核心陷阱（想通再动手，否则用例 4/5 会把假 AC 抓出来）：
    //      同一数字出现 3 次以上时，第 ③ 步到底是"只在没记录过时写入"，还是"【无条件覆盖】"？
    //      提示：和 i 差值【最小】的那一次，一定是它【最近一次】出现的位置 —— 保留"最有希望的候选"。
    //      ★ 对比 09-20 的 205：那里的"先查后登记"是【幂等】写（值相同，再写一遍无害）；
    //        这里如果沿用"已存在就跳过"的写法，留下的是【最早】下标 → 就是这道题最常见的假 AC。
    //      用 [1,2,3,1,2,3], k=2 和 [99,99], k=0 两组用例自己验证这一点。
    //
    //   复杂度：O(n) 时间、O(n) 空间（空间是"不同数字的个数"，往往远小于 n）。
    //   ★ 进阶（AC 后再想，两个方向都值得写）：
    //     ① 【滑动窗口 Set】：只在窗口里留最近 k 个元素，超过 k 就把 nums[i-k] 移除
    //        → 空间降到 O(k)，适合"k 很小、数据是流式"的场景（面试爱问）。
    //     ② 卡住就先写 O(n²) 暴力（双重循环 + 距离判断）拿个 AC，标注清楚再回来优化。
    public bool ContainsNearbyDuplicate(int[] nums, int k) {
        if(nums.Length < 2) return false;
        Dictionary<int,int> dict = new Dictionary<int, int>();
        for(int i = 0;i < nums.Length;i++)
        {
            if(dict.TryGetValue(nums[i],out int n)) if(i - n <= k) return true;
            dict[nums[i]] = i;
        }
        return false;
    }
}
