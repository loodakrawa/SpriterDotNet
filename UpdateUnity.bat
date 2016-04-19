set srcPath=SpriterDotNet
set libPath=SpriterDotNet.Unity\Assets\SpriterDotNet\Lib

del %libPath% /s /q /f
for /f %%f in ('dir /ad /b %libPath%') do rd /s /q %libPath%\%%f
robocopy %srcPath% %libPath% /e *.cs