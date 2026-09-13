# C# 基础笔记（Phase 0 · 2026-08-27 起）

> 学习过程中的知识点沉淀，按主题组织，随进度更新。
> 代码实例都在 `CSharpPractice/` 对应日期目录里，可运行验证。
> 覆盖进度：第 1-7 章 = 语言基础；第 8 章 = 踩坑；第 9-13 章 = 字符串/方法/静态成员；**第 14-21 章 = OOP 全家桶（类/引用/属性/封装/继承/多态/null/工程判断）**

## 1. 变量与类型（Day 1）

| 类型 | 用途 | 示例 |
|---|---|---|
| `int` | 整数 | `int age = 18;` |
| `double` | 小数 | `double price = 9.99;` |
| `bool` | 真假 | `bool study = true;` |
| `char` | 单字符（单引号） | `char grade = 'A';` |
| `string` | 字符串（双引号） | `string name = "World";` |

- 输入：`Console.ReadLine()` 返回 **string**，要数字必须转换
- 输出：`Console.WriteLine()` / 不换行用 `Write()`

## 2. 运算符与类型转换（Day 2）

- 算术：`+ - * / %`（取余）
- 整数除法会**丢小数**：`7 / 2 == 3`，想要 3.5 得先转 double
- 类型转换：`(double)a` 显式转换；`Convert.ToInt32()` 万能转换（对 null 宽容）；`int.Parse()` 只吃字符串、失败即崩

## 3. 字符串插值（Day 1 起，贯穿始终）

```csharp
Console.WriteLine($"{a} + {b} = {answer}");   // 推荐
Console.WriteLine(a + "+" + b + "=" + answer); // 拼接，长串易错
```

## 4. 分支（Day 3）

- `if / else if / else`：范围判断
- `switch (n) / case / break`：离散值匹配（菜单），`default` 兜底
- 三元：`条件 ? 值A : 值B`
- ⚠️ switch 里的 `break` 只跳出 switch，**退不出外层 while**（要退出循环用 `break` 于循环体或标志位）

## 5. 循环（Day 5）

| 循环 | 适用场景 | 口诀 |
|---|---|---|
| `for` | 知道次数 | 三段式：初始化; 条件; 步进 |
| `while` | 只知道条件 | 先判后做，注意改条件防死循环 |
| `do...while` | 至少执行一次 | 菜单/输入类首选 |
| `foreach` | 只读遍历集合 | 拿不到下标、不能改元素 |

- `break`：跳出整个循环；`continue`：跳过本轮
- 实战：九九乘法表 = 嵌套 for（外层行、内层列）；求和器 = do-while + 累加器

## 6. 数组与 List（Day 6）

```csharp
int[] nums = { 3, 1, 4 };        // 定长，Length
List<int> list = new List<int>(); // 可伸缩，Count（不是 Length！）
list.Add(x); list.Remove(x); list.RemoveAt(i); list[i]
```

- 数组：数量固定时用；`foreach` 便利但只读
- List：数量未知时用；`<int>` 是泛型语法（第 5 周深入）
- **统计初始值陷阱**：求 max/min 时初始值用 `nums[0]`（或范围边界如 100），**永远别用 0**——53 题全负数和成绩单 min=0 两次踩坑

## 7. 输入验证：TryParse（Day 6，重要）

```csharp
double num;
while (!double.TryParse(Console.ReadLine(), out num))
{
    Console.Write("请输入有效数字：");
}
```

- `TryParse` 转换失败**返回 false**（不崩溃），成功则结果通过 `out` 带出
- `int.TryParse` 验整数，`double.TryParse` 验小数
- 只要成败不要值：`out _` 丢弃符
- 对照：`int.Parse`（崩）、`Convert.ToInt32`（null 返 0，其余照崩）

## 8. 已踩过的坑（详见错题本.md）

1. 覆盖写/清零前先想清楚 `i - offset` 会不会等于 `i` 自己（283）
2. 全局极值配对 ≠ 最优解，单向遍历维护历史信息更稳（121/53）
3. 复制粘贴代码后逐字检查「模板部分」是否也要改（计算器历史记录运算符）
4. 测试用例必须覆盖：全无/全有/边界在中间（0 的三种位置、全负数、单元素）

