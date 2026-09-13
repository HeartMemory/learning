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

    // TODO（盲写重做）：入队
    public void Push(int a) {inStack.Push(a);}

    // TODO（盲写重做）：出队——返回队头元素并移除
    public int Pop()
    {
        if(outStack.Count == 0)
        {
            while(inStack.Count != 0)
            {
                outStack.Push(inStack.Pop());
            }
        }
        return outStack.Pop();
    }

    // TODO（盲写重做）：看队头——返回队头元素但不移除
    public int Peek()
    {
        if(outStack.Count == 0)
        {
            while(inStack.Count != 0)
            {
                outStack.Push(inStack.Pop());
            }
        }
        return outStack.Peek();
    }

    // TODO（盲写重做）：判空——队列为空时返回 true
    public bool Empty()
    {
        if(inStack.Count == 0 && outStack.Count == 0) return true;
        return false;
    }
}

// 验收标准：dotnet run 后 11 行输出与测试区注释中的期望完全一致
