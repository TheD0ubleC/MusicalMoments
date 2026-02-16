#!/usr/bin/env python3
"""MM 版本管理脚本。

命令：
- show：显示当前版本并执行一致性检查
- check：仅执行一致性检查
- update：一键更新 MM 主版本

说明：
- 该脚本只管理 MM 主程序版本，不会自动修改插件清单版本。
"""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

ROOT_DIR = Path(__file__).resolve().parents[1]
PROPS_PATH = ROOT_DIR / "Directory.Build.props"

VERSION_RE = re.compile(r"^\d+\.\d+\.\d+$")
DELTA_RE = re.compile(r"^\+(\d+)\.(\d+)\.(\d+)$")
PLACEHOLDER_DELTA_RE = re.compile(r"^([xX]|\d+)\.([yY]|\d+)\.([zZ]|\d+)$")
MM_TAG_LITERAL_RE = re.compile(r"\"(v\d+\.\d+\.\d+(?:\.\d+)?-[A-Za-z0-9]+-[A-Za-z0-9]+)\"")
MM_RELEASE_NAME_RE = re.compile(r"MM\.Release-(\d+\.\d+\.\d+)")
ABOUT_VERSION_RE = re.compile(r"版本号[:：]\s*([vV]?\d+\.\d+\.\d+(?:[-+][^\s\"']+)?)")

IGNORED_DIR_NAMES = {
    ".git",
    ".github",
    ".vs",
    "bin",
    "obj",
    "_build_tmp",
    "_artifacts",
    "node_modules",
}

ALLOWED_MM_TAG_LITERALS = {
    "v0.0.0-release-x64",
}


@dataclass
class MmVersion:
    version: str
    channel: str
    architecture: str
    tag: str


@dataclass
class VersionIssue:
    path: Path
    line: int
    message: str


def fail(message: str) -> None:
    print(f"[version] {message}", file=sys.stderr)
    raise SystemExit(1)


def parse_version_tuple(version: str) -> tuple[int, int, int]:
    if not VERSION_RE.match(version):
        fail(f"invalid version format: '{version}', expected semantic version like 1.5.0")

    major_str, minor_str, patch_str = version.split('.')
    return int(major_str), int(minor_str), int(patch_str)


def format_version(version_tuple: tuple[int, int, int]) -> str:
    return f"{version_tuple[0]}.{version_tuple[1]}.{version_tuple[2]}"


def resolve_update_expression(expression: str, current_version: str) -> str:
    expr = expression.strip()
    current_tuple = parse_version_tuple(current_version)

    if expr.startswith("="):
        absolute_expr = expr[1:].strip()
        if VERSION_RE.match(absolute_expr):
            # Explicit absolute version update, e.g. =1.5.1
            return absolute_expr
        fail(f"invalid absolute version expression: '{expr}', expected format '=1.5.1'")

    if VERSION_RE.match(expr):
        # Absolute version update, e.g. 1.5.1
        return expr

    delta_match = DELTA_RE.match(expr)
    if delta_match:
        # Explicit numeric increment, e.g. +1.5.6
        dx = int(delta_match.group(1))
        dy = int(delta_match.group(2))
        dz = int(delta_match.group(3))
        return format_version((current_tuple[0] + dx, current_tuple[1] + dy, current_tuple[2] + dz))

    placeholder_match = PLACEHOLDER_DELTA_RE.match(expr)
    if placeholder_match and any(ch in expr.lower() for ch in ("x", "y", "z")):
        # Placeholder delta, e.g. x.y.1 -> patch +1
        parts = [placeholder_match.group(1), placeholder_match.group(2), placeholder_match.group(3)]
        next_parts = [0, 0, 0]
        for index, token in enumerate(parts):
            lowered = token.lower()
            if lowered in ("x", "y", "z"):
                next_parts[index] = current_tuple[index]
            else:
                next_parts[index] = current_tuple[index] + int(token)

        return format_version((next_parts[0], next_parts[1], next_parts[2]))

    fail(
        "unsupported update expression. Use one of: \n"
        "  - absolute version: 1.5.1\n"
        "  - placeholder increment: x.y.1\n"
        "  - explicit increment: +1.5.6"
    )
    return current_version


def load_props_tree() -> ET.ElementTree:
    if not PROPS_PATH.exists():
        fail(f"missing file: {PROPS_PATH}")

    return ET.parse(PROPS_PATH)


