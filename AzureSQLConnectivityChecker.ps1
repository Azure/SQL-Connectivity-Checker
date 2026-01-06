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
using namespace System.Diagnostics.Tracing
using namespace System.Data.SqlClient

# Parameter region for when script is run directly
# Supports Azure SQL Database, Azure SQL Managed Instance, and Azure Synapse Analytics (*.sql.azuresynapse.net / *.database.windows.net)
# Supports Public Cloud (*.database.windows.net), Azure China (*.database.chinacloudapi.cn), and Azure Government (*.database.usgovcloudapi.net)

# Set the type of authentication you wish to use:
# 'SQL Server Authentication' (default),
# 'Microsoft Entra Password', (supported only with MSAL)
# 'Microsoft Entra Integrated',
# 'Microsoft Entra Interactive',
# 'Microsoft Entra Managed Identity' ('Microsoft Entra MSI') NOTE: Managed Identity authentication works only when your application is running as an Azure resource, not with your personal account
$AuthenticationType = 'SQL Server Authentication'
# To be used with Microsoft Entra, set the authentication library you wish to use: 'ADAL' or 'MSAL'. Default is 'ADAL'.
$AuthenticationLibrary = 'MSAL'
$Server = '.database.windows.net' # or any other supported FQDN
$Database = ''  # Set the name of the database you wish to test, 'master' will be used by default if nothing is set
$User = ''  # Set the login username you wish to use, 'AzSQLConnCheckerUser' will be used by default if nothing is set
$Password = ''  # Set the login password you wish to use, 'AzSQLConnCheckerPassword' will be used by default if nothing is set
$UserAssignedIdentityClientId = '' # To be used with Microsoft Entra Managed Identity, set the Client ID of the User Assigned Identity you wish to use, if nothing is set, the script will use the system-assigned identity

# In case you want to hide the password (like during a remote session), uncomment the 2 lines below (by removing leading #) and password will be asked during execution
# $Credentials = Get-Credential -Message "Credentials to test connections to the database (optional)" -User $User
# $Password = $Credentials.GetNetworkCredential().password

# Optional parameters (default values will be used if omitted)
$SendAnonymousUsageData = $true  # Set as $true (default) or $false
$RunAdvancedConnectivityPolicyTests = $true  # Set as $true (default) or $false#Set as $true (default) or $false, this will download library needed for running advanced connectivity policy tests
$ConnectionAttempts = 1
$DelayBetweenConnections = 1
$CollectNetworkTrace = $true  # Set as $true (default) or $false
$EncryptionProtocol = 'Tls 1.2'  # Supported values: 'Tls 1.0', 'Tls 1.1', 'Tls 1.2'; Without this parameter operating system will choose the best protocol to use
$TrustServerCertificate = $false  # Set as $true or $false (default)

# Parameter region when Invoke-Command -ScriptBlock is used
$parameters = $args[0]
if ($null -ne $parameters) {
    $AuthenticationType = $parameters['AuthenticationType']
    $AuthenticationLibrary = $parameters['AuthenticationLibrary']
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
    if ($null -ne $parameters['EncryptionProtocol']) {
        $EncryptionProtocol = $parameters['EncryptionProtocol']
    }
    if ($null -ne $parameters['Local']) {
        $Local = $parameters['Local']
    }
    if ($null -ne $parameters['LocalPath']) {
        $LocalPath = $parameters['LocalPath']
    }
    if ($null -ne $parameters['RepositoryBranch']) {
        $RepositoryBranch = $parameters['RepositoryBranch']
    }
    if ($null -ne $parameters['ConnectionAttempts']) {
        $ConnectionAttempts = $parameters['ConnectionAttempts']
    }
    if ($null -ne $parameters['DelayBetweenConnections']) {
        $DelayBetweenConnections = $parameters['DelayBetweenConnections']
    }
    if ($null -ne $parameters['TrustServerCertificate']) {
        $TrustServerCertificate = $parameters['TrustServerCertificate']
    }
}

# Setting default parameters if not provided

if ($null -eq $AuthenticationType -or '' -eq $AuthenticationType) {
    $AuthenticationType = 'SQL Server Authentication'
    $AuthenticationLibrary = ''
}

if ($null -eq $AuthenticationLibrary -or '' -eq $AuthenticationLibrary) {
    $AuthenticationLibrary = 'MSAL'
}

