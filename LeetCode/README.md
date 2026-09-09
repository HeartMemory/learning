# LeetCode 刷题记录

> 一律用 **C#** 实现。每题一个独立目录：`题号-题名/`，一次 commit 一道题。
> 索引按**难度分类**，组内按**题号升序**排序。

## 简单（17 题）

| 题号 | 题名 | AC 日期 | 解法 | 目录 |
|---|---|---|---|---|
| 1 | 两数之和 | 8/28 | 暴力解法（双层循环）O(n²) | [0001-TwoSum](0001-TwoSum/) |
| 13 | 罗马数字转整数 | 9/4 | 逐字符查值 + 右邻比较定加减 O(n) | [0013-RomanToInteger](0013-RomanToInteger/) |
| 21 | 合并两个有序链表 | 9/7 | 哑节点 + tail 逐个摘取 O(m+n) + 递归版（09-09 重写练习） | [0021-MergeTwoSortedLists](0021-MergeTwoSortedLists/) |
| 66 | 加一 | 8/28 | 末位遍历进位（9→0，否则 +1 返回；全 9 扩容）O(n) | [0066-PlusOne](0066-PlusOne/) |
| 83 | 删除排序链表中的重复元素 | 9/8 | 单指针原地跳过：有序 → 重复必相邻，相等跳过 cur 不动 O(n) | [0083-RemoveDuplicatesFromSortedList](0083-RemoveDuplicatesFromSortedList/) |
| 88 | 合并两个有序数组 | 9/1 | 三指针从后往前原地合并 O(m+n)（[错题本04](../Notes/错题本.md)） | [0088-MergeSortedArray](0088-MergeSortedArray/) |
| 118 | 杨辉三角 | 9/1 | 嵌套 List<List<int>> 逐行生成 O(n²) | [0118-PascalsTriangle](0118-PascalsTriangle/) |
| 121 | 买卖股票的最佳时机 | 8/29 | 单向遍历 + 历史最低价 O(n)（[错题本01](../Notes/错题本.md)） | [0121-BestTimeToBuyAndSellStock](0121-BestTimeToBuyAndSellStock/) |
| 125 | 验证回文串 | 9/3 | IsLetterOrDigit 过滤 + ToLower + 双指针 | [0125-ValidPalindrome](0125-ValidPalindrome/) |
| 141 | 环形链表 | 9/8 | 快慢指针判圈（Floyd）O(n) 时间 O(1) 空间 | [0141-LinkedListCycle](0141-LinkedListCycle/) |
| 203 | 移除链表元素 | 9/7 | 哑节点 + tail 原地拼接，断悬尾 O(n) | [0203-RemoveLinkedListElements](0203-RemoveLinkedListElements/) |
| 206 | 反转链表 | 9/8 | 头插法：存后继 → 接结果头 → 前进 O(n) | [0206-ReverseLinkedList](0206-ReverseLinkedList/) |
| 242 | 有效的字母异位词 | 9/3 | int[26] 计数 +1/-1 全零校验 O(n) | [0242-ValidAnagram](0242-ValidAnagram/) |
| 283 | 移动零 | 8/29 | 一次遍历覆盖写（零计数器）O(n)（[错题本02](../Notes/错题本.md)） | [0283-MoveZeroes](0283-MoveZeroes/) |
| 344 | 反转字符串 | 9/3 | 双指针原地交换 O(n) | [0344-ReverseString](0344-ReverseString/) |
| 389 | 找不同 | 9/4 | 全字符异或，成对抵消 O(n) 时间 O(1) 空间 | [0389-FindTheDifference](0389-FindTheDifference/) |
| 485 | 最大连续 1 的个数 | 8/28 | 一次遍历 + 计数器 O(n) | [0485-MaxConsecutiveOnes](0485-MaxConsecutiveOnes/) |

## 中等（2 题）

| 题号 | 题名 | AC 日期 | 解法 | 目录 |
|---|---|---|---|---|
| 53 | 最大子数组和 | 8/30 | Kadane：单向遍历，延续/重启取大 O(n)（[错题本03](../Notes/错题本.md)） | [0053-MaximumSubarray](0053-MaximumSubarray/) |
| 82 | 删除排序链表中的重复元素 II | 9/9 | 哑节点 + prev/cur 状态机：跳过整族跨接，prev 不动 O(n) | [0082-RemoveDuplicatesFromSortedListII](0082-RemoveDuplicatesFromSortedListII/) |

## 计划（Block 1：字符串收尾 → 链表 → 栈与队列）

- [x] 字符串题 ×5（344 / 242 / 125 / 389 / 13，09-03~09-04 完成 ✓）——字符串线 5/5 达标 🎉
- [x] 链表题 ×6（203 / 21 / 206 / 141 / 83 / 82，09-07~09-09 完成 ✓）——链表周毕业 🎓
- [x] 回顾错题（09-06 复盘日：121/283 盲写重做 + List 戒断练习）
- [ ] 栈与队列入门 1-2 题（09-11 起）

> 进阶预告：两数之和的哈希表解法（O(n)）—— 第 5 周学完 Dictionary 后回来优化。
