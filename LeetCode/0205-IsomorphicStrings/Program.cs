// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.IsIsomorphic("egg", "add"));      // 期望 True
Console.WriteLine(sol.IsIsomorphic("paper", "title"));  // 期望 True  （p→t, a→i, e→l, r→e）
Console.WriteLine(sol.IsIsomorphic("abab", "baba"));    // 期望 True
Console.WriteLine(sol.IsIsomorphic("a", "a"));          // 期望 True  （单字符边界）
Console.WriteLine(sol.IsIsomorphic("", ""));            // 期望 True  （空串边界）
Console.WriteLine(sol.IsIsomorphic("foo", "bar"));      // 期望 False （o 一会儿→a 一会儿→r，自己打自己）
Console.WriteLine(sol.IsIsomorphic("ab", "aa"));        // 期望 False （a、b 两个字符都映射到 a）
Console.WriteLine(sol.IsIsomorphic("badc", "baba"));    // 期望 False ★ 只查单向会漏掉的那一类

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 判断 s 和 t 是否「同构」——s 里的字符能按一套一致的规则替换成 t 里的字符，且不改变顺序。
    //
    //   把规则拆成两句读三遍（⚠️ 陷阱藏在第二句里）：
    //     ① 同一个字符必须【始终】映射到同一个字符 —— s 方向不能一会儿 a→x 一会儿 a→y
    //     ② 【不同】的字符【不能】映射到同一个字符 —— t 方向不能 b→x 且 d→x
    //
    //   ★ 先回答这个问题再动手：
    //     如果我只用一个 Dictionary<char,char> 记「s 的字符 → t 的字符」，
    //     下面 8 条测试里哪一条会**漏过去**？为什么漏？（提示：看最后一条 "badc"/"baba"）
    //
    //   想明白"为什么会漏"之后，两条路自己权衡：
    //     ① 两个 Dictionary：一个记 s→t，一个记 t→s，两边都查
    //     ② 一个 Dictionary<char,char> + 一个 HashSet<char>：一个管"谁映射过"，
    //        一个管"谁被映射过"——哪个集合存哪一边，自己想清楚
    //
    //   小提示：每条 mapping 都是"先检查、不通过就 return false；通过才登记"。
    //           两个方向的检查顺序会影响结果，写完拿最后一条测试验证。
    //
    //   边界：题目保证 s 和 t 等长；空串/单字符要不要特判？先试试不特判会不会错。
    public bool IsIsomorphic(string s, string t) {
        if(s.Length != t.Length) return false;
        Dictionary<char,char> dict1 = new Dictionary<char, char>();
        Dictionary<char,char> dict2 = new Dictionary<char, char>();
        for(int i = 0;i < s.Length; i++)
        {
            if(dict1.TryGetValue(s[i], out char c))
            {
                if(c != t[i]) return false;
            }
            else
            {
                dict1[s[i]] = t[i];
            }
            if(dict2.TryGetValue(t[i], out char x))
            {
                if(x != s[i]) return false;
            }
            else
            {
                dict2[t[i]] = s[i];
            }
        }
        return true;
    }
}
