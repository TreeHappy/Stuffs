
#!/bin/sh
echo "Checking for BOM in .cs files..."
git diff --cached --name-only --diff-filter=ACM "*.cs" | while read -r file; do
  if ! head -c3 "$file" | grep -q $'\xEF\xBB\xBF'; then
    echo "ERROR: Missing BOM in $file"
    exit 1
  fi
done

#!/usr/bin/env pwsh
$files = git diff --cached --name-only --diff-filter=ACM "*.cs"
foreach ($file in $files) {
  $bytes = Get-Content -Path $file -AsByteStream -ReadCount 3 -TotalCount 3
  if (-not ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)) {
    Write-Error "Missing BOM in $file"
    exit 1
  }
}


