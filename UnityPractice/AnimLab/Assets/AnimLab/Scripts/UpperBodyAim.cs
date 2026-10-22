using UnityEngine;

// ⚠️ 新输入系统：本工程是 New Input System only，
//    旧 API（Input.GetMouseButton 等）会直接抛 InvalidOperationException
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 上半身动作层控制：按住鼠标右键 → UpperBody 层权重淡入到 1；松开 → 淡出到 0。
/// 挂在 PlayerArmature 上（必须与 Animator 在同一个 GameObject）。
/// </summary>
/// <remarks>
/// 判据（写完拿它对，不通过就是错）：
/// ① 按住右键后 0.1~0.3 秒上半身抬起来 —— 不能瞬间跳变
/// ② 松开后 0.2~0.4 秒回到跑步摆臂
/// ③ 不按右键时 Animator.GetLayerWeight(1) == 0（这层完全不参与，Base 层正常摆臂）
/// ④ 输入必须走新输入系统（Mouse.current），旧 Input API 会抛异常
/// ⑤ 平滑用 Mathf.MoveTowards（匀速推进到目标）；用 Lerp(a,b,dt*k) 会永远逼近不到
/// </remarks>
[RequireComponent(typeof(Animator))]
public class UpperBodyAim : MonoBehaviour
{
    [Header("层")]
    [Tooltip("UpperBody 层在 Animator 图层面板里的序号（Base Layer 是 0，UpperBody 是 1）")]
    [SerializeField] private int _upperBodyLayer = 1;

    [Header("淡入 / 淡出速度（权重每秒变化量；6 表示约 1/6 秒从 0 到 1）")]
    [SerializeField] private float _fadeInSpeed = 6f;
    [SerializeField] private float _fadeOutSpeed = 4f;

    private Animator _animator;
    private float _currentWeight;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _currentWeight = 0f;
    }

    private void Update()
    {
        // TODO ② 读输入：鼠标右键是否按住
        Mouse mouse = Mouse.current;
        bool isHolding = mouse != null && mouse.rightButton.isPressed;
        // TODO ③ 目标权重 = 按住 ? 1 : 0，并按淡入/淡出速度朝目标推进 _currentWeight
        float targetWeight = isHolding ? 1f : 0f;
        _currentWeight = Mathf.MoveTowards(_currentWeight, targetWeight, Time.deltaTime * (isHolding ? _fadeInSpeed : _fadeOutSpeed));
        // TODO ④ 写回层权重
        _animator.SetLayerWeight(_upperBodyLayer, _currentWeight);
    }
}
