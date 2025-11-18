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

namespace minesweeper
{
    public class Program
    {
        public const string Version_type = "Pre-Build";
        public const string Version_Prefix = "1"; // latest: Beta 1.6.5
        public const string Version_Suffix = "6.6/3";

        public static string local_version = $"{Program.Version_type} {Program.Version_Prefix}.{Program.Version_Suffix}";
        public static string github_version = "NotSet";
        public static bool frissités_elérhető = false;
        public static string frissítés_info = "nincs";
        public static string frissítés_link = "about:blank";
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
        public static bool LoadedGame = false;

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

        static void Main(string[] args)
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
                    Console.WriteLine("Automatikus frissítés folyamatban...");
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
                StreamWriter sw = new StreamWriter("latestlog.txt");
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
                    case "akna": Console.WriteLine("Aknát találtál, ezért felrobbantál!"); break;
                    case "flagged": Console.WriteLine("Bejelölted az összes aknát, ami azt jelenti, hogy nyertél!"); break;
                    case "quit": Console.WriteLine("Kiléptél a jelenlegi játékból."); break;
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
                    if (visible[x,y] == "false")
                    {
                        if (akna[x, y] != minemark)
                        {
                            return;
                        }
                    }
                }
            }
            if (aknakszama == flagcount)
            {
                gameover = true;
                gameover_type = "flagged";
            }
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
        static void Reset()
        {
            gameover = false;
            gameover_type = "false";
            flagcount = 0;
            cursor_x = 0;
            cursor_y = 0;
            PublicAknakszama = 0;
            PublicFlagcount = 0;
            PublicMeretM = 0;
            PublicMeretSZ = 0;
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
                    Console.Clear();
                    ASCII();
                    Console.WriteLine("Válassz nehézségi szintet:");
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
                        do
                        {
                            converted = false;
                            Console.Write("Szélesség: ");
                            converted = int.TryParse(Console.ReadLine(), out meretSZ);
                            max = Console.WindowWidth;
                            if (!converted)
                            {
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                            }
                            else if (meretSZ < 2)
                            {
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kevesebb!");
                            }
                            else if (meretSZ > max - marginRight)
                            {
                                Console.WriteLine("A megadott szám kívül esik az ablak méretén!");
                                Console.WriteLine($"Az ablak mérete: {Console.WindowWidth - marginRight} × {Console.WindowHeight - marginDown}");
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
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                            }
                            else if (meretM < 2)
                            {
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kevesebb!");
                            }
                            else if (meretM > max - marginDown)
                            {
                                Console.WriteLine("A megadott szám kívül esik az ablak méretén!");
                                Console.WriteLine($"Az ablak mérete: {Console.WindowWidth - marginRight} × {Console.WindowHeight - marginDown}");
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
                                Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                            }
                            else if (aknakszama < 1)
                            {
                                Console.WriteLine("Az aknák száma nem lehet 0 vagy annál kevesebb!");
                            }
                            else if (aknakszama > max)
                            {
                                Console.WriteLine("Legalább egy üres helynek lennie kell a pályán, nem lehet annál többet megadni!");
                            }
                        } while (!(converted && aknakszama < max && aknakszama > 0));
                        siker = true;
                    break;
                    case 4:
                        startLoad = true;
                        MyConfig.LoadGame();
                        Console.WriteLine(PublicAkna.Length); //debug
                        if (PublicAkna != null && PublicAkna.Length > 0)
                        {
                            LoadedGame = true;
                            Console.WriteLine("Loaded game = " + LoadedGame); //debug
                        }
                        Thread.Sleep(5000);
                        break;
                    case 5:
                        Settings();
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
                    Console.WriteLine("A kiválasztott játékterület nem fér ki a képernyőre!");
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
                    "Színek módosítása",
                    "Irányítás módosítása",
                    "Frissítési beállítások",
                    "Vissza"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
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
                    Beállítások.Színek();
                break;
                case 1:
                    Beállítások.Irányítás.Irányítás_Menü();
                break;
                case 2:
                    Beállítások.Frissítés();
                break;
            }
        }
        /// <summary>
        /// Escape menü
        /// </summary>
        static void Quit(string[,] akna, string[,] visible)
        {
            string[] options = {
                    "Vissza a játékba",
                    "Mentés",
                    "Kilépés mentés nélkül"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
                Console.WriteLine("\nJáték megállítva:");
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
                    PublicFlagcount = flagcount;
                    MyConfig.SaveGame(akna, visible);
                    break;
            }
            Draw(akna, visible, true, false);
        }
    }
    /// <summary>
    /// A beállítások menüponttjai
    /// </summary>
    class Beállítások
    {
        public static void Színek()
        {
            string[] options = {
                    "Hátter színek módosítása",
                    "Betűszínek módosítása",
                    "Vissza"
                };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
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
                case 0: Beállítások.Hátterek(); break;
                case 1: Beállítások.Betűszín(); break;
            }
        }
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
                Console.Clear();
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
                Console.Clear();
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
        public class Irányítás
        {
            public static void Irányítás_Menü()
            {
                string[] options = {
                    "Ásás billentyű módosítás",
                    "Zászlózás billentyű módosítás",
                    "Vissza"
                };
                int selected = 0;
                ConsoleKey key;
                do
                {
                    Console.Clear();
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
        public static void Frissítés()
        {
            string[] options = {
                    "Frissítések keresése",
                    "Automatikus frissítések beállításai",
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
                    Console.Clear();
                    Console.WriteLine("\nFrissítések keresése folyamatban...");
                    Update.Check();
                    if (!Program.frissités_elérhető)
                    {
                        Console.WriteLine("\nA legfrissebb verzió van telepítve!");
                        Console.ReadKey();
                    }
                        break;
                case 1:
                    Update.Menü.Főmenü();
                    break;
            }
        }
    }
    class Update
    {
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
                        Program.frissítés_info = sorok[3];
                        Program.frissítés_link = sorok[4];
                        if (Program.frissités_elérhető)
                        {
                            Kérdez();
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public static void Install()
        {
            Console.Clear();
            string url = "https://github.com/vgeri108/minesweeper/raw/refs/heads/master/inno-setup/scripts/Output/minesweeper_setup.exe";
            string filePath = Path.Combine(Environment.CurrentDirectory, "minesweeper_setup.exe");
            try
            {
                Console.WriteLine("\nFájl letöltése folyamatban...");
                using (HttpClient client = new HttpClient())
                {
                    byte[] data = client.GetByteArrayAsync(url).Result;
                    File.WriteAllBytes(filePath, data);
                    File.Create("minesweeper_setup.exe");
                }

                Console.WriteLine($"A letöltés sikeresen befejeződött!");
                if (!Program.UpdateConfig["auto_update"])
                {
                    Console.Write("\nSzeretnéd elindítani a telepítőt? (i/n): ");
                    char answer = Console.ReadKey(true).KeyChar;
                    if (answer == 'i')
                    {
                        Console.WriteLine("\nTelepítő indítása...");
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.WriteLine("\nTelepítő indítása...");
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
            }
        }
        public static void Kérdez()
        {
            Console.Clear();
            Console.WriteLine($"\nEgy frissítés érhető el: {Program.github_version}.\n");
            Console.WriteLine("Frissítési megjegyzés: " + Program.frissítés_info);
            Console.WriteLine("Frissítés linke: " + Program.frissítés_link);
            Console.WriteLine();
            Console.WriteLine("Szeretnéd letölteni? (i/n)");
            char answer = Console.ReadKey(true).KeyChar;
            if (answer == 'i')
            {
                Install();
            }
        }
        public class Menü
        {
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
    class MyConfig
    {
        private static string configPath = "config.json";
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        public class ConfigData
        {
            public Dictionary<string, string> Irányítás { get; set; } = new();
            public Dictionary<string, string> UpdateConfig { get; set; } = new();
            public Dictionary<string, string> Szín_Háttér { get; set; } = new();
            public Dictionary<string, string> Szín_Betű { get; set; } = new();
        }
        public class GameData
        {
            public Dictionary<string, string> akna { get; set; } = new();
            public Dictionary<string, string> visible { get; set; } = new();
            public Dictionary<string, string> meretM { get; set; } = new();
            public Dictionary<string, string> meretSZ { get; set; } = new();
            public Dictionary<string, string> aknakszama { get; set; } = new();
            public Dictionary<string, string> flagged { get; set; } = new();
        }

        public static void Save()
        {
            var config = new ConfigData
            {
                Irányítás = Program.Billentyűk.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                UpdateConfig = Program.UpdateConfig.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                Szín_Háttér = Program.Szín_Háttér.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
                Szín_Betű = Program.Szín_Betű.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()),
            };

            string json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(configPath, json);
        }

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
        private static string NameSave()
        {
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
                    string test = name + ".test";
                    File.Create(test).Close();
                    File.Delete(test);
                }
                catch (Exception e)
                {
                    ok = false;
                    File.AppendAllText("latestlog.txt", e.Message);
                    Console.WriteLine("Érvénytelen fájlnév!");
                }
                if (name == "-")
                {
                    return "-";
                }
                if (File.Exists(name + ".mine"))
                {
                    ok = false;
                    Console.WriteLine("Már létezik ilyen nevű mentés!");
                }

            } while (!ok);
            Console.CursorVisible = false;
            return name;
        }
        public static void SaveGame(string[,] akna, string[,] visible)
        {
            string name = "Save";
            name = NameSave();
            if (name != "-")
            {
                var config = new GameData();
                for (int x = 0; x < akna.GetLength(0); x++)
                {
                    for (int y = 0; y < akna.GetLength(1); y++)
                    {
                        config.akna[$"{x},{y}"] = akna[x, y];
                        config.visible[$"{x},{y}"] = visible[x, y];
                    }
                }
                config.meretM["meretM"] = Program.PublicMeretM.ToString();
                config.meretSZ["meretSZ"] = Program.PublicMeretSZ.ToString();
                config.aknakszama["aknakszama"] = Program.PublicAknakszama.ToString();
                config.flagged["flagged"] = Program.PublicFlagcount.ToString();

                string json = JsonSerializer.Serialize(config, jsonOptions);
                File.WriteAllText($"{name}.mine", json);
            }
        }
        private static string NameLoad()
        {
            Console.CursorVisible = true;
            string name;
            bool ok;
            Console.Clear();
            do
            {
                ok = true;
                Console.WriteLine("Játék betöltése (\"-\" a visszalépéshez)\n");
                Console.Write("A mentés neve: ");
                name = Console.ReadLine()?.Trim() ?? "save";
                if (name == "-")
                {
                    return "-";
                }
                if (!File.Exists(name + ".mine"))
                {
                    ok = false;
                    Console.WriteLine("A fájl nem létezik!");
                }
            } while (!ok);
            Console.CursorVisible = false;
            return name;
        }
        public static void LoadGame()
        {
            string name = NameLoad();
            if (name == "-")
            {
                return;
            }
            string path = $"{name}.mine";
            if (!File.Exists(path))
            {
                Console.WriteLine("File nem létezik"); //debug
                return;
            }

            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<GameData>(json, jsonOptions);
            if (data == null)
                return;

            foreach (var kv in data.meretM)
            {
                Program.PublicMeretM = Convert.ToInt32(kv.Value);
                Console.WriteLine(Program.PublicMeretM); //debug
            }

            foreach (var kv in data.meretSZ)
            {
                Program.PublicMeretSZ = Convert.ToInt32(kv.Value);
            }

            foreach (var kv in data.aknakszama)
            {
                Program.PublicAknakszama = Convert.ToInt32(kv.Value);
            }

            foreach (var kv in data.flagged)
            {
                Program.PublicFlagcount = Convert.ToInt32(kv.Value);
            }

            Program.PublicAkna = new string[Program.PublicMeretM, Program.PublicMeretSZ];
            foreach (var kv in data.akna)
            {
                var parts = kv.Key.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                Program.PublicAkna[x, y] = kv.Value;
            }

            Program.PublicVisible = new string[Program.PublicMeretM, Program.PublicMeretSZ];
            foreach (var kv in data.visible)
            {
                var parts = kv.Key.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                Program.PublicVisible[x, y] = kv.Value;
            }
            Console.WriteLine(Program.PublicVisible[0,0]); //debug
            if (name == "-")
            {
                Program.PublicAknakszama = 0;
                Program.PublicFlagcount = 0;
                Program.PublicMeretM = 0;
                Program.PublicMeretSZ = 0;
                Program.PublicAkna = new string[0, 0];
                Program.PublicVisible = new string[0, 0];
            }
        }
    }
}