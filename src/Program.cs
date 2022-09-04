namespace XivExtractor
{
    public class XivExtractor
    {
        public static void Main(string[] args)
        {
            var client = new SaintCoinachWrapper.Client();
            client.GetTerritoryTypesByAnyName("The World Unsundered").ToList().ForEach(territory_type => Console.WriteLine(client.TerritoryToFullString(client.LoadTerritoryFromTerritoryType(territory_type))));
        }
    }
}