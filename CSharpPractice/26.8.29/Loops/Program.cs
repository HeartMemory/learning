using System;
using System.Diagnostics.CodeAnalysis;


int add = 0;
int mid = 0;
int result = 0;
int sum = 0;
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
    Console.Write("\n");
}

do
{
    sum = int.Parse(Console.ReadLine());
    result += sum;
}while(sum != 0);
Console.WriteLine("数字总和是"+result);