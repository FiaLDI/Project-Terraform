#!/usr/bin/env bash

limit=50000000

large_files=$(git ls-files -s | awk '{print $4}' | while read f; do
  if [ -f "$f" ]; then
    size=$(stat -c%s "$f")
    if [ "$size" -gt "$limit" ]; then
      echo "$f ($size bytes)"
    fi
  fi
done)

if [ ! -z "$large_files" ]; then
  echo "❌ Large files detected (>50MB):"
  echo "$large_files"
  exit 1
fi
