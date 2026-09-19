# LeetCode 刷题记录

> 一律用 **C#** 实现。每题一个独立目录：`题号-题名/`，一次 commit 一道题。
> 索引按**难度分类**，组内按**题号升序**排序。

## 简单（28 题）

| 题号 | 题名 | AC 日期 | 解法 | 目录 |
|---|---|---|---|---|
| 1 | 两数之和 | 8/28（09-14 哈希重写） | 暴力 O(n²) + **哈希表 O(n)**：Dictionary 存「数字→下标」，先查互补数再存自己 | [0001-TwoSum](0001-TwoSum/) |
| 13 | 罗马数字转整数 | 9/4 | 逐字符查值 + 右邻比较定加减 O(n) | [0013-RomanToInteger](0013-RomanToInteger/) |
| 20 | 有效的括号 | 9/9 | 栈就近配对：遇左括号压"期待右括号"，右括号来比栈顶；空栈防线 + 终检栈空 O(n) | [0020-ValidParentheses](0020-ValidParentheses/) |
| 21 | 合并两个有序链表 | 9/7 | 哑节点 + tail 逐个摘取 O(m+n)；递归版（09-09 重写：终止条件 + 摘头递归，信任返回值接线） | [0021-MergeTwoSortedLists](0021-MergeTwoSortedLists/) |
| 66 | 加一 | 8/28 | 末位遍历进位（9→0，否则 +1 返回；全 9 扩容）O(n) | [0066-PlusOne](0066-PlusOne/) |
| 83 | 删除排序链表中的重复元素 | 9/8 | 单指针原地跳过：有序 → 重复必相邻，相等跳过 cur 不动 O(n) | [0083-RemoveDuplicatesFromSortedList](0083-RemoveDuplicatesFromSortedList/) |
| 88 | 合并两个有序数组 | 9/1 | 三指针从后往前原地合并 O(m+n)（[错题本04](../Notes/错题本.md)） | [0088-MergeSortedArray](0088-MergeSortedArray/) |
| 118 | 杨辉三角 | 9/1 | 嵌套 List<List<int>> 逐行生成 O(n²) | [0118-PascalsTriangle](0118-PascalsTriangle/) |
| 121 | 买卖股票的最佳时机 | 8/29 | 单向遍历 + 历史最低价 O(n)（[错题本01](../Notes/错题本.md)） | [0121-BestTimeToBuyAndSellStock](0121-BestTimeToBuyAndSellStock/) |
| 125 | 验证回文串 | 9/3 | IsLetterOrDigit 过滤 + ToLower + 双指针 | [0125-ValidPalindrome](0125-ValidPalindrome/) |
| 141 | 环形链表 | 9/8 | 快慢指针判圈（Floyd）O(n) 时间 O(1) 空间 | [0141-LinkedListCycle](0141-LinkedListCycle/) |
| 202 | 快乐数 | 9/16 | HashSet 判环：每步平方和登记，`Add` 返回 false 即回到老路 O(log n)（踩坑见[错题本](../Notes/错题本.md)） | [0202-HappyNumber](0202-HappyNumber/) |
| 203 | 移除链表元素 | 9/7 | 哑节点 + tail 原地拼接，断悬尾 O(n) | [0203-RemoveLinkedListElements](0203-RemoveLinkedListElements/) |
| 205 | 同构字符串 | 9/17 | **双射（bijection）**：两个 Dictionary 双向守门——`s→t` 管"同一字符不变心"、`t→s` 管"不同字符不撞车"（单字典会漏 `"badc"/"baba"`）O(n) | [0205-IsomorphicStrings](0205-IsomorphicStrings/) |
| 206 | 反转链表 | 9/8 | 头插法：存后继 → 接结果头 → 前进 O(n) | [0206-ReverseLinkedList](0206-ReverseLinkedList/) |
| 217 | 存在重复元素 | 9/14 | HashSet：用 `Add` 返回值一步完成「查+登记」，返回 false 即撞重复 O(n) | [0217-ContainsDuplicate](0217-ContainsDuplicate/) |
| 225 | 用队列实现栈 | 9/11 | 单队列绕圈：新元素入队后老元素"出队即入队"绕一圈顶到队头 O(n)（对照双队列中转版，两种解法都写过） | [0225-ImplementStackUsingQueues](0225-ImplementStackUsingQueues/) |
| 232 | 用栈实现队列 | 9/10 | 双栈倒手：inStack 管进、outStack 管出；outStack 空时倒手一次，顺序翻转 O(n) 均摊（陷阱：for 条件引用会变的 Count） | [0232-ImplementQueueUsingStacks](0232-ImplementQueueUsingStacks/) |
| 242 | 有效的字母异位词 | 9/3 | int[26] 计数 +1/-1 全零校验 O(n) | [0242-ValidAnagram](0242-ValidAnagram/) |
| 283 | 移动零 | 8/29 | 一次遍历覆盖写（零计数器）O(n)（[错题本02](../Notes/错题本.md)） | [0283-MoveZeroes](0283-MoveZeroes/) |
| 290 | 单词规律 | 9/18 | **205 的文字版**：`Split(' ')` 切词 + `char ↔ string` **双字典**双向守门；**长度前置检查**（个数不等直接 False，防越界）O(n) | [0290-WordPattern](0290-WordPattern/) |
| 344 | 反转字符串 | 9/3 | 双指针原地交换 O(n) | [0344-ReverseString](0344-ReverseString/) |
| 349 | 两个数组的交集 | 9/15 | HashSet 双版本：手动 `Contains` 收集 vs `IntersectWith` 集合运算 O(n)（后者原地修改、返回 void） | [0349-IntersectionOfTwoArrays](0349-IntersectionOfTwoArrays/) |
| 350 | 两个数组的交集 II | 9/19 | **计数表 + "库存取货"**：给 nums1 建 Dictionary 计数，扫 nums2 每命中取走一件（计数减到 0 即取完 → 自动实现"次数取较小值"）O(m+n)（349 的去重版会丢次数，不能复用） | [0350-IntersectionOfTwoArraysII](0350-IntersectionOfTwoArraysII/) |
| 383 | 赎金信 | 9/15 | 字符计数消耗：`TryGetValue` 默认值 0 让「不存在」与「配额用完」共用一条判断 O(n) | [0383-RansomNote](0383-RansomNote/) |
| 387 | 字符串中的第一个唯一字符 | 9/16 | 两趟：Dictionary 计数 + 再扫首个计数为 1 的下标 O(n)（对比 `int[26]` 特化哈希表） | [0387-FirstUniqueCharacter](0387-FirstUniqueCharacter/) |
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
- [x] 栈与队列入门（09-09 晚提前：20 有效的括号 ✓）
- [x] 232 用栈实现队列（09-10 ✓——双栈倒手模型）
- [x] 225 用队列实现栈（09-11 ✓——单队列绕圈 + 双队列中转，两种解法对照）
- [x] 回顾错题（09-13 复盘日：82/232 盲写重做，发现两个记忆盲区 ✓）
- [x] **哈希表线开启**（09-14 ✓：1 两数之和哈希重写 + 217 存在重复元素）
- [x] 383 赎金信 + 349 两个数组的交集（09-15 ✓）
- [x] 202 快乐数 + 387 第一个唯一字符（09-16 ✓——哈希表线：判环 + 计数两趟）
- [x] 205 同构字符串（09-17 ✓——**双射**：单字典只能守一个方向的门）
- [x] 290 单词规律（09-18 ✓——同一思路的"文字版"：两侧类型不同（`char` ↔ `string`）+ **先挡长度不等的前提**再进循环）
- [x] 350 两个数组的交集 II（09-19 ✓——计数表"库存取货"；顺手修掉"没命中却给字典插 -1"的脏写入）

> 进阶预告：两数之和的哈希表解法（O(n)）—— 第 5 周学完 Dictionary 后回来优化。
