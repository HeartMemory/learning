// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.FirstUniqChar("leetcode"));      // 期望 0   （l 只出现一次，且在最前面）
Console.WriteLine(sol.FirstUniqChar("loveleetcode"));  // 期望 2   （v 是第一个"只出现一次"的）
Console.WriteLine(sol.FirstUniqChar("z"));             // 期望 0   （边界：只有 1 个字符）
Console.WriteLine(sol.FirstUniqChar("aabb"));          // 期望 -1  （全是成对的）
Console.WriteLine(sol.FirstUniqChar("abcabcde"));      // 期望 6   （a/b/c 各两次，d 在 6 号位）
Console.WriteLine(sol.FirstUniqChar("aadadaad"));      // 期望 -1  （易错：别只看"第一次出现位置"）
Console.WriteLine(sol.FirstUniqChar(""));              // 期望 -1  （空串边界）

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 找到第一个「只出现一次」的字符，返回它的下标；不存在就返回 -1。
    //
    //   关键词：**第一个**。
    //   想一想为什么本题很自然要**走两趟**：
    //     第一趟 → 统计每个字符出现次数（信息不全，不知道谁是"第一个"）
    //     第二趟 → 从下标 0 开始重新扫，碰到的**第一个**计数为 1 的字符就是答案
    //   —— 一趟到底行不行？如果你一遍遍历就返回，会漏掉什么？
    //
    //   ★ 两个容器选择，和昨天 383 一样要自己权衡：
    //     ① int[26]：下标 `s[i] - 'a'`，只适用于"只有小写字母"（本题约束正好满足）
    //     ② Dictionary<char,int>：更通用，也能统计别的字符集
    //   用你昨天写 383 时总结的标准来选，并说出理由。
    //
    //   边界自检：空串会不会崩？"—"这种没有唯一字符的情况会不会误返回 0？
    public int FirstUniqChar(string s) {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        foreach(char c in s)
        {
            dict.TryGetValue(c,out int n);
            dict[c] = n + 1;
        }
        for(int i = 0;i < s.Length; i++)
        {
            dict.TryGetValue(s[i],out int n);
            if(n == 1) return i;
        }
        return -1;
    }
}
