
#!/bin/sh
echo "Checking for BOM in .cs files..."
git diff --cached --name-only --diff-filter=ACM -- "*.cs" "*.cpp" "*.h" | while read -r file; do
  if ! head -c3 "$file" | grep -q $'\xEF\xBB\xBF'; then
    echo "ERROR: Missing BOM in $file"
    exit 1
  fi
done

