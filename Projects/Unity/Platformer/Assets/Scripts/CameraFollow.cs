using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  CameraFollow —— 相机跟随（Block 3 · 2026-10-07 · 第 7 号成品）
//  为什么要它：**关卡 40 宽，视野只有 28.4** → 固定相机装不下。
//  ⭐ 马里奥的解法也是这个：**永远只显示一小窗，关卡想做多长做多长**。
//  今日取景口径（对齐马里奥）：orthographicSize = 8 → 屏幕竖着 **16 个「角色高」**
//     （马里奥 SMB 游玩区 = 竖 13 格；我们 16 格 ≈ 略远一点点，可接受）
// ══════════════════════════════════════════════════════════════
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;      // 拖 Player 进来
    [SerializeField] private float fixedY = 0f;     // ⭐ 只跟 x：关卡高 15 < 视野高 16，上下不用跟

    [Header("夹取范围（相机【中心】能到的左右界）")]
    [SerializeField] private float minX = 0f;       // = 关卡左界 + 半宽
    [SerializeField] private float maxX = 12f;      // = 关卡右界 − 半宽

    private void Awake()
    {
        // TODO ①【响亮地失败】：target 槽空着 → 喊一声 + 把自己关掉
        //   照镜子：FailZone.Awake() / DeathPanel.Awake() 的写法
        //   ⭐ 想清楚为什么必须"关掉自己"：不关的话每帧都会 NRE，一秒钟把控制台刷满
        if(target == null)
        { 
            Debug.LogError("target 槽是空的：相机永远不会跟随", this);
            enabled = false;
        }
    }

    // ⭐ 为什么是 LateUpdate，而不是 Update？
    //   TODO ② 就在这行注释下面写你的答案（提示：同一帧里"谁先动、谁后动"）
    //updata先动
    private void LateUpdate()
    {
        // TODO ③：把相机的 x 贴到 target.x → 夹取到 [minX, maxX]；y 用 fixedY；z 保持不变
        //   ⚠️ 踩坑提醒（承 10-05 的 CS1612）：transform.position 是【属性】、且返回 struct
        //      ⇒ `transform.position.x = ...` 编译不过
        //      ⇒ 必须"先取出来 → 改 → 再整体写回"
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Clamp(target.position.x, minX, maxX);
        newPos.y = fixedY;
        transform.position = newPos;
    }
}
