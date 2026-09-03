Console.WriteLine(Tools.Max(3, 5));
Console.WriteLine(Tools.Max(3.5, 2.1));
Console.WriteLine(Tools.Max(3, 5, 9));
Console.WriteLine(Factorial(10));
Console.WriteLine(Factorial(20));
Console.WriteLine(Fibonacci(35));
Console.WriteLine(Fib(35));
Console.WriteLine(Tools.CountChar("aaaaa","aaa"));

static long Fib(int n)
{
    if(n <= 0)
    {
        return 0;
    }else if(n == 1)
    {
        return 1;
    }else
    {
        long a = 0;
        long b = 1;
        long next = 0;
        for(int i = 2;i <= n; i++)
        {
            next = a+b;
            a = b;
            b = next;
        }
        return next;
    }
}

static long Fibonacci(int n)
{
    if(n <= 0)
    {
        return 0;
    }else if(n == 1)
    {
        return 1;
    }else{return Fibonacci(n-1) + Fibonacci(n-2);}
}

static long Factorial(int n)
{
    if(n <= 1)
    {
        return 1;
    }else{return n*Factorial(n-1);}
}

static class Tools
{
    public static int CountChar(string text, char target)
    {
        int times = 0;
        for(int i = 0;i < text.Length;i++)
        {
            if(text[i] == target)
            {
                times++;
            }
        }
        return times;
    }

    public static int CountChar(string text, string target)
    {
        if (string.IsNullOrEmpty(target))
        {
            Console.WriteLine("查找不能为空");
            return 0;
        }
        if(string.IsNullOrEmpty(text) || text.Length < target.Length)
        {
            return 0;
        }
        Console.WriteLine("重叠算多次");
        int times = 0;
        for(int i = 0;i <= text.Length-target.Length;i++)
        {
            bool judge = true;
            if(text[i] == target[0])
            {
                for(int j = 0;j < target.Length;j++)
                {
                    if(text[i+j] != target[j])
                    {
                        judge = false;
                        break;
                    }
                }
                if(judge){times++;}
            }
        }
        return times;
    }

    public static int Max(int a,int b)
    {
        return a > b ? a : b;
    }

    public static double Max(double a,double b)
    {
        return a > b ? a : b;
    }

    public static int Max(int a,int b,int c)
    {
        return Max(Max(a,b),c);
    }
}
