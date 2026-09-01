double answer;
Console.WriteLine("请输入第一个数字:");
double a = double.Parse(Console.ReadLine());
Console.WriteLine("请输入第二个数字:");
double b = Convert.ToDouble(Console.ReadLine());
Console.WriteLine("1.加法 2.减法 3.乘法 4.除法 5.取余");
Console.WriteLine("请输入数字选择");
int n = Convert.ToInt32(Console.ReadLine());
switch (n)
{
    case 1:answer = a + b;Console.WriteLine(a + "+" + b + "=" + answer);break;
    case 2:answer = a - b;Console.WriteLine(a + "-" + b + "=" + answer);break;
    case 3:answer = a * b;Console.WriteLine(a + "*" + b + "=" + answer);break;
    case 4:if(b == 0)
        {
            Console.WriteLine("除数不能为0");
        }else{answer = a / b;Console.WriteLine(a + "/" + b + "=" + answer);}
        break;
    case 5:answer = a % b;Console.WriteLine(a + "%" + b + "=" + answer);break;
    default:Console.WriteLine("输入不合法");break;
}