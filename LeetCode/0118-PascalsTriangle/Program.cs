Solution sol = new Solution();
foreach (var row in sol.Generate(5))
    Console.WriteLine(string.Join(" ", row));
// 期望：
// 1
// 1 1
// 1 2 1
// 1 3 3 1
// 1 4 6 4 1

public class Solution {
    public IList<IList<int>> Generate(int numRows) {
        IList<IList<int>> result = new  List<IList<int>>();
        for(int i = 0;i < numRows; i++)
        {
            result.Add(new List<int>(new int[i + 1]));
            result[i][0] = 1;
            result[i][i] = 1;
        }
        for(int i = 2;i < numRows; i++)
        {
            for(int j = 1;j < i; j++)
            {
                result[i][j] = result[i-1][j] + result[i-1][j-1];
            }
        }
        return result;
        // TODO: 生成杨辉三角前 numRows 行
        // 每行是一个 List<int>，整体用 List<List<int>> 装
        // 规律：每行首尾是 1，中间元素 = 上一行相邻两数之和
    }
}
