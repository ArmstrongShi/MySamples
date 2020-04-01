
# on your web/IIS server
$sys = get-wmiobject win32_computersystem
$subject='CN=*.'+$sys.Domain
$FriendlyName='SelfSigned.'+$sys.name
$site = 'Default Web Site'
$port='443'
$protocol = 'https'

# 1. Create a new self-signed certificate to Personal store. Or if you've had a certificate file, you can use Import-Certificate or Import-PfxCertificate.
$cert = New-SelfSignedCertificate -CertStoreLocation cert:\localmachine\my -Subject $subject -FriendlyName $FriendlyName -NotAfter (Get-Date).AddYears(20)
 
# 2. Add the new self-signed certificate to Trust Root store as well.
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store 'root','LocalMachine'
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
$store.Add($cert)
$store.Close
 
# 3. Set IIS Web Site Bindings HTTPS
$bind=Get-WebBinding -Name $site -Port $port -Protocol $protocol
if ($null -eq $bind) {
    New-WebBinding -Name $site -Port $port -Protocol $protocol
    $bind=Get-WebBinding -Name $site -Port $port -Protocol $protocol
}
$bind.AddSslCertificate($cert.GetCertHashString(),'my')
 
# 4. (Optional)Set web site to Require SSL & Ignore Client certificates by default. Or if you want to setup "Accept" or "Require" Client certificates, you can use values 'Ssl,SslNegotiateCert' or 'Ssl,SslRequireCert'
# Set-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Location $site -filter 'system.webServer/security/access' -Name 'sslFlags' -Value 'Ssl'