using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace minesweeper
{
    public class Program
    {
        public const string Version_type = "Relase";
        public const string Version_Prefix = "1"; // latest: Relase 1.8.1
        public const string Version_Suffix = "8.1";

        public static string local_version = $"{Program.Version_type} {Program.Version_Prefix}.{Program.Version_Suffix}";
        public static string github_version = "NotSet";
        public static bool frissités_elérhető = false;
        public static Dictionary<string, bool> UpdateConfig = new Dictionary<string, bool>()
        {
            {"auto_check", true},
            {"auto_update", false}
        };

        public static string minemark = "*";
        public static string semmi = " ";
        public static string zaszlo = "!";
        public static string fedes = "-";
        static int aknakszama = 0;
        static int meretM = 0;
        static int meretSZ = 0;
        static int flagcount = 0;
        static int cursor_x = 0;
        static int cursor_y = 0;
        static int marginDown = 5;
        static int marginRight = 1;

        static ConsoleKey quit = ConsoleKey.Escape;
        public static Dictionary<string, ConsoleKey> Billentyűk = new Dictionary<string, ConsoleKey>()
        {
            {"dig", ConsoleKey.W},
            {"flag", ConsoleKey.Spacebar}
        };

        static bool gameover = false;
        static string gameover_type = "false";
        static bool newgame = true;
        static bool settings_opened = false;

        public static string[,] PublicAkna = { };
        public static string[,] PublicVisible = { };
        public static int PublicMeretM = 0;
        public static int PublicMeretSZ = 0;
        public static int PublicAknakszama = 0;
        public static int PublicFlagcount = 0;
        public static int PublicCursorX = 0;
        public static int PublicCursorY = 0;
        public static bool LoadedGame = false;
        public static string PublicSaveName = "-";

        static ConsoleColor default_Background = ConsoleColor.Black;
        static ConsoleColor default_Foreground = ConsoleColor.White;
        public static Dictionary<string, ConsoleColor> Szín_Betű = new Dictionary<string, ConsoleColor>()
        {
            {"1", ConsoleColor.Blue},
            {"2", ConsoleColor.DarkGreen},
            {"3", ConsoleColor.Red},
            {"4", ConsoleColor.DarkBlue},
            {"5", ConsoleColor.DarkRed},
            {"6", ConsoleColor.DarkCyan},
            {"7", ConsoleColor.DarkMagenta},
            {"8", ConsoleColor.Magenta},
            {minemark, default_Foreground},
            {zaszlo, ConsoleColor.White},
            {fedes, ConsoleColor.Blue}
        };
        public static Dictionary<string, ConsoleColor> Szín_Háttér = new Dictionary<string, ConsoleColor>()
        {
            {"1", default_Background},
            {"2", default_Background},
            {"3", default_Background},
            {"4", default_Background},
            {"5", default_Background},
            {"6", default_Background},
            {"7", default_Background},
            {"8", default_Background},
            {minemark, ConsoleColor.Red},
            {zaszlo, ConsoleColor.DarkBlue},
            {fedes, ConsoleColor.DarkBlue}
        };

        public static string frissítés_sor_1 = " ";
        public static string frissítés_sor_2 = " ";
        public static string frissítés_sor_3 = " ";
        public static string frissítés_sor_4 = " ";
        public static string frissítés_sor_5 = " ";
        public static string frissítés_sor_6 = " ";
        public static string frissítés_sor_7 = " ";

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        const uint ENABLE_INSERT_MODE = 0x0020;
        const uint ENABLE_MOUSE_INPUT = 0x0010;

        public static bool VanInternet()
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 500);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
                if (GetConsoleMode(handle, out uint mode))
                {
                    mode &= ~ENABLE_QUICK_EDIT_MODE;
                    mode &= ~ENABLE_INSERT_MODE;
                    mode &= ~ENABLE_MOUSE_INPUT;
                    SetConsoleMode(handle, mode);
                }
            }
            catch { }

            if (!File.Exists("config.json"))
            {
                MyConfig.Save();
            }
            MyConfig.Load();

            if (UpdateConfig["auto_check"])
            {
                Console.Title = "Frissítések keresése...";
                Update.Check();
                Console.Title = "Aknakereső - Frissítés";
                if (frissités_elérhető && UpdateConfig["auto_update"])
                {
                    Console.WriteLine("Frissítés folyamatban...");
                    Update.Install();
                }
            }

            try
            {
                if (File.Exists("minesweeper_setup.exe"))
                {
                    File.Delete("minesweeper_setup.exe");
                }
            }catch (Exception e)
            {
                StreamWriter sw = new StreamWriter("latest_error.txt");
                sw.WriteLine(e);
                sw.Flush();
                sw.Close();
            }

            do
            {
                Console.Title = "Aknakereső";
                do
                {
                    Menu();
                } while (settings_opened);
                if (LoadedGame)
                {
                    meretM = PublicMeretM;
                    meretSZ = PublicMeretSZ;
                }
                string[,] akna = new string[meretM, meretSZ];
                string[,] visible = new string[meretM, meretSZ];
                if (LoadedGame)
                {
                    akna = PublicAkna;
                    visible = PublicVisible;
                    aknakszama = PublicAknakszama;
                    flagcount = PublicFlagcount;
                    cursor_x = PublicCursorX;
                    cursor_y = PublicCursorY;
                }
                Console.Clear();
                Console.Title = "Aknakereső - Generálás...";
                Start(akna, ref visible);
                Console.SetCursorPosition(cursor_x, cursor_y);
                Console.Title = "Aknakereső - Játék";
                do { Select(akna, ref visible); } while (!gameover);
                Console.SetCursorPosition(0, meretM + 1);
                Console.WriteLine();
                Console.WriteLine("                      ");
                Console.SetCursorPosition(0, meretM + 1);
                switch (gameover_type)
                {
                    case "akna":
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Sajnos vesztett. Sok szerencsét a következő játékhoz!");
                        Console.ResetColor();
                        break;
                    case "flagged":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Gratulálunk, megnyerte a játékot!");
                        Console.ResetColor();
                        break;
                    case "quit":
                        Console.WriteLine("Kiléptél a jelenlegi játékból.  ");
                        break;
                }
                if (gameover_type == "flagged")
                {
                    if (PublicSaveName != "-")
                    {
                        Console.WriteLine($"\nSzeretnéd törölni a mentést: {PublicSaveName}? (i/n)");
                        char answer = Console.ReadKey().KeyChar;
                        if (answer == 'i')
                        {
                            try
                            {
                                File.Delete($"Games/{PublicSaveName}.mine");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Hiba! A fájlt nem lehetett törölni. Hibakód: " + e.Message);
                                Thread.Sleep(1500);
                            }
                        }
                    }
                }
                Console.ReadKey(true);
            } while (newgame);
        }
        /// <summary>
        /// Mátrixok feltöltése
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        static string[,] akna_letrehozas(ref string[,] akna, ref string[,] visible)
        {
            Random random = new Random();
            for (int i = 0; i < akna.GetLength(0); i++)
            {
                for (int j = 0; j < akna.GetLength(1); j++)
                {
                    akna[i, j] = semmi;
                    visible[i, j] = "false";
                }
            }
            for (int i = 0; i < aknakszama; i++)
            {
                int x, y;
                do
                {
                    x = random.Next(0, meretM);
                    y = random.Next(0, meretSZ);
                } while ((akna[x, y] != semmi) || (x == cursor_y && y == cursor_x));
                akna[x, y] = minemark;
            }
            return akna;
        }
        /// <summary>
        /// Aknák beszámozása
        /// </summary>
        /// <param name="akna"></param>
        /// <returns></returns>
        static string[,] Generate(ref string[,] akna)
        {
            int count = 0;
            for (int x = 0; x < akna.GetLength(0); x++)
            {
                for (int y = 0; y < akna.GetLength(1); y++)
                {
                    if (akna[x, y] != minemark)
                    {
                        count = 0;
                        if (x - 1 >= 0) //fel
                        {
                            if (akna[x - 1, y] == minemark) count++;
                        }
                        if (y - 1 >= 0) //balra
                        {
                            if (akna[x, y - 1] == minemark) count++;
                        }
                        if ((x - 1 >= 0) && (y - 1 >= 0)) //balra fel
                        {
                            if (akna[x - 1, y - 1] == minemark) count++;
                        }
                        if (x + 1 < meretM) //le
                        {
                            if (akna[x + 1, y] == minemark) count++;
                        }
                        if (((x - 1 >= 0) && (y + 1 < meretSZ))) //jobbra fel
                        {
                            if (akna[x - 1, y + 1] == minemark) count++;
                        }
                        if (y + 1 < meretSZ) //jobbra
                        {
                            if (akna[x, y + 1] == minemark) count++;
                        }
                        if ((y - 1 >= 0) && (x + 1 < meretM)) //balra le
                        {
                            if (akna[x + 1, y - 1] == minemark) count++;
                        }
                        if ((y + 1 < meretSZ) && (x + 1 < meretM)) //jobbra le
                        {
                            if (akna[x + 1, y + 1] == minemark) count++;
                        }
                        if (count == 0)
                        {
                            akna[x, y] = semmi;
                        }
                        else
                        {
                            akna[x, y] = Convert.ToString(count);
                        }
                        /*Console.Clear();
                        Console.WriteLine();
                        Draw(akna);
                        Console.WriteLine("A kurzor itt volt: x:{0}, y:{1}", x, y);
                        Console.WriteLine();
                        Console.ReadKey(false);*/
                    }
                }
            }
            return akna;
        }
        /// <summary>
        /// Tábla kirajzolása és színezése
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible">Ami true az fog látszódni</param>
        /// <param name="clear">Console.Clear() végrehajtása ha true</param>
        /// <param name="nincsFedes">Ha true akkor minden mező látható lesz.</param>
        static void Draw(string[,] akna, string[,] visible, bool clear, bool nincsFedes)
        {
            
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, meretM + 3);
            Console.WriteLine("Rajzolás...");
            if (clear)
            {
                Console.Clear();
            }
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < akna.GetLength(0); i++)
            {
                for (int j = 0; j < akna.GetLength(1); j++)
                {
                    if (visible[i, j] == "true")
                    {
                        Paint(akna[i, j], "akna");
                    }
                    else
                    {
                        if (!nincsFedes)
                        {
                            if (visible[i, j] == "false")
                            {
                                Paint(fedes, "");
                            }
                            else if (visible[i, j] == "flag")
                            {
                                Paint(zaszlo, "visible");
                            }
                        } else Paint(akna[i, j], "");
                    }
                }
                Console.WriteLine();
            }
            Console.SetCursorPosition(0, meretM + 3);
            Console.Write("           ");
            Status();
            Console.CursorVisible = true;
        }
        /// <summary>
        /// Kurzor vezérlése
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        static void Select(string[,] akna, ref string[,] visible)
        {
            /*var cur = Console.GetCursorPosition();
            int CurTop = Convert.ToInt32(cur.Top);
            int CurLeft = Convert.ToInt32(cur.Left);*/
            ConsoleKey ck = Console.ReadKey(true).Key;
            if (ck == ConsoleKey.UpArrow) if (cursor_y - 1 >= 0) cursor_y--;
            if (ck == ConsoleKey.DownArrow) if (cursor_y + 1 < meretM) cursor_y++;
            if (ck == ConsoleKey.LeftArrow) if (cursor_x - 1 >= 0) cursor_x--;
            if (ck == ConsoleKey.RightArrow) if (cursor_x + 1 < meretSZ) cursor_x++;
            if (ck == Billentyűk["flag"])
            {
                if (visible[cursor_y, cursor_x] == "flag")
                {
                    visible[cursor_y, cursor_x] = "false";
                    flagcount--;
                    Paint(fedes, "");
                    Nyeres_Ellenorzes(akna, visible);
                }
                else if (visible[cursor_y, cursor_x] == "false")
                {
                    visible[cursor_y, cursor_x] = "flag";
                    flagcount++;
                    Paint(zaszlo, "visible");
                    Nyeres_Ellenorzes(akna, visible);
                }
            }
            if (ck == Billentyűk["dig"])
            {
                Felfedes(akna, ref visible, cursor_y, cursor_x);
                Nyeres_Ellenorzes(akna, visible);
                Draw(akna, visible, false, false);
            }
            if (ck == quit)
            {
                Quit(akna, visible);
            }
            Console.SetCursorPosition(cursor_x, cursor_y);
            if (gameover_type == "akna")
            {
                Draw(akna, visible, false, true);
            }
        }
        /// <summary>
        /// Blokk kiütés utáni ellenőrzések
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        static void Felfedes(string[,] akna, ref string[,] visible, int x, int y)
        {
            if (x < 0 || x >= meretM || y < 0 || y >= meretSZ) return;
            if (visible[x, y] == "true" || visible[x, y] == "flag") return;
            visible[x, y] = "true";
            if (akna[x, y] == semmi)
            {
                Felfedes(akna, ref visible, x - 1, y); //fel
                Felfedes(akna, ref visible, x + 1, y); //le
                Felfedes(akna, ref visible, x, y - 1); //bal
                Felfedes(akna, ref visible, x, y + 1); //jobb
                Felfedes(akna, ref visible, x - 1, y - 1); //bal-fel
                Felfedes(akna, ref visible, x - 1, y + 1); //jobb-fel
                Felfedes(akna, ref visible, x + 1, y - 1); //bal-le
                Felfedes(akna, ref visible, x + 1, y + 1); //jobb-le
            }
            if (akna[x, y] == minemark)
            {
                gameover = true;
                gameover_type = "akna";
            }
        }
        /// <summary>
        /// Nyerés ellenőrzés
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        static void Nyeres_Ellenorzes(string[,] akna, string[,] visible)
        {
            Console.Title = "Aknakereső - Játék";
            Status();
            for (int x = 0; x < akna.GetLength(0); x++)
            {
                for (int y = 0; y < akna.GetLength(1); y++)
                {
                    if (akna[x, y] != minemark && visible[x, y] != "true")
                    {
                        return;
                    }
                }
            }
            gameover = true;
            gameover_type = "flagged";
        }
        /// <summary>
        /// A kezdő hely kiválasztása és "aknamentes generálás"
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        static void Start(string[,] akna, ref string[,] visible)
        {
            if (!LoadedGame)
            {
                Console.Title = "Aknakereső - Kezdés";
                Status();
                Console.SetCursorPosition(0, 0);
                for (int i = 0; i < akna.GetLength(0); i++)
                {
                    for (int j = 0; j < akna.GetLength(1); j++)
                    {
                        Paint(fedes, "");
                    }
                    Console.WriteLine();
                }
                Console.ResetColor();
                Console.SetCursorPosition(0, 0);
                ConsoleKey ck;
                do
                {
                    ck = Console.ReadKey(true).Key;
                    var cur = Console.GetCursorPosition();
                    int CurTop = Convert.ToInt32(cur.Top);
                    int CurLeft = Convert.ToInt32(cur.Left);
                    if (ck == ConsoleKey.UpArrow) if (cursor_y - 1 >= 0) cursor_y--;
                    if (ck == ConsoleKey.DownArrow) if (cursor_y + 1 < meretM) cursor_y++;
                    if (ck == ConsoleKey.LeftArrow) if (cursor_x - 1 >= 0) cursor_x--;
                    if (ck == ConsoleKey.RightArrow) if (cursor_x + 1 < meretSZ) cursor_x++;
                    Console.SetCursorPosition(cursor_x, cursor_y);
                } while (ck != Billentyűk["dig"]);

                bool vanUres;
                bool siker = false;
                for (int tries = 0; tries < 1000 && !siker; tries++)
                {
                    akna_letrehozas(ref akna, ref visible);
                    Generate(ref akna);

                    vanUres = false;
                    for (int x = 0; x < akna.GetLength(0); x++)
                    {
                        for (int y = 0; y < akna.GetLength(1); y++)
                        {
                            if (akna[x, y] == semmi)
                            {
                                vanUres = true;
                                break;
                            }
                        }
                        if (vanUres) break;
                    }

                    if (akna[cursor_y, cursor_x] == semmi)
                    {
                        siker = true;
                    }
                    else if (!vanUres && akna[cursor_y, cursor_x] != minemark)
                    {
                        siker = true;
                    }
                }
                Felfedes(akna, ref visible, cursor_y, cursor_x);
                LoadedGame = false;
            }
            Nyeres_Ellenorzes(akna, visible);
            Draw(akna, visible, false, false);
        }
        /// <summary>
        /// Játék belső változóinak alaphelyzetbe állítása
        /// </summary>
        public static void Reset()
        {
            PublicSaveName = "-";
            LoadedGame = false;
            gameover = false;
            gameover_type = "false";
            flagcount = 0;
            cursor_x = 0;
            cursor_y = 0;
            PublicAknakszama = 0;
            PublicFlagcount = 0;
            PublicMeretM = 0;
            PublicMeretSZ = 0;
            PublicCursorX = 0;
            PublicCursorY = 0;
            PublicAkna = new string[0,0];
            PublicVisible = new string[0,0];
            Console.SetCursorPosition(0, 0);
            Console.Clear();
        }
        /// <summary>
        /// A Menü megjelenítése és használata
        /// </summary>
        static void Menu()
        {
            settings_opened = false;
            Console.CursorVisible = false;
            Reset();
            int max;
            bool siker = false;
            do
            {
                string[] options = {
                    "Könnyű (9x9, 10 akna)",
                    "Közepes (16x16, 40 akna)",
                    "Nehéz (16x30, 99 akna)",
                    "Egyedi pálya",
                    "Játék betöltése",
                    "Beállítások",
                    "Kilépés"
                };
                int selected = 0;
                ConsoleKey key;
                do
                {
                    Console.SetCursorPosition(0,0);
                    ASCII();
                    Console.WriteLine("=== Főmenü ===\n");
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
                bool startLoad = false;
                switch (selected)
                {
                    case 0:
                        meretM = 9;
                        meretSZ = 9;
                        aknakszama = 10;
                        break;
                    case 1:
                        meretM = 16;
                        meretSZ = 16;
                        aknakszama = 40;
                        break;
                    case 2:
                        meretM = 16;
                        meretSZ = 30;
                        aknakszama = 99;
                        break;
                    case 3:
                        bool converted = false;
                        Console.CursorVisible = true;
                        Console.Clear();
                        Console.WriteLine("\nEgyedi méretű pálya létrehozása");
                        Console.WriteLine($"Az ablak mérete: {Console.WindowWidth - marginRight} × {Console.WindowHeight - marginDown}\n");
                        do
                        {
                            converted = false;
                            Console.Write("Szélesség: ");
                            converted = int.TryParse(Console.ReadLine(), out meretSZ);
                            max = Console.WindowWidth;
                            if (!converted)
                            {

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                                Console.ResetColor();
                            }
                            else if (meretSZ < 2)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kevesebb!");
                                Console.ResetColor();
                            }
                            else if (meretSZ > max - marginRight)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A megadott szám kívül esik az ablak méretén!");
                                Console.WriteLine($"Az ablak mérete: {Console.WindowWidth - marginRight} × {Console.WindowHeight - marginDown}");
                                Console.ResetColor();
                            }
                        } while (!(converted && meretSZ <= Console.WindowWidth - marginRight && meretSZ > 1));
                        do
                        {
                            converted = false;
                            Console.Write("Magasság: ");
                            converted = int.TryParse(Console.ReadLine(), out meretM);
                            max = Console.WindowHeight;
                            if (!converted)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                                Console.ResetColor();
                            }
                            else if (meretM < 2)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kevesebb!");
                                Console.ResetColor();
                            }
                            else if (meretM > max - marginDown)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A megadott szám kívül esik az ablak méretén!");
                                Console.WriteLine($"Az ablak mérete: {Console.WindowWidth - marginRight} × {Console.WindowHeight - marginDown}");
                                Console.ResetColor();
                            }
                        } while (!(converted && meretM <= Console.WindowHeight - marginDown && meretM > 1));

                        max = (meretM * meretSZ) - 1;
                        do
                        {
                            converted = false;
                            Console.Write("Aknák száma: ");
                            converted = int.TryParse(Console.ReadLine(), out aknakszama);
                            if (!converted)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                                Console.ResetColor();
                            }
                            else if (aknakszama < 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Az aknák száma nem lehet 0 vagy annál kevesebb!");
                                Console.ResetColor();
                            }
                            else if (aknakszama > max)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Legalább egy üres helynek lennie kell a pályán, nem lehet annál többet megadni!");
                                Console.ResetColor();
                            }
                        } while (!(converted && aknakszama < max && aknakszama > 0));
                        siker = true;
                    break;
                    case 4:
                        startLoad = true;
                        MyConfig.LoadGame();
                        if (PublicAkna != null && PublicAkna.Length > 0)
                        {
                            LoadedGame = true;
                        }
                        Console.CursorVisible = false;
                        break;
                    case 5:
                        Console.Clear();
                        Settings();
                        Console.Clear();
                    break;
                    case 6:
                        Environment.Exit(0);
                    break;
                }
                if (settings_opened) break;
                if (meretM <= Console.WindowHeight-marginDown && meretSZ <= Console.WindowWidth-marginRight)
                {
                    siker = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("A kiválasztott játékterület nem fér ki a képernyőre!");
                    Console.ResetColor();
                    Console.ReadKey(true);
                }
                if (startLoad)
                {
                    if (!LoadedGame)
                    {
                        siker = false;
                    }
                }
                startLoad = false;
            } while (!siker);
            Console.CursorVisible = true;
        }
        /// <summary>
        /// Aknakkereső ASCII kiírása
        /// </summary>
        public static void ASCII()
        {
            Console.WriteLine(@"
    _    _                _                        ____
   / \  | | ___ __   __ _| | _____ _ __ ___  ___  /_/_/
  / _ \ | |/ / '_ \ / _` | |/ / _ \ '__/ _ \/ __|/ _ \ 
 / ___ \|   <| | | | (_| |   <  __/ | |  __/\__ \ |_| |
/_/   \_\_|\_\_| |_|\__,_|_|\_\___|_|  \___||___/\___/ ");
            Console.WriteLine($"\n{local_version}\n");
        }
        /// <summary>
        /// Színezés eljárás, a megadott szöveget a megadott színnel rajzolja ki a pályán
        /// </summary>
        /// <param name="write"></param>
        /// <param name="from"></param>
        static void Paint(string write, string from)
        {
            bool van = false;
            if (Szín_Háttér.ContainsKey(write)) Console.BackgroundColor = Szín_Háttér[write]; van = true; //ex2
            if (Szín_Betű.ContainsKey(write)) Console.ForegroundColor = Szín_Betű[write]; van = true;
            if (van)
            {
                if (from == "akna" || from == "")
                {
                    Console.Write(write);
                }else
                if (from == "visible")
                {
                    if (write == zaszlo)
                    {
                        Console.Write(zaszlo);
                    }
                }
            }
            Console.ResetColor();
        }
        /// <summary>
        /// Ez írja ki a játéktér alá, hogy hány akna van még hátra és mekkor a pálya
        /// </summary>
        static void Status()
        {
            Console.SetCursorPosition(0, meretM + 1);
            Console.WriteLine("                       ");
            Console.SetCursorPosition(0, meretM + 1);
            Console.WriteLine("Hátralévő aknák: " + (aknakszama - flagcount));
            Console.Write("               ");
            Console.SetCursorPosition(0, meretM + 2);
            Console.WriteLine("Méret: " + meretSZ + " × " + meretM);
        }
        /// <summary>
        /// Beállítások menü
        /// </summary>
        static void Settings()
        {
            settings_opened = true;
            string[] options = {
                    "Színek",
                    "Irányítás",
                    "Frissítések",
                    "Mentések törlése",
                    "Vissza"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.SetCursorPosition(0,0);
                Program.ASCII();
                Console.WriteLine("Beállítások:");
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
                    Beállítások.Színek();
                    Console.Clear();
                break;
                case 1:
                    Console.Clear();
                    Beállítások.Irányítás.Irányítás_Menü();
                    Console.Clear();
                    break;
                case 2:
                    Console.Clear();
                    Beállítások.Frissítés();
                    Console.Clear();
                    break;
                case 3:
                    Console.Clear();
                    Beállítások.Törlés.Menü();
                    Console.Clear();
                    break;
            }
        }
        /// <summary>
        /// Escape menü
        /// </summary>
        static void Quit(string[,] akna, string[,] visible)
        {
            Console.Clear();
            string[] options = {
                    "Vissza a játékba",
                    "Mentés",
                    "Kilépés"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("\nJáték megállítva:");
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                        Console.Write("> ");
                    else
                        Console.Write("  ");
                    if (i == 2) Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(options[i]);
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow && selected > 0)
                    selected--;
                else if (key == ConsoleKey.DownArrow && selected < options.Length - 1)
                    selected++;
            } while (key != ConsoleKey.Enter);
            switch (selected)
            {
                case 0: break;
                case 2:
                    Program.gameover = true;
                    Program.gameover_type = "quit";
                    break;
                case 1:
                    PublicAkna = akna;
                    PublicVisible = visible;
                    PublicAknakszama = aknakszama;
                    PublicMeretM = meretM;
                    PublicMeretSZ = meretSZ;
                    PublicCursorX = cursor_x;
                    PublicCursorY = cursor_y;
                    PublicFlagcount = flagcount;
                    MyConfig.SaveGame(akna, visible);
                    break;
            }
            Draw(akna, visible, true, false);
        }
    }
}