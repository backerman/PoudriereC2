# PoudriereC2

&copy; Brad Ackerman. MIT license.

Not an official Microsoft project. This README is not yet complete.

## Setup

### Load database

To log in with Azure Active Directory, specify the username as the tenant user (without the tenant domain) and set the password to a bearer token:

```powershell
$Env:PGPASSWORD=(Get-AzAccessToken -ResourceUrl "https://ossrdbms-aad$((Get-AzContext).Environment.SqlDatabaseDnsSuffix)").Token
```

or

```shell
export PGPASSWORD=$(az account get-access-token --resource-type oss-rdbms --query "[accessToken]" -o tsv)
```

### Redeploying the function

If the function is deleted and redeployed, its AAD managed service identity
will have a different GUID, and PostgreSQL authentication will fail. To fix
this issue, query AAD for the new GUID:

```shell
# Get the SP ID
az ad sp list --display-name ffpoudrierec2 --filter "servicePrincipalType eq 'ManagedIdentity'" --query '[].id'
```

Then open `psql` and update the role's security label:

```sql
security label for "pgaadauth" on role ffpoudrierec2 is 'aadauth,oid=<SP-GUID-goes-here>,type=service';
```

Discussion: Azure Database for PostgreSQL vs IaaS

```postgresql
CREATE USER poudrierec2 WITH PASSWORD '«some random and entropic password»';
GRANT poudriereadmin TO poudrierec2;
```

### Azure Function

Authentication. Create new application.

Configuration -

* PostgresConnection: the connection string for the production database. (FIXME: support Key Vault)

Deployment:

```shell
dotnet publish --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary 
func azure functionapp publish ffpoudrierec2
```

### Azure Active Directory

#### Create application roles

Go to the AAD application created in the "Azure Function" step above. Select the
"Create App Role" button and create three:

| Display name | Value | Allowed member types | Description |
| --- | --- | --- | --- |
| Administrator | PoudriereC2.Administrator | Users/Groups | Read and modify all settings. |
| Viewer | PoudriereC2.Viewer | Users/Groups | Read all non-sensitive settings. |
| Worker node | PoudriereC2.WorkerNode | Applications | Obtain and report status of build jobs. |

(FIXME: Use app manifest here instead?)

Select the application in the "Enterprise applications" blade. Under "Users and groups",
add your user to the Administrator role.

#### Grant VM permission to call functions

It's not yet possible to grant a role to a managed identity from the portal, so
you'll need to do it from the command line.

```shell
appId=$(az ad app list --display-name "PoudriereC2 API" --query '[0].appId' --output tsv)
appSPID=$(az ad sp show --id $appId --query 'objectId' --output tsv)
workerNodeRoleID=$(az ad app list --display-name PoudriereC2 --query "[0].appRoles[?value=='PoudriereC2.WorkerNode'].id | [0]" --output tsv)
workerSPID=$(az identity list --query "[?name=='poudriereidentity'].principalId | [0]" --output tsv)
az rest --method post \
    --uri "https://graph.microsoft.com/v1.0/servicePrincipals/${appSPID}/appRoleAssignedTo" \
    --body "{ \"principalId\": \"${workerSPID}\", \"appRoleId\": \"${workerNodeRoleID}\", \"resourceId\": \"${appSPID}\" }"
```

The `az rest` command will return an `appRoleAssignment` object and the `poudriereidentity`
service principal will now appear in the application's "Users and Groups" blade.

#### Get MSI from worker and validate role present

### Security

## Local development

Install [Azure Functions Core Tools][afct]. Then set `PostgresConnection` with
the development database connection string and run `func start` in `backend/Poudrierec2`
to start the server.

In a different session, run `yarn dev` to start a webserver hosting the client
after configuring the following variables:

| Name | Value |
| ---- | ----- |
| NEXT_PUBLIC_AAD_CLIENT_ID | The client ID of the frontend's AAD application |
| NEXT_PUBLIC_AAD_TENANT_ID | The ID of the tenant containing the frontend application |
| NEXT_PUBLIC_API_BASE_URL | The URL of the development API server; normally `http://localhost:7071/` |
| NEXT_PUBLIC_IS_DEVELOPMENT | "TRUE" or "YES" (case-insensitive) iff a development instance |
| NEXT_PUBLIC_API_SCOPE | The API scope exposed by the Azure Function |

[afct]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools
