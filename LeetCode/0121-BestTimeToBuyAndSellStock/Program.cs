Solution sol = new Solution();
Console.WriteLine(sol.MaxProfit(new int[] { 7, 1, 5, 3, 6, 4 }));     // 期望 5
Console.WriteLine(sol.MaxProfit(new int[] { 7, 6, 4, 3, 1 }));        // 期望 0
Console.WriteLine(sol.MaxProfit(new int[] { 1, 2 }));                 // 期望 1
Console.WriteLine(sol.MaxProfit(new int[] { 5, 6, 1, 2 }));           // 期望 1（昨天打脸你的反例）
Console.WriteLine(sol.MaxProfit(new int[] { 3, 2, 6, 5, 0, 3 }));     // 期望 4（昨天打脸你的反例）

public class Solution {
    public int MaxProfit(int[] prices) {
        int max = 0;
        int min = 0;
        for(int i = 1;i < prices.Length; i++)
        {
            if(prices[min] > prices[i])
            {
                min = i;
            }else if(prices[i] - prices[min] > max)
            {
                max = prices[i] - prices[min];
            }
        }
        return max;
        // 复盘盲写：凭记忆重写「单向遍历 + 历史最低价」解法
    }
}
