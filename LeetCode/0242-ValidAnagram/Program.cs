Solution sol = new Solution();
Console.WriteLine(sol.IsAnagram("anagram", "nagaram")); // 期望 true
Console.WriteLine(sol.IsAnagram("rat", "car"));          // 期望 false
Console.WriteLine(sol.IsAnagram("a", "ab"));             // 期望 false（边界：长度不同）

public class Solution {
    public bool IsAnagram(string s, string t) {
        // TODO: 判断 t 是否是 s 的字母异位词（字母相同、顺序可不同）
        // 提示：int[26] 计数——一个 +1 一个 -1，最后全为 0；或先比长度
    }
}
