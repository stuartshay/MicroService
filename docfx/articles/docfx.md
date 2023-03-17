# DocFX

DocFX generates Documentation directly from source code (.NET, RESTful API, JavaScript, Java, etc...) and Markdown files.

```
https://dotnet.github.io/docfx/
```

# On Windows

rmdir F:\Build3\MicroService\docfx_site
docfx build docFX/docfx.json

# On Linux/Mac

rm -rf \_site
docfx build docFX/docfx.json

![](assets/docfx.png)

#### Prerequisites:

```powershell
choco install docfx
```

#### Build and Serve Website

```powershell
docfx docfx/docfx.json
docfx docfx/docfx.json --serve
```

```
http://localhost:8080
```
