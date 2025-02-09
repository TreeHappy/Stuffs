param (
  [Parameter(Mandatory=$true)]
  [string]$EnvFile
)

# Function to run the script with 1Password environment variables
function RunScriptWithEnvVars ($envFile, $scriptBlock)
{
  # Convert the script block to a string for passing to the command
  $scriptBlockString = $scriptBlock.ToString()

  # Run the command with the environment variables
  pwsh -c $scriptBlockString
}

# Run the script with the 1Password environment variables
RunScriptWithEnvVars -EnvFile $EnvFile -ScriptBlock {
  # Your script code goes here
  Write-Host "Running script..."
  # Your script code goes here
}

