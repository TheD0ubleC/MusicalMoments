#!/usr/bin/env python3
"""
Generate secure credentials for MusicalMoments.Web.

Defaults:
- .env -> MusicalMoments.Web/.env
- admin key -> MusicalMoments.Web/mm_admin.key
- web secret entropy -> 512 bits
- admin key entropy -> 256 bits
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import os
import stat
import sys
from pathlib import Path
import secrets


def b64url_no_padding(raw: bytes) -> str:
    return base64.urlsafe_b64encode(raw).decode("ascii").rstrip("=")


def secure_random_token(bits: int, prefix: str = "") -> str:
    if bits < 256:
        raise ValueError("bits must be >= 256")
    if bits % 8 != 0:
        raise ValueError("bits must be divisible by 8")
    raw = secrets.token_bytes(bits // 8)
    token = b64url_no_padding(raw)
    return f"{prefix}{token}" if prefix else token


def write_secure_text(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")
    try:
        os.chmod(path, stat.S_IRUSR | stat.S_IWUSR)
    except OSError:
        # chmod behavior differs on Windows; ignore.
        pass


def repo_root() -> Path:
    # scripts/generate_mm_web_secret.py -> repo root is parent of scripts/
    return Path(__file__).resolve().parents[1]


def default_env_path() -> Path:
    return repo_root() / "MusicalMoments.Web" / ".env"


def default_key_path() -> Path:
    return repo_root() / "MusicalMoments.Web" / "mm_admin.key"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate MM web secret + admin key (256-bit+)")
    parser.add_argument("--env-file", default=str(default_env_path()), help="Output .env file path")
    parser.add_argument("--key-file", default=str(default_key_path()), help="Output admin key file path")
    parser.add_argument("--addr", default=":8090", help="MM_WEB_ADDR value")
    parser.add_argument("--secret-bits", type=int, default=512, help="MM_WEB_SECRET entropy bits (>=256)")
    parser.add_argument("--key-bits", type=int, default=256, help="Admin key entropy bits (>=256)")
    parser.add_argument("--show-key", action="store_true", help="Print admin key to stdout (use with caution)")
    return parser.parse_args()


def build_env_content(addr: str, web_secret: str, admin_hash: str) -> str:
    return (
        "# MusicalMoments.Web generated security config\n"
        f"MM_WEB_ADDR={addr}\n"
        "MM_WEB_DATA_FILE=./data/site-data.json\n"
        "MM_WEB_UPLOAD_DIR=./data/uploads\n"
        "MM_WEB_SESSION_TTL_HOURS=12\n"
        "MM_WEB_AUTOSAVE_SECONDS=15\n"
        "MM_WEB_MAX_UPLOAD_MB=1024\n"
        "MM_WEB_READ_TIMEOUT_SECONDS=15\n"
        "MM_WEB_WRITE_TIMEOUT_SECONDS=30\n"
        "MM_WEB_IDLE_TIMEOUT_SECONDS=60\n"
        f"MM_WEB_SECRET={web_secret}\n"
        f"MM_ADMIN_KEY_HASH={admin_hash}\n"
    )


def main() -> int:
    args = parse_args()

    if args.secret_bits < 256 or args.key_bits < 256:
        print("Error: --secret-bits and --key-bits must be >= 256", file=sys.stderr)
        return 2
    if args.secret_bits % 8 != 0 or args.key_bits % 8 != 0:
        print("Error: --secret-bits and --key-bits must be divisible by 8", file=sys.stderr)
        return 2

    web_secret = secure_random_token(args.secret_bits, prefix="mms1_")
    admin_key = secure_random_token(args.key_bits, prefix="mmk1_")
    admin_hash = hashlib.sha256(admin_key.encode("utf-8")).hexdigest()

    env_path = Path(args.env_file).resolve()
    key_path = Path(args.key_file).resolve()

    write_secure_text(env_path, build_env_content(args.addr, web_secret, admin_hash))
    write_secure_text(key_path, admin_key + "\n")

    fingerprint = hashlib.sha256(admin_key.encode("utf-8")).hexdigest()[:16]
    print("Security files generated successfully:")
    print(f"  .env file : {env_path}")
    print(f"  key file  : {key_path}")
    print(f"  key bits  : {args.key_bits}")
    print(f"  secret bits: {args.secret_bits}")
    print(f"  key fingerprint: {fingerprint}")
    print("\nNext steps:")
    print("  1) cd MusicalMoments.Web")
    print("  2) go run ./cmd/server")
    print("  3) open /admin and upload mm_admin.key")

    if args.show_key:
        print("\n[ADMIN KEY]")
        print(admin_key)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

