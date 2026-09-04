Solution sol = new Solution();
Console.WriteLine(sol.RomanToInt("III"));     // 期望 3
Console.WriteLine(sol.RomanToInt("IX"));      // 期望 9（小值在大值左边 = 减）
Console.WriteLine(sol.RomanToInt("LVIII"));   // 期望 58（L=50 V=5 III=3）
Console.WriteLine(sol.RomanToInt("MCMXCIV")); // 期望 1994（M CM XC IV：大减小组合连环）

public class Solution {
    public int RomanToInt(string s) {
        List<int> list = new List<int>();
        for(int i = 0;i < s.Length; i++)
        {
            if(s[i] == 'I')
            {
                list.Add(1);
            }else if(s[i] =='V')
            {
                list.Add(5);
            }else if(s[i] == 'X')
            {
                list.Add(10);
            }else if(s[i] == 'L')
            {
                list.Add(50);
            }else if(s[i] == 'C')
            {
                list.Add(100);
            }else if(s[i] == 'D')
            {
                list.Add(500);
            }else if(s[i] == 'M')
            {
                list.Add(1000);
            }
        }
        int answer = list[list.Count-1];
        for(int i = list.Count-2;i >= 0; i--)
        {
            if(list[i+1] > list[i])
            {
                answer -= list[i];
            }
            else
            {
                answer += list[i];
            }
        }
        return answer;
        // TODO: 罗马数字转整数
        // 规则：I=1 V=5 X=10 L=50 C=100 D=500 M=1000
        // 特殊：小值在大值左边时做减法（IV=4, IX=9, XL=40, XC=90, CD=400, CM=900）
        // 思路提示：逐字符累加；如果当前值 < 右边值，就减它；否则加它
    }
}
