SET Solution=SOSIEL_EX1.sln
SET Configuration=Release
SET Runtime=win-x86


dotnet clean --configuration %Configuration% --runtime %Runtime%

dotnet build --configuration %Configuration% --runtime %Runtime% /p:DebugType=None