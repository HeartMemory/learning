Solution sol = new Solution();
Console.WriteLine(sol.FindMaxConsecutiveOnes(new int[] { 1, 1, 0, 1, 1, 1 })); // 期望 3
Console.WriteLine(sol.FindMaxConsecutiveOnes(new int[] { 0, 0, 1, 0 }));       // 期望 1
Console.WriteLine(sol.FindMaxConsecutiveOnes(new int[] { 0, 0 }));             // 期望 0

public class Solution {
    public int FindMaxConsecutiveOnes(int[] nums) {
        int max = 0;
        int result = 0;
        for(int i = 0; i < nums.Length; i++)
        {
            if(nums[i] == 1)
            {
                max += 1;
            }
            else if(max >= nums.Length / 2)
            {
                return max;
            }
            else if(max > result)
            {
                result = max;
                max = 0;
            }else{max = 0;}
        }
        if(max > result)
            {
                result = max;
            }
        return result;
    }
}
