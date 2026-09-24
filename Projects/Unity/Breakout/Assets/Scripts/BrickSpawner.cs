using System.Collections.Generic;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private int cols = 8;
    [SerializeField] private int rows = 4;
    [SerializeField] private float stepX = 1.7f;
    [SerializeField] private float stepY = 0.8f;
    [SerializeField] private Vector2 origin = new Vector2(-5.95f, 3.5f);

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();  // 库房（空闲的砖）
    private int _rentCount;         // 借出总次数（验收用）
    private int _instantiateCount;  // 真正 new 的次数（验收用：应该只涨到 32）

    void Start()
    {
        Preload();
        FillBoard();
        Debug.Log($"预分配 {_instantiateCount} 块 ｜ 池中剩余 {_pool.Count} ｜ 借出 {_rentCount} 次");
    }

    void Preload()
    {
        // TODO 1：一次性造够 cols × rows 块
        //   每块：Instantiate(brickPrefab, transform) → SetActive(false) → 注入（Init）→ 入池
        //   ★ 保险丝照旧（cols<=0 / cols*rows>200 直接拒绝）
        //   ★ _instantiateCount++
        if (cols <= 0 || rows <= 0)
        {
            Debug.LogError("列数或行数不合法，无法摆放！");
            return;
        }
        if (cols * rows > 200)
        {
            Debug.LogError("砖块总数超过 200，无法摆放！");
            return;
        }
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject brick = Instantiate(brickPrefab, transform);
                brick.SetActive(false);
                if (!brick.TryGetComponent<Brick>(out var brickScript))
                {
                    Debug.LogError($"{brick.name}没有 Brick 脚本组件，无法注入池！");
                    return;
                }
                brickScript.Init(this);
                _pool.Enqueue(brick);
                _instantiateCount++;
            }
        }
    }

    void FillBoard()
    {
        // TODO 2：把砖摆到原来的位置
        //   每块：从池借出 → 设置位置（用 transform.position，Board 在原点，值等同）
        //   坐标公式照抄你 09-19 写的两行
        //   ★ 借出返回 null 时怎么办？（你 B 的决策是"响亮地失败"，这里别静默跳过）
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject brick = Rent();
                if (brick == null)
                {
                    Debug.LogError("砖块池空了，无法摆放！");
                    return;
                }
                brick.transform.position = new Vector2(origin.x + stepX * c, origin.y - stepY * r);
                brick.SetActive(true);
            }
        }
    }

    private GameObject Rent()
    {
        // TODO 3：借出
        //   池空 → Debug.LogError + return null
        //   否则 Dequeue → SetActive(true) → _rentCount++ → return
        if(_pool.Count == 0)
        {
            Debug.LogError("砖块池空了，无法借出！");
            return null;
        }
        GameObject brick = _pool.Dequeue();
        _rentCount++;
        return brick;
    }

    public void Return(GameObject brick)
    {
        // TODO 4：归还（决策 C 的四条全在这儿用上）
        if(brick == null)
        {
            Debug.LogError("归还的砖块是 null！");
            return;
        }
        if(brick.transform.parent != transform)
        {
            Debug.LogError("归还的砖块不属于这个池！");
            return;
        }
        if (!brick.activeSelf) return;
        brick.SetActive(false);
        _pool.Enqueue(brick);
    }
    public int RemainingBricks => _instantiateCount - _pool.Count;
    public int TotalBricks => _instantiateCount;
}
