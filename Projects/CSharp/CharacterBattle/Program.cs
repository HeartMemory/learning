// ═══════ 战斗场景（测试区）═══════
List<Character> heroes = new List<Character>();
heroes.Add(new Hero("剑士", 100, 25, "剑技", 5));
heroes.Add(new Hero("法师", 80, 30, "法技", 10));

List<Character> monsters = new List<Character>();
monsters.Add(new Monster("史莱姆", 40, 8, "撕咬", 5));
monsters.Add(new Monster("巨龙", 120, 18, "吐息", 10));

Battle.Run(heroes, monsters);

// ── 接口演示：英雄能攻击"任何能挨打的东西"（宝箱根本不是 Character！）──
Console.WriteLine("\n── 接口演示：攻击宝箱 ──");
Hero soloHero = new Hero("剑士", 100, 25, "剑技", 5);
TreasureChest chest = new TreasureChest(40);
soloHero.Attack(chest);   // 期望：剑士 攻击 宝箱！/ [宝箱] 受到 30 点伤害，剩余 HP=10
soloHero.Attack(chest);   // 期望：剑士 攻击 宝箱！/ [宝箱] 受到 30 点伤害，剩余 HP=-20 / [宝箱] 打开了！
Console.WriteLine($"宝箱状态：{(chest.IsOpened ? "已打开" : "关闭")}");   // 期望：已打开

// ═══════ 类型区 ═══════
// 【接口 = 行为契约】"能挨打的东西"——不关心你是什么，只要求你能承受伤害
//   类比：插座标准（国标三孔）——不管插上来的是什么电器，插头合规就能用
public interface IAttackable {
    string Name { get; }          // 契约条款 1：必须有名字
    bool IsAlive { get; }         // 契约条款 2：必须能报告存活状态
    void TakeDamage(int dmg);     // 契约条款 3：必须能承受伤害
}                                  // ← 注意：全是签名，一行实现都没有

public abstract class Character : IAttackable {    // Character 签下"能挨打"的合同
    private readonly string _skillName;
    private readonly int _skillBonus;
    private static int _totalCount = 0;
    public static int TotalCount => _totalCount;   // 静态统计（Student 类同款）

    public string Name { get; }
    public int HP { get; protected set; }          // 只有受伤流程能改
    public int AttackPower { get; }
    public bool IsAlive => HP > 0;                 // 计算属性

    public Character(string name, int hp, int attackPower, string skillName, int skillBonus) {
        Name = name;
        HP = hp;
        AttackPower = attackPower;
        _skillName = skillName;
        _skillBonus = skillBonus;
        _totalCount++;
    }

    // 攻击骨架：流程锁死（不 virtual）——先宣告、再算伤害、再结算
    // ★ 参数类型从 Character 换成 IAttackable：从此能攻击"任何能挨打的东西"（角色、宝箱……）
    public void Attack(IAttackable target) {
        Console.WriteLine($"{Name} 攻击 {target.Name}！");
        target.TakeDamage(CalcDamage());
    }

    // 伤害钩子：默认返回攻击力，子类可改写（Hero 的剑技加成）
    protected int CalcDamage()
    {
        Console.WriteLine($"[{Name}] 使出{_skillName}！");
        return AttackPower + _skillBonus;
    }

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
    public Hero(string name, int hp, int attackPower, string skillName, int skillBonus) : base(name, hp, attackPower, skillName, skillBonus) {
        MP = 50;
    }

    // TODO 2：override 改写伤害钩子——英雄有剑技加成：
    //         打印"[Name] 使出剑技！"，然后返回 AttackPower + 5
    //         （体会：父类骨架一行都没动，只是换了"伤害怎么算"这一步）
}

public class Monster : Character {
    public Monster(string name, int hp, int attackPower, string skillName, int skillBonus) : base(name, hp, attackPower, skillName, skillBonus){}
    // 不需要改写任何东西——直接继承父类默认行为
    // （体会：不 override = "照着父亲说的做"，这也是多态的一部分）
}

// 【新类型】宝箱：它【不继承 Character】（没有 MP、不会攻击），但签了 IAttackable 合同 → 也能被攻击
public class TreasureChest : IAttackable {
    public string Name => "宝箱";
    public int HP { get; private set; }
    public bool IsAlive => HP > 0;
    public bool IsOpened => !IsAlive;      // 打碎 = 打开

    public TreasureChest(int hp = 40) { HP = hp; }

    // TODO: 实现 TakeDamage——和 Character 的版本像吗？
    //   · 卫语句同款（dmg <= 0 或已打开 → 直接 return）
    //   · 否则扣 HP、打印"[宝箱] 受到 dmg 点伤害，剩余 HP=xx"
    //   · ★ 但"归零"时的措辞不同：宝箱不说"倒下了"，而说"[宝箱] 打开了！"
    //     —— 同一个契约，各自实现细节不同，这正是接口的价值
    public void TakeDamage(int dmg) {
        if(dmg <= 0 || IsOpened) return;
        HP -= dmg;
        if (IsOpened)
        {
            Console.WriteLine("[宝箱] 打开了！");
            return;
        }
        Console.WriteLine($"[宝箱] 受到 {dmg} 点伤害，剩余 HP={HP}");
    }
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