## 9. 环境备忘

- 运行：终端 `dotnet run`（不要 F5，ReadKey 会因输入重定向报错）
- 新建项目：`dotnet new console -o 目录名`
- 提交格式：`MM-DD: [主题] 描述`
- git 全局代理：`http://127.0.0.1:7897`（Clash，push GitHub 必需，没开会连接失败）
- gh CLI 2.98.0 已登录，push 免密（完整路径 `C:\Program Files\GitHub CLI\gh.exe`）
- .NET SDK 8.0.424 ｜ VS Code 已开自动保存 ｜ 未装 Visual Studio（dotnet CLI 足够）

## 10. 字符串与常用方法（09-01/02）

**核心认知：string 不可变**——`s[i] = 'x'` 编译报错，任何「修改」都产生新字符串。

| 成员 | 作用 |
|---|---|
| `s.Length` / `s[i]` | 长度 / 取字符（char 类型，可越界崩溃）|
| `ToUpper() / ToLower()` | 大小写转换（返回新串）|
| `Substring(start, len)` | 截取 |
| `IndexOf(x)` | 查位置，找不到返回 -1 |
| `Contains / Replace / Split / Trim` | 包含 / 替换 / 切分 / 去首尾空白 |
| `char.IsDigit / IsLetter / IsWhiteSpace` | 字符类型判断 |

**char 与 int 互通（ASCII 技巧）**：
- `c - 'a'` → 字母转 0-25 下标（char 参与算术即转数字）
- `(char)('a' + i)` → 反向转回字母
- ⚠️ `char.IsLetter` 对所有语言字母为 true，`é - 'a'` 会数组越界——纯 a-z 场景用范围检查 `c >= 'a' && c <= 'z'`

**倒序两种实现**：循环 `+=`（法 A，直观）vs `ToCharArray()` 首尾交换后 `new string(chars)`（法 B，LeetCode 344 标准解）。

**回文判断**：双指针一头一尾往中间走（88 题三指针的镜像）。注意空字符串要先判 `Length == 0`，大小写敏感与否是设计决策（要合并先 `ToLower()`）。

**输出技巧**：统计结果点名打印（只输出出现过的）——`if (count[i] > 0) WriteLine($"{(char)('a'+i)}: {count[i]}");`

## 11. 方法进阶：重载与递归（09-03）

**重载（Overload）**：同名方法、不同参数列表（类型/个数/顺序），编译器按实参自动配对。返回值不同、参数相同 ≠ 重载（编译错误）。例：`Math.Max` 的 int/double 版本、`Console.WriteLine` 的几十个重载。

**⚠️ 重载是类成员特权**：顶层直接写的方法是**本地函数**，不支持重载（CS0128）。重载方法必须包进类：

```csharp
static class Tools
{
    public static int Max(int a, int b) { ... }        // 不写 public 默认 private，外部调不到
    public static double Max(double a, double b) { ... }
}
// 调用：Tools.Max(3, 5)
```

**顶层程序文件结构铁律**：可执行语句全部在前，类型声明（class）全部在后（CS8803）。

**访问修饰符**（可见范围滑块）：
`public`（谁都能访问）⊃ `protected internal` ⊃ `internal` / `protected`（本项目内 / 本类+子类）⊃ `private protected` ⊃ `private`（默认，仅本类）
现阶段记住：**public 给外人，private 留自己，protected 留儿子**（第 3 周继承时登场）。类成员不写修饰符默认 private。

**递归**：方法调用自己。两要素缺一不可：①终止条件（base case）②每次调用问题规模必须缩小。缺①爆栈 StackOverflowException；②不成立死循环。

```csharp
static long Factorial(int n)
{
    if (n <= 1) return 1;              // 0! = 1! = 1（基准值错了全链全错）
    return n * Factorial(n - 1);
}
```

- 递归 vs 循环：递归优雅贴合数学定义，循环快；递归版 Fib(35) 会因重复计算明显变慢（循环版无此问题）
- `int` 存 13! 就溢出，`long` 撑到 20! ——数据类型有边界

