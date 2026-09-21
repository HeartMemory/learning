using System.Collections.Generic;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [SerializeField] private GameObject brickPrefab;   // 模具
    [SerializeField] private int cols = 8;             // 列数
    [SerializeField] private int rows = 4;             // 行数
    [SerializeField] private float stepX = 1.7f;       // 横向间距（砖宽 1.6 + 缝 0.1）
    [SerializeField] private float stepY = 0.8f;       // 纵向间距（砖高 0.6 + 缝 0.2）
    [SerializeField] private Vector2 origin = new Vector2(-5.95f, 3.5f);  // 左上角第一块

    private readonly List<GameObject> _bricks = new List<GameObject>();

    void Start()
    {
        SpawnBricks();
    }

    void SpawnBricks()
    {
        if (cols <= 0 || rows <= 0) return;                       // 空/负数直接不生成
        if (cols * rows > 200)                                    // 保险丝：超过上限就拒绝
        {
            Debug.LogError($"砖块数量异常：{cols} × {rows} = {cols * rows}，已拒绝生成");
            return;
        }
        Debug.Log($"准备生成 {cols * rows} 块砖");
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // TODO 1：算出这一块的坐标
                //   x = origin.x + col * stepX   （列号往右 → 加）
                //   y = origin.y - row * stepY   （行号往下 → 减！想想为什么是减）
                //   把两个数组装成一个 Vector2 或 Vector3

                float x = origin.x + col * stepX;
                float y = origin.y - row * stepY;

                // TODO 2：Instantiate(brickPrefab, 位置, Quaternion.identity)
                //   注意：它【有返回值】—— 返回刚造出来的那个对象的引用

                GameObject brick = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                brick.transform.SetParent(transform);   // 挂到 Board 底下：层级窗口不炸开，也方便统一管理/回收

                // TODO 3：把返回的引用 Add 进 _bricks

                _bricks.Add(brick);
            }
        }
        Debug.Log($"生成了 {_bricks.Count} 块砖");   // 这句留着，用来验收
    }

}
