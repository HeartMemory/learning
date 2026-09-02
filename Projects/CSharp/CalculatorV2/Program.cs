List<string> list = new List<string>();
double answer = 0;
while (true)
{
    ShowMenu();
    int n = Choice("请输入数字选择");
    if(n == 0)
    {
        break;
    }
    else
    {
        double num1 = ReadNumber("请输入第一个数字");
        double num2 = ReadNumber("请输入第二个数字");
        switch (n)
        {
            case 1:
                answer = Add(num1,num2);
                Console.WriteLine($"{num1}+{num2}={answer}");
                list.Add($"{num1}+{num2}={answer}");
                break;
            case 2:
                answer = Subtract(num1,num2);
                Console.WriteLine($"{num1}-{num2}={answer}");
                list.Add($"{num1}-{num2}={answer}");
                break;
            case 3:
                answer = Multiply(num1,num2);
                Console.WriteLine($"{num1}*{num2}={answer}");
                list.Add($"{num1}*{num2}={answer}");
                break;
            case 4:
                num2 = ZeroJudge(num2);
                answer = Divide(num1,num2);
                Console.WriteLine($"{num1}/{num2}={answer}");
                list.Add($"{num1}/{num2}={answer}");
                break;
            case 5:
                num2 = ZeroJudge(num2);
                answer = Mod(num1,num2);
                Console.WriteLine($"{num1}%{num2}={answer}");
                list.Add($"{num1}%{num2}={answer}");
                break;
            case 6:
                if(list.Count == 0)
                {
                    Console.WriteLine("未进行过运算");
                }
                else
                {
                    for(int i = 0;i < list.Count; i++)
                    {
                        Console.WriteLine(list[i]);
                    }
                }
                break;
        }
    }

}


static int Choice(string prompt)
{
    Console.WriteLine(prompt);
    while (true)
    {
        string line = Console.ReadLine();
        if(line == null)
        {
            return 0;
        }
        if(int.TryParse(line, out int num) && num >= 0 && num <= 6)
        {
            return num;
        }
        Console.WriteLine("输入不合法，请重新输入");
    }
}

static double ReadNumber(string prompt)
{
    Console.WriteLine(prompt);
    while (true)
    {
        string line = Console.ReadLine();
        if(line == null)
        {
            return 0;
        }
        if(double.TryParse(line, out double num))
        {
            return num;
        }
         Console.WriteLine("输入不合法，请重新输入");
    }
}

static void ShowMenu()
{
    Console.WriteLine("计算器菜单");
    Console.WriteLine("0.退出 1.加法 2.减法 3.乘法 4.除法 5.取余 6.历史记录");
}

static double Add(double num1, double num2)
{
    return num1 + num2;
}

static double Subtract(double num1, double num2)
{
    return num1 - num2;
}

static double Multiply(double num1, double num2)
{
    return num1 * num2;
}

static double Divide(double num1, double num2)
{
    return num1 / num2;
}

static double Mod(double num1, double num2)
{
    return num1 % num2;
}

static double ZeroJudge(double num)
{
    while(num == 0)
    {
        num = ReadNumber("除数不为0，请重新输入除数");
    }
    return num;
}
