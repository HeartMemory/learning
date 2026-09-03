# LeetCode 刷题记录

> 一律用 **C#** 实现。每题一个独立目录：`题号-题名/`，一次 commit 一道题。
> 索引按**难度分类**，组内按题号排序。

## 简单（10 题）

| 题号 | 题名 | AC 日期 | 解法 | 目录 |
|---|---|---|---|---|
| 1 | 两数之和 | 8/28 | 暴力解法（双层循环）O(n²) | [0001-TwoSum](0001-TwoSum/) |
| 66 | 加一 | 8/28 | 末位遍历进位（9→0，否则 +1 返回；全 9 扩容）O(n) | [0066-PlusOne](0066-PlusOne/) |
| 125 | 验证回文串 | 9/3 | IsLetterOrDigit 过滤 + ToLower + 双指针 | [0125-ValidPalindrome](0125-ValidPalindrome/) |
| 242 | 有效的字母异位词 | 9/3 | int[26] 计数 +1/-1 全零校验 O(n) | [0242-ValidAnagram](0242-ValidAnagram/) |
| 283 | 移动零 | 8/29 | 一次遍历覆盖写（零计数器）O(n)（[错题本02](../Notes/错题本.md)） | [0283-MoveZeroes](0283-MoveZeroes/) |
| 344 | 反转字符串 | 9/3 | 双指针原地交换 O(n) | [0344-ReverseString](0344-ReverseString/) |
| 485 | 最大连续 1 的个数 | 8/28 | 一次遍历 + 计数器 O(n) | [0485-MaxConsecutiveOnes](0485-MaxConsecutiveOnes/) |
| 88 | 合并两个有序数组 | 9/1 | 三指针从后往前原地合并 O(m+n)（[错题本04](../Notes/错题本.md)） | [0088-MergeSortedArray](0088-MergeSortedArray/) |
| 118 | 杨辉三角 | 9/1 | 嵌套 List<List<int>> 逐行生成 O(n²) | [0118-PascalsTriangle](0118-PascalsTriangle/) |
| 121 | 买卖股票的最佳时机 | 8/29 | 单向遍历 + 历史最低价 O(n)（[错题本01](../Notes/错题本.md)） | [0121-BestTimeToBuyAndSellStock](0121-BestTimeToBuyAndSellStock/) |

## 中等（1 题）

| 题号 | 题名 | AC 日期 | 解法 | 目录 |
|---|---|---|---|---|
| 53 | 最大子数组和 | 8/30 | Kadane：单向遍历，延续/重启取大 O(n)（[错题本03](../Notes/错题本.md)） | [0053-MaximumSubarray](0053-MaximumSubarray/) |

## 计划（Block 1：字符串收尾 → 链表 → 栈与队列）

- [x] 字符串题 ×3（344 / 242 / 125，09-03 补刷完成 ✓）
- [ ] 字符串题 ×1-2（09-04）
- [ ] 链表入门 1-2 题（09-07 起）
- [ ] 栈与队列入门 1-2 题（09-11 起）
- [ ] 回顾错题 ×2（09-06 复盘日）

> 进阶预告：两数之和的哈希表解法（O(n)）—— 第 5 周学完 Dictionary 后回来优化。
