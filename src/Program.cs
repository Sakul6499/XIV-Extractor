using SaintCoinachWrapper;

namespace XivExtractor
{
    public class XivExtractor
    {
        public static void Main(string[] args)
        {
            // Client
            var client = new SaintCoinachWrapper.Client();

            // Search Query
            client.GetTerritoryTypesByAnyName("The World Unsundered").ToList().ForEach(territory_type =>
            {
                var territory = client.LoadTerritoryFromTerritoryType(territory_type);
                Console.WriteLine(client.TerritoryToFullString(territory));
            });

            // Search & Export
            client.GetTerritoryTypesByAnyName("Purgatory").ToList().ForEach(territory_type =>
            {
                var territory = client.LoadTerritoryFromTerritoryType(territory_type);
                Exporter.Export(client, territory);
            });
        }
    }
}