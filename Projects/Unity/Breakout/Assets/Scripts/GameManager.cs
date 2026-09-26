using TMPro;                            // ★ 结果文本用 TMP_Text（09-23 的老朋友）
using UnityEngine;
using UnityEngine.SceneManagement;      // ★ 场景重载在 SceneManagement 里（今天的主角）

// ═══════════════════════════════════════════════════════════════════
//  GameManager —— 唯一的"裁判"（方案甲 · 状态派生）
//
//  依赖方向（箭头读作"知道"）：GameManager ─→ BrickSpawner（只读状态）
//                                        └─→ ScoreManager（只写显示）
//  ★ 两个小弟谁都不认识对方，也不认识裁判。全场只有裁判知道"这一局怎么样了"。
//
//  为什么分数是"派生"的：分数 = f(剩余砖块数)，不需要任何"谁打了我一块"的事件。
//  从状态重算 → 天然幂等（喊一次和喊一百次结果一样）→ 少一条链路就少一类 bug。
// ═══════════════════════════════════════════════════════════════════
public class GameManager : MonoBehaviour
{
    [SerializeField] private BrickSpawner brickSpawner;   // 读状态：还剩几块砖
    [SerializeField] private ScoreManager scoreManager;   // 写分数
    [SerializeField] private TMP_Text resultText;         // 结果提示（YOU WIN / GAME OVER）
    [SerializeField] private GameObject overlay;
    [SerializeField] AudioSource audioSource;

    private bool _isGameOver = false;             // ⭐ 今天最重要的一个字段

    // ─────────────────────────────────────────────────────────────
    // TODO 1：开局准备
    //   void Start()
    //     ① 藏起重开按钮 → restartButton.SetActive(false)
    //     ② 清空结果文本 → resultText.text = ""
    //
    //   ★ 为什么放 Start 不放 Awake？
    //     （提示：Awake 阶段场景里的其它物体可能还没就位；而"这一局开始"的语义
    //       恰好就是 Start 的含义 —— 用对生命周期钩子，代码自己会说话）
    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        overlay.SetActive(false);
    }
    // ─────────────────────────────────────────────────────────────
    // TODO 2：⭐ 每帧检查一次"这一局还在不在"
    //   void Update()
    //
    //   ① 结束了就直接 return（if (_isGameOver) return;）
    //      —— 这是【防止重复结算】的守门员。球还在飞，下一帧 RemainingBricks 还是 0，
    //         没有这一句，Win() 会被反复调用。
    //
    //   ② ★ 先加一道"还没初始化"的防线（自己想为什么需要它）★
    //      提示：_pool 是在【字段初始化器】里 new 出来的（声明那一刻就存在），
    //            而 Preload() 要等 Start 才跑。那么在 Preload 之前，
    //            `_instantiateCount - _pool.Count` 算出来是几？
    //            如果"还没摆砖"和"砖全打光了"算出同一个数，第一帧会发生什么？
    //      → 这叫"空值"与"未初始化"的混淆。想清楚该拿哪个量做守门条件。
    //
    //   ③ 把"打掉了几块"报给计分员：
    //        scoreManager.SetDestroyedCount(brickSpawner.TotalBricks - brickSpawner.RemainingBricks);
    //      ★ 为什么报"打掉几块"而不是"加 10 分"？
    //        （提示：谁该知道"一块砖值几分"？—— 分值属于【计分规则】，不属于裁判）
    //
    //   ④ 通关判定：RemainingBricks == 0 → Win()
    //
    // ⚠️ 注意 ③ 和 ④ 的顺序：先更新分数，再判通关。
    //    如果反了，最后一块砖被打掉的那一帧，分数会【停在 310】——自己想为什么。
    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (_isGameOver) return;
        if (brickSpawner == null || scoreManager == null)
        {
            Debug.LogError("GameManager 引用缺失：请在检查器里挂上 brickSpawner / scoreManager", this);
            enabled = false;        //关掉自己，否则每帧刷一条红字，把真正的错误淹掉
            return;
        }
        if (brickSpawner.TotalBricks == 0) return;
        scoreManager.SetDestroyedCount(brickSpawner.TotalBricks - brickSpawner.RemainingBricks);
        if (brickSpawner.RemainingBricks == 0) Win();
    }
    // ─────────────────────────────────────────────────────────────
    // TODO 3：三个出口，一个收尾
    //   public void OnBallLost()    ← FailZone 漏球时会喊它（★ 必须 public：跨物体调用）
    //   private void Win()
    //   private void Lose()
    //
    //   ★ 三个方法都【只做一件事】：收敛到同一个 End(message)。
    //     别把"停球 + 显示文本 + 显示按钮"抄三遍 —— 这就是错误 07（232 的 Pop/Peek 抄两遍）
    //     和"出口越多越容易漏路径"（错误 13）的同一条纪律。
    // ─────────────────────────────────────────────────────────────
    public void OnBallLost() { Lose(); }
    private void Win() { End("YOU WIN", Sfx.GameWin()); }
    private void Lose() { End("GAME OVER", Sfx.GameOver()); }
    // ─────────────────────────────────────────────────────────────
    // TODO 4：收尾
    //   private void End(string message)
    //     ① _isGameOver = true         ← 先上锁，再干别的（顺序重要吗？自己想）
    //     ② resultText.text = message
    //     ③ restartButton.SetActive(true)
    //     ④ 让场面停下来（球别再飞了）
    //
    //   ★★【今天的陷阱 · 正好接上早上那 10 分钟】★★
    //     最省事的停法是 Time.timeScale = 0。
    //     但它是一个【全局静态设置】—— 记得早上刚讲过：静态状态【不会】随场景重载复位。
    //     所以：这里设成 0 之后，重开的新一局会是【冻结的】。这笔账要在 Restart() 里还。
    //
    //     顺手回答三个问题（都能用"运行中打 Debug.Log"自己验证，别猜）：
    //       Q1：Time.timeScale = 0 之后，Update() 还会跑吗？
    //       Q2：FixedUpdate() 还会跑吗？
    //       Q3：UI 按钮还能点吗？（这个最容易想错，实测一下）
    // ─────────────────────────────────────────────────────────────
    private void End(string message, AudioClip sound)
    {
        if (_isGameOver) return;
        _isGameOver = true;
        resultText.text = message;
        overlay.SetActive(true);
        audioSource.PlayOneShot(sound);
        Time.timeScale = 0;
    }
    // ─────────────────────────────────────────────────────────────
    // TODO 5：重开
    //   public void Restart()      ← ★ 必须 public：按钮的 OnClick 在 Inspector 里
    //                                 只能选到 public 且无参（或只有一个参数）的方法
    //     ① 先还掉全局设置那笔账（Time.timeScale）
    //     ② SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //
    //   ★ 想清楚：为什么"重开"【一行都不用写】"把分数清零 / 把池重新填满 / 把球摆回起点"？
    //     （提示：那些东西的生命属于【场景】，而重开 = 换一条场景的命。
    //       早上那条账反过来用一次：它们能随场景重载复位，静态设置不能。）
    // ─────────────────────────────────────────────────────────────
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
