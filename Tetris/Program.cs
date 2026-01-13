using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tetris
{


    internal class Programm
    {
        public static ConsoleColor backgroundColor = ConsoleColor.Black;
        public static BlockPrototype[] blockPrototypes = { 
                new BlockPrototype(
                        ConsoleColor.Blue,
                        new bool[,]
                        {
                            { true },
                            { true },
                            { true },
                            { true }
                        },
                        "I"
                    )
            };

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Helper...");
            Thread.Sleep(1000);


            Console.Clear();


            BlockPrototype prototype = blockPrototypes[0];
            Block myBlock = new Block(prototype, 20, 2);


            

            myBlock.Draw();

            Thread.Sleep(500);
           
            Menu.Program.Start(args);
            
        }
    }

    public class BlockPrototype
    {
        public ConsoleColor color;
        public bool[,] shape;
        string name;

        public BlockPrototype(ConsoleColor color, bool[,] shape, string name)
        {
            this.color = color;
            this.shape = shape;
            this.name = name;
        }
    }

    public struct Block: Transform
    {
        static Byte tileWidth = 5;
        static Byte tileHeight = 2;


        BlockPrototype prototype;
        SpaceInfo position;
        bool[,] currentShape; 

        public void Draw()
        {
            Console.SetCursorPosition(position.posX, position.posY);
            Console.BackgroundColor = prototype.color;

            string drawText = "";

            for (int i = 0; i < tileWidth; i++)
            {
                drawText += " ";
            }

            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                for (int j = 0; j < tileHeight; j++)
                {
                    Console.SetCursorPosition(position.posX, position.posX + (row * tileHeight) + j);
                    for (int x = 0; x < currentShape.GetLength(1); x++)
                    {
                        bool filled = this.currentShape[row, x];

                        if (!filled)
                        {
                            Console.SetCursorPosition(position.posY + ((x + 1) * tileWidth), position.posX + (row * tileHeight) + j);
                            continue;
                        }

                        Console.Write(drawText);
                    }
                    Console.WriteLine();
                }
            }

            Console.BackgroundColor = Programm.backgroundColor;
        }

        public SpaceInfo getSpaceInfo()
        {
            return this.position;
        }

        public void Redraw(SpaceInfo originalSpace)
        {
            throw new NotImplementedException();
        }

        public void Tick(float deltaTime)
        {
            
        }

        public bool CanEverTick()
        {
            return true;
        }

        public Block(BlockPrototype prototype, Byte x, Byte y)
        {
            this.prototype = prototype;
            this.position = new SpaceInfo(x, y);
            this.currentShape = prototype.shape;
        }
    }

    public struct SpaceInfo
    {
        public Byte rotation;
        public Byte posX;
        public Byte posY;


        public SpaceInfo(Byte posX, Byte posY)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = 0;
        }

        public SpaceInfo(Byte posX, Byte posY, Byte rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }
    }


    public interface Transform
    {
        void Draw();
        void Redraw(SpaceInfo originalSpace);

        SpaceInfo getSpaceInfo();

        void Tick(float deltaTime);

        bool CanEverTick();
        
    }
}

namespace Menu
{
    internal class Program
    {
        static char zeichen = '0';  // globale Variable
        static bool running = true;
        static void Eingabe()
        {
            ConsoleKeyInfo ein;
            ConsoleKey key;

            while (zeichen != 'x' && running)
            {
                ein = Console.ReadKey(true);
                key = ein.Key;
                zeichen = ein.KeyChar;
                switch (key)
                {
                    case ConsoleKey.DownArrow:
                        zeichen = 's';
                        continue;
                    case ConsoleKey.UpArrow:
                        zeichen = 'w';
                        continue;
                    case ConsoleKey.LeftArrow:
                        zeichen = 'a';
                        continue;
                    case ConsoleKey.RightArrow:
                        zeichen = 'd';
                        continue;
                    case ConsoleKey.Enter:
                        zeichen = 'l';
                        continue;
                }
            }
        }
        public static void Start(string[] args)
        {
            int x = 20; // lokale Variable
            int y = 5;

            const int MAX_X = 40;
            const int MAX_Y = 10;

            // parallele Methode zum Einlesen des Tastendrucks
            var myThread = new System.Threading.Thread(Eingabe);
            myThread.Start();

            Console.SetWindowSize(MAX_X, MAX_Y);    // Konsolen Größe 
            Console.Title = "Tetris";

            string[] options = new string[] { "Spielen", "Einstellungen", "Bestenliste", "Beenden" };
            int selectedOption = 0;
            MenuAusgabe(ref selectedOption, options);

            Console.Clear();

            Console.WriteLine(options[selectedOption]);


            Console.ForegroundColor = ConsoleColor.Blue;

            Thread.Sleep(1000);
            Console.Clear();
            Console.ReadLine();
        }


        private static void MenuAusgabe(ref int selectedEntry, string[] options)
        {
            Console.SetCursorPosition(10, 2);
            Console.WriteLine("Menü (Abbruch mit 'e')");
            while (zeichen != 'e')
            {
                if (zeichen == 'ü') continue;
                switch (zeichen)
                {
                    case 'w':
                        if (selectedEntry > 0)
                        {
                            selectedEntry--;
                        }
                        break;
                    case 's':
                        if (selectedEntry < options.Length - 1)
                        {
                            selectedEntry++;
                        }
                        break;
                    case 'l':
                        return;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (i == selectedEntry)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(10, 4 + i);
                    Console.WriteLine(options[i]);
                }
                zeichen = 'ü';
            }
        }
    }
}
