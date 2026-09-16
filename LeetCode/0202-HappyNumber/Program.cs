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
    // TODO: 判断 n 是不是「快乐数」——
    //       反复把 n 换成「各位数字的平方和」，如果最终变成 1 就是快乐数；
    //       如果**永远变不到 1**（说明走进了循环）就不是。
    //
    //   ★ 本题的题眼不是"算平方和"（那是体力活），而是：
    //     你怎么知道它"永远"到不了 1？
    //
    //   两条路，建议都想过再动手：
    //     ① HashSet 路线（哈希线正题）：把每一步算出来的数登记进 HashSet，
    //        某一步算出来的数**已经在集合里** → 说明回到老路了 → 循环 → false。
    //        （提示：你昨天在 217 用的 `Add` 返回值一招，这里同样好用）
    //     ② 快慢指针路线（你 141 判圈的原班人马）：把"平方和"当成链表的下一个节点，
    //        一个一次走 1 步、一个一次走 2 步，相遇即循环。
    //
    //   自己选一条实现；另一条当今天的思考题——**为什么这两条路都能判环？**
    //
    //   小提示：取各位数字用 % 10 和 / 10 配合（66 加一你写过类似的）；
    //           注意循环里 n 被改掉之前，先把平方和存进另一个变量。
    public bool IsHappy(int n) {
        HashSet<int> judge = new HashSet<int>();
        while (true)
        {
            int temp = 0;
            while(n > 0)
            {
                temp += (n % 10)*(n % 10);
                n /= 10;
            }
            n = temp;
            if(!judge.Add(n)) return false;
            if(n == 1) return true;
        }
    }
}