if (($null -eq $User -or '' -eq $User) -and $AuthenticationType -ne "Microsoft Entra Integrated") {
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
    New-Object PSObject -Property @{Region = "Australia Central"; Gateways = ("20.36.105.33", "20.36.105.34"); }
    New-Object PSObject -Property @{Region = "Australia Central2"; Gateways = ("20.36.113.33", "20.36.113.34"); }
    New-Object PSObject -Property @{Region = "Australia East"; Gateways = ("20.53.46.128", "20.53.46.129", "20.53.46.130"); }
    New-Object PSObject -Property @{Region = "Australia South East"; Gateways = ("13.77.49.33", "13.77.49.34", "13.77.49.35", "13.77.49.37"); }
    New-Object PSObject -Property @{Region = "Austria East"; Gateways = ("68.210.175.32", "68.210.175.33"); }
    New-Object PSObject -Property @{Region = "Belgium Central"; Gateways = ("9.160.56.96"); }
    New-Object PSObject -Property @{Region = "Brazil South"; Gateways = ("191.234.142.160", "191.234.142.161"); }
    New-Object PSObject -Property @{Region = "Brazil Southeast"; Gateways = ("191.233.48.32", "191.233.48.33"); }
    New-Object PSObject -Property @{Region = "Canada Central"; Gateways = ("20.48.196.32", "20.48.196.33"); }
    New-Object PSObject -Property @{Region = "Canada East"; Gateways = ("40.69.105.33", "40.69.105.34"); }
    New-Object PSObject -Property @{Region = "Central US"; Gateways = ("20.40.228.128", "20.40.228.129", "20.40.228.130", "20.40.228.131", "20.40.228.133", "20.40.228.135", "20.40.228.136"); }
    New-Object PSObject -Property @{Region = "Central US EUAP"; Gateways = ("40.78.200.131", "40.78.200.133", "40.78.200.134"); }
    New-Object PSObject -Property @{Region = "Chile Central"; Gateways = ("68.211.15.128", "68.211.15.129"); }
    New-Object PSObject -Property @{Region = "China East"; Gateways = ("52.130.112.137", "52.130.112.141"); }
    New-Object PSObject -Property @{Region = "China East 2"; Gateways = ("52.130.120.89", "52.130.120.90", "52.130.120.92"); }
    New-Object PSObject -Property @{Region = "China East 3"; Gateways = ("52.131.155.194", "52.131.155.196"); }
    New-Object PSObject -Property @{Region = "China North"; Gateways = ("52.130.128.91", "52.130.128.92"); }
    New-Object PSObject -Property @{Region = "China North 2"; Gateways = ("52.130.40.65", "52.130.40.66"); }
    New-Object PSObject -Property @{Region = "China North 3"; Gateways = ("159.27.21.32", "159.27.21.33"); }
    New-Object PSObject -Property @{Region = "East Asia"; Gateways = ("20.195.72.32", "20.195.72.33"); }
    New-Object PSObject -Property @{Region = "East US"; Gateways = ("20.62.132.160", "20.62.132.161", "20.62.132.162", "20.62.132.163", "20.62.132.164", "20.62.132.165", "20.62.132.166", "20.62.132.169"); }
    New-Object PSObject -Property @{Region = "East US 2"; Gateways = ("20.62.58.128", "20.62.58.129", "20.62.58.130", "20.62.58.131", "20.62.58.133", "20.62.58.135", "20.62.58.136"); }
    New-Object PSObject -Property @{Region = "East US 2 EUAP"; Gateways = ("20.51.17.168", "20.51.17.169", "40.75.32.40"); }
    New-Object PSObject -Property @{Region = "France Central"; Gateways = ("20.43.47.192", "20.43.47.193", "20.43.47.194"); }
    New-Object PSObject -Property @{Region = "France South"; Gateways = ("40.79.176.41", "40.79.176.42"); }
    New-Object PSObject -Property @{Region = "Germany North"; Gateways = ("51.116.57.32", "51.116.57.33"); }
    New-Object PSObject -Property @{Region = "Germany West Central"; Gateways = ("51.116.149.32", "51.116.149.33", "51.116.149.34"); }
    New-Object PSObject -Property @{Region = "India Central"; Gateways = ("20.192.43.160", "20.192.43.161"); }
    New-Object PSObject -Property @{Region = "India South"; Gateways = ("40.78.192.33", "40.78.192.34"); }
    New-Object PSObject -Property @{Region = "India West"; Gateways = ("104.211.144.34", "104.211.144.35"); }
    New-Object PSObject -Property @{Region = "Indonesia Central"; Gateways = ("70.153.167.65", "70.153.167.66"); }
    New-Object PSObject -Property @{Region = "Israel Central"; Gateways = ("20.217.53.0", "20.217.53.1"); }
    New-Object PSObject -Property @{Region = "Israel Northwest"; Gateways = ("51.4.162.136"); }
    New-Object PSObject -Property @{Region = "Italy North"; Gateways = ("4.232.101.160", "4.232.101.161"); }
    New-Object PSObject -Property @{Region = "Japan East"; Gateways = ("20.191.165.161", "20.191.165.162"); }
    New-Object PSObject -Property @{Region = "Japan West"; Gateways = ("20.189.225.160", "20.189.225.161"); }
    New-Object PSObject -Property @{Region = "Jio India Central"; Gateways = ("20.192.233.33", "20.192.233.34"); }
    New-Object PSObject -Property @{Region = "Jio India West"; Gateways = ("20.193.200.34", "20.193.200.35"); }
    New-Object PSObject -Property @{Region = "Korea Central"; Gateways = ("20.194.73.67", "20.194.73.68"); }
    New-Object PSObject -Property @{Region = "Korea South"; Gateways = ("52.231.151.98", "52.231.151.99", "52.231.151.100"); }
    New-Object PSObject -Property @{Region = "Malaysia South"; Gateways = ("20.17.67.248"); }
    New-Object PSObject -Property @{Region = "Malaysia West"; Gateways = ("20.17.127.96", "20.17.127.97"); }
    New-Object PSObject -Property @{Region = "Mexico Central"; Gateways = ("158.23.112.160", "158.23.112.161"); }
    New-Object PSObject -Property @{Region = "New Zealand North"; Gateways = ("172.204.167.64", "172.204.167.65"); }
    New-Object PSObject -Property @{Region = "North Central US"; Gateways = ("20.125.171.192", "52.162.105.200"); }
    New-Object PSObject -Property @{Region = "North Europe"; Gateways = ("52.146.133.130", "52.146.133.131", "52.146.133.132", "52.146.133.133", "52.146.133.134"); }
    New-Object PSObject -Property @{Region = "Norway East"; Gateways = ("51.120.232.195", "51.120.232.196"); }
    New-Object PSObject -Property @{Region = "Norway West"; Gateways = ("51.120.217.32", "51.120.217.33"); }
    New-Object PSObject -Property @{Region = "Poland Central"; Gateways = ("20.215.13.1", "20.215.13.2"); }
    New-Object PSObject -Property @{Region = "Qatar Central"; Gateways = ("20.21.53.32", "20.21.53.33"); }
    New-Object PSObject -Property @{Region = "South Africa North"; Gateways = ("102.133.221.224", "102.133.221.225", "102.133.221.226", "102.133.221.227"); }
    New-Object PSObject -Property @{Region = "South Africa West"; Gateways = ("102.133.25.32", "102.133.25.33"); }
    New-Object PSObject -Property @{Region = "South Central US"; Gateways = ("20.65.132.160", "20.65.132.161", "20.65.132.163", "20.65.132.164"); }
    New-Object PSObject -Property @{Region = "South Central US 2"; Gateways = ("48.216.10.160"); }
    New-Object PSObject -Property @{Region = "South East Asia"; Gateways = ("20.195.65.33", "20.195.65.34", "20.195.65.35", "20.195.65.36", "20.195.65.37", "20.195.65.39"); }
    New-Object PSObject -Property @{Region = "Southeast US"; Gateways = ("68.154.137.64"); }
    New-Object PSObject -Property @{Region = "Southeast US 3"; Gateways = ("74.7.82.136"); }
    New-Object PSObject -Property @{Region = "Spain Central"; Gateways = ("68.221.40.160", "68.221.40.161"); }
    New-Object PSObject -Property @{Region = "Sweden Central"; Gateways = ("51.12.46.32", "51.12.46.33"); }
    New-Object PSObject -Property @{Region = "Sweden South"; Gateways = ("51.12.200.33", "51.12.200.34"); }
    New-Object PSObject -Property @{Region = "Switzerland North"; Gateways = ("51.107.242.32", "51.107.242.33", "51.107.242.34"); }
    New-Object PSObject -Property @{Region = "Switzerland West"; Gateways = ("51.107.153.32", "51.107.153.33"); }
    New-Object PSObject -Property @{Region = "Taiwan North"; Gateways = ("51.53.107.248", "51.53.107.249", "51.53.107.250"); }
    New-Object PSObject -Property @{Region = "Taiwan Northwest"; Gateways = ("51.53.187.248", "51.53.187.249"); }
    New-Object PSObject -Property @{Region = "UAE Central"; Gateways = ("20.37.72.96", "20.37.72.97"); }
    New-Object PSObject -Property @{Region = "UAE North"; Gateways = ("20.38.143.64", "20.38.143.65", "20.38.143.67", "20.38.143.68"); }
    New-Object PSObject -Property @{Region = "UK South"; Gateways = ("51.143.209.224", "51.143.209.226"); }
    New-Object PSObject -Property @{Region = "UK West"; Gateways = ("51.140.208.99", "51.140.208.100"); }
    New-Object PSObject -Property @{Region = "West Central US"; Gateways = ("57.151.152.24", "172.215.203.64"); }
    New-Object PSObject -Property @{Region = "West Europe"; Gateways = ("20.61.99.192", "20.61.99.193", "20.61.99.194", "20.61.99.197", "20.61.99.198"); }
    New-Object PSObject -Property @{Region = "West US"; Gateways = ("13.86.217.224", "13.86.217.225", "20.168.163.193", "20.168.163.194"); }
    New-Object PSObject -Property @{Region = "West US 2"; Gateways = ("20.51.9.128", "20.51.9.129", "20.51.9.130", "20.51.9.131"); }
    New-Object PSObject -Property @{Region = "West US 3"; Gateways = ("20.150.241.128", "20.150.241.129"); }
    New-Object PSObject -Property @{Region = "US DoD East"; Gateways = ("52.126.200.3"); }
    New-Object PSObject -Property @{Region = "US DoD Central"; Gateways = ("52.182.88.34"); }
    New-Object PSObject -Property @{Region = "US Gov Texas"; Gateways = ("52.127.33.32"); }
    New-Object PSObject -Property @{Region = "US Gov Arizona"; Gateways = ("52.127.0.14"); }
    New-Object PSObject -Property @{Region = "US Gov Virginia"; Gateways = ("13.72.48.140", "52.127.41.2", "20.140.88.2"); }
)


$summaryLog = New-Object -TypeName "System.Text.StringBuilder"
$summaryRecommendedAction = New-Object -TypeName "System.Text.StringBuilder"
$AnonymousRunId = ([guid]::NewGuid()).Guid

# Error Messages

$DNSResolutionFailed = ' Please make sure the server name FQDN is correct and that your machine can resolve it.
 Failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.'

$DNSResolutionGotMultipleAddresses = ' While testing DNS resolution from multiples sources (hosts file/cache/your DNS server/external DNS service) we got multiple addresses.
 To connect to SQL Database or Azure Synapse, you need to allow network traffic to and from all Gateways for the region.
 The Gateway used is not static, configuring a single specific address (like in hosts file) may lead to total lack of connectivity or intermittent connectivity issues (now or in the future).
 Having DNS resolution switching between a couple of Gateway addresses is expected.
 If you are using Private Link, a mismatch between your DNS server and OpenDNS is expected.
 Please review the DNS results.'

$DNSResolutionGotMultipleAddressesMI = ' While testing DNS resolution from multiples sources (hosts file/cache/your DNS server/external DNS service) we got multiple addresses.
 SQL Managed Instance IP address may change, see more at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/frequently-asked-questions-faq#connectivity
 Configuring a specific IP address (like in hosts file) may lead to total lack of connectivity or intermittent connectivity issues (now or in the future).
 Please review the DNS results.'

$DNSResolutionFailedSQLMIPublicEndpoint = ' Please make sure the server name FQDN is correct and that your machine can resolve it.
 You seem to be trying to connect using Public Endpoint, this error can be caused if the Public Endpoint is Disabled.
 See how to enable public endpoint for your managed instance at https://aka.ms/mimanage-publicendpoint
 If public endpoint is enabled, failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.'

$SQLDB_InvalidGatewayIPAddress = ' In case you are not using Private Endpoint, please make sure the server name FQDN is correct and that your machine can resolve it to a valid gateway IP address (DNS configuration).
 In case you are not using Private Link, failure to resolve domain name for your logical server is almost always the result of specifying an invalid/misspelled server name,
 or a client-side networking issue that you will need to pursue with your local network administrator.
 See the valid gateway addresses at https://docs.microsoft.com/azure/azure-sql/database/connectivity-architecture#gateway-ip-addresses
 See more about Private Endpoint at https://docs.microsoft.com/en-us/azure/azure-sql/database/private-endpoint-overview'

$SQLDB_GatewayTestFailed = ' Failure to reach the Gateway is usually a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.
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
 If you are using Redirect, failure to reach ports in the range of 11000-11999 is usually a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.
 Please check more about connection policies at https://docs.microsoft.com/en-us/azure/azure-sql/database/connectivity-architecture#connection-policy"

