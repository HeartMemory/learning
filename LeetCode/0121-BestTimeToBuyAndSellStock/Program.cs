Solution sol = new Solution();
Console.WriteLine(sol.MaxProfit(new int[] { 7, 1, 5, 3, 6, 4 })); // 期望 5（第2天买1，第5天卖6）
Console.WriteLine(sol.MaxProfit(new int[] { 7, 6, 4, 3, 1 }));    // 期望 0（价格一直跌，不买）
Console.WriteLine(sol.MaxProfit(new int[] { 1, 2 }));             // 期望 1
Console.WriteLine(sol.MaxProfit(new int[] { 5, 6, 1, 2 }));    // 期望 1
Console.WriteLine(sol.MaxProfit(new int[] { 3, 2, 6, 5, 0, 3 })); // 期望 4


public class Solution {
    public int MaxProfit(int[] prices) {
        int max = prices.Length-1;  //价格相对高的那一天
        int min = 0;  //价格相对小的那一天
        int result = 0;  //利润
        int may = 0;  //可能存在的更优
        for(int i = prices.Length-1;i > 0; i--)
        {
            if(prices[i] > prices[i - 1] && prices[i] > prices[max])
            {
                max = i;
            }
            if(prices[i] < prices[i - 1] && prices[i] < prices[min])
            {
                min = i;
                if(max > min)
                {
                    result = prices[max] - prices[min];
                }
            }
        }
        if(max > min)
        {
            may = prices[max] - prices[min];
        }
        if(may > result)
        {
            return may;
        }else{return result;}
    }
}
