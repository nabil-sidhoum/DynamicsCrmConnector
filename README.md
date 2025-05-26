# DynamicsCrmConnector pour .Net Core
 
***DynamicsCrmConnector*** est un connecteur HTTP asynchrone qui permet de se connecter à l'API Web Microsoft Dynamics CRM de votre organisation.
 
En résumé, Microsoft Dynamics CRM est une application de gestion de la relation client évolutive dont l'API Web utilise le protocole ODAP (Open Data Protocol).
Ce projet a servi à établir une connexion avec toutes les instances de Microsoft Dynamics CRM d'un client. Il permet d'exécuter les opérations d'accès aux données classiques (Create-Update-Retrieve-Delete) depuis une application externe à Microsoft Dynamics CRM.
 
Sur toutes les [connexions possibles à Dynamics CRM](https://learn.microsoft.com/fr-fr/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect),
ce connecteur ne permet qu'une authentification basée sur le ClientId ou la clé secrète client.
Cette authentification permet d'avoir un utilisateur de l'application, dont nous pourrons gérer les rôles comme tout autre utilisateur, mais qui n'aura pas accès à l'interface utilisateur.
Il possède une clé secrète client avec une date d'expiration. Ces deux aspects permettent une authentification sécurisée dans le temps.
 
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