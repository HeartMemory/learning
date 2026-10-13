# C# 主线练习记录

> 按 **年/月/日/主题** 组织（如 `2026/08/27/HelloWorld`）。每天只学 1 个板块，当天落到代码。

## 已完成

| 日期 | 主题 | 内容 | 目录 |
|---|---|---|---|
| 2026-08-27 | Hello World + 变量类型 + 输入输出 | 变量类型演示；ReadLine 打招呼 | [2026/08/27/HelloWorld](2026/08/27/HelloWorld/) |
| 2026-08-27 | 运算符 + 类型转换（提前） | 计算器雏形（+ - * / % 五则运算） | [2026/08/27/Calculator](2026/08/27/Calculator/) |
| 2026-08-28 | 分支语句（提前） | if/else、switch、三元表达式 → 计算器分支版 | [2026/08/28/Branch](2026/08/28/Branch/) |
| 2026-08-29 | 循环（提前） | for/while/do-while + 乘法表/数列求和/求和器 | [2026/08/29/Loops](2026/08/29/Loops/) |
| 2026-08-31 | 数组与 List + TryParse 输入验证 | 数组统计 + List 成绩单（含边界修复） | [2026/08/31/ArrayAndList](2026/08/31/ArrayAndList/) |
| 2026-09-01 | 字符串与常用方法（提前） | 倒序两种实现 / ASCII 字符统计 / 回文双指针 | [2026/09/01/Strings](2026/09/01/Strings/) |
| 2026-09-02 | 方法：定义/参数/返回值/作用域 | 方法工具箱：CountChar / IsPalindrome / Reverse / ReadNumber | [2026/09/02/Methods](2026/09/02/Methods/) |
| 2026-09-03 | 方法进阶：重载 + 递归 | Max 三重载（Tools 类）/ Factorial(20!) / Fibonacci / CountChar 子串重载 | [2026/09/03/AdvancedMethods](2026/09/03/AdvancedMethods/) |
| 2026-09-04 | 字符串进阶：`Split` / `Replace`（提前） | 连续空格切分后**过滤空串**；交互输入「原文 / 查找词 / 替换词」→ `Replace` 替换 + 用 `Split(查找词).Length - 1` **数出替换处数** | [2026/09/04/SplitReplace](2026/09/04/SplitReplace/) |
| 2026-09-07 | OOP 第一课：类/对象/构造函数/this/引用 | Student 类起步：类/构造/字段/方法/双对象独立验证 + 属性入门（Age/Name 门卫/计算属性/构造走属性） | [2026/09/07/StudentClass](2026/09/07/StudentClass/) |
| 2026-09-07 | **属性（Properties）专题** | `Student` 类的**两道门卫**：`Age`（**0~120 之外不写**）与 `Name`（`IsNullOrEmpty` 拦空）；**计算属性** `Introduce => $"..."`（只有 get）；构造走属性 + 默认参数 | [2026/09/07/Properties](2026/09/07/Properties/) |
| 2026-09-08 | 封装收尾 + 静态成员 | Encapsulation（IReadOnlyList 只读眼镜/卫语句/静态校验源）+ StaticMembers（const/static readonly/TotalCount 计数器/构造函数链） | [2026/09/08/Encapsulation](2026/09/08/Encapsulation/) · [2026/09/08/StaticMembers](2026/09/08/StaticMembers/) |
| 2026-09-09 | 继承入门（提前）+ this/base 专题 | Character/Hero/Party：base(...) 构造链 / this(...) 构造链 / this 传参（party.Add(this)）/ get-only 属性与 private set 封装墙 | [2026/09/09/Inheritance](2026/09/09/Inheritance/) |
| 2026-09-11 | 继承正课：protected 访问级别 | 三种访问级别家族模型（private 日记 / protected 传家宝 / public 公告栏）+ HP `{ get; protected set; }`：子类可写外界不可（Sacrifice 献祭场景 + 编译错误实感） | [2026/09/11/Protected](2026/09/11/Protected/) |
| 2026-09-11 | 多态（提前消化 09-12） | 角色类多态：virtual/override/base.成员 → **钩子模式重构**（骨架锁流程 + `protected virtual OnHit()` 开细节），基底版先 commit 存档再重构 | [2026/09/11/Polymorphism](2026/09/11/Polymorphism/) |
| 2026-09-30 | **Lambda / 匿名方法 / 闭包**（Block 3 第 3 课） | 三种写法（表达式 / 语句 / 无参 lambda）+ **匿名方法**（`delegate (int x) { return x * 3; }`，C# 2.0 前身，与 lambda 可互换）+ ⭐ **闭包实测**：`bump.Method.DeclaringType` = `Program+<>c__DisplayClass0_0`、`Target` 上躺着 `Int32 counter` / `Int32 shared` → **被捕获的局部变量被提升为显示类字段**（生命周期延长、同作用域共用一个显示类、`Target` 就是它）+ 两个经典坑：**`for` 捕获三行都是 3**（修法：循环体内 `int copy = i;`——"变量共享 + 延迟读"缺一不可）／**捕获的是变量不是值** + 判据：**要用 lambda 订阅事件，必须先把它存进变量**（现写的退不掉） | [2026/09/30/LambdaClosures](2026/09/30/LambdaClosures/) |
| 2026-09-29 | **事件 event + 观察者模式**（Block 3 第 2 课） | `event` 的封装语义（外部只能 `+=` / `-=`；**赋值或调用 = CS0070**，报错原文括号里写着"从声明类型中使用时除外"）+ 多播实测：**执行顺序 = 订阅顺序**、**退订顺序随意但须同一方法实例**（lambda 不存实例就退不掉）+ `NewsChannel` / `Subscriber` **观察者模式**（发布者不认识订阅者）+ **退订实验**：未退订的临时对象仍能收到通知（订阅链持有它 → GC 不回收） | [2026/09/29/Events](2026/09/29/Events/) |
| 2026-09-28 | **委托 delegate**（Block 3 首课） | 自定义委托 `MathOp`（声明 / 方法组实例化 / 调用）+ **多播**（`+=`/`-=`，两个都执行但只拿到最后一个返回值）+ `Action` / `Func` / 谓词 + **委托当参数 = 回调**（`Aggregate`）+ **`ScoreBoard` 注入通知者**模型（与 Breakout 的 `GameManager → Action<int>` 同构）；顺带实证：`Invoke` 是编译器生成的成员、委托是名义类型不能互转、`x(args)` = `x.Invoke(args)` | [2026/09/28/Delegates](2026/09/28/Delegates/) |
| 2026-10-08 | **泛型（Generics）** · 用「关卡数据」练手 | 泛型类 / 接口（`List<T>` / `Dictionary<K,V>` / `Queue<T>` —— 天天在用，今天看清长相）→ 泛型方法（`T` 出现在**签名**上）→ **约束**（`where T : IComparable<T>` / `new()`；**约束四连报错 CS1061 / CS0304 / CS0401 / CS0310**，⚠️ 最后一个是**调用处**报的）→ ⭐ **运算符要不来**（比大小用 `CompareTo`，写 `>` 报 CS0019） | [2026/10/08/Generics](2026/10/08/Generics/) |
| 2026-10-08 | **文件读写 + 异常 + JSON 配置表**（提前学 10-09 内容） | 四块地基：① **读**（`File.ReadAllText` / `ReadAllLines`）② **解析**（`JsonSerializer.Deserialize<T>` —— **泛型方法**，上午的泛型直接复用；⚠️ 默认只认 public **属性**、默认**区分大小写**，报错方式是**静默给默认值**）③ **容错**（文件缺失 / 内容坏 / 结构不对各一条契约；**抛型 vs Try** 的取舍）④ **写**（`WriteAllText` + `Serialize`，**写完读回来做往返测试**） | [2026/10/08/FileIO](2026/10/08/FileIO/) |
| 2026-10-09 | **LINQ**（Block 3 主线收口） | `Linq`：**15 组实验**（延迟执行 / 拉模型 / 节流阀与闸门 / 顺序改语义 / 顺序改代价 / `GroupBy` / `ToLookup` / `Join` / `GroupJoin` / 左连接 / `DefaultIfEmpty` / 聚合 / 空序列三种态度）；`LinqReport`：**合流练习「关卡通关报表」5 任务全绿**（筛 → 连 → 榜 → 摊 → 人） | [2026/10/09/Linq](2026/10/09/Linq/) · [2026/10/09/LinqReport](2026/10/09/LinqReport/) |
| 2026-10-12 | **存档 / 读档（三态契约 + 落点 + 原子写）**（Block 4 第 1 课 · 承 Block 3 顺延欠账 · **Phase 0 达标项**） | ⭐ **与 10-08「配置表」的分界**：配置表**只读**、随包发布；存档**运行时写**、会被弄坏、**会演化**（加新字段是常态）。四块：① **落点** —— 相对路径的基准是**进程工作目录**（会随启动方式变）⇒ 换 `AppContext.BaseDirectory` + `Path.Combine`；⚠️ 顺手排掉"看起来是正解"的 `Path.GetFullPath`（它拿工作目录当基准 ⇒ 把病根焊死）与 `Path.GetTempPath`（会被系统清理）② **三态容错读取** —— `Loaded` / `NotFound` / `Corrupted`：**一个 `bool` 装不下「第一次玩」与「存档坏了」**（前者是正常情况、后者要告诉玩家）③ **判题器 8 组**（落点 / 往返 / 没有存档 / 坏档 / **旧档缺字段** / `{}` 与字面 `null` / 原子写 / 覆盖写）④ ⏳ **原子写（TODO 3）顺延**（临时文件 → 整体替换；⚠️ `File.Replace` 要求目标已存在）。⭐⭐ 两条最值钱的判据：**① JSON 里没有的键 ⇒ 反序列化后取字段初始值 ⇒「旧存档缺字段」是免费演化**（判题器④ 四绿实证：旧档缺 `UnlockedLevel` → 照常 `Loaded` + 拿到初始值 `1`，**一行迁移代码都不用写**）；**② 「坏」有两种 ——「读不出来」用异常报告、「读出来了但内容不成立」用返回值报告**（第二种**永远不会抛**，`catch` 再多也接不住）。⭐ 编译器一路在守契约：**CS0177**（`out` 参数每出口必须赋值 ⇒ `catch` 里也得赋）、**CS0161**（**「加分支」≠「换分支」** —— 插 `if` 时别删后面那句 `return`）、**CS8601**（`Deserialize<T>` 返回 `T?` ⇒ 用可空局部变量接住，别用 `!` 压制）；另钉死 **「`catch` 只能接住写在 `try` 里面」的东西**（`try` 包**动作**、不包**判断**）—— 包错范围编译器**不报错**，是最危险的那种错 | [2026/10/12/SaveSystem](2026/10/12/SaveSystem/) |

