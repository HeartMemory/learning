using System;
using System.Diagnostics.CodeAnalysis;


int add = 0;  //累加器
int mid = 0;  //中间值，乘法表里的乘积
int result = 0;  //数字总和
int sum = 0;  //每次输入的数字，用于累加
for(int i = 1;i <= 100; i++)
{
    add +=i;
}
Console.WriteLine(add);

for(int i = 1;i < 10; i++)
{
    for(int j = 1;j <= i; j++)
    {
        mid = i * j;
        Console.Write(j + "*" + i + "=" + mid + "\t");
    }
    Console.WriteLine();
}

do
{
    sum = int.Parse(Console.ReadLine());
    result += sum;
}while(sum != 0);
Console.WriteLine("数字总和是"+result);