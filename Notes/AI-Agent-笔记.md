# AI Agent / 大模型应用笔记（2026-10-28 起）

> AI 主线知识点沉淀，按主题组织，随进度更新（与 `CSharp-基础笔记.md`、`Unity-基础笔记.md` 同规格）。
> 代码实例都在 `AI/` 对应目录里，可运行验证。
> 原则：**只记真实学到的内容与真实踩过的坑**；每个知识点尽量配一条**可实测的判据**。

**覆盖进度**：**S0 环境基线 ✅**（2026-10-28 · 第 1 章）｜ ⚠️ **2026-10-29 起 Python 语言层单独成册** → 语法 / 容器 / 可变性 / 装饰器 / async / httpx / pydantic 一律记进 `Python-基础笔记.md`，**本册只管 LLM 与 Agent 层**

## 回访清单

> 用法同其他两册：周日复盘盲写重做时，先过这张表。

| # | 主题 | 一句话判据 | 首次 | 回访 | 状态 |
|---|---|---|---|---|---|
| 1 | **uv 与 venv 的分工** | `sys.prefix` ≠ `sys.base_prefix` ⇒ 在 venv 里；相等 ⇒ 在全局 | 10-28 | — | 🔴 待盲写 |
| 2 | **解析 ≠ 安装** | `uv.lock` 有内容 而 `site-packages` 空 ⇒ 卡在下载 wheel，别去改依赖 | 10-28 | — | 🔴 待盲写 |
| 3 | **扩展的引擎墙** | 扩展 `engines.vscode` 高于客户端内核版本 ⇒ 装不上（报"不兼容"） | 10-28 | — | 🟡 已实测 |

---

## 1. 环境基线：uv / venv / 解释器隔离（2026-10-28 · S0 前置）

### 1.1 uv 是什么 —— 不是 Python 扩展

