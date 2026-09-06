Solution sol = new Solution();
Console.WriteLine(sol.RomanToInt("III"));     // 期望 3
Console.WriteLine(sol.RomanToInt("IX"));      // 期望 9
Console.WriteLine(sol.RomanToInt("LVIII"));   // 期望 58
Console.WriteLine(sol.RomanToInt("MCMXCIV")); // 期望 1994

public class Solution {
    public int RomanToInt(string s) {
        int answer = 0;
        for(int i = 0;i < s.Length;i++)
        {
            if(s[i] == 'M')
            {
                answer += 1000;
            }else if(s[i] == 'D')
            {
                if(i+1 < s.Length && s[i+1] == 'M')
                {
                    answer -= 500;
                }
                else
                {
                    answer += 500;
                }
            }else if(s[i] == 'C')
            {
                if(i+1 < s.Length && (s[i+1] == 'D' || s[i+1] == 'M'))
                {
                    answer -= 100;
                }
                else
                {
                    answer += 100;
                }
            }else if(s[i] == 'L')
            {
                if(i+1 < s.Length && (s[i+1] == 'C' || s[i+1] == 'D' || s[i+1] == 'M'))
                {
                    answer -= 50;
                }
                else
                {
                    answer += 50;
                }
            }else if(s[i] == 'X')
            {
                if(i+1 < s.Length && (s[i+1] == 'L' || s[i+1] == 'C' || s[i+1] == 'D' || s[i+1] == 'M'))
                {
                    answer -= 10;
                }
                else
                {
                    answer += 10;
                }
            }else if(s[i] == 'V')
            {
                if(i+1 < s.Length && (s[i+1] == 'X' || s[i+1] == 'L' || s[i+1] == 'C' || s[i+1] == 'D' || s[i+1] == 'M'))
                {
                    answer -= 5;
                }
                else
                {
                    answer += 5;
                }
            }else if(s[i] == 'I')
            {
                if(i+1 < s.Length && (s[i+1] == 'V' || s[i+1] == 'X' || s[i+1] == 'L' || s[i+1] == 'C' || s[i+1] == 'D' || s[i+1] == 'M'))
                {
                    answer -= 1;
                }
                else
                {
                    answer += 1;
                }
            }
        }
        return answer;
        // List 戒断版：禁止任何 List，单遍循环 + 一个 answer 变量
        // 思路：从左往右走，当前值 < 右边值 → 它是减数；否则是加数
    }
}
