// ═══════ 测试区 ═══════
Hero hero = new Hero("剑士", 100, 50);
hero.ShowStatus();          // 期望：剑士 HP=100          ← 继承白拿的方法
hero.TakeDamage(30);        // 期望：剑士受到 30 点伤害！HP=70
hero.TakeDamage(-5);        // 期望：无效伤害              ← 父类卫语句拦截
hero.CastSkill("火焰斩");   // 期望：剑士释放[火焰斩]！（MP 50→40）
hero.CastSkill("烈焰风暴"); // 期望：剑士释放[烈焰风暴]！（MP 40→25）
hero.CastSkill("陨石术");   // 期望：剑士释放[陨石术]！（MP 25→5）
hero.CastSkill("流星雨");   // 期望：MP 不足，无法释放[流星雨]！
hero.ShowStatus();          // 期望：剑士 HP=70

// ───── this 专场 ─────
Hero mage = new Hero("法师");                    // 便捷构造（TODO 6：this 链）
Console.WriteLine($"{mage.Name} MP={mage.MP}"); // 期望：法师 MP=30

Party party = new Party();
hero.Join(party);       // 期望：剑士加入了队伍！（1/3）
mage.Join(party);       // 期望：法师加入了队伍！（2/3）
party.ShowMembers();    // 期望：队伍成员：剑士、法师

// ═══════ 类型区 ═══════
public class Character {
    public string Name { get; }
    public int HP { get; private set; }

    // TODO 1: 构造函数（name, hp）——老套路：参数赋给属性
    public Character(string name, int hp) {
        Name = name;
        HP = hp;
    }

    // TODO 2: 受伤逻辑——卫语句拦截 dmg <= 0（无效伤害直接 return），
    //          否则 HP 扣减并打印："[Name]受到 dmg 点伤害！HP=新值"
    public void TakeDamage(int dmg) {
        if(dmg <= 0) return;
        HP -= dmg;
        Console.WriteLine($"「{Name}」受到{dmg}伤害！HP={HP}");
    }

    public void ShowStatus() {
        Console.WriteLine($"{Name} HP={HP}");
    }
}

public class Hero : Character {
    public int MP { get; private set; }

    // TODO 3（今天核心）: 构造函数——参数列表后跟 : base(name, hp) 先造好 Character 部分，
    //          方法体里只负责自己的 MP。对比构造函数链 this(...)，一字之差。
    public Hero(string name, int hp, int mp) : base(name, hp) {
        MP = mp;
    }

    // TODO 4: 释放技能——卫语句 MP < 10 时打印"MP 不足，无法释放[skillName]！"并 return；
    //          否则 MP 扣 10，打印"[Name]释放[skillName]！（MP 旧值→新值）"
    public void CastSkill(string skillName) {
        if(skillName == "火焰斩" && MP >= 10)
        {
            Console.WriteLine($"[{Name}]释放[{skillName}]！（MP {MP}→{MP-10}）");
            MP -= 10;
        }else if(skillName == "烈焰风暴" && MP >= 15)
        {
            Console.WriteLine($"[{Name}]释放[{skillName}]！（MP {MP}→{MP-15}）");
            MP -= 15;
        }else if(skillName == "陨石术" && MP >= 20)
        {
            Console.WriteLine($"[{Name}]释放[{skillName}]！（MP {MP}→{MP-20}）");
            MP -= 20;
        }else if(skillName == "流星雨" && MP >= 25)
        {
            Console.WriteLine($"[{Name}]释放[{skillName}]！（MP {MP}→{MP-25}）");
            MP -= 25;
        }else{Console.WriteLine($"MP 不足，无法释放[{skillName}]！");}
    }

    // TODO 5（this 用法③·最重要）: 入队——Party.Add() 需要一个 Character，
    //          但"应该加进去的"是正在调用 Join 的这个 Hero 自己。
    //          对象在自己的方法里怎么称呼"我"？→ 关键词：this
    public void Join(Party party) {
        party.Add(this);
    }

    // TODO 6（this 用法②）: 便捷构造——单参数版默认 HP=100 MP=30。
    //          用 this(...) 构造链委托给三参版，方法体留空 { } 即可。
    //          体会：this(...) 找"本类的兄弟构造"，base(...) 找"爹的构造"——同一个槽位，两种方向。
    public Hero(string name) : this(name, 100, 30){}
}

// 队伍类：this 的"收货方"（已写好，看懂即可——它就是 Add 的那个容器）
public class Party {
    private List<Character> _members = new List<Character>();
    private const int MaxCount = 3;

    public void Add(Character c) {
        if (_members.Count >= MaxCount) {
            Console.WriteLine("队伍已满，无法加入！");
            return;
        }
        _members.Add(c);
        Console.WriteLine($"{c.Name} 加入了队伍！（{_members.Count}/{MaxCount}）");
    }

    public void ShowMembers() {
        var names = new List<string>();
        foreach (Character c in _members) names.Add(c.Name);
        Console.WriteLine($"队伍成员：{string.Join("、", names)}");
    }
}
