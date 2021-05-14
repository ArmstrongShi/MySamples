
$_xmltemplate='{0}\ApxServerInstall_*.xml' -f ${env:temp}
$_backupDir='{0}\backup\apxfirm' -f ${env:systemdrive}
$_sys = get-wmiobject win32_computersystem
$_hostname = '{0}.{1}' -f $_sys.Name,$_sys.Domain
$_webBaseUrl = 'https://{0}'-f $_hostname
$_idsBaseUrl = 'https://{0}/oauth' -f $_hostname
$_apxQAImg ='\\source.advent.com\Img\qaimg\APX\APXSetup\*_*_*_*'
$_apxImg ='\\source.advent.com\Img\img\APX\APXSetup\*_*_*_*'
$_uiQAImg='\\source.advent.com\Img\qaimg\APX\APXNextgen\*_*_*_*'

[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") 
[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") 

function ShowParameterInputDialog()
{
	$objForm = New-Object System.Windows.Forms.Form 
	$objForm.Text = "Install APX Server"
	
	$objForm.StartPosition = "CenterScreen"
	$objForm.AutoSize = $true;

    $idsPos = 20
    $idsSize = 50
    $idsGroup = New-Object System.Windows.Forms.GroupBox
    $idsGroup.Location = New-Object System.Drawing.Point(10,$idsPos) 
	$idsGroup.Size = New-Object System.Drawing.Point(530,$idsSize) 
    $idsGroup.Text = "Identity Server"
    $objForm.Controls.Add($idsGroup)

	$idsLabel = New-Object System.Windows.Forms.Label
	$idsLabel.Location = New-Object System.Drawing.Point(10,20) 
	$idsLabel.Size = New-Object System.Drawing.Size(60,20) 
	$idsLabel.Text = "Ids Proxy"
	$idsGroup.Controls.Add($idsLabel) 
	
    $idsTextBox = New-Object System.Windows.Forms.TextBox
	$idsTextBox.Location = New-Object System.Drawing.Point(70,20) 
	$idsTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$idsTextBox.Text = $_idsBaseUrl
	$idsGroup.Controls.Add($idsTextBox)


    $apxPos = $idsPos + $idsSize + 10
    $apxSize = 290
    $apxGroup = New-Object System.Windows.Forms.GroupBox
    $apxGroup.Location = New-Object System.Drawing.Point(10,$apxPos) 
	$apxGroup.Size = New-Object System.Drawing.Size(530,$apxSize) 
    $apxGroup.Text = "APX Server and Client"
    $objForm.Controls.Add($apxGroup)

    $verPos = 20

    $verLabel= New-Object System.Windows.Forms.Label
	$verLabel.Location = New-Object System.Drawing.Point(10,$verPos) 
	$verLabel.Size = New-Object System.Drawing.Size(60,20) 
	$verLabel.Text = 'Version'
	$apxGroup.Controls.Add($verLabel)

    $verTextBox = New-Object System.Windows.Forms.TextBox
	$verTextBox.Location = New-Object System.Drawing.Point(70,$verPos) 
	$verTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$verTextBox.Text = GetInstalledVersion
    $verTextBox.Enabled = $false
	$apxGroup.Controls.Add($verTextBox)


    $verButton = New-Object System.Windows.Forms.Button
	$verButton.Location = New-Object System.Drawing.Point(440,$verPos)
	$verButton.Size = New-Object System.Drawing.Size(70,20)
	$verButton.Text = "Refresh"
	$verButton.Add_Click({$verTextBox.Text = GetInstalledVersion })
	$apxGroup.Controls.Add($verButton)

    $xmlPos = $verPos + 30
		
	$xmlLabel= New-Object System.Windows.Forms.Label
	$xmlLabel.Location = New-Object System.Drawing.Point(10,$xmlPos) 
	$xmlLabel.Size = New-Object System.Drawing.Size(60,20) 
	$xmlLabel.Text = 'XML'
	$apxGroup.Controls.Add($xmlLabel)
	
    $xmlComboBox = New-Object System.Windows.Forms.ComboBox
    $xmlComboBox.location = New-Object System.Drawing.Point(70,$xmlPos)
    $xmlComboBox.Size = New-Object System.Drawing.Size(350,20)
    Get-ChildItem -Path $_xmltemplate -file | ForEach-Object -Process {$xmlComboBox.Items.Add($_.FullName)}
    $xmlComboBox.SelectedIndex = 0
    $apxGroup.Controls.Add($xmlComboBox)

    $buildPos = $xmlPos + 30

	$buildLabel = New-Object System.Windows.Forms.Label
	$buildLabel.Location = New-Object System.Drawing.Point(10,$buildPos) 
	$buildLabel.Size = New-Object System.Drawing.Size(60,20) 
	$buildLabel.Text = "Build"
	$apxGroup.Controls.Add($buildLabel) 
	
    $buildComboBox = New-Object System.Windows.Forms.ComboBox
    $buildComboBox.location = New-Object System.Drawing.Point(70,$buildPos)
    $buildComboBox.Size = New-Object System.Drawing.Size(350,20)
    Get-ChildItem -Path $_apxQAImg -Directory | Sort-Object LastWriteTime -Descending | Where-Object {-not (test-path (Join-Path -Path $_.FullName -ChildPath 'By_Manual.txt'))} | Select-Object -First 5 | ForEach-Object -Process { $buildComboBox.Items.Add($_.FullName)}
	$buildComboBox.Items.Add('=======================================================')
    Get-ChildItem -Path $_apxImg -Directory | Sort-Object LastWriteTime  -Descending | Where-Object {-not $_.FullName.Contains('DoNotUse')} | Select-Object -First 5 | ForEach-Object -Process { $buildComboBox.Items.Add($_.FullName) }
    $buildComboBox.SelectedIndex = 0
    $apxGroup.Controls.Add($buildComboBox)

    $bakPos = $buildPos + 30
	
	$bakLabel = New-Object System.Windows.Forms.Label
	$bakLabel.Location = New-Object System.Drawing.Point(10,$bakPos) 
	$bakLabel.Size = New-Object System.Drawing.Size(60,20) 
	$bakLabel.Text = "Backup"
	$apxGroup.Controls.Add($bakLabel) 
	
	$bakTextBox = New-Object System.Windows.Forms.TextBox
	$bakTextBox.Location = New-Object System.Drawing.Point(70,$bakPos) 
	$bakTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$bakTextBox.Text = $_backupDir
	$apxGroup.Controls.Add($bakTextBox)

	$bakButton = New-Object System.Windows.Forms.Button
	$bakButton.Location = New-Object System.Drawing.Size(440,$bakPos)
	$bakButton.Size = New-Object System.Drawing.Size(70,20)
	$bakButton.Text = "Restore"
	$bakButton.Add_Click({RestoreApxDatabasesClick $bakTextBox.Text})
	$apxGroup.Controls.Add($bakButton)
	

    $dbpos = $bakPos + 30
	$dbLabel = New-Object System.Windows.Forms.Label
	$dbLabel.Location = New-Object System.Drawing.Point(10,$dbpos) 
	$dbLabel.Size = New-Object System.Drawing.Point(60,20) 
	$dbLabel.Text = "Database"
	$apxGroup.Controls.Add($dbLabel) 

    $dbTextBox = New-Object System.Windows.Forms.TextBox
	$dbTextBox.Location = New-Object System.Drawing.Point(70,$dbpos) 
	$dbTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$dbTextBox.Text = $_hostname
	$apxGroup.Controls.Add($dbTextBox)

	$dbDropButton = New-Object System.Windows.Forms.Button
	$dbDropButton.Location = New-Object System.Drawing.Size(440,$dbpos)
	$dbDropButton.Size = New-Object System.Drawing.Size(70,20)
	$dbDropButton.Text = "Drop"
	$dbDropButton.Add_Click({DropApxDatabases})
	$apxGroup.Controls.Add($dbDropButton)
	
    $appPos = $dbpos + 30

	$appLabel = New-Object System.Windows.Forms.Label
	$appLabel.Location = New-Object System.Drawing.Point(10,$appPos) 
	$appLabel.Size = New-Object System.Drawing.Size(60,20) 
	$appLabel.Text = "Application"
	$apxGroup.Controls.Add($appLabel) 

    $appTextBox = New-Object System.Windows.Forms.TextBox
	$appTextBox.Location = New-Object System.Drawing.Point(70,$appPos) 
	$appTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$appTextBox.Text = $_hostname
	$apxGroup.Controls.Add($appTextBox)

    $webpos = $apppos + 30

	$webLabel = New-Object System.Windows.Forms.Label
	$webLabel.Location = New-Object System.Drawing.Point(10,$webpos) 
	$webLabel.Size = New-Object System.Drawing.Size(60,20) 
	$webLabel.Text = "Web"
	$apxGroup.Controls.Add($webLabel) 
	
    $webTextBox = New-Object System.Windows.Forms.TextBox
	$webTextBox.Location = New-Object System.Drawing.Point(70,$webpos) 
	$webTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$webTextBox.Text = $_webBaseUrl
	$apxGroup.Controls.Add($webTextBox)
	
	$sitePos = $webpos + 30
	$siteLabel = New-Object System.Windows.Forms.Label
	$siteLabel.Location = New-Object System.Drawing.Point(10,$sitePos) 
	$siteLabel.Size = New-Object System.Drawing.Size(60,20) 
	$siteLabel.Text = "Site"
	$apxGroup.Controls.Add($siteLabel) 
	
	$siteComboBox = New-Object System.Windows.Forms.ComboBox
    $siteComboBox.location = New-Object System.Drawing.Point(70,$sitePos)
    $siteComboBox.Size = New-Object System.Drawing.Size(350,20)
    Get-WebSite | ForEach-Object -Process { $siteComboBox.Items.Add($_.Name)}
    $siteComboBox.SelectedIndex = 0
    $apxGroup.Controls.Add($siteComboBox)	
	
    $btnPos = $sitePos + 30

	$installButton = New-Object System.Windows.Forms.Button
	$installButton.Location = New-Object System.Drawing.Point(330,$btnPos)
	$installButton.Size = New-Object System.Drawing.Size(80,20)
	$installButton.Text = "Install"
	$installButton.Add_Click({InstallApxServerClick $buildComboBox.Text $xmlComboBox.Text $dbTextBox.Text $appTextBox.Text $webTextBox.Text $siteComboBox.Text $idsTextBox.Text $bakTextBox.Text})
	$apxGroup.Controls.Add($installButton)

	$removeButton = New-Object System.Windows.Forms.Button
	$removeButton.Location = New-Object System.Drawing.Point(430,$btnPos)
	$removeButton.Size = New-Object System.Drawing.Size(80,20)
	$removeButton.Text = "Uninstall"
	$removeButton.Add_Click({UninstallApxServerClick $buildComboBox.Text})
	$apxGroup.Controls.Add($removeButton) 

    $ngPos = $apxPos +$apxSize+ 10
    $ngSize = 80
    $ngGroup = New-Object System.Windows.Forms.GroupBox
    $ngGroup.Location = New-Object System.Drawing.Point(10,$ngPos) 
	$ngGroup.Size = New-Object System.Drawing.Point(530,$ngSize) 
    $ngGroup.Text = "APX Nextgen"
    $objForm.Controls.Add($ngGroup)

    $ngbuildPos = 20

	$ngLabel = New-Object System.Windows.Forms.Label
	$ngLabel.Location = New-Object System.Drawing.Point(10,$ngbuildPos) 
	$ngLabel.Size = New-Object System.Drawing.Size(60,20) 
	$ngLabel.Text = "Build"
	$ngGroup.Controls.Add($ngLabel) 
		
	$ngComboBox = New-Object System.Windows.Forms.ComboBox
    $ngComboBox.location = New-Object System.Drawing.Point(70,$ngbuildPos)
    $ngComboBox.Size = New-Object System.Drawing.Size(350,20)	
	Get-ChildItem -Path $_uiQAImg -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 10 | ForEach-Object -Process { $ngComboBox.Items.Add($_.FullName) }
	$ngComboBox.SelectedIndex = 0
	$ngGroup.Controls.Add($ngComboBox)

    $ngbtnPos = $ngbuildPos + 30

	$nginstallButton = New-Object System.Windows.Forms.Button
	$nginstallButton.Location = New-Object System.Drawing.Point(330,$ngbtnPos)
	$nginstallButton.Size = New-Object System.Drawing.Size(80,20)
	$nginstallButton.Text = "Install"
	$nginstallButton.Add_Click({InstallApxNextgen $ngComboBox.Text $webTextBox.Text })
	$ngGroup.Controls.Add($nginstallButton)

	$ngremoveButton = New-Object System.Windows.Forms.Button
	$ngremoveButton.Location = New-Object System.Drawing.Point(430,$ngbtnPos)
	$ngremoveButton.Size = New-Object System.Drawing.Size(80,20)
	$ngremoveButton.Text = "Uninstall"
	$ngremoveButton.Add_Click({UninstallApxNextgen})
	$ngGroup.Controls.Add($ngremoveButton) 

	$ngSize = $ngbtnPos + 30
    $height = $ngPos +$ngsize+ 50
	$objForm.Size = New-Object System.Drawing.Size(570,$height)
	$objForm.Add_Shown({$objForm.Activate()})
	[void] $objForm.ShowDialog()
}


function InstallApxServerClick($build,$xmlTemp,$db,$app,$web,$site,$ids,$bakpath)
{    
	write-host 'Start: Install Apx server.' -ForegroundColor Green
    $setup= Join-Path -Path $build -ChildPath 'ApxServer\Setup.exe'
	if((test-path $setup))
	{
        $ver = GetInstalledVersion
        if ($ver -ne $null)
        {
            ShowVersionWarning($ver)
        }
        else
        {			
			$xmlPath = PrepareApxServerInstallXml $xmlTemp $db $app $web $site $ids
			
            $success = InstallApxServer $setup $xmlPath
		    if($success)
		    {	
			    InstallApxClient $app $web
		    }
        }
	}
	else
	{
		ShowNotFoundError($setup)
	}
	write-host 'End: Install Apx server.' -ForegroundColor Green
}

function UninstallApxServerClick($build)
{
	write-host 'Start: Uninstall Apx server.' -ForegroundColor Green
    $setup= Join-Path -Path $build -ChildPath 'ApxServer\Setup.exe'
	if(test-path $setup)
	{
        $ver = GetInstalledVersion
        if ($ver -ne $null)
        {
            $msg = 'Uninstall APX Server {0}?' -f $ver
			$result = ShowYesNo($msg)
			if($result -eq $true)
		    {
			    UninstallApxServer $setup
				DropApxDatabases				
			}
        }
        else
        {
			write-host 'Apx Server is not installed.' -ForegroundColor Yellow
        }
	}
	else
	{
		ShowNotFoundError($setup)
	}
	write-host 'End: Uninstall Apx server.' -ForegroundColor Green
}

function GetInstalledVersion()
{
    $path = 'HKLM:\SOFTWARE\WOW6432Node\Advent\InstallerSettings\APX'
    if (test-path -Path $path)
    {
		try
		{
			$version = Get-ItemPropertyValue -Path $path -Name Version
			return $version
		}
		catch
		{
			write-host 'Version registry key or value is not found.'
			return $null
		}        
    }
    return $null
}

function RestoreApxDatabasesClick($bakPath)
{
	write-host 'Start: Restoring Apx databases.' -ForegroundColor Green
    $ver = GetInstalledVersion
    if ($ver -ne $null)
    {
        ShowVersionWarning($ver)
    }
    else
    {
        if(Test-Path $bakpath)
        {
			
            RestoreApxDatabases $bakpath
        }        
        else
        {
			ShowNotFoundError($bakPath)
        }
    }
	write-host 'End: Restoring Apx databases. ' -ForegroundColor Green
}

function RestoreApxDatabases($bakPath)
{
	if(Test-Path $bakpath)
	{
		$sqlDataPath= (Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\MSSQLServer\Setup -Name sqlpath) + '\data'    
		if (Test-Path (Join-Path -Path $bakpath -ChildPath 'ApxFirm.bak'))
		{   
			$query = "RESTORE DATABASE [ApxFirm] FROM  DISK = N'$bakpath\ApxFirm.bak' WITH  FILE = 1, NOUNLOAD, REPLACE, STATS = 10"
			invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
			write-host 'ApxFirm.bak is restored.'
		}
		else
		{
			ShowNotFoundError('ApxFirm.bak')
		}
		
		if (Test-Path (Join-Path -Path $bakpath -ChildPath 'ApxFirm_Archive.bak'))
		{
			$query = "RESTORE DATABASE [ApxFirm_Archive] FROM  DISK = N'$bakpath\ApxFirm_Archive.bak' WITH  FILE = 1, NOUNLOAD,  REPLACE, STATS = 10"
			invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 300
			write-host 'ApxFirm_Archive.bak is restored.'
		}
		else
		{
			ShowNotFoundError('ApxFirm_Archive.bak')
		}
		
		if (Test-Path (Join-Path -Path $bakpath -ChildPath 'APXFirm_Doc.bak'))
		{   
			$query = "RESTORE DATABASE [APXFirm_Doc] FROM  DISK = N'$bakpath\APXFirm_Doc.bak' WITH  FILE = 1, NOUNLOAD,  REPLACE, STATS = 10"
			invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 300
			write-host 'APXFirm_Doc.bak is restored.'
		}
		else
		{
			ShowNotFoundError('APXFirm_Doc.bak')
		}
	}
	else
	{
		ShowNotFoundError($bakPath)
	}
}

function PrepareApxServerInstallXml($xmlpath, $dbserver, $appserver, $webserver, $site,$idsAuthority)
{
    if(($xmlpath -ne $null) -and (test-path $xmlpath))
    {
        $xml = (Get-Content -Path $xmlpath -Raw)
		$xml = $xml -replace '{dbserver}', $dbserver
		$xml = $xml -replace '{appserver}', $appserver
		$xml = $xml -replace '{webserver}', $webserver
		$xml = $xml -replace '{website}', $site
		$xml = $xml -replace '{idsAuthority}', $idsAuthority
		$xml = $xml -replace '{temp}', ${env:temp}
		$xml = $xml -replace '{installdir}', (Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath '\advent\apx')
		$saveTo = Join-Path -Path ${env:temp} -ChildPath 'ApxServerInstall.xml'
        Set-Content -Path $saveTo -Value $xml -Force
		return $saveTo
    }
    else
    {
		ShowNotFoundError($xmlpath)
    }
}

function InstallApxServer($setup,$xmpPath)
{
	RemoveInstallShield
	
	write-host 'Installing Apx server.'
    $arg='/z"SuppressDialogs:;InstallXml:\"{0}""' -f $xmpPath
	Start-Process -FilePath $setup -ArgumentList $arg -Verb 'runas' -Wait
	
	write-host 'Starting Apx services.'
	StartApxServices
	
	write-host 'Setting user passwords'
    ResetUserPasswords
	return $true
}

function UninstallApxServer($setup)
{
	write-host 'Stopping Apx services.'
	StopApxServices
	write-host 'Uninstalling Apx server.'
	Start-Process -FilePath $setup -ArgumentList '/Remove' -Wait
}

function DropDatabase($database)
{
	$query = "IF DB_ID('{0}') IS NOT NULL ALTER DATABASE [{0}] SET ONLINE, SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IF EXISTS [{0}];" -f $database
	invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
	write-host ($database, 'is dropped.')
}

function DropApxDatabases()
{
	write-host ('Start: dropping APX databases') -ForegroundColor Green
	$ver = GetInstalledVersion
	if ($ver -eq $null)
	{ 
		$result = ShowYesNo('Drop APX Databases?')
		if($result -eq $true)
		{
			DropDatabase 'APXController'
			DropDatabase 'APXController_Archive'
			DropDatabase 'APXFirm'
			DropDatabase 'APXFirm_Archive'
			DropDatabase 'APXFirm_Doc'
			DropDatabase 'APXFirm_Temp'
			DropDatabase 'MDM'
			DropDatabase 'AdventIdentityServices'
		}
	}
	else
	{
		ShowVersionWarning($ver)
	}
	write-host ('End: dropping APX databases') -ForegroundColor Green
}

function ShowVersionWarning($ver)
{
	write-host ($ver, 'is installed. You must uninstall it first!') -ForegroundColor Yellow
}

function ShowNotFoundError($path)
{
	write-host ($path, 'is not found.') -ForegroundColor Red
}

function ShowYesNo($msg)
{
	$result = [System.Windows.Forms.MessageBox]::Show($msg,'Question',[System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Question)
	if($result -eq [System.Windows.Forms.DialogResult]::Yes)
	{
		return $true
	}
	
	return $false
}

function ResetUserPasswords()
{
	 $query = "IF (EXISTS (SELECT * FROM master.dbo.sysdatabases WHERE name='APXFirm')) 
		BEGIN
			DECLARE @NewPassword varchar(100) = 'advs'
			DECLARE @NewPasswordEncrypted varbinary(260) 
			EXEC master.dbo.xp_AdvPasswordEncrypt 'Rio.1', @NewPassword, @NewPasswordEncrypted output 
			BEGIN TRANSACTION 
			EXEC APXFirm.dbo.pAdvAuditEventBegin @userID = -1001, @functionID=24 
			UPDATE APXFirm.dbo.AOUser SET EncryptedPassword = @NewPasswordEncrypted WHERE userid not in (-1005,-41,-24) 
			EXEC APXFirm.dbo.pAdvAuditEventEnd 
			COMMIT TRANSACTION
		END"

     invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
}

function StopApxServices()
{
	Start-Process powershell -Verb runAs -Wait -ArgumentList 'Stop-Process -name AdvApplicationServer -force -ErrorAction SilentlyContinue;
	Stop-Process -name iexplore -force -ErrorAction SilentlyContinue;
	Stop-Service QubeServer -Force -ErrorAction SilentlyContinue;
	Stop-Service AdvApplicationServer -Force -ErrorAction SilentlyContinue;
	Stop-Service APXMsgManager -Force -ErrorAction SilentlyContinue;
	Stop-Service ApxWatcher -Force -ErrorAction SilentlyContinue;
	Stop-Service AxGate -Force -ErrorAction SilentlyContinue;
	Stop-Service AdvJobManager -Force -ErrorAction SilentlyContinue;
	Stop-Service MDME -Force -ErrorAction SilentlyContinue;
	Stop-Service APXExchangeSync -Force -ErrorAction SilentlyContinue;
	Stop-Service APXExchangeData -Force -ErrorAction SilentlyContinue;
	Stop-Service AdventIdentityServer -Force -ErrorAction SilentlyContinue;
	Stop-Service AdventTraefik -Force -ErrorAction SilentlyContinue;
	Stop-Service AdventConsul -Force -ErrorAction SilentlyContinue;
	Stop-Service RepToJsonService -Force -ErrorAction SilentlyContinue'
}

function StartApxServices()
{
	Start-Process powershell -Verb runAs -Wait -ArgumentList 'Start-Service AdventConsul -ErrorAction SilentlyContinue;
	Start-Service AdventTraefik -ErrorAction SilentlyContinue;
	Start-Service QubeServer -ErrorAction SilentlyContinue;
	Start-Service AdvApplicationServer -ErrorAction SilentlyContinue;
	Start-Service APXMsgManager -ErrorAction SilentlyContinue;
	Start-Service ApxWatcher -ErrorAction SilentlyContinue;
	Start-Service AxGate -ErrorAction SilentlyContinue;
	Start-Service AdvJobManager -ErrorAction SilentlyContinue;
	Start-Service MDME -ErrorAction SilentlyContinue;
	Start-Service APXExchangeSync -ErrorAction SilentlyContinue;
	Start-Service APXExchangeData -ErrorAction SilentlyContinue;
	Start-Service RepToJsonService -ErrorAction SilentlyContinue;
	Start-Service AdventIdentityServer -ErrorAction SilentlyContinue'
}

function RemoveInstallShield()
{
	$path= Join-Path -Path ${Env:ProgramFiles(x86)} -ChildPath 'InstallShield Installation Information\{DF6FC27D-3D51-40AF-94EF-CFF2322B702C}'
	if(test-path -Path $path)
	{
		write-host 'Removing InstallShield registry.'
		remove-item -Path $path -Recurse -Force
	}
}

function InstallApxClient($appServer,$webBaseUrl)
{
	$msiPath='\\{0}\apx$\Setup\ApxClient\ApxClient.msi' -f $appServer
	if(-not (test-path -Path $msiPath))
	{
		$msiPath='\\{0}\apx$\Setup\ApxClient\x86\ApxClient.msi' -f $appServer
		if(${Env:processor_architecture} -eq 'AMD64')
		{
			$msiPath='\\{0}\apx$\Setup\ApxClient\x64\ApxClient.msi' -f $appServer
		}
	}
	
	if(test-path -Path $msiPath)
	{
		write-host 'Klling iexplore process.'
		Stop-Process -name iexplore -force -ErrorAction SilentlyContinue
		
		write-host 'Installing ApxClient'
		$installDir=Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath 'Advent\APXClient'
		$scriptBlock='msiexec /i {0} /qr APX_WEBSERVER={1} ADDLOCAL=ALL INSTALLDIR="{2}"' -f $msiPath,$webBaseUrl,$installDir
		Invoke-Command -ScriptBlock { & cmd /c $scriptBlock}
		
		write-host 'Setting IE home page.'
		$regScriptBlock='reg add "HKcu\Software\Microsoft\Internet Explorer\Main" /d {0}/APXLogin  /v "Start Page" /f'-f $webBaseUrl
		Invoke-Command -ScriptBlock { & cmd /c $regScriptBlock}
	}
	else
	{
		ShowNotFoundError($msiPath)
	}
}

function InstallApxNextgen($buildPath,$apiUrl)
{	
	$msiPath = join-path -Path $buildPath -ChildPath 'APXNextgenSetup.msi'
	if(test-path -Path $msiPath)
	{
        $installFolder=Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath 'Advent\APX\APXUI'
        $logfile=Join-Path -Path $installFolder -ChildPath 'UI_install.log'
		$scriptBlock='msiexec /i {0} /qr WEBSITENAME="Default Web Site" APX_API_URL="{1}" INSTALLFOLDER="{2}" Ids_Username="admin" Ids_Password="advs"' -f $msiPath,$apiUrl,$installFolder
		Invoke-Command -ScriptBlock { & cmd /c $scriptBlock}
	}
	else
	{
		ShowNotFoundError($msiPath)
	}
}

function UninstallApxNextgen()
{
	$scriptBlock='msiexec /x {e5df4831-2502-4a20-ae3b-4e45dc134bc3} /qb'
	Invoke-Command -ScriptBlock { & cmd /c $scriptBlock}
}

ShowParameterInputDialog