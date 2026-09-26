// ═══════ 判题工具（分组题是"顺序无关"的，不能直接比 List）═══════
//   两层排序：① 组内单词排序（"eat"/"tea"/"ate" 谁在前都算对）
//             ② 组与组之间排序（先出哪一组也算对）
//   排完还相等 → 分组结果就是对的。
static List<string> Normalize(IList<IList<string>> groups)
{
    List<string> lines = new List<string>();
    foreach (IList<string> g in groups)
        lines.Add(string.Join("|", g.OrderBy(w => w, StringComparer.Ordinal)));
    return lines.OrderBy(l => l, StringComparer.Ordinal).ToList();
}

static void Check(string title, string[] strs, string[][] expected)
{
    IList<IList<string>> actual = new Solution().GroupAnagrams(strs);
    List<string> a = Normalize(actual);
    List<string> e = Normalize(expected);
    bool ok = a.SequenceEqual(e);
    Console.WriteLine($"{(ok ? "✅" : "❌")} {title}");
    if (!ok)
    {
        Console.WriteLine($"     期望：{string.Join("  /  ", e)}");
        Console.WriteLine($"     实际：{string.Join("  /  ", a)}");
    }
}

// ═══════ 官方约束（09-26 联网核实 · 力扣「提示」原文，设计前先读它）═══════
//   1 <= strs.length <= 10^4         ← 数组至少 1 个词（不用管"空数组"），最多 10^4
//   0 <= strs[i].length <= 100       ← ⚠️ 下限是 0：空串【合法】，官方示例 2 就是 [""]
//   strs[i] 仅包含小写字母            ← 所以 int[26] 计数表可行（进阶①）；Unicode 是"超出承诺"的思考题
//   ⭐ 读约束的三个维度：规模（决定复杂度目标）/ 取值范围（决定要处理的特殊形状）/ 字符集（决定 key 怎么表示）
// ═══════ 用例区（形状优先，不是数量优先）═══════
Check("★ 官方示例 1：三个异位词组",
    new[] { "eat", "tea", "tan", "ate", "nat", "bat" },
    new[] { new[] { "bat" }, new[] { "nat", "tan" }, new[] { "ate", "eat", "tea" } });

Check("★ 官方示例 3：单元素数组（最小规模）",
    new[] { "a" },
    new[] { new[] { "a" } });

// ⚠️ 更正（09-26 联网核实官方「提示」）：空串【在约束内】，不是"额外防守"！
//    官方约束是 0 <= strs[i].length <= 100（允许长度为 0），而且【官方示例 2 就是 [""]】。
//    → 空串是必过用例：key 逻辑只要在空串上崩，这题直接挂。
Check("★ 官方示例 2：一个空串 —— 自己就是一组",
    new[] { "" },
    new[] { new[] { "" } });

Check("★★ 两个空串 —— 空串之间也互为异位词",
    new[] { "", "" },
    new[] { new[] { "", "" } });

Check("★★ 各组长度不一（考试最爱）",
    new[] { "abc", "cba", "bac", "ab", "ba" },
    new[] { new[] { "ab", "ba" }, new[] { "abc", "bac", "cba" } });

Check("★ 没有一个异位词 → 每个词各成一组",
    new[] { "abc", "def" },
    new[] { new[] { "abc" }, new[] { "def" } });

Check("★★ 多组混在一起 + 只差一个字母的词",
    new[] { "aab", "aba", "baa", "abb", "bab", "bba", "abc" },
    new[] { new[] { "aab", "aba", "baa" }, new[] { "abb", "bab", "bba" }, new[] { "abc" } });

Check("★ 全部相同",
    new[] { "z", "z", "z" },
    new[] { new[] { "z", "z", "z" } });

Check("★★ 字符有重复的对照（'aabb' vs 'abab' vs 'abbb'）",
    new[] { "aabb", "abab", "baba", "bbaa", "abbb", "babb" },
    new[] { new[] { "aabb", "abab", "baba", "bbaa" }, new[] { "abbb", "babb" } });

// ═══════ 反例区（09-26 补：专抓"ASCII 和 / 异或"这类【有损编码】当 key 的写法）═══════
//   判据：正确解法下这两组词【互不为异位词】→ 必须是两个组。
//   只要你的 key 让它们落进同一个组，这里的 ❌ 就会把它爆出来。
Check("⛔ 抓'ASCII 和当 key'：'ad' 与 'bc' 的和都是 197（长度也一样，看不出破绽）",
    new[] { "ad", "bc" },
    new[] { new[] { "ad" }, new[] { "bc" } });

Check("⛔ 抓'ASCII 和当 key'（换一对）：'ac' 与 'bb' 的和都是 196",
    new[] { "ac", "bb" },
    new[] { new[] { "ac" }, new[] { "bb" } });

Check("⛔ 抓'异或当 key'：'aa' 与 'bb' 都异或成 0（成对字母把自己抹掉了）",
    new[] { "aa", "bb" },
    new[] { new[] { "aa" }, new[] { "bb" } });

Check("⛔ 抓'计数直接拼接（无分隔符）'：1个a+11个b 与 11个a+1个b 都拼成 '111000…'",
    new[] { "abbbbbbbbbbb", "aaaaaaaaaaab" },
    new[] { new[] { "abbbbbbbbbbb" }, new[] { "aaaaaaaaaaab" } });

