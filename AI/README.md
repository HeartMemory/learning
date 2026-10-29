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
