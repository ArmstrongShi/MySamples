
$defaultXmlPath=${env:temp}+'\ApxServerInstall.xml'
$defaultBakPath=${env:systemdrive}+'\backup'
$sys = get-wmiobject win32_computersystem
$appserverDns = $sys.Name+'.'+$sys.Domain
$webserverUrl = 'HTTP://'+ $appserverDns
$idsProxyUrl = 'HTTP://' + $appserverDns +'/oauth'
$buildRoot='\\source.advent.com\Img\qaimg\APX\APXSetup'
$buildPattern='20_10_0_*'

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
	$idsTextBox.Text = $idsProxyUrl
	$idsGroup.Controls.Add($idsTextBox)


    $apxPos = $idsPos + $idsSize + 10
    $apxSize = 260
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
	
    $xmlTextBox = New-Object System.Windows.Forms.TextBox
	$xmlTextBox.Location = New-Object System.Drawing.Point(70,$xmlPos) 
	$xmlTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$xmlTextBox.Text = $defaultXmlPath
	$apxGroup.Controls.Add($xmlTextBox)
	
    $xmlButton = New-Object System.Windows.Forms.Button
	$xmlButton.Location = New-Object System.Drawing.Point(440,$xmlPos)
	$xmlButton.Size = New-Object System.Drawing.Size(70,20)
	$xmlButton.Text = "Find"
	$xmlButton.Add_Click({$xmlTextBox.Text=FindXml ${Env:temp} })
	$apxGroup.Controls.Add($xmlButton)

    $buildPos = $xmlPos + 30

	$buildLabel = New-Object System.Windows.Forms.Label
	$buildLabel.Location = New-Object System.Drawing.Point(10,$buildPos) 
	$buildLabel.Size = New-Object System.Drawing.Size(60,20) 
	$buildLabel.Text = "Build"
	$apxGroup.Controls.Add($buildLabel) 

    $buildTextBox = New-Object System.Windows.Forms.TextBox
	$buildTextBox.Location = New-Object System.Drawing.Point(70,$buildPos) 
	$buildTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$buildTextBox.Text = GetLatestBuild $buildRoot $buildPattern
	$apxGroup.Controls.Add($buildTextBox)
	
	$buildButton = New-Object System.Windows.Forms.Button
	$buildButton.Location = New-Object System.Drawing.Size(440,$buildPos)
	$buildButton.Size = New-Object System.Drawing.Size(70,20)
	$buildButton.Text = "Browse"
	$buildButton.Add_Click({FindBuild $buildRoot})
	$apxGroup.Controls.Add($buildButton)

    $bakPos = $buildPos + 30
	
	$bakLabel = New-Object System.Windows.Forms.Label
	$bakLabel.Location = New-Object System.Drawing.Point(10,$bakPos) 
	$bakLabel.Size = New-Object System.Drawing.Size(60,20) 
	$bakLabel.Text = "Backup"
	$apxGroup.Controls.Add($bakLabel) 
	
	$bakTextBox = New-Object System.Windows.Forms.TextBox
	$bakTextBox.Location = New-Object System.Drawing.Point(70,$bakPos) 
	$bakTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$bakTextBox.Text = $defaultBakPath
	$apxGroup.Controls.Add($bakTextBox)

	$bakButton = New-Object System.Windows.Forms.Button
	$bakButton.Location = New-Object System.Drawing.Size(440,$bakPos)
	$bakButton.Size = New-Object System.Drawing.Size(70,20)
	$bakButton.Text = "Restore"
	$bakButton.Add_Click({RestoreApxFirm $bakTextBox.Text})
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
	$dbTextBox.Text = $appserverDns
	$apxGroup.Controls.Add($dbTextBox)

    $appPos = $dbpos + 30

	$appLabel = New-Object System.Windows.Forms.Label
	$appLabel.Location = New-Object System.Drawing.Point(10,$appPos) 
	$appLabel.Size = New-Object System.Drawing.Size(60,20) 
	$appLabel.Text = "Application"
	$apxGroup.Controls.Add($appLabel) 

    $appTextBox = New-Object System.Windows.Forms.TextBox
	$appTextBox.Location = New-Object System.Drawing.Point(70,$appPos) 
	$appTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$appTextBox.Text = $appserverDns
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
	$webTextBox.Text = $webserverUrl
	$apxGroup.Controls.Add($webTextBox)
	
    $btnPos = $webpos + 30

	$installButton = New-Object System.Windows.Forms.Button
	$installButton.Location = New-Object System.Drawing.Point(330,$btnPos)
	$installButton.Size = New-Object System.Drawing.Size(80,20)
	$installButton.Text = "Install"
	$installButton.Add_Click({InstallButtonClick $buildTextBox.Text $xmlTextBox.Text $dbTextBox.Text $appTextBox.Text $webTextBox.Text $idsTextBox.Text $bakTextBox.Text})
	$apxGroup.Controls.Add($installButton)

	$removeButton = New-Object System.Windows.Forms.Button
	$removeButton.Location = New-Object System.Drawing.Point(430,$btnPos)
	$removeButton.Size = New-Object System.Drawing.Size(80,20)
	$removeButton.Text = "Remove"
	$removeButton.Add_Click({RemoveButtonClick $buildTextBox.Text})
	$apxGroup.Controls.Add($removeButton) 

    $ngPos = $apxPos +$apxSize+ 10
    $ngSize = 80
    $ngGroup = New-Object System.Windows.Forms.GroupBox
    $ngGroup.Location = New-Object System.Drawing.Point(10,$ngPos) 
	$ngGroup.Size = New-Object System.Drawing.Point(530,$ngSize) 
    $ngGroup.Text = "APX New UI"
    $ngGroup.Enabled = $false
    $objForm.Controls.Add($ngGroup)

    $ngbuildPos = 20

	$ngLabel = New-Object System.Windows.Forms.Label
	$ngLabel.Location = New-Object System.Drawing.Point(10,$ngbuildPos) 
	$ngLabel.Size = New-Object System.Drawing.Size(60,20) 
	$ngLabel.Text = "Build"
	$ngGroup.Controls.Add($ngLabel) 
	
    $ngTextBox = New-Object System.Windows.Forms.TextBox
	$ngTextBox.Location = New-Object System.Drawing.Point(70,$ngbuildPos) 
	$ngTextBox.Size = New-Object System.Drawing.Size(350,20) 
	$ngTextBox.Text = "TBD"
	$ngGroup.Controls.Add($ngTextBox)

	$ngButton = New-Object System.Windows.Forms.Button
	$ngButton.Location = New-Object System.Drawing.Point(440,$ngbuildPos)
	$ngButton.Size = New-Object System.Drawing.Size(70,20)
	$ngButton.Text = "Browse"
	$ngButton.Add_Click({FindBuild $buildRoot})
	$ngGroup.Controls.Add($ngButton)

    $ngbtnPos = $ngbuildPos + 30

	$nginstallButton = New-Object System.Windows.Forms.Button
	$nginstallButton.Location = New-Object System.Drawing.Point(330,$ngbtnPos)
	$nginstallButton.Size = New-Object System.Drawing.Size(80,20)
	$nginstallButton.Text = "Install"
	$nginstallButton.Add_Click({})
	$ngGroup.Controls.Add($nginstallButton)

	$ngremoveButton = New-Object System.Windows.Forms.Button
	$ngremoveButton.Location = New-Object System.Drawing.Point(430,$ngbtnPos)
	$ngremoveButton.Size = New-Object System.Drawing.Size(80,20)
	$ngremoveButton.Text = "Remove"
	$ngremoveButton.Add_Click({})
	$ngGroup.Controls.Add($ngremoveButton) 

    $height = $ngPos +$ngsize+ 50
	$objForm.Size = New-Object System.Drawing.Size(570,$height)
	$objForm.Add_Shown({$objForm.Activate()})
	[void] $objForm.ShowDialog()
}

