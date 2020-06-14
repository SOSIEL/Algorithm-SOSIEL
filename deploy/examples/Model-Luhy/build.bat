SET Configuration=Release


dotnet clean --configuration %Configuration%

dotnet restore

dotnet publish --configuration %Configuration% /p:DebugType=None