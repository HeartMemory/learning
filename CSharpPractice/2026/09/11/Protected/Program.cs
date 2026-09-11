// ═══════ 测试区 ═══════
Character villager = new Character("村民", 50);
villager.ShowStatus();                     // 期望：村民 HP=50

Hero hero = new Hero("剑士", 100, 50);
Console.WriteLine($"剑士 MP={hero.MP}");   // 期望：剑士 MP=50
hero.ShowStatus();                         // 期望：剑士 HP=100

hero.Sacrifice();   // 期望：剑士献祭生命力！HP=80 MP=100（MP 从 50 回满）
hero.Sacrifice();   // 期望：剑士献祭生命力！HP=60 MP=100
hero.Sacrifice();   // 期望：剑士献祭生命力！HP=40 MP=100
hero.Sacrifice();   // 期望：剑士献祭生命力！HP=20 MP=100
hero.Sacrifice();   // 期望：血量不足，无法献祭！（HP <= 20 时卫语句拦截）
hero.ShowStatus();  // 期望：剑士 HP=20

// TODO 2: 解开下面这行注释 → 编译会报错 → 读懂报错 → 再注释回去
//         体会：protected set 对"外界"（Main 也是外界！）是关着门的
// villager.HP = 999;

// ═══════ 类型区 ═══════
public class Character {
    public string Name { get; }
    public int HP { get; protected set; }   // ← 今天的明星：protected set（自己和子孙可写，外界不可）

    public Character(string name, int hp) {
        Name = name;
        HP = hp;
    }

    public void TakeDamage(int dmg) {
        if (dmg <= 0) return;
        HP -= dmg;
        Console.WriteLine($"「{Name}」受到 {dmg} 点伤害！HP={HP}");
    }

    public void ShowStatus() {
        Console.WriteLine($"{Name} HP={HP}");
    }
}

public class Hero : Character {
    public int MP { get; private set; }
    private const int HpCost = 20;

    public Hero(string name, int hp, int mp) : base(name, hp) {
        MP = mp;
    }

    // TODO 1: 献祭——卫语句 HP <= HpCost 时打印"血量不足，无法献祭！"并 return；
    //         否则：HP 扣 20、MP 回满 100，打印"剑士献祭生命力！HP=新值 MP=100"
    //
    //         ⭐ 关键观察：方法体里的 `HP -= HpCost` 竟然能编译通过！
    //            为什么子类 Hero 能改"父亲的" HP？回看 Character 里 HP 的 set 前面写了什么词
    //            （对比昨天：HP 是 private set 时，你在 Hero 里写这句会怎样？）
    public void Sacrifice() {
        if(HP <= HpCost)
        {
            Console.WriteLine("血量不足，无法献祭！");
            return;
        }
        HP -= 20;
        MP = 100;
        Console.WriteLine($"{Name}献祭生命力！HP={HP} MP={MP}");
    }
}
