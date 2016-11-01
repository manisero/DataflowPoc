Param(
  [string]$dataFile = "correct.json",
  [int]$count = 100000
)

$dataFolder = $PSScriptRoot
$sourceFile = Join-Path $dataFolder $dataFile
$targetFolder = Join-Path $dataFolder "generated"
$targetFile = Join-Path $targetFolder "mass.json"

If (-Not (Test-Path $targetFolder)) {
    New-Item $targetFolder -ItemType Directory
}

$sourceContent = Get-Content $sourceFile
$sourceData = New-Object System.Collections.Generic.List[string]
$sourceContent | ForEach-Object { $sourceData.Add($_) }

$targetContent = New-Object System.Collections.Generic.List[string]

for ($i=0; $i -lt $count; $i++) {
    $targetContent.AddRange($sourceData)
}

Set-Content $targetFile $targetContent
