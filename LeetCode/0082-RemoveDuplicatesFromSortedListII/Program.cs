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
        // TODO: 有序链表，删除所有「值重复出现」的节点——重复的全删，一个不留
        // 与 83 的区别：83 保留一个（cur 跳过重复即可）；本题整族删除
        // 哑节点是必需品：如果头节点本身重复，新头会变（t2 测试）
        // 思路提示：dummy + prev（结果链尾）+ cur（探测指针）
        //   cur 沿探测：只要 cur.val == cur.next.val 就一直前移（跳过整族重复）
        //   探测停下后：若 cur 就是 prev.next（没有跳过任何节点）→ 接上、prev 前进
        //              若跳过了整族 → prev.next = cur（直接跨接），prev 不动（cur 可能还是重复的）
        //   循环条件：cur != null（访问 cur.next 前先判空）
    }
}
