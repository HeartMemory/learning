// ═══════ 今日主题：async 实战（10-19 · Block 4 第 8 天）═══════
//
// 【10-13 学了什么】Task / async / await 三个关键词 + 串行 vs 并发的"等待能不能重叠"。
// 【今天练什么】把它**用出去**：并发账 / 异步加载（带进度与异常）/ 技能冷却。
//   顺手把 10-18 计基自测卡暴露的 **async 三核心**用现象钉死（不是背，是看数字）。
//
// 【async 三核心（今天是复述 + 验证，不是新学）】
//   ① 阻塞的本质 —— 被"暂停"的是**状态机**，不是线程；线程在 await 那一刻被**释放回池**。
//      判据：`Thread.Sleep` 会**按住**一条线程，`await Task.Delay` 不会。
//   ② 三层切换者 —— 进程（活儿发生的地方）/ 线程（跑续体的工人）/ 状态机（真正被挂起的对象）。
//      判据：一段 async 方法的两段 await **不保证**在同一条线程上。
//   ③ SynchronizationContext（同步上下文）—— await 恢复时**捕获**上下文，续体交给它**投递**。
//      判据：控制台捕获到 null ⇒ 续体丢给线程池（大概率换线程）；
//            Unity 捕获到 UnitySynchronizationContext ⇒ Post 回主线程（这才是"协程感"的来源）。
//
// 【怎么用这个文件】
//   ① `dotnet run` 先跑骨架（没实现的 TODO 会打 ⏳，不崩）
//   ② 从 TODO 0 看现象 → TODO 1 补并发账 → TODO 2 补异步加载 → TODO 3 补冷却
//   ③ **每补一个就再跑一次**，对着数字核判据；判题器只报告不抛，一次能看到所有格子
//   ④ ⚠️ 线程号是"这次恰好"的快照，**不是**"永远如此"的定律 —— 它证明的是"可能换"。

using System.Diagnostics;   // Stopwatch

// ───────────────────────── 工具 ─────────────────────────

static int Tid() => Thread.CurrentThread.ManagedThreadId;

// 让"还没做"和"做错了"分开：NotImplemented ⇒ ⏳；别的异常继续冒（那是 bug，别捂盖子）
static async Task TryRunAsync(string title, Func<Task> act)
{
    Console.WriteLine($"—— {title} ——");
    try { await act(); }
    catch (NotImplementedException ex) { Console.WriteLine($"      ⏳ 还没做：{ex.Message}"); }
    Console.WriteLine();
}

static void Check(string what, bool ok, string detail = "")
    => Console.WriteLine($"   {(ok ? "✅" : "❌")} {what}{(detail.Length == 0 ? "" : "   " + detail)}");

static void Observe(string what, string detail)
    => Console.WriteLine($"   🔍 {what}   {detail}");

// ══════════════════════ TODO 0：现象（已写好，只跑不改）══════════════════════
//   盯三件事：初始线程号 / await 前后变没变 / 当前有没有 SynchronizationContext。

static async Task ProbeThreadAsync()
{
    Console.WriteLine($"   起点：线程 {Tid()}，上下文 = {SynchronizationContext.Current?.GetType().Name ?? "null"}");
    await Task.Delay(50);
    Console.WriteLine($"   await Delay 之后：线程 {Tid()}");
    await Task.Run(() => Console.WriteLine($"   Task.Run 内部：线程 {Tid()}"));
    Console.WriteLine($"   回到外层：线程 {Tid()}");
}

// ══════════════════════ TODO 1：并发账（时间 vs 线程代价）══════════════════════
//
//   三份"各要等 ms 毫秒"的活 × count 份，量【总耗时】：
//     · 串行 Delay     → 等待**不能重叠** ⇒ ≈ count × ms
//     · 并发 Delay     → 全部**同时等** ⇒ ≈ ms
//     · 并发 Sleep     → 也 ≈ ms（时间一样！）
//   ⭐ 判据（今天最贵的一条）：**"并发 Delay" 与 "并发 Sleep" 的耗时几乎相同** ——
//      差别不在时间，在**有没有线程被按住**。Sleep 版每条都得占一条线程池线程。
//
//   ── 三条实现约束（别绕开，绕开就量不出东西）：
//      ① 计时器必须 **new 在方法体内**、`StartNew()` 拿在**第一个 await 之前**
//      ② `Task.Delay` 版不许用 `Task.Run` 包（包住就变成"占着线程在等"，量不出区别了）
//      ③ `Sleep` 版**必须**包在 `Task.Run` 里 —— 直接 `Thread.Sleep` 会把主线程按住，一条都并发不起来

