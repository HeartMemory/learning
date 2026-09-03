Solution sol = new Solution();
Console.WriteLine(sol.IsAnagram("anagram", "nagaram")); // 期望 true
Console.WriteLine(sol.IsAnagram("rat", "car"));          // 期望 false
Console.WriteLine(sol.IsAnagram("a", "ab"));             // 期望 false（边界：长度不同）

public class Solution {
    public bool IsAnagram(string s, string t) {
        List<char> chars1 = new List<char>();
        List<char> chars2 = new List<char>();
        for(int i = 0;i < s.Length;i++)
        {
            if (char.IsLetterOrDigit(s[i]))
            {
                chars1.Add(s[i]);
            }
        }
        for(int i = 0;i < t.Length;i++)
        {
            if (char.IsLetterOrDigit(t[i]))
            {
                chars2.Add(t[i]);
            }
        }
        if(chars1.Count != chars2.Count)
        {
            return false;
        }
        int[] english = new int[26];
        int[] ENGLISH = new int[26];
        for(int i = 0;i < chars1.Count; i++)
        {
            if(chars1[i] >= 'a' && chars1[i] <= 'z')
            {
                english[chars1[i] - 'a']++;
            }else if(chars1[i] >= 'A' && chars1[i] <= 'Z')
            {
                ENGLISH[chars1[i] - 'A']++;
            }
            if(chars2[i] >= 'a' && chars2[i] <= 'z')
            {
                english[chars2[i] - 'a']--;
            }else if(chars2[i] >= 'A' && chars2[i] <= 'Z')
            {
                ENGLISH[chars2[i] - 'A']--;
            }
        }
        for(int i = 0;i < 26; i++)
        {
            if(english[i] != 0 || ENGLISH[i] != 0)
            {
                return false;
            }
        }
        return true;
        // TODO: 判断 t 是否是 s 的字母异位词（字母相同、顺序可不同）
        // 提示：int[26] 计数——一个 +1 一个 -1，最后全为 0；或先比长度
    }
}
