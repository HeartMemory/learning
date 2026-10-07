using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  EnemyPatrol —— 敌人巡逻（Block 3 · 2026-10-04）
//   三种"到头"信号（提前量不同，互补）：
//     ① 前方探针（悬空预感）：OverlapCircle 查"脚前方还有没有地" → 提前，覆盖平台 / 坑边
//     ② 前方射线（撞墙预感）：Raycast 查"身体高度正前方有没有东西" → 提前，覆盖墙 / 台阶
//        ⭐ ② 是【必需】的：地形是【一个】CompositeCollider2D（地面 + 墙 + 平台合并成一块），
//           敌人脚踩地面时已经在和"那一个碰撞体"接触 → 走到墙边只是"同一个碰撞对多了条接触点"
//           → OnCollisionEnter2D【不会触发】（Enter 的粒度是"两个碰撞体开始接触"）
//     ③ 碰撞回调（撞到别的物体）：Enter 触发 → 覆盖敌人 / 玩家这类"新的碰撞体对"
//   运动方式：Dynamic 交速度（x 归脚本、y 归重力）—— 与玩家同一套
// ══════════════════════════════════════════════════════════════
public class EnemyPatrol : MonoBehaviour
{
    [Header("巡逻")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int startDir = -1;          // 1 = 起步向右；-1 = 起步向左

    [Header("前方探针（悬空预感）")]
    [SerializeField] private Transform edgeCheck;        // 拖 Enemy/EdgeCheck
    [SerializeField] private float checkRadius = 0.12f;  // 世界单位，不受父缩放影响
    [SerializeField] private LayerMask groundLayer;      // ⚠️ 只勾 Ground

    [Header("前方障碍（撞墙预感）")]
    [SerializeField] private float wallCheckDistance = 0.55f;   // 碰撞体半宽 0.4 + 余量 0.15
    [SerializeField] private LayerMask obstacleLayer;            // ⭐ 独立掩码：勾 Ground + Hazard（"挡我的东西"）

    // ⭐ 为什么必须【再开一个】字段，不能继续借用 groundLayer：
    //     "脚前有没有地" 和 "身体正前方有没有墙" 是【两个问题】——
    //     过去两者恰好都是 Ground，所以共用一个字段没暴露问题；
    //     但 Goal 是"只挡人、不站人"的东西，一旦把它塞进 Ground 层：
    //       ① 脚前探针会以为"前方有地" → 敌人会走上 Goal
    //       ② 还会污染玩家的接地判定（PlayerController.groundLayer 也是 Ground）
    //     ⇒ 判据：一个掩码不兼职两件事

    [Header("伤害判定")]
    [SerializeField] private LayerMask playerLayer;      // 🆕 只勾 Player：碰到玩家 = 上报裁判

    [Header("追逐（看到玩家 → 加速）")]
    [SerializeField] private float sightDistance = 6f;      // 视线长度（≈ 半个屏幕）
    [SerializeField] private float chaseMultiplier = 2f;    // 冲刺倍率 → 4；⚠️ 必须 < 玩家速度，否则是"必死"不是"躲避"

    [Header("视觉")]
    [SerializeField] private Transform visual;

    private Rigidbody2D _rb;
    private Vector3 _edgeCheckHome;    // 探针"朝右"时的本地位（x = 0.625）—— 转身只翻 x 符号
    private int _dir = 1;          // ⭐ 给初值：编辑模式下 Awake 不跑，射线的 Gizmos 才画得出来
    private bool _seesPlayer;      // 🆕 本物理步"看到玩家了吗"（给 Gizmos / 插桩用；编辑模式默认 false ✓）


    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _dir = startDir >= 0 ? 1 : -1;
        if (edgeCheck != null) _edgeCheckHome = edgeCheck.localPosition;   // ⭐ 记一次

        // TODO 接线自检（照 PlayerController 的风格：收集全部问题 → enabled = false）
        bool wiringOk = true;

        if (visual == null)
        {
            Debug.LogError("visual 槽位是空的：转身要翻的是【视觉子树】，不是物理根", this);
            wiringOk = false;
        }
        if (_rb == null)
        {
            Debug.LogError("Rigidbody2D 没找到：巡逻需要刚体交速度", this);
            wiringOk = false;
        }
        if (edgeCheck == null)
        {
            Debug.LogError("EdgeCheck 没找到：悬空预感需要探针", this);
            wiringOk = false;
        }
        if (groundLayer == 0)
        {
            Debug.LogError("GroundLayer 没勾：悬空预感需要地面掩码", this);
            wiringOk = false;
        }
        else if ((groundLayer & (1 << gameObject.layer)) != 0)
        {
            Debug.LogError("GroundLayer 勾了 Enemy 层：悬空预感会永远命中自己 → 永不转身", this);
            wiringOk = false;
        }
        // TODO ⑤：照上面几条的写法，给 obstacleLayer 补一条【空槽自检】
        //   想清楚"忘了勾会怎样"：敌人会看不见任何墙 → 一路走到底 → 掉出平台
        //   ⚠️ 报错文案要写清"缺哪个槽 + 后果"（照上面两条的句式）
        if(obstacleLayer.value == 0)
        {
            Debug.LogError("obstacleLayer 一个层都没勾：撞墙预感看不到任何墙 → 敌人会一路走到底 → 掉出平台", this);
            wiringOk = false;
        }
        if (playerLayer.value == 0)
        {
            Debug.LogError("playerLayer 一个层都没勾：撞到玩家不会上报死亡", this);
            wiringOk = false;
        }

        if (!wiringOk) { enabled = false; return; }
    }

