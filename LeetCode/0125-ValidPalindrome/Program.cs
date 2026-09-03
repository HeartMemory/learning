Solution sol = new Solution();
Console.WriteLine(sol.IsPalindrome("A man, a plan, a canal: Panama")); // 期望 true（过滤后 amanaplanacanal）
Console.WriteLine(sol.IsPalindrome("race a car"));                     // 期望 false
Console.WriteLine(sol.IsPalindrome(" "));                              // 期望 true（边界：过滤后为空串）
Console.WriteLine(sol.IsPalindrome("0P"));                             // 期望 false（经典坑：'0' ≠ 'p'）

public class Solution {
    public bool IsPalindrome(string s) {
        List<char> chars = new List<char>();
        for(int i = 0;i < s.Length;i++)
        {
            if (char.IsLetterOrDigit(s[i]))
            {
                chars.Add(char.ToLower(s[i]));
            }else if (char.IsNumber(s[i]))
            {
                chars.Add(s[i]);
            }
        }
        int left = 0;
        int right = chars.Count-1;
        while(left < right)
        {
            if(chars[left] != chars[right])
            {
                return false;
            }
            left++;
            right--;
        }
        return true;
        // TODO: 只考虑字母和数字、忽略大小写，判断是否回文
        // 提示：char.IsLetterOrDigit 过滤 + char.ToLower 统一 + 双指针
    }
}
