# ArtWiz

### For dev on Window run with cmd

#### 👉 Clone with hook
``` cmd
git clone "https://github.com/TrdHuy/ArtWiz.git" && chcp 65001 && cd "ArtWiz" && curl -s https://api.github.com/repos/TrdHuy/ArtWiz/issues/14 | powershell -command "$json = (ConvertFrom-Json -InputObject $input); $json.body | Out-File -FilePath .git\hooks\commit-msg"
```

#### 👉 Hook
``` cmd
chcp 65001 && curl -s https://api.github.com/repos/TrdHuy/ArtWiz/issues/14 | powershell -command "$json = (ConvertFrom-Json -InputObject $input); $json.body | Out-File -FilePath .git\hooks\commit-msg"
```
