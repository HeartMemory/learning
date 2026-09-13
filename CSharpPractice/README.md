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
| 2026-09-07 | OOP 第一课：类/对象/构造函数/this/引用 | Student 类起步：类/构造/字段/方法/双对象独立验证 + 属性入门（Age/Name 门卫/计算属性/构造走属性） | [2026/09/07/StudentClass](2026/09/07/StudentClass/) |
| 2026-09-08 | 封装收尾 + 静态成员 | Encapsulation（IReadOnlyList 只读眼镜/卫语句/静态校验源）+ StaticMembers（const/static readonly/TotalCount 计数器/构造函数链） | [2026/09/08/Encapsulation](2026/09/08/Encapsulation/) · [2026/09/08/StaticMembers](2026/09/08/StaticMembers/) |
| 2026-09-09 | 继承入门（提前）+ this/base 专题 | Character/Hero/Party：base(...) 构造链 / this(...) 构造链 / this 传参（party.Add(this)）/ get-only 属性与 private set 封装墙 | [2026/09/09/Inheritance](2026/09/09/Inheritance/) |
| 2026-09-11 | 继承正课：protected 访问级别 | 三种访问级别家族模型（private 日记 / protected 传家宝 / public 公告栏）+ HP `{ get; protected set; }`：子类可写外界不可（Sacrifice 献祭场景 + 编译错误实感） | [2026/09/11/Protected](2026/09/11/Protected/) |
| 2026-09-11 | 多态（提前消化 09-12） | 角色类多态：virtual/override/base.成员 → **钩子模式重构**（骨架锁流程 + `protected virtual OnHit()` 开细节），基底版先 commit 存档再重构 | [2026/09/11/Polymorphism](2026/09/11/Polymorphism/) |

## 里程碑成品（`Projects/CSharp/`）

- [Calculator](../Projects/CSharp/Calculator/)：计算器循环菜单版（历史记录 / 输入验证 / 除 0 保护 / 0 退出）
- [CalculatorV2](../Projects/CSharp/CalculatorV2/)：计算器函数化重构版（运算 / 输入 / 菜单全部拆成方法，与 V1 对照学习）
- [StringStats](../Projects/CSharp/StringStats/)：字符串统计工具（次数查询 / 字母统计 / 倒序 / 回文）
- [StringToolbox](../Projects/CSharp/StringToolbox/)：字符串工具箱收官版（五功能全方法化 + 重载 + 输入重试上限 + null 防御）
- [CharacterBattle](../Projects/CSharp/CharacterBattle/)：**角色对战模拟器**（第 5 号成品）——继承 + 多态（`List<Character>` 混装）+ 钩子模式（CalcDamage）+ 封装（`protected set`）+ 静态统计 + 战斗主循环

## 后续计划（Block 1 剩余 + Block 2）

- [x] 09-07~09-08：OOP 第一课（类/构造/属性）+ 静态成员 + 封装收尾（超前完成）
- [x] 09-09 晚：继承起步 + this/base 专题（Inheritance 练习，超前完成——09-10 补课勾销）
- [ ] 09-10：**装 Unity Hub**（提前启动下载）+ 学生类收尾验收
- [x] 09-11：继承正课（protected 访问级别）+ 栈与队列 232/225（均已完成）
- [x] 09-11：protected 访问级别 + **多态**（virtual/override/base.成员 + 钩子模式）——均已完成（多态提前一天）
- [x] 09-12：🏆 **角色类多态小 demo**（CharacterBattle 成品完成，第 5 号）
- [x] 09-13（周日）：复盘日 → **产出 Block 2 详细计划**（09-14~09-27，见 `Notes/2026/09/13.md`）
- [ ] **Block 2（09-14~09-27）**：接口/抽象类 → 09-17 **Unity 正式开始** → 泛型/Dictionary → 🏆 **2D 打砖块**（对象池回收 + Animator 动画 + UGUI 计分 + **录屏 ≤30 秒 + 3 截图存档 demos/**）
