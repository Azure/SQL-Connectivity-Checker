using namespace System
using namespace System.Net
using namespace System.net.Sockets
using namespace System.Collections.Generic
using namespace System.Diagnostics

# PowerShell Container Image Support Start

if (!$(Get-Command 'Test-NetConnection' -errorAction SilentlyContinue)) {
    function Test-NetConnection {
        param(
            [Parameter(Position = 0, Mandatory = $true)] $HostName,
            [Parameter(Mandatory = $true)] $Port
        );
        process {
            $client = [TcpClient]::new()

            try {
                $client.Connect($HostName, $Port)
                $result = @{TcpTestSucceeded = $true; InterfaceAlias = 'Unsupported' }
            }
            catch {
                $result = @{TcpTestSucceeded = $false; InterfaceAlias = 'Unsupported' }
            }

            $client.Dispose()
            return $result
        }
    }
}

if (!$(Get-Command 'Resolve-DnsName' -errorAction SilentlyContinue)) {
    function Resolve-DnsName {
        param(
            [Parameter(Position = 0)] $Name,
            [Parameter()] $Server,
            [switch] $CacheOnly,
            [switch] $DnsOnly,
            [switch] $NoHostsFile
        );
        process {
            # ToDo: Add support
            Write-Host "WARNING: Current environment doesn't support multiple DNS sources."
            return @{ IPAddress = [Dns]::GetHostAddresses($Name).IPAddressToString };
        }
    }
}

if (!$(Get-Command 'Get-NetAdapter' -errorAction SilentlyContinue)) {
    function Get-NetAdapter {
        param(
            [Parameter(Position = 0, Mandatory = $true)] $HostName,
            [Parameter(Mandatory = $true)] $Port
        );
        process {
            Write-Host 'Unsupported'
        }
    }
}

# PowerShell Container Image Support End

function PrintAverageConnectionTime($addressList, $port) {
    Write-Host 'Printing average connection times for 5 connection attempts:' -ForegroundColor Green
    $stopwatch = [StopWatch]::new()

    foreach ($ipAddress in $addressList) {
        [double]$sum = 0
        [int]$numFailed = 0
        [int]$numSuccessful = 0

        for ($i = 0; $i -lt 5; $i++) {
            $client = [TcpClient]::new()
            try {
                $stopwatch.Restart()
                $client.Connect($ipAddress, $port)
                $stopwatch.Stop()

                $sum += $stopwatch.ElapsedMilliseconds

                $numSuccessful++
            }
            catch {
                $numFailed++
            }
            $client.Dispose()
        }

        $avg = 0
        if ($numSuccessful -ne 0) {
            $avg = $sum / $numSuccessful
        }

        Write-Host '  IP Address:'$ipAddress'  Port:'$port
        Write-Host '  Successful connections:'$numSuccessful
        Write-Host '  Failed connections:'$numFailed
        Write-Host '  Average response time:'$avg' ms'
    }
}

function PrintDNSResults($dnsResult, [string] $dnsSource) {
    if ($dnsResult) {
        Write-Host ' Found DNS record in' $dnsSource '(IP Address:'$dnsResult.IPAddress')'
    }
    else {
        Write-Host ' Could not find DNS record in' $dnsSource
    }
}

