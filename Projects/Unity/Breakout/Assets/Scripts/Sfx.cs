using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  Sfx —— 程序化生成音效（成品版）
//
//  数字音频 = 一串采样点：
//    · 采样率 44100 = 每秒记多少个点（人耳上限 20kHz × 2 + 余量）
//    · 每个点的值    = 那一瞬间的振幅（-1 ~ +1）
//  所以"造一个音效" == "算出一个 float 数组"。
//
//  ⚠️ 三个 clip 全部【缓存】：生成一次、用一辈子。
//     否则每次撞砖都新建 3528 个 float（14KB）→ 打 32 块砖 = 450KB GC 垃圾。
//     （和对象池同一个思想：把"重复分配"改成"复用"。）
// ═══════════════════════════════════════════════════════════════
public static class Sfx
{
    private const int SampleRate = 44100;

    private static AudioClip[] _brickHits;
    private static AudioClip _paddleHit;
    private static AudioClip _gameOver;

    // ── 对外只暴露这三个 ────────────────────────────────────────
    //   （`??=` 是 C# 8 的"空合并赋值"：左边为 null 才赋值。
    //     写法等价于 if (x == null) x = ...;  想看得更直白就展开成 if）
    public static AudioClip BrickHit()
    {
        if (_brickHits == null)
        {
            _brickHits = new AudioClip[3];
            for (int i = 0; i < _brickHits.Length; i++)
            {
                float detune = 1f + (i - 1) * 0.06f;     // 0.94 / 1.00 / 1.06（音高 ±6%）
                _brickHits[i] = Tone($"BrickHit{i}", 880f * detune, 0.08f, 40f);
            }
        }
        return _brickHits[Random.Range(0, _brickHits.Length)];   // 随机挑一个
    }


    public static AudioClip PaddleHit()
    {
        if (_paddleHit == null) _paddleHit = Tone("PaddleHit", 220f, 0.12f, 25f);
        return _paddleHit;
    }

    public static AudioClip GameOver()
    {
        if (_gameOver == null) _gameOver = Sweep("GameOver", 440f, 110f, 0.7f, 3f);
        return _gameOver;
    }

    // ── 基础波形：固定频率 + 指数衰减 ───────────────────────────
    //   freq     音高（Hz）
    //   duration 时长（秒）
    //   decay    衰减速率（1/秒）；decay × duration ≥ 3 时结尾已衰减到 <5% → 无爆音
    private static AudioClip Tone(string name, float freq, float duration, float decay)
    {
        int count = Mathf.Max(1, (int)(SampleRate * duration));
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;                      // 第几秒
            float envelope = Mathf.Exp(-decay * t);               // 响度轮廓
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
        }
        return Build(name, data);
    }

    // ── 下滑音：频率从 freqFrom 线性滑到 freqTo（"失落感"）──────
    //   ⚠️ 关键细节：频率随时间变时，不能直接写 sin(2π·freq(t)·t) —— 那样【相位会算错】。
    //      正确做法是【累加相位】：每一步把"这一小段转过的角度"加到 phase 上。
    private static AudioClip Sweep(string name, float freqFrom, float freqTo, float duration, float decay)
    {
        int count = Mathf.Max(1, (int)(SampleRate * duration));
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;                        // 0 → 1 的进度
            float freq = Mathf.Lerp(freqFrom, freqTo, progress);   // 频率随时间下降
            phase += 2f * Mathf.PI * freq / SampleRate;            // ★ 累加相位
            data[i] = Mathf.Sin(phase) * Mathf.Exp(-decay * t) * 0.3f;
        }
        return Build(name, data);
    }

    // ── 公共尾部：造空唱片 + 灌采样点 ───────────────────────────
    private static AudioClip Build(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
