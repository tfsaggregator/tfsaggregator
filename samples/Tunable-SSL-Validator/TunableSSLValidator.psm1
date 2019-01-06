Add-Type -Path "${PSScriptRoot}\TunableValidator.cs"

# You need to set the validator so it can do anything...
[Huddled.Net.TunableValidator]::SetValidator()


function Request-WebCertificate {
    #.Synopsis
    #  Make an SSL web request and return the SSL certificate
    [OutputType([System.Security.Cryptography.X509Certificates.X509Certificate2])]
    [CmdletBinding()]
    param(
        # The URL to fetch the SSL certificate from
        [Uri][String]$url = "https://www.csh.rit.edu"
    )

    process {
        if(!$Url.Scheme) {
            $url = "https://" + $Url
        }

        if($url.Scheme -ne "https") {
            Write-Warning "Url is not HTTPS"
            $url = "https://" + $url.Host + ":" + $url.Port + $url.PathAndQuery
        }

        $web = [Net.WebRequest]::Create($url) -as [Net.HttpWebRequest]

        $SSLCallback = {
            param($sender, $certificate, $chain, $sslPolicyErrors)
            $script:RequestedWebCertificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $certificate
            return $true
        }

        if($web | Get-Member ServerCertificateValidationCallback) {
            $web.ServerCertificateValidationCallback = $SSLCallback
            $null = $web.GetResponse()
        } else {
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $SSLCallback
            $null = $web.GetResponse()
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null
        }

        return $RequestedWebCertificate
    }
}

function Add-WindowsTrustedCertificate {
   #.Synopsis
   #  Add a certificate to the Windows certificate store
   [CmdletBinding(DefaultParameterSetName = 'Certificate')]
   param(
      [Parameter(ParameterSetName = "Certificate", Mandatory = $True, Position=0, ValueFromPipeline = $True)]
      [System.Security.Cryptography.X509Certificates.X509Certificate]$Certificate,

      [Parameter(ParameterSetName = "Path", Mandatory = $True, Position = 0, ValueFromPipeline = $True)]
      [Alias("PSPath")]
      [String]$FilePath,

      [Parameter(ParameterSetName = "Path")]
      [SecureString]$Password = $((Get-Credential $FilePath).Password),

      [ValidateSet("CurrentUser","LocalMachine")]
      [String]$Root = "CurrentUser",

      [String]$certStore = "My"
   )

   process {
      if($FilePath) {
         if(-not (Test-Path $FilePath)) {
            throw "Certificate File Not Found at '$FilePath'"
         }
         $Certificate = new-object System.Security.Cryptography.X509Certificates.X509Certificate2
         if ($Password -eq $null) {
            $Certificate.import( (Convert-Path $FilePath) )
         } else {
            $Certificate.import( (Convert-Path $FilePath), $Password, "Exportable,PersistKeySet" )
         }
      }

      $store = new-object System.Security.Cryptography.X509Certificates.X509Store($certStore, $certRootStore)
      $store.open("MaxAllowed")
      $store.add($Certificate)
      $store.close()
   }
}

function Add-SessionTrustedCertificate {
    #.Synopsis
    #  Map a certificate to a URL for this PowerShell session (does not permanently import the certificate)
    [CmdletBinding(DefaultParameterSetName = 'Certificate')]
    param(
        [Parameter(ParameterSetName = "Certificate", Mandatory = $True, Position=0, ValueFromPipeline = $True)]
        [System.Security.Cryptography.X509Certificates.X509Certificate]$Certificate,

        [Parameter(ParameterSetName = "Path", Mandatory = $True, Position = 0, ValueFromPipeline = $True)]
        [Alias("PSPath")]
        [String]$FilePath,

        [Parameter(ParameterSetName = "CertHash", Mandatory = $True)]
        [String]$Hash,

        [Parameter(ParameterSetName = "LastFailedCert", Mandatory = $True)]
        [Switch]$LastFailed,

        [Parameter(ParameterSetName = "NextFailingCert", Mandatory = $True)]
        [Switch]$NextFailingCert,

        [Parameter(ParameterSetName = "Certificate", Mandatory = $true, Position = 1)]
        [Parameter(ParameterSetName = "Path", Mandatory = $true, Position = 1)]
        [Parameter(ParameterSetName = "CertHash", Mandatory = $true, Position = 1)]
        [Uri]$Domain
    )
    process {
        if($LastFailed) {
            [Huddled.Net.TunableValidator]::ApproveLastRequest()
        } elseif($NextFailingCert) {
            [Huddled.Net.TunableValidator]::ApproveNextRequest($True)
        } else {
            if($FilePath) {
                if(-not (Test-Path $FilePath)) {
                    throw "Certificate File Not Found at '$FilePath'"
                }
                $Certificate = new-object System.Security.Cryptography.X509Certificates.X509Certificate2
                if ($Password -eq $null) {
                    $Certificate.import( (Convert-Path $FilePath) )
                } else {
                    $Certificate.import( (Convert-Path $FilePath), $Password, "Exportable,PersistKeySet" )
                }
            }
            if($Certificate) {
                $Hash = $Certificate.GetCertHashString()
            }

            $dnsName = $(if($Domain.Authority) { $Domain.Authority }elseif($Domain.Host){$Domain.Host}else{$Domain.originalstring})

            [Huddled.Net.TunableValidator]::TrustedCerts[$Hash] = $dnsname
        }
    }
}


