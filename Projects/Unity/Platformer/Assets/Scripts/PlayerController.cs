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
    [SerializeField] private float jumpSpeed = 7f;      // h = v²/(2g) ≈ 2.5
                                                        // ⭐ 高度对 v 是【平方】关系：想跳高一倍，v 要 ×1.414
    [SerializeField] private int maxJumps = 2;          // 1 = 普通跳；2 = 二段跳
    [SerializeField] private float checkRadius = 0.15f; // 探针半径：调大更"黏"，但"离地后仍判接地"的窗口也变长
    [SerializeField] private Transform groundCheck;     // 拖 Player/GroundCheck 进来
    [SerializeField] private LayerMask groundLayer;     // ⚠️ 只勾 Ground，不能勾 Player

    [Header("视觉")]
    [SerializeField] private Transform visual;   // 拖 Player/Visual（只装视觉的子物体）

    private Rigidbody2D _rb;
    private Collider2D _col;                                            // 🆕 真接触判定用它
    private readonly ContactPoint2D[] _contacts = new ContactPoint2D[8]; // 🆕 复用缓冲，别每物理步 new
    private InputAction _move;   // 查表一次就缓存（与 Camera.main 同理，别每帧查）
    private InputAction _jump;
    private int _facing = 1;                 // 当前朝向：1 = 朝右，-1 = 朝左


    // ── 两个"跨时钟的桥"：Update 写、FixedUpdate 读 ──
    private float _axis;            // 电平语义：你【正推着】多少
    private int _jumpsLeft;         // 可消耗资源：起跳次数余额，落地回满
    private bool _jumpRequested;    // 边沿语义：这一帧【刚按下】



    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

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

        if (visual == null)
        {
            Debug.LogError("visual 槽位是空的：转身要翻的是【视觉子树】，不是物理根", this);
            wiringOk = false;
        }
        if (_col == null)
        {
            Debug.LogError("Collider2D 没找到：真接触判定需要玩家自己的碰撞体", this);
            wiringOk = false;
        }
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

        // 转身：⭐ 只翻【视觉子树】—— 翻物理根会把 GroundCheck 一起镜像 → 探针跑到身后
        //   阈值 0.01 而不是 != 0：按键/摇杆的微小抖动不该触发转身
        if (_axis > 0.01f) _facing = 1;
        else if (_axis < -0.01f) _facing = -1;
        ApplyFacing();

    }

    void FixedUpdate()
    {
        // ① 接地判定（10-04 改）：从「脚点探针」换成「真接触 + 接触面朝上」
        //    · 为什么换：探针只覆盖脚底中间 30% → 站在平台边缘时"人站着、探针却说没地"
        //      → 回满门被关掉 → 次数永远是 0（实测日志：_jumpsLeft = 0, isGrounded = False）
        //    · 为什么必须看【法线】：贴墙也是接触！只看"有没有接触"会让空中贴墙白送跳次
        //    · 必须在 FixedUpdate 里读：接触每个物理步更新（放 Update 里读到的是滞后值）
        bool isGrounded = IsStandingOnGround();

        // ② y 先取出来：下面三处决策都靠它判断
        float y = _rb.linearVelocity.y;

        // ③ 回满次数：⭐ 只在【停住或下落】时回满（y <= 0f）
        //    判据：起跳后头一两步，探针还压在地里（圆心才抬升 0.1~0.2）→ 若写成 if (isGrounded) 无条件回满，
        //          次数会被非法回满（空中连点能白送跳次）
        //    ⭐ 通用判据：传感器有"跟不上状态变化"的那一瞬间，用它判状态时必须把这一瞬排除
        if (isGrounded && y <= 0f) _jumpsLeft = maxJumps;

        // ④ 一次决策：y 只有两个去处
        //    · 起跳：y = jumpSpeed（【覆盖】不是累加 —— 覆盖才能让高度只由 jumpSpeed 决定）
        //    · 其余：原样保留（y 归重力管，承 09-21"Kinematic 交位置 / Dynamic 交速度"）
        //    ⭐ 落地归零那一步【已删除】：它想保证的"起跳高度确定"，其实由【覆盖式赋值】已经满足；
        //       而它的副作用是"探针一提早喊接地就把下落停住" → 角色悬在半空（缝隙）。
        //       现在落地位置交给【物理求解器】（碰撞体接触面）决定 → 精确贴地。
        if (_jumpRequested && _jumpsLeft > 0)
        {
            y = jumpSpeed;
            _jumpsLeft--;
        }

        // ⑤ 消费掉意图：放在 if【外面】
        //    判据：放外面 = 跳不了就【即时作废】；放里面 = 意图"憋着"，等落地回满那一刻自动释放
        //         → 症状是"落地自己弹一下"（意外的输入缓冲）
        _jumpRequested = false;

        // ⑥ ⭐ 全场唯一一次速度提交：x 归输入，y 归上面那套决策
        //    判据：一个物理步只能提交一次 —— 分两次赋值，后写的会覆盖先写的
        _rb.linearVelocity = new Vector2(_axis * moveSpeed, y);
    }
    // ⭐ 真接触判定：物理上真的"踩在"某个面上
    //    · normal 的方向约定：从【对方表面】指向【我】 → 站在地面上时 ≈ (0, 1)
    //    · 阈值 0.7 ≈ 45° 以上才算"脚下"（台阶 / 斜面的宽容度，实测再调 —— 承 10-03 的台阶经验）
    //    · 复用 _contacts 缓冲：这个方法每物理步都会调，别在里面 new（09-25 的 GC 教训）
    //    · GetContacts 只返回【真实物理接触】→ 触发器、以及层矩阵没放行的对，天然不会出现
    private bool IsStandingOnGround()
    {
        int count = _col.GetContacts(_contacts);
        for (int i = 0; i < count; i++)
        {
            if (_contacts[i].normal.y > 0.7f) return true;
        }
        return false;
    }

    // 转身：只改视觉子树 x 的【符号】，幅值保持不变
    //   脏检查：没变就不写（与 ScoreManager 同族 —— 别每帧无脑写渲染/变换属性）
    private void ApplyFacing()
    {
        Vector3 s = visual.localScale;
        float want = Mathf.Abs(s.x) * _facing;
        if (Mathf.Approximately(s.x, want)) return;
        s.x = want;
        visual.localScale = s;
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
