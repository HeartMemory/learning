using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  PlayerAudio —— 玩家的三类音效：跳 / 落地 / 走路 · 2026-10-10
//
//  结构判据（承 09-26 / 10-06 / 10-07 那几条）：
//    ① 【谁产生的谁消费】—— 三类音都由玩家产生 ⇒ 全部挂在 Player 自己身上，
//       复用 Player 自己的 AudioSource（不给每个音效单开对象、不写全局音效管理器）
//       —— 与 Breakout "球主动判、复用球身上那个 AudioSource" 是同一条判据
//    ② 【消费者，不是裁判】—— 本脚本只订 PlayerController 报出来的【事实】，
//       从不反过来改玩家状态；也不去每帧问它"你跳了吗"
//    ③ 【事件 vs 状态】——
//       · 跳 / 落地 = 【事件】（不可持续）→ 订 PlayerController 的两个事件
//       · 走路     = 【状态】+ 节拍计时器（"持续在走"是状态，"踩到地"是事件）
// ══════════════════════════════════════════════════════════════
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("接线（装配期拖引用）")]
    [SerializeField] private PlayerController player;   // 拖 Player 自己身上那个 PlayerController

    [Header("音效素材（从 Assets/Audio/Sfx/... 拖 clip 进来）")]
    [SerializeField] private AudioClip[] jumpClips;     // Sfx/Jump —— 拖几个，随机播一个
    [SerializeField] private AudioClip[] stepClips;     // Sfx/Step
    [SerializeField] private AudioClip landClip;        // Sfx/Land/jumpland

    [Header("走路节拍")]
    [SerializeField] private float stepInterval = 0.32f;  // 两步之间的间隔（秒）—— 先跑起来再感受着调

    private AudioSource _sfx;      // 短音专用：全程只走 PlayOneShot
    private float _stepTimer;      // 节拍计时器（走路音专用）

    // ── 装配期自检：响亮的失败 ──
    //   照 PlayerController.Awake 的镜子（那里已经攒了 7 条检查）。
    //   ⭐ 用 LogError（红的）而不是 LogWarning：红的才拦得住人
    //   ⭐ 检查必须写在【使用之前】——"日志放在会抛异常的行之后 = 等于没放"（09-24）
    private void Awake()
    {
        _sfx = GetComponent<AudioSource>();

        // TODO ⓪（你写 · 可选但推荐）：
        //   把"槽位没接"提前喊出来，至少覆盖这四种：
        //     · player 槽是空的        · jumpClips 为空 / 没拖够
        //     · stepClips 为空         · landClip 是空的
        //   风格照 PlayerController：先用一个 bool 收集全部问题，最后统一停手（enabled = false）
        bool hasError = false;
        if(player == null)
        {
            Debug.LogError("player 槽是空的：没法订阅 PlayerController 的事件", this);
            hasError = true;
        }
        // ⚠️ 10-10 修正：`== null` 抓不住"没拖"！
        //   Unity 给"没赋值的阵列"发出来的是【空阵列（Length == 0）】，不是 null
        //   ⇒ 只判 null 的话，这条自检【永远不会响】（"响亮地失败"退化成"静默地失败"）
        //   ⭐ 判据：**null 与"空"是两件事** —— 凡是"检查有没有东西"，两个都要判
        if(jumpClips == null || jumpClips.Length == 0)
        {
            Debug.LogError("jumpClips 没拖（空阵列或 null）：没法播跳跃音", this);
            hasError = true;
        }
        if(stepClips == null || stepClips.Length == 0)
        {
            Debug.LogError("stepClips 没拖（空阵列或 null）：没法播走路音", this);
            hasError = true;
        }
        if(landClip == null)
        {
            Debug.LogError("landClip 是空的：没法播落地音", this);
            hasError = true;
        }
        if(hasError)
        {
            enabled = false;
        }
    }

    // ── 订阅 PlayerController 报出来的【事实】 ──
    //   ⭐ 为什么订【事件】而不是每帧去读 `player.IsGrounded` / `player.MoveAxis`：
    //      "跳了""落地了"是【不可持续的事实】，事件天然每个事实只触发一次
    //      ⇒ 音效不用自己判重，也不可能"每帧播"
    //   ⭐ 与 Camera.main / Animator 参数名同族：装配期拖引用，只在 OnEnable 订一次
    private void OnEnable()
    {
        // TODO ①（你写）：订阅
        //   · player.OnJumped  → 本类的播跳方法
        //   · player.OnLanded  → 本类的播落地方法
        //   判据：先判 player 是不是 null ——
        //     不同对象的 Awake / OnEnable 顺序【不确定】（承 10-07 那条"为什么用 Inspector 拖引用而不是 Instance"）
        if(player != null)
        {
            player.OnJumped += PlayJump;
            player.OnLanded += PlayLand;
        }
    }

    private void OnDisable()
    {
        // TODO ②（你写）：退订 —— 照 OnEnable 逐行对称（谁订阅谁退订）
        if(player != null)
        {
            player.OnJumped -= PlayJump;
            player.OnLanded -= PlayLand;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  TODO ③（你写）：播跳跃音
    //    要点两条：
    //      · 从 jumpClips 里【随机】挑一个 —— 同一个音连响会听腻
    //        （前身 = 09-25 撞砖的"3 个音高变体解决同相位叠加"）
    //      · 走 PlayOneShot：跳跃是"事件"，不是"状态"
    //    ⚠️ 空数组要能安全过关（不要因为"没拖够"就抛异常）
    // ══════════════════════════════════════════════════════════════
    private void PlayJump()
    {
        // ✅ 10-10 成品：
        //   ⭐ `Random.Range(int, int)` 是【左闭右开】⇒ 上限要传 Length，不是 Length - 1
        //      （传 Length - 1 的话，最后一个 clip 永远抽不到 —— 一种静默的偏心）
        //   ⚠️ 别和 `Random.Range(float, float)` 混：那个是【闭区间】，同名不同规矩
        PlayRandom(jumpClips);
    }

    // ══════════════════════════════════════════════════════════════
    //  TODO ④（你写）：播落地音
    // ══════════════════════════════════════════════════════════════
    private void PlayLand()
    {
        // ✅ 10-10 成品（⚠️ 顺手改掉一个 copy-paste 陷阱：你原来写的是 stepClips[0]）：
        //   · 落地音要用 landClip —— stepClips 是【走路】音，两者不是一回事
        //   · 单个引用就判单个 null（阵列才用"随机取一个"那套）
        if (landClip == null) return;
        _sfx.PlayOneShot(landClip);
    }

    // ══════════════════════════════════════════════════════════════
    //  TODO ⑤（你写）：走路音的【节拍计时器】—— 在 FixedUpdate 里
    //
    //  ⛔ 动手前先看这个陷阱（09-26 血泪的直系亲属）：
    //     绝不能写成 `if (在走) _sfx.PlayOneShot(step)`
    //     —— 那是【每物理步播一次】= 一秒 50 次叠加 → 削波刺耳
    //     （09-26 那次是"锁没罩住副作用"；这次是"把事件当状态播"）
    //
    //  ⭐ 判据：**踩地是「事件」，不是「状态」**
    //     ⇒ 要用计时器，把"持续在走"切成"每隔 N 秒踩一次"
    //
    //  要写的三件事：
    //    a) 不在走（没接地 或 |MoveAxis| 很小）→ 计时器【归零】
    //       ⭐ 为什么必须归零：下次起步要【立刻】响第一声，而不是等完上一轮剩下的间隔
    //    b) 在走 → _stepTimer -= Time.fixedDeltaTime
    //    c) _stepTimer <= 0 → 播一声（取法同 TODO ③ 的随机）+ 把计时器重置为 stepInterval
    //
    //  ⚠️ 用 Time.fixedDeltaTime，不是 Time.deltaTime —— 本方法跑在【物理时钟】里
    //  ⚠️ 顺序提示：Unity 不保证 PlayerController.FixedUpdate 和本方法谁先跑
    //     ⇒ 你读到的 player.IsGrounded 可能是【上一步】的值（差一步，可接受；但要知道）
    // ══════════════════════════════════════════════════════════════
    private void FixedUpdate()
    {
        // ✅ 10-10 成品：
        //   ① 先把"还在走"算成一个局部变量 —— 判据集中在一处，下面只读它
        //      · 没接地不算走（空中不接受迈步音）
        //      · |轴值| > 0.01f：与 PlayerController 转身用的是【同一个阈值】（微抖不算走）
        bool isWalking = player != null && player.IsGrounded && Mathf.Abs(player.MoveAxis) > 0.01f;

        //   ② 不在走 → 计时器【归零】并立刻返回
        //      ⭐ 归零是这条逻辑的灵魂：下次起步的第一声【立刻】响，
        //         而不是等完上一轮剩下的那点间隔（否则起步会有一个随机长度的静音空档）
        if (!isWalking)
        {
            _stepTimer = 0f;
            return;
        }

        //   ③ 在走 → 走表；到点响一声，然后"重置为满"
        //      ⚠️ 用 Time.fixedDeltaTime（本方法跑在物理时钟里），不是 Time.deltaTime
        _stepTimer -= Time.fixedDeltaTime;
        if (_stepTimer <= 0f)
        {
            PlayRandom(stepClips);
            _stepTimer = stepInterval;   // ⭐ 是"重置为满"，不是 "+="（后者会把误差一路累积成漂移）
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  🆕 10-10 抽出来的小工具：从一组 clip 里【随机挑一个】播
    //    判据：同一段逻辑（"空阵列守卫 + 随机取一个 + PlayOneShot"）出现了第二次
    //          ⇒ 提出来（DRY）—— 于是 PlayJump 与走路各自只剩一行调用
    //    ⚠️ 空阵列守卫放在这儿（而不是让每个调用方自己判）：守卫只有一处，就不可能漏
    //    ⚠️ 这里的 `Random` 解析到 `UnityEngine.Random`（本文件只有 using UnityEngine）。
    //       若将来在文件头加了 `using System;`，`Random` 会【有歧义】（CS0104）
    //       ⇒ 那时要写成全名 `UnityEngine.Random.Range`
    // ══════════════════════════════════════════════════════════════
    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        _sfx.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
