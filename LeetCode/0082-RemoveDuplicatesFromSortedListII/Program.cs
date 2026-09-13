// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
ListNode t1 = Build(new int[] { 1, 2, 3, 3, 4, 4, 5 });
ListNode r1 = sol.DeleteDuplicates(t1);
Console.WriteLine(Show(r1)); // 期望 1→2→5

ListNode t2 = Build(new int[] { 1, 1, 1, 2, 3 });
ListNode r2 = sol.DeleteDuplicates(t2);
Console.WriteLine(Show(r2)); // 期望 2→3（1 整族被删，新头变成 2——哑节点必须有！）

ListNode t3 = Build(new int[] { 1, 1 });
ListNode r3 = sol.DeleteDuplicates(t3);
Console.WriteLine(Show(r3)); // 期望（空）

ListNode t4 = Build(new int[] { });
Console.WriteLine(Show(sol.DeleteDuplicates(t4))); // 期望（空）

// ── 复盘日补充：重复族在【末尾】的边界（盲写暴露的测试盲区）──
ListNode t5 = Build(new int[] { 1, 2, 2 });
Console.WriteLine(Show(sol.DeleteDuplicates(t5))); // 期望 1（末尾的 2 族整族删掉，不能留尾巴）

ListNode t6 = Build(new int[] { 1, 1, 2, 2 });
Console.WriteLine(Show(sol.DeleteDuplicates(t6))); // 期望（空）（两个重复族全删光）

ListNode t7 = Build(new int[] { 1, 2, 3, 3, 4, 4 });
Console.WriteLine(Show(sol.DeleteDuplicates(t7))); // 期望 1→2（末尾连续两个重复族）

// ═══════ 类型与工具区（文件底部）═══════
static ListNode Build(int[] vals) {
    ListNode head = null;
    ListNode tail = null;
    foreach (int v in vals) {
        ListNode node = new ListNode(v);
        if (head == null) { head = node; tail = node; }
        else { tail.next = node; tail = node; }
    }
    return head;
}

static string Show(ListNode head) {
    var parts = new List<string>();
    ListNode cur = head;
    while (cur != null) { parts.Add(cur.val.ToString()); cur = cur.next; }
    return string.Join("→", parts);
}

public class ListNode {
    public int val;
    public ListNode next;
    public ListNode(int val = 0, ListNode next = null) {
        this.val = val;
        this.next = next;
    }
}

public class Solution {
    // TODO（盲写重做）：有序链表中，删除所有「值重复出现」的节点——重复的全删，一个不留
    //   要求：通过上方 4 组测试（常规 / 头节点重复 / 两元素全重复 / 空链表）
    //   验收：dotnet run 后 4 行输出与注释中的期望完全一致
    public ListNode DeleteDuplicates(ListNode head) {
        ListNode dummy = new ListNode();
        ListNode cur = head;
        ListNode prev = dummy;
        while(cur != null)
        {
            if(cur.next != null && cur.val == cur.next.val)
            {
                while(cur.next != null && cur.val == cur.next.val) cur = cur.next;
                    cur = cur.next;
                    prev.next = cur; 
            }
            else
            {
                prev.next = cur;
                prev = prev.next;
                cur = cur.next;
            }
        }
        return dummy.next;
    }

    // ══════════ 等价优化版（复习对照用，未启用）══════════════════════════
    // 思路：跨接只在 else 分支做（省掉 if 里"跳过即接"），循环结束后统一断悬尾
    //
    //   while (cur != null) {
    //       if (cur.next != null && cur.val == cur.next.val) {
    //           while (cur.next != null && cur.val == cur.next.val) cur = cur.next;
    //           cur = cur.next;              // 跳过整族后「不接」，留给下一轮 else
    //       } else {
    //           prev.next = cur;             // 唯一的接链点
    //           prev = prev.next;
    //           cur = cur.next;
    //       }
    //   }
    //   prev.next = null;                    // ★ 收尾断悬尾（否则末尾可能挂着已删除节点）
    //   return dummy.next;
    //
    // ── 两版对比 ──
    //   当前版（上面已实现）：跨接在 if 里完成——跳过整族立刻接上
    //     · 优点：链在任何时刻都保持完整，不依赖"后面还有没有节点"（末尾重复族也安全）
    //     · 代价：重复族在中间时，同一赋值会在 else 里再做一次（幂等，无害）
    //
    //   优化版（本注释）：跨接只在 else 完成 + 循环外断尾
    //     · 优点：赋值次数最少
    //     · 代价：必须记得收尾断尾——忘了就是悬尾 bug，而且它离主逻辑很远、忘得很自然
    //
    //   取舍结论：**保留当前版**——用一次幂等的冗余赋值，换掉一个"必须记得做"的隐形义务。
    //            （幂等的冗余是廉价的，被遗忘的收尾是昂贵的）
    //
    //   关联：203 移除链表元素用的是"悬尾断开"模式——同一思路在不同约束下的两种解法
    // ══════════════════════════════════════════════════════════════════
}
