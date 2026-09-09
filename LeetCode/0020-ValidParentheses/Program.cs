// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
Console.WriteLine(sol.IsValid("()"));     // 期望 True
Console.WriteLine(sol.IsValid("()[]{}")); // 期望 True
Console.WriteLine(sol.IsValid("(]"));     // 期望 False
Console.WriteLine(sol.IsValid("([)]"));   // 期望 False
Console.WriteLine(sol.IsValid("{[]}"));   // 期望 True
Console.WriteLine(sol.IsValid("]"));      // 期望 False（边界：右括号先来，栈空）
Console.WriteLine(sol.IsValid("("));      // 期望 False（边界：走完还剩左括号）

// ═══════ 类型与工具区（文件底部）═══════
public class Solution {
    // TODO: 判断字符串 s 是否有效——每个左括号必须有同类型右括号按正确顺序闭合
    // API 备忘：Push 压入 / Pop 弹出栈顶 / Peek 看栈顶不弹 / Count == 0 即空栈
    //
    // 思路提示（栈的"就近配对"模型）：
    //   ① 遍历字符串，遇到左括号 ( [ { → 压栈（登记：我在等一个匹配的右括号）
    //   ② 遇到右括号 → 它必须匹配"最近一个未匹配的左括号"= 栈顶！
    //      栈空 or 栈顶对不上 → 直接 return false；对上 → Pop 掉这个左括号
    //   ③ 走完整个字符串后：栈必须为空（还剩 = 有左括号没等到闭合）
    //
    // 技巧：压栈时压"期待出现的那个右括号"（遇到 '(' 就压 ')'），
    //       右括号来了直接 c == stack.Peek() 一句比较，不用写三组 if-else
    public bool IsValid(string s) {
        Stack<char> chars = new Stack<char>();
        foreach(char c in s)
        {
            if(c == '(')
            {
                chars.Push(')');
            }
            else if(c == '[')
            {
                chars.Push(']');
            }else if(c == '{')
            {
                chars.Push('}');
            }
            else
            {
                if(chars.Count == 0) return false;
                char temp = chars.Pop();
                if(c != temp) return false;
            }
        }
        return chars.Count == 0;
    }
}