function FindBuild($root)
{
    invoke-item -Path $root
}

function GetLatestBuild($root,$pattern)
{
	$buildPaths=[IO.Directory]::GetDirectories($root, $pattern)
	[System.Array]::Sort($buildPaths, [System.StringComparer]::InvariantCultureIgnoreCase)
	[System.Array]::Reverse($buildPaths)
	
	foreach($buildPath in $buildPaths)
	{
        $fullBuildIndicator = [System.String]::Format('{0}\{1}',$buildPath,'By_Manual.txt')
        if(test-path -Path $fullBuildIndicator)
        {
            continue
        }
        else
		{
			break
		}
	}
	
	#$setupExePath=[System.String]::Format('{0}\ApxServer\Setup.exe', $buildPath)
	return $buildPath
}

function FindXml($inidir)
{
    $dialog = New-Object System.Windows.Forms.OpenFileDialog
    $dialog.InitialDirectory = $inidir
    $dialog.Filter='XML Document|*.xml'
	$dialog.FileName = $defaultXmlPath
    $result = $dialog.ShowDialog()
    if($result -eq [System.Windows.Forms.DialogResult]::OK)
    {
        return $dialog.FileName        
    }
    else
    {
        return $defaultXmlPath
    }
}

function InstallButtonClick($buildPath,$installXmlPath,$dbserver,$appserver,$webserver,$idsAuthority,$bakpath)
{
    PrepareInstallXml $installXmlPath $dbserver $appserver $webserver $idsAuthority
	$setupExePath= [System.String]::Format('{0}\ApxServer\Setup.exe', $buildPath)
	if(test-path -Path $setupExePath)
	{
        $ver = GetInstalledVersion
        if ($ver -ne $null)
        {
            $msg = 'APX '+$ver+' is installed. Continue? '
            $result = [System.Windows.Forms.MessageBox]::Show($msg,'Question',[System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Question)
            if($result -eq [System.Windows.Forms.DialogResult]::Yes)
		    {
                RemoveApxServer $setupExePath
			    RemoveApxDatabase
                RestoreApxFirm $bakpath
                $success = InstallApxServer $setupExePath $installXmlPath
		        if($success)
		        {	
			        InstallApxClient $appserver $webserver
		        }
            }
        }
        else
        {
            $success = InstallApxServer $setupExePath $installXmlPath
		    if($success)
		    {	
			    InstallApxClient $appserver $webserver
		    }
        }
	}
	else
	{
		$msg=[System.String]::Format('File does not exist: {0}', $setupExePath)
		ShowErrorMessage $msg
	}
}

function RemoveButtonClick($buildPath)
{	
	$setupExePath= [System.String]::Format('{0}\ApxServer\Setup.exe', $buildPath)
	if(test-path -Path $setupExePath)
	{
        $ver = GetInstalledVersion
        if ($ver -ne $null)
        {
            $msg = 'Uninstall APX '+$ver+'?'
		    $result=[System.Windows.Forms.MessageBox]::Show($msg,'Question',[System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Question)
		    if($result -eq [System.Windows.Forms.DialogResult]::Yes)
		    {
			    RemoveApxServer $setupExePath
			    RemoveApxDatabase
		    }
        }
        else
        {
            [System.Windows.Forms.MessageBox]::Show('APX is not installed.','Information',[System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        }
	}
	else
	{
		$msg=[System.String]::Format('File does not exist: {0}', $setupExePath)
		ShowErrorMessage $msg
	}
}

function GetInstalledVersion()
{
    try
    {
        $version = Get-ItemPropertyValue -Path HKLM:\SOFTWARE\WOW6432Node\Advent\InstallerSettings\APX -Name Version
        return $version
    }
    catch
    {
        return $null
    }
}


function RestoreApxFirm($bakPath)
{
    $sqlDataPath= (Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\MSSQLServer\Setup -Name sqlpath) + '\data'
    
    If (Test-Path "$bakPath\ApxFirm.bak")
    {   
        $query = "RESTORE DATABASE [ApxFirm] 
            FILE = N'FirmSys',  
            FILE = N'FirmData',  
            FILE = N'FirmIx' 
            FROM  DISK = N'$bakpath\ApxFirm.bak' 
            WITH  FILE = 1,  
            MOVE N'FirmSys' TO N'$sqlDataPath\ApxFirm.mdf',  
            MOVE N'FirmData' TO N'$sqlDataPath\ApxFirm_0.ndf',  
            MOVE N'FirmIx' TO N'$sqlDataPath\ApxFirm_1.ndf',  
            MOVE N'FirmLog' TO N'$sqlDataPath\ApxFirm_2.ldf',  
            NOUNLOAD, REPLACE, STATS = 10"
        invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
    }

    If (Test-Path "$bakPath\ApxFirm_Archive.bak")
    {
        $query = "RESTORE DATABASE [ApxFirm_Archive] 
            FILE = N'FirmArchSys' 
            FROM  DISK = N'$bakpath\ApxFirm_Archive.bak' 
            WITH  FILE = 1,  
            MOVE N'FirmArchSys' TO N'$sqlDataPath\ApxFirm_Archive.mdf',  
            MOVE N'FirmArchLog' TO N'$sqlDataPath\ApxFirm_Archive_0.ldf',  
            NOUNLOAD,  REPLACE, STATS = 10"
        invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 300
    }

    If (Test-Path "$bakPath\APXFirm_Doc.bak")
    {   
        $query = "RESTORE DATABASE [APXFirm_Doc] 
            FILE = N'FirmDocSys' 
            FROM  DISK = N'$bakpath\APXFirm_Doc.bak' 
            WITH  FILE = 1, 
            MOVE N'FirmDocSys' TO N'$sqlDataPath\APXFirm_Doc.mdf', 
            MOVE N'FirmDocLog' TO N'$sqlDataPath\APXFirm_Doc_0.ldf',
            NOUNLOAD,  REPLACE, STATS = 10"
        invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 300
    }
}

function PrepareInstallXml($installXmlPath, $dbserver, $appserver, $webserver, $idsAuthority)
{
    if(test-path -Path $installXmlPath)
    {
        $xml = (Get-Content -Path $installXmlPath -Raw)
		$xml = $xml.Replace('{dbserver}', $dbserver)
		$xml = $xml.Replace('{appserver}', $appserver)
		$xml = $xml.Replace('{webserver}', $webserver)
		$xml = $xml.Replace('{idsAuthority}', $idsAuthority)
		$xml = $xml.Replace('{temp}', ${env:temp})
		$xml = $xml.Replace('{installdir}', ${env:ProgramFiles(x86)}+'\advent\apx')
        Set-Content -Path $installXmlPath -Value $xml -Force
    }
}

function InstallApxServer($setupExePath,$installXmlPath)
{
	RemoveInstallShield
	if(test-path -Path $setupExePath)
	{
		if(test-path -Path $installXmlPath)
		{
			$arg=[System.String]::Format('/z"SuppressDialogs:;InstallXml:\"{0}""', $installXmlPath)
			Start-Process -FilePath $setupExePath -ArgumentList $arg -Verb 'runas' -Wait
			StartApxServices
            ResetAdminPassword
			return $true
		}
		else
		{
			$msg=[System.String]::Format('File does not exist: {0}.', $installXmlPath)
			ShowErrorMessage $msg
			return $false
		}
	}
	else
	{
		$msg=[System.String]::Format('File does not exist: {0}.', $setupExePath)
		ShowErrorMessage $msg
		return $false
	}
}

function RemoveApxServer($setupExePath)
{
	if(test-path -Path $setupExePath)
	{
		Stop-Process -name iexplore -force -ErrorAction SilentlyContinue
		StopApxServices
		Start-Process -FilePath $setupExePath -ArgumentList '/Remove' -Wait
	}
	else
	{
		$msg=[System.String]::Format('File does not exist: {0}.', $setupExePath)
		ShowErrorMessage $msg
	}
}

function RemoveApxDatabase()
{
	 $query = "ALTER DATABASE [APXController] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [APXController]; 
         ALTER DATABASE [APXController_Archive] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [APXController_Archive]; 
         ALTER DATABASE [APXFirm] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [APXFirm]; 
         ALTER DATABASE [APXFirm_Archive] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [APXFirm_Archive];
         ALTER DATABASE [APXFirm_Doc] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [APXFirm_Doc]; 
         ALTER DATABASE [APXFirm_Temp] 
         SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [APXFirm_Temp]; 
         ALTER DATABASE [MDM] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [MDM];
         ALTER DATABASE [AdventIdentityServices] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
         DROP DATABASE [AdventIdentityServices];"

     invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
}

function ResetAdminPassword()
{
	 $query = "USE [APXFirm] DECLARE @NewPassword varchar(100) SET @NewPassword = 'advs' DECLARE @NewPasswordEncrypted varbinary(260) 
         EXEC master.dbo.xp_AdvPasswordEncrypt 'Rio.1', @NewPassword, @NewPasswordEncrypted output 
         SELECT @NewPasswordEncrypted begin transaction EXEC pAdvAuditEventBegin @userID = -1001, @functionID=24 
         UPDATE AOUser SET EncryptedPassword = @NewPasswordEncrypted WHERE userid not in (-1005,-41,-24) EXEC pAdvAuditEventEnd commit transaction"

     invoke-sqlcmd -Username 'sa' -Password 'Advent.sa' -Query $query -QueryTimeout 600
}

function StopApxServices()
{
	Start-Process powershell -Verb runAs -Wait -ArgumentList 'Stop-Service QubeServer -Force -ErrorAction SilentlyContinue;
	Stop-Service AdvApplicationServer -Force -ErrorAction SilentlyContinue;
	Stop-Service APXMsgManager -Force -ErrorAction SilentlyContinue;
	Stop-Service ApxWatcher -Force -ErrorAction SilentlyContinue;
	Stop-Service AxGate -Force -ErrorAction SilentlyContinue;
	Stop-Service AdvJobManager -Force -ErrorAction SilentlyContinue;
	Stop-Service MDME -Force -ErrorAction SilentlyContinue;
	Stop-Service APXExchangeSync -Force -ErrorAction SilentlyContinue;
	Stop-Service APXExchangeData -Force -ErrorAction SilentlyContinue;
	Stop-Service AdventIdentityServer -ErrorAction SilentlyContinue;
	Stop-Process -name iexplore -force -ErrorAction SilentlyContinue'
}

function StartApxServices()
{
	Start-Process powershell -Verb runAs -Wait -ArgumentList 'Start-Service QubeServer -ErrorAction SilentlyContinue;
	Start-Service AdvApplicationServer -ErrorAction SilentlyContinue;
	Start-Service APXMsgManager -ErrorAction SilentlyContinue;
	Start-Service ApxWatcher -ErrorAction SilentlyContinue;
	Start-Service AxGate -ErrorAction SilentlyContinue;
	Start-Service AdvJobManager -ErrorAction SilentlyContinue;
	Start-Service MDME -ErrorAction SilentlyContinue;
	Start-Service APXExchangeSync -ErrorAction SilentlyContinue;
	Start-Service APXExchangeData -ErrorAction SilentlyContinue;
	Start-Service AdventIdentityServer -ErrorAction SilentlyContinue'
}

function RemoveInstallShield()
{
	$path=[System.String]::Format('{0}\InstallShield Installation Information\{1}',${Env:ProgramFiles(x86)},'{DF6FC27D-3D51-40AF-94EF-CFF2322B702C}')
	if(test-path -Path $path)
	{
		remove-item -Path $path -Recurse -Force
	}
}

function InstallApxClient($appServer,$webserverUrl)
{
	$msiPath=[System.String]::Format('\\{0}\apx$\Setup\ApxClient\ApxClient.msi',$appServer);
	if(-not (test-path -Path $msiPath))
	{
		$msiPath=[System.String]::Format('\\{0}\apx$\Setup\ApxClient\x86\ApxClient.msi',$appServer);
		if(${Env:processor_architecture} -eq 'AMD64')
		{
			$msiPath=[System.String]::Format('\\{0}\apx$\Setup\ApxClient\x64\ApxClient.msi',$appServer);
		}
	}
	
	if(test-path -Path $msiPath)
	{
		$installDir=[System.String]::Format('{0}\Advent\APXClient', ${env:ProgramFiles(x86)})
		$scriptBlock=[System.String]::Format('msiexec /i {0} /qr APX_WEBSERVER={1} ADDLOCAL=ALL INSTALLDIR="{2}"',$msiPath,$webserverUrl,$installDir)
		
		Stop-Process -name iexplore -force -ErrorAction SilentlyContinue
		Invoke-Command -ScriptBlock { & cmd /c $scriptBlock}
		
		$regScriptBlock=[System.String]::Format('reg add "HKcu\Software\Microsoft\Internet Explorer\Main" /d {0}/APXLogin  /v "Start Page" /f',$webserverUrl)
		Invoke-Command -ScriptBlock { & cmd /c $regScriptBlock}
	}
	else
	{
		$msg=[System.String]::Format('Script:File does not exist: {0}',$msiPath);
		ShowErrorMessage $msg
	}
}

function ShowErrorMessage($msg)
{
	[System.Windows.Forms.MessageBox]::Show($msg,'Error',[System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
}

function ShowInformation($msg)
{
	[System.Windows.Forms.MessageBox]::Show($msg,'Information',[System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
}

ShowParameterInputDialog