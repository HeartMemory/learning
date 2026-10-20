using System;                      // 🆕 10-20：OnJumped / OnLanded 用的 Action
using UnityEngine;
using UnityEngine.InputSystem;

// ══════════════════════════════════════════════════════════════
//  PlayerController —— 3D 跑酷雏形（Runner · 第 8 号成品）
//  第 1 步（10-14）：前后左右跑 + 按住 Shift 冲刺 ⇒ 闸门①「3D 能跑」
//  第 2 步（10-16）：跳跃 + 重力 + 地面检测（3D 射线）⇒ 闸门②「能跑能跳」
//
//  ✅ 今天已定的五条判据（它们决定方法体怎么写，落笔前先读一遍）：
//    ① 【两个时钟分工】输入在 Update 读、物理在 FixedUpdate 写
//       ⇒ 两者之间只用一个"桥变量"（下面那个 _input）传值，
//         绝不在 Update 里直接改速度
//    ② 【归一化】键盘的 (1,1) 长度是 √2 而不是 1 ⇒ 斜跑会快 41%
//       ⇒ 判"要不要归一化"用 **sqrMagnitude**（免费那一档），不用 magnitude
//    ③ 【Unity 6 改名】`Rigidbody.velocity` 已改名 `linearVelocity`
//       （旧名是遗留写法；本工程 Unity 6.3 LTS）
//    ④ 【别碰 y】只写水平两轴；竖直分量 y 归重力管 ——
//       把 y 也覆盖成 0，角色就"贴地不下落 / 永远飘着"
//    ⑤ 【冲刺 = 倍率，且归一化只作用于"方向"】
//       · `按住 Shift` 是【电平】语义（与 WASD 同类，**不是边沿** ⇒ 不需要意图标志位）
//       · ⚠️ **"速度的大小"只允许在一个地方被决定** —— 归一化只准出现在"方向"那一档；
//         写成"先乘速度再归一化"，9.6 会被抹回 1（等于白干）
//
//  ⭐ 第 2 步（10-16）新增的四条判据（落笔前先读，它们决定新代码怎么写）：
//    ⑩ 【接地 = 主动问，不是等通知】2D 那套"真接触 + 法线朝上"靠的是**碰撞回调**（事后通知）；
//       3D 主流是**主动发探针**：`Physics.CheckSphere`（有体积、不怕边缘）
//       + `Physics.Raycast`（拿**真实表面法线**判坡度）。
//       官方两条坑（原文，已查证）：
//         · `Raycasts will not detect Colliders for which the Raycast origin is inside the Collider.`
//           ⇒ **起点落在碰撞体内部就静默不命中** ⇒ 起点必须抬离地面一点点
//         · `SphereCast` 的 `hit.normal` **不总是表面法线**（常是"接触点→球心"方向）
//           ⇒ **要真法线就用 `Raycast`**（这也是"两个探针合起来用"的理由：一个管"有没有"，一个管"斜不斜"）
//    ⑪ 【3D 没有 `gravityScale`】（已用反射查死：`hasGravityScale=False`）——
//       2D 那套 `gravityScale = 2` 在 3D 无对应物；3D 改重力只有两条路：
//       `Physics.gravity`（**全局**，影响所有刚体）或自己施加额外加速度。
//       ⇒ 今天**先不动重力**，改"起跳初速度"来定高度：`v = √(2·g·h)`（g = 9.81）
//    ⑫ 【边沿 vs 电平 —— 与冲刺正好相反】跳跃是**边沿事件**（`WasPressedThisFrame`）
//       ⇒ 必须过一个**意图标志**（`_jumpRequested`）才能进物理步；
//       冲刺是**电平**（`IsPressed`）⇒ 不要标志。
//       ⚠️ 这个标志的纪律：**谁写、谁消费、谁清**，三者必须说清（否则丢跳 —— 承 Platformer 10-02/10-06）
//    ⑬ 【用物理侧的坐标，不拿渲染插值出来的】探针起点优先 `_col.bounds`（跟物理侧走），
//       不要用 `transform.position`（刚体开了 `Interpolate` ⇒ 它是**插值出来的中间值**，
//       🪞 与 10-15 那条"起点为什么必须用 `_rb.rotation`"是同一个病）
//
//  🪞 镜子（同族写法都在仓库里，卡住就去对照，别凭空想）：
//     · 输入怎么接 → `Platformer/Assets/Scripts/PlayerController.cs` **第 77 行**：
//         `InputSystem.actions.FindActionMap("Player").FindAction("Move")`
//     · "Update 写、FixedUpdate 读"的桥 → 同文件里的 `_axis`
//     · 装配期自检的写法（bool 收集 → LogError → enabled = false）
//         → 同文件的 Awake，或 `PlayerAudio.cs` 的 Awake
//
//  🎯 验收（收工当场拍板）：
//     ① Play 里 WASD 前后左右都能动
//     ② ⭐ 斜跑不快：斜跑时的水平速度模长 ≈ 直跑（都 ≈ moveSpeed）
//     ③ 控制台 0 红字
//     ④ ⭐ 四种读数的水平速度模长：W = 6 ｜ W+A = 6 ｜ W+Shift = 9.6 ｜ W+A+Shift = 9.6
//        （最后一格是探针：若读到 ≈ 13.6，就是"归一化 × 倍率"双重放大 ⇒ 判据⑤ 犯了）
// ══════════════════════════════════════════════════════════════
public class PlayerController : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    //  🆕 10-20：只读出口 + 两个事件（给 PlayerAudio 用）
    //    ⭐ 判据：**数据只有本类能改，外面只能读或订** ——
    //       属性是只读的（没有 set），事件只 Invoke 不给外部触发
    //    · IsGrounded / MoveInput = 【状态】→ 走路音每步读一次（它要的是"现在还在走"）
    //    · OnJumped / OnLanded   = 【事件】→ 音效只在发生的那一瞬间响一次
    //    （同一份数据，两种消费方式；这条分界承 PlayerAudio 判据③）
    // ══════════════════════════════════════════════════════════════
    public bool IsGrounded => _isGrounded;
    public Vector3 MoveInput => _input;
    public event Action OnJumped;
    public event Action OnLanded;

    [Header("移动")]
    [SerializeField] private float moveSpeed = 6f;   // 米/秒；面值放检查器可调

    [Header("冲刺")]
    [SerializeField] private float sprintMultiplier = 1.6f;   // 倍率制：改一处即生效，且"冲刺必比走快"这条关系不会被破坏

    [Header("转身（10-15 改版：角速度制）")]
    [SerializeField] private float turnSpeed = 720f;   // 度 / 秒 —— RotateTowards 的步长 = 它 × fixedDeltaTime

    [Header("跳跃（10-16 新增）")]
    [SerializeField] private float jumpSpeed = 6.3f;             // 起跳初速度（米/秒）：v = √(2·g·h) ⇒ 6.3 ≈ 跳 2 米
    [SerializeField] private float groundCheckRadius = 0.28f;    // 脚底"球形探针"半径（≈ 胶囊半径 0.5 的 0.56 倍，够用）
    [SerializeField] private float groundCheckDistance = 0.15f;  // 脚底射线长度（"离地多近还算站着"）
    [SerializeField] private float groundCheckLift = 0.02f;      // ⚠️ 起点抬离胶囊底这一小段（否则起点在碰撞体内 ⇒ 官方：静默不命中）
    [SerializeField] private LayerMask groundLayer;              // 只问"地面层"（检查器里选 Ground）—— 空白 = 谁都问不到
    [SerializeField] private float maxSlopeAngle = 45f;          // 能站的最大坡度（法线阈值 = cos(它)）

    private Rigidbody _rb;
    private CapsuleCollider _col; // 探针起点要用它（胶囊底 = _col.bounds.min.y）—— 10-16
    private InputAction _move;    // Awake 查一次就缓存（与 Camera.main 同理：别每帧查）
    private InputAction _sprint;  // 同上：Player/Sprint（Button · 左 Shift）
    private InputAction _jump;    // 10-16：Player/Jump（Button · 空格）
    private Vector3 _input;       // ⭐ 跨时钟的桥：Update 写、FixedUpdate 读
    private bool _isGrounded;     // 本物理步的接地结论（FixedUpdate 里算一次，别每处重问）
    private bool _jumpRequested;  // ⭐ 第二个桥（边沿版）：Update 写、FixedUpdate 用掉并清
    private bool _wasGrounded;        // 🆕 10-20：上一步的接地结论（用来判"落地"这个边沿）
    private bool _groundInitialized;  // 🆕 10-20：第一步只记录、不触发（开局"站在地上"不是"刚落地"）
    [SerializeField] private CameraFollow cameraFollow;   // 10-15：移动输入的"参考系"（检查器里拖 Main Camera）

    private void Awake()
    {
        // TODO ① 装配期自检：`_rb` 没挂就"响亮地失败"
        //    判据问题：这条日志必须放在**哪一步之前**？
        //              （提示：日志放在使用它的那一行之后 = 等于没放）
        //    🪞 风格照镜子：bool 收集问题 → Debug.LogError → enabled = false
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();   // 10-16：探针起点要用（胶囊底）
        _move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
        _sprint = InputSystem.actions.FindActionMap("Player").FindAction("Sprint");
        _jump = InputSystem.actions.FindActionMap("Player").FindAction("Jump");   // 10-16：模板自带，不用改输入资产

        // TODO ⑦ 【视角参考系】10-15 改版：**拖引用**（检查器里把 Main Camera 拖进 `cameraFollow`）
        //    ⭐ 为什么不再写 `Camera.main`：
        //       · 它内部要"找出带 MainCamera 标签的对象"（一次查找）；拖引用 = **零查找**
        //       · 拖引用是**显式依赖**：谁提供、提供什么，一眼看得出（与 `target` 同一个理由）
        //    判据问题：下面那几行自检里，`cameraFollow` 的那一行该不该加？
        //              ⭐ 照镜子自己写（🪞 = 紧跟着的 `if (xxx == null) { LogError; isError = true; }`）

        bool isError = false;
        if(_rb == null) { Debug.LogError("PlayerController 缺 Rigidbody"); isError = true; }
        if(cameraFollow == null) { Debug.LogError("PlayerController 缺 CameraFollow"); isError = true; }
        if (_move == null) { Debug.LogError("PlayerController 缺 Player/Move"); isError = true; }
        if(_sprint == null) { Debug.LogError("PlayerController 缺 Player/Sprint"); isError = true; }
        if(_col == null) { Debug.LogError("PlayerController 缺 CapsuleCollider（探针起点要用）"); isError = true; }
        if(_jump == null) { Debug.LogError("PlayerController 缺 Player/Jump"); isError = true; }
        if (isError) { enabled = false; return; }
        // TODO ② 接项目级输入（查一次就缓存成 _move / _sprint）
        //    🪞 镜子：Platformer/PlayerController.cs 第 77 行
        //    · Move   → 本项目 Player/Move（Value · Vector2）
        //    · Sprint → 本项目 Player/Sprint（Button · 左 Shift；模板自带，**不用改资产**）
    }

    private void Update()
    {
        // 10-17：游戏结束后直接 return（读权威状态，不复制、不订阅）
        if (GameManager.Instance.IsGameOver) return;

        // TODO ③ 读 Move → 映射到世界坐标
        //    判据问题：Move 是 Vector2（x = 左右、y = 前后），世界是 Vector3 ——
        //              哪个分量该落到 z 上？为什么？（想清楚"屏幕里往前"是世界的哪个轴）
        Vector2 moveInput = _move.ReadValue<Vector2>();
        // TODO ④ 归一化（**只在需要时**才做）
        //    判据问题：为什么判据写 `sqrMagnitude > 1f` 而不是 `magnitude > 1f`？
        //              （今天底层原理②：哪两个操作里各藏了一次开根号？）
        //    ⚠️ 归一化只准落在**方向**上（判据⑤）—— 这一格里绝不能出现 moveSpeed / 倍率
        moveInput = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
        // TODO ⑧ 【视角参考系】把"屏幕上的方向"翻译成"世界的方向"（10-15 新增）
        //    ⭐ 为什么必须做：视角已归玩家直控（相机 yaw）⇒ 玩家直觉是"W = 我看的前方"，
        //       而不是"W = 世界 +Z"。不翻译 ⇒ 转了视角之后按 W 仍往老方向跑（画面与手感对不上）。
        //    判据问题 A：怎么把 `(x, 0, z)` 绕 **y 轴**转"相机当前的 yaw"？
        //               （提示：`Quaternion.Euler(0f, yaw, 0f) * 向量`；yaw 从 `cameraFollow.Yaw` 读）
        //    判据问题 B：为什么只用 **yaw**、不用相机的完整 `rotation`（含 pitch）？
        //               （提示：移动发生在**地面平面**上 ⇒ 俯仰不该影响"往哪走"）
        //               ⇔ 已落地的收口判据：**同一个状态只能有一个家** ——
        //                 `_yaw` 只在 `CameraFollow` 里存一份，这里通过 `public float Yaw => _yaw` 读
        //    ⚠️ 它跟上面"归一化"（TODO ④）谁先谁后？结果一样吗？
        //       （提示：绕 y 轴旋转**不改变向量长度** ⇒ 谁先谁后都行 —— 这本身就是一条判据）
        _input = new Vector3(moveInput.x, 0f, moveInput.y);
        // TODO ⑧-2 把 `_input` 旋转到"相对相机"，然后赋回 `_input`
        _input = Quaternion.Euler(0f, cameraFollow.Yaw, 0f) * _input;

        // ── ⑫【跳跃意图】10-16 新增：**边沿**语义，只在这一侧"置位" ──
        //    ⚠️ 为什么不在这里直接跳：`Update` 与 `FixedUpdate` 是两个时钟 ⇒
        //       这里只负责"把这一次按下**记下来**"，真正的物理动作留给 FixedUpdate。
        //    ⚠️ 为什么是 `WasPressedThisFrame` 而不是 `IsPressed`：
        //       跳跃是**一次事件**（按下那一刻算数）；按住不放不该连着起跳。
        //       🪞 反面镜子 = 冲刺（判据⑤）—— 同一个输入系统里两种语义并存，别搞混。
        if (_jump.WasPressedThisFrame()) _jumpRequested = true;

        // TODO ⑨ 【朝向】10-15 改版：**已挪到 `FixedUpdate`**（见那边那段）——
        //    为什么挪（今天上网查证的结论 → 笔记第 29 章）：
        //      · 朝向是**物理姿态**（角色身上带 Rigidbody）⇒ 官方明确"**不要用 `transform.rotation`
        //        去转一个受物理控制的刚体**"（会绕过物理引擎、破坏插值、姿态与物理不同步）
        //      · 更标准的接口 = `Rigidbody.MoveRotation`，且**要在 `FixedUpdate` 里**按 `fixedDeltaTime` 驱动
        //    ⭐ 保留一条通用判据：**先问"这个东西有没有物理权限"** ——
        //       视角（相机，纯表现）⇒ 随便写；角色朝向（带刚体）⇒ 必须走 `Rigidbody` 接口。
    }

    private void FixedUpdate()
    {
        // 10-17：游戏结束后直接 return（读权威状态，不复制、不订阅）
        if (GameManager.Instance.IsGameOver) return;

        // ── ⑩ 接地判定（10-16）：每个物理步只问一次，结论存进 `_isGrounded` ──
        _isGrounded = CheckGrounded();

        // ── 🆕 10-20【落地事件】：悬空 → 接地 的那一步 = "落地" ──
        //   ⭐ 3D 里没有 2D 那套碰撞回调能白拿，"落地"只能自己用【边沿】造出来
        //   ⚠️ `_groundInitialized` 这一道闸是必须的：
        //      开局第一步本来就在地上，`_wasGrounded` 的默认值是 false
        //      ⇒ 不挡的话，进入游戏的第一帧就会"咚"一声（把"已经在场"误报成"刚落地"）
        //   ⭐ 判据：**"状态成立" ≠ "状态刚刚变成立"** —— 缺了上一次的快照就分辨不出两者
        if (_groundInitialized && _isGrounded && !_wasGrounded) OnLanded?.Invoke();
        _wasGrounded = _isGrounded;
        _groundInitialized = true;

        // ── ⑫ 消费跳跃意图（10-16）：用掉就清，不管成功没成功 ──
        //    ⭐ 三件事的顺序不能换：**先算接地 → 再决定跳 → 最后清意图**
        //       · 清早了 ⇒ 这一次按键白丢（"丢跳"，而且控制台一声不响 —— 承 Platformer 10-06 那笔账）
        //       · 清晚了 ⇒ 同一个意图被两个物理步消费（连跳 / 双击跳）
        if (_jumpRequested && _isGrounded) Jump();
        _jumpRequested = false;

        // TODO ⑤ 把桥变量汇合成速度
        //    判据问题 A：竖直那一路该写什么？（回到判据④ —— 你只拥有水平两轴）
        //    判据问题 B：松键就停，是靠"每步都覆盖速度"实现的，还是靠摩擦？
        //              两种做法的体感差在哪？
        float speed = _sprint.IsPressed() ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 v = _rb.linearVelocity;
        v.x = _input.x * speed;
        v.z = _input.z * speed;
        _rb.linearVelocity = v;
        // 📌 【探针已摘除 · 10-15】闸门① 四读数（水平模长 6 / 6 / 9.6 / 9.6）已全绿 ⇒ 摘除。
        //    当时那一行留作备查：`Debug.Log($"水平模长 = {Mathf.Sqrt(v.x*v.x + v.z*v.z):F2}");`

        // ── TODO ⑨【朝向】角色面朝"移动方向"（10-15 改版：RotateTowards + MoveRotation）──
        //    ⭐ 三处与旧版的差别（每处都有**官方依据**，收工时进笔记第 29 章）：
        //      ① 位置：`Update` → **`FixedUpdate`**（物理姿态要在物理步里改）
        //      ② 接口：`transform.rotation` → **`_rb.MoveRotation(...)`**
        //         （官方："会遵循该 Rigidbody 的插值设置"；`MoveRotation` = 运动式，而 `Rigidbody.rotation` = 瞬移）
        //      ③ 插值：`Slerp`（比例制，指数逼近）→ **`RotateTowards`（角速度制）**
        //         （官方："`maxDegreesDelta` = 本次旋转允许的最大角度（度）"、"**不会过冲**"、有限帧内精确到达）
        //    判据问题 A：⭐ 起点为什么必须用 **`_rb.rotation`**，不能用 `transform.rotation`？
        //               （提示：刚体开了 `Interpolate` ⇒ `transform.rotation` 是**渲染插值出来的中间值**，
        //                 拿它当起点 = 把插值结果喂回物理 ⇒ 转身变慢 / 抖）
        //    判据问题 B：步长为什么用 `Time.fixedDeltaTime` 而不是 `deltaTime`？
        //    判据问题 C：守卫 `_input.sqrMagnitude > 0f` 为什么必须留？（零向量边界）
        if (_input.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_input, Vector3.up);
            float step = turnSpeed * Time.fixedDeltaTime;   // 度/秒 × 物理步 = 这一步最多转多少度
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, step));
        }
        // TODO ⑥ 冲刺（倍率）：把 sprintMultiplier 乘进"速度那一档"
        //    判据问题 A：`按住` 怎么问？（提示：判据⑤ —— 电平语义，用 IsPressed 而不是自己拿 1/0 比）
        //    判据问题 B：乘完为什么**不能**再归一化？
        //    判据问题 C：倍率该乘在"目标速度"上还是"方向"上？（回到判据⑤：速度大小只在一处决定）
    }

    // ══════════════════════════════════════════════════════════════
    //  ⬇⬇⬇ 10-16 新增：三个待写的部分（方法体留给你）⬇⬇⬇
    // ══════════════════════════════════════════════════════════════

    // TODO ⑩ 【接地判定】脚下有没有地面？（组合拳：先问"有没有"，再问"斜不斜"）
    //   ⭐ 要回答的三个问题（想清楚再落笔）：
    //     ① **起点在哪**？
    //        · 胶囊底 = `_col.bounds.min.y`（**物理侧** —— 承判据⑬）
    //        · ⚠️ 还要往上抬 `groundCheckLift`：官方 Notes 原话
    //          "`Raycasts will not detect Colliders for which the Raycast origin is inside the Collider.`"
    //          ⇒ 起点贴在碰撞体表面/内部就会**静默不命中**
    //        · 动手前先把这句几何关系画出来："起点在胶囊底之上一点点，射线往下打 groundCheckDistance 那么长"
    //     ② 两步各问什么？
    //        · `Physics.CheckSphere(起点, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore)`
    //          ⇒ 只回 bool："这块球里有没有地面"（**有体积** ⇒ 站在平台边缘也不说谎）
    //        · 有的话再 `Physics.Raycast(起点, Vector3.down, out hit, groundCheckDistance, groundLayer)`
    //          ⇒ 拿 `hit.normal` 做坡度校验（官方原话：**要真法线就用 Raycast**；`SphereCast` 的法线不可靠）
    //     ③ 坡度的判据怎么写？
    //        · "面朝上"= `Vector3.Dot(hit.normal, Vector3.up)` 大；阈值 = `Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad)`
    //        · ⚠️ 阈值本身就是"你想允许多陡的坡" —— 45° ⇒ 0.707
    //          （🪞 这就是那天讲的 UE `WalkableFloorZ = 0.71` 的由来，也别写成 `normal == Vector3.up`）
    //   🎯 自测判据：站在平地必须命中；把检查器里 `groundLayer` 清成 Nothing ⇒ 立刻永远 false（怎么按都跳不起来）
    //   ⚠️ 别忘 `groundLayer` 是**空白**时谁也不问 —— 这是最常见的一类"静默不工作"

    // ── ⭐ 探针起点的「唯一一个家」（10-16 新增）────────────────────────────
    //   判据：**"测的地方"和"画的地方"必须是同一个点** —— 所以"起点怎么算"只准写在这一处：
    //     ① `CheckGrounded()` 读它（真正发查询用）
    //     ② `OnDrawGizmosSelected()` 读它（把探针画出来）
    //   为什么用属性而不是各写两行：🪞 承 `CameraFollow` 的 `_yaw` —— **一个状态只能有一个家**；
    //     抄成两份 ⇒ 迟早"改一处忘另一处" ⇒ 你会对着一个**假 Gizmo** 调半天。
    //
    //   ⚠️ 为什么里面要 `??=` 现取（而不是直接用 `_col`）：
    //     `_col` 是在 `Awake` 里赋的，而 **编辑器非播放模式下 `Awake` 不跑**，
    //     `OnDrawGizmosSelected` 却会执行 ⇒ 那时 `_col` 还是 null ⇒ 直接用会 NRE 刷屏。
    //     ⭐ 判据：**编辑器期的可视化代码，不该依赖运行期的初始化顺序。**
    private Vector3 GroundProbeOrigin
    {
        get
        {
            // ⭐ 这里**故意不做任何兜底**（10-16 晚 · 你提的）—— `_col` 的家是 `Awake`：
            //   `Awake` 里那条"响亮地失败"的自检（缺胶囊 ⇒ LogError + `enabled = false` + return）
            //   已经把"`_col` 还是 null"这条路**堵死了** ⇒ 运行期热路径不需要再买一份保险。
            //   ⭐ 判据：**"谁有特殊需求，谁自己付代价"** —— "编辑器期 `Awake` 不跑"是**编辑器期**的问题，
            //      就该在编辑器期的代码里解决（兜底已搬到 `OnDrawGizmosSelected`）。
            //   ⚠️ 说句实在话：省下的那点开销**小到无感**（每秒百来次判断）——
            //      真正的理由是**职责分离**，不是性能（别把"设计清晰"说成"性能优化"）。
            //
            //   📌 另外把 10-16 踩过的那个坑记在这儿（怕以后又写"聪明写法"）：
            //      `??=` / `??` **不能用在 `UnityEngine.Object` 上** —— 它们走 CLR 的"真 null"检查，
            //      会**绕过** Unity 给 `UnityEngine.Object` 重载的 `==`。退出播放 / 域重载后旧引用是
            //      "**假 null**"（引用还在、对象已销毁）：`_col == null` 判 **true**，`_col ??= xxx` 判
            //      "**不用赋值**" ⇒ 症状 = "永远拿不到组件、静默走兜底"（当时 Gizmo 被画到了角色中心）。
            //      ✅ 判据：**Unity 对象判空一律写 `== null`**。

            // ⭐ 起点 = "胶囊**轴心**正下方、再抬一点点"
            //   ⚠️ 别写成 `Vector3 p = _col.bounds.min;`（10-16 我真写错过）：
            //      `bounds.min` 是**包围盒的一个「角点」**（x、y、z 三个分量**全取最小**）
            //      ⇒ 起点会落到"左下角"而不是"脚底中心" ⇒ 探针球/射线整体偏到一边
            //        （站在窄平台或边缘时，探针可能压根探不到脚下那块地）。
            //   ✅ 正确配方：**x、z 取包围盒中心，y 取包围盒最低**。
            //   ⚠️ `bounds` 是**属性、返回结构体** ⇒ 必须先复制到局部变量，再改它的分量（否则 CS1612）
            Vector3 p = _col.bounds.center;              // x、z = 胶囊轴心
            p.y = _col.bounds.min.y + groundCheckLift;   // y = 脚底 + 抬 2cm（躲开"起点在碰撞体内 ⇒ 静默不命中"）
            return p;
        }
    }

    private bool CheckGrounded()
    {
        // ── ① 起点：这里**不要自己算**！读下面那个 `GroundProbeOrigin` ──
        //    ⭐ 判据：**"测的地方"和"画的地方"必须是同一个点** ⇒ 起点的算法只准有一个家
        //      （🪞 承 `CameraFollow` 的 `public float Yaw => _yaw;` —— 同一个状态只能有一个家）
        Vector3 origin = GroundProbeOrigin;

        // ── ② 第一道闸：脚下这块**球**里有没有"地面层"的东西？──
        //    ⭐ CheckSphere 是【重叠】查询：没有方向、但有体积 ⇒ 站在平台边缘也不说谎
        //    ⭐ 第四个参数 `QueryTriggerInteraction.Ignore` = "本次不算触发器"
        //       （写死它，别用默认的 UseGlobal —— 否则行为被全局设置牵着走，将来放个金币触发器在脚边就误判）
        if (!Physics.CheckSphere(origin, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore))
            return false;              // 球里啥也没有 ⇒ 铁定悬空（后面那根射线不用发了，省一次查询）

        // ── ③ 第二道闸：往下打一根短线，拿【真法线】判"这个面能不能站"──
        //    ⚠️ 这个带 out 的重载里，maxDistance / layerMask / queryTriggerInteraction **没有默认值** ⇒ 必须全给
        //    ⭐ 只有 Raycast 的 `hit.normal` 是真实表面法线
        //       （SphereCast 的那个常是"接触点→球心"方向，官方明确警告过 —— 这也是"两个探针合起来用"的理由）
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore)) 
            return false;              // 球里有东西、但脚下这根线没打到「面」（例如只蹭到侧壁的鼓包）

        // ── ④ 坡度：法线够不够"朝上"？阈值 = cos(允许的最大坡度角) ──
        //    · 平地 ⇒ normal ≈ up ⇒ Dot = 1；45° ⇒ 0.707；垂直墙 ⇒ 0 ⇒ 判"不能站"
        //    · ⚠️ 千万别写 `hit.normal == Vector3.up`：浮点相等必挂，而且"斜坡"正是"不相等但能站"
        float minDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        return Vector3.Dot(hit.normal, Vector3.up) >= minDot;
    }

    // TODO ⑪ 【起跳】把 y 那一格**覆盖**成 jumpSpeed
    //   判据问题 A：为什么是"覆盖"而不是 `+=`？
    //     （提示：一个物理步只提交一次速度；累加式在"贴着地面每步都加"时会变成火箭
    //       —— 🪞 承 10-06 敌人追逐那条"覆盖式速度，不是累加式"）
    //   判据问题 B：水平两轴要不要动？（回到判据④ —— 谁负责水平、谁负责竖直？）
    //   判据问题 C：`linearVelocity` 是**属性、返回结构体** ⇒ 写不出 `_rb.linearVelocity.y = x`（CS1612）
    //     ⇒ 照"取出 → 改 → 整份写回"三步走（🪞 下面水平那段就是这么写的，照镜子自己写）
    //   🎯 目标高度：`v = √(2·g·h)`（g = 9.81）⇒ 检查器里 `jumpSpeed = 6.3` 对应约 **2 米**
    //      跳完拿地板的格子量一下（1 格 = 1 米）—— 对不上就改那个数，别改重力
    private void Jump()
    {
        Vector3 v = _rb.linearVelocity;
        v.y = jumpSpeed;
        _rb.linearVelocity = v;

        // 🆕 10-20：跳成功之后才广播（放在"真正改了速度"之后 = 只报已经发生的事实）
        //   ⚠️ `?.Invoke()` 而非 `Invoke()`：没人订阅时直接返回，不抛空引用
        OnJumped?.Invoke();
    }

    // TODO ⑬ 【看得见】把探针画出来（只在选中该物体时画，别污染整个场景视图）
    //   ⭐ 一条硬判据：**画的位置必须与 `CheckGrounded` 用的是同一个起点** ——
    //      否则"画的是 A、测的是 B"，你会对着一个**假 Gizmo** 调半天。
    //      ⇒ 建议把"起点"抽成一个 private 属性（如 `GroundProbeOrigin`），两处都读它
    //        （**同一个状态只能有一个家**，🪞 承 `CameraFollow._yaw` 那条）
    //   画什么：`Gizmos.DrawWireSphere(起点, groundCheckRadius)`
    //        + `Gizmos.DrawLine(起点, 起点 + Vector3.down * groundCheckDistance)`
    //   加分项：接地绿色 / 悬空红色（`Gizmos.color = _isGrounded ? Color.green : Color.red`）
    //   ⚠️ 一个真会踩的坑：Gizmo 在**非播放模式**下也会执行，而 `_col` 是 `Awake` 里赋的
    //      ⇒ 编辑器里选中物体时报 NRE ⇒ 这里得自己兜底（或在属性里"按需现取"）
    private void OnDrawGizmosSelected()
    {
        // ⚠️ 编辑器**非播放模式**下 `Awake` 不跑 ⇒ `_col` 可能是空（或"假 null"，见 `GroundProbeOrigin` 那段注释）
        //    ⇒ **兜底放在这里**（10-16 晚调整）：⭐ 判据 = **"谁有特殊需求，谁自己付代价"**
        //      —— "编辑器期"的需求，不该让运行期每个物理步都替它判一次
        //    ⚠️ 这里必须用 `== null`（不用 `??=`）：Unity 的重载才认得"假 null"
        if (_col == null) _col = GetComponent<CapsuleCollider>();
        if (_col == null) return;      // 连胶囊都没有 ⇒ 干脆什么都不画（不刷屏、不当噪声源）

        // 起点走唯一的那个家（"测的地方"和"画的地方"必须是同一个点）
        Vector3 origin = GroundProbeOrigin;

        // 颜色 = 当前判定结果。
        // ⭐ `Gizmos.color` 是**状态**：它影响**之后**画的每一笔 ⇒ 必须先设色、再画。
        Gizmos.color = _isGrounded ? Color.green : Color.red;

        // ── 第一笔：脚下的"呼啦圈" —— 对应 `Physics.CheckSphere(origin, groundCheckRadius, …)` ──
        //    半径用的就是 `groundCheckRadius`（0.28）⇒ 画面里这条圈**明显小于**胶囊（半径 0.5）
        Gizmos.DrawWireSphere(origin, groundCheckRadius);

        // ── 第二笔：往下探的那根线 —— 对应 `Physics.Raycast(origin, Vector3.down, …, groundCheckDistance, …)` ──
        //    `origin + Vector3.down * groundCheckDistance` 就是射线的**末端**
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);

        // ── 第三笔：把"起点"本身点出来（白色实心小球，半径 0.03）──
        //    为什么值得画：球心/线头在画面里都是空的，不点出来的话你没法确认"起点到底落在哪"
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(origin, 0.03f);
    }
}
