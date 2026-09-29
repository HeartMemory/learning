// ═══════ 今日主题：事件 event —— 给"订阅"上锁 + 观察者模式 ═══════
//
// 【一句话】`event` 就是"带门禁的委托字段"：**订阅公开，发布上锁**。
//
// 【为什么需要它 · 接昨天的坑】
//   昨天 `ScoreBoard.OnScoreChanged` 是个 `public Action<int>?` 字段，于是任何人一句
//   `board.OnScoreChanged = null;` 就能把**别人挂的通知全清掉** —— 那是"把广播站的话筒交给所有人"。
//   今天加一个 `event` 关键字，语义立刻变成：
//     · 外部只能 `+=` / `-=`（订阅 / 退订）
//     · 外部**不能赋值**、**不能调用**（只有声明它的那个类内部才能 raise）
//   ⭐ 实测判据（09-29 用临时项目跑出来的，你可以自己复现）：外部对 event 赋值或调用，
//      编译器报 **CS0070**：「事件"X"只能出现在 += 或 -= 的左边**（从类型"X"中使用时除外）**」——
//      "从类型 X 中使用时除外"这半句就是"内部才能 raise"。
//
// 【今天要打的三块地基】
//   ① `event` 的封装语义（订阅公开 / 发布上锁）
//   ② **观察者模式**：发布者只管"喊"，不认识任何订阅者；订阅者主动来**登记**（`+=`）
//      —— 对照昨天的"依赖注入"：昨天是"**我**把回调塞给你"，今天是"**你**来我这里登记"
//   ③ **退订**：订阅了就要能退（`-=`），不然对象会被"订阅链"一直拽着
//
// 【怎么用这个文件】测试区写了期望输出 → 去类型区把 TODO 补完 → `dotnet run` 对着期望核

// ═══════ 测试区 ═══════
Console.WriteLine("—— ① event 的封装语义（外部：只能 += / -=）——");
ScoreBoard board = new ScoreBoard(10);
board.OnScoreChanged += Calculator.ReportScore;   // 订阅：合法
board.OnScoreChanged += Calculator.ReportLoud;    // 再订阅一个（多播）
board.AddBricks(3);
// 期望两行：[通知] 当前分数：30
//           ★★★ 分数变成 30 了 ★★★
board.AddBricks(2);
// 期望两行：……：50 ／ ★★★ 分数变成 50 了 ★★★
board.OnScoreChanged -= Calculator.ReportLoud;    // 退订：合法
Console.WriteLine("—— 退订 ReportLoud 之后 ——");
board.AddBricks(1);
// 期望只有一行：[通知] 当前分数：60

// ⚠️ 下面两行【故意注释掉】—— 你要亲手解开它们，读一遍报错（实测都是 CS0070）：
// board.OnScoreChanged = Calculator.ReportScore;   // ① 外部【赋值】
// board.OnScoreChanged?.Invoke(99);                // ② 外部【调用】
// 读懂那句错话："事件只能出现在 += 或 -= 的左边（从类型 ScoreBoard 中使用时除外）"
// → 这半句"除外"就是：**只有 ScoreBoard 自己**能在内部 raise。

Console.WriteLine("—— ② 观察者模式：发布者不认识订阅者 ——");
NewsChannel channel = new NewsChannel("学习频道");
Subscriber alice = new Subscriber("alice");
Subscriber bob = new Subscriber("bob");
Subscriber carol = new Subscriber("carol");
alice.SubscribeTo(channel);      // 三个订阅者主动来"登记"
bob.SubscribeTo(channel);
carol.SubscribeTo(channel);
channel.Publish("Block 3 第 2 天开工");
// 期望三行：alice 收到：Block 3 第 2 天开工 ／ bob …… ／ carol ……
bob.UnsubscribeFrom(channel);    // 退订一个
channel.Publish("只剩两人能收到");
// 期望两行：alice ／ carol

