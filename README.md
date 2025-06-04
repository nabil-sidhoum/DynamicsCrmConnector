# DynamicsCrmConnector pour .Net Core

***DynamicsCrmConnector*** est un connecteur HTTP asynchrone conçu pour les applications .NET Core, permettant de se connecter à l'API Web de Microsoft Dynamics CRM de votre organisation.
 
En résumé, Microsoft Dynamics CRM est une application évolutive de gestion de la relation client qui exploite l'API Web basée sur le protocole OData (Open Data Protocol). Cette API permet d'effectuer les opérations classiques d'accès aux données, telles que la création, la mise à jour, la récupération et la suppression, depuis une application externe à Microsoft Dynamics CRM.

Parmi toutes les options de [connexion à Dynamics CRM](https://learn.microsoft.com/fr-fr/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect), ce connecteur n'utilise qu'une authentification basée sur le ClientId et la clé secrète client. Cette méthode d'authentification permet de créer un utilisateur d'application dont les rôles peuvent être gérés comme pour tout autre utilisateur, bien qu'il n'ait pas accès à l'interface utilisateur. L'utilisateur disposera d'une clé secrète client avec une date d'expiration, ce qui garantit une authentification sécurisée dans le temps
 
Il suffit d'ajouter cette section dans votre fichier ***appsettings.json*** en y renseignant les informations de votre organisation.
 
```json
  "DynamicsCRM": {
    "BaseUrl": "L'url de votre organisation crm",
    "WebApiPath": "api/data",
    "Version": "v9.2",
    "Authentication": {
      "TenantId": "votre ID de locataire",
      "ClientId": "votre ID client",
      "SecretId": "votre clé secrète"
    }
  }
```
 
Enfin, il suffit d'enregistrer le connecteur comme service dans ***IServiceCollection***.La méthode d'extension ***AddDynamicsCRM*** requiert comme paramètre toutes les propriétés de configuration d'application ***IConfiguration*** pour obtenir toutes les informations de l'organisation.
 
```cs
public IConfiguration Configuration { get; }
 
public void ConfigureServices(IServiceCollection services)
{
  services.AddDynamicsCRM(Configuration);
}
```
Une fois la configuration précédente effectuée, il suffit d'une instance de ***IDynamicsCrmClient*** pour exécuter les opérations d'accès aux données classiques.

La structure des données de Microsoft Dynamics CRM peut être complexe. Grâce à des outils tels que [***Early Bound Generator for CRM Web API***](https://www.xrmtoolbox.com/plugins/crm.webApi.earlyBoundGenerator/), il est possible de générer automatiquement les entités présentes dans le CRM sous forme de classes. En utilisant le projet d'extension, vous pouvez exploiter ces classes auto-générées (ou créer les vôtres si vous le souhaitez) pour simplifier les opérations d'accès aux données classiques.

## Cas d'usage
### Opération de Création
*Ajouter une nouvelle une donnée de la base de données CRM.*

* Sans l'extension

```cs
private readonly IDynamicsCrmClient _crmclient;
Dictionary<string, object> accounttocreate = new()
{
    { "name", "NewAccountName" }
};
Guid accountid = await _crmclient.CreateAsync("accounts", accounttocreate);
```
 
* Avec l'extension

```cs
private readonly IDynamicsCrmClient _crmclient;
Account accounttocreate = new()
{
    Name = "NewAccountName"
};
Guid accountid = await _crmclient.Create<Account>(accounttocreate);
```
### Opération de Mise à jour

*Modifier une donnée existante de la base de données CRM via son identifiant.*

* Sans l'extension

```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant d'un enregistrement de type Compte";
Dictionary<string, object> accounttoupdate = new()
{
    { "name", "UpdatedName" }
};
bool success = await _crmclient.UpdateAsync("accounts", accountid, accounttoupdate);
```

* Avec l'extension

```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant de l'enregistrement de type Compte à mettre à jour";
Account accounttoupdate = new()
{
    AccountId = accountid,
    Name = "UpdatedName"
};
bool success = await _crmclient.Update<Account>(accounttoupdate);
```

### Opération de Suppression
*Supprimer une donnée de la base de données CRM via son identifiant.*
* Sans l'extension
```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant d'un enregistrement de type Compte"; 
bool success = await _crmclient.DeleteAsync("accounts", accountid);
``` 
* Avec l'extension
```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant d'un enregistrement de type Compte";
bool success = await _crmclient.Delete<Account>(accountid);
```

## Opération de lecture
*Récupérer et lire les données existantes de la base de données CRM.*

1- Récupération simple via son identifiant
* Sans l'extension
 ```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant d'un enregistrement de type Compte";
string[] accountfields =
[
	"accountid",
	"name"
];
Dictionary<string,object> accountdata = await _crmclient.RetrieveAsync("accounts", accountid, accountfields);
```
* Avec l'extension
```cs
private readonly IDynamicsCrmClient _crmclient;
Guid accountid = "Identifiant d'un enregistrement de type Compte";
string[] accountfields =
[
	Account.Fields.AccountId,
	Account.Fields.Name,
];
Account data = await _crmclient.Retrieve<Account>(accountid, accountfields);
```