static async Task<long> SerialDelayAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();
    for(int i = 0;i < count; i++)
    {
        await Task.Delay(ms);
    }
    return sw.ElapsedMilliseconds;
}

static async Task<long> ConcurrentDelayAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();
    List<Task> list = new List<Task>();
    for(int i = 0;i < count; i++)
    {
        list.Add(Task.Delay(ms));
    }
    await Task.WhenAll(list);
    return sw.ElapsedMilliseconds;
}

static async Task<long> ConcurrentSleepAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();
    List<Task> list = new List<Task>();
    for(int i = 0;i < count; i++)
    {
        list.Add(Task.Run(() => Thread.Sleep(ms)));
    }
    await Task.WhenAll(list);
    return sw.ElapsedMilliseconds;
}

// ══════════════════════ TODO 2：异步加载（进度 + 异常）══════════════════════
//
//   ── 行为契约（照这个写，判题区就是按它核的）：
//      · 分 **4 档**推进度：25 → 50 → 75 → 100（用 `progress?.Report(…)`）
//      · 每档之间 `await Task.Delay(50)` 冒充 IO
//      · `name` 里含 `"bad"` ⇒ **在推进到 50 之后**抛 `InvalidOperationException`
//        （中途失败最像真实场景：进度已经报了一半）
//      · 成功返回 `$"{name}:ok"` 这样的字符串（内容随意，判题区只看它含不含 name）
//
//   ── 两个要想清楚的判据（注释里先答，别直接抄）：
//      ① **异常抛在 await 处还是调用处？** —— `LoadAsync` 里 `throw` 的那一行，
//         在调用方看来是"`await LoadAsync(...)` 这一行炸了"，还是"`LoadAsync(...)` 这行炸了"？
//         （提示：try/catch 的位置写错就接不住它）
//      ② **`IProgress<T>` 的回调跑在哪条线程？** —— 判题区会把线程号打出来，
//         注意它**不是** main 线程。原因和 TODO 0 的观察题是同一件事。

static async Task<string> LoadAsync(string name, IProgress<int>? progress)
{
    progress?.Report(25);
    await Task.Delay(50);
    progress?.Report(50);
    if(name.Contains("bad")) throw new InvalidOperationException("运行中途出错");
    await Task.Delay(50);
    progress?.Report(75);
    await Task.Delay(50);
    progress?.Report(100);
    return name;
}

// ══════════════════════ TODO 3：技能冷却（两条路对照）══════════════════════
//
//   ⭐ 这是今天唯一"有两条路"的题，收口判据写在最后。
//
//   ── 路线 A：async / await（事件式）
//      ⚠️⚠️ 先看这一条，它是最容易写错的地方：
//         `await 整个冷却时长` ⇒ **调用方被拖住 400ms**，第一个"首次使用"那一格会超时，
//         而且 await 完冷却早就结束了 ⇒ 紧接着的"冷却中再来"会返回 true（❌）。
//         ⇒ 正确姿势 = **上锁后立刻 return，冷却交给一个"不管它"的后台任务**：
//            `_ = StartCooldownAsync();`（fire-and-forget：内部 await Delay，完事解锁）
//
//      一句话流程：判冷却 → **立刻上锁** → try 跑效果 / catch 退锁并 rethrow
//                  → 启动冷却（不等它）→ `return true`
//      ⚠️ 上锁必须在第一个 await **之前**（放后面 = 没锁，两个调用方会双双通过）
//      ⚠️ 效果炸了必须**退锁**，否则技能**永久锁死**（判题区专门跑一次来验）
//
//   ── 路线 B：时间戳（状态式）
//      一句话流程：读时间戳 → 没到点就 `return false` → 推进 `_nextTick` → 跑效果 → `return true`
//      ⚠️ 时间戳用 `Stopwatch.GetTimestamp()`（不受系统时间被改影响）
//
//   📍 **具体台词（A ①~⑥ / B ①~⑤）已搬到文件末尾「类型区」的类里面** ——
//      就地写在填空处旁边，边看边写，不用来回滚。`Ctrl + End` 直达。
//
//   ⭐⭐ 收口判据（写完自己回答）：**"能不能用"是个【持续条件】还是个【一次性响应】？**
//      · 能随时问一次"现在能用吗" ⇒ 状态式（B）更贴合，零状态机、零分配
//      · 冷却**结束那一刻要做一件事**（播动画 / 发通知）⇒ 事件式（A）才合适
//      这条与 10-17 Unity 那条「**一次性响应用事件、持续条件读状态**」是**同一条**。

