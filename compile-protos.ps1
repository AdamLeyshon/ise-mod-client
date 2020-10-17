function CompileProto  {
    Param([object]$protofile)
    Write-Host "Compiling $protofile"
    protoc --csharp_out="./iseworld/ise-core/packets/" -I ./proto $protofile.Name
}

function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  $Script = Get-Item $Invocation.MyCommand.Path
  $Script.Directory.Parent.FullName
}

$cwd = Get-Location

Get-ScriptDirectory | Set-Location

Get-ChildItem -File -Path ./proto -Filter "*.proto" -Recurse | Set-Variable protoFiles
foreach ($protofile in $protoFiles) {
    CompileProto($protofile)
}
Write-Host "Done"

Set-Location $cwd