def load_props_map() -> dict[str, ET.Element]:
    tree = load_props_tree()
    root = tree.getroot()

    prop_nodes: dict[str, ET.Element] = {}
    for property_group in root.findall("PropertyGroup"):
        for child in property_group:
            prop_nodes[child.tag] = child

    return prop_nodes


def read_current_version() -> MmVersion:
    props = load_props_map()

    def get(name: str, default: str = "") -> str:
        node = props.get(name)
        return (node.text or "").strip() if node is not None else default

    version = get("MMVersion")
    channel = get("MMReleaseChannel", "release")
    architecture = get("MMArchitecture", "x64")
    tag = get("MMVersionTag")

    if not version:
        fail("MMVersion is empty in Directory.Build.props")

    expected_tag = build_tag(version=version, channel=channel, architecture=architecture)
    if not tag:
        tag = expected_tag

    return MmVersion(version=version, channel=channel, architecture=architecture, tag=tag)


def build_tag(version: str, channel: str, architecture: str) -> str:
    return f"v{version}-{channel}-{architecture}"


def update_props(version: str, channel: str, architecture: str) -> None:
    tree = load_props_tree()
    root = tree.getroot()
    expected_tag = build_tag(version=version, channel=channel, architecture=architecture)

    target_values = {
        "MMVersion": version,
        "MMReleaseChannel": channel,
        "MMArchitecture": architecture,
        "MMVersionTag": expected_tag,
        "Version": version,
        "AssemblyVersion": version,
        "FileVersion": version,
        "InformationalVersion": expected_tag,
    }

    property_groups = root.findall("PropertyGroup")
    if not property_groups:
        property_groups = [ET.SubElement(root, "PropertyGroup")]

    primary_group = property_groups[0]

    existing_nodes: dict[str, ET.Element] = {}
    for group in property_groups:
        for child in group:
            existing_nodes[child.tag] = child

    for key, value in target_values.items():
        node = existing_nodes.get(key)
        if node is None:
            node = ET.SubElement(primary_group, key)
        node.text = value

    ET.indent(tree, space="  ")
    tree.write(PROPS_PATH, encoding="utf-8", xml_declaration=False)


def iter_source_files(pattern: str) -> Iterable[Path]:
    for path in ROOT_DIR.rglob(pattern):
        if any(part in IGNORED_DIR_NAMES for part in path.parts):
            continue
        yield path


def check_csproj_nodes(current: MmVersion) -> list[VersionIssue]:
    issues: list[VersionIssue] = []

    expected_tag = build_tag(current.version, current.channel, current.architecture)
    expected_map = {
        "Version": current.version,
        "AssemblyVersion": current.version,
        "FileVersion": current.version,
        "InformationalVersion": expected_tag,
    }

    for project_path in iter_source_files("*.csproj"):
        try:
            tree = ET.parse(project_path)
        except ET.ParseError as ex:
            issues.append(VersionIssue(project_path, 0, f"invalid xml: {ex}"))
            continue

        root = tree.getroot()
        for property_group in root.findall("PropertyGroup"):
            for node_name, expected_value in expected_map.items():
                node = property_group.find(node_name)
                if node is None or node.text is None:
                    continue

                actual = node.text.strip()
                if actual != expected_value:
                    issues.append(
                        VersionIssue(
                            path=project_path,
                            line=0,
                            message=f"<{node_name}> is '{actual}', expected '{expected_value}'",
                        )
                    )

    return issues


def check_directory_build_props_consistency(current: MmVersion) -> list[VersionIssue]:
    issues: list[VersionIssue] = []

    expected_tag = build_tag(current.version, current.channel, current.architecture)
    if current.tag != expected_tag:
        issues.append(
            VersionIssue(
                path=PROPS_PATH,
                line=0,
                message=f"MMVersionTag is '{current.tag}', expected '{expected_tag}'",
            )
        )

    props = load_props_map()
    for name, expected in (
        ("Version", current.version),
        ("AssemblyVersion", current.version),
        ("FileVersion", current.version),
        ("InformationalVersion", expected_tag),
    ):
        node = props.get(name)
        actual = (node.text or "").strip() if node is not None else ""
        if actual != expected:
            issues.append(
                VersionIssue(
                    path=PROPS_PATH,
                    line=0,
                    message=f"{name} is '{actual}', expected '{expected}'",
                )
            )

    return issues


