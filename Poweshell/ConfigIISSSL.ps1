# 1. Create a new self-signed certificate to Personal store. Or if you've had a certificate file, you can use Import-Certificate or Import-PfxCertificate.
$cert = New-SelfSignedCertificate -CertStoreLocation cert:\localmachine\my -Subject CN=*.advent.com -FriendlyName IISSSL
 
# 2. Add the new self-signed certificate to Trust Root store as well.
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store 'root','LocalMachine'
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
$store.Add($cert)
$store.Close
 
# 3. Set IIS Web Site Bindings.
$site = 'Default Web Site'
$port = 443
$protocol = 'https'
$bind=Get-WebBinding -Name $site -Port $port -Protocol $protocol
if ($null -eq $bind) {
    New-WebBinding -Name $site -Port $port -Protocol $protocol
    $bind = Get-WebBinding -Name $site -Port $port -Protocol $protocol
}
 
$bind.AddSslCertificate($cert.GetCertHashString(),'my')