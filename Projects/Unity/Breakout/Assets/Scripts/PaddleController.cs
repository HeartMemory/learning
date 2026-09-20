using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;   // 每秒移动多少单位
    private Rigidbody2D _rb;
    private float _axis;                         // Update 写、FixedUpdate 读
    private float _limit;                        // x 的合法范围（半屏宽 - 半挡板宽）
    private InputAction _move;

    void Awake()
    {
        // 取自身这个 GameObject 上的 Rigidbody2D 组件（找不到返回 null）
        // 放 Awake：生命周期最早的一批，只跑一次，保证后面用到它时已经接好
        _rb = GetComponent<Rigidbody2D>();
        float halfScreen = Camera.main.orthographicSize * Camera.main.aspect;  // 屏幕【半宽】
        float halfPaddle = transform.localScale.x * 0.5f;                       // 挡板【半宽】
        _limit = halfScreen - halfPaddle;                             // x 的合法范围
        _move = InputSystem.actions.FindActionMap("Player").FindAction("Move");
    }

    void Update()
    {
        // 项目级输入资产（Player 地图 / Move 动作）：每帧"拉"一次当前值
        // Move 是 Vector2（x 左右、y 上下），挡板只需 x
        _axis = _move.ReadValue<Vector2>().x;
    }

    void FixedUpdate()
    {
        // 物理时钟驱动（默认 50Hz）：先算"这一步该到哪儿"，再提交给物理引擎去搬
        //   起点用 _rb.position（物理引擎账本里的位置，Vector2），不用 transform.position
        //   位移 = 方向(_axis) × 速度(speed) × 物理步长(fixedDeltaTime)
        //   注意：next 只是"想去的目标位置"，真正的移动要靠 Rigidbody2D 的 MovePosition 提交
        Vector2 next = _rb.position + new Vector2(_axis * speed * Time.fixedDeltaTime, 0f);
        next.x = Mathf.Clamp(next.x, -_limit, _limit);
        _rb.MovePosition(next);
    }
}
