// ═══════ 今日主题：async / await 入门（10-13 · Block 4 第 2 课）═══════
//
// 【一句话】前面学的都是「把结果算出来」；今天学的是「**等的时候别把线程占着**」。
//
// 【为什么要学】Phase 0 之后要进 Unity 客户端：加载资源 / 等网络 / 等动画 / 冷却计时，
//   全是「要等一会儿」的活。今天先把"等"这件事的**成本**看清楚 ——
//   判据只有一句：⭐⭐ **「阻塞」= 一条线程被占住、却什么也没干。**
//
// 【三个关键词各答一个问题】
//   · Task    —— 一张「将来的结果」的**票据**（可等待对象）。⚠️ **它不是线程**。
//   · async   —— 只是**允许**你在方法里写 await，并把方法体编译成**状态机**。
//                ⚠️ 它**不开线程**（和 Unity 协程的 IEnumerator 是同一族手法）。
//   · await   —— **把控制权交回去 + 把剩下的部分登记成"续体"**。
//                ⚠️ 它不是"阻塞地等"；await 后面的代码**可能跑在另一条线程上**。
//
// 【判据链（本课的骨头）】
//   · 同步 vs 异步：同一秒里，**线程在等** 还是 **任务在等**？
//   · 串行 vs 并发：N 个"各等 t 毫秒"的活 —— 串行 ≈ N×t，并发 ≈ t
//                          ⇒ 差别不在"等得久不久"，在**这些等能不能重叠**
//   · 线程号判据：`Thread.CurrentThread.ManagedThreadId` 在 await 前后**可能不同**
//                  ⇒ 说明 async **不是**"在一条后台线程上跑完全程"
//   · ⛔ 边界（最容易搞混的一条）：Task.Delay 之所以不占线程，
//        是因为**等待的活在内核里**（定时器 / IO）。若把"等"换成 **CPU 空转**，
//        那这段代码**必须**占着一条线程 —— 那种活才归 `Task.Run` 管。
//
// 【与 Unity 协程对照（计划表今天那格）】
//                | Unity 协程 IEnumerator        | C# async / await
//   谁在推进      | **PlayerLoop**（每帧问一次）   | 线程池 / IO 完成通知
//   跑在哪条线程  | 主线程                        | 起点 = 当前线程；**await 之后不保证**
//   让出时机      | 写 `yield` 才让               | 写 `await` 才让（同族：协作式）
//   返回值        | ❌ 只能靠字段 / 回调          | ✅ `Task<T>`
//   try/catch     | ❌ 包不住两段之间的异常        | ✅ 异常在 `await` 处重新抛出
//
//   ⚠️ 控制台里**没有 SynchronizationContext** ⇒ await 之后大概率换线程；
//      Unity 里**有** ⇒ await 后会回到主线程（"看起来像协程"的原因就是这个）。
//
// 【怎么用这个文件】
//   ① `dotnet run` 先看骨架（未实现的 TODO 会打 ⏳）
//   ② 从 TODO 1 开始补，**每补一个就再跑一次**，对着数字核判据
//   ③ 判题器只报告、不抛（✅/❌ 一路打完），一次能看到所有格子

using System.Diagnostics;   // Stopwatch

// ───────────────────────── 工具 ─────────────────────────

// 让"还没做"和"做错了"分开：NotImplemented ⇒ ⏳；别的异常继续冒（那是 bug，别捂盖子）
static async Task TryRunAsync(string title, Func<Task> act)
{
    Console.WriteLine($"—— {title} ——");
    try { await act(); }
    catch (NotImplementedException ex) { Console.WriteLine($"      ⏳ 还没做：{ex.Message}"); }
    Console.WriteLine();
}

// 打成 ✅/❌ 而不抛 —— 让判题器一次跑完所有格子
static void Check(string what, bool ok, string detail = "")
    => Console.WriteLine($"   {(ok ? "✅" : "❌")} {what}{(detail.Length == 0 ? "" : "   " + detail)}");

// 观察项（不计分）：有些东西没有"对不对"，只有"看到了什么"
static void Observe(string what, string detail)
    => Console.WriteLine($"   🔍 {what}   {detail}");

// 假 IO：不真的读文件 / 不走网络，用 `Task.Delay` 冒充"这段活要等一会儿"
static Task FakeIoAsync(int ms) => Task.Delay(ms);

// ══════════════════════ TODO 1~4：你要写的方法 ══════════════════════

// ── TODO 1：串行 —— 一个接一个 await（等待会叠加）
//    判据：总耗时 ≈ count × ms
static async Task<long> SerialAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();     // ① 在【循环外】启动，拿到一块"表"（变量名 sw）
    for (int i = 0; i < count; i++)
    {
        await FakeIoAsync(ms);
    }
    return sw.ElapsedMilliseconds;           // ② 读【这块表】上的数（是 sw，不是 Stopwatch）
}


// ── TODO 2：并发 —— 让所有假 IO **同时**开始，再一起等
//    判据：总耗时 ≈ ms（不是 count × ms）
//    ⚠️ 关键点：**先全部启动、再统一等**（想想：如果在循环里就 await，是不是又变回 TODO 1 了？）
static async Task<long> ParallelAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();

    List<Task> list = new List<Task>();        // ① 「一起开始」= 先把票据全收进这个清单
    for (int i = 0; i < count; i++)
    {
        list.Add(FakeIoAsync(ms));                         // ← 填「启动一个假 IO」，⚠️ 这里【绝对不能写 await】
    }

    await Task.WhenAll(list);                   // ② 「统一等」= 等清单里所有票据全部完成

    return sw.ElapsedMilliseconds;
}


