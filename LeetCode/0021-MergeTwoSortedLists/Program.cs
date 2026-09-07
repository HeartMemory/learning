// ═══════ 顶级语句区（测试）═══════
using System.Formats.Asn1;

Solution sol = new Solution();
ListNode m1 = sol.MergeTwoLists(Build(new int[] { 1, 2, 4 }), Build(new int[] { 1, 3, 4 }));
Console.WriteLine(Show(m1)); // 期望 1→1→2→3→4→4

ListNode m2 = sol.MergeTwoLists(null, null);
Console.WriteLine(Show(m2)); // 期望（空）

ListNode m3 = sol.MergeTwoLists(null, Build(new int[] { 0 }));
Console.WriteLine(Show(m3)); // 期望 0

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
    public ListNode MergeTwoLists(ListNode list1, ListNode list2) {
        ListNode dummy = new ListNode();
        ListNode tail = dummy;
        while (true)
        {
            if(list1 == null)
            {
                tail.next = list2;
                return dummy.next;
            }else if(list2 == null)
            {
                tail.next = list1;
                return dummy.next;
            }
            if(list1.val <= list2.val)
            {
                tail.next = list1;
                tail = tail.next;
                list1 = list1.next;
            }
            else
            {
                tail.next = list2;
                tail = tail.next;
                list2 = list2.next;
            }
        }
        // TODO: 合并两个有序链表为一个有序链表，返回头节点
        // 思路（88 题三指针的链表版）：哑节点 dummy + tail 指针
        // 每次比较两个链表的当前头，小的摘下来接到 tail 后面，tail 前进
        // 一方走完后，把另一方整段接上
    }
}