**三元运算符 `条件 ? 值A : 值B`**：if/else 的表达式版，三个槽位（条件/两分支）都能放方法调用（非 void），类型必须匹配。选值用三元，做动作用 if/else。

## 12. Split 与 Replace：切缝模型（09-04）

**Split 切分模型**：`Split(分隔符)` 每遇到一次完整匹配切一刀——**段数 = 切口数 + 1**，所以「出现次数 = `Split(查找词).Length - 1`」。

- ⚠️ 连续分隔符切出**空字符串**：`"a  b".Split(' ')` → `["a", "", "b"]`——统计时跳过空串
- `Split` 返回 `string[]`（数组，定长）——**null 赋值 ≠ 删除**，数组只有覆盖没有删除；需要删改用 List
- `string.Join(分隔符, 数组)` 是逆操作（缝回去）

**Replace = 切 + 缝**：`s.Replace(a, b)` ≡ `string.Join(b, s.Split(a))`——按查找词切开、用替换词缝回。由此推导全部行为：替换所有匹配 / 查不到原样返回 / **替换次数 = `Split(a).Length - 1`**。

- **计数铁律：在原文上、按查找词切**——结果串按替换词数会被「原文残留 + 替换产物拼接」污染
- Replace 从左往右扫，**替换产物不参与后续匹配**（`"aaa".Replace("aa","b")` = `"ba"`）
- Replace 区分大小写；查找词为空串会抛异常

**过渡 List 判断标准**：List 装的数据是「必须留存的」（历史记录、结果集）→ 该用；「只是过渡、转手就被消费」（过滤后统计）→ 用跳过逻辑 / 双指针直接处理，省掉中间容器。

## 13. 静态成员与构造函数专题（09-08）

**三种「不变/共享」成员**：

| | const | static readonly | static（普通）|
|---|---|---|---|
| 值何时定 | 编译期 | 运行期初始化一次 | 任何时刻可变 |
| 装什么 | 仅内置类型 + string | 任意类型 | 任意类型 |
| 用途 | 宇宙真理级常量（MaxAge、Pi）| 运行时定死后锁住（全局 Random）| 全班共享的可变状态（计数器）|

- 三者都**类名访问**；`static const` 是非法写法（const 隐式 static）
- const 值会「烧进」调用方代码——跨项目共享常量建议 static readonly

**静态方法铁律**：没有 this、摸不到实例字段（它不知道哪个对象在调用）；实例方法可以随便用静态成员。

**静态校验源（DRY）**：规则抽成 `public static bool IsValidAge(int age) => age >= MinAge && age <= MaxAge;` —— set 和构造函数共同调用，规则只写一处。

**构造函数链**：

```csharp
public Student() : this("张三", 18) { }   // 无参版委托带参版，门卫/计数自动继承
```

- 委托目标必须真实存在（不能单走）；执行顺序 = 目标构造先跑、自己的身体后跑
- **默认参数构造与无参重载并存 → CS0121 调用不明确**（二选一）
- 无参调用无专属动作 → 用默认参数（一个构造够）；要专属动作 → 才拆链式

**构造函数执行时序**（new Student("王五", 114514)）：
① 参数绑定（默认值轮空）→ ② 对象开地皮（字段=类型默认值：0/null）→ ③ 构造体按书写顺序执行（属性赋值=调 set 过门卫）→ ④ 交出地址

**字段初始化器**：`private List<string> _courses = new List<string>();` 在构造体之前执行；类型默认值（null）不可用时必须写（List 不初始化就是 null 定时炸弹）；int/string 默认值已够用可不写；**初始化器里不能引用实例成员**。

## 14. 类与对象 + 引用类型语义（09-07）

**类 = 模板，对象 = 实例**：`new Student(...)` 按模板造一个真实对象，返回它的**地址**。字段（数据）、方法（行为）、构造函数（出生仪式）是类的三件套。

