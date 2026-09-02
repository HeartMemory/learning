// ═══════ 方法调用示例 ═══════
Console.WriteLine("--- CountChar ---");
Console.WriteLine(CountChar("hello world", 'l'));   // 期望 3
Console.WriteLine(CountChar("aaa", 'b'));           // 期望 0（边界：找不到）

Console.WriteLine("--- IsPalindrome ---");
Console.WriteLine(IsPalindrome("level"));           // 期望 True
Console.WriteLine(IsPalindrome("hello"));           // 期望 False
Console.WriteLine(IsPalindrome(""));                // 期望 True（边界：空串）

Console.WriteLine("--- Reverse ---");
Console.WriteLine(Reverse("abc"));                  // 期望 cba
Console.WriteLine($"\"{Reverse("")}\"");            // 期望 ""（边界：空串）

Console.WriteLine("--- ReadNumber（试试先输个字母再输数字）---");
double a = ReadNumber("请输入第一个数字：");
double b = ReadNumber("请输入第二个数字：");
Console.WriteLine($"{a} + {b} = {a + b}");

static int CountChar(string text, char target)
{
    int times = 0;
    for(int i = 0;i < text.Length; i++)
    {
        if(text[i] == target)
        {
            times++;
        }
    }
    return times;
}

static bool IsPalindrome(string text)
{
    int left = 0;
    int right = text.Length-1;
    while(left < right)
    {
        if(text[left] != text[right])
        {
            return false;
        }
        left++;
        right--;
    }
    return true;
}

static string Reverse(string text)
{
    int left = 0;
    int right = text.Length-1;
    char[] chars = text.ToCharArray();
    while(left < right)
    {
        char temp = chars[left];
        chars[left] = chars[right];
        chars[right] = temp;
        left++;
        right--;
    }
    return new string(chars);
}

static double ReadNumber(string prompt)
{
    Console.WriteLine(prompt);
    double num = 0;
    while(!double.TryParse(Console.ReadLine(),out num))
    {
        Console.WriteLine("输入不合法，请重新输入");
    }
    return num;
}
