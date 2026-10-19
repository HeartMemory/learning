using UnityEngine;
using UnityEngine.InputSystem;

// ══════════════════════════════════════════════════════════════
//  CameraFollow —— 第三人称「球面轨道相机」（Runner · 第 8 号成品 · 第 2 步）
//  第 1 步（10-14）：PlayerController（3D 刚体移动）⇒ 闸门①「3D 能跑」
//  第 2 步（10-15）：视角由玩家直控 + 相机绕「枢轴点」做球面跟随
//
//  ⭐ 10-15 升级（当天的第二个版本，旧版作废）：
//     旧版：位置 = target.position + `Euler(0, yaw, 0) * offset`（**位置不含 pitch**）
//           ⇒ 上下看时相机**不绕**、只"原地抬头低头" ⇒ **角色被顶出屏幕**。
//     新版：位置 = 枢轴点 + `Euler(pitch, yaw, 0) * back * distance`
//           ⇒ 相机绕枢轴做**球面**运动 ⇒ 上下看时**角色始终在画面里**。
//     依据（公开权威文档，非二手说法）：
//       · Minecraft `follow_orbit` 预设：`radius`（距离）+ yaw（±180）+ pitch（±90 防翻转）
//         + `entity offset`（锚在角色身上的枢轴点）；原话"**相机朝向方向默认始终面向玩家**"
//       · Unity Cinemachine `3rd Person Follow`：肩部枢轴（Shoulder Offset）+ 垂直臂长
//         + `Camera Distance`；"**相机始终直视手（目标点）**"是"目标不丢出画面"的硬约束
//     （永劫无间这类商业游戏的**内部实现没有公开资料**，别信"某游戏相机是这么写的"这类传言；
//       但它们的**行为特征**就是上面这套公开标准做法。）
//
//  ✅ 判据（它们决定方法体怎么写）：
//    ① 【两个时钟之外还有第三个】相机跟随写在 **LateUpdate**，不是 Update：
//       玩家在 Update 读输入、在 FixedUpdate（50 Hz）被物理推进；相机写在 Update 时
//       同一帧先后顺序**未定义** ⇒ 可能读到"上一帧的玩家" ⇒ 画面抖。LateUpdate 保证"所有 Update 之后"。
//    ② 【视角 vs 位置 = 两条路，阻尼只准加一条】
//       · 视角（yaw / pitch）与滚轮距离 → **玩家直控** ⇒ 1:1、**零阻尼**、不乘 deltaTime
//       · 相机位置（被动跟随）→ `SmoothDamp` ⇒ 才有阻尼
//    ③ 【pitch 必须夹取、yaw 不夹】pitch 越过 ±90° 会翻跟头 ⇒ 夹 [-30, 70]（比 Minecraft 的 ±90 更保守）
//    ④ 【枢轴点】相机绕的是"角色身上的一点"（胸口），不是脚底 —— 这决定了上下看时角色在屏幕里的位置
//    ⑤ 【跨帧状态必须养字段】`_yaw / _pitch / _velocity / distance` 跨帧 ⇒ 全是字段
//    ⑥ 【装配期自检】target / Look 没接上 ⇒ 响亮地失败（bool 收集 → LogError → enabled = false）
//
//  🪞 镜子（同族写法都在仓库里）：
//     · 接输入（查一次就缓存）→ PlayerController.Awake 的 `_move` / `_sprint`
//     · 装配期自检 → 同上（一次报全、再统一停手；Platformer 的 CameraFollow 也是这套）
//     · 跨帧状态养字段 → Platformer/PlayerController.cs 的 `_axis`
//     · 位置跟随 + 阻尼 → Platformer/Assets/Scripts/CameraFollow.cs:130
//
//  🎯 验收（收工当场拍板）：
//     ① 动鼠标 → 视角 1:1 跟手（不飘、不延迟）
//     ② 上下转到底 → 不翻跟头（pitch 夹取生效）
//     ③ ⭐ **上下看时角色始终在画面里**（本次升级的身份证：相机在绕球面，不是在原地转头）
//     ④ ⭐ 左右看 → 相机绕角色水平转；**角色转身时相机不跟着绕**（视角只归玩家输入）
//     ⑤ 滚轮 → 拉近 / 拉远，角色不跑出画面
//     ⑥ 跑动时画面不抖；控制台 0 红字
//
//  ⚠️ 骨架期控制台会有黄字（字段已赋值但还没被读）—— 写完 TODO 自动消失。
//     （**红字才是禁令，黄字是待办清单** —— 承 114「警告当尺子」）
// ══════════════════════════════════════════════════════════════
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;   // 拖 Hierarchy 里的 Player

    [Header("枢轴点（相机绕着它转、也一直盯着它）")]
    [SerializeField] private float pivotHeight = 0.5f;
    // Player 的胶囊中心 = target.position（世界 y ≈ 1）、身高 2 米 ⇒ 0.5 落在"胸口"高度
    // ⚠️ 取 0 就是看"脚底"：上下看时角色会更快地滑向画面边缘 —— 这就是"枢轴点"存在的理由

    [Header("球面距离（滚轮控制 · 玩家直控 ⇒ 零阻尼）")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float scrollSensitivity = 0.01f;
    // ⚠️ 滚轮给的是**平台原始单位**（Windows 上通常 ≈ 120 / 格）⇒ 0.01 ≈ 一格 1.2 米
    //    若滚一格相机飞出几百米 ⇒ 灵敏度太大；滚半天没反应 ⇒ 太小（自己调，Play 里说了算）
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 12f;

    [Header("视角（鼠标 / 右摇杆 · 1:1 直控）")]
    [SerializeField] private float lookSensitivity = 0.15f;   // 度 / 像素（Look 给的是屏幕像素增量）
    [SerializeField] private float minPitch = -30f;           // 抬头下限（负 = 往上看）
    [SerializeField] private float maxPitch = 70f;            // 低头上限（正 = 往下看）

    [Header("位置阻尼")]
    [SerializeField] private float smoothTime = 0.15f;   // 秒；只作用于"跟位置"那一路

    private InputAction _look;    // Awake 查一次就缓存（照镜子：PlayerController 的 _move）
    private float _yaw;           // 水平视角（可无限累加）
    private float _pitch;         // 俯仰视角（**必须夹取**）
    private Vector3 _velocity;    // SmoothDamp 的"引用式"速度 ⇒ 必须是字段

    // ⭐ 只读出口（10-15 新增）：视角的**水平朝向**（度）
    //   判据「**同一个状态只能有一个家**」：`_yaw` 只在 CameraFollow 里存一份；
    //   别人（PlayerController 的"输入相对相机"）要问 yaw，就来这里读 ——
    //   别去反解 `transform.eulerAngles.y`（那是**反解**：跨 360° 会绕回，两份数据迟早分叉）
    public float Yaw => _yaw;

    private void Awake()
    {
        // TODO ① 装配期自检 + 缓存 Look 输入 —— 你已完成 ✅
        //    判据问题：这两条 LogError 必须放在**哪一步之前**？
        //              （提示：写在"使用它之后" = 等于没放）
        _look = InputSystem.actions.FindActionMap("Player").FindAction("Look");
        bool isError = false;
        if (target == null) { Debug.LogError("CameraFollow 缺 target"); isError = true; }
        if(_look == null) { Debug.LogError("CameraFollow 缺 Player/Look"); isError = true; }
        if(isError) { enabled = false; return; }
    }

    private void Start()
    {
        // TODO ② 初始化视角状态 —— 你已完成 ✅（10-15 升级后 `_pitch` 改为常数）
        //    `_yaw` 仍然从**角色**推（"开场站在角色背后"）
        //    ⚠️ `_pitch = 20f` 的含义：**正 = 相机升到枢轴上方、低头看枢轴**
        //       （Play 里若发现相机跑到了角色**下方** ⇒ 符号反了，改成 -20 —— 用现象定，别背）
        _yaw = target.eulerAngles.y;
        _pitch = 20f;

        // TODO ③ 光标锁定 —— 你已完成 ✅
        Cursor.lockState = CursorLockMode.Locked;

        // ⭐ 10-19 补：**锁了就必须有人解** —— 否则结束时鼠标被藏在屏幕中心，
        //    结算面板上的 RESTART 根本点不到（症状 = "结束了但没鼠标"）。
        //    判据：**谁加锁谁解锁** —— 锁在本类 Start，解也归本类（一个状态一个家）。
        //    用现成事件：`OnGameOver` 是裁判（GameManager）宣布"结束"的唯一出口。
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += ReleaseCursor;
        else
            Debug.LogError("CameraFollow 找不到 GameManager：结束时鼠标不会被释放（点不到重开按钮）", this);
    }

    private void OnDestroy()
    {
        // 订阅者自己负责退订（承 09-29 事件那课）
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= ReleaseCursor;
    }

    // 交还鼠标：显示出来 + 取消锁定（之后 UGUI 按钮才可点）
    // ⚠️ 重开走的是 LoadScene ⇒ 新场景的 CameraFollow.Start 会重新上锁，不必在这里操心
    private void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        // 10-17：游戏结束后直接 return（读权威状态，不复制、不订阅）
        // ⭐ 放读 Look 之前 ⇒ rotation + position 一起冻住（修"结束后视角还能转"）
        if (GameManager.Instance.IsGameOver) return;

        // ── 前半：视角 / 距离 = 玩家直控（**零阻尼**）──────────────────────

        // TODO ④ 读 Look → 累加 yaw / pitch —— 你已完成 ✅
        //    判据：这一格里为什么**绝不能**出现 `Time.deltaTime` 或 SmoothDamp？
        //    实测（Play 里自己定）：鼠标上推 ⇒ 期望"抬头"；若反了 ⇒ 把 `-=` 改成 `+=`
        Vector2 delta = _look.ReadValue<Vector2>();
        _yaw += delta.x * lookSensitivity;
        _pitch -= delta.y * lookSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // TODO ④-2 滚轮控制距离（⭐ 10-15 新增 · 与 ④ 同属"玩家直控"⇒ 同样零阻尼）
        //    · 取滚轮：`Mouse.current.scroll.ReadValue().y`
        //      ⚠️ `Mouse.current` 有可能为 null（没插鼠标 / 纯触屏）⇒ 空引用防线怎么写？
        //         🪞 镜子：`PlayerAudio` 那类"可缺省的依赖"是怎么处理的（缺了就跳过，不是报错）
        //    · 改距离：`distance` 夹到 `[minDistance, maxDistance]`
        //    判据问题 A：滚轮为什么要"向前滚 = 拉近"？（提示：玩家直觉 = 放大 / 看清目标）
        //    判据问题 B：为什么它也不乘 deltaTime？（回到判据② —— 它和 Look 是同一条路）
        float scroll = Mouse.current?.scroll.ReadValue().y ?? 0f;

        distance = Mathf.Clamp(distance - scroll * scrollSensitivity, minDistance, maxDistance);

        // ── 后半：位置 / 朝向 = 被动跟随（**位置有阻尼**）──────────────────
        // TODO ⑤ 摆位置：绕「枢轴点」的球面（⭐ 10-15 升级 —— 替换掉旧的"只绕 yaw"写法）
        //    三步（方法体自己写）：
        //      ① 枢轴点：`Vector3 pivot = target.position + Vector3.up * pivotHeight;`
        //      ② 方向（从枢轴指向相机）：`Vector3 dir = Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.back;`
        //      ③ 目标位置：`Vector3 desired = pivot + dir * distance;`
        //         再 `transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);`
        //    判据问题 A：`Vector3.back` 换成 `Vector3.forward` 的指纹是什么？
        //               （提示：相机跑到角色**前面**还是**后面**）
        //    判据问题 B：第三个参数为什么必须传**字段** `_velocity`？
        //    判据问题 C：`pivotHeight` 取 0（看脚底）与取 0.5（看胸口），上下看时角色位置差多少？
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 dir = Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.back;
        Vector3 desired = pivot + dir * distance;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        // TODO ⑥ 定朝向：把相机转到 (pitch, yaw) —— 你已完成 ✅
        //    ⭐ 10-15 的新理解（比原来那句"不能 LookAt"更本质）：
        //       **这一步之所以"自动盯着角色"，是因为 ⑤ 的位置就是这么绕出来的** ——
        //       位置 = 枢轴 + `Euler(pitch,yaw) * back * distance`
        //       ⇒ 从相机看向枢轴的方向 = `Euler(pitch,yaw) * forward` = 相机的 forward ⇒ **天然对准枢轴**
        //    判据问题：既然如此，`LookAt(pivot)` 是不是多余？
        //              （提示：它不多余，但它是**另一条路** —— 用了它 = 把 pitch 交给几何，你就不能再自由上下看了）
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