- **定位**：Astral（`ruff` 同厂）用 **Rust** 写的**独立可执行程序**，管「解释器 / 项目环境 / 运行」三件事，替代 `pip` + `venv` + `pyenv` + `pipx` + `poetry`
- **判据（实测）**：`(Get-Command uv).Source` → `D:\python\Scripts\uv.exe`，**单文件 40 MB、同级无配套 dll**（静态链接）；在 `C:\` 根目录、无任何工程、未激活环境下 `uv --version` 正常输出版本 ⇒ **它不依赖 Python 就能跑**
- ⚠️ **误区**：`python -c "import uv"` 能成功，是因为用 `pip install uv` 时顺带装了 PyPI 上**同名的包装包**（空壳，`site-packages\uv\__init__.py`），**不是 uv 本体**
- **为什么必须用 Rust 写**：一个"帮你装 Python"的工具**不能依赖 Python 才能跑**（鸡生蛋）；`pip` 正是因此永远装不了 Python 本身

### 1.2 venv 是什么 —— 一个文件夹 + 一张身份证

```
.venv\
├── Scripts\python.exe      ← 启动器（不是复制整个解释器）
├── Lib\site-packages\      ← 本项目独占的包目录
└── pyvenv.cfg              ← 身份证
```

- **机制**：Python 启动时先看自己 exe 旁边有没有 `pyvenv.cfg`；有 → 把 `sys.prefix` 改成该目录，`site-packages` 跟着指过去
- ⭐ **最硬的判据（实测两组输出）**：

| 位置 | `sys.prefix` | `sys.base_prefix` |
|---|---|---|
| 工程内（`uv run`） | `...\2026-10-28-python-env\.venv` | `D:\python` |
| 全局（`cd C:\` 后） | `D:\python` | `D:\python` |

⇒ **两个 prefix 不相等就是在 venv 里**，相等就是在全局。

- `pyvenv.cfg` 两项关键：`home = 真实解释器路径`、`include-system-site-packages = false`（**隔离开关**）
- **为什么 `.venv` 不进 git**：里面写死本机绝对路径（`home = D:\python`）+ 可随时由 `uv sync` 重建

### 1.3 解析 ≠ 安装（今天卡了十几分钟的地方）

- **现象**：`uv sync` 停在 "Downloading…" 不动
- **判据**：`uv.lock` 已生成（346 行 / 50 KB）而 `.venv\Lib\site-packages` 里 **0 个包** ⇒ **卡的是 wheel 下载，不是依赖解析**
- **处置**：换国内镜像（实测握手延迟：**阿里云 582 ms < 清华 976 ms < PyPI 1043 ms**，中科大 HEAD 403）
- **官方写法**（本机 uv 0.12.23 的 `--help` 证据）：用户级 `%APPDATA%\uv\uv.toml` 里写 `[[index]]` + `url` + `default = true`
  - ⚠️ `--index-url` / `UV_INDEX_URL` **已标注 Deprecated** ⇒ 用 `--default-index` / `UV_DEFAULT_INDEX`
- **效果**：同一份 lock，**5.4 秒**装完 19 个包

### 1.4 ruff ≠ 类型检查（最容易混的一对）

| 工具 | 管什么 | 不管什么 |
|---|---|---|
| `ruff` | 风格 / lint（`E F I UP B SIM RUF`）/ 格式化 | **类型检查**（写错 `list[int]` 它不管） |
| `Pylance` / `pyright` | **类型检查**（注解、参数不匹配、返回值） | 风格 / 格式化 |

- **中文项目必配**：`[tool.ruff.lint] ignore = ["RUF001","RUF002","RUF003"]` —— 否则 docstring 里的全角冒号 / 括号被判成"易混淆字符"（实测一次报 9 处）

### 1.5 扩展的「引擎墙」

- 每个扩展的 `package.json` 里有 `engines.vscode` 字段，**客户端内核低于它 ⇒ 装不上**
- **实测**：CodeBuddy（内核 **1.106.1**）装不了要求 `^1.107.0` 的 Pylance / Error Lens / Python Environments；VS Code **1.136.2** 可以
- **判据命令**：`(Get-Content <扩展目录>\package.json -Raw | ConvertFrom-Json).engines.vscode`
- 另一个事实：**不同编辑器产品的扩展目录彼此独立**（`~\.codebuddycn\extensions` vs `~\.vscode\extensions`），用 A 的 CLI 装不到 B

### 1.6 记忆的三层（别把它们当成一个东西）

| 层 | 落点 | 谁写 | 手改有效？ |
|---|---|---|---|
| **画像** | `~\.codebuddy\memery\<UID>_memery.md` | 产品（「本地记忆」开关触发） | ✅ **能落盘**（10-20 受控实验 T0/T1/T2：+100 B / mtime 更新 / **重启会话后标记仍在**）—— ⚠️ 「产品会整份重写」这个旧结论**已被推翻** |
| **记忆条目** | 编辑器面板「保存的记忆」 | AI（每次会话写入） | ✅ |
| **事实源** | 仓库根 `README.md` 打卡表 + `Notes/YYYY/MM/DD.md` | 人与 AI | ✅ **唯一权威** |

- ⚠️ **教训（10-20 修正）**：手改画像**能落盘**，真正的问题是**不可验收**（仓库外 + 无版本控制）⇒ 改前备份 `.bak-YYYYMMDD` → **只写不变量** → **改完立刻读回**（体积 + mtime + 头部版本号）→ 同步 `user-memery-state.json` 的 `current_version`。**不验收 = 没做**
- 记忆**只存不变量**（偏好 / 风格 / 红线 / 入口指针）；**进度、题数、成品数、阶段日期一律读文件**——数字进记忆必然腐烂

---

## 附录 · 待学章节（计划编号 · 学到时展开成正文）

> 只列主题，不提前写内容——避免"抄来的笔记"占位。编号沿用计划表，与正文章号不强制对应。

| 阶段 | 章节 | 主题 |
|---|---|---|
| S0 | 1-9 | **Python 语言层整体迁出** → 见 `Python-基础笔记.md`（语法总览 / list / 可变性陷阱 / dict·set / 装饰器 / 模块 / async / httpx / pydantic）—— 本册不再记 Python 语言本身 |
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
