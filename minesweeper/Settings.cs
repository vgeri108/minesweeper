using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minesweeper
{
    internal class Beállítások
    {
        /// <summary>
        /// Beállításokon belül a Színek menü
        /// </summary>
        public static void Színek()
        {
            string[] options = {
                    "Háttér színek",
                    "Betűszínek",
                    "Vissza"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.SetCursorPosition(0, 0);
                Program.ASCII();
                Console.WriteLine("Szín beállítások:");
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
                    Console.Clear();
                    Beállítások.Hátterek();
                    Console.Clear();
                    break;
                case 1:
                    Console.Clear();
                    Beállítások.Betűszín();
                    Console.Clear();
                    break;
            }
        }
        /// <summary>
        /// Beállításokon belül a Hátterek menü
        /// </summary>
        public static void Hátterek()
        {
            string[] options = {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8",
                    Program.minemark,
                    Program.zaszlo,
                    Program.fedes,
                };
            int selected = 0;
            ConsoleKey key;
            ConsoleColor[] colorsVektor = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            List<ConsoleColor> colors = colorsVektor.ToList();
            int[] selected_color = new int[options.Length];
            bool first = true;
            do
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("--==## Háttérszín ##==--");
                Console.WriteLine();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                    int id = i + 1;
                    if (Program.Szín_Betű.ContainsKey(options[i])) Console.ForegroundColor = Program.Szín_Betű[options[i]];
                    if (first)
                    {
                        if (Program.Szín_Háttér.ContainsKey(options[i]))
                        {
                            Console.BackgroundColor = Program.Szín_Háttér[options[i]];
                            selected_color[i] = colors.IndexOf(Program.Szín_Háttér[options[i]]);
                        }
                    }
                    else
                    {
                        Console.BackgroundColor = colors[selected_color[i]];
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }
                first = false;
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow && selected > 0)
                    selected--;
                else if (key == ConsoleKey.DownArrow && selected < options.Length - 1)
                    selected++;
                else if (key == ConsoleKey.LeftArrow)
                {
                    if (selected_color[selected] - 1 > -1)
                    {
                        selected_color[selected]--;
                    }
                    else
                    {
                        selected_color[selected] = colors.Count - 1;
                    }
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (selected_color[selected] + 1 != colors.Count)
                    {
                        selected_color[selected]++;
                    }
                    else
                    {
                        selected_color[selected] = 0;
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        Program.Szín_Háttér[options[i]] = colors[selected_color[i]];
                    }
                    MyConfig.Save();
                    break;
                }
                else if (key == ConsoleKey.Escape) break;
            } while (key != ConsoleKey.Enter);
        }
        /// <summary>
        /// Mentéstörlés menü
        /// </summary>
        public class Törlés
        {
            /// <summary>
            /// Törlések főmenüje
            /// </summary>
            public static void Menü()
            {
                string[] options = {
                    "Egy mentés törlése",
                    "Összes mentés törlése",
                    "Vissza"
                };
                int selected = 0;
                ConsoleKey key;
                do
                {
                    Console.SetCursorPosition(0, 0);
                    Program.ASCII();
                    Console.WriteLine("Szín beállítások:");
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
                        Console.CursorVisible = true;
                        Beállítások.Törlés.Egy();
                        Console.CursorVisible = false;
                        break;
                    case 1:
                        Console.CursorVisible = true;
                        Beállítások.Törlés.Összes();
                        Console.CursorVisible = false;
                        break;
                }
            }
            /// <summary>
            /// Összes mentés törlése párbeszéd
            /// </summary>
            public static void Összes()
            {
                Console.Clear();
                Console.WriteLine("\nBiztosan szeretnéd törölni az ÖSSZES mentésedet? (i/n)");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ez nem vonható vissza!");
                Console.ResetColor();
                char answer = Console.ReadKey().KeyChar;
                if (answer == 'i')
                {
                    Console.Clear();
                    Console.WriteLine("Törlés folyamatban...");
                    Directory.CreateDirectory("Games");
                    string[] files = Directory.GetFiles("Games", "*.mine");
                    if (files.Length == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Nincs egyetlen mentés sem!");
                        Console.ResetColor();
                        Thread.Sleep(1500);
                    }
                    if (files.Length != 0)
                    {
                        foreach (string file in files)
                        {
                            File.Delete(file);
                        }
                    }
                    if (files.Length != 0)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nSikeres törlés!");
                        Console.ResetColor();
                        Thread.Sleep(1500);
                    }
                }
            }
            /// <summary>
            /// Egy mentés törlése párbeszéd
            /// </summary>
            public static void Egy()
            {
                bool done = false;
                string answerString;
                do
                {
                    bool ok = false;
                    do
                    {
                        Console.Clear();
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
                        Console.WriteLine("\nMelyiket szeretnéd törölni? (\"-\" a visszalépéshez)");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ez a művelet nem vonható vissza!");
                        Console.ResetColor();
                        answerString = Console.ReadLine()?.Trim() ?? "save";
                        if (File.Exists($"Games/{answerString}.mine"))
                        {
                            try
                            {
                                File.Delete($"Games/{answerString}.mine");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\nSikeres törlés!");
                                Console.ResetColor();
                                Thread.Sleep(1500);
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A fájlt nem lehetett törölni. Hibakód: " + e);
                                Console.ResetColor();
                                Thread.Sleep(1500);
                            }
                        }
                        else
                        {
                            if (answerString != "-")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A fájl nem létezik!");
                                Console.ResetColor();
                                Thread.Sleep(1500);
                            }
                            else if (answerString == "-") ok = true;
                        }
                    } while (!ok);
                    if (answerString != "-")
                    {
                        Console.Clear();
                        Console.WriteLine("\nSzeretnél másik fájlt is törölni? (i/n)");
                        char answer = Console.ReadKey().KeyChar;
                        if (answer == 'n') done = true;
                    }
                    else done = true;
                } while (!done);
            }
        }
        /// <summary>
        /// Betűszín beállításai a menüben
        /// </summary>
        public static void Betűszín()
        {
            string[] options = {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8",
                    Program.minemark,
                    Program.zaszlo,
                    Program.fedes,
                };
            int selected = 0;
            ConsoleKey key;
            ConsoleColor[] colorsVektor = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            List<ConsoleColor> colors = colorsVektor.ToList();
            int[] selected_color = new int[options.Length];
            bool first = true;
            do
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("--==## Betűszín ##==--");
                Console.WriteLine();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                    int id = i + 1;
                    if (Program.Szín_Háttér.ContainsKey(options[i])) Console.BackgroundColor = Program.Szín_Háttér[options[i]];
                    if (first)
                    {
                        if (Program.Szín_Háttér.ContainsKey(options[i]))
                        {
                            Console.ForegroundColor = Program.Szín_Betű[options[i]];
                            selected_color[i] = colors.IndexOf(Program.Szín_Betű[options[i]]);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = colors[selected_color[i]];
                    }
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }
                first = false;
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow && selected > 0)
                    selected--;
                else if (key == ConsoleKey.DownArrow && selected < options.Length - 1)
                    selected++;
                else if (key == ConsoleKey.LeftArrow)
                {
                    if (selected_color[selected] - 1 > -1)
                    {
                        selected_color[selected]--;
                    }
                    else
                    {
                        selected_color[selected] = colors.Count - 1;
                    }
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (selected_color[selected] + 1 != colors.Count)
                    {
                        selected_color[selected]++;
                    }
                    else
                    {
                        selected_color[selected] = 0;
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        Program.Szín_Betű[options[i]] = colors[selected_color[i]];
                        MyConfig.Save();
                    }
                    break;
                }
                else if (key == ConsoleKey.Escape) break;
            } while (key != ConsoleKey.Enter);
        }
        /// <summary>
        /// Irányítás beálításai a menüben
        /// </summary>
        public class Irányítás
        {
            /// <summary>
            /// Irányítások főmenüje
            /// </summary>
            public static void Irányítás_Menü()
            {
                string[] options = {
                    "Ásás billentyű",
                    "Zászlózás billentyű",
                    "Vissza"
                };
                int selected = 0;
                ConsoleKey key;
                do
                {
                    Console.SetCursorPosition(0, 0);
                    Program.ASCII();
                    Console.WriteLine("Irányítás beállítások:");
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
                        Console.CursorVisible = true;
                        Beállítások.Irányítás.Dig();
                        Console.CursorVisible = false;
                        break;
                    case 1:
                        Console.CursorVisible = true;
                        Beállítások.Irányítás.Flag();
                        Console.CursorVisible = false;
                        break;
                }
            }
            /// <summary>
            /// Ásás billentyűjének módosítása
            /// </summary>
            public static void Dig()
            {
                Console.Clear();
                Console.WriteLine("Ásás billentyű módosítása");
                Console.WriteLine("Eddig ez a gomb volt használva: " + Program.Billentyűk["dig"]);
                Console.WriteLine();
                Console.WriteLine("[Escape] a félbeszakításhoz");
                ConsoleKey readed = Console.ReadKey().Key;
                if (readed != ConsoleKey.Escape)
                {
                    Program.Billentyűk["dig"] = readed;
                }
            }
            /// <summary>
            /// Zászlózás billentyűjének módosítása
            /// </summary>
            public static void Flag()
            {
                Console.Clear();
                Console.WriteLine("Zászló billentyű módosítása");
                Console.WriteLine("Eddig ez a gomb volt használva: " + Program.Billentyűk["flag"]);
                Console.WriteLine();
                Console.WriteLine("[Escape] a félbeszakításhoz");
                ConsoleKey readed = Console.ReadKey().Key;
                if (readed != ConsoleKey.Escape)
                {
                    Program.Billentyűk["flag"] = readed;
                }
            }
        }
        /// <summary>
        /// Frissítések menü
        /// </summary>
        public static void Frissítés()
        {
            string[] options = {
                    "Frissítések keresése",
                    "Frissítések beállításai",
                    "Vissza"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.SetCursorPosition(0, 0);
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
                    Console.Clear();
                    Console.WriteLine("\nFrissítések keresése folyamatban...");
                    Update.Check();
                    if (!Program.frissités_elérhető)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nA legfrissebb verzió van telepítve!");
                        Console.ResetColor();
                        Console.ReadKey();
                    }
                    break;
                case 1:
                    Update.Menü.Főmenü();
                    break;
            }
        }
    }
}
