using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minesweeper
{
    internal class Update
    {
        /// <summary>
        /// Frissítés ellenőrzés
        /// Kérdez() meghívása
        /// </summary>
        public static void Check()
        {
            if (Program.VanInternet())
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string content = client.GetStringAsync("https://raw.githubusercontent.com/vgeri108/minesweeper/refs/heads/master/minesweeper/version.txt").Result;
                        List<string> sorok = new List<string>(content.Split('\n'));
                        Program.github_version = $"{sorok[0]} {sorok[1]}.{sorok[2]}";
                        Program.frissités_elérhető = !(Program.local_version == Program.github_version);
                        Program.frissítés_sor_1 = sorok[3];
                        Program.frissítés_sor_2 = sorok[4];
                        Program.frissítés_sor_3 = sorok[5];
                        Program.frissítés_sor_4 = sorok[6];
                        Program.frissítés_sor_5 = sorok[7];
                        Program.frissítés_sor_6 = sorok[8];
                        Program.frissítés_sor_7 = sorok[9];
                        if (Program.frissités_elérhető)
                        {
                            Kérdez();
                        }
                    }
                }
                catch (Exception e)
                {
                    StreamWriter sw = new StreamWriter("latest_error.txt");
                    sw.WriteLine(e.Message);
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        /// <summary>
        /// Frissítés telepítése
        /// </summary>
        public static void Install()
        {
            Console.Clear();
            string url = "https://github.com/vgeri108/minesweeper/raw/refs/heads/master/inno-setup/scripts/Output/minesweeper_setup.exe";
            string filePath = Path.Combine(Environment.CurrentDirectory, "minesweeper_setup.exe");
            try
            {
                Console.WriteLine("\nLetöltés folyamatban, kérjük várjon!");
                using (HttpClient client = new HttpClient())
                {
                    byte[] data = client.GetByteArrayAsync(url).Result;
                    File.WriteAllBytes(filePath, data);
                    //File.Create("minesweeper_setup.exe");
                }

                Console.WriteLine("\nTelepítő indítása...");
                Process.Start(new ProcessStartInfo()
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
            }
        }
        /// <summary>
        /// Frissítés letöltése kérdés
        /// </summary>
        public static void Kérdez()
        {
            Console.Clear();
            Console.WriteLine($"\nEgy frissítés érhető el: {Program.github_version}.\n");
            Console.WriteLine(Program.frissítés_sor_1);
            Console.WriteLine(Program.frissítés_sor_2);
            Console.WriteLine(Program.frissítés_sor_3);
            Console.WriteLine(Program.frissítés_sor_4);
            Console.WriteLine(Program.frissítés_sor_5);
            Console.WriteLine(Program.frissítés_sor_6);
            Console.WriteLine(Program.frissítés_sor_7);
            Console.WriteLine();
            Console.WriteLine("Szeretnéd telepíteni? (i/n)");
            char answer = Console.ReadKey(true).KeyChar;
            if (answer == 'i')
            {
                Install();
            }
        }
        /// <summary>
        /// Frissítések menü
        /// </summary>
        public class Menü
        {
            /// <summary>
            /// Frissítések menü főmenüje
            /// </summary>
            public static void Főmenü()
            {
                string[] options = {
                "Automtikus frissítés keresések: " + (Program.UpdateConfig["auto_check"] ? "Bekapcsolva" : "Kikapcsolva"),
                "Automatikus frissítés telepítések: " + (Program.UpdateConfig["auto_update"] ? "Bekapcsolva" : "Kikapcsolva"),
                "Vissza"
            };
                int selected = 0;
                ConsoleKey key;
                do
                {
                    Console.Clear();
                    Program.ASCII();
                    Console.WriteLine("Frissítés beállítások:");
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (i == selected)
                            Console.Write("> ");
                        else
                            Console.Write("  ");
                        Console.WriteLine(options[i]);
                    }

                    key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.UpArrow && selected > 0)
                        selected--;
                    else if (key == ConsoleKey.DownArrow && selected < options.Length - 1)
                        selected++;
                } while (key != ConsoleKey.Enter);
                switch (selected)
                {
                    case 0:
                        Program.UpdateConfig["auto_check"] = !Program.UpdateConfig["auto_check"];
                        MyConfig.Save();
                        Főmenü();
                        break;
                    case 1:
                        Program.UpdateConfig["auto_update"] = !Program.UpdateConfig["auto_update"];
                        MyConfig.Save();
                        Főmenü();
                        break;
                }
            }
        }
    }
}