function ValidateDNS([String] $Server) {
    Try {
        Write-Host 'Validating DNS record for' $Server -ForegroundColor Green

        $DNSfromHosts = Resolve-DnsName -Name $Server -CacheOnly -ErrorAction SilentlyContinue
        PrintDNSResults $DNSfromHosts 'hosts file'

        $DNSfromCache = Resolve-DnsName -Name $Server -NoHostsFile -CacheOnly -ErrorAction SilentlyContinue
        PrintDNSResults $DNSfromCache 'cache'

        $DNSfromCustomerServer = Resolve-DnsName -Name $Server -DnsOnly -ErrorAction SilentlyContinue
        PrintDNSResults $DNSfromCustomerServer 'DNS server'

        $DNSfromAzureDNS = Resolve-DnsName -Name $Server -DnsOnly -Server 208.67.222.222 -ErrorAction SilentlyContinue
        PrintDNSResults $DNSfromAzureDNS 'Open DNS'
    }
    Catch {
        Write-Host "Error at ValidateDNS" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

function TrackWarningAnonymously ([String] $warningCode) {
    Try {
        if ($SendAnonymousUsageData) {

            if ((Get-Host).Version.Major -le 5) {
                #Despite computername and username will be used to calculate a hash string, this will keep you anonymous but allow us to identify multiple runs from the same user
                $StringBuilderHash = New-Object System.Text.StringBuilder
                [System.Security.Cryptography.HashAlgorithm]::Create("MD5").ComputeHash([System.Text.Encoding]::UTF8.GetBytes($env:computername + $env:username)) | ForEach-Object {
                    [Void]$StringBuilderHash.Append($_.ToString("x2"))
                }
                $AnonymousRunId = $StringBuilderHash.ToString()
            }

            $body = New-Object PSObject `
            | Add-Member -PassThru NoteProperty name 'Microsoft.ApplicationInsights.Event' `
            | Add-Member -PassThru NoteProperty time $([System.dateTime]::UtcNow.ToString('o')) `
            | Add-Member -PassThru NoteProperty iKey "a75c333b-14cb-4906-aab1-036b31f0ce8a" `
            | Add-Member -PassThru NoteProperty tags (New-Object PSObject | Add-Member -PassThru NoteProperty 'ai.user.id' $AnonymousRunId) `
            | Add-Member -PassThru NoteProperty data (New-Object PSObject `
                | Add-Member -PassThru NoteProperty baseType 'EventData' `
                | Add-Member -PassThru NoteProperty baseData (New-Object PSObject `
                    | Add-Member -PassThru NoteProperty ver 2 `
                    | Add-Member -PassThru NoteProperty name $warningCode));
            $body = $body | ConvertTo-JSON -depth 5;
            Invoke-WebRequest -Uri 'https://dc.services.visualstudio.com/v2/track' -Method 'POST' -UseBasicParsing -body $body > $null
        }
    }
    Catch {
        Write-Host 'TrackWarningAnonymously exception:'
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

$parameters = $args[0]
$Server = $parameters['Server']
$Port = $parameters['Port']
$User = $parameters['User']
$Password = $parameters['Password']
$Database = $parameters['Database']
$EncryptionProtocol = $parameters['EncryptionProtocol']
$RepositoryBranch = $parameters['RepositoryBranch']
$Local = $parameters['Local']
$LocalPath = $parameters['LocalPath']
$SendAnonymousUsageData = $parameters['SendAnonymousUsageData']
$AnonymousRunId = $parameters['AnonymousRunId']

if ([string]::IsNullOrEmpty($env:TEMP)) {
    $env:TEMP = '/tmp';
}

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls

    if ($Local) {
        $path = $env:TEMP + "/AzureSQLConnectivityChecker/TDSClient.dll"
        Copy-Item -Path $($LocalPath + '/netstandard2.0/TDSClient.dll') -Destination $path
    }
    else {
        $path = $env:TEMP + "/AzureSQLConnectivityChecker/TDSClient.dll"
        Invoke-WebRequest -Uri $('https://github.com/Azure/SQL-Connectivity-Checker/raw/' + $RepositoryBranch + '/netstandard2.0/TDSClient.dll') -OutFile $path -UseBasicParsing
    }

    $path = $env:TEMP + "/AzureSQLConnectivityChecker/TDSClient.dll"
    $assembly = [System.IO.File]::ReadAllBytes($path)
    [System.Reflection.Assembly]::Load($assembly) | Out-Null

    $log = [System.IO.File]::CreateText($env:TEMP + '/AzureSQLConnectivityChecker/ConnectivityPolicyLog.txt')
    [TDSClient.TDS.Utilities.LoggingUtilities]::SetVerboseLog($log)
    try {
        switch ($EncryptionProtocol) {
            'Tls 1.0' {
                $encryption = [System.Security.Authentication.SslProtocols]::Tls
                break
            }
            'Tls 1.1' {
                $encryption = [System.Security.Authentication.SslProtocols]::Tls11
                break
            }
            'Tls 1.2' {
                $encryption = [System.Security.Authentication.SslProtocols]::Tls12
                break
            }
            # Not supported
            #'Tls 1.3' {
            #    $encryption = [System.Security.Authentication.SslProtocols]::Tls13
            #    break
            #}
            default {
                # Allow the operating system to choose the best protocol to use
                $encryption = [System.Security.Authentication.SslProtocols]::Tls12 -bor [System.Security.Authentication.SslProtocols]::Tls11 -bor [System.Security.Authentication.SslProtocols]::Default
            }
        }
        $tdsClient = [TDSClient.TDS.Client.TDSSQLTestClient]::new($Server, $Port, $User, $Password, $Database, $encryption)
        $tdsClient.Connect()
        $tdsClient.Disconnect()
        TrackWarningAnonymously ('Advanced|TDSClient|ConnectAndDisconnect')
    }
    catch {
        [TDSClient.TDS.Utilities.LoggingUtilities]::WriteLog('Failure: ' + $_.Exception.InnerException.Message)
        TrackWarningAnonymously ('Advanced|TDSClient|Exception|' + $_.Exception.InnerException.Message)
    }
    finally {
        $log.Close()
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
    }

    $path = $env:TEMP + '/AzureSQLConnectivityChecker/ConnectivityPolicyLog.txt'
    $result = $([System.IO.File]::ReadAllText($path))
    Write-Host $result

    $match = [Regex]::Match($result, "Routing to: (.*)\.")
    if ($match.Success) {
        $array = $match.Groups[1].Value -split ':'
        $server = $array[0]
        $port = $array[1]

        Write-Host 'Redirect connectivity policy has been detected, running additional tests:' -ForegroundColor Green
        TrackWarningAnonymously 'Advanced|Redirect|Detected'

        ValidateDNS $server

        try {
            $dnsResult = [System.Net.DNS]::GetHostEntry($server)
        }
        catch {
            $msg = ' ERROR: Name resolution (DNS) of ' + $server + ' failed'
            Write-Host $msg -Foreground Red

            $Advanced_DNSResolutionFailed = ' Please make sure the name ' + $server + ' can be resolved (DNS)
 Failure to resolve specific domain names is usually a client-side networking issue that you will need to pursue with your local network administrator.'
            Write-Host $Advanced_DNSResolutionFailed -Foreground Red
            TrackWarningAnonymously 'Advanced|Redirect|DNSResolutionFailedForRedirect'
            return
        }

        $resolvedAddress = $dnsResult.AddressList[0].IPAddressToString
        Write-Host
        PrintAverageConnectionTime $resolvedAddress $port
    }
    else {
        Write-Host ' Proxy connection policy detected!' -ForegroundColor Green
        TrackWarningAnonymously 'Advanced|Proxy|Detected'
    }
}
catch {
    Write-Host 'Running advanced connectivity policy tests failed!' -ForegroundColor Red
    Write-Host $_.Exception
    TrackWarningAnonymously ('Advanced|Exception|' + $_.Exception.Message)
}