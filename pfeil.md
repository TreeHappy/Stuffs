function Export-FileInfoToCsv {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$FolderPath,

        [Parameter(Mandatory=$false)]
        [string]$OutputPath = "FileInformationExport.csv"
    )

    # Validate folder existence
    if (-not (Test-Path -LiteralPath $FolderPath -PathType Container)) {
        Write-Error "The directory path '$FolderPath' does not exist or is not a valid directory."
        return
    }

    # Collect file information recursively
    Get-ChildItem -LiteralPath $FolderPath -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
        [PSCustomObject]@{
            FileName        = $_.Name
            FileSizeBytes   = $_.Length
            FullPath        = $_.FullName
            Directory       = $_.DirectoryName
            DateModified    = $_.LastWriteTime
            DateCreated     = $_.CreationTime
            DateAccessed    = $_.LastAccessTime
            FileExtension   = $_.Extension
            Attributes      = $_.Attributes.ToString()
            LastModifiedBy = try {(Get-Acl $_.FullName).Access[-1].IdentityReference} catch { $null }
        }
    } | Export-Csv -Path $OutputPath -NoTypeInformation -Encoding UTF8 -Force

    Write-Host "File information exported to: $OutputPath" -ForegroundColor Green
}

