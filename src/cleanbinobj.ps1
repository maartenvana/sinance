# This script cleans up all bin and obj folders recursively
# Usually you need to run this if (major) framework versions have changed or big changes to how projects are built

# Change to the correct folder
Push-Location $PSScriptRoot

# Perform action
Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

# Go back to previous directory
Pop-Location