
# 🚀 DynamicsCrmConnector pour .NET Core
[![Build & Tests](https://github.com/nabil-sidhoum/DynamicsCrmConnector/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/nabil-sidhoum/DynamicsCrmConnector/actions/workflows/nuget-publish.yml)

**DynamicsCrmConnector** est un connecteur HTTP asynchrone conçu pour les applications .NET Core, permettant de communiquer facilement avec l'API Web de Microsoft Dynamics CRM.

---

## 📚 Sommaire

- [Présentation](#-présentation)
- [Configuration](#-configuration)
- [Injection de dépendance](#-injection-de-dépendance)
- [📦 Packages NuGet](#-packages-nuget)
- [💡 Cas d’usage](#-cas-dusage)
- [⚠️ Remarques sur les entités](#️-remarques-sur-les-entités)

---

## 🧩 Présentation

Microsoft Dynamics CRM est une solution de gestion de la relation client (CRM) qui expose une API Web RESTful basée sur OData. Ce connecteur permet d'interagir avec cette API pour effectuer des opérations courantes : création, lecture, mise à jour et suppression.

Le connecteur utilise une **authentification basée sur un `ClientId` et une clé secrète (`SecretId`)**. Cela permet une authentification sécurisée via un utilisateur applicatif, sans nécessiter d'interaction avec l'interface utilisateur.

---

## ⚙️ Configuration

Ajoutez la configuration suivante dans votre fichier `appsettings.json` :

```json
"DynamicsCRM": {
  "BaseUrl": "https://votre-org.crm.dynamics.com/",
  "WebApiPath": "api/data",
  "Version": "v9.2",
  "Authentication": {
    "TenantId": "votre-ID-locataire",
    "ClientId": "votre-ID-client",
    "SecretId": "votre-clé-secrète"
  }
}
```

---

## 🧪 Injection de dépendance

Enregistrez le connecteur comme service dans `IServiceCollection` :

```csharp
public IConfiguration Configuration { get; }

public void ConfigureServices(IServiceCollection services)
{
    services.AddDynamicsCRM(Configuration);
}
```

Une fois cela fait, vous pouvez injecter `IDynamicsCrmClient` dans vos classes.

---

## 📦 Packages NuGet

| 📁 Package | 🧾 Version | 📥 Installation |
|------------|------------|------------------|
| `Tools.DynamicsCRM` | [![NuGet](https://img.shields.io/nuget/v/Tools.DynamicsCRM.svg)](https://www.nuget.org/packages/Tools.DynamicsCRM) | `dotnet add package Tools.DynamicsCRM` |
| `Tools.DynamicsCRM.Extensions` | [![NuGet](https://img.shields.io/nuget/v/Tools.DynamicsCRM.Extensions.svg)](https://www.nuget.org/packages/Tools.DynamicsCRM.Extensions) | `dotnet add package Tools.DynamicsCRM.Extensions` |

---

## 💡 Cas d’usage

### 🔧 Création

**Sans l'extension** :

```csharp
Dictionary<string, object> account = new()
{
    { "name", "NewAccountName" }
};
Guid id = await _crmclient.CreateAsync("accounts", account);
```

**Avec l'extension** :

```csharp
Account account = new()
{
    Name = "NewAccountName"
};
Guid id = await _crmclient.Create<Account>(account);
```

---

### 📝 Mise à jour

**Sans l'extension** :

```csharp
Guid id = new("...");
Dictionary<string, object> update = new()
{
    { "name", "UpdatedName" }
};
await _crmclient.UpdateAsync("accounts", id, update);
```

**Avec l'extension** :

```csharp
Account update = new()
{
    AccountId = id,
    Name = "UpdatedName"
};
await _crmclient.Update<Account>(update);
```

---

### ❌ Suppression

**Sans l'extension** :

```csharp
Guid id = new("...");
await _crmclient.DeleteAsync("accounts", id);
```

**Avec l'extension** :

```csharp
Guid id = new("...");
await _crmclient.Delete<Account>(id);
```

---

### 📖 Lecture

**Sans l'extension** :

```csharp
Guid id = new("...");
string[] fields = [ "accountid", "name" ];
Dictionary<string, object> data = await _crmclient.RetrieveAsync("accounts", id, fields);
```

**Avec l'extension** :

```csharp
Guid id = new("...");
string[] fields = [ Account.Fields.AccountId, Account.Fields.Name ];
Account data = await _crmclient.Retrieve<Account>(id, fields);
```

---

## ⚠️ Remarques sur les entités

La structure des données de Dynamics CRM peut être complexe. Pour vous faciliter la vie, vous pouvez utiliser [Early Bound Generator for CRM Web API](https://www.xrmtoolbox.com/plugins/crm.webApi.earlyBoundGenerator/) afin de générer automatiquement les classes d'entités.

> ⚠️ Certaines entités comme `email`, `activity`, ou les champs multi-entités peuvent être générées de manière incorrecte. N'hésitez pas à **créer vos propres classes** si nécessaire.
