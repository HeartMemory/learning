using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;   // 面值放检查器可调（09-18 学过的理由）

    private Rigidbody2D _rb;
    private InputAction _move;
    private float _axis;                              // Update 写、FixedUpdate 读（与挡板同款）

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // 与 Breakout 挡板同一套：项目级输入资产（Player 地图 / Move 动作），只在 Awake 查一次
        _move = InputSystem.actions.FindActionMap("Player").FindAction("Move");

        // ★ 探针：先确认"项目级 Actions 接上了"，再往下写
        Debug.Log($"项目级 Actions 已接上？ → {InputSystem.actions}", this);
    }

    void Update()
    {
        _axis = _move.ReadValue<Vector2>().x;         // 输入的"当前意图"，每帧拉一次
    }

    void FixedUpdate()
    {
        // ★ 关键：只覆盖 x，y 原样保留（y 归重力管）
        _rb.linearVelocity = new Vector2(_axis * moveSpeed, _rb.linearVelocity.y);
    }
}
