#!/usr/bin/env bash

conflicts=$(grep -R "<<<<<<< HEAD" -n Assets/ 2>/dev/null)

if [ ! -z "$conflicts" ]; then
  echo "❌ Merge conflict markers found:"
  echo "$conflicts"
  exit 1
fi
