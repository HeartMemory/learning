// ═══════ 今日主题：委托 delegate —— 把"方法"当成"值"来传 ═══════
//
// 【一句话】委托类型 = 一份【方法签名的合同】；委托实例 = 一个"可以传来传去的**方法**"。
//
// 【为什么要它 · 生活模型】
//   平时写 `Calculator.Add(3, 4)`，是你【直接点名叫人干活】——写代码那一刻就必须知道方法叫什么、属于谁。
//   委托是【先留一个空位】：代码里写"这一格将来要放一个 (int,int)->int 的方法"，
//   具体塞哪个方法，等运行时由调用方决定 → 这就是**回调（callback）**。
//   好处：调用方和被调用的方法【互相不需要认识】，只认那份"签名合同"。
//
// 【今天要亲手打的三块地基】
//   ① 自己声明委托类型 + 用"方法组"造实例 + 调用它（`MathOp op = Calculator.Add;`）
//   ② 多播：一个委托变量挂多个方法（`+=` / `-=`）
//      ⚠️ 带返回值时，多播只能拿到【最后一个】的返回值 —— 想清楚为什么，这是今天的思考题
//   ③ 内置委托 **Action / Func**：官方现在推荐用它们，能不自己声明类型就别声明
//      `Action<T>` = 有参无返回；`Func<..., TResult>` = 有参有返回（**最后一个**泛型参数是返回类型）
//   + 落点：把"委托当参数"写成回调 —— **这就是今天要搬进 Breakout 的写法**（⑤ 那段）
//
// ⚠️ 顺序提醒：**TODO 1 不写，整份文件编译不过**（CS0246 找不到类型 MathOp）。
//    这不是 bug，正是"你还在用没声明过的东西"——先把地基①打好，再往下跑。
//
// 【怎么用这个文件】
//   下面测试区每一行都写了【期望值】。先看期望 → 去类型区把 TODO 补完 → `dotnet run` 对着期望核。

// ═══════ 测试区 ═══════

Console.WriteLine("—— ① 自定义委托：一个变量，先后装两个不同方法 ——");
MathOp op = Calculator.Add;                       // 方法组 → 委托实例（隐式转换，"把 Add 装进空位"）
Console.WriteLine($"op(3, 4) = {op(3, 4)}");      // 期望 7
op = Calculator.Subtract;                         // 同一个变量换一个方法（委托是【变量】，当然能重新赋值）
Console.WriteLine($"op(3, 4) = {op(3, 4)}");      // 期望 -1

Console.WriteLine("—— ② 多播：一个委托挂多个方法 ——");
MathOp chain = Calculator.Add;
chain += Calculator.Subtract;                     // 再加一个（注意：+= 是"加"，不是"换"）
Console.WriteLine($"chain(3, 4) = {chain(3, 4)}");// 期望 -1 ← ⚠️ 两个方法都跑了，但你只拿到【最后一个】的返回值
chain -= Calculator.Add;                          // 减掉一个
// ⚠️ 这一行编译器会给你一条**警告**（CS8602：解引用可能出现空引用）——先别急着消它，读它的意思：
//    编译器在说"`-=` 有可能把 chain 减成【空】"，因为**委托是引用类型的对象**，`chain` 里可以一个方法都没有。
//    这就是明天 event 要管的"订阅链可能为空"，也是"为什么调用前常写 `?.Invoke()`"。
Console.WriteLine($"chain(3, 4) = {chain(3, 4)}");// 期望 -1（现在只剩 Subtract 了，所以结果一样——第二个问题：怎么证明"两个都跑了"？）

Console.WriteLine("—— ③ 内置委托 Action / Func ——");
Action<string> log = Calculator.Log;              // Action<T>：吃一个 T，不返回
log("Action 也能装方法");                          // 期望打印：[通知] Action 也能装方法
Func<int, int, int> mul = Calculator.Multiply;    // Func<in, in, out>：最后那个 int 是返回类型
Console.WriteLine($"mul(3, 4) = {mul(3, 4)}");    // 期望 12
Func<int, bool> isBig = Calculator.IsBig;         // 返回 bool 的 Func —— 习惯上叫"谓词 predicate"
Console.WriteLine($"isBig(50) = {isBig(50)}");    // 期望 True
Console.WriteLine($"isBig(5) = {isBig(5)}");      // 期望 False

Console.WriteLine("—— ④ 委托当参数 = 回调（今天的重头戏）——");
int[] nums = { 1, 2, 3, 4 };
Console.WriteLine($"Aggregate(add) = {Calculator.Aggregate(nums, Calculator.Add)}");         // 期望 10
Console.WriteLine($"Aggregate(mul) = {Calculator.Aggregate(nums, Calculator.Multiply)}");   // 期望 24
Console.WriteLine($"Aggregate(sub) = {Calculator.Aggregate(nums, Calculator.Subtract)}");   // 期望 -8（1-2-3-4）

