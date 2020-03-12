# AzureSQLConnectivityChecker

This PowerShell script will run some connectivity checks from this machine to the server and database.

**In order to run it you need to:**
1. Open Windows PowerShell ISE

2. Open a New Script window

3. Paste the following in the script window:

```powershell
$parameters = @{
    Server = '.database.windows.net'
    Database = ''  # Set the name of the database you wish to test, 'master' will be used by default if nothing is set
    User = ''  # Set the login username you wish to use, 'AzSQLConnCheckerUser' will be used by default if nothing is set
    Password = ''  # Set the login password you wish to use, 'AzSQLConnCheckerPassword' will be used by default if nothing is set

    ## Optional parameters (default values will be used if ommited)
    SendAnonymousUsageData = $true  # Set as $true (default) or $false
    RunAdvancedConnectivityPolicyTests = $true  # Set as $true (default) or $false, this will download library needed for running advanced connectivity tests
    CollectNetworkTrace = $true  # Set as $true (default) or $false
    #EncryptionProtocol = '' # Supported values: 'Tls 1.0', 'Tls 1.1', 'Tls 1.2'; Without this parameter operating system will choose the best protocol to use
}

$ProgressPreference = "SilentlyContinue";
$scriptUrlBase = 'raw.githubusercontent.com/Azure/SQL-Connectivity-Checker/master'
Invoke-Command -ScriptBlock ([Scriptblock]::Create((iwr ($scriptUrlBase+'/AzureSQLConnectivityChecker.ps1')).Content)) -ArgumentList $parameters
#end
```
4. Set the parameters on the script, you need to set server name and database name.

5. Run it.

6. The results can be seen in the output window.
If the user has the permissions to create folders, a folder with the resulting log file will be created.
When running on Windows, the folder will be opened automatically after the script completes.

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
