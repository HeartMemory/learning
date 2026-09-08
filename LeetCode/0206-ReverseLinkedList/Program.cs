// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
ListNode r1 = sol.ReverseList(Build(new int[] { 1, 2, 3, 4, 5 }));
Console.WriteLine(Show(r1)); // 期望 5→4→3→2→1

ListNode r2 = sol.ReverseList(Build(new int[] { 1, 2 }));
Console.WriteLine(Show(r2)); // 期望 2→1

ListNode r3 = sol.ReverseList(null);
Console.WriteLine(Show(r3)); // 期望（空）

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
    public ListNode ReverseList(ListNode head) {
        ListNode dummy = new ListNode();
        ListNode cur = head;
        while(cur != null)
        {
            ListNode next = cur.next;
            cur.next = dummy.next;
            dummy.next = cur;
            cur = next;
        }
        return dummy.next;
        // TODO: 原地反转整个链表，返回新头
        // 经典三指针：prev(已反转部分的头) / cur(正在处理的节点) / next(提前存好的后继)
        // 每轮：next 存好后继 → cur.next 掉头指向 prev → prev/cur 各前进一步
        // 循环结束后 prev 就是新头。注意：原头节点的 next 必须置 null（否则成环！）
    }
}
