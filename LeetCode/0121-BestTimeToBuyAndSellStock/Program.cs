Solution sol = new Solution();
Console.WriteLine(sol.MaxProfit(new int[] { 7, 1, 5, 3, 6, 4 })); // 期望 5（第2天买1，第5天卖6）
Console.WriteLine(sol.MaxProfit(new int[] { 7, 6, 4, 3, 1 }));    // 期望 0（价格一直跌，不买）
Console.WriteLine(sol.MaxProfit(new int[] { 1, 2 }));             // 期望 1
Console.WriteLine(sol.MaxProfit(new int[] { 5, 6, 1, 2 }));    // 期望 1
Console.WriteLine(sol.MaxProfit(new int[] { 3, 2, 6, 5, 0, 3 })); // 期望 4


public class Solution {
    public int MaxProfit(int[] prices) {
        int min = 0;  //当前最低价
        int result = 0;  //当前利润，没有利润时为0
        int max = 0;  //最大利润，没有利润时为0
        for(int i = 1;i < prices.Length; i++)
        {
            if(prices[min] > prices[i])
            {
                min = i;
            }
            else
            {
                result = prices[i] - prices[min];
            }
            if(result > max)
            {
                max = result;
            }
        }
        return max;
    }
}
