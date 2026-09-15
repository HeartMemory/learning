// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.CanConstruct("a", "b"));       // 期望 False（magazine 里根本没有 a）
Console.WriteLine(sol.CanConstruct("aa", "ab"));     // 期望 False（a 只有 1 个，需要 2 个）
Console.WriteLine(sol.CanConstruct("aa", "aab"));    // 期望 True
Console.WriteLine(sol.CanConstruct("", "abc"));      // 期望 True（空赎金信，边界）
Console.WriteLine(sol.CanConstruct("abc", "abc"));   // 期望 True（恰好用完）
Console.WriteLine(sol.CanConstruct("aab", "baa"));   // 期望 True（只看数量，不看顺序）

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 判断 ransomNote 能否用 magazine 里的字符拼出来——每个字符只能用一次
    //
    //   思路方向：先统计 magazine 的字符数量 → 再遍历 ransomNote 逐个"消耗"，
    //             某个字符不够用（数量 < 0）就返回 false
    //
    //   ★ 两种容器选择，自己权衡（这是一个经典判断）：
    //     ① int[26]：用 `magazine[i] - 'a'` 当下标（你在 242 有效的字母异位词用过）
    //        —— 最快、最省内存，但只适用于"连续小整数 key"（正好 26 个小写字母）
    //     ② Dictionary<char, int>：更通用，key 不连续/不是字符也能用
    //        —— 稍慢，但换了题目约束（比如含大写、中文）照样能跑
    //   想一想：本题约束是"只有小写字母"时，哪一个更合适？为什么？
    //
    //   提示：int[26] 本质就是一个"特化的哈希表"——key 是 0~25 的连续整数，
    //         连哈希函数都省了（直接用 key 当下标），所以它比 Dictionary 更快。
    public bool CanConstruct(string ransomNote, string magazine) {
        Dictionary<char,int> dict = new Dictionary<char, int>();
        if(ransomNote.Length > magazine.Length) return false;
        for(int i = 0;i < magazine.Length;i++)
        {
            dict.TryGetValue(magazine[i], out int times);
            dict[magazine[i]] = times + 1;
        }
        for(int i = 0;i < ransomNote.Length; i++)
        {
            dict.TryGetValue(ransomNote[i],out int times);
            if(times == 0) return false;
            dict[ransomNote[i]] = times - 1;
        }
        return true;
    }
}
