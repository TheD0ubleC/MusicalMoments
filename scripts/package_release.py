#!/usr/bin/env python3
"""One-click packager for MusicalMoments releases.

Outputs two zip packages:
1) Framework-dependent: MM.Release-{version}.zip
2) Self-contained:      MM.Release-{version}-SelfContained.zip

Both variants use single-file publish for:
- MusicalMoments (main app)
- MusicalMoments.Updater
- All bundled plugins
"""

from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from zipfile import ZIP_DEFLATED, ZipFile

ROOT_DIR = Path(__file__).resolve().parents[1]
PROPS_PATH = ROOT_DIR / "Directory.Build.props"
TEMP_ROOT = ROOT_DIR / "_build_tmp" / "package"
OUTPUT_ROOT_DEFAULT = ROOT_DIR / "_artifacts"

MAIN_PROJECT = ROOT_DIR / "MusicalMoments" / "MusicalMoments.csproj"
UPDATER_PROJECT = ROOT_DIR / "MusicalMoments.Updater" / "MusicalMoments.Updater.csproj"
PLUGIN_PROJECTS = {
    "MusicalMoments.KeyHelp": ROOT_DIR / "MusicalMoments.KeyHelp" / "MusicalMoments.KeyHelp.csproj",
    "MusicalMoments.PluginExample": ROOT_DIR / "MusicalMoments.PluginExample" / "MusicalMoments.PluginExample.csproj",
    "MusicalMoments.WithinReach": ROOT_DIR / "MusicalMoments.WithinReach" / "MusicalMoments.WithinReach.csproj",
}


def log(message: str) -> None:
    print(f"[pack] {message}")


def run(command: list[str]) -> None:
    resolved = command[:]
    resolved[0] = resolve_executable(command[0])
    log(" ".join(resolved))
    subprocess.run(resolved, cwd=ROOT_DIR, check=True)


def resolve_executable(executable: str) -> str:
    direct = shutil.which(executable)
    if direct:
        return direct

    if sys.platform.startswith("win"):
        for suffix in (".cmd", ".exe", ".bat"):
            candidate = shutil.which(f"{executable}{suffix}")
            if candidate:
                return candidate

    return executable


def parse_properties(path: Path) -> dict[str, str]:
    tree = ET.parse(path)
    root = tree.getroot()
    props: dict[str, str] = {}
    for property_group in root.findall("PropertyGroup"):
        for child in property_group:
            props[child.tag] = (child.text or "").strip()
    return props


def clean_dir(path: Path) -> None:
    if path.exists():
        shutil.rmtree(path)
    path.mkdir(parents=True, exist_ok=True)


def copy_publish_output(source: Path, destination: Path) -> None:
    destination.mkdir(parents=True, exist_ok=True)
    for item in source.iterdir():
        target = destination / item.name
        if item.is_dir():
            if target.exists():
                shutil.rmtree(target)
            shutil.copytree(item, target)
        else:
            shutil.copy2(item, target)


def copy_main_publish_output(source: Path, destination: Path) -> None:
    destination.mkdir(parents=True, exist_ok=True)
    excluded_dirs = {"Plugin"}
    excluded_file_prefixes = (
        "MusicalMoments.Updater",
    )
    for item in source.iterdir():
        if item.is_dir() and item.name in excluded_dirs:
            continue

        if item.is_file() and any(item.name.startswith(prefix) for prefix in excluded_file_prefixes):
            continue

        target = destination / item.name
        if item.is_dir():
            if target.exists():
                shutil.rmtree(target)
            shutil.copytree(item, target)
        else:
            shutil.copy2(item, target)


def zip_directory(source_dir: Path, output_file: Path) -> None:
    output_file.parent.mkdir(parents=True, exist_ok=True)
    with ZipFile(output_file, "w", compression=ZIP_DEFLATED) as archive:
        for file_path in sorted(source_dir.rglob("*")):
            if file_path.is_file():
                arc_name = file_path.relative_to(source_dir)
                archive.write(file_path, arc_name)


def ensure_single_file_publish_shape(output_dir: Path, expected_exe_name: str) -> None:
    expected_exe = output_dir / expected_exe_name
    if not expected_exe.exists():
        raise FileNotFoundError(f"Missing published executable: {expected_exe}")

    disallowed_dll = output_dir / f"{Path(expected_exe_name).stem}.dll"
    if disallowed_dll.exists():
        raise RuntimeError(
            f"Publish output is not single-file: found {disallowed_dll.name} next to {expected_exe_name}"
        )


def validate_zip_layout(zip_path: Path) -> None:
    with ZipFile(zip_path, "r") as archive:
        entries = [name for name in archive.namelist() if name]
        bad_entries = [
            name for name in entries
            if name.startswith("_")
            or "/_work/" in name
            or "/_main_publish/" in name
            or "/_plugin_publish_" in name
            or "/_updater_publish/" in name
        ]
        if bad_entries:
            raise RuntimeError(
                "zip contains intermediate build directories:\n"
                + "\n".join(bad_entries[:20])
            )


def build_local_tailwind() -> None:
    run([
        "npx",
        "tailwindcss@3.4.17",
        "-c",
        "MusicalMoments.WithinReach/tailwind.config.js",
        "-i",
        "MusicalMoments.WithinReach/tailwind.input.css",
        "-o",
        "MusicalMoments.WithinReach/www/tailwind.local.css",
        "--minify",
    ])


