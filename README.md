# MerchStore
1 .


































az keyvault secret list --vault-name merchstore-keyvault --output table

Name                                  Id                                                                                        ContentType    Enabled    Expires
------------------------------------  ----------------------------------------------------------------------------------------  -------------  ---------  ---------
ApiKey--Value                         https://merchstore-keyvault.vault.azure.net/secrets/ApiKey--Value                                        True
ConnectionStrings--DefaultConnection  https://merchstore-keyvault.vault.azure.net/secrets/ConnectionStrings--DefaultConnection                 True
ReviewApiy --ApiKe                    https://merchstore-keyvault.vault.azure.net/secrets/ReviewApi--ApiKey                                    True
User--Password                        https://merchstore-keyvault.vault.azure.net/secrets/User--Password                                       True
User--Password--Development           https://merchstore-keyvault.vault.azure.net/secrets/User--Password--Development                          True
mans@Mac Merchstore % 



##### az keyvault secret show --vault-name merchstore-keyvault --name "ApiKey--Value" --query value -o tsv
API_KEY

##### az keyvault secret show --vault-name merchstore-keyvault --name "ConnectionStrings--DefaultConnection" --query value -o tsv
Data Source=merchstore.database.windows.net;Initial Catalog=merchstoredb;Persist Security Info=False;User ID=CloudSA85e6d86d;Password=MerchStore2025!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

##### az keyvault secret show --vault-name merchstore-keyvault --name "ReviewApi--ApiKey" --query value -o tsv
ywAEEt74cNJg7_EjWiDGIzjJbLeZQ7l4cetjOq0lJzQ

##### az keyvault secret show --vault-name merchstore-keyvault --name "User--Password" --query value -o tsv
$2a$12$rBNxyD7V1aPNtsqTM5hAj.kxd67q7wTBVRUPnnLU9OYbTpNx8xfQm

##### az keyvault secret show --vault-name merchstore-keyvault --name "User--Password--Development" --query value -o tsv
$2a$12$deSLxQTfgqgyfWMbN6ZzkeQIGHon7obgSG8B.FdUcBBI9vT0/v/7S

