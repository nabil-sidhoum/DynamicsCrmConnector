# Guide de contribution

Ce document définit les conventions et standards techniques du projet. Il est destiné à tout contributeur souhaitant comprendre ou faire évoluer le code.

---

## 🚀 Setup de développement

**Prérequis**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 17.8+ ou Rider 2023.3+

**Lancer le projet en local**

```bash
git clone https://github.com/nabil-sidhoum/nabil-sidhoum.github.io.git
cd nabil-sidhoum.github.io/src
dotnet run --project BlazorPortfolio.Client
```

**Lancer les tests**

```bash
cd src
dotnet test BlazorPortfolio.Client.Tests
```

**Vérifier le formatage**

```bash
cd src
dotnet format --verify-no-changes
```

---

## 📝 Conventions de commits

Le projet suit [Conventional Commits](https://www.conventionalcommits.org/). Les messages sont rédigés **en français**.

### Format

```
<type>: <description courte>

- détail optionnel
- détail optionnel
```

### Types autorisés

| Type | Usage |
|---|---|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `refactor` | Restructuration sans changement de comportement |
| `style` | Formatage, espaces, renommage sans impact fonctionnel |
| `test` | Ajout ou modification de tests |
| `ci` | Modification du pipeline CI/CD |
| `chore` | Tâche de maintenance (dépendances, config) |
| `docs` | Documentation uniquement |

### Exemples

```
feat: ajout de la page formations

refactor: migration des données statiques vers des fichiers JSON

- Suppression des classes Data/
- Ajout des fichiers JSON dans wwwroot/data/
- Mise à jour des services pour charger les données via HttpClient

test: ajout des tests unitaires pour ExperienceService

ci: ajout de l'étape dotnet test dans le pipeline de déploiement
```

---

## 🔍 Conventions de code

Le projet suit les conventions C# standard Microsoft, précisées et appliquées via `.editorconfig` et les analyseurs Roslyn.

### Nommage

| Élément | Convention | Exemple |
|---|---|---|
| Classe, interface, propriété, méthode | PascalCase | `ExperienceService` |
| Champ privé | `_camelCase` | `_http` |
| Paramètre, variable locale | camelCase | `experiences` |
| Interface | Préfixe `I` | `IExperienceService` |
| Constante publique | PascalCase | `MaxRetries` |

### Règles générales

**Types explicites — pas de `var`**
```csharp
// ✅
List<Experience> experiences = await _http.GetFromJsonAsync<List<Experience>>("data/experiences.json");

// ❌
var experiences = await _http.GetFromJsonAsync<List<Experience>>("data/experiences.json");
```

**Modificateurs d'accès toujours explicites**
```csharp
// ✅
private readonly HttpClient _http;

// ❌
readonly HttpClient _http;
```

**Accolades obligatoires sur tous les blocs**
```csharp
// ✅
if (result is null)
{
    return Enumerable.Empty<Experience>();
}

// ❌
if (result is null)
    return Enumerable.Empty<Experience>();
```

**`IEnumerable<T>` en retour de service, pas `List<T>`**
```csharp
// ✅
public async Task<IEnumerable<Experience>> GetExperiencesAsync()

// ❌
public async Task<List<Experience>> GetExperiencesAsync()
```

**`Enumerable.Empty<T>()` pour les retours vides**
```csharp
// ✅
return Enumerable.Empty<Experience>();

// ❌
return new List<Experience>();
// ❌
return default;
```

### Structure des services

Tout service suit le même pattern : constructeur explicite avec champ privé `readonly`, appel HTTP typé, gestion d'erreur.

```csharp
public class ExperienceService
{
    private readonly HttpClient _http;

    public ExperienceService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<Experience>> GetExperiencesAsync()
    {
        try
        {
            List<Experience> result = await _http.GetFromJsonAsync<List<Experience>>("data/experiences.json");
            return result?.OrderByDescending(e => e.DateDebut) ?? Enumerable.Empty<Experience>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<Experience>();
        }
    }
}
```

---

## 🧪 Standards de test

Les tests suivent la convention de nommage **`Méthode_Scénario_RésultatAttendu`** et sont structurés en blocs `// Arrange / Act / Assert`.

```csharp
[Fact]
public async Task GetExperiencesAsync_RetourneListeVide_QuandReponseEst404()
{
    // Arrange
    MockHttpMessageHandler mockHttp = new();
    mockHttp
        .When("http://localhost/data/experiences.json")
        .Respond(HttpStatusCode.NotFound);
    HttpClient http = new(mockHttp) { BaseAddress = new Uri("http://localhost/") };
    ExperienceService service = new(http);

    // Act
    IEnumerable<Experience> result = await service.GetExperiencesAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
}
```

Chaque service doit couvrir au minimum :
- Le cas nominal — données correctement désérialisées
- Le cas d'erreur HTTP — retour d'une collection vide sans exception
- Le mapping des propriétés — cohérence modèle / JSON

---

## ⚙️ Pipeline CI/CD

Le déploiement est automatique sur `main`. Toute contribution doit passer les étapes suivantes avant d'être déployée :

```
push sur main
     │
     ▼
dotnet test        ← bloque si KO
     │
     ▼
dotnet publish
     │
     ▼
GitHub Pages
```

Ne jamais pousser directement sur `main` un code qui fait échouer les tests localement.
