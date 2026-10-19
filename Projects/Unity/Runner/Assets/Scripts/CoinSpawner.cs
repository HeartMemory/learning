using UnityEngine;
using UnityEngine.Pool;

// ══════════════════════════════════════════════════════════════
//  CoinSpawner —— 金币生成（Runner · 2026-10-19）
//  🪞 整份照 ObstacleSpawner.cs：同样的四回调对象池、同样的程序化摆放循环。
//     ⭐ **唯一的差别 = 纵坐标**：金币抬到 `spawnHeight`（方块上方），
//        其余（z 序列、间距、根物体归置）与障碍完全一致 ——
//        ⇒ 两边的 `spawnCount / spawnStartZ / spawnSpacing` 填【同样的数】，
//           金币就会正好长在每个障碍的正上方（跳过障碍时顺路吃到）
//
//  ⭐ 判据（照镜子）：
//     · 池里装 **组件**（`ObjectPool<Coin>`），Get/Release 直接对组件说话
//     · 四个回调各一行，缺一个都漏（Create / OnGet / OnRelease / OnDestroy）
//     · `defaultCapacity` 只是容器初始容量，对象是**懒创建**的
//     · 谁生成谁摆放：Get → 设 position（SetActive 已由 OnGet 做，别重复）
//     · `collectionCheck` 防"一个实例还两次"
// ══════════════════════════════════════════════════════════════
public class CoinSpawner : MonoBehaviour
{
    [Header("对象池")]
    [SerializeField] private Coin coinPrefab;              // 金币预制体（Cube + Coin 脚本）
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private bool collectionCheck = true;

    [Header("生成布局（与 ObstacleSpawner 填同样的数）")]
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnStartZ = 6f;
    [SerializeField] private float spawnSpacing = 4f;
    [SerializeField] private float spawnHeight = 1.2f;     // ⭐ 与障碍唯一的差别：抬到方块上方

    [Header("层级归置（可留空 = 不归置）")]
    [SerializeField] private Transform coinRoot;
    [SerializeField] private Transform poolRoot;

    private IObjectPool<Coin> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Coin>(
            CreateCoin,
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
        for (int i = 0; i < spawnCount; i++)
        {
            Coin coin = _pool.Get();
            coin.transform.position = new Vector3(0f, spawnHeight, spawnStartZ + i * spawnSpacing);
        }
    }

    private Coin CreateCoin()
    {
        // 出生就挂在回收站：否则新实例会在场景根级冒一帧
        return Instantiate(coinPrefab, poolRoot);
    }

    private void OnGetFromPool(Coin coin)
    {
        if (coin == null) return;
        coin.gameObject.SetActive(true);
        coin.transform.SetParent(coinRoot, worldPositionStays: true);
    }

    private void OnReleaseToPool(Coin coin)
    {
        if (coin == null) return;
        coin.transform.SetParent(poolRoot, worldPositionStays: true);
        coin.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Coin coin)
    {
        if (coin == null) return;
        Destroy(coin.gameObject);
    }
}
