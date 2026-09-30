// ═══════ 今日主题：Lambda + 匿名方法 + 闭包 ═══════
//
// 【一句话】lambda 是"**没有名字的方法**"，而且它能**顺走外面的变量** —— 后者就是闭包。
//
// 【为什么要它】
//   前两天你已经会"把方法当值传"（方法组 → 委托）。但很多回调**只用一次、也不值得起个名字**：
//     `list.Sort((a, b) => a.CompareTo(b))`  ← 为这么一小段逻辑单独写个方法，纯属啰嗦
//   lambda 就是为这种场景生的：**就地写、就地传**。
//
// 【今天三块地基】
//   ① **写法**：表达式 lambda `x => x * 2` ／ 语句 lambda `x => { … }` ／ 无参 `() => …`
//   ② **闭包**：lambda 能读写它外层的局部变量 —— 那些变量会被编译器**搬进一个隐藏类**
//      ⭐ 这解释了昨天那条谜题的底层原因：**"现写的 lambda 退不掉"**（每次都是新对象）
//   ③ **两个经典坑**：`for` 循环捕获（全看到最后一个值）／ 捕获的是**变量**不是**值**
//
// 【怎么用这个文件】测试区写了期望输出 → 去类型区把 TODO 补完 → `dotnet run` 对着期望核

using System.Reflection;

// ═══════ 测试区 ═══════
Console.WriteLine("—— ① lambda 的三种写法（表达式 / 语句 / 无参）——");
Func<int, int> doubleIt = x => x * 2;                     // 表达式 lambda：只有一句、省略 return
Func<int, int, int> addAll = (a, b) =>
{
    int sum = a + b;                                      // 语句 lambda：多行要花括号 + return（TODO 1）
    return sum;
};
Action hello = () => Console.WriteLine("      无参 lambda 也能当 Action");   // 无参 lambda（TODO 1）
Console.WriteLine($"      doubleIt(21) = {doubleIt(21)}");     // 期望 42
Console.WriteLine($"      addAll(3, 4) = {addAll(3, 4)}");     // 期望 7
hello();

Console.WriteLine("—— ② 匿名方法（lambda 的前身，了解级）——");
Func<int, int> tripleOld = delegate (int x) { return x * 3; };   // C# 2.0 的写法（TODO 2）
Console.WriteLine($"      tripleOld(7) = {tripleOld(7)}");      // 期望 21

Console.WriteLine("—— ③ 闭包：lambda 会“顺走”外面的局部变量 ——");
int counter = 0;
Action bump = () => counter++;
bump();
bump();
Console.WriteLine($"      counter = {counter}");               // 期望 2（lambda 改的是外面那个变量）

