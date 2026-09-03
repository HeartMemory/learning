Solution sol = new Solution();
char[] t1 = { 'h', 'e', 'l', 'l', 'o' };
sol.ReverseString(t1);
Console.WriteLine(string.Join(",", t1)); // 期望 o,l,l,e,h

char[] t2 = { 'H', 'a', 'n', 'n', 'a', 'h' };
sol.ReverseString(t2);
Console.WriteLine(string.Join(",", t2)); // 期望 h,a,n,n,a,H

public class Solution {
    public void ReverseString(char[] s) {
        int left = 0;
        int right = s.Length-1;
        while(left < right)
        {
            char char1 = s[left];
            s[left] = s[right];
            s[right] = char1;
            left++;
            right--;
        }
        // TODO: 原地反转字符数组（不许用 ToCharArray/新建数组——你已经会双指针交换）
    }
}
