// ═══════ 战斗场景（测试区）═══════
List<Character> heroes = new List<Character>();
heroes.Add(new Hero("剑士", 100, 25));
heroes.Add(new Hero("法师", 80, 30));

List<Character> monsters = new List<Character>();
monsters.Add(new Monster("史莱姆", 40, 8));
monsters.Add(new Monster("巨龙", 120, 18));

Battle.Run(heroes, monsters);

// ═══════ 类型区 ═══════
public class Character {
    private static int _totalCount = 0;
    public static int TotalCount => _totalCount;   // 静态统计（Student 类同款）

    public string Name { get; }
    public int HP { get; protected set; }          // 只有受伤流程能改
    public int AttackPower { get; }
    public bool IsAlive => HP > 0;                 // 计算属性

    public Character(string name, int hp, int attackPower) {
        Name = name;
        HP = hp;
        AttackPower = attackPower;
        _totalCount++;
    }

    // 攻击骨架：流程锁死（不 virtual）——先宣告、再算伤害、再结算
    public void Attack(Character target) {
        Console.WriteLine($"{Name} 攻击 {target.Name}！");
        target.TakeDamage(CalcDamage());
    }

    // 伤害钩子：默认返回攻击力，子类可改写（Hero 的剑技加成）
    protected virtual int CalcDamage() => AttackPower;

    // TODO 1：受伤结算——卫语句（dmg <= 0 或 已阵亡 → 直接 return）
    //         否则：HP 扣 dmg，打印"[Name] 受到 dmg 点伤害，剩余 HP=xx"
    //         若 HP <= 0：打印"[Name] 倒下了！"
    public void TakeDamage(int dmg) {
        if(dmg <= 0 || HP <= 0) return;
        HP -= dmg;
        Console.WriteLine($"[{Name}] 受到 {dmg} 点伤害，剩余 HP={HP}");
        if(HP <= 0) Console.WriteLine($"[{Name}] 倒下了！");
    }
}

public class Hero : Character {
    public int MP { get; private set; }

    public Hero(string name, int hp, int attackPower) : base(name, hp, attackPower) {
        MP = 50;
    }

    // TODO 2：override 改写伤害钩子——英雄有剑技加成：
    //         打印"[Name] 使出剑技！"，然后返回 AttackPower + 5
    //         （体会：父类骨架一行都没动，只是换了"伤害怎么算"这一步）
    protected override int CalcDamage()
    {
        int dmg = base.CalcDamage();
        if(Name == "剑士")
        {
            Console.WriteLine($"[{Name}] 使出剑技！");
            dmg += 5;
        }
        if(Name == "法师")
        {
            Console.WriteLine($"[{Name}] 使出法技！");
            dmg += 10;
        }
        return dmg;
    }
}

public class Monster : Character {
    public Monster(string name, int hp, int attackPower) : base(name, hp, attackPower) { }

    // 不需要改写任何东西——直接继承父类默认行为
    // （体会：不 override = "照着父亲说的做"，这也是多态的一部分）
}

public class Battle {
    // ⭐ TODO 3（多态的核心演出）：战斗主循环
    //
    //   流程：
    //   ① 打印开场："⚔️ 战斗开始！英雄队 vs 怪物队"
    //   ② while (HasAlive(heroes) && HasAlive(monsters))：
    //        - 英雄队逐个出手：活着的才攻击 → 目标 = 怪物队第一个活着的
    //        - 怪物队逐个出手：活着的才攻击 → 目标 = 英雄队第一个活着的
    //   ③ 循环结束：哪队还有活人，哪队赢；打印"🏆 英雄队获胜！"或"💀 怪物队获胜！"
    //
    //   建议先写两个辅助方法（都在本类里）：
    //     private static bool HasAlive(List<Character> team)          → 有人活着吗
    //     private static Character FindFirstAlive(List<Character> team) → 第一个活着的（找不到返回 null）
    //
    //   提示：foreach (Character c in heroes) 里调 c.Attack(target)——c 的声明类型是 Character，
    //         但剑士会自己使出剑技、史莱姆会老老实实平A——多态就在这一行上演
    public static void Run(List<Character> heroes, List<Character> monsters) {
        while(HasAlive(heroes) && HasAlive(monsters))
        {
            foreach(Character c in heroes)
            {
                if(c.HP <= 0) continue;
                Character enemy = FindFirstAlive(monsters);
                if(enemy == null) break;
                c.Attack(enemy);
            }
            foreach(Character c in monsters)
            {
                if(c.HP <= 0) continue;
                Character enemy = FindFirstAlive(heroes);
                if(enemy == null) break;
                c.Attack(enemy);
            }
        }
        if(HasAlive(heroes)) Console.WriteLine("英雄队获胜");
        else Console.WriteLine("怪物队获胜");
    }
    private static bool HasAlive(List<Character> team) => FindFirstAlive(team) != null;
    private static Character? FindFirstAlive(List<Character> team)
    {
        for(int i = 0;i < team.Count; i++)
        {
            if(team[i].HP > 0) return team[i];
        }
        return null;
    }
}
