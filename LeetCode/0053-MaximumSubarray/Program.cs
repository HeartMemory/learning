Solution sol = new Solution();
Console.WriteLine(sol.MaxSubArray(new int[] { -2, 1, -3, 4, -1, 2, 1, -5, 4 })); // 期望 6（[4,-1,2,1]）
Console.WriteLine(sol.MaxSubArray(new int[] { 1 }));                             // 期望 1
Console.WriteLine(sol.MaxSubArray(new int[] { 5, 4, -1, 7, 8 }));                // 期望 23（整个数组）
Console.WriteLine(sol.MaxSubArray(new int[] { -3, -1, -2 }));                    // 期望 -1（边界：全负数，必须选一个）

public class Solution {
    public int MaxSubArray(int[] nums) {
        int frist = 0;
        int sum = 0;  //中间值用于记录遇见负数后的总和
        int result = 0;
        while(frist < nums.Length && nums[frist] < 0)
        {
            frist++;
        }
        for(int i = frist;i < nums.Length; i++)
        {
            if(nums[i] >= 0)
            {
                result += nums[i];
            }
            else
            {
                sum = 0;
                while(sum <= 0)
                {
                    sum += nums[i];
                    i++;
                    if(i == nums.Length){return result;}
                }
                result += sum;
            }
        }
        return result;
        // TODO: 找出连续子数组的最大和（子数组至少包含一个元素）
        // 模式提示：还是「遍历时维护历史信息」——
        // 一个变量记「以当前位置结尾的最大子数组和」，一个变量记「全局最大」
    }
}
