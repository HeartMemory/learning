using UnityEngine;
using UnityEngine.Pool;

// ══════════════════════════════════════════════════════════════
//  ObstacleSpawner —— 程序化生成 + 对象池（UnityEngine.Pool 官方 API）
//
//  ⭐ 今天的新 API（查证结论，收工进笔记）：
//     UnityEngine.Pool.ObjectPool<T> —— Unity 2021+ 内置，官方推荐
//     （Manual "performance-reusable-code"，与 Scripting Reference 交叉确认）。
//     四个回调 + Get()/Release()，就是对象池的全部：
//     · Create  = 真 new 一个实例（Instantiate）
//     · OnGet   = 取出时激活（SetActive true）
//     · OnRelease = 归还时停用（SetActive false）
//     · OnDestroy = 超 maxSize 才真销毁（Destroy）
//
//  ⭐ 判据（落笔前先读）：
//     ① 【池子装什么】ObjectPool<Obstacle>（组件类型，照官方示例 RevisedProjectile）
//        —— 不是 ObjectPool<GameObject>：Obstacle 上才有"撞玩家上报"的逻辑，
//        池子里装组件，Get/Release 直接对组件说话。
//     ② 【四个回调各一行】Create / OnGet / OnRelease / OnDestroy —— 缺一个都漏，
//        这是"复用 vs 重建"的四个动作（今日底层原理）。
//     ③ 【生成 = Get 取出 + 摆位置】程序化摆障碍 = 从池 Get → 设 transform.position
//        （SetActive 已由 OnGet 做了，别重复做）。
//        ⭐ 坐标账要自己推：障碍沿哪个轴摆？第一个摆在哪？间隔多少？
//        （提示：相机在 -Z 朝 +Z，"画面里的前方"是世界的哪个轴？）
//     ④ 【开局全摆出来】跑酷的障碍是"进场就位"，不是"边跑边刷" ——
//        今天先 Start() 一次性把 spawnCount 个摆好，玩家跑过去撞。
//     ⑤ 【对象池的账】Instantiate 只发生 defaultCapacity 次（第一次摆满时），
//        之后反复"借 / 还"都不再 new —— 这就是"对抗 GC"。
//
//  🪞 镜子（同族写法）：
//     · Breakout 的 `BrickSpawner`：Queue 池 / 预分配 / Init 注入 / 三道防线
//       —— 那是"手写轮子"版；今天换成官方 `UnityEngine.Pool`，动作更少、逻辑更聚焦。
// ══════════════════════════════════════════════════════════════
public class ObstacleSpawner : MonoBehaviour
{
    [Header("对象池")]
    [SerializeField] private Obstacle obstaclePrefab;      // 障碍物预制体（Cube + Obstacle 脚本）
    [SerializeField] private int defaultCapacity = 20;     // 池初始容量（第一次会 new 这么多）
    [SerializeField] private int maxSize = 100;            // 池上限（超过才真 Destroy）
    [SerializeField] private bool collectionCheck = true;  // 归还时查重（防一个实例还两次）

    [Header("生成布局（坐标账）")]
    [SerializeField] private int spawnCount = 8;           // 摆几个障碍
    [SerializeField] private float spawnStartZ = 6f;       // 第一个障碍的 z（玩家前方）
    [SerializeField] private float spawnSpacing = 4f;      // 相邻障碍间距

    private IObjectPool<Obstacle> _pool;

    private void Awake()
    {
        // TODO ①：建池
        //   new ObjectPool<Obstacle>(CreateObstacle, OnGetFromPool, OnReleaseToPool,
        //                           OnDestroyPooledObject, collectionCheck, defaultCapacity, maxSize)
        _pool = new ObjectPool<Obstacle>(
            CreateObstacle,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    private void Start()
    {
        // TODO ②：程序化摆障碍（照判据 ③④）
        //   循环 spawnCount 次：_pool.Get() → 设 transform.position → （激活已由 OnGet 做）
        for (int i = 0; i < spawnCount; i++)
        {
            Obstacle obstacle = _pool.Get();
            float zPosition = spawnStartZ + i * spawnSpacing;
            obstacle.transform.position = new Vector3(0f, 0f, zPosition);
        }
    }

    private Obstacle CreateObstacle()
    {
        // TODO ③：真 new 一个实例（Instantiate(obstaclePrefab)）
        return Instantiate(obstaclePrefab);
    }

    private void OnGetFromPool(Obstacle obstacle)
    {
        // TODO ④：取出时激活（obstacle.gameObject.SetActive(true)）
        if (obstacle == null) return;
        obstacle.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(Obstacle obstacle)
    {
        // TODO ⑤：归还时停用（obstacle.gameObject.SetActive(false)）
        if (obstacle == null) return;
        obstacle.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Obstacle obstacle)
    {
        // TODO ⑥：超上限才真销毁（Destroy(obstacle.gameObject)）
        if (obstacle == null) return;
        Destroy(obstacle.gameObject);
    }
}
