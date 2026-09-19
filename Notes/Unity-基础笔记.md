# Unity 基础笔记（Phase 0 · 2026-09-16 起）

> 从「C# 基础笔记」拆出的 Unity 专册（2026-09-19 拆分），此后 Unity 相关知识一律记在这里，C# 语法/语言特性留在 `CSharp-基础笔记.md`。
> 代码实例在 `Projects/Unity/`（成品）与 `D:\Unity\Projects\`（练习）里，可运行验证。

**章节对照（拆分时的来源）**：

| 本册 | 原标题 | 日期 |
|---|---|---|
| 第 1 章 | C# 基础笔记 · 第 25 章 Unity 脚本基础与生命周期 | 09-16 |
| 第 2 章 | C# 基础笔记 · 第 26 章 Unity 空间与运动 | 09-17 |
| 第 3 章 | C# 基础笔记 · 第 29 章 Unity 输入与物理 | 09-18 |
| 第 4 章 | 本册新增（Prefab / 实例化 / 场景装配） | 09-19 |

---

## 1. Unity 脚本基础与生命周期（09-16）

### 一、从 .NET 控制台到 Unity：4 个环境差异（同一份 C# 代码会"炸"的原因）

| # | 差异 | 控制台项目（dotnet） | Unity 脚本程序集 |
|---|---|---|---|
| 1 | **入口点** | 支持**顶级语句**（文件顶层直接写代码） | ❌ 报 **CS8805**（程序集是 DLL，没有入口）→ 代码必须进方法 |
| 2 | **隐式 using** | `.csproj` 有 `<ImplicitUsings>enable</ImplicitUsings>`，`List<>`/`Console` 自动可用 | ❌ 老式 csproj，**必须手写** `using System.Collections.Generic;` → 否则 **CS0246** |
| 3 | **输出目标** | `Console.WriteLine` → 终端 | ⚠️ 能编译，但只写进 `Editor.log`，**不显示在控制台窗口** → 必须用 `Debug.Log` |
| 4 | **可空上下文** | 可开 `<Nullable>enable</Nullable>` | ❌ 默认关闭，`Character?` 报 **CS8632**（警告）→ 去掉 `?`（原理见 `CSharp-基础笔记.md` 第 18 章） |

> 🔑 **迁移检查清单**（任何 C# 代码搬进 Unity，先过这四项）：顶级语句？using 齐吗？`Console` 换 `Debug.Log` 了吗？可空问号？

### 二、MonoBehaviour = "能被挂到 GameObject 上的组件"

```csharp
public class LifecycleDemo : MonoBehaviour   // 继承 MonoBehaviour = "我是一个组件"
```

- **写类 ≠ 有对象**：脚本文件建好了但**没挂到任何 GameObject 上 → 一行都不会执行**（引擎只对场景中的**组件实例**点名）。这就是"播放后控制台一片空白"的头号原因
- 硬约束：**一个 `.cs` 文件最多一个 `MonoBehaviour`，且文件名必须 = 类名**；**普通类 / 接口不受此限**（一个文件可装任意多个，文件名随便叫）
- 同一个 GameObject 可以挂**多个相同组件**（要禁止得在类上加 `[DisallowMultipleComponent]`）——排查"改了脚本没反应"时先看一眼是不是挂了两个

### 三、生命周期回调（引擎按名字点名）

```
Awake → OnEnable → Start → (每帧: Update → LateUpdate) → OnDisable → OnDestroy
```

| 回调 | 触发时机 | 次数 |
|---|---|---|
| `Awake` | 实例被加载（物体激活时） | **一生一次** |
| `OnEnable` | 物体**且**组件都启用时 | **每次进出都触发** |
| `Start` | 第一次启用后、第一帧 `Update` 前 | **一生一次** |
| `Update` | 每帧 | 每帧 |
| `LateUpdate` | 本帧所有 `Update` 跑完后 | 每帧 |
| `OnDisable` | 物体**或**组件被禁用 / 销毁前 | **每次进出都触发** |
| `OnDestroy` | 实例被销毁 | 一生一次 |

**三条关键结论**：
1. `Awake` → `OnEnable` → `Start`；**`Awake`/`Start` 的"一次"指"这个实例的一生"**，重新启用**不会**重跑
2. **`Update` 频率不是固定 60**：编辑器空场景可跑上千帧/秒（没开垂直同步）→ **任何速度/时间逻辑必须乘 `Time.deltaTime`**（帧率无关性铁律）
3. **`LateUpdate` 存在的理由**：多个组件之间 `Update` 的**执行顺序不保证** → 相机跟随必须放 `LateUpdate`，才能拿到所有物体本帧的**最终位置**（否则画面抖动）

### 四、⭐ 按名字点名 vs vtable（两种调用机制对照）

| | `virtual` / `override` | Unity 生命周期（消息机制） |
|---|---|---|
| 谁决定被调用 | **编译器**（编译期焊进 vtable） | **引擎**（运行时扫描方法名，结果缓存成"消息表"） |
| 定位方式 | 类型指针 → 虚方法表 → 跳转 | 按**名字**匹配 |
| 写错了 | **编译报错** | **不报错，静默失效**（`void update()` 永远不跑） |
| 访问修饰符 | 只能 `public`/`protected` | **随便**（`private` 也能被调） |
| 类比 | 填表格（格子印好了，每格都要处理） | 喊名字（答应哪个干哪个，没人应也没人管） |

**Unity 为什么选"点名"**：① **`private` 也能被调用**（`void Start()` 就是隐式私有，这在 virtual 体系里做不到）；② **不依赖继承体系**（ScriptableObject、编辑器脚本也能接回调）；③ 多语言兼容（早期支持 JS/Boo）；④ 想用哪个写哪个，新增回调不用改基类。
**代价**：没有编译期保护 → **生命周期方法永远用 IDE 补全，绝不手打**。

> 注意：`virtual` 只是"**可以**改写"，不是"必须"（必须的是 `abstract`）。`Monster` 没 override 就用父类默认实现 ✓

### 五、组件禁用 vs 物体禁用（两个长得几乎一样的 checkbox）

| | 组件勾选框（`enabled`） | 物体勾选框（`activeSelf` / `SetActive`） |
|---|---|---|
| 影响范围 | **只有这一个组件** | **物体上所有组件** |
| 子物体 | 不受影响 | **全部一起失活** |
| 层级窗口 | 物体**不变灰** | 物体名**变灰** |

**关键洞察（实测得出）**：**从脚本自身看，两种情况收到的回调一模一样（`OnDisable` / `OnEnable`）**——脚本根本分不清"我被禁用了"还是"我所在的物体被禁用了"。唯一区别在**影响范围**。
（实锤实验：一个物体挂两个相同组件，勾选的跑完整流程，未勾选的一条日志都没有。）

### 六、场景心智模型

- **你运行的不是"某个脚本"，而是"整个场景"**：播放时场景里**所有物体同时活着、同时运行**（所以会莫名多出别的脚本的日志）
- 场景会"记住"你放进去的一切（`Ctrl+S` 保存 `.unity` 文件）；**代码保存（VS 的 `Ctrl+S`）和场景保存是两件独立的事**

### 七、两个必踩的工程坑

| 坑 | 现象 | 原因 / 修法 |
|---|---|---|
| **编码** | VS 弹"某些 Unicode 字符未能保存到当前代码页" | 脚本里有中文/emoji，VS 默认 ANSI/GBK 存不下 → **存 UTF-8**（Unity 按 UTF-8 解析 `.cs`）；VS2022 里用 `文件 → 高级保存选项 → Unicode (UTF-8 带签名)`（详见第 3 章第十一节） |
| **粘贴括号层级** | `CS1513: } expected` + `CS1022: Type or namespace definition...` | 粘贴时**忘闭合方法体** → 后面的类型声明被"吸"进方法里。**看见括号类报错先数括号**（VS 光标放 `{` 上可高亮配对，`Ctrl+]` 跳转），别盯代码内容 |

### 八、接口回收：`IEnumerable<T>` / `IList<T>` / `IDictionary<K,V>` / `KeyValuePair<K,V>`

**和自写的 `IAttackable` 是同一种东西**，区别只在"谁写的 + **消费者是谁**"：

| 接口 | 契约内容 | **消费者（谁在认它）** | 使用痕迹 |
|---|---|---|---|
| `IAttackable` | `Name` / `IsAlive` / `TakeDamage` | 自写的 `Character.Attack()` | 09-14 |
| `IEnumerable<T>` | "我能被逐个遍历" | **`foreach` 关键字本身** | 从第一天起每条 `foreach` |
| `IList<T>` | "能按下标访问 + Add + Count" | `[]` 索引器、`Add` | 118 杨辉三角（`IList<IList<int>>`） |
| `IReadOnlyList<T>` | "能下标、不能改" | 只读访问 | 09-08 Encapsulation |
| `IDictionary<K,V>` | "能用 key 查 value" | `dict[key]`、`TryGetValue` | 09-14 起 |
| `KeyValuePair<K,V>` | "一对 (key, value)" | 遍历 Dictionary 时 | ❌ 还没用过 |

**继承层次（越往下能力越多）**：

```
IEnumerable<T>            ← 能被 foreach（最底层能力）
├── ICollection<T>        ← 能 Count / Add / Remove
│   ├── IList<T>          ← 能按下标 list[0]
│   └── IDictionary<K,V>  ← 能用 key 查 value
└── IReadOnlyList<T>      ← 只读版：能下标、不能改
```

- **铁律：接口必须有"另一端"（消费者）才有意义** —— 没有消费者的接口是废纸。判断任何接口先问：**谁在认这份契约？**
- 预告：`IComparable<T>`（消费者 `List.Sort()`）、`IDisposable`（消费者 `using`）、`IEquatable<T>`（消费者 `Dictionary`/`HashSet` 判重）—— 全是同一套逻辑
- **选用原则：自己内部用 → 选实现（`List<T>`）；给别人用 → 选接口（`IReadOnlyList<T>` / `IEnumerable<T>`）**

### 九、`using` 的机制（易误解点）

- `using X;` **不是"引入"什么**，它只是把 X 命名空间里的类型名加进**检索范围**（短名 → 全名的查找表）
- 类型的**全名**永远是 `命名空间.类型名`（如 `UnityEngine.Debug`）；`using` 只让你能写短名。联想：`using` ≈ 把目录加进 `PATH`
- `System.Collections.Generic` 读作**路径**：`System` → `Collections`（老式非泛型 `ArrayList`/`Hashtable`，存 `object` 要装箱）→ `Generic`（泛型版，类型安全 + 免装箱）
- 两个 `Debug` 会打架：`UnityEngine.Debug`（控制台输出）vs `System.Diagnostics.Debug`（诊断）—— 同时 using 会报 **CS0104 不明确的引用**，届时只能写全名
- **万能判定法：删掉这个 `using`，编译一次，看哪儿报红。** 不报红 = 没用（VS 也会显示成灰色）

## 2. Unity 空间与运动（09-17）

### 三件套的关系

```
GameObject  = 空容器（本身什么都不会）
Component   = 装在容器里的功能件（行为 / 数据 / 外观）
Transform   = 唯一【强制】自带的组件（删不掉）
```

- **Unity 里没有"物体的行为"，只有"组件的行为"**：想移动 → 挂脚本组件；想碰撞 → Collider；想被看见 → 渲染组件
- **`transform` 不是全局变量**，是**继承自 `Component` 的属性**（≡ `this.transform`）
  - `: MonoBehaviour` 的真正价值 = 白送一批快捷访问（`transform`/`gameObject`/`name`/`enabled`）+ 生命周期方法的"报名资格"
- **Scene 窗口 = 上帝视角**（右键环视、右键+WASD 飞行、滚轮缩放、选中按 `F` 聚焦）｜**Game 窗口 = 相机视角**

### ⭐ 运动母公式

```csharp
transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
//                              └──── 这一帧应该走的距离 ────┘
```

- **`Update` 是"每帧一次"，不是"每秒一次"** → 必须 `× Time.deltaTime` 换算成"每秒"
- **去掉 `deltaTime`** → "每秒移动"变"每帧移动" → 实测一秒上千帧 → **速度放大 ~1000 倍**
- **铁律**：凡是"每秒多少"的（移动/旋转/冷却/计时）一律 `× Time.deltaTime`；"每帧一次"的（读输入）不乘
- 数学本质 = **数值积分（欧拉法）**：时间切片，每片当匀速累加
- "10 秒后 ≈ 10 而非精确 10"的两个原因：①**`deltaTime` 每帧在波动**（主因）②浮点累加误差（次因）
- **两条互斥的移动路线**：直接改 `transform.position`（运动学，自己算，会穿墙）｜ 操作 `Rigidbody`（物理引擎算，有碰撞重力）
  - ⚠️ **千万别混用**（加了 `Rigidbody` 还在 `Update` 里手改 `position` → 抖动/穿模/弹飞）；两套的正确姿势见第 3 章第六节

### ⚠️ 世界坐标 vs 本地坐标（**最容易搞反的一处**）

| | 代码 API | **Inspector 的字段** |
|---|---|---|
| 世界坐标 | **`transform.position`** ✅ | ❌ **看不到** |
| 本地坐标 | `transform.localPosition` | ✅ **「位置」显示的就是它** |

**Unity 官方手册原文**（Unity 6.0 与 4.3 两版一致）：

> *"The Transform values for any child GameObject are displayed **relative to the parent GameObject's Transform values**. These values are called **local coordinates**."*
> *"Unity measures the Position, Rotation and Scale values of a Transform **relative to the Transform's parent**. If the Transform has no parent, Unity measures the properties in world space."*

**结论**：
- **Inspector 的「位置 / 旋转 / 缩放」全部是本地坐标**（有父物体时）；Normal 和 Debug **两个模式显示的都是本地值**，Debug 只是把序列化字段名（`局部位置`）直接暴露出来
- **世界坐标没有任何面板字段** → **只能在代码里取**
- 在面板里改「位置」等价于 `transform.localPosition = ...`（**不是** `transform.position = ...`）——**两套量搞混是父子结构下最经典的 bug 来源**
- **为什么这样设计**：调子物体时关心的是"相对父物体的偏移"（父动子动是常态）
- **父子关系**：`世界坐标 = 父的世界坐标 + 自己的本地坐标`（无旋转缩放时）；拖拽成子物体时 Unity **保持世界位置不变**，自动改本地坐标
- **父子关系的价值**：角色跑动时手里的剑**不需要写任何跟随代码** —— 剑的 `localPosition` 一直是 `(0.5, 0, 0)`，世界坐标被"继承"着算出来

### CS1612：属性返回 struct → 只能"整体读写"

```csharp
transform.position += v;        // ✅ 展开成 transform.position = transform.position + v（整体赋值）
transform.position.x = 5f;      // ❌ CS1612：无法修改"Transform.position"的返回值，因为它不是变量
```

**根因链**：`position` 是**属性**（执行代码，**返回一份拷贝**）→ `Vector3` 是 **struct** → 属性返回 struct = 返回拷贝 → 拷贝没有名字、没有存储位置 → **不是变量** → 不能出现在赋值号左边

| 写法 | 流程 | 合法 |
|---|---|---|
| `transform.position += v;` | 读 → 算新的 → **整体写回** | ✅ |
| `transform.position.x = 5f;` | 读 → 改拷贝 → **丢掉** | ❌ |

**三种正解**：
1. `Vector3 p = transform.position; p.x = 5f; transform.position = p;`（局部变量是变量，合法）
2. `transform.position = new Vector3(5f, p.y, p.z);`（整体赋值）
3. **优先用 Unity 封装的方法**：`transform.Translate(...)` / `transform.Rotate(...)` —— **这类方法存在的意义就是帮你做"读-改-写回"**

**类推**：
| 写法 | 合法 | 原因 |
|---|---|---|
| `list[0].x = 5f`（`List<Vector3>`） | ❌ | 索引器是**属性**（返回拷贝） |
| `arr[0].x = 5f`（`Vector3[]`） | ✅ | **数组下标是语言内建的"变量"** |

**为什么编译器要拦**：它能确定这个操作毫无意义。**宁可编译报错，也不要静默 bug。**

> 与 Unity 生命周期对照：**编译器能管的错 → 报错**（`virtual`/CS1612）；**编译器管不了的（引擎反射）→ 静默失效**（`void update()`）。**凡是能靠编译器抓的，绝不留到运行时。**

### 字段 vs 属性（判定三步法）

| 长相 | 是什么 |
|---|---|
| 类型 + 名字 + `;` | **字段** |
| `{ get; set; }` / `{ get {…} set {…} }` / `=> 表达式` | **属性** |
| `this[int] { get; set; }` | **索引器**（本质是属性） |

1. **看写法**（有花括号 → 属性）
2. **看命名惯例**（字段 `_camelCase`、属性 `PascalCase`）——⚠️ **只能猜，不能定**
3. **悬停 / `F12` 转到定义** —— ⭐ **唯一权威判定**

**四个实际后果**：
| # | 差异 | 字段 | 属性 |
|---|---|---|---|
| 1 | 能否"只改一个分量" | ✅（是变量） | ❌ CS1612 |
| 2 | 能否当 `out`/`ref` 参数 | ✅ | ❌ CS0206 |
| 3 | 读取代价 | 近乎零 | **要执行代码**（`transform.position` 要跨到引擎内部取数 → **读一次缓存到局部变量**） |
| 4 | 结果是否稳定 | 一定稳定 | 可能每次不同（计算属性） |

**记忆锚点**：**字段 = 抽屉**（直接拿/放）｜**属性 = 前台**（跑一套流程再给你；**没法隔着前台动抽屉里的某一格**）

## 3. Unity 输入与物理（09-18）

### 一、读输入：轮询（拉），不是消息（推）
```csharp
Input.GetKey(KeyCode.A)        // 按住期间每帧为真（状态）
Input.GetKeyDown(KeyCode.A)    // 按下那一帧为真（事件）
Input.GetKeyUp(KeyCode.A)      // 松开那一帧为真（事件）
Input.GetAxisRaw("Horizontal") // -1 / 0 / +1，瞬时
Input.GetAxis("Horizontal")    // -1 ~ +1，平滑过渡
```
- 与生命周期对照：生命周期是**引擎按名字点名（推）**，输入是**你每帧主动去问（拉）**

### 二、⭐ GetAxis vs GetAxisRaw（"松手后滑行"的根因）
| API | 松手瞬间 | 适合 |
|---|---|---|
| `GetAxis` | **平滑回落**：按该轴 `Gravity` 参数衰减（默认 3 → 约 0.33 秒归零） | 要缓动手感（赛车/转向） |
| `GetAxisRaw` | **立刻归零** | 要"指哪打哪"（挡板） |

- **滑行 ≠ 物理惯性**：Kinematic 刚体没有质量/摩擦/惯性，位置完全由代码指定 → 滑行只来自**输入值的平滑**
- 手感可调：`项目设置 → 输入管理器 → 轴` 里改 `Gravity`（回落速度）/ `Sensitivity`（上升速度）→ **参数化而不是写死**
- 排查心法：先分清"**输入平滑**"和"**刚体阻尼**"两个完全不同的成因，方向错了会白调半天

### 三、轴名表 = 数据驱动的输入配置
- `"Horizontal"` 是**名字**，Unity 拿它去 `ProjectSettings/InputManager.asset` 这张表里查（拼错/大小写错 → `Input Axis xxx is not setup` 异常）
- 默认 `Horizontal` = 主键 `left`/`right` + 备用键 `a`/`d`（`Alt Negative/Positive Button`）
- 意义：**代码只说"我要水平轴"，具体是哪些键由表决定** → 换键位不改代码，还能同时吃键盘与手柄
- 表属于 `ProjectSettings/` → **会进 git**，换机器克隆下来键位设置跟着走

### 四、⭐ 双时钟：Update vs FixedUpdate
| | `Update` | `FixedUpdate` |
|---|---|---|
| 频率 | 每帧一次（跟帧率，几十 ~ 上千次/秒） | 固定物理步长，默认 0.02s = **50Hz** |
| 谁驱动 | 渲染循环 | 物理引擎 |
| 一帧内跑几次 | 必然 1 次 | **0 / 1 / 多次** |
| 放什么 | 读输入、相机（`LateUpdate`）、纯逻辑/UI | **所有物理操作**（力、速度、`MovePosition`） |
| 配套时间量 | `Time.deltaTime` | `Time.fixedDeltaTime` |

- 为什么输入在 `Update` 读：`GetKeyDown` 只在"按下那一帧"为真，而 `FixedUpdate` 一帧可能 0 次或多次 → **会漏按键 / 重复触发**
- 代价：`Update` 写的值最快要到下一次 `FixedUpdate` 才被用掉（最多一帧延迟，对挡板无感）
- "50Hz"是**目标频率/长期平均值**，不是"每秒恰好 50 次"：卡顿时会**连跑多次追赶**，超时（`最大允许时间步长` 默认 0.3333s）则**主动丢步**防"死亡螺旋"
- ⚠️ `FixedUpdate` 拼错 = 永不执行且不报错（生命周期"按名字点名"同款坑）

### 五、Rigidbody2D：三种主体类型（Body Type）
| 类型 | 谁控制位置 | 典型用途 |
|---|---|---|
| **Dynamic** 动态 | 物理引擎（重力/受力/被撞飞） | 球 |
| **Kinematic** 运动学 | **代码**（`MovePosition`），但仍参与碰撞、能顶飞别人 | 挡板 |
| **Static** 静态 | 谁都不动 | 地板、墙 |

- 改成 Kinematic 后，`质量 / 线性阻尼 / 重力大小` 这些参数失效（谁在管位置，一目了然）
- **`Collider2D` 决定"能不能被撞"，`Rigidbody2D` 决定"位置归不归物理引擎管"** —— 两件事分开的。砖块只要前者（没刚体的碰撞体 = 静态碰撞体，最省）

### 六、⭐ 移动的两种语义（09-18 的分水岭）
```csharp
transform.position += v;    // 直接改坐标（瞬移）：绕过物理引擎 → 两套账打架（抖动/穿墙/碰撞失真）
_rb.MovePosition(next);     // 申请："这一步请把我搬到 next" → 引擎执行 + 同步 Transform + 顺便做碰撞检测
```
- `MovePosition` 要的是**目标位置**，不是增量 → 得自己算 `当前位置 + 位移`
- 基准用 `_rb.position`（**物理引擎账本里的位置**，Vector2），比 `transform.position` 更可靠

### 七、为什么 `_rb.position += v` 编译不过
- `Rigidbody2D.position` 是**只读属性**（只有 get）→ **CS0200**（不能赋值）
- 对照：`transform.position` 有 setter → `+=` 能过（编译器展开成"整体读-加-整体写回"）；`transform.position.x = 5f` 是 **CS1612**（改的是返回的拷贝）→ 同属本册第 2 章"字段 vs 属性"

### 八、取组件与暴露字段
- `_rb = GetComponent<Rigidbody2D>();`：去**自己所在的 GameObject** 上找组件（找不到返回 `null`）；`transform` 本质就是白送的 `GetComponent<Transform>()`
- 放 `Awake`：生命周期最早、只跑一次，保证后续方法用时已接好
- `[SerializeField] private float speed = 8f;` = **对检查器开窗、对代码关门**（`public` 会拆掉封装墙）；运行时才赋值的引用（`_rb`）**不要**加
- **检查器里拖进去的字段值，在 `Awake` 时已经可用**（Unity 在反序列化阶段就填好了）→ 所以 `speed` 读到的一直是你设的值

### 九、⚠️ 2D / 3D 是两套物理引擎
| | 3D | 2D |
|---|---|---|
| 刚体 | `Rigidbody` | `Rigidbody2D` |
| 碰撞体 | `BoxCollider` | `BoxCollider2D` / `CircleCollider2D` |
| 渲染 | `MeshRenderer` | `SpriteRenderer` |

- 混用**不报错、静默失效**（2D 物体挂 3D `Rigidbody` = 完全不参与 2D 物理）

### 十、活动输入处理（旧 / 新 / 两者）
- `项目设置 → 玩家 → 其他设置 → 活动输入处理`：Unity 6 新项目模板默认「**输入系统包（新）**」→ 旧的 `Input.*` 会**直接抛异常**
- 学习期选「**两者**」（改完需**重启编辑器**）：两套 API 都能用；新输入系统（Input Actions + 按键重映射）留到需要多平台/自定义键位时专门学
- 心法：**API 不生效 → 先查项目设置，再怀疑代码**

### 十一、编码坑：VS2022 存 GB2312 → CodeBuddy 乱码
- 症状：同一文件 VS2022 中文正常，CodeBuddy（VS Code 系，默认按 UTF-8 解码）全是乱码
- 根因：VS2022 按中文 Windows 的**系统代码页（GB2312 / 936）**保存，没存成 UTF-8
- 修法：VS2022 `文件 → 高级保存选项` → **`Unicode (UTF-8 带签名) - 代码页 65001`**
- 为什么"带签名"（BOM）：文件头写死"我是 UTF-8"（`EF BB BF`），任何编辑器都不用猜
- Unity 要求 `.cs` 用 UTF-8（GBK 字节被按 UTF-8 解码 → 注释乱码，甚至编译报错）
- 校验技巧：`UTF8Encoding($false, $true)` 严格解码能过 = 合法 UTF-8；再看头 3 字节有没有 BOM
- 编辑器日志：CodeBuddy 输出面板的 `[info]` 只是正常心跳（识别工作区/仓库），**只有 `[error]` 才要管**

### 十二、Unity 项目进 git（首次入库实测）
- 待跟踪文件 39 个（`Assets/` + `Packages/` + `ProjectSettings/`），`Library/ Temp/ Logs/ *.csproj *.sln` 全被 `.gitignore` 拦住
- 验证手段：`git status --short --untracked-files=all` + `Select-String 'Library/'`（应为空）；`git check-ignore -v <路径>`（能看到是哪一条规则拦下的）
- 必须提交的三样：`Assets/`（**含 `.meta`**）、`Packages/`、`ProjectSettings/`

## 4. Prefab、实例化与场景装配（09-19）

### 一、Prefab = 模具，场景里的对象 = 副本

- **Prefab 源文件**住在 `Assets/Prefabs/`（拖进去后，层级窗口里那个对象的名字**变蓝** + 右侧出现「覆盖 / Overrides」按钮）
- 改**模具**（双击 Prefab 进编辑）→ 所有副本里**未被覆盖**的字段一起变；改**某个副本** → 只在它身上生效（会记一笔"覆盖"）
- 为什么要 Prefab：一次配好（外观/碰撞体/脚本参数），后面靠代码批量复制 —— 手摆 32 块是不可维护的

### 二、`Instantiate` 在做什么

```csharp
Instantiate(prefab);                          // 用模板自己的位置
Instantiate(prefab, 位置, 旋转);                // 常用
Instantiate(prefab, 位置, 旋转, 父物体);        // + 顺手挂父物体（坐标按世界空间解释）
```

| 步骤 | 内部干了什么 |
|---|---|
| ① 反序列化 | 把 `Assets/` 里那个 Prefab 文件（YAML/二进制）读出来 |
| ② 装配 | 按数据建出 GameObject + 各组件，把 `[SerializeField]` 的值填好，修好引用 |
| ③ 入场景 | 加进当前场景，触发 `Awake`/`OnEnable`（`Start` 等下一帧） |

**与 C# `new` 的分水岭**：

| | `new GameObject()` | `Instantiate(prefab)` |
|---|---|---|
| 造出什么 | **空壳**，一个组件都没有 | **一整套**（渲染/碰撞/脚本 + 数值全在） |
| 数据来源 | 从零构造 | 从磁盘上的模板**拷贝** |
| 成本 | 便宜 | **贵**（读文件 + 堆分配）→ 别放 `Update`，这就是对象池（09-22）的动机 |

**三条实操铁律**：
1. **返回值必须接住**（`GameObject b = Instantiate(...)`）—— 不接住对象照样存在，但你**再也拿不到它的引用**，以后想回收只能满世界 `Find`
2. **`Quaternion.identity` = 不旋转**（identity ≈ 数字里的 1）；注意第三格是**覆盖**：传 `identity` 会抹掉模板自带旋转 —— 想跟模板一致就传 `prefab.transform.rotation`
3. 副本名字带 **`(Clone)`** 后缀 —— 播放后看到一堆 `Brick(Clone)` 就说明复制成功

### 三、生成器（Spawner）该挂谁 + 容器要"干净变换"

| 候选 | 评价 |
|---|---|
| **场景级空物体**（`Board` / `GameManager`） | ✅ 职责匹配：管整局游戏，不参与画面 |
| 相机 | ⚠️ 能跑，但语义混乱（相机是"眼睛"） |
| 挡板 / 砖块自己 | ❌ 会移动、会连累逻辑；**挂在自己生成的东西上 = 指数爆炸**（见第六节事故 2） |

**容器（Board）保持 `位置(0,0,0)` + `旋转(0,0,0)` + `缩放(1,1,1)`** —— 这样子的**本地坐标 = 世界坐标**，所有算坐标的代码都不会错。要整体挪动只改它的**位置**。

### 四、`SetParent`：父子变换对子物体的三个影响

```csharp
brick.transform.SetParent(transform);   // 把生成的砖块挂到 Board 底下（默认保持世界位置不变）
```

| 改父物体的… | 子物体（砖块）会怎样 |
|---|---|
| **位置** | 整体**平移**，形状与间距比例不变 ✅ 安全 |
| **旋转** | 整片砖墙**跟着转** |
| **缩放** | 位置与尺寸**一起被乘** ⚠️ 形状可能变形 |

缩放的具体后果（`Board` 的 `scale.x` 改成 2）：砖块世界尺寸 `1.6×0.6 → 3.2×0.6`（**变宽变扁**）、相邻间距 `1.7 → 3.4`（阵列以父的本地原点为中心撑开）、`BoxCollider2D` 跟着放大。而**检查器里显示的是本地值**（还是 1.6/0.6）→ 从数据上看不出，只在 Scene 视图里看得出来。

**隐藏的连带坑**：`transform.localScale.x * 0.5f` 这种算"半宽"的写法只在**没有父物体 / 父缩放为 1** 时成立；一旦被缩放的父物体包住，要改用 `lossyScale`（世界缩放）或更稳的 `SpriteRenderer.bounds.extents`。

**挂成子物体后的坐标语义**：子物体的检查器「位置」变成**相对父的本地坐标**；`transform.position` 仍是世界坐标 —— 父在原点且无缩放旋转时两者数值相同，父一动就分家（第 2 章那个坑的现场版）。

### 五、正交相机的世界单位换算 + `Mathf` 速查

```
屏幕【全高】= orthographicSize × 2
屏幕【全宽】= 全高 × aspect
屏幕【半宽】= orthographicSize × aspect      ← 布局/边界最常用
```

- `Camera.main` = 标签为 `MainCamera` 的相机（**可能为 null**：场景里没有主相机时）；内部是按标签查找，**建议 `Awake` 里取一次存字段**，别每帧调
- 相机是**透视**时 `orthographicSize` 无意义（2D 模板默认正交）

| `Mathf` API | 作用 |
|---|---|
| `Clamp(v, min, max)` | 夹进区间 |
| `Clamp01(v)` | 夹到 0~1（进度条、插值 t） |
| `Abs / Max / Min` | 绝对值 / 取大 / 取小 |
| `Lerp(a, b, t)` | 线性插值 |
| `Approximately(a, b)` | 浮点近似相等（"别用 `==` 比浮点"的正解） |

### 六、⭐ 两起事故复盘（09-19）

**事故 1：字段初始化器里访问 `transform` → 报错**

```csharp
// ❌ 写在类成员位置（字段初始化器）
float halfPaddle = transform.localScale.x * 0.5f;
```
> 报错大意：`get_transform is not allowed to be called from a MonoBehaviour constructor (or instance field initializer), call it in Awake or Start instead.`

**生命周期顺序（关键）**：
```
C# 构造函数（字段初始化器在这里跑）  ← Unity 的"原生物体"还没和 C# 对象绑定
      ↓
    Awake  ← 从这里开始 transform / gameObject / GetComponent / Camera.main 才能用
      ↓
  OnEnable → Start → Update…
