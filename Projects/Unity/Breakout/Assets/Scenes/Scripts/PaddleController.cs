using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;   // 每秒移动多少单位
    private Rigidbody2D _rb;
    private float _axis;                         // Update 写、FixedUpdate 读

    void Awake()
    {
        // 取自身这个 GameObject 上的 Rigidbody2D 组件（找不到返回 null）
        // 放 Awake：生命周期最早的一批，只跑一次，保证后面用到它时已经接好
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 输入在 Update 里读（每帧一次）：按 ← 得 -1、→ 得 +1、不按回 0
        // 用 GetAxisRaw：只给 -1 / 0 / +1，松手立刻归零 → 挡板"指哪打哪"、不滑行
        //   （GetAxis 是平滑值，松手后按该轴 Gravity=3 约 0.33 秒才回落到 0，会有缓动滑行感；要那种手感时才用它）
        _axis = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        // 物理时钟驱动（默认 50Hz）：先算"这一步该到哪儿"，再提交给物理引擎去搬
        //   起点用 _rb.position（物理引擎账本里的位置，Vector2），不用 transform.position
        //   位移 = 方向(_axis) × 速度(speed) × 物理步长(fixedDeltaTime)
        //   注意：next 只是"想去的目标位置"，真正的移动要靠 Rigidbody2D 的 MovePosition 提交
        Vector2 next = _rb.position + new Vector2(_axis * speed * Time.fixedDeltaTime, 0f);
        _rb.MovePosition(next);
    }
}
