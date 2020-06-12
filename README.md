# AzureSQLConnectivityChecker

This PowerShell script will run some connectivity checks from this machine to the server and database.  
- Supports Single, Elastic Pools, Managed Instance and SQL Data Warehouse (please provide FQDN, MI public endpoint is supported).
- Supports Public Cloud (\*.database.windows.net), Azure China (\*.database.chinacloudapi.cn) and Azure Germany (\*.database.cloudapi.de).   
- Also supports SQL on-demand (\*.ondemand.sql.azuresynapse.net or \*.ondemand.database.windows.net).  

**In order to run it you need to:**
1. Open Windows PowerShell ISE in Administrator mode  
For the better results, our recommendation is to use the advanced connectivity tests which demand to start PowerShell in Administrator mode. You can still run the basic tests, in case you decide not to run this way. Please note that script parameters 'RunAdvancedConnectivityPolicyTests' and 'CollectNetworkTrace' will only work if the admin privileges are granted.

2. Open a New Script window

3. Paste the following in the script window:

```powershell
$parameters = @{
    # Supports Single, Elastic Pools and Managed Instance (please provide FQDN, MI public endpoint is supported)
    # Supports Azure Synapse / Azure SQL Data Warehouse (*.sql.azuresynapse.net / *.database.windows.net)
    # Supports Public Cloud (*.database.windows.net), Azure China (*.database.chinacloudapi.cn) and Azure Germany (*.database.cloudapi.de)
    Server = '.database.windows.net' # or any other supported FQDN
    Database = ''  # Set the name of the database you wish to test, 'master' will be used by default if nothing is set
    User = ''  # Set the login username you wish to use, 'AzSQLConnCheckerUser' will be used by default if nothing is set
    Password = ''  # Set the login password you wish to use, 'AzSQLConnCheckerPassword' will be used by default if nothing is set

    ## Optional parameters (default values will be used if omitted)
    SendAnonymousUsageData = $true  # Set as $true (default) or $false
    RunAdvancedConnectivityPolicyTests = $true  # Set as $true (default) or $false, this will load the library from Microsoft's GitHub repository needed for running advanced connectivity tests
    CollectNetworkTrace = $true  # Set as $true (default) or $false
    #EncryptionProtocol = '' # Supported values: 'Tls 1.0', 'Tls 1.1', 'Tls 1.2'; Without this parameter operating system will choose the best protocol to use
}

$ProgressPreference = "SilentlyContinue";
if ([string]::IsNullOrEmpty($parameters.RepositoryBranch)) {
    $branch = 'master'
} else {
    $branch = $parameters.RepositoryBranch
}
$scriptUrlBase = 'raw.githubusercontent.com/Azure/SQL-Connectivity-Checker/' + $branch
Invoke-Command -ScriptBlock ([Scriptblock]::Create((Invoke-WebRequest ($scriptUrlBase+'/AzureSQLConnectivityChecker.ps1') -UseBasicParsing).Content)) -ArgumentList $parameters
#end
```
4. Set the parameters on the script, you need to set server name. Database name, user and password are optional but desirable.

5. Run it.

6. The results can be seen in the output window.
If the user has the permissions to create folders, a folder with the resulting log file will be created.
When running on Windows, the folder will be opened automatically after the script completes.
A zip file with all the log files (AllFiles.zip) will be created.

**Running SQL Connectivity Checker in containerized environment**

In order to troubleshoot your containerized application you'll have to temporarily deploy a Powershell Image which will allow you to execute this script and collect the results, you can see all the available Powershell Images [here](https://hub.docker.com/_/microsoft-powershell).

Our suggestion would be to use a lightweight image for this purpose, such as `lts-alpine-3.10` image.

**Kubernetes**

The following steps show the Kubernetes kubectl commands required to download the image and start an interactive PowerShell session.

```
kubectl run -it sqlconncheckerpowershellinstance --image=mcr.microsoft.com/powershell:lts-alpine-3.10
```

The following command is used to exit the current Powershell session.
```
exit
```

The following command is used to attach to an existing Powershell instance.
```
kubectl attach -it sqlconncheckerpowershellinstance
```

The following command is used to delete the pod running this image when you no longer need it.

```
kubectl delete pod sqlconncheckerpowershellinstance
```

**Docker**

The following steps show the Docker commands required to download the image and start an interactive PowerShell session.

```
docker run -it --name sqlconncheckerpowershellinstance --image=mcr.microsoft.com/powershell:lts-alpine-3.10
```

The following command is used to exit the current Powershell session.
```
exit
```

The following command is used to attach to an existing Powershell instance.
```
docker attach sqlconncheckerpowershellinstance
```

The following command is used to delete the container running this image when you no longer need it.

```
docker container rm sqlconncheckerpowershellinstance
```


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