// ═══════ 类型区 ═══════
public class Solution {
    // TODO: 把互为【字母异位词】的单词分到同一组，返回所有组。
    //
    //   ── 先抓题眼："互为异位词"到底是什么关系？
    //      两个词互为异位词 ⇔ 【字母的种类和个数完全一样】（顺序无所谓）。
    //      注意它满足三条性质（这就是计基那 10 分钟讲的东西）：
    //        自反：自己和自己是异位词
    //        对称：A 与 B 是 → B 与 A 也是
    //        传递：A 与 B 是、B 与 C 是 → A 与 C 也是
    //      → 这叫**等价关系**；等价关系能把一堆东西**切成互不重叠的几堆**（等价类划分）。
    //      这道题就是"手动做一次等价类划分"，每一组 = 一个等价类。
    //
    //   ── ★ 核心问题只有一个：拿什么当 Dictionary 的 key？★
    //      同一个等价类里的单词必须**算出同一个 key**。所以 key 要满足：
    //        · 异位词之间 key 相同（否则被拆散）
    //        · 非异位词之间 key 不同（否则被错误地并成一组）
    //      符合这两条的最朴素候选 → **把单词的字母排序**：
    //        "eat" / "tea" / "ate" → 排序后全都是 "aet" ✓（异位词同一把钥匙）
    //        "tan" → "ant" ✗ 长得就像另一把钥匙 ✓（不同类不同钥匙）
    //      ——"排序后是同一个字符串"就是异位词的**规范形式（canonical form）**。
    //
    //   ── 结构上是"哈希分组"，正好复用你这两周的线：
    //        217 / 349 / 383 / 387：Dictionary 存【计数 / 存在性】
    //        205 / 290          ：Dictionary 存【映射关系】（键是"原字符"）
    //        350                ：Dictionary 存【计数】并"取货"
    //        ★ 49               ：Dictionary 存【一组东西】→ value 是 List<string>
    //                            （从"键 → 一个值"升级成"键 → 一个筐"，这是新的那一步）
    //
    //   ── 落笔前先自己想清楚这三处（都会踩）：
    //     ① 第一次见到一个新 key 时字典里还没有这个 key，怎么起一个空 List 并放进去？
    //        （TryGetValue 的返回值 + out 参数，你 383 / 219 用过；也可以看 Dictionary 的
    //          `GetValueOrDefault`）
    //        ★ 顺序是【造空筐 → 把筐放进字典 → 再往筐里装词】。
    //        ⚠️ 少写"把筐放进字典"这一步会很隐蔽：下一轮同一个 key 又查不到 → 又造一个新筐
    //           → 结果【每组只剩最后一个词】，而且一个异常都不抛（静默出错）。
    //           所以上面的判题工具是按"组"逐个比对，不只看"组数对不对"。
    //     ② 排序要动字符，怎么从 string → char[] → 排序 → 再回到 string？
    //        （`ToCharArray()` / `Array.Sort` / `new string(char[])` —— 三件套；
    //          ⚠️ 别写成 `chars.ToString()`，那返回的是类型名，不是内容）
    //     ③ ★ 返回类型是 `IList<IList<string>>`（题目要求，别改成 `List<List<string>>`）
    //        —— 接口 vs 具体类型，09-08 的 IReadOnlyList 那课（对外收窄、对内放开）。
    //        ⚠️ 一个【实测过】的坑（CS0266）：
    //           `List<List<string>>` 【不能】赋给 `IList<IList<string>>`
    //           —— 因为 `IList<T>` 是【不变】的（T 既进 `Add(T)`、又出 `this[int]`）。
    //        三条能编译通过的写法（已实证，任选其一）：
    //           · new List<IList<string>>(map.Values)
    //             （靠 List<T> 构造函数吃 IEnumerable<T> + `IEnumerable<out T>` 的协变）
    //           · 建一个 List<IList<string>>，逐组 Add(new List<string>{ ... })
    //             （`List<string>` → `IList<string>` 是【直接实现】，不是协变）
    //           · 如果契约只要求 IEnumerable：`IEnumerable<IList<string>> x = map.Values;`
    //        先按能过的写法来，AC 之后再问自己"为什么协变只发生在 out 位置上"。
    //
    //   ── 复杂度（写完自己算一遍）：
    //      设 n 个单词、平均长度 k：每个词排序 O(k log k) → 总共 O(n·k log k)；
    //      Dictionary 查找均摊 O(1)；总共 **O(n · k log k)** 时间，O(n · k) 空间（存所有字母）。
    //
    //   ── ★ 进阶（AC 之后想，面经高频）：
    //      ① 排序是不是必须的？—— 用 **int[26] 计数表**当"规范形式"（242 那条路），
    //         每个词只要 O(k) 扫一遍 + O(26) 拼 key → 复杂度降到 **O(n·k)**。
    //         但计数表怎么当 Dictionary 的 key？（int[] 是引用类型、默认按引用比相等 →
    //         ★ 想清楚：该怎么把它"变成"一个能比相等的东西？拼成字符串 / 用元组 / 自定义比较器）
    //      ② 如果单词里是 Unicode 字符（中文、emoji）呢？int[26] 还能用吗？
    //      ③ 如果不用哈希，能不能先给所有词排序再线性扫一遍？（复杂度变成什么？
    //         会不会把"组"的顺序也确定了？—— 这叫"先排序再分组"的套路）
    public IList<IList<string>> GroupAnagrams(string[] strs) {
        Dictionary<string,List<string>> dict = new Dictionary<string, List<string>>();
        List<IList<string>> result = new List<IList<string>>();
        foreach(string s in strs)
        {
            int[] judge = new int[26];
            foreach(char c in s)
            {
                judge[c - 'a']++;
            }
            string key = string.Join(",",judge);
            if (dict.TryGetValue(key, out List<string> group))
            {
                group.Add(s);
            }
            else
            {
                group = new List<string>();
                dict[key] = group;
                group.Add(s);
            }
        } 
        foreach(List<string> list in dict.Values)
        {
            result.Add(list);
        }
        return result;
    }
}
