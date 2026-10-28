# practice —— AI 线小练习

> 为验证某个 API / 写法而建的**一次性小工程**：不追求完整、不占项目名额，学完留在原地当参照。
> 完整交付物放 `../projects/`。

约定：每个练习一个子目录，目录名 = `日期-主题`（如 `2026-11-05-python-async`），里面至少一个能跑的 `.py` + 一行 `# 目的 / 判据` 注释。

## 已有练习

| 目录 | 主题 | 关键判据（实测） |
|---|---|---|
| `2026-10-28-python-env/` | uv 工程骨架 + 环境自检（S0 前置） | `uv run python check_env.py` 9 个包无 MISSING；`uv run ruff check .` 全绿；`sys.prefix` 指向本工程 `.venv` |
