# List-Instructions.ps1
Write-Host "AetherRISC Unified Instruction Inventory" -ForegroundColor Cyan
Write-Host "------------------------------------------"

# Define metadata for each extension/family
$FamilyMetadata = @{
    "Base"     = @{ Title = "[RV64I] Base Integer Set"; Desc = "Core integer arithmetic, loads, stores, and control flow." }
    "M"        = @{ Title = "[M] Multiplier Extension"; Desc = "Hardware integer multiplication and division." }
    "A"        = @{ Title = "[A] Atomic Extension";     Desc = "Atomic Memory Operations (AMO) and LR/SC for synchronization." }
    "F"        = @{ Title = "[F] Single-Precision FP";  Desc = "32-bit floating-point arithmetic and memory operations." }
    "D"        = @{ Title = "[D] Double-Precision FP";  Desc = "64-bit floating-point support (required for RV64G)." }
    "Zicsr"    = @{ Title = "[Zicsr] Control/Status";   Desc = "Access to system registers (CSRs) and performance counters." }
    "Zifencei" = @{ Title = "[Zifencei] Instruction Fence"; Desc = "Ensures instruction cache consistency for self-modifying code." }
}

# Get all instruction classes in the Core assembly
$Instructions = [System.Reflection.Assembly]::LoadFrom("$(Get-Location)/AetherRISC.Core/bin/Debug/net9.0/AetherRISC.Core.dll").GetTypes() | 
    Where-Object { $_.IsSubclassOf([AetherRISC.Core.Architecture.ISA.Base.Instruction]) -and -not $_.IsAbstract }

# Group by the extension folder/namespace
$Groups = $Instructions | Group-Object { 
    if ($_.Namespace -match "Extensions\.(\w+)") { $Matches[1] } else { "Base" }
} | Sort-Object Name

foreach ($Group in $Groups) {
    $Meta = $FamilyMetadata[$Group.Name]
    $Title = if ($Meta) { $Meta.Title } else { "[$($Group.Name)] Extension" }
    $Desc = if ($Meta) { " - $($Meta.Desc)" } else { "" }

    Write-Host "`n$Title$Desc" -ForegroundColor Yellow
    
    $Group.Group | Sort-Object Mnemonic | ForEach-Object {
        $Name = $_.Name.Replace("Instruction", "").ToUpper()
        
        # Get constructor to show parameters
        $Ctor = $_.GetConstructors()[0]
        $Params = ($Ctor.GetParameters() | ForEach-Object { "$($_.Name)" }) -join ", "
        
        Write-Host "  $($Name.PadRight(10)) ($Params)"
    }
}

Write-Host "`nInventory Complete." -ForegroundColor Green