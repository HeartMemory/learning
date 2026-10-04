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

    private Rigidbody2D _rb;
    private Vector3 _edgeCheckHome;    // 探针"朝右"时的本地位（x = 0.625）—— 转身只翻 x 符号
    private int _dir = 1;              // ⭐ 给初值：编辑模式下 Awake 不跑，射线的 Gizmos 才画得出来

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _dir = startDir >= 0 ? 1 : -1;
        if (edgeCheck != null) _edgeCheckHome = edgeCheck.localPosition;   // ⭐ 记一次

        // TODO 接线自检（照 PlayerController 的风格：收集全部问题 → enabled = false）
        bool wiringOk = true;

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

        if (!wiringOk) { enabled = false; return; }
    }

    void FixedUpdate()
    {
        // ① 把探针搬到"当前朝向的前方"
        // ✅ 覆盖式：每步都用【不变的基准 × _dir】重新算一次（别改 _edgeCheckHome 本身！）
        edgeCheck.localPosition = new Vector3(_edgeCheckHome.x * _dir, _edgeCheckHome.y, _edgeCheckHome.z);

        // ② 观测：同一帧的两个传感器一起读
        //    · 脚前方的地：返回 null = 前方没地
        bool isGroundAhead = Physics2D.OverlapCircle(edgeCheck.position, checkRadius, groundLayer);

        //    · 身体高度正前方的障碍（撞墙预感）
        //      TODO 为什么从【身体中心】发：地面在脚下，不在这个高度的水平路径上
        //           → 天然只探到"墙 / 台阶"，不会把地面误判成墙
        //      TODO 为什么掩码只勾 Ground：敌人自己（Enemy 层）不会被命中 —— 顺便防自伤
        bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(_dir, 0f), wallCheckDistance, groundLayer);

        // ③ 决策（一步只做一次）：任一条成立就转身
        //    ⭐ 自带闸门，不会每物理步乱翻：转身后探针搬到另一侧、射线也指向反方向 → 条件自动消失
        if (!isGroundAhead || wallAhead) _dir = -_dir;

        // ④ 交速度：x 归脚本（覆盖式）、y 归重力（原样保留）
        _rb.linearVelocityX = _dir * moveSpeed;
    }

    // ── ③ 撞到"别的物体"也转身（敌人 / 玩家）──
    //    · 判据：用【接触法线】区分两种接触 —— 竖直法线(|y|大) = 站在地上；水平法线(|x|大) = 顶到东西
    //    · ⚠️ 它【不覆盖墙】：地形是合并碰撞体，脚踩地时已经处在接触中 → Enter 不触发（墙交给 ②）
    //    · ⚠️ 别给这里加"只放行 Ground 层"的门卫 —— 那会把"撞敌人 / 撞玩家回头"一起挡掉
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D c in collision.contacts)
        {
            if (Mathf.Abs(c.normal.x) > 0.7f) { _dir = -_dir; return; }
        }
    }

    // ── ④ 可视化（复用玩家那套：只在选中该物体时画）──
    void OnDrawGizmosSelected()
    {
        if (edgeCheck == null) return;

        Gizmos.color = Color.magenta;                              // 品红：悬空探针（和玩家的青区分）
        Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);

        Gizmos.color = Color.yellow;                               // 黄：撞墙射线
        Gizmos.DrawRay(transform.position, new Vector3(_dir, 0f, 0f) * wallCheckDistance);
    }
}
