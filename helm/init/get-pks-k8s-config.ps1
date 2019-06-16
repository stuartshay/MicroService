# v 0.0.1
 
# get-pks-k8s-config.ps1
# gmerlin@vmware.com
#
# Requires Powershell 6.x & Open SSL for Windows 1.1 @ https://slproweb.com/products/Win32OpenSSL.html
# usage:  .\get-pks-k8s-config.ps1 -API pks.mg.lab -CLUSTER k8s1.mg.lab -USER naomi
 
param (
  # [string]$server = "http://defaultserver",
   [Parameter(Mandatory=$true)][string]$API,
   [Parameter(Mandatory=$true)][string]$CLUSTER,
   [Parameter(Mandatory=$true)][string]$USER
)
 
### Secured Password input
$masked = Read-host "Password" -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($masked))
 
 
#### Set Skip Trusts for Self Signed Certs && Force TLS 1.2
[System.Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}  
[System.Net.ServicePointManager]::CheckCertificateRevocationList = {$false}
 
#### Get Tokens
$URI = ("https://" + $API + ":8443/oauth/token")
$BODY = @{
   client_id = "pks_cluster_client"
   client_secret = ""
   grant_type = "password"
   username = $USER
   password =  [System.Web.HttpUtility]::UrlEncode($password)
   response_type = "id_token"
}
   try {$oidc_tokens=Invoke-RestMethod -Method Post -SkipCertificateCheck  -Uri $URI -Body $BODY}
   catch {
   write-error "Auth Failed"
   throw $_
   }
$access_token = $oidc_tokens.access_token
$refresh_token = $oidc_tokens.refresh_token
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
 
#### WIP on Getting CA Cert Need to Clean Up
 
& echo "Q" | openssl.exe s_client -connect ${CLUSTER}:8443 -showcerts | Select-String " 1 s:CN = CA" -Context 0,20 | Out-File -filepath ${CLUSTER}-ca.cert
$content = Get-Content ${CLUSTER}-ca.cert
$content | Foreach {$_.TrimStart()} | Set-Content ${CLUSTER}-ca.cert
& openssl x509 -in ${CLUSTER}-ca.cert -outform PEM | Out-File -filepath ${CLUSTER}-ca-clean.cert
 
 
#### Set kubeconfig
 
& kubectl config set-cluster ${CLUSTER} --server=https://${CLUSTER}:8443 --certificate-authority=./${CLUSTER}-ca-clean.cert --embed-certs=true
& kubectl config set-context ${CLUSTER} --cluster=${CLUSTER} --user=${USER}
& kubectl config use-context ${CLUSTER}
 
& kubectl config set-credentials ${USER} `
 --auth-provider oidc `
 --auth-provider-arg client-id=pks_cluster_client `
 --auth-provider-arg cluster_client_secret="" `
 --auth-provider-arg id-token=$access_token `
 --auth-provider-arg idp-issuer-url=https://${API}:8443/oauth/token `
 --auth-provider-arg refresh-token=$refresh_token
 
#### Cleanup
Remove-Item -path ${CLUSTER}-ca-clean.cert
Remove-Item -path ${CLUSTER}-ca.cert
 
