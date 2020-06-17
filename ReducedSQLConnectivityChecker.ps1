# Parameter region when Invoke-Command -ScriptBlock is used
$parameters = $args[0]
if ($null -ne $parameters) {
    $Server = $parameters['Server']
    $Database = $parameters['Database']
    $User = $parameters['User']
    $Password = $parameters['Password']
    $EncryptionProtocol = $parameters['EncryptionProtocol']
    if ($null -ne $parameters['RepositoryBranch']) {
        $RepositoryBranch = $parameters['RepositoryBranch']
    }
}

$Server = $Server.Trim()
$Server = $Server.Replace('tcp:', '')
$Server = $Server.Replace(',1433', '')
$Server = $Server.Replace(',3342', '')
$Server = $Server.Replace(';', '')

if ($null -eq $User -or '' -eq $User) {
    $User = 'AzSQLConnCheckerUser'
}

if ($null -eq $Password -or '' -eq $Password) {
    $Password = 'AzSQLConnCheckerPassword'
}

if ($null -eq $Database -or '' -eq $Database) {
    $Database = 'master'
}

if ($null -eq $Local) {
    $Local = $false
}

if ($null -eq $RepositoryBranch) {
    $RepositoryBranch = 'master'
}

function IsManagedInstancePublicEndpoint([String] $Server) {
    return [bool]((IsManagedInstance $Server) -and ($Server -match '.public.'))
}

if ([string]::IsNullOrEmpty($env:TEMP)) {
    $env:TEMP = '/tmp';
}

try {
    Write-Output '******************************************'
    Write-Output '      Azure SQL Connectivity Checker      '
    Write-Output '******************************************'
    Write-Output 'WARNING: Reduced version of Azure SQL Connectivity Checker is running due to environment nature/limitations.'
    Write-Output 'WARNING: This version does not create any output files, please copy the output directly from the console.'
    
    if (!$Server -or $Server.Length -eq 0) {
        Write-Output 'The $Server parameter is empty'
        Write-Output 'Please see more details about how to use this tool at https://github.com/Azure/SQL-Connectivity-Checker'
        Write-Output
        throw
    }
    
    if (!$Server.EndsWith('.database.windows.net') `
            -and !$Server.EndsWith('.database.cloudapi.de') `
            -and !$Server.EndsWith('.database.chinacloudapi.cn') `
            -and !$Server.EndsWith('.sql.azuresynapse.net')) {
        $Server = $Server + '.database.windows.net'
    }
    
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls
    $path = $env:TEMP + "/TDSClient.dll"
    
    if (Test-Path $path) {
        Remove-Item $path
    }
    
    Invoke-WebRequest -Uri $('https://github.com/Azure/SQL-Connectivity-Checker/raw/' + $RepositoryBranch + '/netstandard2.0/TDSClient.dll') -OutFile $path -UseBasicParsing
    
    $path = $env:TEMP + "/TDSClient.dll"
    $assembly = [System.IO.File]::ReadAllBytes($path)
    [System.Reflection.Assembly]::Load($assembly) | Out-Null
    
    [TDSClient.TDS.Utilities.LoggingUtilities]::SetVerboseLog([Console]::Out)
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
    
        if (IsManagedInstancePublicEndpoint $Server) {
            $Port = 3342
        } else {
            $Port = 1433
        }
    
        $tdsClient = [TDSClient.TDS.Client.TDSSQLTestClient]::new($Server, $Port, $User, $Password, $Database, $encryption)
        $tdsClient.Connect()
        $tdsClient.Disconnect()
    }
    catch {
        [TDSClient.TDS.Utilities.LoggingUtilities]::WriteLog('Failure: ' + $_.Exception.InnerException.Message)
    }
    finally {
        [TDSClient.TDS.Utilities.LoggingUtilities]::ClearVerboseLog()
    }
} catch {

} finally {
    Remove-Item $path
}