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
        _rb.linearVelocity = new Vector2(0f, 1f).normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // ① 门卫：不是挡板就滚蛋（判定方式跟 Brick 保持一致）

        if (!other.gameObject.CompareTag("Paddle")) return;

        // ② 取击打点偏移（选 contacts 还是球心，先想清楚再写）
        //    float offset = ...;

        float offset = GetHitOffset(other);


        // ③ 归一化：除什么？(提示：挡板的【半宽】)
        //    float t = offset / 半宽;
        //    t = Mathf.Clamp(t, -1f, 1f);

        float t = offset / (other.transform.localScale.x * 0.25f);
        t = Mathf.Clamp(t,-2f,2f);

        // ④ 覆盖速度方向
        //    Vector2 dir = new Vector2(t, 1f).normalized;
        //    _rb.linearVelocity = dir * speed;
        //    ★ 注意：这里乘的必须是【速率】(标量)，不是原来的速度向量
        Vector2 dir = new Vector2(t, 1f).normalized;
        _rb.linearVelocity = dir * speed;
    }

    private float GetHitOffset(Collision2D other)
    {
        if (other.contactCount == 0) return 0f;
        ContactPoint2D contact = other.GetContact(0);
        return contact.point.x - other.transform.position.x;
    }
}
