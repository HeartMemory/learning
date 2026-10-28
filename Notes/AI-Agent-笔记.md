# AI Agent / 大模型应用笔记（2026-10-28 起）

> AI 主线知识点沉淀，按主题组织，随进度更新（与 `CSharp-基础笔记.md`、`Unity-基础笔记.md` 同规格）。
> 代码实例都在 `AI/` 对应目录里，可运行验证。
> 原则：**只记真实学到的内容与真实踩过的坑**；每个知识点尽量配一条**可实测的判据**。

**覆盖进度**：（待填，按阶段记）

## 回访清单

> 用法同其他两册：周日复盘盲写重做时，先过这张表。

| # | 主题 | 一句话判据 | 首次 | 回访 | 状态 |
|---|---|---|---|---|---|
| | | | | | |

---

## 0. 待学章节（骨架，学到再展开）

> 只列主题，不提前写内容——避免"抄来的笔记"占位。

| 阶段 | 章节 | 主题 |
|---|---|---|
| S0 | 1 | Python 与 C# 的语法差异（类型注解 / 解包 / 作用域） |
| S0 | 2 | 可变性陷阱：列表/dict 传参被改 |
| S0 | 3 | 装饰器与 C# 委托/Attribute 的对照 |
| S0 | 4 | `async/await` 与 C# `Task` 的异同 |
| S1 | 5 | messages 结构 / role / system vs user |
| S1 | 6 | 流式（SSE）与首 token 延迟 |
| S1 | 7 | 采样参数：temperature / top_p / max_tokens |
| S1 | 8 | token 计量与成本估算 |
| S1 | 9 | 结构化输出：JSON Schema + 校验 + 重试 |
| S1 | 10 | Context Engineering：上下文怎么组织 |
| S2 | 11 | 工具调用协议与工具描述（ACI） |
| S2 | 12 | ReAct 循环与终止条件 |
| S2 | 13 | 上下文裁剪 / 压缩 / 记忆 |
| S3 | 14 | 切分策略与 embedding |
| S3 | 15 | 向量检索与混合检索 / rerank |
| S3 | 16 | 评测：eval 集 / LLM-as-judge / 幻觉归因 |
| S4 | 17 | LangGraph：State / Node / Edge / checkpoint |
| S4 | 18 | MCP：Tools / Resources / Prompts 与 server 编写 |
| S4 | 19 | 可观测：trace / 成本与延迟报表 |
| S4 | 20 | 部署：FastAPI + Docker |
| 原理 | 21 | nanoGPT：token → embedding → attention → 训练循环 |
