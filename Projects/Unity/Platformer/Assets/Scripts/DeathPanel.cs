using TMPro;                 // ★ TMP 在 TMPro 命名空间（承 ScoreManager）
using UnityEngine;
using UnityEngine.UI;        // Button 在这里（UGUI）

// ══════════════════════════════════════════════════════════════
//  DeathPanel —— 结算面板的"开关"（挂在常驻的 Canvas 上）
//  🆕 10-07：由"只服务【死】"升级为"【死】/【赢】共用一个入口" —— 同一块 Overlay，两条路进来
//  ⭐ 为什么脚本不挂在 Overlay 自己身上：Overlay 初始 SetActive(false) 时，
//     它身上的 Awake/OnEnable 不会执行 → 永远订阅不上事件
//  🆕 10-08：多关卡 —— 面板上多一颗「下一关」按钮（它只对【赢】有意义）
//     按钮按"对谁有意义"【分组】：DeadButtons（死：只有 RESTART，居中）
//                                WinButtons（赢：RESTART 在左 + NEXT 在右）
//     ⭐ 于是"死时居中、赢时分侧"这个【布局问题】被消化成了【显隐问题】—— 代码只管两组的开/关
// ══════════════════════════════════════════════════════════════
public class DeathPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;   // 装配期接线
    [SerializeField] private GameObject overlayRoot;    // 拖 Overlay 进来
    [SerializeField] private TMP_Text resultText;       // 拖 Canvas/Overlay/「GameOver」那个 TMP 文本
    [SerializeField] private Button nextLevelButton;    // 拖 Overlay/WinButtons 里那颗「下一关」按钮（⚠️ 只用于【接线】，显隐交给 winButtons）

    // ═══ 🆕 10-08（方案 B）：按钮按"对谁有意义"【分组】 ═══
    [SerializeField] private GameObject deadButtons;    // 拖 Overlay/DeadButtons（"死"时显示那组：只有 RESTART，居中）
    [SerializeField] private GameObject winButtons;     // 拖 Overlay/WinButtons（"赢"时显示那组：RESTART 在左 + NEXT 在右）

    private void Awake()
    {
        // ⭐ 空槽防线：这四样是面板的"命门"，缺一样就【停摆】（红字 + return）
        //    ⚠️ "停摆"的代价 = 【整个面板都不工作】—— 之所以接受，是因为它们都属于
        //       "少了它面板就是废的"（不订阅 / 打不开 / 没字 / 赢了没出口）。
        //    ⭐ 配套的"让失败可见"（10-08 定）：场景里 Overlay 初始【开着】（m_IsActive = 1）——
        //       只要有谁 return 了，最后那句 SetActive(false) 就不会执行
        //       ⇒ **一 Play 面板就盖在屏幕上** —— 不用翻控制台也看得见。
        if (gameManager == null) { Debug.LogError("gameManager 槽是空的：面板永远不会出现", this); return; }
        if (overlayRoot == null) { Debug.LogError("overlayRoot 槽是空的：没东西可打开", this); return; }
        if(resultText == null) { Debug.LogError("resultText 槽是空的：没东西可写", this); return; }
        if(nextLevelButton == null) { Debug.LogError("nextLevelButton 槽是空的：赢了之后没出口", this); return; }

        overlayRoot.SetActive(false);   // ⭐ 这一句现在【有职责】了：它同时是"四道防线全过"的自检信号
    }

    private void OnEnable()
    {
        // 标准 C# 事件：谁订阅谁退订
        if (gameManager != null) gameManager.OnGameOver += Show;
        if (gameManager != null) gameManager.OnGameWin += ShowWin;

        // ⭐ 「下一关」按钮的接线：UGUI 的 onClick 也是一个事件，规矩一字不改（谁挂谁摘）
        //    ⚠️ 这里【故意不先 RemoveListener】：
        //       OnEnable / OnDisable 由 Unity 保证【成对】调用 ⇒ 真出现"重复挂"就说明成对性被破坏了，
        //       那是 bug，应当让它【暴露】（症状 = 点一下跳两关），而不是用"先摘"把它拍平。
        //    ⭐ 判据：防御代码必须【可观测】—— 静默地拍平异常状态 = 掩盖 bug。
        if(nextLevelButton != null) nextLevelButton.onClick.AddListener(gameManager.LoadNextLevel);
    }
    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnGameOver -= Show;
        if (gameManager != null) gameManager.OnGameWin -= ShowWin;

        // 摘掉 —— 与上面那条【配对】（同一个方法组的挂/摘必须在同一对生命周期里成对出现）
        if(nextLevelButton != null) nextLevelButton.onClick.RemoveListener(gameManager.LoadNextLevel);
    }

    private void Show()          // 死
    {
        resultText.text = "YOU DIED";

        // ⭐ 切到"死"那一组：两句都写【绝对值】（谁必须开 / 谁必须关）
        //    ⚠️ 别写"只改变动的那一个" —— 那会依赖上一次的状态（增量式写法），迟早静默错位
        //    （布局已经在场景里分好组了 ⇒ 代码这边只剩显隐）
        deadButtons.SetActive(true);
        winButtons.SetActive(false);

        overlayRoot.SetActive(true);   // ⭐ 一句管三样：Image + CanvasGroup + Animator 一起上线
        // Animator 无参数 → 激活时自动播默认状态 → 淡入自己开始
    }

    private void ShowWin()       // 赢
    {
        resultText.text = "YOU WIN";

        // ⭐ 与 Show() 是【一对】：同样两句、取值相反 —— 两个入口都要写【完整】
        //    （只写"变动的那一个" = 依赖上一次状态 = 增量式）
        deadButtons.SetActive(false);
        winButtons.SetActive(true);

        overlayRoot.SetActive(true);
    }
}
