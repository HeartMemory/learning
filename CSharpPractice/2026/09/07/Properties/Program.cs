Student student1 = new Student("张雪峰",30);
Student student2 = new Student("牢大",114514);
Console.WriteLine(student1.Introduce);
Console.WriteLine(student2.Introduce);

public class Student
{
    private int _age;
    public int Age
    {
        get{return _age;}
        set
        {
            if(value >= 0 && value <= 120)
            {
                _age = value;
            }else{Console.WriteLine("年龄不合法");}
        }
    }
    private string _name;
    public string Name
    {
        get{return _name;}
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _name = value;
            }
        }
    }

    public Student(string Name = "张三", int Age = 18)
    {
        this.Name = Name;
        this.Age = Age;
    }
    public string Introduce => $"大家好，我叫{Name}，今年{Age}岁";
}
