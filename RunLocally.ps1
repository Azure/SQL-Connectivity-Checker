#Run locally parameters
$LocalPath = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)

# Script parameters
$parameters = @{
    Server = '.database.windows.net'
    Database = ''  # Set the name of the database you wish to test, 'master' will be used by default if nothing is set
    User = ''  # Set the login username you wish to use, 'AzSQLConnCheckerUser' will be used by default if nothing is set
    Password = ''  # Set the login password you wish to use, 'AzSQLConnCheckerPassword' will be used by default if nothing is set

    ## Optional parameters (default values will be used if ommited)
    SendAnonymousUsageData = $true  # Set as $true (default) or $false
    RunAdvancedConnectivityPolicyTests = $true  # Set as $true (default) or $false, this will download the library needed for running advanced connectivity tests
    CollectNetworkTrace = $true  # Set as $true (default) or $false
    #EncryptionProtocol = '' # Supported values: 'Tls 1.0', 'Tls 1.1', 'Tls 1.2'; Without this parameter operating system will choose the best protocol to use

    ## Run locally parameters
    Local = $true # Do Not Change
    LocalPath = $LocalPath # Do Not Change
}

$ProgressPreference = "SilentlyContinue";
Invoke-Command -ScriptBlock ([Scriptblock]::Create($($LocalPath + './AzureSQLConnectivityChecker.ps1 $parameters')))
#end