// ══════════════════════ 判题 / 演示区（已写好，只跑不改）══════════════════════

await TryRunAsync("TODO 0 · 线程号与同步上下文（只看现象）", async () =>
{
    await ProbeThreadAsync();
    Observe("控制台的 SynchronizationContext", "null ⇒ await 之后续体交给线程池，**不保证**回到原线程");
    Observe("Unity 里为何回主线程", "有 UnitySynchronizationContext ⇒ 续体被 Post 回主线程");
});

await TryRunAsync("TODO 1 · 并发账：时间 vs 线程代价", async () =>
{
    long serial = await SerialDelayAsync(5, 300);
    Check("串行 5×300ms ≈ 1500ms（等待叠加）", serial >= 1300 && serial <= 1900, $"{serial} ms");

    long conc = await ConcurrentDelayAsync(5, 300);
    Check("并发 Delay 5×300ms ≈ 300ms（等待重叠）", conc <= 700, $"{conc} ms");

    long sleep = await ConcurrentSleepAsync(5, 300);
    Check("并发 Sleep 5×300ms 也 ≈ 300ms（⚠️ 时间一样！）", sleep <= 800, $"{sleep} ms");

    Observe("三者耗时对照", $"串行 {serial} / 并发Delay {conc} / 并发Sleep {sleep} ms");
    Observe("⭐ 判据", "并发 Delay 与并发 Sleep 耗时几乎相同 ⇒ 差别不在时间，在\"有没有线程被按住\"");
    Observe("当前线程池线程数", $"{ThreadPool.ThreadCount}（并发 Sleep 那 5 条各占一条）");
});

await TryRunAsync("TODO 2 · 异步加载：进度 + 异常", async () =>
{
    int reported = 0;
    Progress<int> progress = new Progress<int>(p =>
    {
        reported++;
        Console.WriteLine($"     进度 {p}%　（回调线程 {Tid()}）");
    });

    string asset = await LoadAsync("hero", progress);
    await Task.Delay(80);   // 等 Progress 的回调排完（它是异步投递的）
    Check("成功返回且带上资源名", asset.Contains("hero"), asset);
    Check("进度报了 4 档（25/50/75/100）", reported == 4, $"实际 {reported} 次");

    try
    {
        await LoadAsync("bad-hero", null);
        Check("含 bad 时应当抛异常", false);
    }
    catch (InvalidOperationException ex)
    {
        Check("异常在【await 那一行】被接住", true, ex.Message);
    }

    Task<string> t1 = LoadAsync("bad-a", null);
    Task<string> t2 = LoadAsync("bad-b", null);
    try { await Task.WhenAll(t1, t2); }
    catch (InvalidOperationException) { /* WhenAll 只把【第一个】异常抛给你 */ }
    Observe("WhenAll 的异常账", $"t1 内部 {t1.Exception?.InnerExceptions.Count ?? 0} 个 / t2 内部 {t2.Exception?.InnerExceptions.Count ?? 0} 个");
    Observe("⭐ 判据", "await WhenAll 只抛【第一个】⇒ 想全都要，得去读每个 task 的 Exception");
});

