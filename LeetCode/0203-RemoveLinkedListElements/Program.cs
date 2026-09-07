// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();
ListNode t1 = Build(new int[] { 1, 2, 6, 3, 4, 5, 6 });
sol.RemoveElements(t1, 6);
Console.WriteLine(Show(t1)); // 期望 1→2→3→4→5

ListNode t2 = Build(new int[] { });
sol.RemoveElements(t2, 1);
Console.WriteLine(Show(t2)); // 期望（空）

ListNode t3 = Build(new int[] { 7, 7, 7, 7 });
ListNode r3 = sol.RemoveElements(t3, 7);
Console.WriteLine(Show(r3)); // 期望（空）——注意：头节点也要被删时返回的可能是 null！

// ═══════ 类型与工具区（文件底部）═══════
static ListNode Build(int[] vals) {          // 把数组串成链表：1→2→6→3
    ListNode head = null;
    ListNode tail = null;
    foreach (int v in vals) {
        ListNode node = new ListNode(v);
        if (head == null) { head = node; tail = node; }
        else { tail.next = node; tail = node; }
    }
    return head;
}

static string Show(ListNode head) {          // 遍历打印：head 走到 null 为止
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
    public ListNode RemoveElements(ListNode head, int val) {
        ListNode dummy = new ListNode();
        ListNode tail = dummy;
        while (head != null)
        {
            if(head.val != val)
            {
                tail.next = head;
                tail = tail.next;
            }else{tail.next = null;}
            head = head.next;
        }
        return dummy.next;
        // TODO: 删除链表中所有值为 val 的节点，返回新链表的头
        // ⚠️ 链表的坑：节点删不掉「自己」，只能被「前一个」跳过——头节点没有前一个！
        // 哑节点技巧（推荐）：造一个假节点 dummy 挂在 head 前面，从头遍历，最后返回 dummy.next
    }
}
