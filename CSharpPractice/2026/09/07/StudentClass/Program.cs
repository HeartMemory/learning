Student student1 = new Student("张雪峰",30);
Student student2 = new Student("牢大",24);
student1.Introduce();
student2.Introduce();

public class Student
{
    public string Name;
    public int Age;
    public Student(string Name = "张三", int Age = 18)
    {
        this.Name = Name;
        this.Age = Age;
    }
    public void Introduce()
    {
        Console.WriteLine($"大家好，我叫{Name}，今年{Age}岁");
    }
}
