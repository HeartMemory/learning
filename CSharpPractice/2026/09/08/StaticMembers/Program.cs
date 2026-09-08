Student s1 = new Student("张三", 20);
Console.WriteLine(Student.TotalCount);   // 期望 1

Student s2 = new Student("李四", 21);
Student s3 = new Student("王五", 114514);   // 年龄非法 → 门卫拦截 → 不计数
Console.WriteLine(Student.TotalCount);   // 期望 2（非法学生不进户口本）

s1.Age = -500;                           // 卫语句拦截
Console.WriteLine(s1.Age);               // 期望 20（-500 被丢弃）
Console.WriteLine(s1.Info);              // 期望 张三（20岁）

public class Student
{
    private static int _totalCount = 0;
    private const int MaxAge = 120;
    private const int MinAge = 0;
    private int _age;
    private string _name;
    public int Age
    {
        get{return _age;}
        set
        {
            if(!IsValidAge(value)) return;
            _age = value;
        }
    }
    public string Name
    {
        get{return _name;}
        set
        {
            if(string.IsNullOrEmpty(value)) return;
            _name = value;
        }
    }
    public static int TotalCount => _totalCount;
    public string Info => $"{Name}（{Age}岁）";
    public static bool IsValidAge(int age) => age >= MinAge && age <= MaxAge;
    public Student(string name = "张三", int age = 18)
    {
        Name = name;
        Age = age;
        if (IsValidAge(age)) _totalCount++;
    }
}
