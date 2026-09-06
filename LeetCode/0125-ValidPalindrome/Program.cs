Solution sol = new Solution();
Console.WriteLine(sol.IsPalindrome("A man, a plan, a canal: Panama")); // 期望 true
Console.WriteLine(sol.IsPalindrome("race a car"));                     // 期望 false
Console.WriteLine(sol.IsPalindrome(" "));                              // 期望 true
Console.WriteLine(sol.IsPalindrome("0P"));                             // 期望 false

public class Solution {
    public bool IsPalindrome(string s) {
        char a;
        char b;
        int left = 0;
        int right = s.Length-1;
        while(left < right)
        {
            if (char.IsLetter(s[left]))
            {
                a = char.ToLower(s[left]);
            }else if (char.IsNumber(s[left]))
            {
                a = s[left];
            }
            else
            {
                left++;
                continue;
            }
            if (char.IsLetter(s[right]))
            {
                b = char.ToLower(s[right]);
            }else if (char.IsNumber(s[right]))
            {
                b = s[right];
            }
            else
            {
                right--;
                continue;
            }
            if(a != b)
            {
                return false;
            }
            else
            {
                left++;
                right--;
            }
        }
        return true;
        // 零额外空间版：禁止 List<char>，双指针直接在原串上走
        // 思路：left/right 各自跳过非字母数字字符，都停在合法字符上后比较（不等 return false）
        // 提醒：跳过动作是「就地 while」——注意别让指针越界
    }
}
