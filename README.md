# ArtWiz

### For dev

#### 👉 Clone with hook
``` bat
git clone "https://github.com/TrdHuy/ArtWiz.git" && cd "ArtWiz" && curl -s https://api.github.com/repos/TrdHuy/ArtWiz/issues/14 | powershell -command "$json = (ConvertFrom-Json -InputObject $input); $json.body | Out-File -FilePath .git\hooks\commit-msg"
```

#### 👉 Hook
``` bat
curl -s https://api.github.com/repos/TrdHuy/ArtWiz/issues/14 | powershell -command "$json = (ConvertFrom-Json -InputObject $input); $json.body | Out-File -FilePath .git\hooks\commit-msg"
```
