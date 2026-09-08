// ═══════ 主流程测试 ═══════
Student s = new Student("张三", 20);

s.Enroll("数学");   // 正常选课
s.Enroll("英语");   // 正常选课
s.Enroll("数学");   // 重复 → 门卫拦截
s.Enroll("");       // 空串 → 门卫拦截

s.PrintCourses();   // 期望打印：数学、英语 两行
Console.WriteLine($"课程数：{s.Courses.Count}");      // 期望 2（重复和空串都没混进来）
Console.WriteLine(s.Info);                            // 期望 张三（20岁）
Console.WriteLine($"学生总数：{Student.TotalCount}"); // 期望 1

// 解开注释体验只读眼镜的编译错误：
// s.Courses.Add("混进去的课");   // ❌ CS1061：IReadOnlyList 没有 Add

public class Student
{
    private static int _totalCount = 0;
    private const int MaxAge = 120;
    private const int MinAge = 0;
    private int _age;
    private string _name;
    private List<string> _courses = new List<string>();
    public int Age
    {
        get{return _age;}
        set
        {
            if(!IsValidAge(value)) return;
            _age = value;
        }
    }
    public IReadOnlyList<string> Courses => _courses;
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
    public void Enroll(string course)
    {
        if(string.IsNullOrEmpty(course)) return;
        if(_courses.Contains(course)) return;
        _courses.Add(course);
    }
    public void PrintCourses()
    {
        for(int i = 0;i < Courses.Count;i++)
        {
            Console.WriteLine(Courses[i]);
        }
    }
}
