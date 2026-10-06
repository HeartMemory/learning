using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  DeathPanel —— 死亡面板的"开关"（挂在常驻的 Canvas 上）
//  ⭐ 为什么脚本不挂在 Overlay 自己身上：Overlay 初始 SetActive(false)
//      → 它身上的 Awake/OnEnable 不会执行 → 永远订阅不上事件
// ══════════════════════════════════════════════════════════════
public class DeathPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;   // 装配期接线
    [SerializeField] private GameObject overlayRoot;    // 拖 Overlay 进来

    private void Awake()
    {
        if (gameManager == null) { Debug.LogError("gameManager 槽是空的：面板永远不会出现", this); return; }
        if (overlayRoot == null) { Debug.LogError("overlayRoot 槽是空的：没东西可打开", this); return; }
        overlayRoot.SetActive(false);                   // 保险：确保初始是关的
    }

    private void OnEnable() { if (gameManager != null) gameManager.OnGameOver += Show; }
    private void OnDisable() { if (gameManager != null) gameManager.OnGameOver -= Show; }   // 谁订阅谁退订

    private void Show()
    {
        overlayRoot.SetActive(true);   // ⭐ 一句管三样：Image + CanvasGroup + Animator 一起上线
        // Animator 无参数 → 激活时自动播默认状态 → 淡入自己开始
    }
}
