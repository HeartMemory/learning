using TMPro;    // ★ TMP 在 TMPro 命名空间（承 ScoreManager）
using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  DeathPanel —— 结算面板的"开关"（挂在常驻的 Canvas 上）
//  🆕 10-07：由"只服务【死】"升级为"【死】/【赢】共用一个入口" —— 同一块 Overlay，两条路进来
//  ⭐ 为什么脚本不挂在 Overlay 自己身上：Overlay 初始 SetActive(false)
//      → 它身上的 Awake/OnEnable 不会执行 → 永远订阅不上事件
// ══════════════════════════════════════════════════════════════
public class DeathPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;   // 装配期接线
    [SerializeField] private GameObject overlayRoot;    // 拖 Overlay 进来
    [SerializeField] private TMP_Text resultText;       // 🆕 TODO ⑥：拖 Canvas/Overlay/「GameOver」那个 TMP 文本

    private void Awake()
    {
        if (gameManager == null) { Debug.LogError("gameManager 槽是空的：面板永远不会出现", this); return; }
        if (overlayRoot == null) { Debug.LogError("overlayRoot 槽是空的：没东西可打开", this); return; }
        // TODO ⑦：照上面两条补一条 resultText 的空槽防线
        //   少了它会怎样？——"面板弹出来了，但一个字都没有"，查半天
        if(resultText == null) { Debug.LogError("resultText 槽是空的：没东西可写", this); return; }
        overlayRoot.SetActive(false);                   // 保险：确保初始是关的
    }

    // TODO ⑧：OnEnable / OnDisable 各补一行"赢"的订阅 / 退订
    //   ⭐ 谁订阅谁退订；⚠️ 别把第二行写成 `=`（写 = 会把"死"那条订阅直接顶掉）
    private void OnEnable()
    {
        if (gameManager != null) gameManager.OnGameOver += Show;
        if (gameManager != null) gameManager.OnGameWin += ShowWin;
    }
    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnGameOver -= Show;
        if (gameManager != null) gameManager.OnGameWin -= ShowWin;
    }
    private void Show()
    {
        // TODO ⑨：先把 resultText 的文字写成【死】的那一句（写死字面量就够，不必做成可配置）
        resultText.text = "YOU DIED";
        overlayRoot.SetActive(true);   // ⭐ 一句管三样：Image + CanvasGroup + Animator 一起上线
        // Animator 无参数 → 激活时自动播默认状态 → 淡入自己开始
    }

    // TODO ⑩：新增 ShowWin() —— 与 Show() 只差一行文本，overlayRoot / 激活方式全部一样
    //   自检：为什么【不】再复制一块 Overlay 出来？（"两个入口 → 一块面板"省掉了什么？）
    private void ShowWin()
    {
        resultText.text = "YOU WIN";
        overlayRoot.SetActive(true);
    }
}
