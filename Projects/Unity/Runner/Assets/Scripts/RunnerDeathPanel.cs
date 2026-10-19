using TMPro;
using UnityEngine;
using UnityEngine.UI;        // Button

// ══════════════════════════════════════════════════════════════
//  RunnerDeathPanel —— 结算面板的"开关"（Runner · 2026-10-19）
//  🪞 照 Platformer/DeathPanel.cs，**砍掉"赢"那一整组**（本工程只有"死"）
//
//  ⭐ 判据（照镜子）：
//     · 脚本**不挂在 Overlay 自己身上**：Overlay 初始 SetActive(false) 时，
//       它自己的 Awake/OnEnable 不会执行 ⇒ 永远订阅不上事件。⇒ 挂在常驻的 Canvas 上
//     · 槽位缺一个就"响亮地失败"（红字 + return），且场景里 Overlay **初始开着**
//       ⇒ 一 Play 就盖在屏幕上，不用翻控制台也看得见
//     · 订阅/退订成对（OnEnable / OnDisable）
//     · ⭐ `Time.timeScale = 0` **不影响 UGUI**（按钮照样能点）——
//       这正是"冻结世界"与"结算界面可交互"能并存的原因
// ══════════════════════════════════════════════════════════════
public class RunnerDeathPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;   // 装配期接线
    [SerializeField] private GameObject overlayRoot;    // 拖 Overlay（含 Image + CanvasGroup + 结果文本 + 按钮）
    [SerializeField] private TMP_Text resultText;       // 拖 Overlay 里的 TMP 文本
    [SerializeField] private Button restartButton;      // 拖 Overlay 里的 RESTART 按钮

    [Header("渐入")]
    [SerializeField] private float fadeDuration = 0.3f; // 淡入时长（秒）；0 = 直接出现

    private CanvasGroup _group;      // Overlay 上的 CanvasGroup（Alpha 靠它）
    private float _fadeTimer;
    private bool _fading;

    private void Awake()
    {
        if (gameManager == null) { Debug.LogError("gameManager 槽是空的：面板永远不会出现", this); return; }
        if (overlayRoot == null) { Debug.LogError("overlayRoot 槽是空的：没东西可打开", this); return; }
        if (resultText == null) { Debug.LogError("resultText 槽是空的：没东西可写", this); return; }
        if (restartButton == null) { Debug.LogError("restartButton 槽是空的：结束了没出口", this); return; }

        // ⭐ 渐入靠 CanvasGroup.Alpha（Image 的 color.a 也能淡，但 CanvasGroup 是"整块一起淡"，
        //    子物体（文本 / 按钮）不用逐个改材质，这也是 Container 存在的理由）
        _group = overlayRoot.GetComponent<CanvasGroup>();
        if (_group == null)
        {
            Debug.LogError("Overlay 上没有 CanvasGroup：无法渐入（会变成直接出现）", this);
        }

        overlayRoot.SetActive(false);   // 四道防线全过 = 自检信号
    }

    private void OnEnable()
    {
        if (gameManager != null) gameManager.OnGameOver += Show;
        if (gameManager != null && restartButton != null) restartButton.onClick.AddListener(gameManager.Restart);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnGameOver -= Show;
        if (gameManager != null && restartButton != null) restartButton.onClick.RemoveListener(gameManager.Restart);
    }

    private void Update()
    {
        if (!_fading || _group == null) return;

        // ⚠️ 必须用 **unscaledDeltaTime**：结束那一刻 `timeScale = 0`，
        //    用 `Time.deltaTime` 会恒等于 0 ⇒ 永远淡不进来（Breakout 那条"结算动画要用 Unscaled"的翻版）
        _fadeTimer += Time.unscaledDeltaTime;
        float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(_fadeTimer / fadeDuration);
        _group.alpha = t;

        if (t >= 1f) _fading = false;
    }

    private void Show()
    {
        // ⚠️ 只用英文/数字（TMP 默认字体无中文字形）
        resultText.text = "GAME OVER";

        // ⭐ 渐入三件套：先归零 → 打开 → 交给 Update 往上涨
        _fadeTimer = 0f;
        if (_group != null) _group.alpha = 0f;
        overlayRoot.SetActive(true);
        _fading = true;
    }
}
