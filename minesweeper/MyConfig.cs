using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace minesweeper
{
    internal class MyConfig
    {
        private static string configPath = "config.json";
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        /// <summary>
        /// config.json adatai
        /// </summary>
        public class ConfigData
        {
            public string JsonVersion { get; set; } = "Console";
            public Dictionary<string, string> Irányítás { get; set; } = new();
            public Dictionary<string, string> UpdateConfig { get; set; } = new();
            public Dictionary<string, string> Szín_Háttér { get; set; } = new();
            public Dictionary<string, string> Szín_Betű { get; set; } = new();
        }
        /// <summary>
        /// .mine fájlok adatai
        /// </summary>
        public class GameData
        {
            public string JsonVersion { get; set; } = "Console";
            public int meretM { get; set; }
            public int meretSZ { get; set; }
            public int CursorX { get; set; }
            public int CursorY { get; set; }
            public int aknakszama { get; set; }
            public int flagged { get; set; }
            public Dictionary<string, string> akna { get; set; } = new();
            public Dictionary<string, string> visible { get; set; } = new();
        }
        /// <summary>
        /// Konfiguráció elmentése
        /// </summary>
        public static void Save()
        {
            var config = new ConfigData
            {
                JsonVersion = Program.local_version,
                Irányítás = Program.Billentyűk.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                UpdateConfig = Program.UpdateConfig.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                Szín_Háttér = Program.Szín_Háttér.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                Szín_Betű = Program.Szín_Betű.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
            };

            string json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(configPath, json);
        }
        /// <summary>
        /// Konfiguráció betöltése
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(configPath))
                return;

            string json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<ConfigData>(json, jsonOptions);

            if (config == null)
                return;

            foreach (var kv in config.Irányítás)
            {
                if (Enum.TryParse(kv.Value, out ConsoleKey key))
                    Program.Billentyűk[kv.Key] = key;
            }

            foreach (var kv in config.UpdateConfig)
            {
                if (bool.TryParse(kv.Value, out bool value))
                    Program.UpdateConfig[kv.Key] = value;
            }

            foreach (var kv in config.Szín_Háttér)
            {
                if (Enum.TryParse(kv.Value, out ConsoleColor color))
                    Program.Szín_Háttér[kv.Key] = color;
            }

            foreach (var kv in config.Szín_Betű)
            {
                if (Enum.TryParse(kv.Value, out ConsoleColor color))
                    Program.Szín_Betű[kv.Key] = color;
            }
        }
        /// <summary>
        /// Mentés nevének bekérése
        /// </summary>
        /// <returns>Validált Név</returns>
        private static string NameSave()
        {
            Directory.CreateDirectory("Games");

            Console.CursorVisible = true;
            string name;
            bool ok;

            Console.Clear();
            do
            {
                ok = true;
                Console.WriteLine("Játék mentése (\"-\" a visszalépéshez)\n");
                Console.Write("A mentés neve: ");
                name = Console.ReadLine()?.Trim() ?? "save";

                try
                {
                    string test = $"Games/{name}.test";
                    File.Create(test).Close();
                    File.Delete(test);
                }
                catch
                {
                    ok = false;
                    Console.WriteLine("Érvénytelen fájlnév!");
                }

                if (name == "-")
                    return "-";

                if (File.Exists($"Games/{name}.mine"))
                {
                    ok = false;
                    Console.WriteLine("Már létezik ilyen nevű mentés!");
                }

            } while (!ok);

            Console.CursorVisible = false;
            return name;
        }
        /// <summary>
        /// Mentés kiírása .mine fájlba
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        public static void SaveGame(string[,] akna, string[,] visible)
        {
            Directory.CreateDirectory("Games");

            string name = NameSave();
            if (name == "-")
                return;

            var config = new GameData
            {
                JsonVersion = Program.local_version,
                meretM = Program.PublicMeretM,
                meretSZ = Program.PublicMeretSZ,
                CursorX = Program.PublicCursorX,
                CursorY = Program.PublicCursorY,
                aknakszama = Program.PublicAknakszama,
                flagged = Program.PublicFlagcount
            };

            for (int x = 0; x < akna.GetLength(0); x++)
            {
                for (int y = 0; y < akna.GetLength(1); y++)
                {
                    config.akna[$"{x},{y}"] = akna[x, y];
                    config.visible[$"{x},{y}"] = visible[x, y];
                }
            }
            string json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText($"Games/{name}.mine", json);
        }
        /// <summary>
        /// Mentés nevének bekérése
        /// </summary>
        /// <returns>Validált Név</returns>
        private static string NameLoad()
        {
            Console.CursorVisible = true;
            string name;
            bool ok;

            Console.Clear();
            do
            {
                ok = true;

                Console.WriteLine("\nJáték betöltése (\"-\" a visszalépéshez)\n");
                Directory.CreateDirectory("Games");
                string[] files = Directory.GetFiles("Games", "*.mine");
                Console.WriteLine("Elérhető mentések:");
                if (files.Length == 0)
                {
                    Console.WriteLine("Nincs egyetlen mentés sem!");
                }
                if (files.Length != 0)
                {
                    foreach (string file in files)
                    {
                        Console.WriteLine(Path.GetFileNameWithoutExtension(file));
                    }
                }
                Console.Write("\nA mentés neve: ");
                name = Console.ReadLine()?.Trim() ?? "save";

                if (name == "-")
                    return "-";

                if (!File.Exists($"Games/{name}.mine"))
                {
                    ok = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("A fájl nem létezik!");
                    Console.ResetColor();
                    Console.ReadKey(true);
                }
                Console.Clear();
            } while (!ok);

            Console.CursorVisible = false;
            return name;
        }
        /// <summary>
        /// .mine betöltése
        /// </summary>
        public static void LoadGame()
        {
            string name = NameLoad();

            if (name == "-")
            {
                Program.Reset();
                return;
            }

            string path = $"Games/{name}.mine";
            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<GameData>(json, jsonOptions);
            if (data == null) return;

            Program.PublicMeretM = data.meretM;
            Program.PublicMeretSZ = data.meretSZ;
            Program.PublicCursorX = data.CursorX;
            Program.PublicCursorY = data.CursorY;
            Program.PublicAknakszama = data.aknakszama;
            Program.PublicFlagcount = data.flagged;

            Program.PublicAkna = new string[data.meretM, data.meretSZ];
            Program.PublicVisible = new string[data.meretM, data.meretSZ];

            foreach (var kv in data.akna)
            {
                var p = kv.Key.Split(',');
                int x = int.Parse(p[0]);
                int y = int.Parse(p[1]);
                Program.PublicAkna[x, y] = kv.Value;
            }

            foreach (var kv in data.visible)
            {
                var p = kv.Key.Split(',');
                int x = int.Parse(p[0]);
                int y = int.Parse(p[1]);
                Program.PublicVisible[x, y] = kv.Value;
            }

            for (int x = 0; x < data.meretM; x++)
            {
                for (int y = 0; y < data.meretSZ; y++)
                {
                    if (Program.PublicAkna[x, y] == null)
                        Program.PublicAkna[x, y] = Program.semmi;

                    if (Program.PublicVisible[x, y] == null)
                        Program.PublicVisible[x, y] = "false";
                }
            }
            Program.PublicSaveName = name;
            Program.LoadedGame = true;
        }
    }
}
