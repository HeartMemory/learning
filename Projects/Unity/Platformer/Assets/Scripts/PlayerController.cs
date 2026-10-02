using UnityEngine;
using UnityEngine.InputSystem;

// ══════════════════════════════════════════════════════════════
//  PlayerController —— 平台跳跃（Block 3 · 第 7 号成品）
//  今天引入的三套机制：
//    ① 输入侧：边沿触发 → 意图标志位 → 物理步消费（两种时钟之间的桥）
//    ② 检测侧：脚点探针 + 层掩码（OverlapCircle）→ 现问现答，无延迟
//    ③ 运动侧：Dynamic 刚体只交【速度】；落地归零让起跳高度成为确定值
// ══════════════════════════════════════════════════════════════
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 6f;      // 面值放检查器可调（09-18 学过的理由）

    [Header("跳跃手感")]
    [SerializeField] private float jumpSpeed = 7f;      // h = v²/(2g) ≈ 2.5（屏幕顶只到 +5）
                                                        // ⭐ 高度对 v 是【平方】关系：想跳高一倍，v 要 ×1.414
    [SerializeField] private int maxJumps = 2;          // 1 = 普通跳；2 = 二段跳
    [SerializeField] private float checkRadius = 0.15f; // 探针半径：调大更"黏"，但"离地后仍判接地"的窗口也变长
    [SerializeField] private Transform groundCheck;     // 拖 Player/GroundCheck 进来
    [SerializeField] private LayerMask groundLayer;     // ⚠️ 只勾 Ground，不能勾 Player

    private Rigidbody2D _rb;
    private InputAction _move;   // 查表一次就缓存（与 Camera.main 同理，别每帧查）
    private InputAction _jump;

    // ── 两个"跨时钟的桥"：Update 写、FixedUpdate 读 ──
    private float _axis;            // 电平语义：你【正推着】多少
    private int _jumpsLeft;         // 可消耗资源：起跳次数余额，落地回满
    private bool _jumpRequested;    // 边沿语义：这一帧【刚按下】

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // ⭐ 校验必须放在使用之前（"日志放在会抛异常的行之后 = 等于没放"，09-24 的教训）
        Debug.Log($"项目级 Actions 已接上？ → {InputSystem.actions}", this);   // 接线验证完可删

        // 与 Breakout 挡板同一套：项目级输入资产（Player 地图），只在 Awake 查一次
        _move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
        _jump = InputSystem.actions.FindActionMap("Player").FindAction("Jump");

        // ── 接线自检：一次把问题全报出来，再统一停手 ──
        //   ⭐ 用 LogError 而不是 LogWarning：红的 + 能配「Error Pause」把 Play 停住；
        //      "响亮地失败"的关键是【停手】—— 不停手的话真正的病因（槽位没接）会被
        //      下游每物理步刷屏的 NullReferenceException 埋掉，报错指向错误的地方
        //   ⭐ "收集全部问题再停" 而不是 "报第一个就 return"：前者修一轮，后者修三轮
        bool wiringOk = true;

        if (groundCheck == null)
        {
            Debug.LogError("groundCheck 槽位是空的：没把 Player/GroundCheck 拖进来", this);
            wiringOk = false;
        }
        if (groundLayer.value == 0)
        {
            Debug.LogError("groundLayer 一个层都没勾：检查器里至少要勾上 Ground", this);
            wiringOk = false;
        }
        if ((groundLayer.value & (1 << gameObject.layer)) != 0)
        {
            // ⚠️ 探针是个【圆】，圆心就在自己碰撞体的底边上 → 掩码一旦放行 Player 层，
            //    就会"永远算接地"，而且控制台一声不响。
            //    ⭐ 判据：value == 0 只抓"忘了勾"，抓不了"勾错了" → 两个检查都要有
            Debug.LogError($"groundLayer 勾到了本物体所在的层（第 {gameObject.layer} 层）：探针会命中自己 → 永远算接地", this);
            wiringOk = false;
        }

        if (!wiringOk) { enabled = false; return; }   // ⭐ 停手

        _jumpsLeft = maxJumps;   // 开局次数置满
    }

    void Update()
    {
        _axis = _move.ReadValue<Vector2>().x;         // 输入的"当前意图"，每帧拉一次

        // ⭐ 边沿 API：只有"按下那一帧"为 true → 按住空格不会连跳
        //    （写成 IsPressed() = 电平，按住期间每帧都真 → 连续跳）
        // ⭐ 为什么要转成 _jumpRequested：这是"帧级事件"跨进物理步的唯一桥梁 ——
        //    直接在 FixedUpdate 里读它，一个渲染帧的 0~N 个物理步会【漏读或重读】
        if (_jump.WasPressedThisFrame()) _jumpRequested = true;
    }

    void FixedUpdate()
    {
        // ① 接地判定：脚点探针，现问现答
        //    · 圆心取 .position（世界坐标）——查询在世界空间做；检查器「位置」显示的是本地坐标
        //    · 掩码只放行 Ground 层 → 天然排除玩家自己（那个圆会压到自己身上）
        //    · 返回 Collider2D 或 null → "有没有拿到东西"就是"接不接地"（用 != null，别用 is not null）
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer) != null;

        // ② y 先取出来：下面三处决策都靠它判断
        float y = _rb.linearVelocity.y;

        // ③ 回满次数：⭐ 只在【停住或下落】时回满（y <= 0f）
        //    判据：起跳后头一两步，探针还压在地里（圆心才抬升 0.1~0.2）→ 若写成 if (isGrounded) 无条件回满，
        //          次数会被非法回满（空中连点能白送跳次）
        //    ⭐ 通用判据：传感器有"跟不上状态变化"的那一瞬间，用它判状态时必须把这一瞬排除
        if (isGrounded && y <= 0f) _jumpsLeft = maxJumps;

        // ④ 一次决策，把 y 的三个去处合成一个：
        //    · 起跳：y = jumpSpeed（【覆盖】不是累加 —— 覆盖才能让高度只由 jumpSpeed 决定）
        //    · 落地：y < 0 时才归零（站住时求解器会残留一点负值；归零后起跳高度确定）
        //    · 其余：原样保留（y 归重力管，承 09-21"Kinematic 交位置 / Dynamic 交速度"）
        if (_jumpRequested && _jumpsLeft > 0)
        {
            y = jumpSpeed;
            _jumpsLeft--;
        }
        else if (isGrounded && y < 0f)
        {
            y = 0f;
        }

        // ⑤ 消费掉意图：放在 if【外面】
        //    判据：放外面 = 跳不了就【即时作废】；放里面 = 意图"憋着"，等落地回满那一刻自动释放
        //         → 症状是"落地自己弹一下"（意外的输入缓冲）
        _jumpRequested = false;

        // ⑥ ⭐ 全场唯一一次速度提交：x 归输入，y 归上面那套决策
        //    判据：一个物理步只能提交一次 —— 分两次赋值，后写的会覆盖先写的
        _rb.linearVelocity = new Vector2(_axis * moveSpeed, y);
    }

    // ══════════════════════════════════════════════════════════════
    //  探针可视化：选中 Player 时，在场景视图里画出那个"接地的圆"
    //    · OnDrawGizmosSelected = 只在【选中该物体】时画（用 OnDrawGizmos 会一直画、场景会乱）
    //    · Play 模式下也会画 → 能亲眼看到"接地时圈贴地 / 起跳后圈离地"
    //    · 只用于【看】，不参与物理
    // ══════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;   // 还没接线就别画（编辑器里会先跑到这一步）

        Gizmos.color = Color.cyan;         // 青色：跟 Unity 默认的碰撞体绿区分开
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }

}
