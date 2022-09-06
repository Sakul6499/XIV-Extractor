# XIV Extractor

A small tool using [Saint Coinach] to export [Final Fantasy XIV] models/meshes and textures/materials.

## Usage

A few warnings first:

- ⚠️ Due to [Saint Coinach] being very outdated in both, .NET C# version and dependencies (especially dependencies o.o''), we highly recommend to simply fork this project and use it as-is.  
- ⚠️ Dependencies are both NuGet and some are _required_ to be DLLs. They are **absolutely required** (try not to compile [Saint Coinach] yourself ...).
- ⚠️ This project is intended to be used on Windows and Windows only.
- ⚠️ The `Definitions/` folder is absolutely required as it holds most of the data used and needed to run [Saint Coinach] in the first place.
- ⚠️ Definitions will be updated automatically (or attempted to ...) if not disabled!
- ⚠️ Using this tool is **strictly** against [Final Fantasy XIV]'s ToS! You may loose your account if you use this and SquareEnix catches you.
- ⚠️ That said, this project is **not** affiliated with SquareEnix and/or [Final Fantasy XIV] in **any** way.

To actually use this project simply go into `src/Program.cs` and have at the least the following:

```csharp
using SaintCoinachWrapper;

var client = new SaintCoinachWrapper.Client();
```

> You can change the Game-Directory, Export Language and whether we should automatically update or not via parameters to the `SaintCoinachWrapper::Client()` method.

Now that the API is initialized, we can simply call it with types to retrieve data:

```csharp
client.GetData<THE_TYPE_HERE>();
```

For example to get all `TerritoryType`s:

```csharp
var data = client.GetData<TerritoryType>();
```

Or, if you prefer having an Array:

```csharp
var data = client.GetDataArray<TerritoryType>();
```

We then can filter e.g. by name:

```csharp
var data = client.GetDataArray<TerritoryType>();
data.Where(territory_type => territory_type.Name == "something");
```

Or go through all:

```csharp
var data = client.GetDataArray<TerritoryType>();
data.ForEach(territory_type => Console.WriteLine(territory_type.Name));
```

### Territory specialty

This tool was primarily developed to **mass export** Territories (i.e. maps, zones, arenas, etc.), especially their materials/textures.
Thus, for Territories we have special functions to make it easier:

```csharp
IEnumerable<TerritoryType> Client::GetTerritoryTypesByName(String name, Boolean strict = false)
IEnumerable<TerritoryType> Client::GetTerritoryTypesByPlaceName(String place_name, Boolean strict = false)
IEnumerable<TerritoryType> Client::GetTerritoryTypesByZonePlaceName(String zone_place_name, Boolean strict = false)
IEnumerable<TerritoryType> Client::GetTerritoryTypesByRegionPlaceName(String region_place_name, Boolean strict = false)
IEnumerable<TerritoryType> Client::GetTerritoryTypesByAnyName(String name, Boolean strict = false)
```

Whereas a Territory has multiple names and "categories".  
For example, `Limsa Lominsa` is split into the following:

```text
# <Region>/<Zone>/<Place>/<Name>
La Noscea/Limsa Lominsa/Limsa Lominsa Upper Decks/s1t1
La Noscea/Limsa Lominsa/Limsa Lominsa Lower Decks/s1t2
```

Each zone has it's own **unique name** (e.g.: `s1t1`, `s1t2` for `Limsa Lominsa`) and different boss arenas/stages sometimes even have multiple territories or the normal territory differs from the extreme/savage version.

If you use `GetTerritoryTypesByAnyName(String name, Boolean strict = false)` it will search for the name in **any** of those four names.

#### Territory export

To export a territory you simply call the `Exporter::Export` function:

```csharp
static void Exporter::Export(Client client, Territory territory, String output_folder = "output/", bool write_to_console = true)
```

A full example of how to export a given zone looks like this:

```csharp
using SaintCoinachWrapper;

// Initialize Client
var client = new SaintCoinachWrapper.Client();

// Get Territories by unique name
// Note: Since a Territory can have multiple names, we can have multiple results here! Even for the unique names.
//
// Change "n5r5" with the unique name! "n5r5" is one of the recent Pandaemonium raids.
client.GetTerritoryTypesByName("n5r5", true).ToList().ForEach(territory_type =>
{
    // Load Territory
    var territory = client.LoadTerritoryFromTerritoryType(territory_type);

    // Export
    Exporter.Export(client, territory);
});
```

Alternatively, we can export e.g. all of `Limsa Lominsa`:

```csharp
using SaintCoinachWrapper;

// Initialize Client
var client = new SaintCoinachWrapper.Client();

// Get Territories by unique name
// Note: Since a Territory can have multiple names, we can have multiple results here! Even for the unique names.
client.GetTerritoryTypesByName("Limsa Lominsa", true).ToList().ForEach(territory_type =>
{
    // Load Territory
    var territory = client.LoadTerritoryFromTerritoryType(territory_type);

    // Export
    Exporter.Export(client, territory);
});
```

#### Territory additional functions

There are four additional functions for Territories:

Loading a `Territory` (model data and such) from a `TerritoryType`:

```csharp
Territory Client::LoadTerritoryFromTerritoryType(TerritoryType territoryType)
```

Mass/Batch converting from a `TerritoryType` to `Territory`:

```csharp
IEnumerable<Territory> Client::ConvertTerritoryTypesToTerritories(IEnumerable<TerritoryType> territory_types)
```

And two methods to make a "pretty name" for Territories (following the above schema of `<Region>/<Zone>/<Place>/<Name>`):

```csharp
String Client::TerritoryToFullString(Territory territory)
String Client::TerritoryTypeToString(TerritoryType territory_type)
```

## ⚠️ Disclaimer ⚠️

This project is **NOT** affiliated with SquareEnix or [Final Fantasy XIV] in **ANY** shape or form.  
Furthermore, using this tool may lead to your account getting banned.  
Use it at your **OWN** risk.

[Saint Coinach]: https://github.com/xivapi/SaintCoinach
[Final Fantasy XIV]: https://www.finalfantasyxiv.com/
