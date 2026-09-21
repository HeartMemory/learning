using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float speed = 6f;   // 面板可调
    private Rigidbody2D _rb;

    void Awake()
    {
        // TODO 1：取自己身上的 Rigidbody2D
        //   照抄 PaddleController 第 16 行那一招（GetComponent<T>()）
        //   为什么放 Awake：只跑一次，且保证 Start/回调里一定能用
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // TODO 2：发球 —— 给一个斜向初速度，只发一次，所以放 Start 不放 Update
        //   API：_rb.linearVelocity = 方向 × 速率
        //   方向怎么写：new Vector2(1f, 1f).normalized
        //   ★ 为什么必须 normalize？自己推：不归一化的话 (1,1) 的长度是 1.414，
        //     等于实际速度变成 6 × 1.414 ≈ 8.5 —— 斜着发比直着发快，轨迹会怪
        _rb.linearVelocity = new Vector2(1f, 1f).normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // TODO 3：打印"撞到谁了" + 当前速度，用来把碰撞"看见"
        //   注意参数类型是 Collision2D，不是 Collider2D（后者是触发器的回调参数）
        Debug.Log($"撞到 {other.gameObject.name}，撞后速度 {_rb.linearVelocity}，撞击相对速度 {other.relativeVelocity}");
    }
}