Console.WriteLine("—— ③ 退订实验：不退订会怎样 ——");
NewsChannel ch2 = new NewsChannel("临时频道");
CreateTempSubscriber(ch2);       // 造一个"临时订阅者"，函数返回后主流程就再也拿不到它的引用了
ch2.Publish("我还收得到吗？");
// 期望：*** 临时订阅者 收到：我还收得到吗？ ***
// ⭐ 想清楚：主流程已经"丢掉"了这个对象，它为什么还能收到通知？
//    （提示：发布者的订阅链里**还牵着它** → 这就是"忘记退订 = 对象被悄悄留住"的最小模型）

// ③ 段用的辅助函数：造一个临时订阅者，**故意不退订**
//   ⚠️ 注意它写在「类型区」之前 —— 顶级语句文件里，**局部函数必须在类型声明之前**（否则 CS8803）
static void CreateTempSubscriber(NewsChannel channel) {
    Subscriber temp = new Subscriber("*** 临时订阅者");
    temp.SubscribeTo(channel);
    // ⚠️ 这里【故意不写】temp.UnsubscribeFrom(channel);
    //    想知道"写上行会怎样"？自己加一行跑一次对照（期望：那行通知就收不到了）
}

// ═══════ 类型区 ═══════

// TODO 1: 把下面这行从【委托字段】改成【事件】—— 只加一个关键字，语义就完全变了
//         （对照昨天：那时候它是个 public 字段 → 外部能赋值、能调用）
//         改完之后：外部只剩 += / -= 两个动作；raise 只能在类内部
public class ScoreBoard {
    public event Action<int>? OnScoreChanged;      // ← TODO 1：给它加上 event 关键字

    private readonly int _scorePerBrick;
    private int _score;
    public ScoreBoard(int scorePerBrick) {
        _scorePerBrick = scorePerBrick;
        _score = 0;
    }
    public int Score => _score;

    public void AddBricks(int count) {
        _score += count * _scorePerBrick;
        // 注意：这一行**不用改**——因为它就在 ScoreBoard 内部，内部 raise 是允许的
        OnScoreChanged?.Invoke(_score);
    }
}

public static class Calculator {
    // 已给：安静版播报
    public static void ReportScore(int score) {
        Console.WriteLine($"[通知] 当前分数：{score}");
    }

    // TODO 2: 吵闹版播报 —— 打印格式：`★★★ 分数变成 {score} 了 ★★★`
    public static void ReportLoud(int score) {
        Console.WriteLine($"★★★ 分数变成 {score} 了 ★★★");
    }
}

// TODO 3: NewsChannel（发布者）—— 它只管"喊"，不认识任何订阅者
//   · 声明一个事件：`public event Action<string>? Breaking;`（表示"有突发新闻"，内容是个 string）
//   · `Publish(string headline)`：把 headline 广播出去（**内部** raise；别忘了 `?.`）
public class NewsChannel {
    public string Name { get; }
    public NewsChannel(string name) { Name = name; }
    // TODO 3a: 声明事件
    public event Action<string>? Breaking;
    // TODO 3b: 广播
    public void Publish(string headline) {
        Breaking?.Invoke(headline);
    }
}

// TODO 4: Subscriber（订阅者）—— 它认识频道，频道不认识它（这就是解耦方向）
//   · 构造时只需要一个名字（先别订阅，订阅由 `SubscribeTo` 负责 —— 为什么？想想"对象还没造完就订阅"的风险）
//   · `SubscribeTo(channel)`：`channel.Breaking += OnBreakingNews;`
//   · `UnsubscribeFrom(channel)`：`-=`（**同一个方法实例**：`+=` 和 `-=` 必须指向同一个委托，否则退不掉）
//   · `OnBreakingNews(string headline)`：打印 `{Name} 收到：{headline}`
public class Subscriber {
    public string Name { get; }
    public Subscriber(string name) { Name = name; }

    public void SubscribeTo(NewsChannel channel) {
        channel.Breaking += OnBreakingNews;
    }

    public void UnsubscribeFrom(NewsChannel channel) {
        channel.Breaking -= OnBreakingNews;
    }

    private void OnBreakingNews(string headline) {
        Console.WriteLine($"{Name} 收到：{headline}");
    }
}

