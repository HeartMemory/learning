// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.WordPattern("abba", "dog cat cat dog"));   // 期望 True
Console.WriteLine(sol.WordPattern("abc", "dog cat bird"));       // 期望 True  （三个都不同，一一对应）
Console.WriteLine(sol.WordPattern("a", "dog"));                  // 期望 True  （单元素边界）
Console.WriteLine(sol.WordPattern("abba", "dog cat cat fish"));  // 期望 False （最后的 cat≠fish，规则①）
Console.WriteLine(sol.WordPattern("aaaa", "dog cat cat dog"));   // 期望 False （一个 a 撞了 4 个词，规则①）
Console.WriteLine(sol.WordPattern("abba", "dog dog dog dog"));   // 期望 False （4 个词都是 dog，规则②：撞车）
Console.WriteLine(sol.WordPattern("abc", "dog cat"));            // 期望 False ★ 数量不等（最容易漏的一条）
Console.WriteLine(sol.WordPattern("ab", "dog dog"));             // 期望 False （b、a 撞车，规则②）

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 判断字符串 s 里的单词排列，是否和 pattern 的"图案"一致。
    //
    //   和昨天 205 同构字符串是【同一道题的文字版】，但多了两个新东西：
    //     ① 左边是 char（pattern 的每个字母），右边是 string（每个单词）—— **两侧类型不同**
    //     ② 要先把 s 拆成单词数组（你 09-04 学的 Split 切缝模型，回来用一次）
    //
    //   规则还是那两条（和 205 完全一样）：
    //     · 同一个 pattern 字符必须【始终】对应同一个单词（a 不能一会儿 dog 一会儿 cat）
    //     · 【不同】的 pattern 字符【不能】对应同一个单词（a→dog 且 b→dog 不行）
    //
    //   ★★ 但本题有个【最容易漏的前提检查】，昨天没有、今天必须加：
    //      先想清楚：如果 pattern 长度 ≠ 单词个数，应该返回什么？为什么必须先检查它？
    //      （提示：看看测试第 7 条 "abc" / "dog cat" —— 如果直接进循环会发生什么？
    //        是"正确返回 False"，还是"数组越界崩溃"？）
    //
    //   动手顺序建议：
    //     第 1 步：用 Split 把 s 拆成单词数组（思考：`s.Split(' ')` 还是 `s.Split()`？两者有什么区别？）
    //     第 2 步：做那个前提检查
    //     第 3 步：双字典双向守门（注意两张表的类型：一张 key 是 char，一张 key 是 string）
    //
    //   边界：pattern 是空串怎么办？（题目保证不为空，但你可以试试不特判会怎样）
    public bool WordPattern(string pattern, string s) {
        if(pattern.Length == 0 || s.Length == 0) return false;
        string[] s1 = s.Split(' ');
        if(s1.Length != pattern.Length) return false;  // 没有做连续空格出现空字符的防御
        Dictionary<char,string> dict1 = new Dictionary<char, string>();
        Dictionary<string,char> dict2 = new Dictionary<string, char>();
        for(int i = 0;i < pattern.Length; i++)
        {
            if(!dict2.TryGetValue(s1[i], out char n))
            {
                dict2[s1[i]] = pattern[i];
            }else if(n != pattern[i]) return false;
            if(!dict1.TryGetValue(pattern[i], out string n1))
            {
                dict1[pattern[i]] = s1[i];
            }else if(n1 != s1[i]) return false;
        }
        return true;
    }
}
