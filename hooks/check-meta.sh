#!/usr/bin/env bash

missing=$(git ls-files --others --exclude-standard | grep -v '\.meta$' | while read f; do
  if [ ! -f "$f.meta" ]; then
    echo "$f"
  fi
done)

if [ ! -z "$missing" ]; then
  echo "❌ Missing .meta files:"
  echo "$missing"
  exit 1
fi