    void FixedUpdate()
    {
        // ① 把探针搬到"当前朝向的前方"
        // ✅ 覆盖式：每步都用【不变的基准 × _dir】重新算一次（别改 _edgeCheckHome 本身！）
        edgeCheck.localPosition = new Vector3(_edgeCheckHome.x * _dir, _edgeCheckHome.y, _edgeCheckHome.z);

        // ② 观测：同一物理步把三个传感器一起读
        //    · 脚前方的地：返回 null = 前方没地
        bool isGroundAhead = Physics2D.OverlapCircle(edgeCheck.position, checkRadius, groundLayer);

        //    · 身体高度正前方的障碍（撞墙预感）
        //      TODO ⑥：把这一行的掩码换成你新加的那个字段
        //        ⭐ 判据：这一行问的是"前面有没有【挡我的东西】"，不等于"脚下有没有地" ——
        //          换完之后：地形（Ground）照旧会挡，而"只挡人、不站人"的东西（Hazard 层，如终点）也会被提前看到
        bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(_dir, 0f), wallCheckDistance, obstacleLayer);

        //    · 🆕 视线：与"看墙"同一个 API 家族，只换【掩码】+ 独立【距离】
        //      · 掩码只勾 Player → 不会命中自己（探针那种"命中自己"的防线这里天然不需要）
        //      · ⚠️ 局限：射线只探【一条水平线】→ 玩家跳高就"看不见"（已登记回访清单）
        _seesPlayer = Physics2D.Raycast(transform.position, new Vector2(_dir, 0f), sightDistance, playerLayer);

        // ③ 决策：转头（规则【不变】—— 悬崖 / 墙都要回头，追逐时也一样，别让自己冲出平台）
        if (!isGroundAhead || wallAhead) _dir = -_dir;
        ApplyFacing();

        // ④ 交速度：⭐【覆盖式重算】，不是累加
        //    · 判据：累加式的症状 = "离开视野后不减速"（速度只会涨，没人减回去）
        //    · 覆盖式 = 每步都从 moveSpeed 这个【基准】重新算一次 → 视野一丢自动回落
        float speed = _seesPlayer ? moveSpeed * chaseMultiplier : moveSpeed;
        _rb.linearVelocityX = _dir * speed;
    }

    // ── ③ 撞到"别的物体"也转身（敌人 / 玩家）──
    //    · 判据：用【接触法线】区分两种接触 —— 竖直法线(|y|大) = 站在地上；水平法线(|x|大) = 顶到东西
    //    · ⚠️ 它【不覆盖墙】：地形是合并碰撞体，脚踩地时已经处在接触中 → Enter 不触发（墙交给 ②）
    //    · ⚠️ 别给这里加"只放行 Ground 层"的门卫 —— 那会把"撞敌人 / 撞玩家回头"一起挡掉
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 🆕 ① 伤害上报：撞到【玩家】→ 走与掉落【同一个出口】
        //    判据：致命源只负责"喊一声"，"决定死"永远是裁判的事（唯一出口）
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            GameManager.Instance?.NotifyPlayerLost();
        }

        // ② 原有：撞到东西就转身（水平法线）
        foreach (ContactPoint2D c in collision.contacts)
        {
            if (Mathf.Abs(c.normal.x) > 0.7f) { _dir = -_dir; return; }
        }
    }


    // 转身：只改视觉子树 x 的【符号】，幅值保持不变
    //   脏检查：没变就不写（与 ScoreManager 同族 —— 别每帧无脑写渲染/变换属性）
    private void ApplyFacing()
    {
        Vector3 s = visual.localScale;
        float want = Mathf.Abs(s.x) * _dir;
        if (Mathf.Approximately(s.x, want)) return;
        s.x = want;
        visual.localScale = s;
    }

    // ── ④ 可视化（复用玩家那套：只在选中该物体时画）──
    void OnDrawGizmosSelected()
    {
        if (edgeCheck == null) return;

        Gizmos.color = Color.magenta;                              // 品红：悬空探针（和玩家的青区分）
        Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);

        Gizmos.color = Color.yellow;                               // 黄：撞墙射线
        Gizmos.DrawRay(transform.position, new Vector3(_dir, 0f, 0f) * wallCheckDistance);

        Gizmos.color = _seesPlayer ? Color.red : new Color(1f, 0.5f, 0f);   // 🆕 视线：看到=红 / 没看到=橙
        Gizmos.DrawRay(transform.position, new Vector3(_dir, 0f, 0f) * sightDistance);

    }
}
