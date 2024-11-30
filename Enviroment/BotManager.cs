#nullable enable

using Newtonsoft.Json;

namespace KIBAEMON2024_CSharp.Enviroment;

public static class BotManager
{
    public static IReadOnlyList<Bot> Bots => BotsInternal.AsReadOnly();

    private static List<Bot> BotsInternal { get; set; } = [];

    public static void Initialize()
    {
        if (File.Exists("Bots.json"))
        {
            var authFile = File.ReadAllText("Bots.json");
            BotsInternal = JsonConvert.DeserializeObject<List<Bot>>(authFile) ?? new List<Bot>();
        }
        else
        {
            BotsInternal =
            [
                new Bot { Name = "UnknownBot", Authorization = Authorization.Empty }
            ];

            var emptyFile = JsonConvert.SerializeObject(Bots);
            var writer = File.CreateText("Bots.json");

            writer.Write(emptyFile);
            writer.Dispose();
        }
    }

    public static Bot? GetBot(string name)
    {
        return BotsInternal.FirstOrDefault(bot => bot.Name == name);
    }
}