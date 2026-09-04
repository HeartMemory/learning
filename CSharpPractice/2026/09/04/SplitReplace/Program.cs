string s = "a   a b cd";
string[] words = s.Split(' ');
for(int i = 0;i < words.Length; i++)
{
    if(words[i] != "")
    {
        Console.WriteLine(words[i]);
    }
}

Console.WriteLine("输入原文");
s = Console.ReadLine();
Console.WriteLine("输入查找词");
string a = Console.ReadLine();
Console.WriteLine("输入替换词");
string b = Console.ReadLine();
string r = s.Replace(a,b);
Console.WriteLine(r);
int times = s.Split(a).Length-1;
Console.WriteLine($"替换了 {times} 处，结果：{r}");