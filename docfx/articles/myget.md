# Myget Package Deployment

Windows

```powershell
  $env:mygetApiKey = "adab4634-8ddb-4789-ae92-6461295ac69c"
  .\build.ps1 --target=push-myget
```

Linux
 
```bash
 export mygetApiKey="adab4634-8ddb-4789-ae92-6461295ac69c"
./build.sh --target=push-myget
```
