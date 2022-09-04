using SaintCoinach.Graphics;
using SaintCoinach.Xiv;

namespace SaintCoinachWrapper
{
    public class Client
    {
        public SaintCoinach.ARealmReversed a_realm_reversed { get; private set; }

        public Client(String gameDirectory = @"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn", SaintCoinach.Ex.Language language = SaintCoinach.Ex.Language.English, Boolean doUpdate = true)
        {
            Console.WriteLine("Starting up ...");
            this.a_realm_reversed = new SaintCoinach.ARealmReversed(gameDirectory, language);

            this.PrintVersionInformation();

            // Check for updates
            if (doUpdate && !this.a_realm_reversed.IsCurrentVersion)
            {
                Console.WriteLine("Updating ...");
                const bool IncludeDataChanges = true;
                var updateReport = this.a_realm_reversed.Update(IncludeDataChanges);
                Console.WriteLine("Update complete.");

                Console.WriteLine("Updated version: " + updateReport.UpdateVersion);
                Console.WriteLine("Changes:");
                foreach (var change in updateReport.Changes)
                {
                    Console.WriteLine("\t" + change);
                }
            }
        }

        public void PrintVersionInformation()
        {
            Console.WriteLine("Definitions Version: " + this.a_realm_reversed.DefinitionVersion);
        }

        public IXivSheet<T> GetData<T>() where T : IXivRow
        {
            return this.a_realm_reversed.GameData.GetSheet<T>();
        }

        public T[] GetDataArray<T>() where T : IXivRow
        {
            return this.GetData<T>().ToArray();
        }

        public IXivSheet<TerritoryType> GetTerritoryTypes()
        {
            return this.GetData<TerritoryType>();
        }

        public IEnumerable<TerritoryType> GetTerritoryTypesByName(String name, Boolean strict = false)
        {
            if (strict)
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.Name.ToString().Contains(name));
            }
            else
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.Name.ToString().Equals(name));
            }
        }

        public IEnumerable<TerritoryType> GetTerritoryTypesByPlaceName(String place_name, Boolean strict = false)
        {
            if (strict)
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.PlaceName.ToString().Contains(place_name));
            }
            else
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.PlaceName.ToString().Equals(place_name));
            }
        }

        public IEnumerable<TerritoryType> GetTerritoryTypesByZonePlaceName(String zone_place_name, Boolean strict = false)
        {
            if (strict)
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.ZonePlaceName.ToString().Contains(zone_place_name));
            }
            else
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.ZonePlaceName.ToString().Equals(zone_place_name));
            }
        }

        public IEnumerable<TerritoryType> GetTerritoryTypesByRegionPlaceName(String region_place_name, Boolean strict = false)
        {
            if (strict)
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.RegionPlaceName.ToString().Contains(region_place_name));
            }
            else
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.RegionPlaceName.ToString().Equals(region_place_name));
            }
        }

        public IEnumerable<TerritoryType> GetTerritoryTypesByAnyName(String name, Boolean strict = false)
        {
            if (strict)
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.Name.ToString().Equals(name) || territory_type.PlaceName.ToString().Equals(name) || territory_type.ZonePlaceName.ToString().Equals(name) || territory_type.RegionPlaceName.ToString().Equals(name));
            }
            else
            {
                return this.GetData<TerritoryType>().Where(territory_type => territory_type.Name.ToString().Contains(name) || territory_type.PlaceName.ToString().Contains(name) || territory_type.ZonePlaceName.ToString().Contains(name) || territory_type.RegionPlaceName.ToString().Contains(name));
            }
        }

        public Territory LoadTerritoryFromTerritoryType(TerritoryType territoryType)
        {
            return new Territory(territoryType);
        }

        public IEnumerable<Territory> ConvertTerritoryTypesToTerritories(IEnumerable<TerritoryType> territory_types)
        {
            return territory_types.Select(territory_type => this.LoadTerritoryFromTerritoryType(territory_type));
        }

        public String TerritoryToFullString(Territory territory)
        {
            return this.TerritoryTypeToString(this.GetTerritoryTypesByName(territory.Name, true).First());
        }

        public String TerritoryTypeToString(TerritoryType territory_type)
        {
            return $"{territory_type.RegionPlaceName}/{territory_type.ZonePlaceName}/{territory_type.PlaceName}/{territory_type.Name}";
        }
    }
}
