using UnityEngine;
using UnityEngine.Tilemaps;   // 🆕 10-08：关卡边界改从 Tilemap 自己量

// ══════════════════════════════════════════════════════════════
//  CameraFollow —— 相机跟随（10-07 建 · 10-08 升级）
//  ⭐ 升级 1：不再手填 minX/maxX —— 相机自己量 Tilemap 的范围来算夹取边界
//     （手填的常量会被"复制场景"一起带走 ⇒ 新关卡变长后相机卡住，还不报错）
//  ⭐ 升级 2：构图 + 手感 —— yBias（角色偏下）/ 阻尼 / 死区 / 前瞻
//     · yBias 依据：Cinemachine `Screen Y` 官方示范 0.4（角色偏下）；攀爬类推荐偏下 15%~25%
//     · 阻尼 依据：官方 `Lookahead Ignore Y` 那条注明"2D 侧视常用，避免跳跃时上下颠簸"
//                  ⇒ 纵向跟随时每跳一下相机都会动，**阻尼是必需品**
//  ⭐ 统一判据（x、y 同一套）：
//     「关卡在某个方向上【装不进】视野 ⇒ 跟随 + 夹取；【装得进】⇒ 固定」
// ══════════════════════════════════════════════════════════════
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;      // 拖 Player
    [SerializeField] private float fixedY = 0f;     // ⭐ y 装得下时固定到这个高度（Level1/2 用）

    [Header("关卡边界（留空 = 自动找场景里的 Tilemap）")]
    [SerializeField] private Tilemap levelBounds;

    [Header("兜底：找不到 Tilemap 时才用")]
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 12f;

    [Header("构图：y 装不下时让角色偏下（= 相机比角色高）")]
    [SerializeField] private float yBias = 2f;      // 2 格 / 视野 16 格 = 12.5% ⇒ 角色在屏幕约 37.5% 处
                                                    // ⚠️ 别超 4 格（25%）⇒ 再多就看不到脚下

    [Header("手感")]
    [SerializeField] private float smoothTime = 0.15f;     // 阻尼：追上目标约需几秒（越小越硬跟）
    [SerializeField] private float deadZoneX = 0.5f;       // 死区：目标与相机差得比它小 ⇒ 相机不动
    [SerializeField] private float deadZoneY = 0.5f;
    [SerializeField] private float lookAheadX = 2f;        // 前瞻：全速时最多朝运动方向多看几格
    [SerializeField] private float lookAheadAtSpeed = 6f;  // 达到满前瞻所需速度（= moveSpeed）

    private Camera _cam;
    private Rigidbody2D _targetRb;
    private Vector3 _velocity;    // SmoothDamp 的速度缓存（它自己维护）
    private Vector3 _goal;        // ⭐ 本帧目标点：夹取 / 死区都作用在它上面
    private bool _snapped;        // 开场第一帧直接吸附，免得从旧位置滑过来

    private void Awake()
    {
        // ── 接线自检：一次报全，再统一停手（承 10-06）──
        bool ok = true;

        if (target == null) { Debug.LogError("target 槽是空的：相机永远不会跟随", this); ok = false; }

        _cam = GetComponent<Camera>();
        if (_cam == null) { Debug.LogError("没有 Camera 组件：算不出视野多大", this); ok = false; }

        // ⭐ 留空 = 自动找场景里唯一的 Tilemap（显式拖进来更好，一眼看得出依赖谁）
        if (levelBounds == null)
        {
            levelBounds = FindFirstObjectByType<Tilemap>();
            if (levelBounds == null)
                Debug.LogWarning("没找到 Tilemap ⇒ 退回手填的 minX/maxX；关卡变长时相机会卡住", this);
        }

        // ⭐ 前瞻要读角色速度；拿不到就自动跳过这一项（不是错误）
        if (target != null)
        {
            _targetRb = target.GetComponent<Rigidbody2D>();
            if (_targetRb == null)
                Debug.LogWarning("角色没有 Rigidbody2D ⇒ 前瞻自动跳过", this);
        }

        // ⭐ 必须用【当前位置】初始化：否则死区会从 (0,0,0) 起算，开场猛地跳一下
        _goal = transform.position;

        if (!ok) enabled = false;
    }

    // ⭐ 为什么是 LateUpdate：同一帧里 target 先动完，才轮到相机（"谁先动、谁后动"）
    //updata先动
    private void LateUpdate()
    {
        // ── ① 视野多大（每帧算：窗口一拉 aspect 就变，夹取边界要跟着变）──
        float halfH = _cam.orthographicSize;              // 半高 = 正交尺寸
        float halfW = halfH * _cam.aspect;                // 半宽 = 半高 × 宽高比

        // ── ② 关卡边界（世界坐标）──
        //    localBounds 是【局部】空间（且已含砖块边缘）⇒ 用 Tilemap 自己的 transform 转世界
        float left, right, bottom, top;
        if (levelBounds != null)
        {
            Bounds b = levelBounds.localBounds;
            Transform lt = levelBounds.transform;
            left   = lt.TransformPoint(new Vector3(b.min.x, 0f, 0f)).x;
            right  = lt.TransformPoint(new Vector3(b.max.x, 0f, 0f)).x;
            bottom = lt.TransformPoint(new Vector3(0f, b.min.y, 0f)).y;
            top    = lt.TransformPoint(new Vector3(0f, b.max.y, 0f)).y;
        }
        else
        {
            // 兜底：只跟 x；y 组成"装得下"的假区间 ⇒ 下面必然走 fixedY 分支
            left = minX - halfW;  right = maxX + halfW;
            bottom = top = fixedY;
        }

        // ── ③ x：前瞻 + 夹取；装得下就居中 ──
        float look = 0f;
        if (_targetRb != null)
            look = Mathf.Clamp(_targetRb.linearVelocity.x / lookAheadAtSpeed, -1f, 1f) * lookAheadX;

        if (right - left > halfW * 2f)
            _goal.x = Mathf.Clamp(target.position.x + look, left + halfW, right - halfW);
        else
            _goal.x = (left + right) * 0.5f;   // ⭐ 硬夹会出现"下界 > 上界"⇒ 装得下时改用居中

        // ── ④ y：偏下 + 夹取；装得下就 fixedY（🏯 爬塔关靠这条开始纵向跟随）──
        if (top - bottom > halfH * 2f)
            _goal.y = Mathf.Clamp(target.position.y + yBias, bottom + halfH, top - halfH);
        else
            _goal.y = fixedY;                  // ⭐ 关卡竖着也装得下 ⇒ 保持原来的固定高度

        // ── ⑤ 死区：目标离相机太近 ⇒ 吸附到相机位置（画面彻底静止）──
        //    ⚠️ 取舍：贴着关卡边缘时最多会"差 deadZone 格就停住"—— 这是消抖的代价（0.5 格肉眼看不出）
        if (Mathf.Abs(_goal.x - transform.position.x) < deadZoneX) _goal.x = transform.position.x;
        if (Mathf.Abs(_goal.y - transform.position.y) < deadZoneY) _goal.y = transform.position.y;
        _goal.z = transform.position.z;        // z 保持不变

        // ── ⑥ 平滑逼近目标 ──
        //    · SmoothDamp 自带速度缓存 ⇒ 帧率无关（比 Lerp 稳）
        //    · 用 Time.deltaTime ⇒ 游戏结束时 timeScale=0，相机会一起冻结（正好）
        if (!_snapped) { transform.position = _goal; _snapped = true; }   // 开场直接吸附
        else transform.position = Vector3.SmoothDamp(transform.position, _goal, ref _velocity, smoothTime);
    }
}
