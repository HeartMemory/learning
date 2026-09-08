// ═══════ 顶级语句区（测试）═══════
Solution sol = new Solution();

// 手工造带环链表：1→2→3→4→2（4 的 next 指回节点 2）
ListNode a = new ListNode(1);
ListNode b = new ListNode(2);
ListNode c = new ListNode(3);
ListNode d = new ListNode(4);
a.next = b; b.next = c; c.next = d; d.next = b;   // 成环！
Console.WriteLine(sol.HasCycle(a)); // 期望 true

ListNode x = new ListNode(1);
ListNode y = new ListNode(2);
x.next = y;                                          // 不成环
Console.WriteLine(sol.HasCycle(x)); // 期望 false

Console.WriteLine(sol.HasCycle(null)); // 期望 false（边界：空链表）

// ═══════ 类型与工具区（文件底部）═══════
public class ListNode {
    public int val;
    public ListNode next;
    public ListNode(int val = 0, ListNode next = null) {
        this.val = val;
        this.next = next;
    }
}

public class Solution {
    public bool HasCycle(ListNode head) {
        ListNode slow = head;
        ListNode fast = head;
        while (true)
        {
            if(fast != null && fast.next != null)
            {
                fast = fast.next.next;
                slow = slow.next;
                if(slow == fast)
                {
                    return true;
                }
            }else{return false;}
        }
        // TODO: 判断链表中是否有环
        // ⚠️ 有环链表千万不能 while(cur != null) 遍历——永远走不到头，死循环！
        // 快慢指针（Floyd 判圈）：slow 每次走 1 步，fast 每次走 2 步
        // 无环 → fast 先到 null；有环 → fast 会从后面追上 slow（两指针相等）
        // 提醒：fast 一次跳两格，访问 fast.next.next 前要先确认 fast 和 fast.next 都不是 null
    }
}