```
**修法**：需要引擎信息的计算全部搬进 `Awake`；只把**要跨方法用的结果**升成字段（如 `_limit`），中间量用局部变量。

**事故 2：生成器自引用 → 指数爆炸 → 编辑器假死**

- 症状：一进播放就卡死，任务管理器里 Unity 无响应
- 根因（从场景文件里挖出的证据）：`BrickSpawner` 挂在**场景里的 Brick** 上，而它的 `brickPrefab` 又指向**它自己**
  ```yaml
  # 场景文件里的证据
  --- !u!114 &1906376736                  # BrickSpawner 组件
    m_GameObject: {fileID: 1906376735}    # 挂在 Brick 上
    brickPrefab: {fileID: 1906376735}     # ← 指向自己！
  ```
  → `Instantiate(self)` 出来的每个副本**也带着生成器** → 32 → 32² → 32³ … 几秒几十万个物体 💥
- **正确姿势**：生成器挂**场景级空物体**；槽位拖的是 **Project 窗口里的 Prefab 资产**（引用里带 `guid`），**不是层级窗口里的对象**（引用里只有 `fileID`）—— 这是"我拖对了吗"的硬核判别法
- **救场流程**：① 任务管理器强杀 `Unity.exe`（播放模式的改动不会保存，场景不会坏）② 重启后若弹 **Recovering Scene Backups** 选「是」→ 备份进 `Assets/_Recovery/`（强杀前未保存的编辑只在这里）③ 打开恢复场景检查、修好、**`Ctrl+S` 存场景** ④ 确认无用后删掉 `_Recovery/`（别提交进 git）

### 七、防御性编程：给自己装保险丝

```csharp
if (cols <= 0 || rows <= 0) return;
if (cols * rows > 200)
{
    Debug.LogError($"砖块数量异常：{cols} × {rows}，已拒绝生成");
    return;
}
Debug.Log($"准备生成 {cols * rows} 块砖");   // 先打印"打算做多少"，出问题时日志能说明意图
```
原则：**对"外部输入"（检查器里手输的值、别人拖进来的引用）永远先做合法性检查，再动手**。

### 八、场景保存节律（血泪教训）

> **播放模式的改动不会保存，但"你拖进槽位的引用"属于场景数据 —— 没按 `Ctrl+S` 就等于没做。**

节律：**加物体 / 拖引用 → `Ctrl+S` 存场景 → 再播放**。

---

## 📎 环境与工具备忘（Unity 部分）

- 编辑器：**Unity 6.3 LTS（6000.3.23f1）**，**中文界面**（菜单名一律用中文：层级 / 项目 / 检查器 / 控制台 / 创建 / 播放）
- 项目位置：练习项目在 `D:\Unity\Projects\`；**成品**在仓库 `Projects/Unity/<项目名>/`（配 Unity 专用 `.gitignore`）
- 外部脚本编辑器：**Visual Studio 2022**（Unity 集成最好：双击脚本跳转、附加进程断点调试）；CodeBuddy VS 2022 插件已装
- 三条硬约束：**脚本文件名 = 类名** ｜ 改代码必 `Ctrl+S` ｜ **播放模式的改动不保存**
- 场景保存：加物体 / 拖引用后必 `Ctrl+S`（否则崩溃即丢失）
- 生命周期方法一律**用 IDE 补全**（拼错静默失效）
- 编码：`.cs` 一律 **UTF-8（带 BOM）**
- 完整环境信息见仓库根目录 `HANDOVER.md`（不上传）

## 📌 回访清单（学到对应内容时回来重构）

- [x] ~~学完 **Unity** 后：把 `CharacterBattle` 的类结构搬到 Unity 角色系统~~（**09-16 完成 ✓**：`CharacterClasses.cs` + `BattleDemo.cs` 跑通全流程，4 个环境坑已踩）
- [ ] 学完 **prefab / 对象池**后：`Battle.Run` 里 `new Hero(...)` 的写法换成 Unity 的 `Instantiate`（09-22 打砖块时自然撞上）
- [ ] **`TransformProbe` 未跑（09-17 记）**：用脚本打印 `transform.position` / `transform.localPosition`，亲手对照 Inspector 的「位置」到底绑的是哪个（**顺延到 09-20 复盘日**）
- [ ] **实验 5 未做（09-17 记）**：同一物体挂两个反向 `Mover` —— 体会"组件是各自独立的动力源"（09-20 复盘日）
- [ ] **世界/本地坐标的代码实践（09-17 记）**：写代码让子物体"绕父物体转"，体会 `localPosition` 相对父物体的语义（打砖块前补齐）
- [ ] **手感实验未做（09-18 记）**：把 `Horizontal` 的 `Gravity` 改成 100（或换回 `GetAxis`）对比挡板手感，顺手看 `Sensitivity`
- [ ] **`Update` / `FixedUpdate` 顺序实验（09-18 记）**：两个方法里各打 `Time.time` + `Time.frameCount`，亲眼看次数与顺序（验证"物理步在渲染前"）
- [x] ~~**边界 Clamp 未做（09-18 记）**~~（**09-19 完成 ✓**：`_limit = orthographicSize × aspect − localScale.x × 0.5f`，`next.x = Mathf.Clamp(...)`）
- [ ] **挡板 Body Type 对照实验（09-18 记）**：换成 Dynamic + 冻结 Y/旋转，体会"能被球撞飞"的差别（09-21 学碰撞时做）
- [ ] **学完新输入系统（Input Actions）后**：把挡板/角色输入改成 `InputAction` 版本，再做一次按键重映射（Block 3 平台跳跃时安排）
- [ ] **Prefab 同步实验（09-19 记）**：改 Prefab 源文件的颜色/缩放 → 播放，看所有副本是否同步（体会"模具 vs 副本"）
- [ ] **对象池回收砖块（09-19 记）**：`_bricks` 这个 `List` 先留着 —— 09-22 用它做回收统计
- [ ] **4 参 `Instantiate` 的坐标语义（09-19 记）**：把 `Board` 挪到非原点后，验证第 2 格的坐标是按世界还是本地解释
- [ ] **`lossyScale` 隐患（09-19 记）**：把挡板挂到一个被缩放的父物体下，看 `localScale` 算半宽会错成什么样
- [ ] **`Camera.main` 缓存（09-19 记）**：把 `Camera.main` 从 `Awake` 里存成字段，体会"别每帧查找"
