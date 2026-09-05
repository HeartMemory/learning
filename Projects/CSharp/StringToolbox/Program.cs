StringToolBox tool = new StringToolBox();

while (true)
{
    tool.ShowMenu();
    int n = tool.ReadMenu(Console.ReadLine());
    switch (n)
    {
        case 0:break;
        case 1:
            {
                Console.WriteLine("请输入文本");
                string a = tool.NullJudge(Console.ReadLine());
                if(a == null)
                {
                    n = 0;
                    break;
                }
                Console.WriteLine("请输入查找词，多个输入以第一个为准");
                string b = tool.NullJudge(Console.ReadLine());
                if(b == null)
                {
                    n = 0;
                    break;
                }
                int times = tool.CountText(a,b[0]);
                Console.WriteLine($"出现了{times}次");
                break;
            }
        case 2:
            {
                Console.WriteLine("请输入文本");
                string a = tool.NullJudge(Console.ReadLine());
                if(a == null)
                {
                    n = 0;
                    break;
                }
                Console.WriteLine("请输入查找词");
                string b = tool.NullJudge(Console.ReadLine());
                if(b == null)
                {
                    n = 0;
                    break;
                }
                int times = tool.CountText(a,b);
                Console.WriteLine($"出现了{times}次");
                break;
            }
        case 3:
            {
                Console.WriteLine("请输入文本");
                string a = tool.NullJudge(Console.ReadLine());
                if(a == null)
                {
                    n = 0;
                    break;
                }
                int times = tool.words(a);
                Console.WriteLine($"共{times}个单词");
                break;
            }
        case 4:
            {
                Console.WriteLine("输入原文");
                    string s = tool.NullJudge(Console.ReadLine());
                    if(s == null)
                    {
                        n = 0;
                        break;
                    }
                    Console.WriteLine("输入查找词");
                    string a = tool.NullJudge(Console.ReadLine());
                    if(a == null)
                    {
                        n = 0;
                        break;
                    }
                    Console.WriteLine("输入替换词");
                    string b = tool.NullJudge(Console.ReadLine());
                    if(b == null)
                    {
                        n = 0;
                        break;
                    }
                    tool.Change(s,a,b);
                    break;
            }
        case 5:
            {
                Console.WriteLine("请输入文本");
                string a = tool.NullJudge(Console.ReadLine());
                if(a == null)
                {
                    n = 0;
                    break;
                }
                if (tool.IsPalindrome(a))
                {
                    Console.WriteLine("是回文");
                }
                else
                {
                    Console.WriteLine("不是回文");
                }
                break;
            }
    }
    if(n == 0)
    {
        break;
    }
}

class StringToolBox
{
    public bool IsPalindrome(string s){
        List<char> chars = new List<char>();
        for(int i = 0;i < s.Length;i++)
        {
            if (char.IsLetterOrDigit(s[i]))
            {
                chars.Add(char.ToLower(s[i]));
            }else if (char.IsNumber(s[i]))
            {
                chars.Add(s[i]);
            }
        }
        int left = 0;
        int right = chars.Count-1;
        while(left < right)
        {
            if(chars[left] != chars[right])
            {
                return false;
            }
            left++;
            right--;
        }
        return true;
    }

    public void Change(string s,string a,string b)
    {
        string r = s.Replace(a,b);
        Console.WriteLine(r);
        int times = s.Split(a).Length-1;
        Console.WriteLine($"替换了 {times} 处，结果：{r}");
    }
    
    public int words(string s)
    {
        int times = 0;
        string[] words = s.Split(' ');
        for(int i = 0;i < words.Length; i++)
        {
            if(words[i] != "")
            {
                Console.WriteLine(words[i]);
                times++;
            }
        }
        return times;
    }
    
    public int CountText(string text, char target)
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

    public int CountText(string text, string target)
    {
        if(string.IsNullOrEmpty(target))
        {
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
    
    public void ShowMenu()
    {
        Console.WriteLine("=====菜单=====");
        Console.WriteLine("1.单字符出现次数");
        Console.WriteLine("2.字符串出现次数");
        Console.WriteLine("3.单词拆分并统计个数");
        Console.WriteLine("4.查找替换");
        Console.WriteLine("5.回文判断");
        Console.WriteLine("0.退出");
    }

    public int ReadMenu(string s)
    {
        int num = 0;
        for(int i = 1;i <= 5; i++)
        {
            if(string.IsNullOrEmpty(s) && i < 5)
            {
                Console.WriteLine($"输入非法（第{i}次），请重新输入，剩余{5 - i}次机会");
                s = Console.ReadLine();
            }else if(int.TryParse(s,out num) && num >= 0 && num <= 5)
            {
                return num;
            }
            else if(i < 5)
            {
                Console.WriteLine($"输入非法（第{i}次），请重新输入，剩余{5 - i}次机会");
                s = Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"输入非法，已使用完次数，自动退出");
            }
        }
        return 0;
    }

    public string NullJudge(string s)
    {
        for(int i = 1;i <= 5; i++)
        {
            if(!string.IsNullOrEmpty(s))
            {
                return s;
            }
            else if(i < 5)
            {
                Console.WriteLine($"输入非法（第{i}次），请重新输入，剩余{5 - i}次机会");
                s = Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"输入非法，已使用完次数，自动退出");
            }
        }
        return null;
    }
}