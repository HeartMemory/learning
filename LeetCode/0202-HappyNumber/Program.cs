// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.IsHappy(19));        // 期望 True  （1²+9²=82 → 68 → 100 → 1）
Console.WriteLine(sol.IsHappy(1));         // 期望 True  （边界：一开始就是 1）
Console.WriteLine(sol.IsHappy(100));       // 期望 True
Console.WriteLine(sol.IsHappy(7));         // 期望 True
Console.WriteLine(sol.IsHappy(1111111));   // 期望 True  （7 个 1 → 7 → …；大数练一下不溢出）
Console.WriteLine(sol.IsHappy(2));         // 期望 False （会掉进 4→16→37→58→89→145→42→20→4 的死循环）
Console.WriteLine(sol.IsHappy(4));         // 期望 False （4 正好是那个循环里的数）

// ═══════ 类型区 ═══════
public class Solution {
    // ── 题面 ───────────────────────────────────────────────────────────
    // 判断 n 是不是「快乐数」：反复把 n 换成「各位数字的平方和」，
    //   最终变成 1 → 快乐数；若**永远变不到 1**（说明走进了循环）→ 不是。
    //
    // ── 实现（09-20 复盘日盲写版，结构比初版更防守）────────────────────
    // · 判环：HashSet 登记"走过的数"；`Add` 的返回值一步完成「查 + 登记」
    // · 顺序：**先判出口（`n == 1`）→ 再变换 → 再登记新值**
    //     出口判断放在变换之前 ⇒ 每轮都对"当前值"做判断，
    //     因此【不依赖"起点是否登记"】（初版"先变换再登记"能跑对，但属侥幸）
    // · 取位：`% 10` 取个位、`/ 10` 去掉个位 —— 终止条件必须 `> 0`
    //     （写 `> 10` 会丢个位：n 只剩一位时整轮被跳过）
    // · 平方：写成 `d * d`；C# 的 `^` 是**位异或**，不是乘方
    // · 题眼："永远到不了 1" ⇔ **某个值第二次出现**（值域有界 → 环必然被撞上）
    public bool IsHappy(int n) {
        
        HashSet<int> hash = new HashSet<int>();
        while (true)
        {
            if(n == 1) return true;
            int result = 0;
            while(n > 0)
            {
                int temp = (n % 10) * (n % 10);
                result += temp;
                n /= 10;
            }
            if(!hash.Add(result)) return false;
            n = result;
        }
    }
}
