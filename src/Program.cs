using System.Reflection;
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
            Console.WriteLine("Search Query");
            client.GetTerritoryTypesByAnyName("The World Unsundered").ToList().ForEach(territory_type =>
            {
                var territory = client.LoadTerritoryFromTerritoryType(territory_type);
                Console.WriteLine(client.TerritoryToFullString(territory));
            });
            Console.WriteLine("");

            // Search & Export
            Console.WriteLine("Search & Export");
            client.GetTerritoryTypesByName("n5r5", true).ToList().ForEach(territory_type =>
            {
                var territory = client.LoadTerritoryFromTerritoryType(territory_type);
                Console.WriteLine(client.TerritoryToFullString(territory));
                Exporter.Export(client, territory);
            });
            Console.WriteLine("");
        }
    }
}