def publish_project(
    project_path: Path,
    output_dir: Path,
    configuration: str,
    runtime_identifier: str,
    self_contained: bool,
    publish_single_file: bool,
) -> None:
    clean_dir(output_dir)

    run([
        "dotnet",
        "publish",
        str(project_path),
        "-c",
        configuration,
        "-r",
        runtime_identifier,
        f"-p:PublishSelfContained={'true' if self_contained else 'false'}",
        f"-p:SelfContained={'true' if self_contained else 'false'}",
        f"-p:PublishSingleFile={'true' if publish_single_file else 'false'}",
        "-p:PublishTrimmed=false",
        "-p:DebugType=None",
        "-p:DebugSymbols=false",
        "-o",
        str(output_dir),
    ])


def build_variant(
    variant_name: str,
    self_contained: bool,
    configuration: str,
    runtime_identifier: str,
    output_zip: Path,
) -> None:
    log(f"Building variant: {variant_name}")

    variant_base_dir = TEMP_ROOT / variant_name
    variant_work_dir = variant_base_dir / "_work"
    variant_package_dir = variant_base_dir / "package"
    main_publish_dir = variant_work_dir / "main_publish"
    updater_publish_dir = variant_work_dir / "updater_publish"

    clean_dir(variant_base_dir)
    clean_dir(variant_work_dir)
    clean_dir(variant_package_dir)

    publish_project(
        project_path=UPDATER_PROJECT,
        output_dir=updater_publish_dir,
        configuration=configuration,
        runtime_identifier=runtime_identifier,
        self_contained=self_contained,
        publish_single_file=True,
    )
    ensure_single_file_publish_shape(updater_publish_dir, "MusicalMoments.Updater.exe")

    publish_project(
        project_path=MAIN_PROJECT,
        output_dir=main_publish_dir,
        configuration=configuration,
        runtime_identifier=runtime_identifier,
        self_contained=self_contained,
        publish_single_file=True,
    )
    ensure_single_file_publish_shape(main_publish_dir, "MusicalMoments.exe")

    copy_main_publish_output(main_publish_dir, variant_package_dir)
    copy_publish_output(updater_publish_dir, variant_package_dir)

    plugin_root = variant_package_dir / "Plugin"
    if plugin_root.exists():
        shutil.rmtree(plugin_root)
    plugin_root.mkdir(parents=True, exist_ok=True)

    for plugin_name, plugin_project in PLUGIN_PROJECTS.items():
        plugin_publish_dir = variant_work_dir / f"plugin_publish_{plugin_name}"
        publish_project(
            project_path=plugin_project,
            output_dir=plugin_publish_dir,
            configuration=configuration,
            runtime_identifier=runtime_identifier,
            self_contained=self_contained,
            publish_single_file=True,
        )
        ensure_single_file_publish_shape(plugin_publish_dir, f"{plugin_name}.exe")

        plugin_target_dir = plugin_root / plugin_name
        clean_dir(plugin_target_dir)
        copy_publish_output(plugin_publish_dir, plugin_target_dir)

    zip_directory(variant_package_dir, output_zip)
    validate_zip_layout(output_zip)
    log(f"Created package: {output_zip}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Build MusicalMoments release zip packages.")
    parser.add_argument("--configuration", default="Release", help="Build configuration (default: Release)")
    parser.add_argument("--runtime", default="win-x64", help="Runtime identifier (default: win-x64)")
    parser.add_argument(
        "--output-dir",
        default=str(OUTPUT_ROOT_DEFAULT),
        help=f"Output directory for zip files (default: {OUTPUT_ROOT_DEFAULT})",
    )
    parser.add_argument("--skip-tailwind", action="store_true", help="Skip rebuilding local Tailwind CSS")
    args = parser.parse_args()

    if not PROPS_PATH.exists():
        raise FileNotFoundError(f"Missing version file: {PROPS_PATH}")

    props = parse_properties(PROPS_PATH)
    version = props.get("MMVersion")
    if not version:
        raise ValueError("MMVersion not found in Directory.Build.props")

    output_dir = Path(args.output_dir).resolve()
    output_dir.mkdir(parents=True, exist_ok=True)

    if not args.skip_tailwind:
        build_local_tailwind()

    framework_dependent_zip = output_dir / f"MM.Release-{version}.zip"
    self_contained_zip = output_dir / f"MM.Release-{version}-SelfContained.zip"

    build_variant(
        variant_name="framework-dependent",
        self_contained=False,
        configuration=args.configuration,
        runtime_identifier=args.runtime,
        output_zip=framework_dependent_zip,
    )
    build_variant(
        variant_name="self-contained",
        self_contained=True,
        configuration=args.configuration,
        runtime_identifier=args.runtime,
        output_zip=self_contained_zip,
    )

    log("Done.")
    log(f"Framework-dependent: {framework_dependent_zip}")
    log(f"Self-contained:      {self_contained_zip}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except subprocess.CalledProcessError as ex:
        log(f"Command failed with exit code {ex.returncode}")
        raise SystemExit(ex.returncode)
    except Exception as ex:  # pragma: no cover
        log(f"Failed: {ex}")
        raise SystemExit(1)
