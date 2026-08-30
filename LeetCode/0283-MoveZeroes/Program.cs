Solution sol = new Solution();
int[] t1 = { 0, 1, 0, 3, 12 };
sol.MoveZeroes(t1);
Console.WriteLine(string.Join(",", t1)); // 期望 1,3,12,0,0

int[] t2 = { 0 };
sol.MoveZeroes(t2);
Console.WriteLine(string.Join(",", t2)); // 期望 0

int[] t3 = { 0, 0, 1 };
sol.MoveZeroes(t3);
Console.WriteLine(string.Join(",", t3)); // 期望 1,0,0

int[] t4 = { 1, 2, 3 };
sol.MoveZeroes(t4);
Console.WriteLine(string.Join(",", t4)); // 期望 1,2,3（边界：没有0）

int[] t5 = { 1, 0, 2 };
sol.MoveZeroes(t5);
Console.WriteLine(string.Join(",", t5)); // 期望 1,2,0（边界：0在中间）

public class Solution {
    public void MoveZeroes(int[] nums) {
        int zero = 0;
        for(int i = 0;i < nums.Length; i++)
        {
            if(nums[i] == 0)
            {
                zero += 1;
            }else if(zero != 0)
            {
                nums[i - zero] = nums[i];
                nums[i] = 0;
            }
        }
        // 复盘盲写：凭记忆重写「零计数器 + 覆盖写」解法
        // 提醒自己：zero == 0 时能不能清零当前位？
    }
}
