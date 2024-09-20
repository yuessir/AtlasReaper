using System;

namespace AtlasReaper
{
    /* debug command
                     string[] command = {
    "confluence",
    "listattachments",
    "-u",
    "http://lan-confluence.dbsports.online",
    "-c",
    "112985096%3Adc97a770c1c3c3af2564ce2365cf19ac0466ca70",
    "-p",
    "108967828",
     "-d",
    "D://"
};

                string[] command2 = {
    "confluence",
    "listpages",
    "-u",
    "http://lan-confluence.dbsports.online",
    "-c",
    "112985096%3Adc97a770c1c3c3af2564ce2365cf19ac0466ca70",
    "-s",
    "TW",
    "-p",
    "80073240"
};
     */
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Create an instance of ArgHandler
                ArgHandler argHandler = new ArgHandler();
                //confluence listpages -u http://lan-confluence.dbsports.online -c 120291564%3Ae20a92ab4fb88d0f85d2a99f043240dfbccef8b9 -s TW -p 108967828"
                //confluence listspaces -u http://lan-confluence.dbsports.online -c 120291564%3Ae20a92ab4fb88d0f85d2a99f043240dfbccef8b9 -s OP
                // Invoke the HandleArgs method

                string[] command = {
    "confluence",
    "listattachments",
    "-u",
    "http://lan-confluence.dbsports.online",
    "-c",
    "120291564%3Ae20a92ab4fb88d0f85d2a99f043240dfbccef8b9",
    "-p",
    "108967828",
     "-d",
    "D://"
};

                string[] command2 = {
    "confluence",
    "listpages",
    "-u",
    "http://lan-confluence.dbsports.online",
    "-c",
    "120291564%3Ae20a92ab4fb88d0f85d2a99f043240dfbccef8b9",
       "-s",
    "penda",
    "-p",
    "76724643"
};
                string[] command3 = {
    "confluence",
    "listspaces",
    "-u",
    "http://lan-confluence.dbsports.online",
    "-c",
    "120291564%3Ae20a92ab4fb88d0f85d2a99f043240dfbccef8b9",
       "-s",
    "PM"
};
                argHandler.HandleArgs(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured: " + ex.Message);
            }

        }
    }
}
