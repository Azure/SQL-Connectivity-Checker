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
            Write-Output "WARNING: Current environment doesn't support multiple DNS sources."
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
            Write-Output 'Unsupported'
        }
    }
}

# PowerShell Container Image Support End

function PrintAverageConnectionTime($addressList, $port) {
    Write-Output 'Printing average connection times for 5 connection attempts:' -ForegroundColor Green
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

        Write-Output '  IP Address:'$ipAddress'  Port:'$port'  Successful connections:'$numSuccessful'  Failed connections:'$numFailed'  Average response time:'$avg' ms '
    }
}

function PrintDNSResults($dnsResult, [string] $dnsSource) {
    if ($dnsResult) {
        Write-Output ' Found DNS record in' $dnsSource '(IP Address:'$dnsResult.IPAddress')'
    }
    else {
        Write-Output ' Could not find DNS record in' $dnsSource
    }
}

function ValidateDNS([String] $Server) {
    Try {
        Write-Output 'Validating DNS record for' $Server -ForegroundColor Green

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
        Write-Output "Error at ValidateDNS" -Foreground Red
        Write-Output $_.Exception.Message -ForegroundColor Red
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


if ([string]::IsNullOrEmpty($env:TEMP)) {
    $env:TEMP = '/tmp';
}

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls
    
    if ($Local) {
        $path = $env:TEMP +  "/AzureSQLConnectivityChecker/TDSClient.dll"
        Copy-Item -Path $($LocalPath + '/netstandard2.0/TDSClient.dll') -Destination $path
    }
    else {
        $path = $env:TEMP +  "/AzureSQLConnectivityChecker/TDSClient.dll"
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
    }
    catch {
        [TDSClient.TDS.Utilities.LoggingUtilities]::WriteLog('Failure: ' + $_.Exception.InnerException.Message)
    }
    finally {
        $log.Close()
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
    }

    $path = $env:TEMP + '/AzureSQLConnectivityChecker/ConnectivityPolicyLog.txt'
    $result = $([System.IO.File]::ReadAllText($path))

    Write-Output $result

    $match = [Regex]::Match($result, "Routing to: (.*)\.")
    if ($match.Success) {
        $array = $match.Groups[1].Value -split ':'
        $server = $array[0]
        $port = $array[1]

        Write-Output 'Redirect connectivity policy has been detected, running additional tests:' -ForegroundColor Green
        ValidateDNS $server

        try {
            $dnsResult = [System.Net.DNS]::GetHostEntry($Server)
        }
        catch {
            Write-Output ' ERROR: Name resolution of' $Server 'failed' -ForegroundColor Red
            throw
        }

        $resolvedAddress = $dnsResult.AddressList[0].IPAddressToString

        Write-Output
        PrintAverageConnectionTime $resolvedAddress $port
    }
    else {
        Write-Output ' Proxy connection policy detected!' -ForegroundColor Green
    }
}
catch {
    Write-Output 'Running advanced connectivity policy tests failed!' -ForegroundColor Red
    Write-Output $_.Exception
}