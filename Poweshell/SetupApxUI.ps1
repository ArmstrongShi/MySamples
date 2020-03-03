
$sys = get-wmiobject win32_computersystem
$webServer = 'HTTP://' + $sys.Name + '.' + $sys.Domain
$installFolder=${env:programfiles(x86)}+"\Advent\APX\APXUI"

[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") 
[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") 

function ShowParameterInputDialog()
{
	$objForm = New-Object System.Windows.Forms.Form 
	$objForm.Text = "APX UI Component Setup Script"
	$objForm.Size = New-Object System.Drawing.Size(530,140) 
	$objForm.StartPosition = "CenterScreen"
	$objForm.AutoSize = $True;
		
	$buildPathLabel = New-Object System.Windows.Forms.Label
	$buildPathLabel.Location = New-Object System.Drawing.Size(10,20) 
	$buildPathLabel.Size = New-Object System.Drawing.Size(100,20) 
	$buildPathLabel.Text = "Build:"
	$objForm.Controls.Add($buildPathLabel) 
	
	$buildPathTextBox = New-Object System.Windows.Forms.TextBox
	$buildPathTextBox.Location = New-Object System.Drawing.Size(110,20) 
	$buildPathTextBox.Size = New-Object System.Drawing.Size(300,20) 
	
	$objForm.Controls.Add($buildPathTextBox)
	
    $browseButton = New-Object System.Windows.Forms.Button
	$browseButton.Location = New-Object System.Drawing.Size(420,20)
	$browseButton.Size = New-Object System.Drawing.Size(80,20)
	$browseButton.Text = "Browse"
	$browseButton.Add_Click({$buildPathTextBox.Text=FindBuild })
	$objForm.Controls.Add($browseButton)

	$ApxServerLabel = New-Object System.Windows.Forms.Label
	$ApxServerLabel.Location = New-Object System.Drawing.Size(10,50) 
	$ApxServerLabel.Size = New-Object System.Drawing.Size(100,20) 
	$ApxServerLabel.Text = "APX Server:"
	$objForm.Controls.Add($ApxServerLabel) 
	
	$ApxServerTextBox = New-Object System.Windows.Forms.TextBox
	$ApxServerTextBox.Location = New-Object System.Drawing.Size(110,50) 
	$ApxServerTextBox.Size = New-Object System.Drawing.Size(400,20) 
	$ApxServerTextBox.Text = $webServer
	$objForm.Controls.Add($ApxServerTextBox)

	$installButton = New-Object System.Windows.Forms.Button
	$installButton.Location = New-Object System.Drawing.Size(320,80)
	$installButton.Size = New-Object System.Drawing.Size(80,20)
	$installButton.Text = "Install"
	$installButton.Add_Click({InstallAPXUI $buildPathTextBox.Text $ApxServerTextBox.Text})
	$objForm.Controls.Add($installButton)

	$removeButton = New-Object System.Windows.Forms.Button
	$removeButton.Location = New-Object System.Drawing.Size(420,80)
	$removeButton.Size = New-Object System.Drawing.Size(80,20)
	$removeButton.Text = "Uninstall"
	$removeButton.Add_Click({UninstallAPXUI})
	$objForm.Controls.Add($removeButton)
	
	$objForm.Add_Shown({$objForm.Activate()})
	[void] $objForm.ShowDialog()
}

function FindBuild()
{
    $dialog = New-Object System.Windows.Forms.OpenFileDialog
    $dialog.InitialDirectory = "\\source.advent.com\img\qaimg\APX\APX_UI"
    $result = $dialog.ShowDialog()
    if($result -eq [System.Windows.Forms.DialogResult]::OK)
    {
        return $dialog.FileName        
    }
}

function InstallAPXUI($msiPath,$server)
{	
	UninstallAPXUI
	
	if(test-path -Path $msiPath)
	{
		$scriptBlock=[System.String]::Format('msiexec /i {0} /qr WEBSITENAME="Default Web Site" APX_API_URL="{1}" INSTALLFOLDER="{2}" Ids_Username="admin" Ids_Password="advs"',$msiPath,$server,$installFolder)
		InvokeCommand $scriptBlock
	}
}

function UninstallAPXUI()
{
	InvokeCommand 'msiexec /x {e5df4831-2502-4a20-ae3b-4e45dc134bc3} /qb'
}

function InvokeCommand ($scriptBlock) 
{
	Stop-Process -name iexplore -force -ErrorAction SilentlyContinue
	Invoke-Command -ScriptBlock { & cmd /c $scriptBlock}
}

ShowParameterInputDialog