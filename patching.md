# Patching

https://github.com/sisong/HDiffPatch
https://github.com/CollapseLauncher/SharpHDiffPatch.Core


# Incremental Patch Script for Application Bin Directory
# Requires HDiffPatch: https://github.com/sisong/HDiffPatch

param(
    [ValidateSet("create", "apply")]
    [string]$Mode,
    [string]$OldPath,
    [string]$NewPathOrPatch,
    [string]$OutputPath
)

# Path to HDiffPatch tools (adjust as needed)
$HDiffPath = ".\tools\hdiffz.exe"
$HPatchPath = ".\tools\hpatchz.exe"

function Create-Patch {
    param($oldDir, $newDir, $patchFile)
    if (-not (Test-Path $HDiffPath)) { throw "HDiffPatch tools not found at $HDiffPath" }
    
    # Create patch with compression (zstd) and file timestamp checking
    & $HDiffPath -M -c-zstd -f-ctime "$oldDir" "$newDir" "$patchFile"
    
    if (-not (Test-Path $patchFile)) {
        throw "Patch creation failed!"
    }
    Write-Host "Patch created: $patchFile ($((Get-Item $patchFile).Length/1MB) MB)"
}

function Apply-Patch {
    param($oldDir, $patchFile, $outputDir)
    if (-not (Test-Path $HPatchPath)) { throw "HPatch tool not found at $HPatchPath" }
    
    # Apply patch with verification
    & $HPatchPath -f "$oldDir" "$patchFile" "$outputDir"
    
    if (-not (Test-Path $outputDir)) {
        throw "Patch application failed!"
    }
    Write-Host "Successfully patched to: $outputDir"
}

# Main execution
switch ($Mode.ToLower()) {
    "create" {
        if (-not (Test-Path $OldPath)) { throw "Old directory not found: $OldPath" }
        if (-not (Test-Path $NewPathOrPatch)) { throw "New directory not found: $NewPathOrPatch" }
        Create-Patch $OldPath $NewPathOrPatch $OutputPath
    }
    "apply" {
        if (-not (Test-Path $OldPath)) { throw "Base directory not found: $OldPath" }
        if (-not (Test-Path $NewPathOrPatch)) { throw "Patch file not found: $NewPathOrPatch" }
        Apply-Patch $OldPath $NewPathOrPatch $OutputPath
    }
}

