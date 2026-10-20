using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  SceneAudio —— 场景级音频：BGM + 失败结算音（Runner · 2026-10-20）
//
//  🪞 结构照 Platformer/SceneAudio.cs，核心那条判据原样搬过来：
//
//  ── 为什么需要它（问题起因）──
//    GameManager 结束游戏时会 `Time.timeScale = 0f` —— 那【只冻物理】。
//    ⭐ `AudioSource` 跑在【实时时钟】上，根本不吃 timeScale ⇒ BGM 会一直播下去。
//    （09-25 那条「timeScale 的影响力半径」的另一面：
//       · Animator 【吃】timeScale  ⇒ 要它动，得把 Update Mode 改成 Unscaled Time
//       · AudioSource【不吃】timeScale ⇒ 要它停，只能自己订阅"结束"）
//    ⇒ 判据：**"世界冻结"这个动作，不会替你把声音停了**
//
//  🆕 与 Platformer 的差别（Runner 只有"输"，没有"赢"）：
//    · Platformer 订三个事件：OnGameEnded（停 BGM）/ OnGameOver / OnGameWin
//    · Runner 的 GameManager 只有一个 `OnGameOver`（跑酷没有通关条件）
//    ⇒ 停 BGM 与播失败音**合并到同一个处理函数**里
//    ⚠️ 将来若加了"到终点 = 胜利"，就该拆成 OnGameEnded + OnGameWin（照 Platformer 那份）
//
//  ── 两条结构判据 ──
//    ① 【订阅放 Start，不是 OnEnable】Awake / OnEnable 的跨对象顺序不确定
//       ⇒ 那时 `GameManager.Instance` 可能还是 null；Start 在所有 Awake 之后 ⇒ 一定就位
//    ② 【两个 AudioSource 都用拖引用，不用 GetComponent】
//       同一物体上挂两个同类型组件时，`GetComponent<AudioSource>()` 返回哪一个【不确定】
//       ⇒ 凡是"系统明说结果未定义"的地方，就别让它替你做决定
// ══════════════════════════════════════════════════════════════
public class SceneAudio : MonoBehaviour
{
    [Header("接线（装配期拖引用）")]
    [SerializeField] private AudioSource bgmSource;      // BGM 那个（loop 勾上，输出 = Music 组）
    [SerializeField] private AudioSource stingerSource;   // 第 2 个（输出 = Sfx 组），专播结算音
    [SerializeField] private AudioClip loseClip;          // Sfx/Lose/GameOver_1

    [Header("BGM")]
    [SerializeField] private bool playBgmOnStart = true;

    // ── 装配期自检：响亮地失败 ──
    private void Awake()
    {
        bool wiringOk = true;

        if (bgmSource == null)
        {
            Debug.LogError("SceneAudio 的 bgmSource 槽是空的：游戏结束时 BGM 停不下来", this);
            wiringOk = false;
        }
        if (stingerSource == null)
        {
            Debug.LogError("SceneAudio 的 stingerSource 槽是空的：失败音放不出来", this);
            wiringOk = false;
        }
        if (loseClip == null)
        {
            Debug.LogError("SceneAudio 的 loseClip 是空的：撞障碍时不会响", this);
            wiringOk = false;
        }

        if (!wiringOk) { enabled = false; return; }   // ⭐ 停手（Start 也不会再跑）
    }

    private void Start()
    {
        // ⚠️ 先判 Instance 再订阅：配置坏了 / 场景里没有 GameManager 时，
        //    别让它变成一行空引用异常（而且要【响亮】地说出后果）
        if (GameManager.Instance == null)
        {
            Debug.LogError("SceneAudio 拿不到 GameManager.Instance：BGM 不会在结束时停、失败音也不会响", this);
            return;
        }

        // BGM 起播：判 isPlaying 是为了"重开一局"时别把正在播的重新点一遍
        if (playBgmOnStart && bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    // ⚠️ 用 OnDestroy 而不是 OnDisable：场景物体是【被销毁】，不是【被禁用】；
    //    而且订阅了不退订 = 悬挂引用（GameManager 持有一份指向已销毁物体的委托）
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    // ── 游戏结束：先停 BGM，再播失败音 ──
    //  ⚠️ 顺序有意义：先停循环音，再把结算音叠上去（反过来会有一瞬间"BGM 还在响"）
    //  ⭐ `timeScale = 0f` 不影响 PlayOneShot（音频不玩游戏时间）——
    //     这正是本脚本存在的理由的反面用法：**该响的时候，冻不住它**
    private void HandleGameOver()
    {
        if (bgmSource != null) bgmSource.Stop();

        if (loseClip != null && stingerSource != null)
        {
            stingerSource.PlayOneShot(loseClip);
        }
    }
}
