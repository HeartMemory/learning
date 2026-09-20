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
    // ── 题面 ───────────────────────────────────────────────────────────
    // 判断 s 和 t 是否「同构」：s 里的字符能按一套一致的规则替换成 t 里的字符，且不改变顺序。
    //   把规则拆成两句读三遍（⚠️ 陷阱藏在第二句里）：
    //     ① 同一个字符必须【始终】映射到同一个字符 —— s 方向不能一会儿 a→x 一会儿 a→y
    //     ② 【不同】的字符【不能】映射到同一个字符 —— t 方向不能 b→x 且 d→x
    //
    // ── 实现（09-20 复盘日盲写版，比初版更紧凑）────────────────────────
    // · 规则①②分属两个方向 ⇒ **两张表**：dict1 守 s→t（同一字符不变心）、
    //     dict2 守 t→s（不同字符不撞车）—— 单表只能守住一个方向的门
    // · 顺序：**先查（TryGetValue）→ 不一致立即 return false → 无条件登记**
    //     登记不放进 else：值相同再写一遍是【幂等】操作，少一层分支、也绝不会漏登记
    // · 边界：长度不等直接 false；空串/单字符时循环不执行 → 天然返回 true
    // · 题眼：最后一条 `"badc" / "baba"` 正是单表会漏掉的形状
    //     （`b` 和 `d` 两个不同字符都映射到 `b`，只有 t→s 的视角才看得出来）
    public bool IsIsomorphic(string s, string t) {

        if(s.Length != t.Length) return false;
        Dictionary<char,char> dict1 = new Dictionary<char, char>();
        Dictionary<char,char> dict2 = new Dictionary<char, char>();
        for(int i = 0;i < s.Length; i++)
        {
            if(dict1.TryGetValue(s[i],out char n1)) if(n1 != t[i]) return false;
            dict1[s[i]] = t[i];
            if(dict2.TryGetValue(t[i],out char n2)) if(n2 != s[i]) return false;
            dict2[t[i]] = s[i];
        }
        return true;
    }
}
