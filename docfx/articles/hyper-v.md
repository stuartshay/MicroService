# Hyper-V

## Prerequisites

Azure VM with Hyper-V
 - Standard D2s v3 (2 vcpus, 8 GiB memory)

## Install Hyper-V

```
Write-Host "Installing and Enabling Hyper-V..."
Install-WindowsFeature -Name Hyper-V -IncludeManagementTools

Write-Host "Proceeding to restart computer..."
Restart-Computer
```

### Create Virtual Switch inside Hyper-V

```
New-VMSwitch -Name "InternalNATSwitch" -SwitchType Internal

Get-NetAdapter
```

### IP address for NAT gateway

```
New-NetIPAddress -IPAddress 192.168.0.1 -PrefixLength 24 -InterfaceIndex 13
```

### Scripts

```
$hyperv = Get-WindowsOptionalFeature -FeatureName Microsoft-Hyper-V -Online
if($hyperv.State -eq "Disabled"){
    Install-WindowsFeature -Name Hyper-V -IncludeManagementTools -Restart
    Restart-Computer
} 
else{
    Write-Host "Hyper-V is already Installed and Enabled. (Skipping Hyper-V installation and Restart)`n"
}
```

### References

```
https://mohitgoyal.co/2018/06/01/enable-and-use-nested-virtualization-on-azure-virtual-machine/
```

```
https://medium.com/@JockDaRock/minikube-on-windows-10-with-hyper-v-6ef0f4dc158c
```
