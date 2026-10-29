# AI 主线（智能体 / 大模型应用）

> **2026-10-28 起，本目录是学习主线**；Unity / C# 线已冻结为作品集（见根 `README.md`）。
> 本目录只放**代码 + 索引 + 实测数字**；路线与判据以本文件的阶段表为准（规划文档在仓库外维护，不进 git）。

## 阶段与交付物

| 阶段 | 内容 | 交付物 | 状态 |
|---|---|---|---|
| **S0** | Python 迁移（uv / 类型注解 / async / httpx / pydantic） | `practice/` 下小工具（并发抓取 + 存 JSON） | 🔄 **环境就绪（10-28）** · 练习未开始 |
| **S1** | LLM API：流式 / 结构化输出 / token 与成本 / prompt | `projects/llm-cli/` | ⬜ |
| **S2** | **手写 Agent（禁用框架）**：工具调用 / ReAct / trace / 重试 | `projects/mini-agent/` | ⬜ |
| **S3** | RAG + 评测：切分 / embedding / 检索 / eval 集 / **失败归因三分法** | `projects/rag-notes/` | ⬜ |
| **S3.5** | **Agent 安全与红队**：提示注入 / 间接注入 / 工具越权 / 沙箱 / guardrails | `projects/redteam-report/` | ⬜ |
| **S4** | LangGraph / MCP server / 观测 / FastAPI + Docker | `projects/mcp-server/` + LangGraph 重写对比 | ⬜ |
| **S5** | 旗舰项目（双简历通用：游戏场景 Agent） | `projects/game-agent/` | ⬜ |
| 并行 | 底层原理：手敲一遍 nanoGPT（每周 1-2h） | `practice/nanogpt-notes/` | ⬜ |

## 目录

```
AI/
├── README.md        # 本文件：阶段 / 项目 / 纪律
├── practice/        # 小练习（学会某个 API 就丢这里，不追求完整）
└── projects/        # 交付物（要能跑、有 README、有实测数字）
```

## 怎么跑

| 需要什么 | 怎么跑 |
|---|---|
| **Python 3.14+**（实测 **3.14.8**）+ **uv**（实测 **0.12.23**） | 进到具体项目目录：`uv sync` → `uv run python -m <模块>`（每个项目自带 `pyproject.toml` + `.python-version` + `uv.lock`） |
| 模型 key | 只写各项目根目录的 **`.env`**（**不入库**）；仓库里只保留 **`.env.example`** 模板 |
| 代码检查 | `uv run ruff check .`（配置在项目 `pyproject.toml`；中文注释已 ignore `RUF001`/`002`/`003`） |
| 依赖下载慢（国内） | 配**用户级** `%APPDATA%\uv\uv.toml` → `[[index]]` + `url = "https://mirrors.aliyun.com/pypi/simple"` + `default = true`（**在仓库外，不进 git**）；⚠️ `--index-url` 已废弃，改用 `--default-index` |
| 环境自检样板 | [`practice/2026-10-28-python-env/`](practice/2026-10-28-python-env/README.md)：`uv sync` → `uv run python check_env.py` → `uv run ruff check .` |
| 编辑器扩展 | `ms-python.python`（含 Pylance / debugpy）· **`charliermarsh.ruff`**（实时告警 + 保存时格式化）· `tamasfe.even-better-toml` · `usernamehw.errorlens` · `ms-toolsai.jupyter`；⚠️ **打开工程后要手动选一次 `.venv` 解释器**；⚠️ **别装 flake8 / pylint / black / isort**（与 ruff 打架） |

## 纪律（硬规则）

1. **不跳阶段**：S2 手写 Agent 没过，不许碰 LangGraph。
2. **S2 之前代码里出现 `langchain` / `langgraph` 判不合格**——先会手写，才允许用框架。
3. **每个交付物必须有 README**：判据是什么、怎么跑、**实测数字**（token / 耗时 / 通过率 / 成本）。
4. **key 永不入库**：`.env` 已被 `.gitignore` 排除；提交前自查一次。
5. **提交信息**：`MM-DD: [AI] 描述`（与 `[算法]/[笔记]/[Unity]` 同格式）。
6. **算法题用 Python**（2026-10-28 起）：新题一律 `.py`，运行 `python <文件>`；**旧 C# 题解在 `LeetCode/` 里原样保留，不重写不迁移**。

## key 暴露检测（怎么做 · 2026-10-29）

> **前提认知**：**key 一旦进过提交，就视为已泄漏** —— 唯一正确处置是**去平台吊销重发**。改写历史救不回已经出去的东西（远端对象、fork、缓存、爬虫都可能有副本）。所以检测的目标不是"事后补救"，而是**在它进库之前拦住**。

### 三道防线（成本递增，按顺序都做）

| # | 时机 | 做什么 | 命令 | 期望 |
|---|---|---|---|---|
| 1 | **写代码时** | `.env` 被忽略、仓库只留 `.env.example` | `git check-ignore -v .env`<br>`git ls-files \| Select-String '\.env'` | 第一条打印 `.gitignore` 规则；第二条**只列出** `.env.example` |
| 2 | **提交前** | 扫**暂存内容**（只看新增行更准）；本机钩子兜底 | `git diff --cached \| Select-String 'sk-[A-Za-z0-9_-]{16,}'`<br>`git diff --cached \| Select-String '(?i)api[_-]?key\s*=\s*\S'` | 两条都**无输出** |
| 3 | **事后审计** | 扫**已追踪文件** + **全历史** | `git grep -I -n -E 'sk-[A-Za-z0-9_-]{16,}'`<br>`git log --all --oneline -G'sk-[A-Za-z0-9_-]{16,}'`<br>`git log --all --oneline -G'eyJhbGciOi'` | 全部**无输出** |

### 怎么判读结果

| 现象 | 是泄漏吗 | 处置 |
|---|---|---|
| `.env.example` 里有 `LLM_API_KEY=`（**等号后为空**） | ❌ 不是 | 模板只写变量名，正常 |
| 命中行是 `.env.example` 且等号后有值 | ⚠️ **是** | 清空值 + **吊销重发** |
| 命中行是锁文件里的 `sha256:` / URL 十六进制串 | ❌ 误报 | 十六进制里恰好出现形似串，看那行内容判断 |
| 命中 `eyJhbGciOi`（JWT 头） | ⚠️ **是**，且是最严重的一类 | 立刻吊销该令牌并重发 |
| `git ls-files` 里出现**名为 `.env` 的文件** | ⚠️ **是** | `git rm --cached <路径>` + 吊销重发（撤出索引不等于没泄漏） |

⭐ **判据：`git rm --cached` 只是"不再跟踪"，历史里那一版仍然可以被 `git log -p` 翻出来** —— 所以第一动作永远是**吊销**，不是删文件。

### 本仓库实测（2026-10-29 · 325 个提交）

| 扫描 | 结果 |
|---|---|
| 已追踪文件里的 `sk-` 形态 key | **0** |
| 全历史 diff 里的 `sk-` 形态 key | **0** |
| 全历史 diff 里的 JWT 形态（`eyJhbGciOi`） | **0** |
| 历史上被 add 过的 `.env` | **只有 2 个 `.env.example`**（空模板） |
| 结论 | **key 从未进过任何一次提交** ✅ |

⚠️ **本机 pre-commit 钩子只是第 2 道防线的补充**：它在 `.git/hooks/` 里、**不进仓库**，换机器 / 重新 clone **必须重建**；所以第 1、2、3 道命令要能手动跑，别只依赖钩子。
