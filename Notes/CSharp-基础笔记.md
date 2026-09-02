# C# 基础笔记（Phase 0 · Day 1-6）

> 学习过程中的知识点沉淀，按主题组织，随进度更新。
> 代码实例都在 `CSharpPractice/` 对应日期目录里，可运行验证。

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

## 📌 回访清单（学到对应内容时回来重构）

- [ ] **`ReadNumber`（CalculatorV2）**：现在输入流结束（null）时只能返回 0 凑合——学到**异常处理 / 可空类型**后，改成把「无输入」上抛给主循环统一处理的正规写法（2026-09-02 记）
- [ ] 学完**重载/递归**后：回顾 `ReadNumber` 的提示语设计是否可以用重载简化
