double answer;
int n = 0;
double a = 0;
double b = 0;
List<string> list = new List<string>();
while(true)
{
    Console.WriteLine("0.退出 1.加法 2.减法 3.乘法 4.除法 5.取余 6.历史记录");
    Console.WriteLine("请输入数字选择");
    if(int.TryParse(Console.ReadLine(), out n))
    {
        if(n == 0)
        {
            break;
        }
        if(n != 6)
        {
            Console.WriteLine("请输入第一个数字:");
            if(!double.TryParse(Console.ReadLine(), out a))
            {
                Console.WriteLine("输入非法，请输入数字");
                if(!double.TryParse(Console.ReadLine(), out a))
                {
                    break;
                }
            }
            Console.WriteLine("请输入第二个数字:");
            if(!double.TryParse(Console.ReadLine(), out b))
            {
                Console.WriteLine("输入非法，请输入数字");
                if(!double.TryParse(Console.ReadLine(), out b))
                {
                    break;
                }
            }
        }
        switch (n)
        {
            case 1:
                answer = a + b;Console.WriteLine(a + "+" + b + "=" + answer);
                list.Add(a + "+" + b + "=" + answer);
                break;
            case 2:
                answer = a - b;Console.WriteLine(a + "-" + b + "=" + answer);
                list.Add(a + "-" + b + "=" + answer);
                break;
            case 3:
                answer = a * b;Console.WriteLine(a + "*" + b + "=" + answer);
                list.Add(a + "*" + b + "=" + answer);
                break;
            case 4:
                if(b == 0)
                {
                    Console.WriteLine("除数不能为0");
                }else
                {
                    answer = a / b;Console.WriteLine(a + "/" + b + "=" + answer);
                    list.Add(a + "/" + b + "=" + answer);
                }
                break;
            case 5:
                answer = a % b;Console.WriteLine(a + "%" + b + "=" + answer);
                list.Add(a + "%" + b + "=" + answer);
            break;
            case 6:
                if(list.Count == 0)
                {
                    Console.WriteLine("未进行过计算");
                }
                else
                {
                    for(int i = 0;i < list.Count; i++)
                        {
                            Console.WriteLine(list[i]);
                        }
                }
                break;
            default:
            Console.WriteLine("输入非法");
            break;
        }
    }
    Console.WriteLine("输入任意整数以继续使用计算机");
    if(!int.TryParse(Console.ReadLine(),out _))
    {
        break;
    }
}