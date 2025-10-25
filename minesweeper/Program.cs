using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace minesweeper
{
    internal class Program
    {
        const string Version = "Beta 1.5";
        
        static string minemark = "*";
        static string semmi = " ";
        static string zaszlo = "!";
        static string fedes = "-";
        static int aknakszama = 0;
        static int meretM = 0;
        static int meretSZ = 0;
        static int flagcount = 0;
        static int cursor_x = 0;
        static int cursor_y = 0;
        static int marginDown = 5; //az az érték, hogy mennyi hely legyen a játéktábla alatt
        static int marginRight = 1;
        static ConsoleKey dig = ConsoleKey.W;
        static ConsoleKey flag = ConsoleKey.Spacebar;
        static ConsoleKey quit = ConsoleKey.Escape;
        static bool gameover = false;
        static string gameover_type = "false";
        static bool newgame = true;

        static ConsoleColor default_Background = ConsoleColor.Black;
        static ConsoleColor default_Foreground = ConsoleColor.White;
        static Dictionary<string, ConsoleColor> Szín_Betű = new Dictionary<string, ConsoleColor>()
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
            {"flag", ConsoleColor.White},
            {fedes, ConsoleColor.Blue}
        };
        static Dictionary<string, ConsoleColor> Szín_Háttér = new Dictionary<string, ConsoleColor>()
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
            {"flag", ConsoleColor.DarkBlue},
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
            do
            {
                Console.Title = "Aknakereső - Új";
                Menu();
                string[,] akna = new string[meretM, meretSZ];
                string[,] visible = new string[meretM, meretSZ];
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
        static void Draw(string[,] akna, string[,] visible, bool clear)
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
                    else if (visible[i, j] == "false")
                    {
                        Paint(fedes, "");
                    }
                    else if (visible[i, j] == "flag")
                    {
                        Paint("flag", "visible");
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
            if (ck == flag)
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
                    Paint("flag", "visible");
                    //Draw(akna, visible, true);
                    Nyeres_Ellenorzes(akna, visible);
                }
            }
            if (ck == dig)
            {
                Felfedes(akna, ref visible, cursor_y, cursor_x);
                Nyeres_Ellenorzes(akna, visible);
                Draw(akna, visible, false);
            }
            if (ck == quit)
            {
                gameover = true;
                gameover_type = "quit";
            }
            Console.SetCursorPosition(cursor_x, cursor_y);
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
            } while (ck != dig);

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
            Nyeres_Ellenorzes(akna, visible);
            Draw(akna, visible, false);
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
            Console.SetCursorPosition(0, 0);
            Console.Clear();
        }
        /// <summary>
        /// A Menü megjelenítése és használata
        /// </summary>
        static void Menu()
        {
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
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kisebb!");
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
                                Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kisebb!");
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
                        Environment.Exit(0);
                    break;
                }
                if (meretM <= Console.WindowHeight-marginDown && meretSZ <= Console.WindowWidth-marginRight)
                {
                    siker = true;
                }
                else
                {
                    Console.WriteLine("A kiválasztott játékterület nem fér ki a képernyőre!");
                    Console.ReadKey(true);
                }

            } while (!siker);
            Console.CursorVisible = true;
        }
        /// <summary>
        /// Aknakkereső ASCII kiírása
        /// </summary>
        static void ASCII()
        {
            Console.WriteLine(@"
    _    _                _                        ____
   / \  | | ___ __   __ _| | _____ _ __ ___  ___  /_/_/
  / _ \ | |/ / '_ \ / _` | |/ / _ \ '__/ _ \/ __|/ _ \ 
 / ___ \|   <| | | | (_| |   <  __/ | |  __/\__ \ |_| |
/_/   \_\_|\_\_| |_|\__,_|_|\_\___|_|  \___||___/\___/ ");
            Console.WriteLine("\n"+Version+"\n");
        }
        static void Paint(string write, string from)
        {
            bool van = false;
            if (Szín_Háttér.ContainsKey(write)) Console.BackgroundColor = Szín_Háttér[write]; van = true;
            if (Szín_Betű.ContainsKey(write)) Console.ForegroundColor = Szín_Betű[write]; van = true;
            if (van)
            {
                if (from == "akna" || from == "")
                {
                    Console.Write(write);
                }else
                if (from == "visible")
                {
                    if (write == "flag")
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
    }
}
