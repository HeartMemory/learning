using UnityEngine;
using UnityEngine.InputSystem;

// ══════════════════════════════════════════════════════════════
//  PlayerController —— 3D 跑酷雏形（Runner · 第 8 号成品）
//  第 1 步（10-14）：前后左右跑 + 按住 Shift 冲刺 ⇒ 闸门①「3D 能跑」
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
    [Header("移动")]
    [SerializeField] private float moveSpeed = 6f;   // 米/秒；面值放检查器可调

    [Header("冲刺")]
    [SerializeField] private float sprintMultiplier = 1.6f;   // 倍率制：改一处即生效，且"冲刺必比走快"这条关系不会被破坏

    private Rigidbody _rb;
    private InputAction _move;    // Awake 查一次就缓存（与 Camera.main 同理：别每帧查）
    private InputAction _sprint;  // 同上：Player/Sprint（Button · 左 Shift）
    private Vector3 _input;       // ⭐ 跨时钟的桥：Update 写、FixedUpdate 读

    private void Awake()
    {
        // TODO ① 装配期自检：`_rb` 没挂就"响亮地失败"
        //    判据问题：这条日志必须放在**哪一步之前**？
        //              （提示：日志放在使用它的那一行之后 = 等于没放）
        //    🪞 风格照镜子：bool 收集问题 → Debug.LogError → enabled = false
        _rb = GetComponent<Rigidbody>();
        _move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
        _sprint = InputSystem.actions.FindActionMap("Player").FindAction("Sprint");

        bool isError = false;
        if(_rb == null) { Debug.LogError("PlayerController 缺 Rigidbody"); isError = true; }
        if(_move == null) { Debug.LogError("PlayerController 缺 Player/Move"); isError = true; }
        if(_sprint == null) { Debug.LogError("PlayerController 缺 Player/Sprint"); isError = true; }
        if (isError) { enabled = false; return; }
        // TODO ② 接项目级输入（查一次就缓存成 _move / _sprint）
        //    🪞 镜子：Platformer/PlayerController.cs 第 77 行
        //    · Move   → 本项目 Player/Move（Value · Vector2）
        //    · Sprint → 本项目 Player/Sprint（Button · 左 Shift；模板自带，**不用改资产**）
    }

    private void Update()
    {
        // TODO ③ 读 Move → 映射到世界坐标
        //    判据问题：Move 是 Vector2（x = 左右、y = 前后），世界是 Vector3 ——
        //              哪个分量该落到 z 上？为什么？（想清楚"屏幕里往前"是世界的哪个轴）
        Vector2 moveInput = _move.ReadValue<Vector2>();
        // TODO ④ 归一化（**只在需要时**才做）
        //    判据问题：为什么判据写 `sqrMagnitude > 1f` 而不是 `magnitude > 1f`？
        //              （今天底层原理②：哪两个操作里各藏了一次开根号？）
        //    ⚠️ 归一化只准落在**方向**上（判据⑤）—— 这一格里绝不能出现 moveSpeed / 倍率
        moveInput = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
        _input = new Vector3(moveInput.x, 0f, moveInput.y);
    }

    private void FixedUpdate()
    {
        // TODO ⑤ 把桥变量汇合成速度
        //    判据问题 A：竖直那一路该写什么？（回到判据④ —— 你只拥有水平两轴）
        //    判据问题 B：松键就停，是靠"每步都覆盖速度"实现的，还是靠摩擦？
        //              两种做法的体感差在哪？
        float speed = _sprint.IsPressed() ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 v = _rb.linearVelocity;
        v.x = _input.x * speed;
        v.z = _input.z * speed;
        _rb.linearVelocity = v;
        // TODO ⑥ 冲刺（倍率）：把 sprintMultiplier 乘进"速度那一档"
        //    判据问题 A：`按住` 怎么问？（提示：判据⑤ —— 电平语义，用 IsPressed 而不是自己拿 1/0 比）
        //    判据问题 B：乘完为什么**不能**再归一化？
        //    判据问题 C：倍率该乘在"目标速度"上还是"方向"上？（回到判据⑤：速度大小只在一处决定）
    }
}
