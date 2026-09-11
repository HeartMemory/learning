// ═══════ 测试区 ═══════
// ★ 多态的核心体验：一个循环，三种行为
List<Character> party = new List<Character>();
party.Add(new Hero("剑士", 100, 50));
party.Add(new Monster("史莱姆", 30));
party.Add(new Monster("巨龙", 500));

foreach (Character c in party) {
    c.Attack();   // 同一个调用，谁被喊到谁出自己的招（运行时看真实身份）
}
// 期望输出：
// 剑士 发动了基础攻击！
// 剑士挥剑斩击！
// 史莱姆 扑咬！
// 巨龙 扑咬！

Console.WriteLine();

// ★ base.成员 单独体验：英雄的攻击 = 父类的基础攻击 + 自己的剑技
Hero hero = new Hero("剑士", 100, 50);
hero.Attack();
// 期望：同上两行（和循环里第一次调用完全一致）

Console.WriteLine();

// ★ 边界体验：父类引用指向父类对象 → 执行父类版本
Character plain = new Character("村民", 50);
plain.Attack();   // 期望：村民 发动了基础攻击！（没有子类版本可选，走父类）

// ═══════ 类型区 ═══════
public class Character {
    public string Name { get; }
    public int HP { get; protected set; }

    public Character(string name, int hp) {
        Name = name;
        HP = hp;
    }

    // virtual：给子类发"改写许可证"
    public void Attack() {
        Console.WriteLine($"{Name} 发动了基础攻击！");
        OnHit();                          // ← 可变点：空钩子
        Console.WriteLine("攻击结束。");   // ← 固定流程
    }
    protected virtual void OnHit() { }

    public void ShowStatus() {
        Console.WriteLine($"{Name} HP={HP}");
    }
}

public class Hero : Character {
    public int MP { get; private set; }

    public Hero(string name, int hp, int mp) : base(name, hp) {
        MP = mp;
    }

    // TODO 1：override 改写 Attack——先调用 base.Attack() 打出基础攻击那一行，
    //         再打印"剑士挥剑斩击！"
    //         （体会：不写 base.Attack() 会怎样？父类那行就消失了——保留 + 扩展）
    protected override void OnHit() {
        Console.WriteLine("剑士挥剑斩击！");
    }
}

public class Monster : Character {
    public Monster(string name, int hp) : base(name, hp) { }

    // TODO 2：override 改写 Attack——这次完全自定义，不调父类：
    //         直接打印"[Name] 扑咬！"
    protected override void OnHit() {
        Console.WriteLine($"「{Name}」 扑咬！");
    }
}
