// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.SingleNumber(new[] { 2, 2, 1 }));                          // 期望 1
Console.WriteLine(sol.SingleNumber(new[] { 4, 1, 2, 1, 2 }));                    // 期望 4 ★ 答案在开头
Console.WriteLine(sol.SingleNumber(new[] { 1 }));                                // 期望 1 ★ 单元素边界
Console.WriteLine(sol.SingleNumber(new[] { 1, 1, 2 }));                          // 期望 2 ★ 答案在末尾
Console.WriteLine(sol.SingleNumber(new[] { 0, 0, 7 }));                          // 期望 7 ★ 0 参与运算的边界
Console.WriteLine(sol.SingleNumber(new[] { -1, -1, -2 }));                       // 期望 -2 ★ 负数（异或对符号位同样成立）
Console.WriteLine(sol.SingleNumber(new[] { 5, 1, 5 }));                          // 期望 1 ★ 答案在中间
Console.WriteLine(sol.SingleNumber(new[] { 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1 })); // 期望 7 ★ 长数组，答案正中间

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 返回那个只出现了一次的数字（其余都恰好出现两次）。
    //
    //   题目给了三条约束，等于三张路标：
    //     ① 线性时间 O(n)   → 不能排序（O(n log n) 已经超了）
    //     ② 常量额外空间 O(1) → 不能开哈希表/集合
    //     ③ 其余元素"恰好出现两次" ← 这个"两次"不是随便写的，它在暗示一种【自己抵消自己】的运算
    //
    //   ── 路线 A：哈希（先拿 AC 用，10 秒能写完，但不是本题要练的）
    //     你已经会的两种：
    //       ① 计数：Dictionary 数一遍，再找计数为 1 的（O(n) 空间）
    //       ② "遇到就加、再遇到就删"：HashSet，Add 返回 false 说明是第二次 → Remove
    //          → 遍历完集合里只剩那一个（想想为什么它能自动成立）
    //     缺点：① 和 ② 都占了 O(n) 空间，不满足题目的 ② 号路标。
    //
    //   ── 路线 B：位运算（⭐ 今天该写的，满足 O(n) + O(1)）
    //     你在 389（找不同）用过同一招，先把异或的三条性质回想清楚：
    //       性质 1：a ^ a = ?     （同一个数异或自己）
    //       性质 2：a ^ 0 = ?     （跟 0 异或）← 用例 5（0 参与）就是来验证这条的
    //       性质 3：交换律、结合律成立吗？
    //     然后把这三个问题连起来想：
    //       "把数组里所有数字全部异或起来"——那些成对的会不会自动消失？剩下的会是谁？
    //     自己推一遍（拿 {2,2,1} 手算：2^2^1 = ?），推通了再写代码。
    //
    //   ── 路线 C：数学（只当智力题）
    //     2 × sum(去重后的集合) − sum(全部数字) = 那个落单的数。为什么成立？
    //     代价：要一个集合存去重结果（O(n) 空间），而且大数会溢出——面试会被追问这两点。
    //
    //   ★ 今天的验收标准：路线 B 写出来，且 8 组用例全绿。
    //     写之前先答应我一件事：写完别只看"全绿"，回头核对一遍"边界形状"
    //     （答案是开头 / 末尾 / 中间 / 0 / 负数 / 单元素）——昨天 219 就是栽在"用例形状不全"上。
    public int SingleNumber(int[] nums) {
        int result = 0;
        foreach(int a in nums)
        {
            result ^= a;
        }
        return result;
    }
}
