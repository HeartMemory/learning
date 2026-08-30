Solution sol = new Solution();
Console.WriteLine(sol.MaxSubArray(new int[] { -2, 1, -3, 4, -1, 2, 1, -5, 4 })); // 期望 6（[4,-1,2,1]）
Console.WriteLine(sol.MaxSubArray(new int[] { 1 }));                             // 期望 1
Console.WriteLine(sol.MaxSubArray(new int[] { 5, 4, -1, 7, 8 }));                // 期望 23（整个数组）
Console.WriteLine(sol.MaxSubArray(new int[] { -3, -1, -2 }));                    // 期望 -1（边界：全负数，必须选一个）

public class Solution {
    public int MaxSubArray(int[] nums) {
        int sum = nums[0];
        int max = nums[0];
        for(int i = 1;i < nums.Length; i++)
        {
            if(nums[i] > sum + nums[i])
            {
                sum = nums[i];
            }else{sum += nums[i];}
            if(sum > max)
            {
                max = sum;
            }
        }
        return max;
        // TODO: 找出连续子数组的最大和（子数组至少包含一个元素）
        // 模式提示：还是「遍历时维护历史信息」——
        // 一个变量记「以当前位置结尾的最大子数组和」，一个变量记「全局最大」
    }
}
