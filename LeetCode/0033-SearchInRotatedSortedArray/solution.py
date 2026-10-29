# 33. 搜索旋转排序数组 —— 二分查找（转型后第一题 · Python）
#
# 目的：一条升序"尺子"被掰断、后半截挪到前面 ⇒ 全局无序，但**被切成两段各自升序**。
#       要求 O(log n) ⇒ 必须二分，不能在无序的全局上二分，而要**先在每步里找出有序的那一半**。
#
# 判据（写完逐条自查）：
#   1. 循环不变式：每次收缩后，target 若存在，**一定仍在 [lo, hi] 里**。
#   2. `nums[lo] <= nums[mid]` 的 `=` 不能丢（lo == mid 时左半只有一个元素，它一定有序）。
#   3. 收缩必须让区间**严格变小**（`lo = mid` 在 mid == lo 时就是死循环）。
#
# 运行： python solution.py
import io
import random
import sys
import time


class Solution:
    def search(self, nums: list[int], target: int) -> int:
        """返回 target 在 nums 中的下标；不存在返回 -1。要求 O(log n)。"""
        "TODO 你来写：二分；每步先判【哪一半连续（有序）】，"
        "再在那一半上做普通闭区间包含判断，据此收缩另一侧"
        min = 0;
        max = len(nums) - 1;
        while min <= max:
            mid = (min + max) // 2
            if nums[mid] == target:
                return mid
            if nums[min] <= nums[mid]:  # 左半有序
                if nums[min] <= target < nums[mid]:  # target 在左半
                    max = mid - 1
                else:  # target 在右半
                    min = mid + 1
            else:  # 右半有序
                if nums[mid] < target <= nums[max]:  # target 在右半
                    min = mid + 1
                else:  # target 在左半
                    max = mid - 1
        return -1
    


# ==================== 判题器（脚手架 · 不用改） ====================
# 版本登记表：新写一版解法就往这里加一个实例，否则它一次都不会被跑到
_VERSIONS: list[tuple[str, Solution]] = [("主解", Solution())]

# (nums, target, 期望下标, 说明) —— 期望值全部独立手算，不抄解法输出
_CASES: list[tuple[list[int], int, int, str]] = [
    ([4, 5, 6, 7, 0, 1, 2], 0, 4, "官方示例 1：命中在断口右侧第一格"),
    ([4, 5, 6, 7, 0, 1, 2], 3, -1, "官方示例 2：不存在（落在断口里）"),
    ([1], 0, -1, "官方示例 3：单元素不命中"),
    ([1], 1, 0, "单元素命中"),
    ([3, 1], 1, 1, "n=2 已旋转：命中后半"),
    ([3, 1], 3, 0, "n=2 已旋转：命中前半（lo 就是答案）"),
    ([2, 3, 1], 1, 2, "断口在最右"),
    ([3, 1, 2], 1, 1, "断口在中：目标在断口右侧第一格"),
    ([1, 2, 3, 4, 5], 5, 4, "k=0 未旋转：命中末格"),
    ([1, 2, 3, 4, 5], 0, -1, "未旋转：比全体都小"),
    ([1, 2, 3, 4, 5], 6, -1, "未旋转：比全体都大"),
    ([4, 5, 6, 7, 8, 1, 2, 3], 8, 4, "断口两侧各取一格：断口左侧"),
    ([4, 5, 6, 7, 8, 1, 2, 3], 1, 5, "断口两侧各取一格：断口右侧"),
    ([6, 7, 1, 2, 3, 4, 5], 6, 0, "旋转后第一格 = 原最大值"),
    ([6, 7, 1, 2, 3, 4, 5], 5, 6, "最后一格 = 原次小"),
    ([-5, -3, -1, -9, -7], -9, 3, "含负数"),
    ([-5, -3, -1, -9, -7], -4, -1, "含负数：落在断口的空隙里"),
    ([10, 20, 30, 40, 50, 60, 70, 80, 90, 100], 90, 8, "大数组未旋转取倒数第二"),
]


def _is_rotation_of_sorted(nums: list[int]) -> bool:
    """前提自检：nums 必须是「某个升序数组旋转 k 格」的结果（按定义逐个 k 试，最硬）。"""
    s = sorted(nums)
    n = len(s)
    return any(nums == s[k:] + s[:k] for k in range(n))


