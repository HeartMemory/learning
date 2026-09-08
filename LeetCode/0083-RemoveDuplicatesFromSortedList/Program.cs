// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
ListNode d1 = Build(new int[] { 1, 1, 2, 3, 3 });
sol.DeleteDuplicates(d1);
Console.WriteLine(Show(d1)); // 期望 1→2→3

ListNode d2 = Build(new int[] { 1, 1, 1 });
ListNode r2 = sol.DeleteDuplicates(d2);
Console.WriteLine(Show(r2)); // 期望 1

ListNode d3 = Build(new int[] { });
Console.WriteLine(Show(sol.DeleteDuplicates(d3))); // 期望（空）

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
    public ListNode DeleteDuplicates(ListNode head) {
        ListNode cur = head;
        while(cur != null && cur.next != null)
        {
            if(cur.val == cur.next.val)
            {
                cur.next = cur.next.next;
            }else{cur = cur.next;}
        }
        return head;
        // TODO: 删除有序链表中所有重复的节点，使每个值只出现一次，返回头
        // 关键前提：链表【有序】——重复的值一定相邻！
        // 思路：cur 走一遍，每次比较 cur.val 和 cur.next.val
        //   相等 → cur.next = cur.next.next（跳过重复，cur 不动——下一个可能还重复）
        //   不等 → cur = cur.next（正常前进）
        // 提醒：访问 cur.next.val 前先确认 cur.next 不是 null
    }
}
