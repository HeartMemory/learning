// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.MajorityElement(new[] { 3, 2, 3 }));                   // 期望 3 ★ 最小规模（n=3）
Console.WriteLine(sol.MajorityElement(new[] { 2, 2, 1, 1, 1, 2, 2 }));       // 期望 2 ★ 官方经典样例
Console.WriteLine(sol.MajorityElement(new[] { 1 }));                         // 期望 1 ★ 单元素边界
Console.WriteLine(sol.MajorityElement(new[] { 7, 7, 7, 7 }));                // 期望 7 ★ 全同
Console.WriteLine(sol.MajorityElement(new[] { 1, 1, 2, 3 }));                // 期望 1 ★ 答案在【开头】
Console.WriteLine(sol.MajorityElement(new[] { 2, 3, 4, 5, 5, 5, 5 }));       // 期望 5 ★★ 答案在【末尾】
Console.WriteLine(sol.MajorityElement(new[] { 0, 0, 0, 1 }));                // 期望 0 ★★ 答案是 0（数值边界）
Console.WriteLine(sol.MajorityElement(new[] { -1, -1, -1, 2, 2 }));          // 期望 -1 ★ 负数
Console.WriteLine(sol.MajorityElement(new[] { 1, 2, 1, 2, 1, 2, 1 }));       // 期望 1 ★★ 两个值交替出现
Console.WriteLine(sol.MajorityElement(new[] { 1, 2, 3, 4, 5, 5, 5, 5, 5 })); // 期望 5 ★ 长数组，多数在尾部连着出现

// ═══════ 反例区（09-24 补：穷举 n≤11 的合法输入抓出来的 WA，前 10 组全都是"友好形状"漏掉的）═══════
Console.WriteLine(sol.MajorityElement(new[] { 1, 1, 2, 2, 2 }));       // 期望 2 ★★★ 多数在【后半段反超】
Console.WriteLine(sol.MajorityElement(new[] { 1, 1, 1, 2, 2, 2, 2 })); // 期望 2 ★★★ 同上，n=7 版本
Console.WriteLine(sol.MajorityElement(new[] { 3, 3, 4, 4, 4 }));       // 期望 4 ★★★ 换个值，确认不是数值巧合

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 返回那个出现次数【严格超过 n/2】的元素（题目保证它一定存在）。
    //
    //   ★ 题目给的那把钥匙：「出现次数 > n/2」——不是"最多"，是"**超过一半**"。
    //     这一个字之差，决定了这题能做到 O(1) 额外空间；普通的"求众数"做不到。
    //
    //   ── 复杂度目标：**O(n) 时间、O(1) 空间**
    //
    //   ── 验收标准（写完自己核对）：
    //      ① 13 组全绿 —— 特别盯**反例区那 3 组**：它们是 09-24 用「没有证明的提前剪枝」栽过的形状；
    //      ② 形状覆盖自查：答案在开头 / 末尾 / 中间 / 单元素 / 全同 / 负数 / 答案是 0 / 两值交替
    //         —— 每种形状对应算法里一条不同的分支，缺哪种 = 漏测一条路径；
    //      ③ ★ 写下任何"提前 return / 提前剪枝"之前，先问自己：**"我凭什么认为它是对的？"**
    //         （09-24 的假 AC 就出在这一步：剪枝看起来合理，但拿穷举反例一打就露馅）
    //
    //   ── 进阶（AC 之后想，面试高频追问）：
    //      ① 如果题目**不保证**多数元素存在，你的算法还能用吗？要怎么补救？
    //      ② 如果要求找"出现次数 > n/3"的**所有**元素呢？
    //
    //   ⚠️ 本文件是【盲写版】：09-24 写的三条路线（排序 / 哈希 / 摩尔投票）提示 + 完整推导
    //      + 实现坑提示，已全部移除，只保留题目、目标、形状与验收。
    //      想对照旧版 → 先 `git log --oneline -- LeetCode/0169-MajorityElement/Program.cs`
    //                   找到那次提交，再 `git show <哈希>:LeetCode/0169-MajorityElement/Program.cs`
    public int MajorityElement(int[] nums) {
        int result = nums[0];
        int times = 0;
        foreach(int n in nums)
        {
            if(times == 0) result = n;
            if(n == result) times++;
            else times--;
        }
        return result;
    }
}