**this 的本质**：每个实例方法/构造函数都自动带一个看不见的参数 `this`，指向“正在被操作的那个对象”——`s1.Introduce()` 时 this = s1，`s2.Introduce()` 时 this = s2（同一个方法能输出不同结果，就是 this 在换人）。

| this 用法 | 场景 |
|---|---|
| `this.成员` | 参数与字段同名时消歧义（`this.Name = Name`）——内层参数遮蔽外层字段，this 是唯一逃生通道 |
| `this(...)` | 构造函数链，委托本类另一个构造 |
| `this`（裸用） | 把“我自己”交出去（`party.Add(this)`）——完整对象，不是类的门牌号 |

**引用类型 vs 值类型（最重要的地基）**：

```csharp
Student a = new Student("张三");   // a 装的是【地址】，不是对象本身
Student b = a;                      // 复制地址 → 两个变量指向【同一个对象】
b.Name = "李四";                    // a 看到的也变了（别名效应）
ListNode cur = head;
cur = cur.next;                     // 【换地址】，不是"把对象改成下一个"
```

- `b.val = 99` 影响 `a` = 同一个对象的两个名字（别名）
- `null` = 空地址（变量没指向任何对象；碰它就是 NullReferenceException）
- 口诀：**值类型变量装值，引用类型变量装地址；赋值复制的是地址，不是对象**

## 15. 属性与封装（09-07 / 09-08）

**属性 = 字段的门卫**（get/set 就是两扇门）：

| 写法 | 含义 | 使用场景 |
|---|---|---|
| `public int Age { get; set; }` | 自动属性（编译器代管隐藏字段） | 无校验、全公开 |
| `public string Name { get; }` | 只读属性 | **构造函数是唯一写入窗口**（出生定死） |
| `public int HP { get; private set; }` | 读公开、写类内 | 外部只能看，只有类自己的方法能改 |
| 完整属性（带 `_age` 字段 + 卫语句） | 带校验的门卫 | 需要规则（`IsValidAge` 拦截非法值） |
| `public string Info => $"{Name}（{Age}岁）"` | 计算属性 | 没有独立存储，每次现算 |

**封装三件套（完整闭环）**：`private` 字段（藏起来）→ 属性门卫（校验）→ `IReadOnlyList` 只读眼镜（受控暴露）。
口诀：**字段藏起来、门卫站好岗、窗开多小自己定**。

- 只读眼镜：外部能 `Count`、能索引，**不能 `Add`**（编译错误 CS1061）——`List<string>` 只交出 `IReadOnlyList<string>` 视图
- 静态校验源（DRY）：`public static bool IsValidAge(int age) => age >= MinAge && age <= MaxAge;`——set 和构造函数共用，规则只写一处

## 16. 继承（09-09 / 09-11）

```csharp
public class Hero : Character   // 读作“Hero 是一种 Character”（is-a）
```

**子类白拿什么**：父类的 `public` 成员直接当自己的用；**但 private 的墙对亲儿子照样立着**（封装的墙不因继承而拆）。

**构造链：`base(...)` 与 `this(...)`**（同一个槽位，二选一）：

| 写法 | 委托给谁 | 典型场景 |
|---|---|---|
| `this(...)` | **本类**的另一个构造 | 多个构造共享初始化 |
| `base(...)` | **父类**的构造 | 子类构造必须交代“怎么造父类那部分” |

- **初始化器槽位是语法强制**：父类部分必须先完成、子类构造体后跑（先盖一楼再盖二楼），不是靠程序员自觉
- 父类**没有无参构造**时，子类必须显式写 `: base(...)`，否则编译错误
- 执行顺序：`new Hero(...)` → 先跑 `Character` 构造体 → 再跑 `Hero` 构造体
- 错误示范：把“调父类构造”写成方法体里的 `Character(name, hp);` ——语法错误，那是“把类名当方法调用”

**访问级别（三级家族模型）**：

| 修饰符 | 谁能访问 | 比喻 |
|---|---|---|
| `private` | 只有本类内部 | 我的日记，亲儿子也不给看 |
| **`protected`** | **本类 + 所有子孙类** | 家族传家宝：儿孙能用，外人免谈 |
| `public` | 全世界 | 门口公告栏 |

