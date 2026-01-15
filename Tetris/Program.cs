using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Tetris
{


    internal class Programm
    {
        public static bool slowDownDrawing = false;
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


        public static BlockPrototype[] letters = {
                new BlockPrototype(
                        ConsoleColor.Blue,
                        new bool[,]
                        {
                            { true, true, true, true, true },
                            { false, false, true, false, false },
                            { false, false, true, false, false },
                            { false, false, true, false, false },
                            { false,false, true, false, false },
                        },
                        "T0"
                    ),
                new BlockPrototype(
                        ConsoleColor.Red,
                        new bool[,]
                        {
                            { true, true, true, true, true },
                            { true, false, false, false, false },
                            { true, true, true, true, true },
                            { true, false, false, false, false },
                            { true, true, true, true, true },
                        },
                        "E"
                    ),

                new BlockPrototype(
                        ConsoleColor.Yellow,
                        new bool[,]
                        {
                            { true, true, true, true, true },
                            { false, false, true, false, false },
                            { false, false, true, false, false },
                            { false, false, true, false, false },
                            { false,false, true, false, false },
                        },
                        "T1"
                    ),

                new BlockPrototype(
                        ConsoleColor.Green,
                        new bool[,]
                        {
                            { true, true, true, true, },
                            { true, false, false, true },
                            { true, true, true, true },
                            { true, false, true, false, },
                            { true,false, false, true,  },
                        },
                        "R"
                    ),
                 new BlockPrototype(
                        ConsoleColor.Cyan,
                        new bool[,]
                        {
                            { true,},
                            { true, },
                            { true, },
                            { true, },
                            { true,  },
                        },
                        "I"
                    ),
                  new BlockPrototype(
                        ConsoleColor.Magenta,
                        new bool[,]
                        {
                            { true, true, true, true, },
                            { true, false, false, false },
                            { true, true, true, true, },
                            { false, false, false, true, },
                            { true, true, true, true, },
                        },
                        "S"
                    ),

        };

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Helper...");
            Thread.Sleep(1000);


            Console.Clear();

            Input.Program.Start(args);
            Console.SetWindowSize(152, 40);
            Console.Clear();

            int charOffset = 1;


            foreach (BlockPrototype prototype in letters)
            {
                int letterOffset = charOffset * Block.tileWidth;
                Block myLetter = new Block(prototype, (byte)(letterOffset), 5);
                myLetter.Draw();
                charOffset += prototype.shape.GetLength(1) + 1;
                Thread.Sleep(150);
            }

            Thread.Sleep(4000);
            Console.Clear();

            string[] options = new string[] { "Start Game", "Leaderboard", "Settings", "Quit", "Manual"};
            int selectedOption = 0;
           
            Input.Program.MenuAusgabe(ref selectedOption, options);
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

    public struct Block: ITransform
    {
        public const Byte tileWidth = 4;
        public const Byte tileHeight = 2;


        public BlockPrototype prototype;
        public SpaceInfo position;
        public Byte[] currentShape;
        public Byte width;

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
                Byte rowContents = currentShape[row];
                for (int j = 0; j < tileHeight; j++)
                {
                    Console.SetCursorPosition(position.posX, position.posY + (row * tileHeight) + j);
                    for (int x = 0; x < width; x++)
                    {
                        bool filled = (rowContents >> (width - x) & 0x1) == 1;
                        //Console.WriteLine("filled: " + filled);

                        if (!filled)
                        {
                            if (Programm.slowDownDrawing)
                            {
                                Thread.Sleep(50);
                            }
                            if (x + 1 < width)
                            {
                                Console.CursorLeft += tileWidth;
                            }
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
            this.width = (byte)prototype.shape.GetLength(1);
            this.currentShape = new Byte[prototype.shape.GetLength(0)];

            for (int i = 0; i < this.currentShape.GetLength(0); i++)
            {
                Byte contents = 0;

                for (int j = 0; j < this.width; j++)
                {
                    bool filled = prototype.shape[i, j];
                    if (filled)
                    {
                        contents |= 1;
                    }
                    contents <<= 1;
                }


                this.currentShape[i] = contents;
            }
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


    public interface ITransform
    {
        void Draw();
        void Redraw(SpaceInfo originalSpace);

        SpaceInfo getSpaceInfo();

        void Tick(float deltaTime);

        bool CanEverTick();
        
    }
}

namespace Input
{
    internal class Program
    {
        static public List<Input> currentInputs = new List<Input> { };  // globale Variable
        static public bool running = true;


        public const int MAX_X = 150;
        public const int MAX_Y = 40;
        static void Eingabe()
        {
            ConsoleKeyInfo ein;
            ConsoleKey key;

            while (!currentInputs.Contains(Input.Exit))
            {
                ein = Console.ReadKey(true);
                key = ein.Key;
                Input? input = KeyToInput(key);

                if (input == null)
                {
                    continue;
                }

                if (!currentInputs.Contains(input.Value))
                {
                    currentInputs.Add(input.Value);
                }
            }
        }

        public enum Input
        {
             Left,
             Right, 
             Rotate,
             Select,
             Exit,
        }

        private static Input? KeyToInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    return Input.Rotate;
                case ConsoleKey.LeftArrow:
                    return Input.Left;
                case ConsoleKey.RightArrow:
                    return Input.Right;
                case ConsoleKey.Enter:
                    return Input.Select;
                case ConsoleKey.Escape:
                    return Input.Exit;
                default:
                    return null;
            }
        }
        



        public static void Start(string[] args)
        {
            int x = 20; // lokale Variable
            int y = 5;

           

            // parallele Methode zum Einlesen des Tastendrucks
            var myThread = new System.Threading.Thread(Eingabe);
            myThread.Start();


            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorVisible = false;

            Console.SetWindowSize(MAX_X, MAX_Y);    // Konsolen Größe 
            Console.Title = "Tetris";

            string[] options = new string[] { "Spielen", "Einstellungen", "Bestenliste", "Beenden" };
            int selectedOption = 0;
            //MenuAusgabe(ref selectedOption, options);

            Console.Clear();

            //Console.WriteLine(options[selectedOption]);


            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.Clear();
        }


        public static bool ProcessKey(Input key)
        {
            int index = currentInputs.IndexOf(key);

            if (index == -1)
            {
                return false;
            }

            currentInputs.RemoveAt(index);

            return true;
        }


        public static void MenuAusgabe(ref int selectedEntry, string[] options)
        {
            Console.SetCursorPosition(10, 2);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Menü (Abbruch mit 'esc')");


            int longestEntry = 0;

            foreach (string entry in options)
            {
                longestEntry = longestEntry > entry.Length ? longestEntry : entry.Length;
            }


            string[] optionsClone = options;

            for (int i = 0; i < optionsClone.Length; i++)
            {
                while (options[i].Length <= longestEntry)
                {
                    options[i] += " ";
                }
            }


            while (!currentInputs.Contains(Input.Exit))
            {
                if (ProcessKey(Input.Left))
                {
                    if (selectedEntry > 0)
                    {
                        selectedEntry--;
                    }
                }

                if (ProcessKey(Input.Right))
                {
                    if (selectedEntry < options.Length - 1)
                    {
                        selectedEntry++;
                    }
                }

                if (ProcessKey(Input.Select))
                {
                    return;
                }

                
                for (int i = 0; i < options.Length; i++)
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
                    Console.WriteLine(" " + options[i]);
                }
            }
        }
    }
}
