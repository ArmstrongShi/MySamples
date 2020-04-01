ECHO ON

xcopy /y \\sfnetfile.advent.com\Xfer\ashi\InstallApx\ConfigureSSL.ps1 %temp%

start powershell -ExecutionPolicy RemoteSigned -Command "& {%temp%\ConfigureSSL.ps1}"