- 典型用法：`public int HP { get; protected set; }` ——**子类能直接改血量、外界只能看**
- 解决“父类数据子类要能用、外界不能碰”的矛盾（`private set` 太严，`public set` 太松）

## 17. 多态（09-11 / 09-12）

**三要素**：

| 关键词 | 写在哪 | 说什么 |
|---|---|---|
| `virtual` | 父类方法 | “我允许被改写”（发许可证） |
| `override` | 子类方法 | “我改写父亲的那个方法” |
| `base.方法()` | 子类 override 里 | “先跑父亲原版，我再加料” |

**绑定机制（灵魂）**：

```csharp
List<Character> party = ...;      // 声明类型全是 Character
foreach (Character c in party) c.Attack();   // 运行时按【真实身份】调子类版本
```

- 有 `virtual` + `override` → **运行期绑定**（看对象真实身份）
- 无 `virtual`（子类只能 `new` 隐藏）→ **编译期绑定**（看变量声明类型，多态失效，且编译器给 CS0114 警告）
- 删掉父类 `virtual` 却保留子类 `override` → **CS0506**：无法重写未标记为 virtual 的成员

**边界（都是追问出来的）**：
- `virtual` 管“能否改写”，访问级别管“谁能调用”——两件事；`override` **不能改变访问级别**
- `base` 不要求 virtual（`base(...)` 构造链和 `base.方法()` 都能用；virtual 决定的是多态能否生效）
- `private virtual` 不存在（看不见怎么改写）；`static virtual` 不存在（静态成员没有对象）
- `base` 永远指向**直接父类**（三级继承时逐级叠加，不是跳到最顶层）

**改写粒度 = 整个方法**。想“只改一部分”，两个办法：
1. `base.方法()` —— 父类整段拿来，前后加料
2. **父类拆钩子**（模板方法模式）——见下

**钩子模式（模板方法）**：

```csharp
public void Attack() {            // 骨架：不 virtual = 流程锁死
    Console.WriteLine($"{Name} 发动了基础攻击！");
    OnHit();                      // 可变点：交给子类
    Console.WriteLine("攻击结束。");
}
protected virtual void OnHit() { }   // 空钩子：子类按需改写，不改就用空的
```

- **骨架方法**（锁流程）+ **`protected virtual` 钩子**（开细节）——流程安全、细节自由
- Unity 的 `Start()` / `Update()` 就是这个机制：全是空钩子等你填

## 18. null 与可空引用类型（09-12）

**隐式契约 vs 显式契约**：

```
隐式契约：HasAlive 看「最后一个」+ FindFirstAlive 兜底返回「最后一个」+ 调用处判 .HP
          → 三处必须“同时”盯着同一个元素，改一处就炸（实践里当场 NRE）
显式契约：FindFirstAlive 找不到就返回 null —— “没有”用数据本身表达，不依赖任何隐含前提
```

- 原则：**错误要用数据表达**（null / 异常 / 空集合），不要靠“别处的实现恰好配合”
- **三步铁律**：先接住（存进变量）→ 再检查（判空）→ 后使用——绝不在表达式里直接 `F().某个成员`

**可空引用类型标注**：

```csharp
private static Character? FindFirstAlive(...)   // ? = “我可能返回空”
```

- 加了 `?` 后，调用处直接 `.HP` 会得到**编译警告**（“解引用可能为 null”）——编译器替你在每个调用点站岗
- 返回值类型声明为 `Character`（承诺非空）却返回 null → **CS8603 警告**：承诺与实现不一致

## 19. 控制流层级：return / break / continue（09-12）

| 关键词 | 结束谁 | 影响 |
|---|---|---|
| `continue` | 跳过本轮 | 循环继续下一轮 |
| `break` | **当前这一层循环** | 循环结束，**流程继续往下**（循环外代码会执行） |
| `return` | **整个方法** | 循环外的一切代码**永远不执行** |

**判断标准**：结束“这一轮 / 这一队”用 `break`；结束“整个流程”（参数非法等卫语句）用 `return`。

