## Copyright (c) Microsoft Corporation.
#Licensed under the MIT license.

#Azure SQL Connectivity Checker

#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
#WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using namespace System
using namespace System.Net
using namespace System.net.Sockets
using namespace System.Collections.Generic
using namespace System.Diagnostics

# Parameter region for when script is run directly
# Supports Single, Elastic Pools and Managed Instance (please provide FQDN, MI public endpoint is supported)
# Supports Azure Synapse / Azure SQL Data Warehouse (*.sql.azuresynapse.net / *.database.windows.net)
# Supports Public Cloud (*.database.windows.net), Azure China (*.database.chinacloudapi.cn), Azure Germany (*.database.cloudapi.de) and Azure Government (*.database.usgovcloudapi.net)
$Server = '.database.windows.net' # or any other supported FQDN
$Database = ''  # Set the name of the database you wish to test, 'master' will be used by default if nothing is set
$User = ''  # Set the login username you wish to use, 'AzSQLConnCheckerUser' will be used by default if nothing is set
$Password = ''  # Set the login password you wish to use, 'AzSQLConnCheckerPassword' will be used by default if nothing is set
# In case you want to hide the password (like during a remote session), uncomment the 2 lines below (by removing leading #) and password will be asked during execution
# $Credentials = Get-Credential -Message "Credentials to test connections to the database (optional)" -User $User
# $Password = $Credentials.GetNetworkCredential().password

# Optional parameters (default values will be used if omitted)
$SendAnonymousUsageData = $true  # Set as $true (default) or $false
$RunAdvancedConnectivityPolicyTests = $true  # Set as $true (default) or $false#Set as $true (default) or $false, this will download library needed for running advanced connectivity policy tests
$CollectNetworkTrace = $true  # Set as $true (default) or $false
#EncryptionProtocol = ''  # Supported values: 'Tls 1.0', 'Tls 1.1', 'Tls 1.2'; Without this parameter operating system will choose the best protocol to use

