using namespace System
using namespace System.Net
using namespace System.net.Sockets
using namespace System.Collections.Generic
using namespace System.Diagnostics

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

        Write-Host '  IP Address:'$ipAddress'  Port:'$port'  Successful connections:'$numSuccessful'  Failed connections:'$numFailed'  Average response time:'$avg' ms '
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

$parameters = $args[0]
$Server = $parameters['Server']
$Port = $parameters['Port']
$User = $parameters['User']
$Password = $parameters['Password']
$Database = $parameters['Database']
$EncryptionProtocol = $parameters['EncryptionProtocol']

try {
    #ToDo change branch to master once this is merged into master
    Invoke-WebRequest -Uri 'https://github.com/Azure/SQL-Connectivity-Checker/raw/pr/2/TDSClient.dll' -OutFile "$env:TEMP\AzureSQLConnectivityChecker\TDSClient.dll"

    $assembly = [System.IO.File]::ReadAllBytes("$env:TEMP\AzureSQLConnectivityChecker\TDSClient.dll")
    [System.Reflection.Assembly]::Load($assembly) | Out-Null

    $log = [System.IO.File]::CreateText($env:TEMP + '\AzureSQLConnectivityChecker\ConnectivityPolicyLog.txt')
    [TDSClient.TDS.Utilities.LoggingUtilities]::SetVerboseLog($log)
    try {
        switch($EncryptionProtocol) {
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
    } catch {
        [TDSClient.TDS.Utilities.LoggingUtilities]::WriteLog('Failure: ' + $_.Exception.InnerException.Message)
    } finally {
        $log.Close()
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
    }

    $result = $([System.IO.File]::ReadAllText($env:TEMP + '\AzureSQLConnectivityChecker\ConnectivityPolicyLog.txt'))

    Write-Host $result

    $match = [Regex]::Match($result, "Routing to: (.*)\.")
    if ($match.Success) {
        $array = $match.Groups[1].Value -split ':'
        $server = $array[0]
        $port = $array[1]

        Write-Host 'Redirect connectivity policy has been detected, running additional tests:' -ForegroundColor Green
        ValidateDNS $server

        try {
            $dnsResult = [System.Net.DNS]::GetHostEntry($Server)
        }
        catch {
            Write-Host ' ERROR: Name resolution of' $Server 'failed' -ForegroundColor Red
            throw
        }

        $resolvedAddress = $dnsResult.AddressList[0].IPAddressToString

        Write-Host
        PrintAverageConnectionTime $resolvedAddress $port
    } else {
        Write-Host ' Proxy connection policy detected!' -ForegroundColor Green
    }
} catch {
    Write-Host 'Running advanced connectivity policy tests failed!' -ForegroundColor Red
    Write-Host $_.Exception
}