$SQLMI_GatewayTestFailed = " You can connect to SQL Managed Instance via private endpoint if you are connecting from one of the following:
 - machine inside the same virtual network
 - machine in a peered virtual network
 - machine that is network connected by VPN or Azure ExpressRoute

 Failure to reach the Gateway is usually a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.
 We strongly recommend you request assistance from your network administrator, some validations you may do together are:

 - The host name is valid and port used for the connection is 1433, format is tcp:<mi_name>.<dns_zone>.database.windows.net,1433

 - The Network Security Groups (NSG) on the managed instance subnet allows access on port 1433.

 - If you are unable to connect from an Azure hosted client (like an Azure virtual machine), check if you have a Network Security Group set on the client subnet that might be blocking *outbound* access on port 1433.

 - If the connection type is Redirect:
    - Ensure the Network Security Groups (NSG) on the managed instance subnet allows access on ports **11000-11999**.
    - If you are unable to connect from an Azure hosted client (like an Azure virtual machine), check if you have a Network Security Group set on the client subnet that might be blocking *outbound* access on ports **11000-11999**.

 - Any networking device used (like firewalls, NVAs) do not block the traffic mentioned above.

 - Routing is properly configured, and asymmetric routing is avoided.
   A route with the 0.0.0.0/0 address prefix instructs Azure how to route traffic destined for an IP address that is not within the address prefix of any other route in a subnet's route table. When a subnet is created, Azure creates a default route to the 0.0.0.0/0 address prefix, with the **Internet** next hop type. Check if this route was overridden. See the details about impact of changes on this default route at https://docs.microsoft.com/azure/virtual-network/virtual-networks-udr-overview#default-route

 - If you are using virtual network peering between different regions, ensure that **global virtual network peering** is supported. See more at https://docs.microsoft.com/azure/azure-sql/managed-instance/connect-application-instance#connect-inside-a-different-vnet

 - If you are using peering via VPN gateway, ensure the two virtual networks are properly peered, see more at https://docs.microsoft.com/azure/azure-sql/managed-instance/connect-application-instance#connect-from-on-premises

Learn more about how to connect your application to Azure SQL Managed Instance at https://docs.microsoft.com/azure/azure-sql/managed-instance/connect-application-instance
"

$SQLMI_PublicEndPoint_GatewayTestFailed = " This usually indicates a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.
 We strongly recommend you request assistance from your network administrator, some validations you may do together are:

 - You have Public Endpoint enabled, see https://docs.microsoft.com/azure/azure-sql/managed-instance/public-endpoint-configure#enabling-public-endpoint-for-a-managed-instance-in-the-azure-portal

 - You have allowed public endpoint traffic on the network security group, see https://docs.microsoft.com/azure/azure-sql/managed-instance/public-endpoint-configure#allow-public-endpoint-traffic-on-the-network-security-group

 - The host name contains .public. and that port used in the connection string is 3342, format is <mi_name>.public.<dns_zone>.database.windows.net,3342

 - Network traffic to this endpoint and port is allowed from the source and any networking appliances you may have (firewalls, etc.).

 - Routing is properly configured, and asymmetric routing is avoided.
   A route with the 0.0.0.0/0 address prefix instructs Azure how to route traffic destined for an IP address that is not within the address prefix of any other route in a subnet's route table. When a subnet is created, Azure creates a default route to the 0.0.0.0/0 address prefix, with the **Internet** next hop type. Check if this route was overridden. See the details about impact of changes on this default route at https://docs.microsoft.com/azure/virtual-network/virtual-networks-udr-overview#default-route

See more about connectivity using Public Endpoint at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/public-endpoint-configure
"

$AAD_login_windows_net = ' If you are using AAD Password or AAD Integrated Authentication please make sure you fix the connectivity from this machine to login.windows.net:443
 This usually indicates a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.'

$AAD_login_microsoftonline_com = ' If you are using AAD Universal with MFA authentication please make sure you fix the connectivity from this machine to login.microsoftonline.com:443
 This usually indicates a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.'

$AAD_secure_aadcdn_microsoftonline_p_com = ' If you are using AAD Universal with MFA authentication please make sure you fix the connectivity from this machine to secure.aadcdn.microsoftonline-p.com:443
 This usually indicates a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.'

$error18456RecommendedSolution = ' This error indicates that the login request was rejected, the most common reasons are:
 - Incorrect or empty password: Please ensure that you have provided the correct password.
 - Database does not exist: Please ensure that the connection string has the correct database name.
 - Insufficient permissions: The user does not have CONNECT permissions to the database. Please ensure that the user is granted the necessary permissions to login.
 - Connections rejected due to DoSGuard protection: DoSGuard actively tracks failed logins from IP addresses. If there are multiple failed logins from a specific IP address within a period of time, the IP address is blocked from accessing any resources in the service for a pre-defined time period even if the password and other permissions are correct.'

$ServerNameNotSpecified = ' The parameter $Server was not specified, please set the parameters on the script, you need to set server name. Database name, user and password are optional but desirable.
 You can see more details about how to use this tool at https://github.com/Azure/SQL-Connectivity-Checker'

$followUpMessage = ' If this is a database engine error code you may see more about it at https://docs.microsoft.com/sql/relational-databases/errors-events/database-engine-events-and-errors'

$SQLMI_PrivateEndpoint_Error40532 = " Error 40532 is usually related to one of the following scenarios:
- The username (login) contains the '@' symbol (e.g., a login of the form 'user@mydomain.com').
  You can't currently login with usernames containing these characters. We are working on removing this limitation.
- Trying to connect using the IP address instead of the FQDN of your server.
  Connecting to a managed instance using an IP address is not supported. A Managed Instance's host name maps to the load balancer in front of the Managed Instance's virtual cluster. As one virtual cluster can host multiple Managed Instances, a connection can't be routed to the proper Managed Instance without specifying its name.
- The IP address associated with your managed instance changed but you DNS record still points to previous address.
  The managed instance service doesn't claim static IP address support, we strongly discourage relying on immutability of the IP address as it could cause unnecessary downtime.
"

$SQLDB_Error40532 = ' Error 40532 is usually related to one of the following scenarios:

  - The username (login) contains the "@" symbol (e.g., a login of the form "user@mydomain.com").
    If the {servername} value shown in the error is "mydomain.com" then you are encountering this scenario.
    See how to handle this at https://techcommunity.microsoft.com/t5/azure-database-support-blog/providing-the-server-name-explicitly-in-user-names-for-azure-sql/ba-p/368942

  - The subnet where you are trying to connect from has Microsoft.Sql service endpoint enabled
    Turning on virtual network service endpoints to Microsoft.Sql in the subnet enables the endpoints for Azure SQL Database, Azure Synapse Analytics, Azure Database for PostgreSQL server, Azure Database for MySQL server and Azure Database for MariaDB. Attempts to connect from subnet might fail if virtual network rules are not set.

    This issue is usually originated by one of the following:
    - Aiming to connect to SQL Database using service endpoints, Microsoft.Sql was enabled in the subnet but the virtual network rule for the originating subnet in the Firewalls and virtual networks settings on the server was not added.
    - Aiming to connect to other database service (like Azure Database for MySQL as an example), Azure SQL Database was also impacted.

    To fix this issue create a virtual network rule in your server in SQL Database, for the originating subnet in the Firewalls and virtual networks.
    See how to at https://docs.microsoft.com/azure/azure-sql/database/vnet-service-endpoint-rule-overview#use-the-portal-to-create-a-virtual-network-rule
    You can also consider removing the service endpoint from the subnet, but you will need to take into consideration the impact in all the services mentioned above.'
 
$CannotDownloadAdvancedScript = ' Advanced connectivity policy tests script could not be downloaded!
 Confirm this machine can access https://github.com/Azure/SQL-Connectivity-Checker/
 or use a machine with Internet access to see how to run this from machines without Internet. See how at https://github.com/Azure/SQL-Connectivity-Checker/'

$DNSResolutionDNSfromHostsFile = "We detected a configuration on local cache (while including hosts file), note that Azure SQL Database and Azure Synapse Analytics doesn't have a static IP address.
Logins for Azure SQL Database or Azure Synapse Analytics can land on any of the Gateways in a region.
For this reason, we strongly discourage relying on immutability of the IP address as it could cause unnecessary downtime.
Usage of hosts file should be avoided, local cache refresh rate should be reviewed."

$DNSResolutionDNSfromHostsFileMI = "We detected a configuration on local cache (while including hosts file), note that Managed instance doesn't have a static IP address.
The managed instance service doesn't claim static IP address support and reserves the right to change it without notice as a part of regular maintenance cycles.
For this reason, we strongly discourage relying on immutability of the IP address as it could cause unnecessary downtime.
Usage of hosts file should be avoided, local cache refresh rate should be reviewed."

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
            try {
                Write-Host " Trying to resolve DNS for" $Name
                return @{ IPAddress = [System.Net.DNS]::GetHostAddresses($Name).IPAddressToString };
            }
            catch {
                TrackWarningAnonymously ('Error at Resolve-DnsName override: ' + $_.Exception.Message)
            }
        }
    }
}

