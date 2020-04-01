ECHO ON

xcopy /y \\sfnetfile.advent.com\Xfer\ashi\InstallApx\scripts\*.* %temp%
xcopy /y \\sfnetfile.advent.com\Xfer\ashi\InstallApx\xml\*.* %temp%
xcopy /y \\sfnetfile.advent.com\Xfer\ashi\InstallApx\keys\*.* %temp%

start powershell -ExecutionPolicy RemoteSigned -Command "& {%temp%\InstallApx.ps1}"