# Parameter region when Invoke-Command -ScriptBlock is used
$parameters = $args[0]
if ($null -ne $parameters) {
    $Server = $parameters['Server']
    $Database = $parameters['Database']
    $User = $parameters['User']
    $Password = $parameters['Password']
    if ($null -ne $parameters['SendAnonymousUsageData']) {
        $SendAnonymousUsageData = $parameters['SendAnonymousUsageData']
    }
    if ($null -ne $parameters['RunAdvancedConnectivityPolicyTests']) {
        $RunAdvancedConnectivityPolicyTests = $parameters['RunAdvancedConnectivityPolicyTests']
    }
    if ($null -ne $parameters['CollectNetworkTrace']) {
        $CollectNetworkTrace = $parameters['CollectNetworkTrace']
    }
    $EncryptionProtocol = $parameters['EncryptionProtocol']
    if ($null -ne $parameters['Local']) {
        $Local = $parameters['Local']
    }
    if ($null -ne $parameters['LocalPath']) {
        $LocalPath = $parameters['LocalPath']
    }
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

$CustomerRunningInElevatedMode = $false
if ($PSVersionTable.Platform -eq 'Unix') {
    if ((id -u) -eq 0) {
        $CustomerRunningInElevatedMode = $true
    }
}
else {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if ($currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        $CustomerRunningInElevatedMode = $true
    }
}

$SQLDBGateways = @(
    New-Object PSObject -Property @{Region = "Australia Central"; Gateways = ("20.36.105.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'australiacentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Australia Central2"; Gateways = ("20.36.113.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'australiacentral2-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Australia East"; Gateways = ("13.75.149.87", "40.79.161.1", "13.70.112.9"); Affected20191014 = $false; TRs = ('tr2', 'tr3', 'tr4'); Cluster = 'australiaeast1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Australia South East"; Gateways = ("13.73.109.251", "13.77.48.10", "191.239.192.109"); Affected20191014 = $false; TRs = ('tr2', 'tr3', 'tr4'); Cluster = 'australiasoutheast1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Brazil South"; Gateways = ("104.41.11.5", "191.233.200.14"); Affected20191014 = $true; TRs = ('tr11', 'tr12', 'tr15'); Cluster = 'brazilsouth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Canada Central"; Gateways = ("40.85.224.249", "52.246.152.0", "20.38.144.1"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'canadacentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Canada East"; Gateways = ("40.86.226.166", "52.242.30.154"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'canadaeast1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Central US"; Gateways = ("23.99.160.139", "13.67.215.62", "52.182.137.15", "104.208.21.1", "104.208.16.96"); Affected20191014 = $true; TRs = ('tr4', 'tr8', 'tr9'); Cluster = 'centralus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "China East"; Gateways = ("139.219.130.35"); Affected20191014 = $false; TRs = ('tr2', 'tr3'); Cluster = 'chinaeast1-a.worker.database.chinacloudapi.cn'; }
    New-Object PSObject -Property @{Region = "China East 2"; Gateways = ("40.73.82.1"); Affected20191014 = $false; TRs = ('tr1', 'tr5', 'tr11'); Cluster = 'chinaeast2-a.worker.database.chinacloudapi.cn'; }
    New-Object PSObject -Property @{Region = "China North"; Gateways = ("139.219.15.17"); Affected20191014 = $false; TRs = ('tr2', 'tr3'); Cluster = 'chinanorth1-a.worker.database.chinacloudapi.cn'; }
    New-Object PSObject -Property @{Region = "China North 2"; Gateways = ("40.73.50.0"); Affected20191014 = $false; TRs = ('tr1', 'tr67', 'tr119'); Cluster = 'chinanorth2-a.worker.database.chinacloudapi.cn'; }
    New-Object PSObject -Property @{Region = "East Asia"; Gateways = ("191.234.2.139", "52.175.33.150", "13.75.32.4"); Affected20191014 = $true; TRs = ('tr4', 'tr8', 'tr9'); Cluster = 'eastasia1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "East US"; Gateways = ("191.238.6.43", "40.121.158.30", "40.79.153.12", "40.78.225.32"); Affected20191014 = $true; TRs = ('tr7', 'tr8', 'tr9'); Cluster = 'eastus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "East US 2"; Gateways = ("191.239.224.107", "40.79.84.180", "52.177.185.181", "52.167.104.0", "104.208.150.3"); Affected20191014 = $true; TRs = ('tr10', 'tr8', 'tr9'); Cluster = 'eastus2-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "France Central"; Gateways = ("40.79.137.0", "40.79.129.1"); Affected20191014 = $false; TRs = ('tr1', 'tr7', 'tr8'); Cluster = 'francecentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Germany Central"; Gateways = ("51.4.144.100"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'germanycentral1-a.worker.database.cloudapi.de'; }
    New-Object PSObject -Property @{Region = "Germany North East"; Gateways = ("51.5.144.179"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'germanynortheast1-a.worker.database.cloudapi.de'; }
    New-Object PSObject -Property @{Region = "Germany North"; Gateways = ("51.116.56.0"); Affected20191014 = $false; TRs = ('tr1', 'tr3', 'tr4'); Cluster = 'germanynorth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Germany West Central"; Gateways = ("51.116.152.0", "51.116.240.0", "51.116.248.0"); Affected20191014 = $false; TRs = ('tr1', 'tr3', 'tr4'); Cluster = 'germanywestcentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "India Central"; Gateways = ("104.211.96.159"); Affected20191014 = $false; TRs = ('tr1', 'tr3', 'tr16'); Cluster = 'indiacentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "India South"; Gateways = ("104.211.224.146"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr5'); Cluster = 'indiasouth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "India West"; Gateways = ("104.211.160.80"); Affected20191014 = $false; TRs = ('tr41', 'tr42', 'tr54'); Cluster = 'indiawest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Japan East"; Gateways = ("191.237.240.43", "13.78.61.196", "40.79.184.8", "40.79.192.5", "13.78.106.224"); Affected20191014 = $true; TRs = ('tr4', 'tr5', 'tr9'); Cluster = 'japaneast1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Japan West"; Gateways = ("191.238.68.11", "104.214.148.156", "40.74.97.10", "40.74.100.192"); Affected20191014 = $true; TRs = ('tr11', 'tr12', 'tr13'); Cluster = 'japanwest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Korea Central"; Gateways = ("52.231.32.42"); Affected20191014 = $false; TRs = ('tr1', 'tr10', 'tr118'); Cluster = 'koreacentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Korea South"; Gateways = ("52.231.200.86"); Affected20191014 = $false; TRs = ('tr1', 'tr3', 'tr75'); Cluster = 'koreasouth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "North Central US"; Gateways = ("23.98.55.75", "23.96.178.199", "52.162.104.33"); Affected20191014 = $true; TRs = ('tr7', 'tr8', 'tr9'); Cluster = 'northcentralus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "North Europe"; Gateways = ("191.235.193.75", "40.113.93.91", "52.138.224.1", "13.74.104.113"); Affected20191014 = $true; TRs = ('tr7', 'tr8', 'tr9'); Cluster = 'northeurope1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Norway East"; Gateways = ("51.120.96.0"); Affected20191014 = $false; TRs = ('tr1', 'tr45', 'tr14'); Cluster = 'norwayeast1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Norway West"; Gateways = ("51.120.216.0"); Affected20191014 = $false; TRs = ('tr1', 'tr17', 'tr14'); Cluster = 'norwaywest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "South Africa North"; Gateways = ("102.133.152.0", "102.133.120.2"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr4'); Cluster = 'southafricanorth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "South Africa West"; Gateways = ("102.133.24.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'southafricawest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "South Central US"; Gateways = ("23.98.162.75", "13.66.62.124", "104.214.16.32", "20.45.121.1", "20.49.88.1"); Affected20191014 = $true; TRs = ('tr10', 'tr8', 'tr9'); Cluster = 'southcentralus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "South East Asia"; Gateways = ("23.100.117.95", "104.43.15.0", "40.78.232.3"); Affected20191014 = $true; TRs = ('tr7', 'tr8', 'tr4'); Cluster = 'southeastasia1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Switzerland North"; Gateways = ("51.107.56.0", "51.107.57.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr54'); Cluster = 'switzerlandnorth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "Switzerland West"; Gateways = ("51.107.152.0", "51.107.153.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr52'); Cluster = 'switzerlandwest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "UAE Central"; Gateways = ("20.37.72.64"); Affected20191014 = $false; TRs = ('tr1', 'tr4'); Cluster = 'uaecentral1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "UAE North"; Gateways = ("65.52.248.0"); Affected20191014 = $false; TRs = ('tr1', 'tr4', 'tr9'); Cluster = 'uaenorth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "UK South"; Gateways = ("51.140.184.11", "51.105.64.0"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'uksouth1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "UK West"; Gateways = ("51.141.8.11"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr4'); Cluster = 'ukwest1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "West Central US"; Gateways = ("13.78.145.25", "13.78.248.43"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'westcentralus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "West Europe"; Gateways = ("191.237.232.75", "40.68.37.158", "104.40.168.105", "52.236.184.163"); Affected20191014 = $true; TRs = ('tr7', 'tr8', 'tr9'); Cluster = 'westeurope1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "West US"; Gateways = ("23.99.34.75", "104.42.238.205", "13.86.216.196"); Affected20191014 = $true; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'westus1-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "West US 2"; Gateways = ("13.66.226.202", "40.78.240.8", "40.78.248.10"); Affected20191014 = $false; TRs = ('tr1', 'tr2', 'tr3'); Cluster = 'westus2-a.worker.database.windows.net'; }
    New-Object PSObject -Property @{Region = "US DoD East"; Gateways = ("52.181.160.27"); TRs = ('tr3', 'tr4', 'tr5'); Cluster = 'usdodeast1-a.worker.database.usgovcloudapi.net'; }
    New-Object PSObject -Property @{Region = "US DoD Central"; Gateways = ("52.182.88.34"); TRs = ('tr1', 'tr4', 'tr7'); Cluster = 'usdodcentral1-a.worker.database.usgovcloudapi.net'; }
    New-Object PSObject -Property @{Region = "US Gov Iowa"; Gateways = ("13.72.189.52"); TRs = ('tr1'); Cluster = 'usgovcentral1-a.worker.database.usgovcloudapi.net'; }
    New-Object PSObject -Property @{Region = "US Gov Texas"; Gateways = ("52.238.116.32"); TRs = ('tr1', 'tr2', 'tr29'); Cluster = 'usgovsouthcentral1-a.worker.database.usgovcloudapi.net'; }
    New-Object PSObject -Property @{Region = "US Gov Arizona"; Gateways = ("52.244.48.33"); TRs = ('tr1', 'tr4', 'tr13'); Cluster = 'usgovsouthwest1-a.worker.database.usgovcloudapi.net'; }
    New-Object PSObject -Property @{Region = "US Gov Virginia"; Gateways = ("13.72.48.140"); TRs = ('tr1', 'tr3', 'tr5'); Cluster = 'usgoveast1-a.worker.database.usgovcloudapi.net'; }
)

$TRPorts = @('11000', '11001', '11003', '11005', '11006')
$summaryLog = New-Object -TypeName "System.Text.StringBuilder"
$summaryRecommendedAction = New-Object -TypeName "System.Text.StringBuilder"

# Error Messages
$DNSResolutionFailed = ' Please make sure the server name FQDN is correct and that your machine can resolve it.
 Failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.'

$DNSResolutionFailedSQLMIPublicEndpoint = ' Please make sure the server name FQDN is correct and that your machine can resolve it.
 You seem to be trying to connect using Public Endpoint, this error can be caused if the Public Endpoint is Disabled.
 See how to enable public endpoint for your managed instance at https://aka.ms/mimanage-publicendpoint
 If public endpoint is enabled, failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.'

$SQLDB_InvalidGatewayIPAddress = ' Please make sure the server name FQDN is correct and that your machine can resolve it to a valid gateway IP address (DNS configuration).
 Failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.
 See the valid gateway addresses at https://docs.microsoft.com/azure/azure-sql/database/connectivity-architecture#gateway-ip-addresses'

$SQLDB_GatewayTestFailed = ' Failure to reach the Gateway is usually a client-side networking issue that you will need to pursue with your local network administrator.
 See more about connectivity architecture at https://docs.microsoft.com/azure/azure-sql/database/connectivity-architecture'

$SQLDB_Redirect = " Servers in SQL Database and Azure Synapse support Redirect, Proxy or Default for the server's connection policy setting:

 Default: This is the connection policy in effect on all servers after creation unless you explicitly alter the connection policy to either Proxy or Redirect.
  The default policy is Redirect for all client connections originating inside of Azure (for example, from an Azure Virtual Machine)
  and Proxy for all client connections originating outside (for example, connections from your local workstation).

 Redirect (recommended): Clients establish connections directly to the node hosting the database, leading to reduced latency and improved throughput.
  For connections to use this mode, clients need to:
  - Allow outbound communication from the client to all Azure SQL IP addresses in the region on ports in the range of 11000-11999.
  - Allow outbound communication from the client to Azure SQL Database gateway IP addresses on port 1433.

 Proxy: In this mode, all connections are proxied via the Azure SQL Database gateways, leading to increased latency and reduced throughput.
  For connections to use this mode, clients need to allow outbound communication from the client to Azure SQL Database gateway IP addresses on port 1433.

 If you are using Proxy, the Redirect Policy related tests would not be a problem.
 If you are using Redirect, failure to reach ports in the range of 11000-11999 is usually a client-side networking issue that you will need to pursue with your local network administrator.
 Please check more about connection policies at https://docs.microsoft.com/en-us/azure/azure-sql/database/connectivity-architecture#connection-policy"

$SQLMI_GatewayTestFailed = ' Failure to reach the Gateway is usually a client-side networking issue that you will need to pursue with your local network administrator.
 See more about connectivity architecture at https://docs.microsoft.com/azure/azure-sql/managed-instance/connectivity-architecture-overview'

$SQLMI_PublicEndPoint_GatewayTestFailed = ' This usually indicates a client-side networking issue that you will need to pursue with your local network administrator.
 See more about connectivity using Public Endpoint at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/public-endpoint-configure'

$AAD_login_windows_net = ' If you are using AAD Password or AAD Integrated Authentication please make sure you fix the connectivity from this machine to login.windows.net:443
 This usually indicates a client-side networking issue that you will need to pursue with your local network administrator.'

$AAD_login_microsoftonline_com = ' If you are using AAD Universal with MFA authentication please make sure you fix the connectivity from this machine to login.microsoftonline.com:443
 This usually indicates a client-side networking issue that you will need to pursue with your local network administrator.'

$AAD_secure_aadcdn_microsoftonline_p_com = ' If you are using AAD Universal with MFA authentication please make sure you fix the connectivity from this machine to secure.aadcdn.microsoftonline-p.com:443
 This usually indicates a client-side networking issue that you will need to pursue with your local network administrator.'

$error18456RecommendedSolution = ' This error indicates that the login request was rejected, the most common reasons are:
 - Incorrect or empty password: Please ensure that you have provided the correct password.
 - Database does not exist: Please ensure that the connection string has the correct database name.
 - Insufficient permissions: The user does not have CONNECT permissions to the database. Please ensure that the user is granted the necessary permissions to login.
 - Connections rejected due to DoSGuard protection: DoSGuard actively tracks failed logins from IP addresses. If there are multiple failed logins from a specific IP address within a period of time, the IP address is blocked from accessing any resources in the service for a pre-defined time period even if the password and other permissions are correct.'

$ServerNameNotSpecified = ' The parameter $Server was not specified, please set the parameters on the script, you need to set server name. Database name, user and password are optional but desirable.
 You can see more details about how to use this tool at https://github.com/Azure/SQL-Connectivity-Checker'

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

if (!$(Get-Command 'netsh' -errorAction SilentlyContinue) -and $CollectNetworkTrace) {
    Write-Host "WARNING: Current environment doesn't support network trace capture. This option is now disabled!"
    $CollectNetworkTrace = $false
}

# PowerShell Container Image Support End

function PrintDNSResults($dnsResult, [string] $dnsSource) {
    Try {
        if ($dnsResult) {
            $msg = ' Found DNS record in ' + $dnsSource + ' (IP Address:' + $dnsResult.IPAddress + ')'
            Write-Host $msg
            [void]$summaryLog.AppendLine($msg)
        }
        else {
            Write-Host ' Could not find DNS record in' $dnsSource
        }
    }
    Catch {
        $msg = "Error at PrintDNSResults for " + $dnsSource + '(' + $_.Exception.Message + ')'
        #Write-Host $msg -Foreground Red
        #Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously $msg
    }
}

function ValidateDNS([String] $Server) {
    Write-Host 'Validating DNS record for' $Server -ForegroundColor Green

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
        $DNSfromAzureDNS = Resolve-DnsName -Name $Server -DnsOnly -Server 208.67.222.222 -ErrorAction SilentlyContinue
        PrintDNSResults $DNSfromAzureDNS 'Open DNS'
    }
    Catch {
        Write-Host "Error at ValidateDNS from Open DNS" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously 'Error at ValidateDNS from Open DNS'
    }
}

function IsManagedInstance([String] $Server) {
    return [bool]((($Server.ToCharArray() | Where-Object { $_ -eq '.' } | Measure-Object).Count) -ge 4)
}

function IsSqlOnDemand([String] $Server) {
    return [bool]($Server -match '-ondemand.')
}

function IsManagedInstancePublicEndpoint([String] $Server) {
    return [bool]((IsManagedInstance $Server) -and ($Server -match '.public.'))
}

function HasPrivateLink([String] $Server) {
    [bool]((((Resolve-DnsName $Server) | Where-Object { $_.Name -Match ".privatelink." } | Measure-Object).Count) -gt 0)
}

function SanitizeString([String] $param) {
    return ($param.Replace('\', '_').Replace('/', '_').Replace("[", "").Replace("]", "").Replace('.', '_').Replace(':', '_').Replace(',', '_'))
}

function FilterTranscript() {
    Try {
        if ($canWriteFiles) {
            $lineNumber = (Select-String -Path $file -Pattern '..TranscriptStart..').LineNumber
            if ($lineNumber) {
                (Get-Content $file | Select-Object -Skip $lineNumber) | Set-Content $file
            }
        }
    }
    Catch {
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

function TestConnectionToDatabase($Server, $gatewayPort, $Database, $User, $Password) {
    Write-Host
    [void]$summaryLog.AppendLine()
    Write-Host ([string]::Format("Testing connecting to {0} database:", $Database)) -ForegroundColor Green
    Try {
        $masterDbConnection = [System.Data.SqlClient.SQLConnection]::new()
        $masterDbConnection.ConnectionString = [string]::Format("Server=tcp:{0},{1};Initial Catalog={2};Persist Security Info=False;User ID='{3}';Password='{4}';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Application Name=Azure-SQL-Connectivity-Checker;",
            $Server, $gatewayPort, $Database, $User, $Password)
        $masterDbConnection.Open()
        Write-Host ([string]::Format(" The connection attempt succeeded", $Database))
        return $true
    }
    catch [System.Data.SqlClient.SqlException] {
        $ex = $_.Exception
        Switch ($_.Exception.Number) {
            18456 {
                if ($User -eq 'AzSQLConnCheckerUser') {
                    if ($Database -eq 'master') {
                        $msg = [string]::Format(" Dummy login attempt reached '{0}' database, login failed as expected.", $Database)
                        Write-Host ($msg)
                        [void]$summaryLog.AppendLine($msg)
                    }
                    else {
                        $msg = [string]::Format(" Dummy login attempt on '{0}' database resulted in login failure.", $Database)
                        Write-Host ($msg)
                        [void]$summaryLog.AppendLine($msg)

                        $msg = ' This was either expected due to dummy credentials being used, or database does not exist, which also results in login failed.'
                        Write-Host ($msg)
                        [void]$summaryLog.AppendLine($msg)
                    }
                }
                else {
                    [void]$summaryRecommendedAction.AppendLine()
                    $msg = [string]::Format(" Login against database {0} failed for user '{1}'", $Database, $User)
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryLog.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine($msg)

                    $msg = $error18456RecommendedSolution
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                    TrackWarningAnonymously 'FailedLogin18456UserCreds'
                }
            }
            40532 {
                if ($_.Exception.Number -eq 40532 -and $gatewayPort -eq 3342) {
                    $msg = ' You seem to be trying to connect to MI using Public Endpoint but Public Endpoint may be disabled'
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryLog.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine($msg)

                    $msg = ' Learn how to configure public endpoint at https://docs.microsoft.com/en-us/azure/sql-database/sql-database-managed-instance-public-endpoint-configure'
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                    TrackWarningAnonymously 'SQLMI|PublicEndpoint|Error40532'
                }
                else {
                    $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryLog.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine()
                    [void]$summaryRecommendedAction.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine(' Please follow-up on this error.')
                    TrackWarningAnonymously ('TestConnectionToDatabase|Error:' + $ex.Number + 'State:' + $ex.State)
                }
            }
            40615 {
                $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine(' Please follow-up on this error.')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error:' + $ex.Number + 'State:' + $ex.State)
            }
            47073 {
                $msg = ' Connection to database ' + $Database + ' was denied since Deny Public Network Access is set to Yes.
 When Deny Public Network Access setting is set to Yes, only connections via private endpoints are allowed.
 When this setting is set to No (default), clients can connect using either public endpoints (IP-based firewall rules, VNET-based firewall rules) or private endpoints (using Private Link).
 See more at https://docs.microsoft.com/azure/azure-sql/database/connectivity-settings#deny-public-network-access'
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine(' Please follow-up on this error.')
                TrackWarningAnonymously ('TestConnectionToDatabase|47073')
            }
            default {
                $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine(' Please follow-up on this error.')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error:' + $ex.Number + 'State:' + $ex.State)
            }
        }
        return $false
    }
    Catch {
        Write-Host $_.Exception.Message -ForegroundColor Yellow
        TrackWarningAnonymously 'TestConnectionToDatabase|Exception'
        return $false
    }
}

function PrintLocalNetworkConfiguration() {
    if (![System.Net.NetworkInformation.NetworkInterface]::GetIsNetworkAvailable()) {
        Write-Host "There's no network connection available!" -ForegroundColor Red
        throw
    }

    $computerProperties = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties()
    $networkInterfaces = [System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces()

    Write-Host 'Interface information for '$computerProperties.HostName'.'$networkInterfaces.DomainName -ForegroundColor Green

    foreach ($networkInterface in $networkInterfaces) {
        if ($networkInterface.NetworkInterfaceType -eq 'Loopback') {
            continue
        }

        $properties = $networkInterface.GetIPProperties()

        Write-Host ' Interface name: ' $networkInterface.Name
        Write-Host ' Interface description: ' $networkInterface.Description
        Write-Host ' Interface type: ' $networkInterface.NetworkInterfaceType
        Write-Host ' Operational status: ' $networkInterface.OperationalStatus

        Write-Host ' Unicast address list:'
        Write-Host $('  ' + [String]::Join([Environment]::NewLine + '  ', [System.Linq.Enumerable]::Select($properties.UnicastAddresses, [Func[System.Net.NetworkInformation.UnicastIPAddressInformation, IPAddress]] { $args[0].Address })))

        Write-Host ' DNS server address list:'
        Write-Host $('  ' + [String]::Join([Environment]::NewLine + '  ', $properties.DnsAddresses))

        Write-Host
    }
}

function CheckAffected20191014($gateway) {
    $isCR1 = $CRaddress -eq $gateway.Gateways[0]
    if ($gateway.Affected20191014) {
        Write-Host 'This region WILL be affected by the Gateway migration starting at Oct 14 2019!' -ForegroundColor Yellow
        if ($isCR1) {
            Write-Host 'and this server is running on one of the affected Gateways' -ForegroundColor Red
        }
        else {
            Write-Host 'but this server is NOT running on one of the affected Gateways (never was or already migrated)' -ForegroundColor Green
            Write-Host 'Please check other servers you may have in the region' -ForegroundColor Yellow
        }
    }
    else {
        Write-Host 'This region will NOT be affected by the Oct 14 2019 Gateway migration!' -ForegroundColor Green
    }
    Write-Host
}

function RunSqlMIPublicEndpointConnectivityTests($resolvedAddress) {
    Try {
        $msg = 'Detected as Managed Instance using Public Endpoint'
        Write-Host $msg -ForegroundColor Yellow
        [void]$summaryLog.AppendLine($msg)
        TrackWarningAnonymously 'SQLMI|PublicEndpoint'

        Write-Host 'Public Endpoint connectivity test:' -ForegroundColor Green
        $testResult = Test-NetConnection $resolvedAddress -Port 3342 -WarningAction SilentlyContinue

        if ($testResult.TcpTestSucceeded) {
            Write-Host ' -> TCP test succeed' -ForegroundColor Green
            PrintAverageConnectionTime $resolvedAddress 3342
            $msg = ' Gateway connectivity to ' + $resolvedAddress + ':3342 succeed'
            [void]$summaryLog.AppendLine($msg)
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            $msg = ' Gateway connectivity to ' + $resolvedAddress + ':3342 FAILED'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            $msg = ' Please make sure you fix the connectivity from this machine to ' + $resolvedAddress + ':3342 (SQL MI Public Endpoint)'
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = $SQLMI_PublicEndPoint_GatewayTestFailed
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            TrackWarningAnonymously 'SQLMI|PublicEndPoint|GatewayTestFailed'
        }
    }
    Catch {
        Write-Host "Error at RunSqlMIPublicEndpointConnectivityTests" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously 'RunSqlMIPublicEndpointConnectivityTests|Exception'
    }
}

function RunSqlMIVNetConnectivityTests($resolvedAddress) {
    Try {
        Write-Host 'Detected as Managed Instance' -ForegroundColor Yellow
        TrackWarningAnonymously 'SQLMI|PrivateEndpoint'
        Write-Host
        Write-Host 'Gateway connectivity tests:' -ForegroundColor Green
        $testResult = Test-NetConnection $resolvedAddress -Port 1433 -WarningAction SilentlyContinue

        if ($testResult.TcpTestSucceeded) {
            Write-Host ' -> TCP test succeed' -ForegroundColor Green
            PrintAverageConnectionTime $resolvedAddress 1433
            return $true
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            Write-Host
            Write-Host ' Trying to get IP routes for interface:' $testResult.InterfaceAlias
            Get-NetAdapter $testResult.InterfaceAlias -ErrorAction SilentlyContinue -ErrorVariable ProcessError | Get-NetRoute
            If ($ProcessError) {
                Write-Host '  Could not to get IP routes for this interface'
            }
            Write-Host

            $msg = ' Gateway connectivity to ' + $resolvedAddress + ':1433 FAILED'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine()
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = ' Please fix the connectivity from this machine to ' + $resolvedAddress + ':1433'
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = $SQLMI_GatewayTestFailed
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            TrackWarningAnonymously 'SQLMI|GatewayTestFailed'
            return $false
        }
    }
    Catch {
        Write-Host "Error at RunSqlMIVNetConnectivityTests" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously 'RunSqlMIVNetConnectivityTests|Exception'
        return $false
    }
}

function PrintAverageConnectionTime($addressList, $port) {
    Write-Host ' Printing average connection times for 5 connection attempts:'
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

        $ilb = ''
        if ((IsManagedInstance $Server) -and !(IsManagedInstancePublicEndpoint $Server) -and ($ipAddress -eq $resolvedAddress)) {
            $ilb = ' [ilb]'
        }

        Write-Host '   IP Address:'$ipAddress'  Port:'$port
        Write-Host '   Successful connections:'$numSuccessful
        Write-Host '   Failed connections:'$numFailed
        Write-Host '   Average response time:'$avg' ms '$ilb
    }
}

function RunSqlDBConnectivityTests($resolvedAddress) {

    if (IsSqlOnDemand $Server) {
        Write-Host 'Detected as SQL on-demand endpoint' -ForegroundColor Yellow
        TrackWarningAnonymously 'SQL on-demand'
    }
    else {
        Write-Host 'Detected as SQL DB/DW Server' -ForegroundColor Yellow
        TrackWarningAnonymously 'SQL DB/DW'
    }

    $hasPrivateLink = HasPrivateLink $Server
    $gateway = $SQLDBGateways | Where-Object { $_.Gateways -eq $resolvedAddress }

    if (!$gateway) {
        if ($hasPrivateLink) {
            Write-Host ' This connection may be using Private Link, skipping Gateway connectivity tests' -ForegroundColor Yellow
            TrackWarningAnonymously 'SQLDB|PrivateEndpoint'
        }
        else {
            $msg = ' ERROR:' + $resolvedAddress + ' is not a valid gateway address'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = $SQLDB_InvalidGatewayIPAddress
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            TrackWarningAnonymously 'SQLDB|InvalidGatewayIPAddress'
            Write-Error '' -ErrorAction Stop
        }
    }
    else {
        Write-Host ' The server' $Server 'is running on ' -ForegroundColor White -NoNewline
        Write-Host $gateway.Region -ForegroundColor Yellow

        Write-Host
        [void]$summaryLog.AppendLine()
        Write-Host 'Gateway connectivity tests:' -ForegroundColor Green
        foreach ($gatewayAddress in $gateway.Gateways) {
            Write-Host
            Write-Host ' Testing (gateway) connectivity to' $gatewayAddress':1433' -ForegroundColor White -NoNewline
            $testResult = Test-NetConnection $gatewayAddress -Port 1433 -WarningAction SilentlyContinue

            if ($testResult.TcpTestSucceeded) {
                Write-Host ' -> TCP test succeed' -ForegroundColor Green
                PrintAverageConnectionTime $gatewayAddress 1433
                $msg = ' Gateway connectivity to ' + $gatewayAddress + ':1433 succeed'
                [void]$summaryLog.AppendLine($msg)
            }
            else {
                Write-Host ' -> TCP test FAILED' -ForegroundColor Red
                PrintAverageConnectionTime $gatewayAddress 1433
                Write-Host
                Write-Host ' IP routes for interface:' $testResult.InterfaceAlias
                Get-NetAdapter $testResult.InterfaceAlias | Get-NetRoute
                tracert -h 10 $Server

                $msg = ' Gateway connectivity to ' + $gatewayAddress + ':1433 FAILED'
                Write-Host $msg -Foreground Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)

                $msg = ' Please make sure you fix the connectivity from this machine to ' + $gatewayAddress + ':1433 to avoid issues!'
                Write-Host $msg -Foreground Red
                [void]$summaryRecommendedAction.AppendLine($msg)

                $msg = $SQLDB_GatewayTestFailed
                Write-Host $msg -Foreground Red
                [void]$summaryRecommendedAction.AppendLine($msg)

                TrackWarningAnonymously 'SQLDB|GatewayTestFailed'
            }
        }

        if ($gateway.TRs -and $gateway.Cluster -and $gateway.Cluster.Length -gt 0 ) {
            Write-Host
            Write-Host 'Redirect Policy related tests:' -ForegroundColor Green
            $redirectSucceeded = 0
            $redirectTests = 0
            foreach ($tr in $gateway.TRs | Where-Object { $_ -ne '' }) {
                foreach ($port in $TRPorts) {
                    $addr = [string]::Format("{0}.{1}", $tr, $gateway.Cluster)
                    Write-Host ' Tested (redirect) connectivity to' $addr':'$port -ForegroundColor White -NoNewline

                    $tcpClient = New-Object System.Net.Sockets.TcpClient
                    $portOpen = $tcpClient.ConnectAsync($addr, $port).Wait(6000)
                    if ($portOpen) {
                        $redirectTests += 1
                        $redirectSucceeded += 1
                        Write-Host ' -> TCP test succeeded' -ForegroundColor Green
                    }
                    else {
                        $redirectTests += 1
                        Write-Host ' -> TCP test FAILED' -ForegroundColor Red
                    }
                }
            }

            if ($redirectTests -gt 0) {
                $redirectTestsResultMessage = [System.Text.StringBuilder]::new()
                [void]$redirectTestsResultMessage.AppendLine()
                $redirectTestsResultMessage.ToString()

                [void]$redirectTestsResultMessage.AppendLine(' Tested (redirect) connectivity ' + $redirectTests + ' times and ' + $redirectSucceeded + ' of them succeeded')
                [void]$redirectTestsResultMessage.AppendLine(' Please note this was just some tests to check connectivity using the 11000-11999 port range, not your database')

                if (IsSqlOnDemand $Server) {
                    [void]$redirectTestsResultMessage.Append(' Some tests may even fail and not be a problem since ports tested here are static and SQL on-demand is a dynamic serverless environment.')
                }
                else {
                    [void]$redirectTestsResultMessage.Append(' Some tests may even fail and not be a problem since ports tested here are static and SQL DB is a dynamic environment.')
                }
                $msg = $redirectTestsResultMessage.ToString()
                Write-Host $msg -Foreground Yellow
                [void]$summaryLog.AppendLine($msg)

                TrackWarningAnonymously ('SQLDB|Redirect|' + $gateway.Region + '|' + $redirectSucceeded + '/' + $redirectTests)

                if ($redirectSucceeded / $redirectTests -ge 0.5 ) {
                    $msg = ' Based on the result it is likely the Redirect Policy will work from this machine'
                    Write-Host $msg -Foreground Green
                    [void]$summaryLog.AppendLine($msg)
                }
                else {

                    if ($redirectSucceeded / $redirectTests -eq 0.0 ) {
                        $msg = ' Based on the result the Redirect Policy will NOT work from this machine'
                        Write-Host $msg -Foreground Red
                        [void]$summaryLog.AppendLine($msg)
                        TrackWarningAnonymously 'SQLDB|Redirect|AllTestsFailed'
                    }
                    else {
                        $msg = ' Based on the result the Redirect Policy MAY NOT work from this machine, this can be expected for connections from outside Azure'
                        Write-Host $msg -Foreground Red
                        [void]$summaryLog.AppendLine($msg)
                        TrackWarningAnonymously ('SQLDB|Redirect|MoreThanHalfFailed|' + $redirectSucceeded + '/' + $redirectTests)
                    }

                    [void]$summaryRecommendedAction.AppendLine($msg)
                    $msg = $SQLDB_Redirect
                    Write-Host $msg -Foreground Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                }
            }
        }
    }
}

function RunConnectivityPolicyTests($port) {
    Write-Host
    Write-Host 'Advanced connectivity policy tests:' -ForegroundColor Green

    # Check removed
    #if (!$CustomerRunningInElevatedMode) {
    #    Write-Host ' Powershell must be run as an administrator to run advanced connectivity policy tests!' -ForegroundColor Yellow
    #    return
    #}

    if ($(Get-ExecutionPolicy) -eq 'Restricted') {
        Write-Host ' Advanced connectivity policy tests cannot be run because of current execution policy (Restricted)!' -ForegroundColor Yellow
        Write-Host ' Please use Set-ExecutionPolicy to allow scripts to run on this system!' -ForegroundColor Yellow
        return
    }

    $jobParameters = @{
        Server             = $Server
        Database           = $Database
        Port               = $port
        User               = $User
        Password           = $Password
        EncryptionProtocol = $EncryptionProtocol
        RepositoryBranch   = $RepositoryBranch
        Local              = $Local
        LocalPath          = $LocalPath
    }

    if (Test-Path "$env:TEMP\AzureSQLConnectivityChecker\") {
        Remove-Item $env:TEMP\AzureSQLConnectivityChecker -Recurse -Force
    }

    New-Item "$env:TEMP\AzureSQLConnectivityChecker\" -ItemType directory | Out-Null

    if ($Local) {
        Copy-Item -Path $($LocalPath + './AdvancedConnectivityPolicyTests.ps1') -Destination "$env:TEMP\AzureSQLConnectivityChecker\AdvancedConnectivityPolicyTests.ps1"
    }
    else {
        Invoke-WebRequest -Uri $('https://raw.githubusercontent.com/Azure/SQL-Connectivity-Checker/' + $RepositoryBranch + '/AdvancedConnectivityPolicyTests.ps1') -OutFile "$env:TEMP\AzureSQLConnectivityChecker\AdvancedConnectivityPolicyTests.ps1" -UseBasicParsing
    }

    $job = Start-Job -ArgumentList $jobParameters -FilePath "$env:TEMP\AzureSQLConnectivityChecker\AdvancedConnectivityPolicyTests.ps1"
    Wait-Job $job | Out-Null
    Receive-Job -Job $job
    Remove-Item $env:TEMP\AzureSQLConnectivityChecker -Recurse -Force
}

function SendAnonymousUsageData {
    try {
        #Despite computername and username will be used to calculate a hash string, this will keep you anonymous but allow us to identify multiple runs from the same user
        $StringBuilderHash = [System.Text.StringBuilder]::new()

        $text = $env:computername + $env:username
        if ([string]::IsNullOrEmpty($text)) {
            $text = $Host.InstanceId
        }

        [System.Security.Cryptography.HashAlgorithm]::Create("MD5").ComputeHash([System.Text.Encoding]::UTF8.GetBytes($text)) | ForEach-Object {
            [Void]$StringBuilderHash.Append($_.ToString("x2"))
        }

        $body = New-Object PSObject `
        | Add-Member -PassThru NoteProperty name 'Microsoft.ApplicationInsights.Event' `
        | Add-Member -PassThru NoteProperty time $([System.dateTime]::UtcNow.ToString('o')) `
        | Add-Member -PassThru NoteProperty iKey "a75c333b-14cb-4906-aab1-036b31f0ce8a" `
        | Add-Member -PassThru NoteProperty tags (New-Object PSObject | Add-Member -PassThru NoteProperty 'ai.user.id' $StringBuilderHash.ToString()) `
        | Add-Member -PassThru NoteProperty data (New-Object PSObject `
            | Add-Member -PassThru NoteProperty baseType 'EventData' `
            | Add-Member -PassThru NoteProperty baseData (New-Object PSObject `
                | Add-Member -PassThru NoteProperty ver 2 `
                | Add-Member -PassThru NoteProperty name '1.12'));

        $body = $body | ConvertTo-JSON -depth 5;
        Invoke-WebRequest -Uri 'https://dc.services.visualstudio.com/v2/track' -Method 'POST' -UseBasicParsing -body $body > $null
    }
    catch {
        Write-Host 'Error sending anonymous usage data:'
        Write-Host $_.Exception.Message
    }
}

function TrackWarningAnonymously ([String] $warningCode) {
    Try {
        #Despite computername and username will be used to calculate a hash string, this will keep you anonymous but allow us to identify multiple runs from the same user
        $StringBuilderHash = New-Object System.Text.StringBuilder
        [System.Security.Cryptography.HashAlgorithm]::Create("MD5").ComputeHash([System.Text.Encoding]::UTF8.GetBytes($env:computername + $env:username)) | ForEach-Object {
            [Void]$StringBuilderHash.Append($_.ToString("x2"))
        }

        $body = New-Object PSObject `
        | Add-Member -PassThru NoteProperty name 'Microsoft.ApplicationInsights.Event' `
        | Add-Member -PassThru NoteProperty time $([System.dateTime]::UtcNow.ToString('o')) `
        | Add-Member -PassThru NoteProperty iKey "a75c333b-14cb-4906-aab1-036b31f0ce8a" `
        | Add-Member -PassThru NoteProperty tags (New-Object PSObject | Add-Member -PassThru NoteProperty 'ai.user.id' $StringBuilderHash.ToString()) `
        | Add-Member -PassThru NoteProperty data (New-Object PSObject `
            | Add-Member -PassThru NoteProperty baseType 'EventData' `
            | Add-Member -PassThru NoteProperty baseData (New-Object PSObject `
                | Add-Member -PassThru NoteProperty ver 2 `
                | Add-Member -PassThru NoteProperty name $warningCode));
        $body = $body | ConvertTo-JSON -depth 5;
        Invoke-WebRequest -Uri 'https://dc.services.visualstudio.com/v2/track' -Method 'POST' -UseBasicParsing -body $body > $null
    }
    Catch {
        Write-Host 'TrackWarningAnonymously exception:'
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

$ProgressPreference = "SilentlyContinue";

if ([string]::IsNullOrEmpty($env:TEMP)) {
    $env:TEMP = '/tmp';
}

try {
    Clear-Host
    $canWriteFiles = $true
    try {
        $logsFolderName = 'AzureSQLConnectivityCheckerResults'
        Set-Location -Path $env:TEMP
        If (!(Test-Path $logsFolderName)) {
            New-Item $logsFolderName -ItemType directory | Out-Null
            Write-Host 'The folder' $logsFolderName 'was created'
        }
        else {
            Write-Host 'The folder' $logsFolderName 'already exists'
        }
        Set-Location $logsFolderName
        $outFolderName = [System.DateTime]::Now.ToString('yyyyMMddTHHmmss')
        New-Item $outFolderName -ItemType directory | Out-Null
        Set-Location $outFolderName

        $file = '.\Log_' + (SanitizeString ($Server.Replace('.database.windows.net', ''))) + '_' + (SanitizeString $Database) + '_' + [System.DateTime]::Now.ToString('yyyyMMddTHHmmss') + '.txt'
        Start-Transcript -Path $file
        Write-Host '..TranscriptStart..'
    }
    catch {
        $canWriteFiles = $false
        Write-Host Warning: Cannot write log file -ForegroundColor Yellow
    }

    if ($SendAnonymousUsageData) {
        SendAnonymousUsageData
    }

    try {
        Write-Host '******************************************' -ForegroundColor Green
        Write-Host '  Azure SQL Connectivity Checker v1.12  ' -ForegroundColor Green
        Write-Host '******************************************' -ForegroundColor Green
        Write-Host
        Write-Host 'Parameters' -ForegroundColor Yellow
        Write-Host ' Server:' $Server -ForegroundColor Yellow
        if ($null -ne $Database) {
            Write-Host ' Database:' $Database -ForegroundColor Yellow
        }
        if ($null -ne $RunAdvancedConnectivityPolicyTests) {
            Write-Host ' RunAdvancedConnectivityPolicyTests:' $RunAdvancedConnectivityPolicyTests -ForegroundColor Yellow
        }
        if ($null -ne $CollectNetworkTrace) {
            Write-Host ' CollectNetworkTrace:' $CollectNetworkTrace -ForegroundColor Yellow
        }
        if ($null -ne $EncryptionProtocol) {
            Write-Host ' EncryptionProtocol:' $EncryptionProtocol -ForegroundColor Yellow
        }
        Write-Host

        if (!$Server -or $Server.Length -eq 0 -or $Server -eq '.database.windows.net') {
            $msg = $ServerNameNotSpecified
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'ServerNameNotSpecified'
            Write-Error '' -ErrorAction Stop
        }

        if (!$Server.EndsWith('.database.windows.net') `
                -and !$Server.EndsWith('.database.cloudapi.de') `
                -and !$Server.EndsWith('.database.chinacloudapi.cn') `
                -and !$Server.EndsWith('.database.usgovcloudapi.net') `
                -and !$Server.EndsWith('.sql.azuresynapse.net')) {
            $Server = $Server + '.database.windows.net'
        }

        #Print local network configuration
        PrintLocalNetworkConfiguration

        if ($canWriteFiles -and $CollectNetworkTrace) {
            if (!$CustomerRunningInElevatedMode) {
                Write-Host ' Powershell must be run as an administrator in order to collect network trace!' -ForegroundColor Yellow
                $netWorkTraceStarted = $false
            }
            else {
                $traceFileName = (Get-Location).Path + '\NetworkTrace_' + [System.DateTime]::Now.ToString('yyyyMMddTHHmmss') + '.etl'
                $startNetworkTrace = "netsh trace start persistent=yes capture=yes tracefile=$traceFileName"
                Invoke-Expression $startNetworkTrace
                $netWorkTraceStarted = $true
            }
        }

        ValidateDNS $Server

        try {
            $dnsResult = [System.Net.DNS]::GetHostEntry($Server)
        }
        catch {
            $msg = ' ERROR: Name resolution (DNS) of ' + $Server + ' failed'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            if (IsManagedInstancePublicEndpoint $Server) {
                $msg = $DNSResolutionFailedSQLMIPublicEndpoint
                Write-Host $msg -Foreground Red
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously 'DNSResolutionFailedSQLMIPublicEndpoint'
            }
            else {
                $msg = $DNSResolutionFailed
                Write-Host $msg -Foreground Red
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously 'DNSResolutionFailed'
            }
            Write-Error '' -ErrorAction Stop
        }
        $resolvedAddress = $dnsResult.AddressList[0].IPAddressToString
        $dbPort = 1433

        #Run connectivity tests
        Write-Host
        if (IsManagedInstance $Server) {
            if (IsManagedInstancePublicEndpoint $Server) {
                RunSqlMIPublicEndpointConnectivityTests $resolvedAddress
                $dbPort = 3342
            }
            else {
                if (!(RunSqlMIVNetConnectivityTests $resolvedAddress)) {
                    throw
                }
            }
        }
        else {
            RunSqlDBConnectivityTests $resolvedAddress
        }

        $customDatabaseNameWasSet = $Database -and $Database.Length -gt 0 -and $Database -ne 'master'

        #Test master database
        $canConnectToMaster = TestConnectionToDatabase $Server $dbPort 'master' $User $Password

        if ($customDatabaseNameWasSet) {
            if ($canConnectToMaster) {
                Write-Host ' Checking if' $Database 'exist in sys.databases:' -ForegroundColor White
                $masterDbConnection = [System.Data.SqlClient.SQLConnection]::new()
                $masterDbConnection.ConnectionString = [string]::Format("Server=tcp:{0},{1};Initial Catalog='master';Persist Security Info=False;User ID='{2}';Password='{3}';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Application Name=Azure-SQL-Connectivity-Checker;",
                    $Server, $dbPort, $User, $Password)
                $masterDbConnection.Open()

                $masterDbCommand = New-Object System.Data.SQLClient.SQLCommand
                $masterDbCommand.Connection = $masterDbConnection

                $masterDbCommand.CommandText = "select count(*) C from sys.databases where name = '" + $Database + "'"
                $masterDbResult = $masterDbCommand.ExecuteReader()
                $masterDbResultDataTable = new-object 'System.Data.DataTable'
                $masterDbResultDataTable.Load($masterDbResult)

                if ($masterDbResultDataTable.Rows[0].C -eq 0) {
                    $msg = ' ERROR: ' + $Database + ' was not found in sys.databases!'
                    Write-Host $msg -Foreground Red
                    [void]$summaryLog.AppendLine()
                    [void]$summaryLog.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine()
                    [void]$summaryRecommendedAction.AppendLine($msg)

                    $msg = ' Please confirm the database name is correct and/or look at the operation logs to see if the database has been dropped by another user.'
                    Write-Host $msg -Foreground Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                    TrackWarningAnonymously 'DatabaseNotFoundInMasterSysDatabases'
                }
                else {
                    $msg = ' ' + $Database + ' was found in sys.databases of master database'
                    Write-Host $msg -Foreground Green
                    [void]$summaryLog.AppendLine()
                    [void]$summaryLog.AppendLine($msg)

                    #Test database from parameter
                    if ($customDatabaseNameWasSet) {
                        TestConnectionToDatabase $Server $dbPort $Database $User $Password | Out-Null
                    }
                }
            }
            else {
                #Test database from parameter anyway
                if ($customDatabaseNameWasSet) {
                    TestConnectionToDatabase $Server $dbPort $Database $User $Password | Out-Null
                }
            }
        }

        Write-Host
        [void]$summaryLog.AppendLine()
        Write-Host 'Test endpoints for AAD Password and Integrated Authentication:' -ForegroundColor Green
        Write-Host ' Tested connectivity to login.windows.net:443' -ForegroundColor White -NoNewline
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $portOpen = $tcpClient.ConnectAsync("login.windows.net", 443).Wait(10000)
        if ($portOpen) {
            Write-Host ' -> TCP test succeeded' -ForegroundColor Green
            $msg = ' Connectivity to login.windows.net:443 succeed (used for AAD Password and Integrated Authentication)'
            [void]$summaryLog.AppendLine($msg)
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            $msg = ' Connectivity to login.windows.net:443 FAILED (used for AAD Password and AAD Integrated Authentication)'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            $msg = $AAD_login_windows_net
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'AAD|login.windows.net'
        }

        Write-Host
        Write-Host 'Test endpoints for Universal with MFA authentication:' -ForegroundColor Green
        Write-Host ' Tested connectivity to login.microsoftonline.com:443' -ForegroundColor White -NoNewline
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $portOpen = $tcpClient.ConnectAsync("login.microsoftonline.com", 443).Wait(10000)
        if ($portOpen) {
            Write-Host ' -> TCP test succeeded' -ForegroundColor Green
            $msg = ' Connectivity to login.microsoftonline.com:443 succeed (used for AAD Universal with MFA authentication)'
            [void]$summaryLog.AppendLine($msg)
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            $msg = ' Connectivity to login.microsoftonline.com:443 FAILED (used for AAD Universal with MFA authentication)'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            $msg = $AAD_login_microsoftonline_com
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'AAD|login.microsoftonline.com'
        }

        Write-Host ' Tested connectivity to secure.aadcdn.microsoftonline-p.com:443' -ForegroundColor White -NoNewline
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $portOpen = $tcpClient.ConnectAsync("secure.aadcdn.microsoftonline-p.com", 443).Wait(10000)
        if ($portOpen) {
            Write-Host ' -> TCP test succeeded' -ForegroundColor Green
            $msg = ' Connectivity to secure.aadcdn.microsoftonline-p.com:443 succeed (used for AAD Universal with MFA authentication)'
            [void]$summaryLog.AppendLine($msg)
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            $msg = ' Connectivity to secure.aadcdn.microsoftonline-p.com:443 FAILED (used for AAD Universal with MFA authentication)'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            $msg = $AAD_secure_aadcdn_microsoftonline_p_com
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'AAD|secure.aadcdn.microsoftonline-p.com'
        }

        #Advanced Connectivity Tests
        try {
            if ($RunAdvancedConnectivityPolicyTests) {
                RunConnectivityPolicyTests $dbPort
            }
        }
        catch {
            $msg = ' ERROR running Advanced Connectivity Tests: ' + $_.Exception.Message
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)
            TrackWarningAnonymously 'ERROR running Advanced Connectivity Test'
        }

        Write-Host
        Write-Host 'All tests are now done!' -ForegroundColor Green
    }
    catch {
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host 'Exception thrown while testing, stopping execution...' -ForegroundColor Yellow
    }
    finally {
        if ($netWorkTraceStarted) {
            Write-Host 'Stopping network trace.... please wait, this may take a few minutes' -ForegroundColor Yellow
            $stopNetworkTrace = "netsh trace stop"
            Invoke-Expression $stopNetworkTrace
            $netWorkTraceStarted = $false
        }

        Write-Host
        Write-Host '######################################################' -ForegroundColor Green
        Write-Host 'SUMMARY:' -ForegroundColor Yellow
        Write-Host '######################################################' -ForegroundColor Green
        Write-Host $summaryLog.ToString() -ForegroundColor Yellow
        Write-Host
        Write-Host '######################################################' -ForegroundColor Green
        Write-Host 'RECOMMENDED ACTION(S):' -ForegroundColor Yellow
        Write-Host '######################################################' -ForegroundColor Green
        if ($summaryRecommendedAction.Length -eq 0) {
            Write-Host ' Based on test results, there are no recommended actions.' -ForegroundColor Green
            TrackWarningAnonymously 'NoRecommendedActions'
        }
        else {
            Write-Host $summaryRecommendedAction.ToString() -ForegroundColor Yellow
        }
        Write-Host
        Write-Host

        if ($canWriteFiles) {
            try {
                Stop-Transcript | Out-Null
            }
            catch [System.InvalidOperationException] { }

            FilterTranscript
        }
    }
}
finally {
    if ($canWriteFiles) {
        Write-Host Log file can be found at (Get-Location).Path
        if ($PSVersionTable.PSVersion.Major -ge 5) {
            $destAllFiles = (Get-Location).Path + '/AllFiles.zip'
            Compress-Archive -Path (Get-Location).Path -DestinationPath $destAllFiles -Force
            Write-Host 'A zip file with all the files can be found at' $destAllFiles -ForegroundColor Green
        }

        if ($PSVersionTable.Platform -eq 'Unix') {
            Get-ChildItem
        }
        else {
            Invoke-Item (Get-Location).Path
        }
    }
}