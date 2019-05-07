#!/usr/bin/python3
import sys

text = sys.stdin.read()
for line in text.splitlines():
    if len(line) == 34 and line.startswith('.') and line.endswith(':'):
        if text.count(line[1:-1]) == 1: continue

    print(line)
