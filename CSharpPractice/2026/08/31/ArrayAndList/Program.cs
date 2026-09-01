int[] nums = new int[]{0,1,-3,-9,8,7,5};
double max = nums[0];
double min = nums[0];
double mid = nums[0];
double all = nums[0];
for(int i = 1;i < nums.Length; i++)
{
    max = Math.Max(max,nums[i]);
    min = Math.Min(min,nums[i]);
    all += nums[i];
}
mid = all / nums.Length;
Console.WriteLine($"最大值是{max} 最小值是{min} 平均数是{mid}");

double grade = 0;
max = 0;
min = 100;
mid = 0;
all = 0;
List<double> list = new List<double>();
Console.WriteLine("输入非法输入自动退出");
Console.WriteLine("请输入成绩");
while(true)
{
    if(double.TryParse(Console.ReadLine(),out grade) && grade >= 0 && grade <= 100){
        Console.WriteLine($"您输入了{grade}");
    }else{break;}
    list.Add(grade);
    max = Math.Max(max,grade);
    min = Math.Min(min,grade);
    all += grade;
}
if(list.Count > 0){
    mid = all / list.Count;
    Console.WriteLine($"最大值是{max} 最小值是{min} 平均数是{mid}");
}else{Console.WriteLine("未输入任何数字");}
