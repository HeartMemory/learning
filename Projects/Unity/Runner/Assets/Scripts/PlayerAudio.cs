using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  PlayerAudio —— 玩家的三类音效：跳 / 落地 / 走路（Runner · 3D 版 · 2026-10-20）
//
//  🪞 结构照 Platformer/PlayerAudio.cs，判据一条没改：
//    ① 【谁产生的谁消费】三类音都由玩家产生 ⇒ 挂 Player 自己身上，
//       复用 Player 自己的 AudioSource（不给每个音效单开对象）
//    ② 【消费者，不是裁判】只订 PlayerController 报出来的【事实】，从不改玩家状态
//    ③ 【事件 vs 状态】
//       · 跳 / 落地 = 【事件】（不可重复的瞬间）→ 订 PlayerController 的两个事件
//       · 走路     = 【状态】+ 节拍计时器（"持续在走"是状态，"踩到地"是事件）
//
//  🆕 3D 与 2D 的三处差别（照搬会出事，所以这里改了）：
//    ① 数据源换了：Platformer 读 `player.MoveAxis`（float），
//       3D 的方向是向量 ⇒ 读 `player.MoveInput`（Vector3），判据用 sqrMagnitude
//    ② Platformer 的 PlayerController 原本就有 OnJumped / OnLanded；
//       Runner 的一个都没有 ⇒ 今天在 PlayerController 里【新加】了这两个事件
//    ③ 3D 里 AudioSource 有 `Spatial Blend`：玩家自身音效设 0（2D）
//       —— 否则相机离角色稍远，脚步声就衰减得听不见
//       （接线说明里写着这一项，检查器里改）
// ══════════════════════════════════════════════════════════════
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("接线（装配期拖引用）")]
    [SerializeField] private PlayerController player;   // 拖 Player 自己身上的 PlayerController

    [Header("音效素材")]
    [SerializeField] private AudioClip[] jumpClips;     // 拖几个，随机播一个（同一个音连响会听腻）
    [SerializeField] private AudioClip[] stepClips;     // 走路节拍
    [SerializeField] private AudioClip landClip;        // 落地

    [Header("走路节拍")]
    [SerializeField] private float stepInterval = 0.32f;  // 两步之间的间隔（秒）—— 先跑起来再感受着调

    private AudioSource _sfx;      // 短音专用：全程只走 PlayOneShot
    private float _stepTimer;      // 节拍计时器（走路音专用）

    // ── 装配期自检：响亮地失败（照 PlayerController.Awake 的镜子）──
    //   ⭐ 用 LogError（红的）；先收集全部问题再统一停手（修一轮而不是修三轮）
    //   ⚠️ 空阵列判据（10-10 那条）：`== null` 抓不住"没拖" ——
    //      Unity 给"没赋值的阵列"发出来的是【空阵列（Length == 0）】，不是 null
    //      ⇒ 只判 null 的话这条自检永远不会响（"响亮地失败"退化成"静默地失败"）
    private void Awake()
    {
        _sfx = GetComponent<AudioSource>();

        bool hasError = false;
        if (player == null)
        {
            Debug.LogError("PlayerAudio 的 player 槽是空的：没法订阅跳跃 / 落地事件", this);
            hasError = true;
        }
        if (jumpClips == null || jumpClips.Length == 0)
        {
            Debug.LogError("PlayerAudio 的 jumpClips 没拖（空阵列或 null）：跳跃没声音", this);
            hasError = true;
        }
        if (stepClips == null || stepClips.Length == 0)
        {
            Debug.LogError("PlayerAudio 的 stepClips 没拖（空阵列或 null）：走路没声音", this);
            hasError = true;
        }
        if (landClip == null)
        {
            Debug.LogError("PlayerAudio 的 landClip 是空的：落地没声音", this);
            hasError = true;
        }
        if (hasError) enabled = false;
    }

    // ── 订阅 PlayerController 报出来的【事实】 ──
    //   ⭐ 订【事件】而不是每帧读状态："跳了""落地了"是不可持续的事实，
    //      事件天然每次只触发一次 ⇒ 音效不用自己判重，也不可能"每帧播"
    //   ⚠️ 先判 null：不同对象的 Awake / OnEnable 顺序不确定
    private void OnEnable()
    {
        if (player == null) return;
        player.OnJumped += PlayJump;
        player.OnLanded += PlayLand;
    }

    // 谁订阅谁退订（逐行对称）
    private void OnDisable()
    {
        if (player == null) return;
        player.OnJumped -= PlayJump;
        player.OnLanded -= PlayLand;
    }

    private void PlayJump() => PlayRandom(jumpClips);

    private void PlayLand()
    {
        // 单个引用就判单个 null（阵列才用"随机取一个"那套）
        if (landClip == null) return;
        _sfx.PlayOneShot(landClip);
    }

    // ── 走路音：节拍计时器（在物理时钟里跑）──
    //  ⛔ 绝不能写成 `if (在走) _sfx.PlayOneShot(step)`
    //     —— 那是每物理步播一次 = 一秒 50 次叠加 → 削波刺耳（承 09-26 那条"锁没罩住副作用"）
    //  ⭐ 判据：**踩地是「事件」，不是「状态」** ⇒ 用计时器把"持续在走"切成"每隔 N 秒踩一次"
    private void FixedUpdate()
    {
        // ① 先把"还在走"算成一个局部变量（判据集中一处）
        //    · 没接地不算走（空中不接受迈步音）
        //    · 判据用 sqrMagnitude：方向向量长度 1 ⇒ 平方也是 1，省一次开根号
        bool isWalking = player != null
                         && player.IsGrounded
                         && player.MoveInput.sqrMagnitude > 0.01f;

        // ② 不在走 → 计时器【归零】并立刻返回
        //    ⭐ 归零是灵魂：下次起步的第一声立刻响，而不是等完上一轮剩下的间隔
        if (!isWalking)
        {
            _stepTimer = 0f;
            return;
        }

        // ③ 在走 → 走表；到点响一声，然后"重置为满"
        //    ⚠️ 用 Time.fixedDeltaTime（本方法跑在物理时钟里），不是 Time.deltaTime
        _stepTimer -= Time.fixedDeltaTime;
        if (_stepTimer <= 0f)
        {
            PlayRandom(stepClips);
            _stepTimer = stepInterval;   // ⭐ 重置为满，不是 "+="（后者会把误差累积成漂移）
        }
    }

    // 从一组 clip 里随机挑一个播
    //  ⚠️ 空阵列守卫放在这儿（而不是让每个调用方自己判）：守卫只有一处，就不可能漏
    //  ⚠️ `Random.Range(int, int)` 是【左闭右开】⇒ 上限传 Length，不是 Length - 1
    //     （传 Length - 1 的话最后一个 clip 永远抽不到 —— 一种静默的偏心）
    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        _sfx.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