// ── TODO 3：让"线程号"自己说话（本次运行的观察项）
//    要做两件事：① 在方法第一行打印一次线程号（标 [进入]）
//                ② await FakeIoAsync(300) 之后，再打印一次（标 [await 之后]）
//    判据：这两个号**可能不同** —— 若不同，说明 await 之后是**另一条线程**接手的；
//          如果相同，说明恰好调度回同一条（控制台里两种都可能出现，多跑几次看）
static async Task ThreadReportAsync()
{
    Console.WriteLine($"   🔍 [进入]       线程号 = {Thread.CurrentThread.ManagedThreadId}");
    await FakeIoAsync(300);
    Console.WriteLine($"   🔍 [await 之后] 线程号 = {Thread.CurrentThread.ManagedThreadId}");
}


// ── TODO 4-A：Delay 版 —— count 个"各 ms 毫秒"的等待**一起**跑（全是假 IO）
//    判据：总耗时 ≈ ms
static async Task<long> DelayAllAsync(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();

    List<Task> list = new List<Task>();
    for (int i = 0; i < count; i++)
    {
        list.Add(Task.Delay(ms));
    }

    await Task.WhenAll(list);

    return sw.ElapsedMilliseconds;
}

// ── TODO 4-B：Sleep 版 —— count 个"各 ms 毫秒"的等待**串行**做（全是真阻塞）
//    判据：总耗时 ≈ count × ms；⚠️ 注意它**没有 async**：Thread.Sleep 是纯同步的阻塞
//    ⭐ 对照的意义：两个版本"等的时间"完全一样，差别只在【等的时候线程有没有被占住】
static long SleepSerial(int count, int ms)
{
    Stopwatch sw = Stopwatch.StartNew();

    for (int i = 0; i < count; i++)
    {
        Thread.Sleep(ms);        // 当场睡；没有东西可以收
    }

    return sw.ElapsedMilliseconds;
}


// ══════════════════════════════ 判题器 ══════════════════════════════

Console.WriteLine("═══════ 实验 A：串行 await（等待会叠加）═══════\n");
await TryRunAsync("TODO 1：SerialAsync(3, 300)", async () =>
{
    long t = await SerialAsync(3, 300);
    Check("总耗时 ≈ 900 ms（3 × 300 叠加）", t >= 850, $"{t} ms");
    Observe("判据", "这里线程有没有在等？—— 三个等待是一个接一个发生的");
});

Console.WriteLine("═══════ 实验 B：并发 Task.WhenAll（等待可以重叠）═══════\n");
await TryRunAsync("TODO 2：ParallelAsync(3, 300)", async () =>
{
    long t = await ParallelAsync(3, 300);
    Check("总耗时 ≈ 300 ms（三个等待重叠）", t <= 500, $"{t} ms");
});

Console.WriteLine("═══════ 实验 C：换一种「等」 —— Delay（假 IO）vs Sleep（真阻塞）═══════\n");
Console.WriteLine("   5 个「各 700 ms」的等待，两种做法【等的时间完全一样】。\n");
await TryRunAsync("TODO 4-A：DelayAllAsync(5, 700)  —— 假 IO 并发", async () =>
{
    long t = await DelayAllAsync(5, 700);
    Check("总耗时 ≈ 700 ms（5 个等待重叠，且没占线程）", t <= 1200, $"{t} ms");
});
await TryRunAsync("TODO 4-B：SleepSerial(5, 700)   —— 真阻塞串行", () =>
{
    long t = SleepSerial(5, 700);
    Check("总耗时 ≈ 3500 ms（5 × 700 叠加）", t >= 3300, $"{t} ms");
    return Task.CompletedTask;
});
Console.WriteLine("   ⭐ 数字对完，回答一句：这两版**等的时间**一样，差的是哪一件事？\n");

Console.WriteLine("═══════ 实验 D：线程号（观察项，不计分）═══════\n");
await TryRunAsync("TODO 3：ThreadReportAsync()", async () =>
{
    Console.WriteLine($"   🔍 [调用方] 调用前的线程号 = {Thread.CurrentThread.ManagedThreadId}");
    await ThreadReportAsync();
});
Console.WriteLine();
Console.WriteLine("   ⭐ 三个问题（这才是这块数字的用处）：");
Console.WriteLine("      ① 两个号不一样 ⇒ 说明 async **不是**「在一条线程上跑完全程」，那它是靠什么接上后半段的？");
Console.WriteLine("      ② Task.Delay 那一秒「占」了什么？（提示：不是线程 —— 那是什么在数时间？）");
Console.WriteLine("      ③ ⛔ 边界：如果把 FakeIoAsync 换成【CPU 空转】（while 空转到时间到），");
Console.WriteLine("         上面那套「并发 ≈ 300 ms」还成立吗？那种活该交给谁？（Task.Delay 还是 Task.Run？）");
Console.WriteLine();

Console.WriteLine("═══ 收尾：一句话判据 ═══");
Console.WriteLine("   · 同步 = 线程在等；异步 = 任务在等（线程被还回去了）");
Console.WriteLine("   · 串行 vs 并发：差的是【等能不能重叠】，不是【等得久不久】");
Console.WriteLine("   · ⭐ 只有\"等待发生在别处\"（定时器 / IO）时才省线程；CPU 空转永远占线程");
