# serverless-partner-workshop

Install .NET 6 and Azure Functions tools v4.
https://dotnet.microsoft.com/download/dotnet/6.0
https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash%2Ckeda#install-the-azure-functions-core-tools

Workshop is optimized for Visual Studio 2022, but can be done via other IDE like VSCode or Rider.
And structured around steps with Start and End repositories for the each step, so you can monitor 


Infrastructure as a code via Azure CLI.

```bash

# Azure CLI infrastructure script.
-----------------

# subscription switch and check
subscriptionID=$(az account list --query "[?contains(name,'Microsoft')].[id]" -o tsv)
echo "Test subscription ID is = " $subscriptionID
az account set --subscription $subscriptionID
az account show

#----------------------------------------------------------------------------------
# Resource group
#----------------------------------------------------------------------------------

location=northeurope
postfix=$RANDOM

# resource group
groupName=adventWorkshop$postfix

az group create --name $groupName --location $location
#az group delete --name $groupName

#----------------------------------------------------------------------------------
# Storage account with Blob and Queue
#----------------------------------------------------------------------------------

location=northeurope
accountSku=Standard_LRS
accountName=${groupName,,}
echo "accountName  = " $accountName

az storage account create --name $accountName --location $location --kind StorageV2 \
--resource-group $groupName --sku $accountSku --access-tier Hot  --https-only true

accountKey=$(az storage account keys list --resource-group $groupName --account-name $accountName --query "[0].value" | tr -d '"')
echo "storage account key = " $accountKey

connString="DefaultEndpointsProtocol=https;AccountName=$accountName;AccountKey=$accountKey;EndpointSuffix=core.windows.net"
echo "connection string = " $connString

blobName=${groupName,,}

az storage container create --name $blobName \
--account-name $accountName --account-key $accountKey --public-access off

queueName=${groupName,,}

az storage queue create --name $queueName --account-key $accountKey \
--account-name $accountName --connection-string $connString

#----------------------------------------------------------------------------------
# Application insights instance
#----------------------------------------------------------------------------------

insightsName=${groupName,,}
echo "insightsName  = " $insightsName

insightsName=msactiondaprlogs$postfix
az monitor app-insights component create --resource-group $groupName --app $insightsName --location $location --kind web --application-type web --retention-time 120

instrumentationKey=$(az monitor app-insights component show --resource-group $groupName --app $insightsName --query  "instrumentationKey" --output tsv)

echo "Insights key = " $instrumentationKey

# on your PC run CMD as administrator, then execute following commands and reboot PC.
# just copy command output below to CMD and execute.
echo "setx APPINSIGHTS_INSTRUMENTATIONKEY "$instrumentationKey

#----------------------------------------------------------------------------------
# Function app with staging slot and consumption plan
#----------------------------------------------------------------------------------

runtime=dotnet
location=northeurope
applicationName=${groupName,,}
accountName=${groupName,,}
echo "applicationName  = " $applicationName

az functionapp create --resource-group $groupName \
--name $applicationName --storage-account $accountName --runtime $runtime \
--app-insights-key $instrumentationKey --consumption-plan-location westeurope --functions-version 3

az functionapp update --resource-group $groupName --name $applicationName --set dailyMemoryTimeQuota=400000

az functionapp deployment slot create --resource-group $groupName --name $applicationName --slot staging

az functionapp identity assign --resource-group $groupName --name $applicationName

az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "MSDEPLOY_RENAME_LOCKED_FILES=1"

managedIdKey=$(az functionapp identity show --name $applicationName --resource-group $groupName --query principalId --output tsv)
echo "Managed Id key = " $managedIdKey

#----------------------------------------------------------------------------------
# Azure SQL Server and database.
#----------------------------------------------------------------------------------

location=northeurope
serverName=${groupName,,}
adminLogin=Admin$groupName
password=Sup3rStr0ng$groupName$postfix
databaseName=${groupName,,}
catalogCollation="SQL_Latin1_General_CP1_CI_AS"

az sql server create --name $serverName --resource-group $groupName \
--location $location --admin-user $adminLogin --admin-password $password

az sql db create --resource-group $groupName --server $serverName --name $databaseName \
--edition GeneralPurpose --family Gen5 --compute-model Serverless \
--auto-pause-delay 60 --capacity 4 --catalog-collation $catalogCollation

outboundIps=$(az webapp show --resource-group $groupName --name $applicationName --query possibleOutboundIpAddresses --output tsv)
IFS=',' read -r -a ipArray <<< "$outboundIps"

for ip in "${ipArray[@]}"
do
  echo "$ip add"
  az sql server firewall-rule create --resource-group $groupName --server $serverName \
  --name "WebApp$ip" --start-ip-address $ip --end-ip-address $ip
done

sqlClientType=ado.net

#TODO add Admin login and remove password, set to variable.
sqlConString=$(az sql db show-connection-string --name $databaseName --server $serverName --client $sqlClientType --output tsv)
sqlConString=${sqlConString/Password=<password>;}
sqlConString=${sqlConString/<username>/$adminLogin}
echo "SQL Connection string is = " $sqlConString

#----------------------------------------------------------------------------------
# Key Vault with policies.
#----------------------------------------------------------------------------------

location=northeurope
keyVaultName=${groupName,,}
echo "keyVaultName  = " $keyVaultName

az keyvault create --name $keyVaultName --resource-group $groupName --location $location 

az keyvault set-policy --name $keyVaultName --object-id $managedIdKey \
--certificate-permissions get list --key-permissions get list --secret-permissions get list

az keyvault secret set --vault-name $keyVaultName --name FancySecret  --value 'SuperSecret'
az keyvault secret set --vault-name $keyVaultName --name SqlConnectionString  --value "$sqlConString"
az keyvault secret set --vault-name $keyVaultName --name SqlConnectionPassword  --value $password
az keyvault secret set --vault-name $keyVaultName --name StorageConnectionString  --value "$connString"

# on your PC run CMD as administrator, then execute following commands and reboot PC.
# just copy command setx string output below to CMD and execute, reboot Windows to take effect.

echo "setx PartnerSqlString \""$sqlConString\"
echo "setx PartnerSqlPassword "$password
echo "setx PartnerStorageString \""$connString\"
echo "setx APPINSIGHTS_PARTNER "$instrumentationKey

az functionapp deployment list-publishing-credentials --resource-group $groupName --name $applicationName
url=$(az functionapp deployment source config-local-git --resource-group $groupName --name $applicationName --query url --output tsv)
echo $url

```
