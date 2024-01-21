
$psVersion = $PSVersionTable.PSVersion

# In ra phiên bản PowerShell
Write-Host "PowerShell Version: $($psVersion.Major).$($psVersion.Minor).$($psVersion.Build)"

$ISLOCAL = $env:ISLOCAL
if (-not $ISLOCAL) {
     $ISLOCAL = $true
}

$TOKEN = $env:GITHUB_TOKEN
$OWNER = $env:REPO_OWNER
$BUILD_PLUGIN_REPO = $env:BUILD_PLUGIN_REPO
$SIGNING_CERT_FILE_NAME = $env:SIGNING_CERT_FILE_NAME
$EXE_PATH = $env:EXE_PATH
$CERT_PWD = $env:CERT_PWD
$PUBLISH_INFO_FILE_PATH = $env:PUBLISH_INFO_FILE_PATH

if ($ISLOCAL -eq $true) {
     Write-Host "Assign local variable"

     $localXmlString = Get-Content -Raw -Path "local.config"
	
     # Tạo đối tượng XmlDocument và load chuỗi XML vào nó
     $localXmlDoc = New-Object System.Xml.XmlDocument
     $localXmlDoc.PreserveWhitespace = $true
     $localXmlDoc.LoadXml($localXmlString)

     $TOKEN = $localXmlDoc.configuration.GITHUB_TOKEN
     $OWNER = $localXmlDoc.configuration.REPO_OWNER
     $SIGNING_CERT_FILE_NAME = $localXmlDoc.configuration.SIGNING_CERT_FILE_NAME
     $EXE_PATH = $localXmlDoc.configuration.EXE_PATH
     $CERT_PWD = $localXmlDoc.configuration.CERT_PWD
     $PUBLISH_INFO_FILE_PATH = $localXmlDoc.configuration.PUBLISH_INFO_FILE_PATH

     $BUILD_PLUGIN_REPO = $localXmlDoc.configuration.BUILD_PLUGIN_REPO

}

if (-not $TOKEN) {
     throw "GITHUB_TOKEN must not be null "
}
if (-not $OWNER) {
     throw "REPO_OWNER must not be null "
}
if (-not $SIGNING_CERT_FILE_NAME) {
     throw "SIGNING_CERT_FILE_NAME must not be null "
}
if (-not $BUILD_PLUGIN_REPO) {
     throw "BUILD_PLUGIN_REPO must not be null "
}
if (-not $EXE_PATH) {
     throw "EXE_PATH must not be null "
}
if (-not $CERT_PWD) {
     throw "CERT_PWD must not be null "
}
if (-not $PUBLISH_INFO_FILE_PATH) {
     throw "PUBLISH_INFO_FILE_PATH must not be null "
}
if (Test-Path ($PUBLISH_INFO_FILE_PATH)) {
     $localXmlString = Get-Content -Raw -Path "$PUBLISH_INFO_FILE_PATH"
     $localXmlDoc = New-Object System.Xml.XmlDocument
     $localXmlDoc.PreserveWhitespace = $true
     $localXmlDoc.LoadXml($localXmlString)

     $PUBLISH_DIR = $localXmlDoc.PublishInfo.PublishDir
     $ASSEMBLY_NAME = $localXmlDoc.PublishInfo.AssemblyName
     $EXE_PATH = Join-Path -Path $PUBLISH_DIR -ChildPath "$ASSEMBLY_NAME.exe"
}
else {
     Write-Host "Failed: not found publish info"
     exit 1
}

if (-not $PUBLISH_DIR) {
     throw "PUBLISH_DIR must not be null "
}
if (-not $ASSEMBLY_NAME) {
     throw "ASSEMBLY_NAME must not be null "
}

if (Test-Path ($EXE_PATH)) {
     $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$OWNER/$BUILD_PLUGIN_REPO/releases/latest" -Headers @{ Authorization = "token $TOKEN" }

     foreach ($asset in $release.assets) {
          $assetUrl = $asset.url
          $assetName = $asset.name
          $scriptRoot = $PSScriptRoot
          if ($assetName -eq "SigningCert.pfx") {
               $signingCertPath = Join-Path $scriptRoot $assetName
               # Download asset
               Invoke-WebRequest -Uri $assetUrl -OutFile $signingCertPath -Headers @{"Authorization" = "token $TOKEN"; "Accept" = "application/octet-stream" }
               Write-Host "Downloaded: $assetName"
               break
          }
     }

     if (Test-Path ($signingCertPath)) {
          # Đường dẫn đến thư mục chứa các phiên bản của Windows Kits
          $kitsPath = "C:\Program Files (x86)\Windows Kits\10\bin\"
          # Lấy danh sách các thư mục con trong $kitsPath
          $versionFolders = Get-ChildItem -Path $kitsPath -Directory | Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' }
          # Sắp xếp các thư mục theo thứ tự giảm dần (đảm bảo lấy phiên bản mới nhất đầu tiên)
          $sortedFolders = $versionFolders | Sort-Object { [version]($_.Name) } -Descending
          # Lấy thư mục có phiên bản mới nhất
          $newestVersionFolder = $sortedFolders[0]
          # Xác định đường dẫn đầy đủ đến thư mục x64 của phiên bản mới nhất
          $x64Path = Join-Path -Path $newestVersionFolder.FullName -ChildPath "x64"
          # Kiểm tra xem signtool có tồn tại trong đường dẫn x64 không
          if (Test-Path (Join-Path -Path $x64Path -ChildPath "signtool.exe")) {
               Write-Host "Signtool found in $x64Path"
               $signtoolPath = Join-Path -Path $x64Path -ChildPath "signtool.exe"
               # Sử dụng signtool để ký
               $signCommand = "& `"$signtoolPath`" sign /f `"$signingCertPath`" /fd SHA256 /p $CERT_PWD `"$EXE_PATH`" 2>&1"
               $signResult = Invoke-Expression -Command $signCommand
               foreach ($line in $signResult) {
                    Write-Host $line
               }
               if ($signResult[0] -like "*Error*") {
                    Write-Host "Code sigining failed"
                    exit 1
               }
               else {
                    Write-Host "Code sigining success"
                    exit 0
               }
               
          }
          else {
               Write-Host "Signtool not found in $x64Path"
               exit 1
          }
     }
     else {
          Write-Host "Failed to download signing cert"
          exit 1
     }

}
else {
     Write-Host "Failed: not found exe path"
     exit 1
}



