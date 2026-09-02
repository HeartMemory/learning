int n = 0;
while (true)
{
    Console.WriteLine("=== 字符串统计工具 ===\n1. 查某个字符出现几次\n2. 统计每个字母出现次数\n3. 字符串倒序\n4. 回文判断\n0. 退出");
    Console.WriteLine("请输入数字选择");
    if(int.TryParse(Console.ReadLine(),out n) && n >= 0 && n <= 4)
    {
        if(n == 0)
        {
            break;
        }
        switch(n)
        {
            case 1:
                Console.WriteLine("请输入需要查找的文本内容");
                string found = Console.ReadLine();
                if (string.IsNullOrEmpty(found))
                {
                    Console.WriteLine("输入非法，请重新开始");
                    break;
                }
                Console.WriteLine("请输入需要查找的字符");
                Console.WriteLine("仅需输入单个字符(多个字符取第一个)");
                string tofound = Console.ReadLine();
                if (string.IsNullOrEmpty(tofound))
                {
                    Console.WriteLine("输入非法，请重新开始");
                    break;
                }
                char char1 = tofound[0];
                int times = 0;
                for(int i = 0;i < found.Length; i++)
                {
                    if(found[i] == char1)
                    {
                        times++;
                    }
                }
                Console.WriteLine($"{char1}一共出现了{times}次");
                break;
            case 2:
                Console.WriteLine("请输入需要统计的英文");
                Console.WriteLine("只识别输入中的英文");
                Console.WriteLine("区分大小写");
                string english = Console.ReadLine();
                int[] times1 = new int[26];
                int[] times2 = new int[26];
                for(int i = 0;i < english.Length; i++)
                {
                    if(english[i] - 'a' >= 0 && english[i] - 'z' <= 0)
                    {
                        times1[english[i] - 'a']++;
                    }else if(english[i] - 'A' >= 0 && english[i] - 'Z' <= 0)
                    {
                        times2[english[i] - 'A']++;
                    }
                }
                int judge = 0;
                for(int i = 0;i < 26; i++)
                {
                    if(times1[i] != 0)
                    {
                        Console.WriteLine($"{(char)(i + 'a')}出现了{times1[i]}次");
                        judge++;
                    }
                    if(times2[i] != 0)
                    {
                        Console.WriteLine($"{(char)(i + 'A')}出现了{times2[i]}次");
                        judge++;
                    }
                }
                if(judge == 0)
                {
                    Console.WriteLine("未找到有效英文");
                }
                break;
            case 3:
                Console.WriteLine("请输入要倒序的内容");
                string input = Console.ReadLine();
                char[] txt = input.ToCharArray();
                int left = 0;
                int right = txt.Length-1;
                while(left < right)
                {
                    char temp = txt[left];
                    txt[left] = txt[right];
                    txt[right] = temp;
                    left++;
                    right--;
                }
                Console.WriteLine(string.Join("",txt));
                break;
            case 4:
                Console.WriteLine("请输入需要判断的内容");
                string back = Console.ReadLine();
                if(string.IsNullOrEmpty(back))
                {
                    Console.WriteLine("输入非法，请重新开始");
                    break;
                }
                int left1 = 0;
                int right1 = back.Length-1;
                int judge1 = 0;
                while(left1 < right1)
                {
                    if(back[left1] != back[right1])
                    {
                        Console.WriteLine("不是回文");
                        judge1++;
                        break;
                    }
                    left1++;
                    right1--;
                }
                if(judge1 == 0)
                {
                    Console.WriteLine("是回文");
                }
                break;
            default:break;
        }
    }else
    {
        Console.WriteLine("输入非法，请重新输入");
    }
}