if (!$(Get-Command 'Get-NetRoute' -errorAction SilentlyContinue)) {
    function Get-NetRoute {
        param(
            [Parameter(Position = 0, Mandatory = $true)] $InterfaceAlias
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

function PrintDNSResults($dnsResult, [string] $dnsSource, $errorVariable, $Server) {
    Try {
        $dnsResultIpAddress = $null
        if ($errorVariable -and $errorVariable[0].Exception.Message -notmatch 'DNS record does not exist' -and $errorVariable[0].Exception.Message -notmatch 'DNS name does not exist') {
            $msg = ' Error getting DNS record in ' + $dnsSource + ' (' + $errorVariable[0].Exception.Message.Replace(" : " + $Server, "") + ')'
            Write-Host $msg
            [void]$summaryLog.AppendLine($msg)
            TrackWarningAnonymously $msg
        }
        else {
            if ($dnsResult -and $dnsResult.IPAddress -and !([string]::IsNullOrEmpty($dnsResult.IPAddress))) {
                $dnsResultIpAddress = $dnsResult.IPAddress
                $msg = ' Found DNS record in ' + $dnsSource + ' (IP Address:' + $dnsResult.IPAddress + ')'
                Write-Host $msg
                [void]$summaryLog.AppendLine($msg)
            }
            else {
                Write-Host ' Could not find DNS record in' $dnsSource
            }
        }
        return $dnsResultIpAddress
    }
    Catch {
        $msg = "Error at PrintDNSResults for " + $dnsSource + '(' + $_.Exception.Message + ')'
        #Write-Host $msg -Foreground Red
        #Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously $msg
    }
}

function ValidateDNS([String] $Server, [bool]$isManagedInstance) {
    Try {
        Write-Host 'Validating DNS record for' $Server -ForegroundColor Green
        $DNSlist = New-Object Collections.Generic.List[string]

        if ($PSVersionTable.PSVersion.Major -le 5 -or $IsWindows) {
            Try {
                $DNSfromCacheError = $null
                $DNSfromCache = Resolve-DnsName -Name $Server -NoHostsFile -CacheOnly -ErrorAction SilentlyContinue -ErrorVariable DNSfromCacheError
                $DNSfromCacheAddress = PrintDNSResults $DNSfromCache 'local cache (while excluding hosts file)' $DNSfromCacheError $Server
                if ($DNSfromCacheAddress -and -1 -eq $DNSlist.IndexOf($DNSfromCacheAddress)) {
                    $DNSlist.Add($DNSfromCacheAddress);
                }
            }
            Catch {
                Write-Host "Error at ValidateDNS from local cache (while excluding hosts file)" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from local cache (while excluding hosts file)'
            }

            Try {
                $DNSfromHostsError = $null
                $DNSfromHosts = Resolve-DnsName -Name $Server -CacheOnly -ErrorAction SilentlyContinue -ErrorVariable DNSfromHostsError
                $DNSfromHostsAddress = PrintDNSResults $DNSfromHosts 'local cache (while including hosts file)' $DNSfromHostsError $Server
                if ($DNSfromHostsAddress -and -1 -eq $DNSlist.IndexOf($DNSfromHostsAddress)) {
                    $DNSlist.Add($DNSfromHostsAddress);
                }
            }
            Catch {
                Write-Host "Error at ValidateDNS from local cache (while including hosts file)" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from local cache (while including hosts file)'
            }

            Try {
                $DNSfromCustomerServerError = $null
                $DNSfromCustomerServer = Resolve-DnsName -Name $Server -DnsOnly -ErrorAction SilentlyContinue -ErrorVariable DNSfromCustomerServerError
                $DNSfromCustomerServerAddress = PrintDNSResults $DNSfromCustomerServer 'DNS server' $DNSfromCustomerServerError $Server
                if ($DNSfromCustomerServerAddress -and -1 -eq $DNSlist.IndexOf($DNSfromCustomerServerAddress)) {
                    $DNSlist.Add($DNSfromCustomerServerAddress);
                }
            }
            Catch {
                Write-Host "Error at ValidateDNS from DNS server" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from DNS server'
            }

            Try {
                $DNSfromOpenDNSError = $null
                $DNSfromOpenDNS = Resolve-DnsName -Name $Server -DnsOnly -Server 208.67.222.222 -ErrorAction SilentlyContinue -ErrorVariable DNSfromOpenDNSError
                $DNSfromOpenDNSAddress = PrintDNSResults $DNSfromOpenDNS 'Open DNS' $DNSfromOpenDNSError $Server
                if ($DNSfromOpenDNSAddress -and -1 -eq $DNSlist.IndexOf($DNSfromOpenDNSAddress)) {
                    $DNSlist.Add($DNSfromOpenDNSAddress);
                }
            }
            Catch {
                Write-Host "Error at ValidateDNS from Open DNS" -Foreground Red
                Write-Host $_.Exception.Message -ForegroundColor Red
                TrackWarningAnonymously 'Error at ValidateDNS from Open DNS'
            }

            if ($DNSfromHostsAddress) {
                if ($isManagedInstance) {
                    $msg = $DNSResolutionDNSfromHostsFileMI
                }
                else {
                    $msg = $DNSResolutionDNSfromHostsFile
                }
                Write-Host
                Write-Host $msg -ForegroundColor Red
                [void]$summaryLog.AppendLine()
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
            }

            if (!$DNSfromCustomerServerAddress) {
                Write-Host
                $msg = ('DNS resolution using DNS Server could not be verified, please verify if FQDN is valid and address is getting resolved properly.');
                Write-Host $msg -ForegroundColor Red
                [void]$summaryLog.AppendLine()
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously 'EmptyDNSfromCustomerServer'
            }

            if (!$DNSfromOpenDNSAddress) {
                Write-Host
                $msg = ('DNS resolution using an external provider (OpenDNS) could not be verified, please verify if FQDN is valid and address is getting resolved properly.');
                Write-Host $msg -ForegroundColor Red
                [void]$summaryLog.AppendLine()
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously 'EmptyDNSfromOpenDNS'
            }

            $hasPrivateLinkAlias = HasPrivateLinkAlias $Server

            if (($DNSlist.Count -gt 1) -and ($hasPrivateLinkAlias -eq $false)) {
                Write-Host
                $msg = ('WARNING: Distinct DNS records were found! (' + [string]::Join(", ", $DNSlist) + ')');
                Write-Host $msg -ForegroundColor Red
                [void]$summaryLog.AppendLine()
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously $msg

                if ($isManagedInstance) {
                    $msg = $DNSResolutionGotMultipleAddressesMI
                    Write-Host $msg -Foreground Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                }
                else {
                    $msg = $DNSResolutionGotMultipleAddresses
                    Write-Host $msg -Foreground Red
                    [void]$summaryRecommendedAction.AppendLine($msg)
                }
            }
        }
        else {
            Write-Host ' DNS resolution:' ([System.Net.DNS]::GetHostAddresses($Server).IPAddressToString)
        }
    }
    Catch {
        Write-Host "Error at ValidateDNS" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

function IsManagedInstance([String] $Server) {
    $hasKnownFQDN = ($Server -match '.database.windows.net') -or ($Server -match '.database.chinacloudapi.cn') -or ($Server -match '.database.usgovcloudapi.net');
    $has4parts = [bool]((($Server.ToCharArray() | Where-Object { $_ -eq '.' } | Measure-Object).Count) -ge 4);
    $isSQLDBPE = ($Server -match '.privatelink.database.');
    $has3342 = ($Server -match ',3342') -or ($Server -match ', 3342')
    return ($hasKnownFQDN -and $has4parts -and !$isSQLDBPE) -or $has3342
}

function IsSqlOnDemand([String] $Server) {
    return [bool]($Server -match '-ondemand.')
}

function IsManagedInstancePublicEndpoint([String] $Server) {
    $isMI = (IsManagedInstance $Server);
    $hasPublic = ($Server -match '.public.');
    $has3342 = ($Server -match ',3342') -or ($Server -match ', 3342');
    return $isMI -and ($hasPublic -or $has3342)
}

function HasPrivateLinkAlias([String] $Server) {
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

function TestConnectionToDatabase($Server, $gatewayPort, $Database, $AuthenticationType, $AuthenticationLibrary, $User, $Password, $TrustServerCertificate) {
    Write-Host

    if ($AuthenticationType -ne "SQL Server Authentication") {
        [void]$summaryLog.AppendLine()
        Write-Host ([string]::Format("Skipping testing connecting to {0} database at this point since SQL Server Authentication is not being used, ensure you have advanced connectivity tests enabled.", $Database)) -ForegroundColor Green
        return $false
    }

    [void]$summaryLog.AppendLine()
    Write-Host ([string]::Format("Testing connecting to {0} database (please wait):", $Database)) -ForegroundColor Green
    Try {
        $DbConnection = [System.Data.SqlClient.SQLConnection]::new()
        $DbConnection.ConnectionString = GetConnectionString $Server $gatewayPort $Database $User $Password $TrustServerCertificate
        $DbConnection.Open()
        Write-Host ([string]::Format(" The connection attempt succeeded", $Database))
        [void]$summaryLog.AppendLine([string]::Format(" The connection attempt to {0} database succeeded", $Database))
        return $true
    }
    catch [System.Data.SqlClient.SqlException] {
        $ex = $_.Exception
        Switch ($_.Exception.Number) {
            121 {
                $msg = ' Connection to database ' + $Database + ' failed due to "The semaphore timeout period has expired" error.'
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine('  "The semaphore timeout period has expired" is a network error, not a SQL timeout.')
                [void]$summaryRecommendedAction.AppendLine('  This appears as a SQL error because Windows passes this to the SQL process, so it is often mistaken to be a SQL error, when it is a client operating system level error.')
                [void]$summaryRecommendedAction.AppendLine('  This error can occur for a very wide variety of reasons, but are typically due to a network or driver-related issue.')

                [void]$summaryRecommendedAction.AppendLine('  We suggest you:')
                [void]$summaryRecommendedAction.AppendLine('  - Verify if you are using an updated version of the client driver or tool.')
                [void]$summaryRecommendedAction.AppendLine('  - Verify if you can connect using a different client driver or tool.')
                if (IsManagedInstance $Server ) {
                    [void]$summaryRecommendedAction.AppendLine( '  See required versions of drivers and tools at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/connect-application-instance#required-versions-of-drivers-and-tools')
                }
                [void]$summaryRecommendedAction.AppendLine('  - Check with your local network administrator for client-side networking issues.')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error121 State' + $ex.State)
            }
            916 {
                $msg = ' Connection to database ' + $Database + ' failed, the login does not have sufficient permissions to connect to the named database.'
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine(' See more details and how to fix this error at https://docs.microsoft.com/sql/relational-databases/errors-events/mssqlserver-916-database-engine-error')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error916 State' + $ex.State)
            }
            10060 {
                $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine(' This usually indicates a client-side networking issue (like DNS issue or a port being blocked) that you will need to pursue with your local network administrator.')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error10060 State' + $ex.State)
            }
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
                if (IsManagedInstance $Server ) {
                    if ($gatewayPort -eq 3342) {
                        $msg = ' You seem to be trying to connect to MI using Public Endpoint but Public Endpoint may be disabled'
                        Write-Host ($msg) -ForegroundColor Red
                        [void]$summaryLog.AppendLine($msg)
                        [void]$summaryRecommendedAction.AppendLine($msg)

                        $msg = ' Learn how to configure public endpoint at https://docs.microsoft.com/en-us/azure/sql-database/sql-database-managed-instance-public-endpoint-configure'
                        Write-Host ($msg) -ForegroundColor Red
                        [void]$summaryRecommendedAction.AppendLine($msg)
                        TrackWarningAnonymously ('SQLMI|PublicEndpoint|Error40532 State' + $ex.State)
                    }
                    else {
                        $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                        Write-Host ($msg) -ForegroundColor Red
                        [void]$summaryLog.AppendLine($msg)
                        [void]$summaryRecommendedAction.AppendLine()
                        [void]$summaryRecommendedAction.AppendLine($msg)
                        [void]$summaryRecommendedAction.AppendLine($SQLMI_PrivateEndpoint_Error40532)
                        TrackWarningAnonymously ('SQLMI|PrivateEndpoint|Error40532 State' + $ex.State)
                    }
                }
                else {
                    $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                    Write-Host ($msg) -ForegroundColor Red
                    [void]$summaryLog.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine()
                    [void]$summaryRecommendedAction.AppendLine($msg)
                    [void]$summaryRecommendedAction.AppendLine($SQLDB_Error40532)
                    TrackWarningAnonymously ('SQLDB|Error40532 State' + $ex.State)
                }
            }
            40615 {
                $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine('  The client is trying to connect from an IP address that is not authorized to connect to the server. The server firewall has no IP address rule that allows a client to communicate from the given IP address to the database.')
                [void]$summaryRecommendedAction.AppendLine('  Add the IP address as an IP rule, see how at https://docs.microsoft.com/en-us/azure/azure-sql/database/firewall-configure')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error40615 State' + $ex.State)
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
                TrackWarningAnonymously ('TestConnectionToDatabase|47073 State' + $ex.State)
            }
            40914 {
                $msg = ' Connection to database ' + $Database + ' failed, client is not allowed to access the server.'
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine('  The client is in a subnet that has virtual network server endpoints. But the server has no virtual network rule that grants to the subnet the right to communicate with the database.')
                [void]$summaryRecommendedAction.AppendLine('  On the Firewall pane of the Azure portal, use the virtual network rules control to add a virtual network rule for the subnet.')
                [void]$summaryRecommendedAction.AppendLine('  See how at https://docs.microsoft.com/en-us/azure/azure-sql/database/vnet-service-endpoint-rule-overview#use-the-portal-to-create-a-virtual-network-rule')
                TrackWarningAnonymously ('TestConnectionToDatabase|Error40914 State' + $ex.State)
            }
            default {
                $msg = ' Connection to database ' + $Database + ' failed (error ' + $ex.Number + ', state ' + $ex.State + '): ' + $ex.Message
                Write-Host ($msg) -ForegroundColor Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($followUpMessage)
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

function GetConnectionString ($Server, $gatewayPort, $Database, $User, $Password, $TrustServerCertificate) {
    return [string]::Format("Server=tcp:{0},{1};Initial Catalog={2};Persist Security Info=False;User ID='{3}';Password='{4}';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate={5};Connection Timeout=30;Application Name=Azure-SQL-Connectivity-Checker;",
        $Server, $gatewayPort, $Database, $User, $Password, $TrustServerCertificate.ToString())
}

function PrintSupportedCiphers() {
    Try {
        if ( ($PSVersionTable.PSVersion.Major -le 5 ) -or ($PSVersionTable.Platform -eq 'Windows')) {
            $suites = Get-TlsCipherSuite | Format-Table Name -HideTableHeaders -Wrap -AutoSize | Out-String -Width 4096
            Write-Host
            Write-Host 'Client Tls Cipher Suites:'
            Write-Host $suites.Trim()
            Write-Host

            $suites = Get-TlsCipherSuite
            $supportedSuites = @(
                'TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384',
                'TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256',
                'TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384',
                'TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256',
                'TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA',
                'TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA',
                'TLS_RSA_WITH_AES_256_CBC_SHA256',
                'TLS_RSA_WITH_AES_128_CBC_SHA256',
                'TLS_RSA_WITH_AES_256_CBC_SHA',
                'TLS_RSA_WITH_AES_128_CBC_SHA',
                'TLS_RSA_WITH_3DES_EDE_CBC_SHA'
            )

            if (($suites | Where-Object Name -in $supportedSuites | Measure-Object).Count -eq 0) {
                Write-Host
                $msg = "WARNING: Client machine may not have any supported cipher suite enabled!"
                Write-Host $msg -Foreground Red
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                Write-Host
                Write-Host
                $msg = 'Supported Tls Cipher Suites:'
                Write-Host $msg
                [void]$summaryRecommendedAction.AppendLine($msg)
                $msg = $supportedSuites | Format-Table Name -HideTableHeaders -Wrap -AutoSize | Out-String -Width 4096
                Write-Host $msg
                [void]$summaryRecommendedAction.AppendLine($msg)

                TrackWarningAnonymously 'TLS|CipherSuites|Unsupported'
            }
        }
    }
    Catch {
        Write-Host "Error at PrintSupportedCiphers" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously 'PrintSupportedCiphers|Exception'
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

function RunFabricConnectivityTests($resolvedAddress) {
    Try {
        $msg = 'Detected as Microsoft Fabric'
        Write-Host $msg -ForegroundColor Yellow
        [void]$summaryLog.AppendLine($msg)

        Write-Host 'Microsoft Fabric connectivity test:' -ForegroundColor Green
        $testResult = Test-NetConnection $resolvedAddress -Port 1433 -WarningAction SilentlyContinue

        if ($testResult.TcpTestSucceeded) {
            Write-Host ' -> TCP test succeed' -ForegroundColor Green
            PrintAverageConnectionTime $resolvedAddress 1433
            $msg = ' Connectivity to ' + $resolvedAddress + ':1433 succeed'
            [void]$summaryLog.AppendLine($msg)
            TrackWarningAnonymously 'Fabric|Endpoint|TestSucceeded'
            RunConnectionToDatabaseTestsAndAdvancedTests $Server '1433' $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            $msg = ' Connectivity to ' + $resolvedAddress + ':1433 FAILED'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            $msg = ' Please make sure you fix the connectivity from this machine to ' + $resolvedAddress + ':1433'
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'Fabric|Endpoint|TestFailed'
        }
    }
    Catch {
        Write-Host "Error at RunFabricConnectivityTests" -Foreground Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        TrackWarningAnonymously 'RunFabricConnectivityTests|Exception'
    }
}

function RunSqlMIPublicEndpointConnectivityTests($resolvedAddress) {
    Try {
        $msg = 'Detected as Managed Instance using Public Endpoint'
        Write-Host $msg -ForegroundColor Yellow
        [void]$summaryLog.AppendLine($msg)

        Write-Host 'Public Endpoint connectivity test:' -ForegroundColor Green
        $testResult = Test-NetConnection $resolvedAddress -Port 3342 -WarningAction SilentlyContinue

        if ($testResult.TcpTestSucceeded) {
            Write-Host ' -> TCP test succeed' -ForegroundColor Green
            PrintAverageConnectionTime $resolvedAddress 3342
            $msg = ' Gateway connectivity to ' + $resolvedAddress + ':3342 succeed'
            [void]$summaryLog.AppendLine($msg)
            TrackWarningAnonymously 'SQLMI|PublicEndpoint|GatewayTestSucceeded'
            RunConnectionToDatabaseTestsAndAdvancedTests $Server '3342' $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate
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

            TrackWarningAnonymously 'SQLMI|PublicEndpoint|GatewayTestFailed'
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
        $hasPrivateLinkAlias = HasPrivateLinkAlias $Server
        if ($hasPrivateLinkAlias) {
            Write-Host ' This instance has a privatelink alias, confirm if IP is resolving to privatelink or regular VNet internal endpoint' -ForegroundColor Yellow
            TrackWarningAnonymously 'SQLMI|PrivateLinkAliasExists'
        }
        Write-Host
        Write-Host 'Gateway connectivity tests (please wait):' -ForegroundColor Green
        $testResult = Test-NetConnection $resolvedAddress -Port 1433 -WarningAction SilentlyContinue

        if ($testResult.TcpTestSucceeded) {
            Write-Host ' -> TCP test succeed' -ForegroundColor Green
            PrintAverageConnectionTime $resolvedAddress 1433
            TrackWarningAnonymously 'SQLMI|PrivateEndpoint|GatewayTestSucceeded'
            RunConnectionToDatabaseTestsAndAdvancedTests $Server '1433' $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate
            return $true
        }
        else {
            Write-Host ' -> TCP test FAILED' -ForegroundColor Red
            Write-Host
            Write-Host ' Trying to get IP routes for interface:' $testResult.InterfaceAlias
            Get-NetRoute -InterfaceAlias $testResult.InterfaceAlias -ErrorAction SilentlyContinue -ErrorVariable ProcessError
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

            TrackWarningAnonymously 'SQLMI|PrivateEndpoint|GatewayTestFailed'
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
    Write-Host ' Average TCP connection times:'
    $stopwatch = [StopWatch]::new()

    foreach ($ipAddress in $addressList) {
        [double]$sum = 0
        [int]$numFailed = 0
        [int]$numSuccessful = 0
        Write-Host '   IP Address:'$ipAddress'  Port:'$port

        for ($i = 0; $i -lt 5; $i++) {
            $client = [TcpClient]::new()
            try {
                $stopwatch.Restart()
                $client.Connect($ipAddress, $port)
                $stopwatch.Stop()
                Write-Host '    Opening a new TCP connection took:'$stopwatch.ElapsedMilliseconds' ms'
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
        Write-Host '   Summary'
        Write-Host '    Successful connections:'$numSuccessful
        Write-Host '    Failed connections:'$numFailed
        Write-Host '    Average response time:'$avg' ms '$ilb
        TrackWarningAnonymously ('Average response time:' + $avg + ' ms ' + $ilb + '| Successful connections:' + $numSuccessful + '| Failed connections:' + $numFailed)
    }
}

function RunSqlDBConnectivityTests($resolvedAddress) {

    if (IsSqlOnDemand $Server) {
        Write-Host 'Detected as SQL on-demand endpoint' -ForegroundColor Yellow
        TrackWarningAnonymously 'SQL on-demand'
    }
    else {
        Write-Host 'Detected as SQL Database/Azure Synapse' -ForegroundColor Yellow
        TrackWarningAnonymously 'SQL DB/DW'
    }

    $hasPrivateLinkAlias = HasPrivateLinkAlias $Server
    $gateway = $SQLDBGateways | Where-Object { $_.Gateways -eq $resolvedAddress }

    if (!$gateway) {
        if ($hasPrivateLinkAlias) {
            Write-Host ' This connection seems to be using Private Link, skipping Gateway connectivity tests' -ForegroundColor Yellow
            TrackWarningAnonymously 'SQLDB|PrivateLink'
        }
        else {
            $msg = ' WARNING: ' + $resolvedAddress + ' is not a valid gateway address'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine()
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = $SQLDB_InvalidGatewayIPAddress
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)

            TrackWarningAnonymously 'SQLDB|InvalidGatewayIPAddressWarning'
        }

        RunConnectionToDatabaseTestsAndAdvancedTests $Server '1433' $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate
    }
    else {
        if ($hasPrivateLinkAlias) {
            $msg = ' This server has a privatelink alias but DNS is resolving to a regular Gateway IP address, running public endpoint tests.'
            Write-Host $msg -Foreground Yellow
            [void]$summaryLog.AppendLine()
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'SQLDB|HasPEbutDNSresolvesToGW'
        }

        Write-Host ' The server' $Server 'is running on ' -ForegroundColor White -NoNewline
        Write-Host $gateway.Region -ForegroundColor Yellow

        Write-Host
        [void]$summaryLog.AppendLine()
        Write-Host 'Gateway connectivity tests (please wait):' -ForegroundColor Green
        $hasGatewayTestSuccess = $false
        foreach ($gatewayAddress in $gateway.Gateways) {
            Write-Host
            Write-Host ' Testing (gateway) connectivity to' $gatewayAddress':1433' -ForegroundColor White -NoNewline
            $testResult = Test-NetConnection $gatewayAddress -Port 1433 -WarningAction SilentlyContinue

            if ($testResult.TcpTestSucceeded) {
                $hasGatewayTestSuccess = $true
                Write-Host ' -> TCP test succeed' -ForegroundColor Green
                TrackWarningAnonymously ('SQLDB|GatewayTestSucceeded|' + $gatewayAddress)
                PrintAverageConnectionTime $gatewayAddress 1433
                $msg = ' Gateway connectivity to ' + $gatewayAddress + ':1433 succeed'
                [void]$summaryLog.AppendLine($msg)
            }
            else {
                Write-Host ' -> TCP test FAILED' -ForegroundColor Red
                Write-Host
                Write-Host ' IP routes for interface:' $testResult.InterfaceAlias
                Get-NetRoute -InterfaceAlias $testResult.InterfaceAlias -ErrorAction SilentlyContinue -ErrorVariable ProcessError
                If ($ProcessError) {
                    Write-Host '  Could not to get IP routes for this interface'
                }
                Write-Host
                if ($PSVersionTable.PSVersion.Major -le 5 -or $IsWindows) {
                    tracert -h 10 $Server
                }

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

                TrackWarningAnonymously ('SQLDB|GatewayTestFailed|' + $gatewayAddress)
            }
        }

        if ($hasGatewayTestSuccess -eq $true) {
            RunConnectionToDatabaseTestsAndAdvancedTests $Server '1433' $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate
        }
    }
}

function RunConnectivityPolicyTests($port) {
    try {
        Write-Host
        Write-Host 'Advanced connectivity policy tests (please wait):' -ForegroundColor Green

        if ($(Get-ExecutionPolicy) -eq 'Restricted') {
            $msg = ' Advanced connectivity policy tests cannot be run because of current execution policy (Restricted)!
 Please use Set-ExecutionPolicy to allow scripts to run on this system!'
            Write-Host $msg -Foreground Yellow
            [void]$summaryLog.AppendLine()
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine()
            [void]$summaryRecommendedAction.AppendLine($msg)

            TrackWarningAnonymously 'Advanced|RestrictedExecutionPolicy'
            return
        }

        $jobParameters = @{
            Server                       = $Server
            Database                     = $Database
            Port                         = $port
            AuthenticationType           = $AuthenticationType
            AuthenticationLibrary        = $AuthenticationLibrary
            UserAssignedIdentityClientId = $UserAssignedIdentityClientId
            User                         = $User
            Password                     = $Password
            EncryptionProtocol           = $EncryptionProtocol
            RepositoryBranch             = $RepositoryBranch
            Local                        = $Local
            LocalPath                    = $LocalPath
            SendAnonymousUsageData       = $SendAnonymousUsageData
            AnonymousRunId               = $AnonymousRunId
            logsFolderName               = $logsFolderName
            outFolderName                = $outFolderName
            ConnectionAttempts           = $ConnectionAttempts
            DelayBetweenConnections      = $DelayBetweenConnections
            TrustServerCertificate       = $TrustServerCertificate
        }

        if ($Local) {
            Copy-Item -Path $($LocalPath + '\AdvancedConnectivityPolicyTests.ps1') -Destination ".\AdvancedConnectivityPolicyTests.ps1"
        }
        ### USING GITHUB HERE
        else {
            try {
                [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls
                Invoke-WebRequest -Uri $('https://raw.githubusercontent.com/Azure/SQL-Connectivity-Checker/' + $RepositoryBranch + '/AdvancedConnectivityPolicyTests.ps1') -OutFile ".\AdvancedConnectivityPolicyTests.ps1" -UseBasicParsing
            }
            catch {
                $msg = $CannotDownloadAdvancedScript
                Write-Host $msg -Foreground Yellow
                [void]$summaryLog.AppendLine()
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine()
                [void]$summaryRecommendedAction.AppendLine($msg)
                TrackWarningAnonymously 'Advanced|CannotDownloadScript'
                return
            }
        }

        TrackWarningAnonymously 'Advanced|Invoked'
        $job = Start-Job -ArgumentList $jobParameters -FilePath ".\AdvancedConnectivityPolicyTests.ps1"
        Wait-Job $job | Out-Null
        Receive-Job -Job $job

        Set-Location -Path $env:TEMP
        Set-Location $logsFolderName
        Set-Location $outFolderName
        $logPath = Join-Path ((Get-Location).Path) 'AdvancedTests_LastRunLog.txt'
        $result = $([System.IO.File]::ReadAllText($logPath))
        $routingMatch = [Regex]::Match($result, "Routing to: (.*)\.")

        if ($routingMatch.Success) {
            $routingArray = $routingMatch.Groups[1].Value -split ':'
            $routingServer = $routingArray[0]
            $routingPort = $routingArray[1]
            $networkingErrorMatch = [Regex]::Match($result, "Networking error 10060 while trying to connect to (.*)\.")
            $networkingErrorArray = $networkingErrorMatch.Groups[1].Value -split ':'
            $networkingErrorServer = $networkingErrorArray[0]
            $networkingErrorPort = $networkingErrorArray[1]

            if ($networkingErrorMatch.Success -and ($routingServer -ieq $networkingErrorServer) -and ($routingPort -ieq $networkingErrorPort)) {
                [void]$summaryLog.AppendLine()
                [void]$summaryRecommendedAction.AppendLine()
                $msg = "ROOT CAUSE:"
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                $msg = "The issue is caused by lack of direct network connectivity to the node hosting the database under REDIRECT connection type."
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                $msg = [string]::Format("This machine cannot connect to {0} on port {1}", $networkingErrorServer, $networkingErrorPort);
                [void]$summaryLog.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine($msg)
                [void]$summaryRecommendedAction.AppendLine('This indicates a client-side networking issue (usually a port being blocked) that you will need to pursue with your local network administrator.')
                if (IsManagedInstance $Server ) {
                    [void]$summaryRecommendedAction.AppendLine('Make sure firewalls and Network Security Groups (NSG) are open to allow access on ports 11000-11999')
                    [void]$summaryRecommendedAction.AppendLine('Check more about connection types at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/connection-types-overview')
                    TrackWarningAnonymously ('Advanced|SQLMI|RCA|Port' + $networkingErrorPort)
                }
                else {
                    [void]$summaryRecommendedAction.AppendLine('Make sure you allow outbound communication from the client to all Azure SQL IP addresses in the region on ports in the range of 11000-11999.')
                    [void]$summaryRecommendedAction.AppendLine('Check more about connection policies at https://docs.microsoft.com/en-us/azure/azure-sql/database/connectivity-architecture#connection-policy')
                    TrackWarningAnonymously ('Advanced|SQLDB|RCA|Port' + $networkingErrorPort)
                }
            }
        }
        Remove-Item ".\AdvancedConnectivityPolicyTests.ps1" -Force
    }
    catch {
        $msg = ' ERROR running Advanced Connectivity Tests: ' + $_.Exception.Message
        Write-Host $msg -Foreground Red
        [void]$summaryLog.AppendLine()
        [void]$summaryLog.AppendLine($msg)
        TrackWarningAnonymously 'ERROR running Advanced Connectivity Test'
    }
}

function LookupDatabaseInSysDatabases($Server, $dbPort, $Database, $AuthenticationType, $User, $Password) {
    Write-Host
    [void]$summaryLog.AppendLine()
    Write-Host ([string]::Format("Testing connecting to {0} database (please wait):", $Database)) -ForegroundColor Green
    Try {
        Write-Host ' Checking if' $Database 'exist in sys.databases:' -ForegroundColor White
        $masterDbConnection = [System.Data.SqlClient.SQLConnection]::new()
        $masterDbConnection.ConnectionString = GetConnectionString $Server $dbPort $Database $User $Password $TrustServerCertificate
        $masterDbConnection.Open()

        $masterDbCommand = New-Object System.Data.SQLClient.SQLCommand
        $masterDbCommand.Connection = $masterDbConnection

        $masterDbCommand.CommandText = "select count(*) C from sys.databases where name = '" + $Database + "'"
        $masterDbResult = $masterDbCommand.ExecuteReader()
        $masterDbResultDataTable = new-object 'System.Data.DataTable'
        $masterDbResultDataTable.Load($masterDbResult)

        return $masterDbResultDataTable.Rows[0].C -ne 0
    }
    Catch {
        Write-Host $_.Exception.Message -ForegroundColor Yellow
        TrackWarningAnonymously 'LookupDatabaseInSysDatabases|Exception'
        return $false
    }
}

function RunConnectionToDatabaseTestsAndAdvancedTests($Server, $dbPort, $Database, $AuthenticationType, $AuthenticationLibrary, $User, $Password, $TrustServerCertificate) {
    try {
        $customDatabaseNameWasSet = $Database -and $Database.Length -gt 0 -and $Database -ne 'master'

        #Test master database
        $canConnectToMaster = TestConnectionToDatabase $Server $dbPort 'master' $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate

        if ($customDatabaseNameWasSet) {
            if ($canConnectToMaster) {
                $databaseFound = LookupDatabaseInSysDatabases $Server $dbPort $Database $AuthenticationType $User $Password

                if ($databaseFound -eq $true) {
                    $msg = '  ' + $Database + ' was found in sys.databases of master database'
                    Write-Host $msg -Foreground Green
                    [void]$summaryLog.AppendLine($msg)

                    #Test database from parameter
                    if ($customDatabaseNameWasSet) {
                        TestConnectionToDatabase $Server $dbPort $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate | Out-Null
                    }
                }
                else {
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
            }
            else {
                #Test database from parameter anyway
                if ($customDatabaseNameWasSet) {
                    TestConnectionToDatabase $Server $dbPort $Database $AuthenticationType $AuthenticationLibrary $User $Password $TrustServerCertificate | Out-Null
                }
            }
        }

        # Advanced Connectivity Tests
        if ($RunAdvancedConnectivityPolicyTests) {
            RunConnectivityPolicyTests $dbPort
        }
    }
    catch {
        $msg = ' ERROR at RunConnectionToDatabaseTestsAndAdvancedTests: ' + $_.Exception.Message
        Write-Host $msg -Foreground Red
        [void]$summaryLog.AppendLine()
        [void]$summaryLog.AppendLine($msg)
        TrackWarningAnonymously 'ERROR at RunConnectionToDatabaseTestsAndAdvancedTests'
    }
}

function TrackWarningAnonymously ([String] $warningCode) {
    Try {
        if ($SendAnonymousUsageData) {
            $body = New-Object PSObject `
            | Add-Member -PassThru NoteProperty name 'Microsoft.ApplicationInsights.Event' `
            | Add-Member -PassThru NoteProperty time $([System.dateTime]::UtcNow.ToString('o')) `
            | Add-Member -PassThru NoteProperty iKey "26c1eb99-f1b2-4ad7-a601-bfe5775581ab" `
            | Add-Member -PassThru NoteProperty tags (New-Object PSObject | Add-Member -PassThru NoteProperty 'ai.user.id' $AnonymousRunId) `
            | Add-Member -PassThru NoteProperty data (New-Object PSObject `
                | Add-Member -PassThru NoteProperty baseType 'EventData' `
                | Add-Member -PassThru NoteProperty baseData (New-Object PSObject `
                    | Add-Member -PassThru NoteProperty ver 2 `
                    | Add-Member -PassThru NoteProperty name $warningCode));
            $body = $body | ConvertTo-JSON -depth 5;
            Invoke-WebRequest -Uri 'https://dc.services.visualstudio.com/v2/track' -ErrorAction SilentlyContinue -Method 'POST' -UseBasicParsing -body $body > $null
        }
    }
    Catch {
        #Write-Host 'TrackWarningAnonymously exception:'
        #Write-Host $_.Exception.Message -ForegroundColor Red
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

    TrackWarningAnonymously 'v2.8'
    TrackWarningAnonymously ('PowerShell ' + $PSVersionTable.PSVersion + '|' + $PSVersionTable.Platform + '|' + $PSVersionTable.OS )

    try {
        Write-Host '******************************************' -ForegroundColor Green
        Write-Host '  Azure SQL Connectivity Checker v2.8  ' -ForegroundColor Green
        Write-Host '******************************************' -ForegroundColor Green
        Write-Host
        Write-Host 'Parameters' -ForegroundColor Yellow

        if ($AuthenticationType -NotIn "Microsoft Entra Password", "Microsoft Entra Integrated", "Microsoft Entra Interactive", "Microsoft Entra Managed Identity", "Microsoft Entra MSI", "SQL Server Authentication") {
            $msg = "Authentication type " + $AuthenticationType + " is not supported, switching to SQL Server Authentication"
            Write-Host $msg -ForegroundColor Green
            $AuthenticationType = "SQL Server Authentication"
        }

        if ($AuthenticationType -in ("Microsoft Entra Password", "Microsoft Entra Interactive", "Microsoft Entra Managed Identity", "Microsoft Entra MSI") -and $AuthenticationLibrary -NotIn ("ADAL", "MSAL")) {
            $msg = $AuthenticationLibrary + "authentication library is not supported with " + $AuthenticationType + " authentication, switching to MSAL library"
            Write-Host $msg -ForegroundColor Green
            $AuthenticationLibrary = "MSAL"
        }

        if (($AuthenticationType -eq "Microsoft Entra Password" -or $AuthenticationType -eq "Microsoft Entra Interactive" -or $AuthenticationType -eq "Microsoft Entra Managed Identity" -or $AuthenticationType -eq "Microsoft Entra MSI") -and $AuthenticationLibrary -eq "ADAL") {
            $msg = $AuthenticationType + " authentication is not supported with ADAL library, switching to MSAL library"
            Write-Host $msg -ForegroundColor Green
            $AuthenticationLibrary = "MSAL"
        }

        Write-Host ' Authentication type:' $AuthenticationType -ForegroundColor Yellow
        TrackWarningAnonymously ('Authentication type:' + $AuthenticationType)

        if ($AuthenticationType -like "*Microsoft Entra*") {
            Write-Host ' Authentication library:' $AuthenticationLibrary -ForegroundColor Yellow
            TrackWarningAnonymously ('Authentication library:' + $AuthenticationLibrary)
        }

        Write-Host ' Server:' $Server -ForegroundColor Yellow

        if ($null -ne $Database) {
            Write-Host ' Database:' $Database -ForegroundColor Yellow
        }
        if ($null -ne $RunAdvancedConnectivityPolicyTests) {
            Write-Host ' RunAdvancedConnectivityPolicyTests:' $RunAdvancedConnectivityPolicyTests -ForegroundColor Yellow
            TrackWarningAnonymously ('RunAdvancedConnectivityPolicyTests:' + $RunAdvancedConnectivityPolicyTests)
        }
        if ($null -ne $CollectNetworkTrace) {
            Write-Host ' CollectNetworkTrace:' $CollectNetworkTrace -ForegroundColor Yellow
            TrackWarningAnonymously ('CollectNetworkTrace:' + $CollectNetworkTrace)
        }
        if ($null -ne $EncryptionProtocol) {
            Write-Host ' EncryptionProtocol:' $EncryptionProtocol -ForegroundColor Yellow
            TrackWarningAnonymously ('EncryptionProtocol:' + $EncryptionProtocol)
        }
        if ($null -ne $ConnectionAttempts) {
            Write-Host ' ConnectionAttempts:' $ConnectionAttempts -ForegroundColor Yellow
            TrackWarningAnonymously ('ConnectionAttempts:' + $ConnectionAttempts)
        }
        if ($null -ne $DelayBetweenConnections) {
            Write-Host ' DelayBetweenConnections:' $DelayBetweenConnections -ForegroundColor Yellow
            TrackWarningAnonymously ('DelayBetweenConnections:' + $DelayBetweenConnections)
        }
        Write-Host

        $Server = $Server.Trim()

        $isManagedInstance = (IsManagedInstance $Server);
        $isManagedInstancePublicEndpoint = (IsManagedInstancePublicEndpoint $Server);

        if ($isManagedInstancePublicEndpoint -and !($Server -match ',3342')) {
            $msg = ' You seem to be trying to connect using SQL MI Public Endpoint but port 3342 was not specified'

            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = ' Note that the public endpoint host name comes in the format <mi_name>.public.<dns_zone>.database.windows.net and that the port used for the connection is 3342.
                    Please specify port 3342 by setting Server parameter like: <mi_name>.public.<dns_zone>.database.windows.net,3342'
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'ManagedInstancePublicEndpoint|WrongPort'
            Write-Error '' -ErrorAction Stop
        }

        if ($isManagedInstance -and !$isManagedInstancePublicEndpoint -and ($Server -match ',3342')) {
            $msg = ' You seem to be trying to connect using SQLMI Private Endpoint but using Public Endpoint port number (3342)'

            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine($msg)

            $msg = ' The private endpoint host name comes in the format <mi_name>.<dns_zone>.database.windows.net and the port used for the connection is 1433.
 Please specify port 1433 by setting Server parameter like: <mi_name>.<dns_zone>.database.windows.net,1433 (or do not specify any port number).
 In case you are trying to use Public Endpoint, note that:
 - the public endpoint host name comes in the format <mi_name>.public.<dns_zone>.database.windows.net
 - the port used for the connection is 3342.'
            Write-Host $msg -Foreground Red
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'ManagedInstancePrivateEndpoint|WrongPort'
            Write-Error '' -ErrorAction Stop
        }

        $Server = $Server.Replace('tcp:', '')
        $Server = $Server.Replace(',1433', '')
        $Server = $Server.Replace(',3342', '')
        $Server = $Server.Replace(';', '')

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
                -and !$Server.EndsWith('.sql.azuresynapse.net') `
                -and !$Server.EndsWith('.fabric.microsoft.com')) {
            $msg = 'You seem to be using a custom domain, if not, please provide the FQDN like server1.database.windows.net'
            Write-Host $msg -Foreground Yellow
            [void]$summaryLog.AppendLine($msg)
            [void]$summaryRecommendedAction.AppendLine($msg)
            TrackWarningAnonymously 'CustomDomain'
        }

        #Print local network configuration
        PrintLocalNetworkConfiguration

        #Print supported ciphers
        PrintSupportedCiphers

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

        ValidateDNS $Server $isManagedInstance

        try {
            $dnsResult = [System.Net.DNS]::GetHostEntry($Server)
        }
        catch {
            $msg = ' ERROR: Name resolution (DNS) of ' + $Server + ' failed'
            Write-Host $msg -Foreground Red
            [void]$summaryLog.AppendLine($msg)

            if ($isManagedInstancePublicEndpoint) {
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

        Write-Host
        [void]$summaryLog.AppendLine()
        Write-Host 'Test endpoints for AAD Password and Integrated Authentication:' -ForegroundColor Green
        Write-Host ' Tested connectivity to login.windows.net:443' -ForegroundColor White -NoNewline
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $portOpen = $tcpClient.ConnectAsync("login.windows.net", 443).Wait(10000)
        if ($portOpen) {
            Write-Host ' -> TCP test succeeded' -ForegroundColor Green
            $msg = ' Connectivity to login.windows.net:443 succeeded (used for AAD Password and Integrated Authentication)'
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
            $msg = ' Connectivity to login.microsoftonline.com:443 succeeded (used for AAD Universal with MFA authentication)'
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

        #Run connectivity tests
        Write-Host

        if ($Server -match '.fabric.microsoft.com') {
            RunFabricConnectivityTests $resolvedAddress
        }
        else {
            if ($isManagedInstance) {
                if ($isManagedInstancePublicEndpoint) {
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
            Write-Host ' We could not detect any issue while using SqlClient driver, we suggest you:' -ForegroundColor Green
            Write-Host ' - Verify if you are using an updated version of the client driver or tool.' -ForegroundColor Yellow
            Write-Host ' - Verify if you can connect using a different client driver or tool.' -ForegroundColor Yellow

            if (IsManagedInstance $Server ) {
                Write-Host ' See required versions of drivers and tools at https://docs.microsoft.com/en-us/azure/azure-sql/managed-instance/connect-application-instance#required-versions-of-drivers-and-tools' -ForegroundColor Yellow
            }

            Write-Host ' - Verify your connection string and credentials.' -ForegroundColor Yellow
            Write-Host ' See more at https://docs.microsoft.com/en-us/azure/azure-sql/database/connect-query-content-reference-guide' -ForegroundColor Yellow
            Write-Host
            Write-Host 'If you have any feedback/issue/request let us know at https://github.com/Azure/SQL-Connectivity-Checker/issues' -ForegroundColor Green

            TrackWarningAnonymously 'NoRecommendedActions2'
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
            Compress-Archive -Path ((Get-Location).Path + '/*.txt'), ((Get-Location).Path + '/*.cab'), ((Get-Location).Path + '/*.etl') -DestinationPath $destAllFiles -Force
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