function Get-SessionTrustedCertificate {
    foreach($key in @([Huddled.Net.TunableValidator]::TrustedCerts.Keys)) {
        New-Object PSObject -Property @{
            "CertHash" = $key
            "Domain" = [Huddled.Net.TunableValidator]::TrustedCerts[$key]
        }
    }
}

function Remove-SessionTrustedCertificate {
    param(
        [Parameter(ParameterSetName = "CertHash", Mandatory = $True, ValueFromPipelineByPropertyName = $True)]
        [Alias("CertHash")]
        [String]$Hash
    )
    process {
        Write-Verbose "Removing $Hash"
        if(![Huddled.Net.TunableValidator]::TrustedCerts.Remove($Hash)) {
            Write-Error "Couldn't find $Hash in TrustedCerts"
        }
    }
}

function Disable-ShowConsoleStandardOutput {
    #.Synopsis
    #  Disables methods from writing to the standard output stream
    [Huddled.Net.TunableValidator]::ShowConsoleStandardOutput = $False
}
function Enable-ShowConsoleStandardOutput {
    #.Synopsis
    #  Enables methods to write to the standard output stream
    [Huddled.Net.TunableValidator]::ShowConsoleStandardOutput = $True
}
function Get-ShowConsoleStandardOutput {
    #.Synopsis
    #  Retrieves the setting for whether methods to write to the standard output stream
    [Huddled.Net.TunableValidator]::ShowConsoleStandardOutput
}


function Disable-SSLChainValidation {
    #.Synopsis
    #  Disables validation of the SSL certificate chain, essentially allowing self-signed certificates
    [Huddled.Net.TunableValidator]::IgnoreChainErrors = $True
}
function Enable-SSLChainValidation {
    #.Synopsis
    #  Enables normal validation of the SSL certificate chain
    [Huddled.Net.TunableValidator]::IgnoreChainErrors = $False
}

function Get-IgnoreChainErrors {
    #.Synopsis
    #  Retrieves validation setting for the SSL certificate chain
    [Huddled.Net.TunableValidator]::IgnoreChainErrors
}


function Invoke-WebRequest {
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=217035')]
    param(
        [switch]
        ${UseBasicParsing},

        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [uri]
        ${Uri},

        [Microsoft.PowerShell.Commands.WebRequestSession]
        ${WebSession},

        [Alias('SV')]
        [string]
        ${SessionVariable},

        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        ${Credential},

        [switch]
        ${UseDefaultCredentials},

        [ValidateNotNullOrEmpty()]
        [string]
        ${CertificateThumbprint},

        [ValidateNotNull()]
        [System.Security.Cryptography.X509Certificates.X509Certificate]
        ${Certificate},

        [string]
        ${UserAgent},

        [switch]
        ${DisableKeepAlive},

        [int]
        ${TimeoutSec},

        [System.Collections.IDictionary]
        ${Headers},

        [ValidateRange(0, 2147483647)]
        [int]
        ${MaximumRedirection},

        [Microsoft.PowerShell.Commands.WebRequestMethod]
        ${Method},

        [uri]
        ${Proxy},

        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        ${ProxyCredential},

        [switch]
        ${ProxyUseDefaultCredentials},

        [Parameter(ValueFromPipeline=$true)]
        [System.Object]
        ${Body},

        [string]
        ${ContentType},

        [ValidateSet('chunked','compress','deflate','gzip','identity')]
        [string]
        ${TransferEncoding},

        [string]
        ${InFile},

        [string]
        ${OutFile},

        [switch]
        ${PassThru},

        # Ignore SSL Errors for this request
        [switch]
        ${Insecure}
    )

    begin {
        if($Insecure) {
            [Huddled.Net.TunableValidator]::ApproveNextRequest()
        }
        $null = $PSBoundParameters.Remove("Insecure")

        try {
            $outBuffer = $null
            if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))
            {
                $PSBoundParameters['OutBuffer'] = 1
            }
            $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Microsoft.PowerShell.Utility\Invoke-WebRequest', [System.Management.Automation.CommandTypes]::Cmdlet)
            $scriptCmd = {& $wrappedCmd @PSBoundParameters }
            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
            $steppablePipeline.Begin($PSCmdlet)
        } catch {
            throw
        }
    }

    process {
        try {
            $steppablePipeline.Process($_)
        } catch {
            throw
        }
    }

    end {
        try {
            $steppablePipeline.End()
        } catch {
            throw
        }
    }
    <#
        .ForwardHelpTargetName Microsoft.PowerShell.Utility\Invoke-WebRequest
        .ForwardHelpCategory Cmdlet
    #>
}


