string s = Console.ReadLine();
string result = new string("");
for(int i = s.Length-1;i >= 0; i--)
{
    result += s[i];
}
Console.WriteLine(result);

char[] a = s.ToCharArray();
int left = 0;
int right = s.Length - 1;
while(left < right)
{
    char temp = a[left];
    a[left] = a[right];
    a[right] = temp;
    left++;
    right--;
}
Console.WriteLine(string.Join("",a));

string x = s.ToLower();
int[] letter = new int[26];
for(int i = 0;i < x.Length; i++)
{
    if (char.IsLetter(x[i]))
    {
        letter[x[i]-'a'] += 1; 
    }
}
Console.WriteLine(string.Join(".",letter));

left = 0;
right = s.Length - 1;
int sum = 0;
int judge = s.Length / 2;
while(sum <= judge)
{
    if(s[left] != s[right])
    {
        Console.WriteLine("不是回文");
        judge = -1;
        break;
    }
    left++;
    right--;
    sum++;
}
if(judge != -1){Console.WriteLine("是回文");}
