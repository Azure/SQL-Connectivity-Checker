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

        if (!$(Get-Command 'Resolve-DnsName' -errorAction SilentlyContinue)) {
            Write-Host " WARNING: Current environment doesn't support multiple DNS sources."
            Write-Host ' DNS resolution:' ([Dns]::GetHostAddresses($Name).IPAddressToString)
        }
        else {
            Try {
                $DNSfromHosts = Resolve-DnsName -Name $Server -CacheOnly -ErrorAction SilentlyContinue
                PrintDNSResults $DNSfromHosts 'hosts file'
            }
            Catch {
                Write-Host "Error at ValidateDNS from hosts file" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from hosts file'
            }

            Try {
                $DNSfromCache = Resolve-DnsName -Name $Server -NoHostsFile -CacheOnly -ErrorAction SilentlyContinue
                PrintDNSResults $DNSfromCache 'cache'
            }
            Catch {
                Write-Host "Error at ValidateDNS from cache" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from cache'
            }

            Try {
                $DNSfromCustomerServer = Resolve-DnsName -Name $Server -DnsOnly -ErrorAction SilentlyContinue
                PrintDNSResults $DNSfromCustomerServer 'DNS server'
            }
            Catch {
                Write-Host "Error at ValidateDNS from DNS server" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from DNS server'
            }

            Try {
                $DNSfromOpenDNS = Resolve-DnsName -Name $Server -DnsOnly -Server 208.67.222.222 -ErrorAction SilentlyContinue
                PrintDNSResults $DNSfromOpenDNS 'Open DNS'
            }
            Catch {
                Write-Host "Error at ValidateDNS from Open DNS" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from Open DNS'
            }
        }
    }
    Catch {
        Write-Host "Error at ValidateDNS" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

function TrackWarningAnonymously ([String] $warningCode) {
    Try {
        if ($SendAnonymousUsageData) {
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
$AuthenticationType = $parameters['AuthenticationType']
$User = $parameters['User']
$Password = $parameters['Password']
$Database = $parameters['Database']
$EncryptionProtocol = $parameters['EncryptionProtocol']
$RepositoryBranch = $parameters['RepositoryBranch']
$Local = $parameters['Local']
$LocalPath = $parameters['LocalPath']
$SendAnonymousUsageData = $parameters['SendAnonymousUsageData']
$AnonymousRunId = $parameters['AnonymousRunId']
$logsFolderName = $parameters['logsFolderName']
$outFolderName = $parameters['outFolderName']

$ConnectionAttempts = 1
if ($null -ne $parameters['ConnectionAttempts']) {
    $ConnectionAttempts = $parameters['ConnectionAttempts']
}

$DelayBetweenConnections = 1
if ($null -ne $parameters['DelayBetweenConnections']) {
    $DelayBetweenConnections = $parameters['DelayBetweenConnections']
}


if ([string]::IsNullOrEmpty($env:TEMP)) {
    $env:TEMP = '/tmp';
}

try {
    Set-Location -Path $env:TEMP
    Set-Location $logsFolderName
    Set-Location $outFolderName
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls

    $TDSClientPath = Join-Path ((Get-Location).Path) "TDSClient.dll"
    if ($Local) {
        Copy-Item -Path $($LocalPath + 'net472\TDSClient.dll') -Destination $TDSClientPath
    }
    # else {
    #     Invoke-WebRequest -Uri $('https://github.com/Azure/SQL-Connectivity-Checker/raw/' + $RepositoryBranch + '/net472/TDSClient.dll') -OutFile $TDSClientPath -UseBasicParsing
    # }
    $assembly = [System.IO.File]::ReadAllBytes("D:\Connectivity checker\SQL-Connectivity-Checker\net472\TDSClient.dll")
    [System.Reflection.Assembly]::Load($assembly) | Out-Null

    $fullLogPath = Join-Path ((Get-Location).Path) 'AdvancedTests_FullLog.txt'
    $logPath = Join-Path ((Get-Location).Path) 'AdvancedTests_LastRunLog.txt'
    $summaryLogPath = Join-Path ((Get-Location).Path) 'AdvancedTests_SummaryLog.txt'
    $summaryLog = [System.IO.File]::CreateText($summaryLogPath)

    [TDSClient.TDS.Utilities.LoggingUtilities]::SetSummaryLog($summaryLog)

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
        $tdsClient = [TDSClient.TDS.Client.TDSSQLTestClient]::new($Server, $Port, $AuthenticationType, $User, $Password, $Database, $encryption)

        for ($i = 1; $i -le $ConnectionAttempts; ++$i) {
            $log = [System.IO.File]::CreateText($logPath)
            [TDSClient.TDS.Utilities.LoggingUtilities]::SetVerboseLog($log)

            $result = $tdsClient.Connect().GetAwaiter().GetResult()
            $tdsClient.Disconnect()

            $log.Close()
            [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
            $result = $([System.IO.File]::ReadAllText($logPath))
            Write-Host $result
            Add-Content -Path $fullLogPath -Value $result

            if ($i -lt $ConnectionAttempts) {
                Write-Host ('Waiting ' + $DelayBetweenConnections + ' second(s)...')
                Start-Sleep -Seconds $DelayBetweenConnections
            }
        }
        TrackWarningAnonymously ('Advanced|TDSClient|ConnectAndDisconnect')
    }
    catch {
        [TDSClient.TDS.Utilities.LoggingUtilities]::WriteLog('Failure: ' + $_.Exception.InnerException.Message)
        TrackWarningAnonymously ('Advanced|TDSClient|Exception|' + $_.Exception.InnerException.Message)
    }
    finally {
        $log.Close()
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
        $summaryLog.Close()
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearSummaryLog()
        Remove-Item $TDSClientPath -Force
    }

    $result = $([System.IO.File]::ReadAllText($logPath))
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