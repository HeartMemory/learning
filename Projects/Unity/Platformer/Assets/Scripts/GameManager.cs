using System;
using System.Collections.Generic;      // 🆕 List<T>
using Newtonsoft.Json;                 // 🆕 JsonConvert（Unity 官方 UPM 包 com.unity.nuget.newtonsoft-json）
using UnityEngine;
using UnityEngine.SceneManagement;

// ══════════════════════════════════════════════════════════════
//  GameManager —— 全场唯一裁判（Block 3 · 2026-10-06）
//  职责：接收"致命源"的上报 → 决定死 → 上锁 → 改状态 → 广播（唯一出口）
//
//  ⭐ 结构（10-06 定）：
//     致命源（FailZone / 敌人 / 将来的陷阱） ──直接调用──▶ GameManager.Instance.NotifyPlayerLost()
//     裁判 ──发事件──▶ 订阅者（DeathPanel 淡入；将来：音效 / 统计）
//  ⭐ 判据分层：
//     · 唯一的【服务】用单例 —— 听众只有 1 个，且敌人是【预制体】，裁判拖不了它的引用
//     · 一条【消息】用事件 —— 听众未知 / 会变
//  🆕 10-08：多关卡 —— 加一张"关卡配置表"（从 Assets/Data/levels.json 读）+ LoadNextLevel()
// ══════════════════════════════════════════════════════════════
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }   // ⭐ 唯一服务：让预制体一行就能找到裁判

    public event Action OnGameOver;      // 一条消息：**只有"死"** —— 需要区分死/赢的订阅者订它（如结算面板）
    public event Action OnGameWin;       // 一条消息：**只有"赢"**
    public event Action OnGameEnded;     // ⭐ 一条消息：**不管怎么结束** —— 不关心原因的订阅者（如"玩家停手"）只订它

    private bool _isGameOver;            // ⭐ 幂等门卫：全场唯一的上锁处

    // ═══════ 🆕 多关卡（10-08）═══════
    [SerializeField] private TextAsset levelConfig;   // 拖 Assets/Data/levels.json 进来
    private List<LevelData> _levels = new List<LevelData>();   // 解析出来的关卡表（Awake 读一次，之后复用）

    private void Awake()
    {
        // ⭐ 单例自检：静态槽位不随场景卸载自动清 → 重开场景时可能残留旧值
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"场景里有不止一个 GameManager（{name}）：本实例会被停用", this);
            enabled = false;
            return;
        }
        Instance = this;

        // ═══════ 关卡配置表（10-08 · 多关卡 · 已实现）═══════
        //  三个关键点：
        //    ① ⭐ **数据来源是 `levelConfig.text`**，不是 `File.ReadAllText` ——
        //       `TextAsset` 已经替你把文件读好了（Unity 里"资产"取代了"文件路径"）
        //    ② 尖括号里填【整个 JSON 对应的类型】：本表顶层是【数组】⇒ `List<LevelData>`
        //    ③ 失败分两种，处理【不同】：
        //       · 空槽 / 解析出空表 → `LogError`（消息带上【后果】）+ **不 return**
        //         （配置坏了只是"下一关"废了，"死 / 赢"两条链路还得照常工作）
        //       · JSON 格式坏 → **不 catch，让它崩** ← ⭐ 知情的选择（10-08 定的）：
        //         崩在 `Awake` 的真实代价 = 该组件剩余初始化跳过 + 一条红字
        //         （Unity 会捕获异常，**其他组件照常初始化**）
        //         ⇒ 对"不会随时看控制台"的人来说，**崩才是最响亮的可见失败**
        if(levelConfig == null)
        {
            Debug.LogError("[GameManager] levelConfig 槽是空的：关卡表永远不会被读", this);
        }
        else
        {
            _levels = JsonConvert.DeserializeObject<List<LevelData>>(levelConfig.text);
            if(_levels == null || _levels.Count == 0)
            {
                Debug.LogError("[GameManager] 关卡表解析失败或为空：点「下一关」会失败", this);
            }
        }
    }

    private void OnDestroy()
    {
        // ⭐ 两笔账之一：静态字段【不随场景卸载自动清】→ 自己清入口（与 ScoreManager 同族）
        if (Instance == this) Instance = null;
    }

    // ⭐ 所有致命源的【唯一入口】：哨兵只管喊，裁判只管判
    public void NotifyPlayerLost()
    {
        if (_isGameOver) return;         // ① 先上锁（第二声、第三声都在这儿被吃掉）
        _isGameOver = true;

        Time.timeScale = 0f;             // ② 再改状态：世界冻结
        Debug.Log("[GameManager] 玩家死亡 → 世界冻结 + 广播 OnGameOver");

        OnGameOver?.Invoke();            // ③ 最后广播：谁关心谁反应（裁判不认识 UI）

        OnGameEnded?.Invoke();
    }

    // ⭐ 所有【取胜源】的唯一入口（与 NotifyPlayerLost 逐行对称）
    public void NotifyPlayerWon()
    {
        if (_isGameOver) return;         // ① 先上锁（第二声、第三声都在这儿被吃掉）
        _isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] 玩家通关 → 世界冻结 + 广播 OnGameWin");
        OnGameWin?.Invoke();

        OnGameEnded?.Invoke();
    }

    public void Restart()
    {
        Time.timeScale = 1f;             // ⚠️ 必须先还 timeScale，否则新场景一开场就是冻结的
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ═══════ 🆕 TODO B（10-08 · 多关卡）：加载【下一关】 ═══════
    //  判据链（写之前先答）：
    //    ① **"我现在是哪一关"怎么认出来？**
    //       提示：`SceneManager.GetActiveScene().name` 与表里的 `SceneName` 能对上 ⇒ 不需要额外记状态
    //    ② 若"当前场景名"在表里【找不到】怎么办？
    //       （开发期很常见：直接 Play 了某一关、场景改名了 …）
    //    ③ 若【已经是最后一关】→ 现在还没有"全部通关"的画面 ⇒ 先 `Debug.Log` 收尾（留给打磨日）
    //    ④ ⚠️ **与 `Restart()` 同一条教训**：先 `Time.timeScale = 1f`，再 `LoadScene`
    //       （照镜子：`Restart()` 就在上面几行，那两行就是模板）
    //    ⑤ 查表走 `List.Find` 还是建个 `Dictionary<int, LevelData>`？
    //       —— 两个都行。选完在心里说一句"为什么"（承今天的泛型：容器按【访问方式】选）
    //    ⑥ ⭐ **用【场景名】加载**（`LoadScene(level.SceneName)`），不要用 `buildIndex`：
    //       配置表里存的就是名字；而且【索引会漂移】——在 Build Settings 里挪一下顺序，所有索引全变，名字不会
    //    ⑦ ⭐ "**已经是最后一关**"【不该崩】：那是正常游戏进度；
    //       该崩的是"配置坏了 / 当前关在表里找不到"（那是数据问题）
    public void LoadNextLevel()
    {
        throw new NotImplementedException("TODO B：找下一关 → 还 timeScale → LoadScene");
    }
}
