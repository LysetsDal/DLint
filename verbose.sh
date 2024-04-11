#!/bin/bash

# Script to visualize dockerfiles with newline '\n' 
# and space '\s' characters verbose
func() {
    if [ -n "$1" ]; then
        perl -pe 's/\n/\\n/g; s/\t/\\t/g; s/ /\\s/g' "$1"
    else
        perl -pe 's/\n/\\n/g; s/\t/\\t/g; s/ /\\s/g' ./Dockerfile 
    fi
}

func "$1" | awk '{gsub(/\\n/, "\n")} 1'