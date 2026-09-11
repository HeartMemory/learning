// ═══════ 顶级语句区（测试）═══════
// LeetCode 225 标准样例流程
MyStack st = new MyStack();
st.Push(1);
st.Push(2);
Console.WriteLine(st.Top());    // 期望 2（栈顶=最新进来的）
Console.WriteLine(st.Pop());    // 期望 2（弹出最新的）
Console.WriteLine(st.Empty());  // 期望 False（1 还在）

st.Push(3);
Console.WriteLine(st.Pop());    // 期望 3
Console.WriteLine(st.Pop());    // 期望 1（关键边界：3 弹出后，剩下最早的 1）
Console.WriteLine(st.Empty());  // 期望 True

// 交错压弹（考察"绕圈"次数是否精确）
st.Push(10);
st.Push(20);
st.Push(30);
Console.WriteLine(st.Pop());    // 期望 30
Console.WriteLine(st.Pop());    // 期望 20
st.Push(40);
Console.WriteLine(st.Top());    // 期望 40
Console.WriteLine(st.Pop());    // 期望 40（40 是最后进来的，最先出）
Console.WriteLine(st.Pop());    // 期望 10
Console.WriteLine(st.Empty());  // 期望 True

// ═══════ 类型区 ═══════
public class MyStack {
    private Queue<int> queue = new Queue<int>();

    // TODO 1 (核心·魔法在这里): 入栈——先入队，再让它前面的"老元素"全体绕圈
    //        绕法：把老元素 Dequeue 出来后立刻 Enqueue 回队尾
    //        ⚠️ 陷阱：绕几个？"入队前的元素个数"——循环条件别直接引用 queue.Count！
    //           想清楚：如果用 for + 直接引用 Count，绕圈过程中 Count 会怎样？
    //           （昨天 232 在 inStack.Count 上栽过——这次场景反过来，但同样要防）
    public void Push(int x) {
        int times = queue.Count;
        queue.Enqueue(x);
        while(times != 0)
        {
            queue.Enqueue(queue.Dequeue());
            times--;
        }
    }

    // TODO 2: 出栈——队头就是栈顶，一行解决
    public int Pop() {
        return queue.Dequeue();
    }

    // TODO 3: 看栈顶——同样一行
    public int Top() {
        return queue.Peek();
    }

    // TODO 4: 判空
    public bool Empty() {
        if(queue.Count == 0) return true;
        return false;
    }
}

// ═══════ 提示备忘 ═══════
// Queue<T> API：Enqueue（入队尾）/ Dequeue（出队头，移除）/ Peek（看队头不弹）/ Count
// 与 232 对照：昨天魔法在"出"（倒手翻转顺序），今天魔法在"入"（绕圈把新元素顶到队头）
