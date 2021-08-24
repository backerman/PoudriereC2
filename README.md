# PoudriereC2

&copy; Brad Ackerman. MIT license.

Not an official Microsoft project. This README is not yet complete.

## Setup

### Load database

Discussion: Azure Database for PostgreSQL vs IaaS

```postgresql
CREATE USER poudrierec2 WITH PASSWORD '«some random and entropic password»';
GRANT poudriereadmin TO poudrierec2;
```

### Azure Function

Authentication. Create new application.

Configuration - 
* PostgresConnection: the connection string for the production database. (FIXME: support Key Vault)

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