## 里程碑成品（`Projects/CSharp/`）

- [Calculator](../Projects/CSharp/Calculator/)：计算器循环菜单版（历史记录 / 输入验证 / 除 0 保护 / 0 退出）
- [CalculatorV2](../Projects/CSharp/CalculatorV2/)：计算器函数化重构版（运算 / 输入 / 菜单全部拆成方法，与 V1 对照学习）
- [StringStats](../Projects/CSharp/StringStats/)：字符串统计工具（次数查询 / 字母统计 / 倒序 / 回文）
- [StringToolbox](../Projects/CSharp/StringToolbox/)：字符串工具箱收官版（五功能全方法化 + 重载 + 输入重试上限 + null 防御）
- [CharacterBattle](../Projects/CSharp/CharacterBattle/)：**角色对战模拟器**（第 5 号成品）——继承 + 多态（`List<Character>` 混装）+ 钩子模式（CalcDamage）+ 封装（`protected set`）+ 静态统计 + 战斗主循环 + **接口 `IAttackable`**（09-14：宝箱不继承 Character 也能挨打，`Attack` 参数换成接口）+ **09-15 重构三部曲**：`abstract class`（不可实例化）→ **参数化**（技能名/加成作构造参数，消灭 `if (Name == ...)`）→ **公共上提父类**（Hero/Monster 各只剩构造函数，`abstract` 退位为普通方法）

## 后续计划（Block 3 已收官 · 当前 Block 4）

- [x] 09-07~09-08：OOP 第一课（类/构造/属性）+ 静态成员 + 封装收尾（超前完成）
- [x] 09-09 晚：继承起步 + this/base 专题（Inheritance 练习，超前完成——09-10 补课勾销）
- [ ] 09-10：**装 Unity Hub**（提前启动下载）+ 学生类收尾验收
- [x] 09-11：继承正课（protected 访问级别）+ 栈与队列 232/225（均已完成）
- [x] 09-11：protected 访问级别 + **多态**（virtual/override/base.成员 + 钩子模式）——均已完成（多态提前一天）
- [x] 09-12：🏆 **角色类多态小 demo**（CharacterBattle 成品完成，第 5 号）
- [x] 09-13（周日）：复盘日 → **产出 Block 2 详细计划**（09-14~09-27，见 `Notes/2026/09/13.md`）
- [x] **Block 2（09-14~09-27）**：接口/抽象类 → 09-17 **Unity 正式开始** → 泛型/Dictionary → 🏆 **2D 打砖块**（对象池回收 + Animator 动画 + UGUI 计分 + **录屏 ≤30 秒 + 3 截图存档 demos/**）—— **09-27 收官 ✓**（全勤、无顺延）
- [x] **Block 3（09-28~10-11）**：**委托**（09-28 ✓）→ **事件**（09-29 ✓）→ **Lambda / 闭包**（09-30 ✓）→ **泛型收口**（10-08 ✓）→ **文件读写 / 异常**（10-08 提前 ✓）→ **LINQ**（10-09 ✓，五块实验 + 一份「关卡通关报表」合流练习）→ 第 7 号成品「2D 平台跳跃」的**存档 / 读档**（顺延 Block 4，已排 10-12）；C# 新特性一律先在**已入库旧项目**上做纯内部重构验证 —— **10-11 收官 ✓**（全勤 14 天，仅 1 笔顺延）
- [ ] **Block 4（10-12~10-25）**：**文件读写 + 异常 + 存档 / 读档**（10-12 ✓ 落点 + 三态容错读取全绿；**10-13 ✓ 原子写落地** —— 承 Block 3 顺延欠账，**Phase 0 达标项**）→ **async / await**（**10-13 ✓ 入门**：`AsyncBasics` 四组实测 —— 串行 940 / 并发 `WhenAll` 308 / `Delay` 并发 711 / `Sleep` 串行 3565 ms；10-19 实战）→ Unity 转 3D：**第 8 号成品「3D 跑酷雏形」`Projects/Unity/Runner`**（**10-13 ✓ 立项**：Universal 3D / URP 模板 + MCP 桥接自检 3/3 + `Assets/Scripts`；10-21 = Phase 0 验收日）
