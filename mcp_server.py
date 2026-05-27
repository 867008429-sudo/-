from pathlib import Path

from fastmcp import FastMCP


mcp = FastMCP("HuanXian Local Tools")

PROJECT_ROOT = Path(__file__).resolve().parent


def _resolve_project_path(path: str) -> Path:
    target = (PROJECT_ROOT / path).resolve()
    if target != PROJECT_ROOT and PROJECT_ROOT not in target.parents:
        raise ValueError(f"Path is outside project root: {path}")
    return target


@mcp.tool
def read_text_file(path: str) -> str:
    """Read a UTF-8 text file from the HuanXian project directory."""
    target = _resolve_project_path(path)
    return target.read_text(encoding="utf-8")


@mcp.tool
def write_text_file(path: str, content: str) -> str:
    """Write a UTF-8 text file inside the HuanXian project directory."""
    target = _resolve_project_path(path)
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8")
    return f"Wrote {target.relative_to(PROJECT_ROOT)}"


@mcp.tool
def list_project_files(path: str = ".") -> list[str]:
    """List files and folders under a project-relative directory."""
    target = _resolve_project_path(path)
    if not target.exists():
        return []
    return [item.relative_to(PROJECT_ROOT).as_posix() for item in sorted(target.iterdir())]


if __name__ == "__main__":
    mcp.run()
