Solution sol = new Solution();
Console.WriteLine(sol.FindTheDifference("abcd", "abcde")); // 期望 e
Console.WriteLine(sol.FindTheDifference("", "y"));         // 期望 y
Console.WriteLine(sol.FindTheDifference("a", "aa"));       // 期望 a（边界：多的是重复字母）
Console.WriteLine(sol.FindTheDifference("ae", "eaa"));     // 期望 a（顺序打乱）

public class Solution {
    public char FindTheDifference(string s, string t) {
        int XOR = 0;
        for(int i = 0;i < s.Length; i++)
        {
            XOR ^= s[i];
            XOR ^= t[i];
        }
        XOR ^= t[t.Length-1];
        return (char)XOR;
        // TODO: t 是 s 打乱后多加了一个字母，找出这个字母
        // 思路 A：int[26] 计数（t 的 +1，s 的 -1，剩下非 0 的就是它）——242 的镜像
        // 思路 B：全字符异或（进阶：相同数异或 = 0，最后剩的就是多的那个）
    }
}
