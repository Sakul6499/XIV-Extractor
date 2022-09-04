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
    }
}
