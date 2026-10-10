using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  SceneAudio —— 场景级音频：BGM + 结算音 · 2026-10-11（10-10 活动延续）
//
//  ── 为什么需要它（问题起因）──
//    GameManager 结束游戏时会 `Time.timeScale = 0f` —— 那【只冻物理】。
//    ⭐ `AudioSource` 跑在【实时时钟】上，根本不吃 timeScale ⇒ BGM 会一直播下去。
//    这是 09-25 那条「timeScale 的影响力半径」的【另一面】：
//      · Animator 【吃】timeScale  ⇒ 要它动，得把 Update Mode 改成 Unscaled Time
//      · AudioSource【不吃】timeScale ⇒ 要它停，只能【自己订阅"结束"】
//    ⇒ 判据：**"世界冻结"这个动作，不会替你把声音停了**
//       （和 10-06 "玩家停手"是同一条镜子：谁想跟着停，谁自己订阅）
//
//  ── 结构判据 ──
//    ① 【挂 Prefab 上】BGM 是"每关都要有"的东西 ⇒ 做成预制体（三场景共用模具）
//       ⚠️ 但预制体【不能拖引用】场景对象 ⇒ 拿不到场景里的 GameManager
//       ⇒ 只能用 `GameManager.Instance`
//       （与 10-06 写敌人时的结论完全同构："敌人是预制体，裁判拖不了它的引用"）
//    ② 【订阅放 Start，不是 OnEnable】
//       Awake / OnEnable 的跨对象顺序【不确定】⇒ 那时 `Instance` 可能还是 null
//       Start 在所有 Awake 跑完之后 ⇒ 那时 `Instance` 一定就位
//    ③ 【两种订阅并存，正好各演一遍】
//       · "停 BGM"  订 `OnGameEnded` —— 与【怎么结束】无关
//         （10-07 的教训原话：订阅端该订「语义」，不是某一个具体原因）
//       · "播哪个音" 订 `OnGameOver` / `OnGameWin` —— 这里【必须】区分具体原因
// ══════════════════════════════════════════════════════════════
public class SceneAudio : MonoBehaviour
{
    [Header("接线（装配期拖引用）")]
    // ⚠️ 为什么两个 AudioSource 都用【拖引用】而不是 GetComponent：
    //   同一个物体上挂了【两个同类型组件】时，`GetComponent<AudioSource>()` 返回哪一个
    //   【是不确定的】（取决于添加 / 序列化顺序）——"平局不可靠"。
    //   ⇒ 与 10-05 那条 `Order in Layer` 平局的教训同族：
    //      凡是"系统明说结果未定义"的地方，就别让它替你做决定。
    [SerializeField] private AudioSource bgmSource;      // BGM 那个（loop，输出 = Music）
    [SerializeField] private AudioSource stingerSource;   // 第 2 个（输出 = Sfx），专播结算音
    [SerializeField] private AudioClip winClip;           // Sfx/Win/winfretless
    [SerializeField] private AudioClip loseClip;          // Sfx/Lose/GameOver_1

    // ── 装配期自检：响亮的失败（照 PlayerController / PlayerAudio 的镜子）──
    //   ⭐ 用 LogError（红的）；先收集全部问题再统一停手（修一轮而不是修三轮）
    private void Awake()
    {
        bool wiringOk = true;

        if (bgmSource == null)
        {
            Debug.LogError("bgmSource 槽是空的：游戏结束时 BGM 停不下来", this);
            wiringOk = false;
        }
        if (stingerSource == null)
        {
            Debug.LogError("stingerSource 槽是空的：结算音放不出来", this);
            wiringOk = false;
        }
        if (winClip == null)
        {
            Debug.LogError("winClip 是空的：通关时不会响", this);
            wiringOk = false;
        }
        if (loseClip == null)
        {
            Debug.LogError("loseClip 是空的：失败时不会响", this);
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
            Debug.LogError("GameManager.Instance 是 null：BGM 不会在结束时停、结算音也不会响", this);
            return;
        }

        // TODO ①（你写）：订阅三件事
        //   · OnGameEnded → 停 BGM      ⬅ 订【语义】（不管怎么结束）
        //   · OnGameOver  → 播失败音     ⬅ 订【具体原因】
        //   · OnGameWin   → 播胜利音
        // ✅ 成品（照 PlayerAudio 的镜子，方法组装配 + 方法名就是"待会儿要写的那三个"）：
        GameManager.Instance.OnGameEnded += HandleGameEnded;
        GameManager.Instance.OnGameOver += HandleGameOver;
        GameManager.Instance.OnGameWin += HandleGameWin;
    }

    private void OnDestroy()
    {
        // TODO ②（你写）：退订 —— 照 Start 逐行对称（谁订阅谁退订）
        //   ⚠️ 用 OnDestroy 而不是 OnDisable：
        //      场景物体是【被销毁】，不是【被禁用】；而且静态事件 + 不退订 = 悬挂引用
        //      （承 10-06 那条"静态槽位不随场景卸载自动清"的账）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnGameWin -= HandleGameWin;
        }
    }

    // TODO ③（你写）：停 BGM
    //   ✅ v1 = 硬切。⚠️ 注意先判空：自检没通过时这个物体可能没启用，但保险起见判一下
    //   进阶（以后可做）= 用 Mixer 的 `SetFloat` 把 Music 音量滑到 -80dB 做【淡出】，
    //                     而不是"啪"地断（判据：这是"观感"问题，不是"对错"问题）
    private void HandleGameEnded()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    // TODO ④（你写）：播失败音
    //   ⚠️ 结算音走【第 2 个】AudioSource（stingerSource），别用 bgmSource：
    //      那是 loop 的通道，拿它 PlayOneShot 会跟循环音混在一起
    //   ⭐ 顺带：`Time.timeScale = 0f` 不影响 PlayOneShot（音频不玩游戏时间）——
    //      这正是本脚本存在的理由的反面用法：**该响的时候，冻不住它**
    private void HandleGameOver()
    {
        if (loseClip == null) return;
        stingerSource.PlayOneShot(loseClip);
    }

    // TODO ⑤（你写）：播胜利音
    private void HandleGameWin()
    {
        if (winClip == null) return;
        stingerSource.PlayOneShot(winClip);
    }
}