function Invoke-RestMethod {
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=217034')]
    param(
        [Microsoft.PowerShell.Commands.WebRequestMethod]
        ${Method},

        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [uri]
        ${Uri},

        [Microsoft.PowerShell.Commands.WebRequestSession]
        ${WebSession},

        [Alias('SV')]
        [string]
        ${SessionVariable},

        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        ${Credential},

        [switch]
        ${UseDefaultCredentials},

        [ValidateNotNullOrEmpty()]
        [string]
        ${CertificateThumbprint},

        [ValidateNotNull()]
        [System.Security.Cryptography.X509Certificates.X509Certificate]
        ${Certificate},

        [string]
        ${UserAgent},

        [switch]
        ${DisableKeepAlive},

        [int]
        ${TimeoutSec},

        [System.Collections.IDictionary]
        ${Headers},

        [ValidateRange(0, 2147483647)]
        [int]
        ${MaximumRedirection},

        [uri]
        ${Proxy},

        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        ${ProxyCredential},

        [switch]
        ${ProxyUseDefaultCredentials},

        [Parameter(ValueFromPipeline=$true)]
        [System.Object]
        ${Body},

        [string]
        ${ContentType},

        [ValidateSet('chunked','compress','deflate','gzip','identity')]
        [string]
        ${TransferEncoding},

        [string]
        ${InFile},

        [string]
        ${OutFile},

        [switch]
        ${PassThru},

        # Ignore SSL Errors for this request
        [switch]
        ${Insecure}
    )

    begin {
        if($Insecure) {
            [Huddled.Net.TunableValidator]::ApproveNextRequest()
        }
        $null = $PSBoundParameters.Remove("Insecure")

        try {
            $outBuffer = $null
            if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))
            {
                $PSBoundParameters['OutBuffer'] = 1
            }
            $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Microsoft.PowerShell.Utility\Invoke-RestMethod', [System.Management.Automation.CommandTypes]::Cmdlet)
            $scriptCmd = {& $wrappedCmd @PSBoundParameters }
            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
            $steppablePipeline.Begin($PSCmdlet)
        } catch {
            throw
        }
    }

    process {
        try {
            $steppablePipeline.Process($_)
        } catch {
            throw
        }
    }

    end {
        try {
            $steppablePipeline.End()
        } catch {
            throw
        }
    }
    <#
        .ForwardHelpTargetName Microsoft.PowerShell.Utility\Invoke-RestMethod
        .ForwardHelpCategory Cmdlet
    #>
}

if(Get-Command Export-ODataEndpointProxy -ErrorAction SilentlyContinue) {

function Export-ODataEndpointProxy {
    [CmdletBinding(DefaultParameterSetName='CDXML')]
    param(
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
        [string]
        ${Uri},

        [Parameter(Mandatory=$true, Position=1, ValueFromPipelineByPropertyName=$true)]
        [string]
        ${OutputPath},

        [Parameter(Position=2, ValueFromPipelineByPropertyName=$true)]
        [string]
        ${MetadataUri},

        [Parameter(Position=3, ValueFromPipelineByPropertyName=$true)]
        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        ${Credential},

        [switch]
        ${Insecure}
    )

    begin {
        if($Insecure) {
            [Huddled.Net.TunableValidator]::ApproveNextRequest()
        }
        $null = $PSBoundParameters.Remove("Insecure")

        try {
            $outBuffer = $null
            if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))
            {
                $PSBoundParameters['OutBuffer'] = 1
            }
            $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Microsoft.PowerShell.ODataUtils\Export-ODataEndpointProxy', [System.Management.Automation.CommandTypes]::Function)
            $scriptCmd = {& $wrappedCmd @PSBoundParameters }
            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
            $steppablePipeline.Begin($PSCmdlet)
        } catch {
            throw
        }
    }

    process {
        try {
            $steppablePipeline.Process($_)
        } catch {
            throw
        }
    }

    end {
        try {
            $steppablePipeline.End()
        } catch {
            throw
        }
    }
    <#
        .ForwardHelpTargetName Microsoft.PowerShell.ODataUtils\Export-ODataEndpointProxy
        .ForwardHelpCategory Function
    #>
}

}

# Should add a module unload hook to remove the validation hook
# [Net.ServicePointManager]::ServerCertificateValidationCallback = $null
