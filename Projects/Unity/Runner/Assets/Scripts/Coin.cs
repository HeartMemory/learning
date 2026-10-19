using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  Coin —— 金币（Runner · 2026-10-19）
//  🪞 骨架照 Platformer/Coin.cs：门卫 + 上报 + `SetActive(false)`
//  🆕 新增两样**纯表现**：原地绕世界 Y 轴旋转 + 轻微上下浮动
//
//  ⭐ 判据：
//     · 门卫用**组件判定**（本工程没有 Player 层；照抄层判定会"永远吃不到且不报错"）
//     · `?.` 容错：忘放 ScoreManager 时照样能被吃（只是不加分）
//     · 用 `SetActive(false)` 而不是 `Destroy`（承对象池思想）
//     · 触发器消息要求两边至少一个带刚体 ⇒ 玩家是 Dynamic 刚体，已满足 ⇒ 金币不用刚体
//
//  ⚠️ 与对象池配合的**唯一陷阱**：基准位置必须"等摆放完再记"
//     Get 的顺序是：`SetActive(true)`（触发 OnEnable）→ **然后** Spawner 才设 position。
//     若在 OnEnable 里记基准，记到的是【上一轮的旧位置】⇒ 浮动会围着错误的高度跳。
//     ⇒ 所以这里用【懒初始化】：第一帧 Update 时才记 `_base`，
//       并在 `OnDisable`（归还池时）把标记清掉，下次 Get 重新记。
// ══════════════════════════════════════════════════════════════
public class Coin : MonoBehaviour
{
    [Header("表现（纯视觉，不影响判定）")]
    [SerializeField] private float rotateSpeed = 180f;      // 度/秒，绕世界 Y 轴
    [SerializeField] private float floatAmplitude = 0.08f;  // 上下浮动的幅度（米）——"轻微"
    [SerializeField] private float floatSpeed = 2f;         // 浮动快慢（弧度/秒）

    private Vector3 _base;      // 这一轮的基准位置（懒初始化）
    private bool _inited;

    private void OnDisable()
    {
        _inited = false;        // 归还池 ⇒ 下一轮重新记基准
    }

    private void Update()
    {
        if (!_inited)
        {
            _base = transform.position;   // ⭐ 此刻 Spawner 已经把位置摆好了
            _inited = true;
        }

        // 旋转：绕【世界】Y 轴，不随自身姿态跑偏
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // 浮动：围绕基准 y 做正弦摆动
        // ⚠️ `Time.timeScale = 0` 时 `Time.deltaTime` = 0、`Time.time` 冻结 ⇒ 旋转/浮动自动停下
        float y = _base.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(_base.x, y, _base.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 门卫：碰到的是不是"带 PlayerController 的那个"？
        if (other.GetComponent<PlayerController>() == null) return;

        ScoreManager.Instance?.AddCoin(1);

        gameObject.SetActive(false);
    }
}
