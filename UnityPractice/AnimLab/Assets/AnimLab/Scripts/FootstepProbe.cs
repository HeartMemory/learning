using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画事件探针 —— 用日志（而不是耳朵）证明「脚步声由剪辑驱动 ⇒ 步频随速度变化」。
///
/// ⚠️ 两条必须知道的机制（决定了这个类怎么摆）：
///   ① 动画事件按【方法名】找方法（ScriptReference：similar to SendMessage）
///      ⇒ 方法必须叫 OnFootstep / OnLand，换个名字就一次都收不到，而且【不报错】。
///   ② 派发对象 = 挂 Animator 的那个物体，它会调用自己【所有组件】上的同名方法
///      ⇒ 本类与 Starter Assets 的 ThirdPersonController.OnFootstep 会【同时】被调用。
///        Starter Assets 那边负责出声，本类只打日志（不碰 AudioSource）⇒ 【不会双声】。
///
/// ⚠️ 为什么不能直接写进 ThirdPersonController：那是官方示例包，改它等于改第三方代码；
///    而且同一个类里再写一个 OnFootstep 就是 CS0111 重复定义。
///
/// ⚠️ 为什么要看"权重"：混合树里 Walk / Run 可能【同时在播】，两个剪辑的事件都会上来。
///    `animatorClipInfo.weight` 是唯一能分辨"这条事件来自哪个剪辑、此刻占多少分量"的信号 ——
///    这也正是 ThirdPersonController 里那句 `weight > 0.5f` 门卫存在的理由。
/// </summary>
public class FootstepProbe : MonoBehaviour
{
    private int _count;                                  // 累计收到多少次脚印事件
    private float _startTime;                            // 观察起点
    private float _lastTime;                             // 上一次事件的时刻（用来算间隔）
    private readonly Dictionary<string, int> _byClip = new();   // 每个剪辑各贡献了几次

    private void Start()
    {
        _count = 0;
        _startTime = Time.time;
        _lastTime = Time.time;      // ⚠️ 必须与 _startTime 一致，否则第一次的间隔是个天文数字
        _byClip.Clear();
        Debug.Log($"[Probe] 开始观察 @ {_startTime:F2}s");
    }

    /// <summary>脚印事件（Walk_N / Run_N / Run_S 各有两条，帧位在脚触地那一拍）。</summary>
    public void OnFootstep(AnimationEvent animationEvent)
    {
        _count++;

        float now = Time.time;
        float gap = now - _lastTime;      // 距上一次脚印的间隔 = 步频的倒数
        _lastTime = now;

        string clip = ClipNameOf(animationEvent);
        _byClip[clip] = _byClip.TryGetValue(clip, out int n) ? n + 1 : 1;

        Debug.Log($"[脚印] 第 {_count} 次 · {Describe(animationEvent)} · 间隔 {gap:F3}s · " +
                  $"累计 {now - _startTime:F2}s · 分账 {Summary()}");
    }

    /// <summary>
    /// 落地事件。⚠️ `OnLand` 写在【三条剪辑】里，所以一次落地可能收到多条：
    ///   · `JumpLand`（来自 Jump--Jump.anim.fbx；该 FBX 内含 `JumpStart` + `JumpLand` 两个剪辑）
    ///   · `Walk_N_Land` / `Run_N_Land`（JumpLand 状态那棵混合树按 `Speed` 混合而成）
    /// ⇒ 同一事件名、多个来源，靠 `weight > 0.5` 才能去重（Starter Assets 就是这么做的）。
    /// </summary>
    public void OnLand(AnimationEvent animationEvent)
    {
        Debug.Log($"[落地] {Describe(animationEvent)} · 累计 {Time.time - _startTime:F2}s");
    }

    // ── 小工具（与事件机制无关，纯拼接）──

    private static string ClipNameOf(AnimationEvent animationEvent)
    {
        AnimationClip clip = animationEvent.animatorClipInfo.clip;
        return clip != null ? clip.name : "(无剪辑)";
    }

    private static string Describe(AnimationEvent animationEvent)
    {
        AnimatorClipInfo info = animationEvent.animatorClipInfo;
        return $"剪辑={ClipNameOf(animationEvent)} 权重={info.weight:F2}";
    }

    private string Summary()
    {
        if (_byClip.Count == 0) return "{}";

        List<string> parts = new();
        foreach (KeyValuePair<string, int> pair in _byClip)
            parts.Add($"{pair.Key}={pair.Value}");
        return "{" + string.Join(", ", parts) + "}";
    }
}