- ⚠️ 用 `return` 提前退出循环 → 循环外的收尾代码（统计、结算、胜负判定）全部被跳过
- **防护归防护、决策归决策**：循环内判空只 `break`（局部防护），胜负/结果判定放循环外**唯一一处**（DRY）

## 20. 容器 API 速查（09-09 ~ 09-13）

| 容器 | 进 | 出 | 看 | 其他 |
|---|---|---|---|---|
| `Stack<T>`（弹夹，LIFO） | `Push(x)` | `Pop()` | `Peek()` | `Count` / `Contains` / `Clear` |
| `Queue<T>`（排队，FIFO） | `Enqueue(x)` | `Dequeue()` | `Peek()` | 同上 |
| `List<T>` | `Add(x)` | `Remove(x)` / `RemoveAt(i)` | `list[i]` | `Count` / `Contains` |
| `IReadOnlyList<T>` | ❌ | ❌ | `list[i]` | `Count`（只读眼镜） |

- **空栈/空队列上 `Pop`/`Dequeue`/`Peek` 直接抛异常**——动手前先看 `Count`
- `Count` 是**属性**（无括号），`Contains()` 是**方法**（有括号）
- 循环边界铁律：`for` 条件里**别引用会变的集合**（`i < list.Count` 会在删除/弹出时半路翻车）→ 用 `while (list.Count != 0)`；必须用 for 时先把 Count **冻进局部变量**
- Stack/Queue 的进出都是 O(1)；`Contains` 是 O(n) 遍历

## 21. 工程判断小抄（09-12 / 09-13）

1. **幂等的冗余是廉价的，被遗忘的收尾是昂贵的**——82 题：`prev.next = cur` 在中间位置会重复赋一次同值（无害），但省掉它就得靠循环后 `prev.next = null` 收尾（忘了就悬尾 bug）→ 保留冗余
2. **“碰巧对”≠“保证对”**——测试全绿不等于逻辑正确，边界用例（重复族在末尾、开头无 0、全负数）才是照妖镜
3. **数据做【分支条件】❌，数据做【计算输入】✅**——`if (Name == "剑士") dmg += 5;` 是反模式；`return base.CalcDamage() + _skillBonus;`（参数传入）才对
4. **数值差异用数据参数，行为差异才用子类**——职业只差加成值 → 构造参数；职业有真正不同的行为 → 子类
5. **重构后必须重跑完整流程**——旧行为可能“寄生”在被你删掉的那行代码上（88 判空曾兼职胜负判定）
6. **注释是代码的影子**——代码改了注释必须同步，否则误导后来人（包括未来的自己）
7. **改写前先 commit 存档**——git 历史就是最真实的错题记录，`git diff` 一拉，从错到对的演化一目了然

## 📌 回访清单（学到对应内容时回来重构）

- [ ] **`ReadNumber`（CalculatorV2）**：现在输入流结束（null）时只能返回 0 凑合——学到**异常处理**后，改成把「无输入」上抛给主循环统一处理的正规写法（2026-09-02 记）
- [ ] 学完**重载/递归**后：回顾 `ReadNumber` 的提示语设计是否可以用重载简化（2026-09-03 重载已学，待实践）
- [x] **复盘日（09-06）：List 过渡重构练习**——①13 罗马数字：硬编码减法搭档单遍版 ✓；②125 回文：原串双指针跳脏字符零额外空间版 ✓（2026-09-06 完成）
- [ ] 学完 **Dictionary**（第 5 周）后：`CharacterBattle` 的 `if (Name == "剑士")` 分支 → 用参数化或查表重构（2026-09-09/09-12 记）
- [ ] 学完 **HashSet**（第 5 周）后：141 环形链表补一个哈希集合解法对比（2026-09-08 记）
- [ ] 学完**异常处理**后：Student 属性的「丢弃策略」（非法值静默 return）升级为抛异常（2026-09-08 记）
- [ ] 学完 **Unity** 后：把 `CharacterBattle` 的类结构（Character/Hero/Monster + 钩子）搬到 Unity 角色系统（2026-09-12 记）