Console.WriteLine("—— ⑤ 落点：把「分数变了」通知出去（= Breakout 里 GameManager → ScoreManager 那条线）——");
ScoreBoard board = new ScoreBoard(10, Calculator.ReportScore);  // ⭐ 造板子时把"通知者"塞进去（依赖注入的味道）
board.AddBricks(3);                                             // 期望 [通知] 当前分数：30
board.AddBricks(3);                                             // 期望 [通知] 当前分数：60
board.AddBricks(2);                                             // 期望 [通知] 当前分数：80（3+3+2 = 8 块 × 10 分）

// ⚠️ 明天要收拾的烂摊子（今天故意先写坏）：
//   `OnScoreChanged` 是个 public 字段，外部一句话就能把别人挂的通知整个清掉：
board.OnScoreChanged = Calculator.ReportScore;   // ← 能编译、能跑，这就是问题本身
//   09-29 的 event 就是来上这把锁的 —— 留着这段，明天对照着改。
Console.WriteLine(board.Score);                  // 期望 80（分数是【状态】，通知是【副作用】，互不影响）

// ═══════ 类型区 ═══════

// TODO 1: 声明一个【委托类型】，名字叫 MathOp，它描述的签名是："吃两个 int、返回一个 int"。
//         语法：`public delegate 返回类型 委托类型名(参数列表);`
//         ⭐ 写的时候念一遍：这不是"一个方法"，是"**一种方法的形状**"（签名合同）。
// public delegate ___ ___;
public delegate int MathOp(int a, int b);

public static class Calculator {
    // TODO 2: Add / Subtract / Multiply 三个纯计算方法（减法按 (a - b)，乘法按 a * b）
    public static int Add(int a, int b) {
        return a + b;   // ← 已给：委托装的就是这种"形状对得上"的方法
    }
    public static int Subtract(int a, int b) {
        // TODO 2a: 实现减法
        return a - b;
    }
    public static int Multiply(int a, int b) {
        // TODO 2b: 实现乘法
        return a * b;
    }

    // TODO 3: IsBig —— 判断一个数是不是"大数"（约定：大于 10 就算大）
    public static bool IsBig(int n) {
        if(n > 10) return true;
        return false;
    }

    // TODO 4: Log —— 打印一行，格式：`[通知] {msg}`
    public static void Log(string msg) {
        Console.WriteLine($"[通知]{msg}");
    }

    // TODO 5: ReportScore —— 分数变化时的"广播稿"，打印格式：`[通知] 当前分数：{score}`
    //         （它和上面的 Log 长得像，但**签名不同**：这个吃 int、那个吃 string ——
    //           委托最挑的就是签名，差一个类型都装不进同一个 Action）
    public static void ReportScore(int score) {
        Console.WriteLine($"[通知]当前分数{score}");
    }

    // ★ TODO 6（今天最值钱的一步）：Aggregate —— 把数组里的数【按 op 指定的规则】合并成一个结果
    //
    //   看签名：它不认识 Add / Multiply，只认识"一个 MathOp"（= 签名合同）→ 这就是**解耦**。
    //   实现思路：拿一个累加器，**从第 0 个元素起手**（⚠️ 别从 0 起手！从 0 开始的话 subtract / multiply 都会错），
    //             剩下的元素逐个 `累加器 = op(累加器, 当前元素);`
    //
    //   ⚠️ 顺手想一个边界：nums 长度为 0 时你这个实现会怎样？（先想，再实测验证你的判断）
    public static int Aggregate(int[] nums, MathOp op) {
        int result = nums[0];
        for(int i = 1;i < nums.Length; i++)
        {
            result = op(result,nums[i]);
        }
        return result;
    }
}

// ⑤ 段的"板子"：它自己不知道分数变了要通知谁，只认一个"能吃 int 的回调"
public class ScoreBoard {
    // ⚠️ 这是一个【字段】，而且现在是 public —— 今天先这样，明天的 event 会把它锁上
    public Action<int>? OnScoreChanged;

    private readonly int _scorePerBrick;
    private int _score;

    // TODO 7: 把"每块砖多少分"和"分数变化时通知谁"存下来
    //         （提示：委托就是一个普通值，直接赋给字段即可；别在这里就把"怎么通知"写死）
    public ScoreBoard(int scorePerBrick, Action<int> onScoreChanged) {
        _score = 0;   // ← 已给：开局 0 分
        _scorePerBrick = scorePerBrick;
        OnScoreChanged = onScoreChanged;
    }

    public int Score => _score;   // 只读状态属性（对外能看不能改 —— 09-08 的 IReadOnlyList 同一思路）

    // TODO 8: 打掉 count 块砖 → 分数累加 → 通知出去
    //         期望输出：`[通知] 当前分数：{新分数}`（用注入进来那个委托，**别直接调 Calculator.ReportScore**）
    //         ⭐ 要体会的正是"解耦"：这个类从头到尾没出现过 Calculator 这个名字
    public void AddBricks(int count) {
        _score += count * _scorePerBrick;
        OnScoreChanged?.Invoke(_score);
    }
}
