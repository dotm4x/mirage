#!/bin/sh
set -e

git diff --cached --name-only --diff-filter=ACMR -- '*.cs' | while IFS= read -r file; do
  [ -n "$file" ] || continue
  dotnet csharpier format "$file"
  git add "$file"
done