def _run_cases() -> tuple[int, int]:
    print("=" * 74)
    print("① 前提自检（尺子先校准：这批用例本身是不是合法输入？）")
    print("=" * 74)
    premise_ok = True
    for i, (nums, target, expected, note) in enumerate(_CASES, 1):
        legal = _is_rotation_of_sorted(nums) and len(set(nums)) == len(nums)
        # 期望值的第二个独立来源：Python 自带的列表查找
        truth = nums.index(target) if target in nums else -1
        agree = truth == expected
        if not (legal and agree):
            premise_ok = False
        flag = "✅" if legal and agree else "❌"
        print(f"  {flag} {i:02d} len={len(nums):2d} {note}（独立对账 {truth} == {expected}）")
    print(f"  ⇒ 前提区：{'全部合法' if premise_ok else '有用例不合法，下面的判分不算数'}\n")

    print("=" * 74)
    print("② 期望值比对（解法 vs 手算期望）")
    print("=" * 74)
    total = 0
    passed = 0
    for name, solver in _VERSIONS:
        print(f"  [{name}]")
        for i, (nums, target, expected, note) in enumerate(_CASES, 1):
            total += 1
            try:
                got = solver.search(list(nums), target)
            except Exception as exc:  # noqa: BLE001 - 判题器要如实报错，不吞
                print(f"    ❌ {i:02d} {note} → 抛 {type(exc).__name__}: {exc}")
                continue
            if got == expected:
                passed += 1
                print(f"    ✅ {i:02d} {note} → {got}")
            else:
                print(f"    ❌ {i:02d} {note} → 得到 {got}，期望 {expected}")
        print()
    return passed, total


def _random_check(rounds: int = 400) -> tuple[int, int]:
    print("=" * 74)
    print("③ 全量对账（随机升降数组 × 逐个 target 与暴力扫描比）")
    print("=" * 74)
    rng = random.Random(20261029)
    tried = 0
    bad = 0
    for _ in range(rounds):
        n = rng.randint(1, 12)
        base = sorted(rng.sample(range(-30, 31), n))
        k = rng.randrange(n)
        nums = base[k:] + base[:k]
        for _, solver in _VERSIONS:
            for target in range(-31, 32):
                tried += 1
                try:
                    got = solver.search(list(nums), target)
                except Exception:  # noqa: BLE001
                    bad += 1
                    continue
                want = nums.index(target) if target in nums else -1
                if got != want:
                    bad += 1
                    if bad <= 3:
                        print(f"    ❌ nums={nums} target={target} → 得到 {got}，期望 {want}")
    print(f"  ⇒ {rounds} 组随机数组 / {tried} 次查询，**{bad} 处不符**\n")
    return tried, bad


def _bench(n: int = 1_000_000) -> None:
    print("=" * 74)
    print("④ 性能探针（O(log n) 该比线性快，不是「感觉快」）")
    print("=" * 74)
    base = list(range(n))
    k = n // 3
    nums = base[k:] + base[:k]
    target = n - 2  # 落在原数组靠后的位置

    _, solver = _VERSIONS[0]
    t0 = time.perf_counter()
    hit = solver.search(nums, target)
    t1 = time.perf_counter()

    t2 = time.perf_counter()
    linear = -1
    for i, v in enumerate(nums):  # 线性基线：只算期望，不参与解法比较
        if v == target:
            linear = i
            break
    t3 = time.perf_counter()

    print(f"  二分：{hit:>9d}   {(t1 - t0) * 1e6:8.2f} µs")
    print(f"  线性：{linear:>9d}   {(t3 - t2) * 1e6:8.2f} µs   （基线，n = {n:,}）")
    print(f"  ⇒ 两条索引必须相同：{hit} == {linear} → {'✅' if hit == linear else '❌'}\n")


if __name__ == "__main__":
    # Windows 控制台默认 GBK，打 ✅ / ❌ 会 UnicodeEncodeError ⇒ 强制 UTF-8 输出
    if isinstance(sys.stdout, io.TextIOWrapper):
        sys.stdout.reconfigure(encoding="utf-8")

    p, t = _run_cases()
    print(f"② 期望值比对：**{p} / {t} 全绿**\n" if p == t else f"② 期望值比对：{p} / {t}（有红）\n")
    _random_check()
    try:
        _bench()
    except Exception as exc:  # noqa: BLE001
        print(f"④ 性能探针：跳过（解法还没跑通 —— {type(exc).__name__}）\n")
