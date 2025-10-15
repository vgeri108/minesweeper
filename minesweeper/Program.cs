using System.ComponentModel.Design;
using System.Runtime.ConstrainedExecution;

namespace minesweeper
{
    internal class Program
    {
        static string minemark = "*";
        static string semmi = " ";
        static string zaszlo = "!";
        static string fedes = "-";
        static int aknakszama = 0;
        static int meret = 0;
        static int flagcount = 0;
        static int cursor_x = 0;
        static int cursor_y = 0;
        static ConsoleKey dig = ConsoleKey.W;
        static ConsoleKey flag = ConsoleKey.Spacebar;
        static ConsoleKey quit = ConsoleKey.Escape;
        static bool gameover = false;
        static string gameover_type = "false";
        static bool newgame;
        static void Main(string[] args)
        {
            do
            {
                Console.Title = "Aknakereső - Debug 1.3.4 - Új játék létrehozása";
                Menu();
                string[,] akna = new string[meret, meret];
                string[,] visible = new string[meret, meret];
                Console.Clear();
                Console.Title = "Aknakereső - Játék generálása...";
                Start(akna, ref visible);
                Console.SetCursorPosition(cursor_x, cursor_y);
                Console.Title = $"Aknakereső - {meret} x {meret} - {aknakszama} aknával";
                do { Select(akna, ref visible); } while (!gameover);
                Console.SetCursorPosition(0, meret + 1);
                switch (gameover_type)
                {
                    case "akna": Console.WriteLine("Aknát találtál, ezért felrobbantál!"); break;
                    case "flagged": Console.WriteLine("Bejelölted az összes aknát, ami azt jelenti, hogy nyertél!"); break;
                    case "quit": Console.WriteLine("Kiléptél a jelenlegi játékból."); break;
                }
                Console.WriteLine();
                bool correct = false;
                do
                {
                    Console.WriteLine("Szeretnél új játékot kezdeni?");
                    string answer = Console.ReadLine();
                    if (answer == "igen" || answer == "IGEN" || answer == "Igen" || answer == "i" || answer == "I")
                    {
                        correct = true;
                        Reset();
                        newgame = true;
                    }
                    else
                    if (answer == "nem" || answer == "NEM" || answer == "Nem" || answer == "n" || answer == "N")
                    {
                        correct = true;
                        newgame = false;
                    }
                    else
                    {
                        correct = false;
                        Console.WriteLine("A válasz csak \"igen\" vagy \"nem\" lehet!");
                    }
                } while (!correct);
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
            for (int i = 0; i < akna.GetLength(1); i++)
            {
                for (int j = 0; j < akna.GetLength(0); j++)
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
                    x = random.Next(0, meret);
                    y = random.Next(0, meret);
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
                        if (x + 1 < meret) //le
                        {
                            if (akna[x + 1, y] == minemark) count++;
                        }
                        if (((x - 1 >= 0) && (y + 1 < meret))) //jobbra fel
                        {
                            if (akna[x - 1, y + 1] == minemark) count++;
                        }
                        if (y + 1 < meret) //jobbra
                        {
                            if (akna[x, y + 1] == minemark) count++;
                        }
                        if ((y - 1 >= 0) && (x + 1 < meret)) //balra le
                        {
                            if (akna[x + 1, y - 1] == minemark) count++;
                        }
                        if ((y + 1 < meret) && (x + 1 < meret)) //jobbra le
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
            
            if (clear)
            {
                //Console.Clear();
                Console.SetCursorPosition(0, 0);
            }
            for (int i = 0; i < akna.GetLength(1); i++)
            {
                for (int j = 0; j < akna.GetLength(0); j++)
                {
                    if (visible[i, j] == "true")
                    {
                        if (akna[i, j] == semmi)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else if (akna[i, j] == minemark)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                        }
                        else if (akna[i, j] == "1")
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }
                        else if (akna[i, j] == "2")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                        }
                        else if (akna[i, j] == "3")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else if (akna[i, j] == "4")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                        }
                        else if (akna[i, j] == "5")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                        }
                        else if (akna[i, j] == "6")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                        }
                        else if (akna[i, j] == "7")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        }
                        else if (akna[i, j] == "8")
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                        }
                        Console.Write(akna[i, j]);
                        Console.ResetColor();
                    }
                    else if (visible[i, j] == "false")
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.Write(fedes);
                        Console.ResetColor();
                    }
                    else if (visible[i, j] == "flag")
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.Write(zaszlo);
                        Console.ResetColor();
                    }
                }
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Kurzor vezérlése
        /// </summary>
        /// <param name="akna"></param>
        /// <param name="visible"></param>
        static void Select(string[,] akna, ref string[,] visible)
        {
            var cur = Console.GetCursorPosition();
            int CurTop = Convert.ToInt32(cur.Top);
            int CurLeft = Convert.ToInt32(cur.Left);
            ConsoleKey ck = Console.ReadKey(true).Key;
            if (ck == ConsoleKey.UpArrow) if (cursor_y - 1 >= 0) cursor_y--;
            if (ck == ConsoleKey.DownArrow) if (cursor_y + 1 < meret) cursor_y++;
            if (ck == ConsoleKey.LeftArrow) if (cursor_x - 1 >= 0) cursor_x--;
            if (ck == ConsoleKey.RightArrow) if (cursor_x + 1 < meret) cursor_x++;
            if (ck == flag)
            {
                if (visible[CurTop, CurLeft] == "flag")
                {
                    visible[CurTop, CurLeft] = "false";
                    flagcount--;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write(fedes);
                    Console.ResetColor();
                    Nyeres_Ellenorzes(akna, visible);
                }
                else if (visible[CurTop, CurLeft] == "false")
                {
                    visible[CurTop, CurLeft] = "flag";
                    flagcount++;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write(zaszlo);
                    Console.ResetColor();
                    //Draw(akna, visible, true);
                    Nyeres_Ellenorzes(akna, visible);
                }
            }
            if (ck == dig)
            {
                Felfedes(akna, ref visible, cursor_y, cursor_x);
                Nyeres_Ellenorzes(akna, visible);
                Draw(akna, visible, true);
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
            if (x < 0 || x >= meret || y < 0 || y >= meret) return;
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
            Console.Title = $"Aknakereső - {meret} x {meret} - {aknakszama} aknával - Hátra van még: {aknakszama - flagcount}";
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
        static void Start(string[,] akna, ref string[,] visible)
        {
            Console.Title = $"Aknakereső - {meret} x {meret} - {aknakszama} aknával";
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            for (int i = 0; i < akna.GetLength(1); i++)
            {
                for (int j = 0; j < akna.GetLength(0); j++)
                {
                    Console.Write(fedes);
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
                if (ck == ConsoleKey.DownArrow) if (cursor_y + 1 < meret) cursor_y++;
                if (ck == ConsoleKey.LeftArrow) if (cursor_x - 1 >= 0) cursor_x--;
                if (ck == ConsoleKey.RightArrow) if (cursor_x + 1 < meret) cursor_x++;
                Console.SetCursorPosition(cursor_x, cursor_y);
            } while (ck != dig);
            /*do
            {
                akna_letrehozas(ref akna, ref visible);
                Generate(ref akna);
            } while (akna[cursor_y, cursor_x] == minemark);  //== minemark*/

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
            Draw(akna, visible, true);
        }
        static void Reset()
        {
            gameover = false;
            gameover_type = "false";
            flagcount = 0;
            cursor_x = 0;
            cursor_y = 0;
            Console.Clear();
        }
        static void Menu()
        {
            string[] options = {
                "Könnyű (9x9, 10 akna)",
                "Közepes (16x16, 40 akna)",
                "Nehéz (24x24, 99 akna)",
                "Egyedi pálya"
            };
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
                Title();
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
                    meret = 9;
                    aknakszama = 10;
                    break;
                case 1:
                    meret = 16;
                    aknakszama = 40;
                    break;
                case 2:
                    meret = 24;
                    aknakszama = 99;
                    break;
                case 3:
                    int max;
                    bool converted;
                    do
                    {
                        converted = false;
                        Console.Write("Méret: ");
                        converted = int.TryParse(Console.ReadLine(), out meret);
                        max = Console.WindowHeight;
                        if (Console.WindowWidth < max) max = Console.WindowWidth;
                        if (!converted)
                        {
                            Console.WriteLine("A megadott érték nem szám vagy nem egész szám!");
                        }
                        else if (meret < 2)
                        {
                            Console.WriteLine("A játékterület mérete nem lehet 1 vagy annál kisebb!");
                        }
                        else if (meret > max - 3)
                        {
                            Console.WriteLine("A megadott szám kívül esik az ablak méretén!");
                        }
                    } while (!(converted && meret < max - 2 && meret > 1));

                    max = (meret * meret) - 1;
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
                    break;
            }
            //ide be kell raknom, hogy ellenőrizze hoyg ráfér e a képernyőre
            static void Title()
            {
                Console.WriteLine(
@" __  __ _                                                   
 |  \/  (_)                                                  
 | \  / |_ _ __   ___  _____      _____  ___ _ __   ___ _ __ 
 | |\/| | | '_ \ / _ \/ __\ \ /\ / / _ \/ _ \ '_ \ / _ \ '__|
 | |  | | | | | |  __/\__ \\ V  V /  __/  __/ |_) |  __/ |   
 |_|  |_|_|_| |_|\___||___/ \_/\_/ \___|\___| .__/ \___|_|   
                                            | |              
                                            |_|              ");
            }
        }
    }
}
