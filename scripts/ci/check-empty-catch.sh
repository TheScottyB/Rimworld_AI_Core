#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR="RimAI.Core/Source"

if rg -n --pcre2 "catch\s*(\([^)]*\))?\s*\{\s*\}" "$TARGET_DIR"; then
  echo "❌ Empty catch block detected. Replace with typed catch + ErrorPolicy logging/fallback."
  exit 1
fi

echo "✅ No empty catch blocks found in $TARGET_DIR"