await TryRunAsync("TODO 3 · 技能冷却：async 版 vs 时间戳版", async () =>
{
    SkillCooldownAsync a = new SkillCooldownAsync(400);
    Stopwatch w = Stopwatch.StartNew();
    Check("A 首次使用 → true", await a.TryUseAsync());
    w.Stop();
    Check("A 首次使用【不许阻塞】，应 < 200ms", w.ElapsedMilliseconds < 200, $"{w.ElapsedMilliseconds} ms（≈400ms = 你把冷却 await 了）");
    Check("A 冷却中再来 → false", !await a.TryUseAsync(), "（说明\"先上锁\"生效了）");
    await Task.Delay(500);
    Check("A 冷却结束后 → true", await a.TryUseAsync());

    SkillCooldownAsync b = new SkillCooldownAsync(1000);
    try { await b.TryUseAsync(() => throw new InvalidOperationException("效果炸了")); }
    catch (InvalidOperationException) { /* 预期内 */ }
    Check("A 效果抛异常后冷却【已复位】（忘了 finally ⇒ 这里会 false）", await b.TryUseAsync());

    SkillCooldownStamp s = new SkillCooldownStamp(400);
    Check("B 首次使用 → true", s.TryUse());
    Check("B 冷却中再来 → false", !s.TryUse());
    Thread.Sleep(500);
    Check("B 冷却结束后 → true", s.TryUse());

    // ── 真实场景长什么样：调用方把「效果」当 lambda 塞进去 ──
    Console.WriteLine();

    SkillCooldownAsync dash = new SkillCooldownAsync(400);
    bool dashed = await dash.TryUseAsync(async () =>          // ← 这是一个 Func<Task>
    {
        Console.WriteLine("     🏃 冲刺动画开始…");
        await Task.Delay(120);                                 //    效果本身是异步的（要等）
        Console.WriteLine("     🏃 冲刺动画结束");
    });
    Observe($"A 版调用方：冲刺（用出去了 = {dashed}）", "效果是个异步 lambda ⇒ 必须是 Func<Task>");

    SkillCooldownStamp fireball = new SkillCooldownStamp(400);
    bool fired = fireball.TryUse(() =>                         // ← 这是一个 Action
        Console.WriteLine("     💥 火球飞出去了（同步效果，不用等）"));
    Observe($"B 版调用方：火球（用出去了 = {fired}）", "效果是同步的一句 ⇒ 用 Action 就够");

    Observe("两版对外行为", "一致 ⇒ 但代价不同：A 有状态机 + 定时器，B 只有一次时间戳比较");
    Observe("⭐ 收口判据", "冷却\"现在能不能用\"是【持续条件】⇒ 状态式更贴合；\"冷却结束要播动画\"才是【一次性响应】⇒ 才用事件式");
});

// ══════════════════════ 类型区（方法体留给你写）══════════════════════
//   ⚠️ 顶级语句必须排在类型声明**之前** —— 所以类只能放在文件末尾（CS8803 就是这么来的）

public class SkillCooldownAsync
{
    private readonly int _cdMs;
    private bool _cooling;

    public SkillCooldownAsync(int cdMs) => _cdMs = cdMs;

    // 返回 true = 这次用出去了；false = 还在冷却
    public async Task<bool> TryUseAsync(Func<Task>? effect = null)
    {
        // ⭐ 六个空位，一行一格：
        //  ① 冷却中 ⇒ return false
        //  ② 立刻上锁（必须在第一个 await 之前）
        //  ③ try { 若 effect 不为空 → await effect(); }
        //  ④ catch { 退锁; throw; }        ← 忘了 throw; 判题区那格就接不到异常
        //  ⑤ 启动冷却任务，但【不等它】：  _ = StartCooldownAsync();
        //  ⑥ return true
        if (_cooling) return false;          // ①
        _cooling = true;                     // ② 上锁（注意：在第一个 await 之前）

        try                                  // ③
        {
            if (effect != null) await effect();
        }
        catch                                // ④
        {
            _cooling = false;                // 效果没成，这次用不算数 ⇒ 退锁
            throw;                           // 但异常要照实抛给调用方（throw; 保留原始栈）
        }

        _ = StartCooldownAsync();            // ⑤ 启动冷却，不等它（fire-and-forget）
        return true;                         // ⑥
    }

    // ⭐ 两格（这个才是"真的等"的那一段，所以它 await 得住）
    private async Task StartCooldownAsync()
    {
        await Task.Delay(_cdMs);             // ① 冷却时长走完
        _cooling = false;                    // ② 解锁
    }
}

public class SkillCooldownStamp
{
    private readonly int _cdMs;
    private long _nextTick;

    public SkillCooldownStamp(int cdMs) => _cdMs = cdMs;

    // 返回 true = 这次用出去了；false = 还在冷却
    public bool TryUse(Action? effect = null)
    {
        // ⭐ 五个空位，一行一格：
        //  ① 读当前时间戳 → long now = Stopwatch.GetTimestamp();
        //  ② 还没到 _nextTick ⇒ return false
        //  ③ 推进：_nextTick = now + 冷却时长【换算成 tick】
        //     Stopwatch.Frequency = 每秒多少个 tick ⇒ 400ms 的 tick 数 = _cdMs / 1000.0 * Stopwatch.Frequency
        //     ⚠️ 别写 _cdMs / 1000（整数除法 ⇒ 400/1000 = 0 ⇒ 冷却变成 0 毫秒）
        //  ④ 执行效果：effect?.Invoke();
        //  ⑤ return true
        long now = Stopwatch.GetTimestamp();                                  // ①
        if (now < _nextTick) return false;                                    // ②
        _nextTick = now + (long)(_cdMs / 1000.0 * Stopwatch.Frequency);       // ③
        effect?.Invoke();                                                     // ④
        return true;                                                          // ⑤
    }
}
