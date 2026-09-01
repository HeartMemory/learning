Solution sol = new Solution();
int[] t1 = { 1, 2, 3, 0, 0, 0 };
sol.Merge(t1, 3, new int[] { 2, 5, 6 }, 3);
Console.WriteLine(string.Join(",", t1)); // 期望 1,2,2,3,5,6

int[] t2 = { 1 };
int[] t2b = { };
sol.Merge(t2, 1, t2b, 0);
Console.WriteLine(string.Join(",", t2)); // 期望 1

int[] t3 = { 0 };
sol.Merge(t3, 0, new int[] { 1 }, 1);
Console.WriteLine(string.Join(",", t3)); // 期望 1（边界：nums1 实际元素为 0 个）

int[] t4 = { 3, 0, 0 };
sol.Merge(t4, 1, new int[] { 1, 2 }, 2);
Console.WriteLine(string.Join(",", t4)); // 期望 1,2,3


public class Solution {
    public void Merge(int[] nums1, int m, int[] nums2, int n) {
        if(m != 0)
        {
            for(int i = n - 1;i >= 0; i--)
            {
                for(int j = m - 1;j >= 0; j--)
                {
                    if(nums1[j] > nums2[i])
                    {
                        nums1[j+i+1] = nums1[j];
                    }
                    else
                    {
                        nums1[j+i+1] = nums2[i];
                        break;
                    }
                }
            }
        }
        else if(n !=0)
        {
            for(int i = 0;i < n; i++)
            {
                nums1[i] = nums2[i];
            }
        }
        // TODO: nums1 的前 m 个是有效元素，后面预留了 n 个 0 的空位；
        // 把 nums2 合并进去，结果仍要有序（原地修改，不返回值）
    }
}
