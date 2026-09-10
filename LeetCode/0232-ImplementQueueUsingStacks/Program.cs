// ═══════ 顶级语句区（测试）═══════
// LeetCode 232 样例流程：push→push→peek→pop→empty
MyQueue q = new MyQueue();
q.Push(1);
q.Push(2);
Console.WriteLine(q.Peek());   // 期望 1（先进先出：1 比 2 先进）
Console.WriteLine(q.Pop());    // 期望 1（出队出的是最早进来的）
Console.WriteLine(q.Empty());  // 期望 False（2 还在）

q.Push(3);
Console.WriteLine(q.Peek());   // 期望 2（3 排在 2 后面，队头仍是 2）
Console.WriteLine(q.Pop());    // 期望 2
Console.WriteLine(q.Pop());    // 期望 3
Console.WriteLine(q.Empty());  // 期望 True

// 边界：连续多次倒腾（考察"outStack 不空时不要重复倒"）
q.Push(4);
q.Push(5);
Console.WriteLine(q.Pop());    // 期望 4
q.Push(6);
Console.WriteLine(q.Pop());    // 期望 5（此时 6 在 inStack，5 在 outStack——顺序不能乱）
Console.WriteLine(q.Pop());    // 期望 6
Console.WriteLine(q.Empty());  // 期望 True

// ═══════ 类型区 ═══════
public class MyQueue {
    private Stack<int> inStack = new Stack<int>();
    private Stack<int> outStack = new Stack<int>();

    // TODO 1: 入队——直接压进 inStack 即可（一行）
    public void Push(int a){inStack.Push(a);} 
    // TODO 2 (核心): 私有辅助方法"倒手"——仅当 outStack 为空时执行：
    //                把 inStack 的元素逐个 Pop 出来 Push 进 outStack
    //                想清楚：为什么倒一次手之后，outStack 的栈顶就是"最早的元素"？
    private void EnsureOutHasItems()
    {
        if(outStack.Count != 0) return;
        while (inStack.Count != 0)
        {
            outStack.Push(inStack.Pop());
        }
    }
    // TODO 3: 出队——先确保倒手过，再 Pop outStack 并返回
    public int Pop()
    {
        EnsureOutHasItems();
        return outStack.Pop();
    }
    // TODO 4: 看队头——和出队几乎一样，只是 Pop 换成 Peek
    public int Peek()
    {
        EnsureOutHasItems();
        return outStack.Peek();
    }
    // TODO 5: 判空——两个栈都空才算空
    public bool Empty()
    {
        if(inStack.Count == 0 && outStack.Count == 0) return true;
        return false;
    } 
}

// ═══════ 提示备忘 ═══════
// Stack<T> API：Push / Pop（弹出并返回）/ Peek（看但不弹）/ Count
// 本题精髓："倒手"只发生在 outStack 为空时——outStack 还有东西就绝不能倒，
//           否则 4→5→6 那组测试会暴露顺序错误