Console.WriteLine("      ⭐ 那些被捕获的变量，其实被搬进了编译器生成的隐藏类：");
Console.WriteLine($"      lambda 的方法声明类型 = {bump.Method.DeclaringType}");   // 期望形如 Program+<>c__DisplayClass…（前缀随文件结构变化）
Console.WriteLine("      这个隐藏类里的字段：");
foreach (var f in bump.Target!.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
    Console.WriteLine($"        {f.FieldType.Name} {f.Name} = {f.GetValue(bump.Target)}");
// 期望能看到两个 Int32 字段：counter / shared（同一作用域里被捕获的变量住进同一个显示类）
// TODO 3 说明：`bump.Target` 就是这个隐藏类的实例——**委托的 Target 不再是你写的对象，而是它**

Console.WriteLine("—— ④ 坑 A：for 循环里捕获（经典面试题）——");
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
{
    int copy = i;                    // ★ 每轮循环体执行时新建一个变量（新箱子）
    actions.Add(() => Console.WriteLine($"        for 里的 lambda 看到 i = {copy}"));
}
foreach (var a in actions) a();
// ✅ 已修（TODO 4 完成）：三行分别 0 / 1 / 2 —— 因为循环体里 `int copy = i;` 让【每轮新建一个变量】
//   ⭐ 对照：若 lambda 直接读 for 的 i，则三行都是 3。原因是两条【缺一不可】：
//     ① for 的 i 声明在循环头 → 全程只有【一个】变量（一个箱子），三个 lambda 共享它
//     ② lambda 是【调用时才读】→ 攒起来循环结束后才调用，那时 i 已经是 3
//   （实测：循环体里【立刻调用】同一个 i 会打印 0 1 2 —— 变量共享但读得早）

Console.WriteLine("—— ⑤ 对照：foreach 循环里捕获（C# 5 起，每轮一个新变量）——");
var actions2 = new List<Action>();
foreach (var name in new[] { "A", "B", "C" }) actions2.Add(() => Console.WriteLine($"        foreach 里的 lambda 看到 name = {name}"));
foreach (var a in actions2) a();
// 期望 A / B / C —— 同一个语言里两种循环行为不同，这就是"**要记的不是行为，是原因**"的典型

Console.WriteLine("—— ⑥ 坑 B：捕获的是【变量】不是【值】——");
int shared = 0;
Action inc = () => shared++;
Action show = () => Console.WriteLine($"        shared = {shared}");
inc();
inc();
show();
// 期望 2：两个 lambda 看到的是【同一个】shared（同一个显示类里的同一个字段）

Console.WriteLine("—— ⑦ 自己写的两个 lambda（TODO 1）——");
Console.WriteLine($"      MakeIsEven(4) = {LambdaBasics.MakeIsEven()(4)}");    // 期望 True
Console.WriteLine($"      MakeIsEven(5) = {LambdaBasics.MakeIsEven()(5)}");    // 期望 False
Console.WriteLine($"      MakeMax(3, 9) = {LambdaBasics.MakeMax()(3, 9)}");   // 期望 9

Console.WriteLine("—— ⑧ 计数器工厂（TODO 3）——");
var c1 = CounterFactory.MakeCounter();
var c2 = CounterFactory.MakeCounter();
c1(); c1(); c2();
Console.WriteLine($"      c1() = {c1()}   c2() = {c2()}");                       // 期望 3 / 2
Console.WriteLine($"      c1.Target 与 c2.Target 同一个对象吗 = {ReferenceEquals(c1.Target, c2.Target)}");  // 期望 False

Console.WriteLine("—— ⑨ lambda 订阅事件 / 退订对照（TODO 5）——");
var channel = new EventChannel();
Action<string> handler = msg => Console.WriteLine($"      [存了变量] 收到：{msg}");
channel.Subscribe(handler);
channel.Publish("第一次");        // 期望打印
channel.Unsubscribe(handler);
channel.Publish("第二次");        // 期望【不打印】—— 退订成功

var ch2 = new EventChannel();
ch2.Subscribe(msg => Console.WriteLine($"      [现写] 收到：{msg}"));
ch2.Unsubscribe(msg => Console.WriteLine($"      [现写] 收到：{msg}"));   // ⚠️ 两个不同的 lambda
ch2.Publish("第三次");            // 期望【仍然打印】—— 这就是"退不掉"


// ⚠️ 下面两行【故意注释掉】——解开它们读报错（这就是"lambda 不能起个 var"）：
// var f = x => x + 1;                    // ① 推断不出来：lambda 自己没类型，要靠目标类型
// Func<int,int> g = x => x + 1;          // ② 右边必须给出目标类型才行（对比上面那行）

// ═══════ 类型区 ═══════

// TODO 1: 上面测试区的 `addAll` / `hello` 已经写好了一部分，**把它们补完整**
//         （如果你已经看到它们能编译，说明我会替你写好了；那你换成**自己写一遍**：
//           用 lambda 写一个 Action<string> 打印"[日志] {msg}"、写一个 Func<int,int,bool> 比较两数大小）
public static class LambdaBasics
{
    // TODO 1a: 用**表达式 lambda** 返回一个"判断偶数"的 Func<int, bool>
    //   ⚠️ 现在是"恒 false"的占位实现 → 测试区 ⑦ 会 ❌，把它改对
    public static Func<int, bool> MakeIsEven() => n => n % 2 == 0;

    // TODO 1b: 用 lambda 返回一个"取最大值"的 Func<int, int, int>
    //   ⚠️ 现在恒返回 0 → 改成"谁大返回谁"
    public static Func<int, int, int> MakeMax() => (a, b) => a > b ? a : b;

}

// TODO 2: 用【匿名方法】写一份等价逻辑（了解级，不强求）
//         语法：`delegate (参数列表) { 方法体 }`
//         写完之后自己回答：**为什么它和 lambda 能互相替换**？（提示：两者最终都编译成委托实例）

// TODO 3（今天的核心实验）：**计数器工厂** —— 每次调用得到一个【独立】的计数器
//   骨架：
//     public static Func<int> MakeCounter() {
//         int count = 0;
//         return () => ++count;      // 这个 lambda 捕获了 count
//     }
//   在测试区这样验证（自己加）：
//     var c1 = MakeCounter(); var c2 = MakeCounter();
//     c1(); c1(); c2();
//     Console.WriteLine(c1());     // 期望 3 —— c1 有自己的 count
//     Console.WriteLine(c2());     // 期望 2 —— c2 的是另一个 count
//   ⭐ 由此得出判据：**捕获的是"那一个变量实例"**；每次方法调用产生新的实例 → 互相隔离
public static class CounterFactory
{
    public static Func<int> MakeCounter()
    {
        int count = 0;
        return () => ++count;
    }
}

// TODO 4: 修好 ④ 段的 for 捕获（让三行分别打印 0 / 1 / 2）
//   ⚠️ 不许改测试区的调用方式；只能在循环体里动"变量"的引入位置
//   写完后回答：为什么这样就能"每轮一个变量"？（提示：for 的 i 全程只有一个；
//   在循环体内 `int copy = i;` 才是每轮新建）

// TODO 5（连回昨天的事件）：lambda 订阅事件，然后**试着退订**
//   骨架：
//     var channel = new EventChannel();
//     Action<string> handler = msg => Console.WriteLine("收到：" + msg);   // 存进变量
//     channel.Subscribe(handler);   // 内部 channel.News += handler
//     channel.Publish("第一次");
//     channel.Unsubscribe(handler);
//     channel.Publish("第二次");    // 期望：不再打印
//   ★ 再做一次对照：**不存变量、每次现写 lambda** 去 Subscribe/Unsubscribe → 退不掉（昨天实测的结论）
//   判据：**lambda 要用在"需要退订"的场合，就必须先把它存进字段/变量**
public class EventChannel
{
    public event Action<string>? News;

    public void Subscribe(Action<string> handler) => News += handler;      // ← 已给
    public void Unsubscribe(Action<string> handler) => News -= handler;    // ← 已给
    public void Publish(string msg) => News?.Invoke(msg);                  // ← 已给
    // 你只需要在测试区把 TODO 5 的用法写出来（并做那个"退不掉"的对照）
}