def check_csharp_literals(current: MmVersion) -> list[VersionIssue]:
    issues: list[VersionIssue] = []

    expected_tag = build_tag(current.version, current.channel, current.architecture)

    for cs_path in iter_source_files("*.cs"):
        try:
            text = cs_path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            text = cs_path.read_text(encoding="utf-8", errors="ignore")

        for line_no, line in enumerate(text.splitlines(), start=1):
            for match in MM_TAG_LITERAL_RE.finditer(line):
                value = match.group(1)
                if value in ALLOWED_MM_TAG_LITERALS:
                    continue
                if value != expected_tag:
                    issues.append(
                        VersionIssue(
                            path=cs_path,
                            line=line_no,
                            message=f"hardcoded MM tag '{value}' != '{expected_tag}'",
                        )
                    )

            for match in MM_RELEASE_NAME_RE.finditer(line):
                version = match.group(1)
                if version != current.version:
                    issues.append(
                        VersionIssue(
                            path=cs_path,
                            line=line_no,
                            message=f"MM.Release version '{version}' != '{current.version}'",
                        )
                    )

            for match in ABOUT_VERSION_RE.finditer(line):
                value = match.group(1)
                normalized = value.strip().lstrip("vV")
                if normalized.startswith(current.version):
                    continue
                if "加载中" in line:
                    continue
                issues.append(
                    VersionIssue(
                        path=cs_path,
                        line=line_no,
                        message=f"about-page-like version literal '{value}' is not aligned to '{current.version}'",
                    )
                )

    return issues


def run_check() -> list[VersionIssue]:
    current = read_current_version()
    issues: list[VersionIssue] = []
    issues.extend(check_directory_build_props_consistency(current))
    issues.extend(check_csproj_nodes(current))
    issues.extend(check_csharp_literals(current))
    return issues


def print_issues(issues: list[VersionIssue]) -> None:
    if not issues:
        print("[version] check passed: no MM version inconsistencies found.")
        return

    print(f"[version] found {len(issues)} inconsistency(s):")
    for issue in issues:
        location = f"{issue.path.relative_to(ROOT_DIR)}"
        if issue.line > 0:
            location += f":{issue.line}"
        print(f"  - {location} -> {issue.message}")


def cmd_show(_args: argparse.Namespace) -> int:
    current = read_current_version()
    expected_tag = build_tag(current.version, current.channel, current.architecture)

    print("[version] current MM version")
    print(f"  version      : {current.version}")
    print(f"  channel      : {current.channel}")
    print(f"  architecture : {current.architecture}")
    print(f"  tag          : {current.tag}")
    print(f"  expected tag : {expected_tag}")

    issues = run_check()
    print_issues(issues)
    return 0 if not issues else 1


def cmd_check(_args: argparse.Namespace) -> int:
    issues = run_check()
    print_issues(issues)
    return 0 if not issues else 1


def cmd_update(args: argparse.Namespace) -> int:
    current = read_current_version()
    new_version = resolve_update_expression(args.expression, current.version)

    update_props(version=new_version, channel=current.channel, architecture=current.architecture)

    updated = read_current_version()
    print("[version] updated Directory.Build.props")
    print(f"  previous version : {current.version}")
    print(f"  expression       : {args.expression}")
    print(f"  new version      : {updated.version}")
    print(f"  tag              : {updated.tag}")

    issues = run_check()
    print_issues(issues)
    return 0 if not issues else 1


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="管理 MM 主程序版本，并检查版本引用一致性。",
        epilog=(
            "示例：\n"
            "  python scripts/mm_version.py show\n"
            "  python scripts/mm_version.py check\n"
            "  python scripts/mm_version.py update 1.5.1\n"
            "  python scripts/mm_version.py update x.y.1\n"
            "  python scripts/mm_version.py update +1.5.6"
        ),
        formatter_class=argparse.RawTextHelpFormatter,
    )
    subparsers = parser.add_subparsers(dest="command", required=True)

    show_parser = subparsers.add_parser("show", help="显示当前 MM 版本，并执行一致性检查。")
    show_parser.set_defaults(func=cmd_show)

    check_parser = subparsers.add_parser("check", help="检查 MM 版本在 props/csproj/C# 中是否一致。")
    check_parser.set_defaults(func=cmd_check)

    update_parser = subparsers.add_parser(
        "update",
        help="使用绝对版本或自增表达式更新 MM 版本。"
    )
    update_parser.add_argument(
        "expression",
        help=(
            "绝对版本: 1.5.1 或 =1.5.1 | "
            "占位自增: x.y.1 | 显式自增: +1.5.6"
        ),
    )
    update_parser.set_defaults(func=cmd_update)

    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()
    return args.func(args)


if __name__ == "__main__":
    raise SystemExit(main())
