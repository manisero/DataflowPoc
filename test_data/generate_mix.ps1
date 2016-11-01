$dataFolder = $PSScriptRoot
$targetFolder = Join-Path $dataFolder "generated"
$targetFile = Join-Path $targetFolder "mix.json"

If (-Not (Test-Path $targetFolder)) {
    New-Item $targetFolder -ItemType Directory
}

$data = New-Object System.Collections.Generic.List[string]
$dataFiles = Get-ChildItem $dataFolder -File -Filter *.json

$dataFiles | ForEach-Object {
    $content = Get-Content $_.FullName
    $data.Add($content)
}

Set-Content $targetFile $data
