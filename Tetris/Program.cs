using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tetris;

namespace Tetris
{


    internal class Programm
    {
        private static List<ITransform> objects = new List<ITransform>();
        public static bool slowDownDrawing = false;
        public static ConsoleColor backgroundColor = ConsoleColor.Black;
        public static Byte GRID_END = (Input.Program.MAX_Y - 10) / Block.tileHeight;
        public static Byte GRID_WIDTH = 10;
        public static int score;
        public static BlockPrototype[] blockPrototypes = {
                new BlockPrototype(
                        ConsoleColor.Blue,
                        new bool[,]
                        {
                            { true, false, false, false },
                            { true, false, false, false },
                            { true, false, false, false },
                            { true, false, false, false }
                        },
                        "I"
                    ),
                new BlockPrototype(
                        ConsoleColor.Magenta,
                        new bool[,]
                        {
                            { true, false, false },
                            { true, true, false },
                            { true, false, false },
                        },
                        ">"
                    ),
                new BlockPrototype(
                        ConsoleColor.Green,
                        new bool[,]
                        {
                            { true, false, false },
                            { true, true, false },
                            { false, true, false },
                        },
                        "Z"
                    ),
                 new BlockPrototype(
                        ConsoleColor.Red,
                        new bool[,]
                        {
                            { false, true, false },
                            { true, true, false },
                            { true, false, false },
                        },
                        "S"
                    ),
                  new BlockPrototype(
                        ConsoleColor.DarkYellow,
                        new bool[,]
                        {
                            { true, true, true, true },
                            { true, false, false, false },
                            { false, false, false, false },
                            { false, false, false, false }
                        },
                        "L"
                    ),
                   new BlockPrototype(
                        ConsoleColor.Yellow,
                        new bool[,]
                        {
                            { true, true },
                            { true, true },
                        },
                        "O"
                    )
            };

        public static byte START_X = 5;
        public static byte START_Y = 5;
        public static byte NEXT_POS_X = 20;
        public static byte NEXT_POS_Y = 5;
        public static bool SKIP_TITLE = true;
        public static float block_speed = 1f;
        /// <summary>
        /// The grid that is "set in stone", meaning it doesn't include 
        /// blocks whilst they are still moving.
        /// 
        /// The grid's outer sides are filled with "true"s.
        /// </summary>
        public static bool[,] staticGrid;

        public static byte[] RemoveAt(byte[] source, int index)
        {
            byte[] dest = new byte[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }


        /// <summary>
        /// Builds the grid's outer sides as described.
        /// </summary>
        private static void SetUpGrid()
        {
            staticGrid = new bool[GRID_END + 2, GRID_WIDTH + 2];

            for (int i = 0; i < staticGrid.GetLength(0); i++)
            {
                staticGrid[i, 0] = true;
                staticGrid[i, GRID_WIDTH + 1] = true;

                if (i == 0 || i >= GRID_END)
                {
                    for (int j = 1; j < staticGrid.GetLength(1) - 1; j++)
                    {
                        staticGrid[i, j] = true;
                        
                    }
                    Console.WriteLine("Zeile vertruet");
                }
            }
        }

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
            Console.OutputEncoding = Encoding.UTF8;
            if (!SKIP_TITLE)
            {
                Console.WriteLine("Starting Helper...");
                Thread.Sleep(1000);
            }

            SetUpGrid();


            Console.Clear();

            Input.Program.Start(args);
            Console.Clear();

            if (!SKIP_TITLE)
            {
                ShowTitle();
            }


            

            string[] options = new string[] { "Start Game", "Leaderboard", "Settings", "Quit", "Manual"};
            int selectedOption = 0;
           
            

            while (!Input.Program.currentInputs.Contains(Input.Program.Input.Exit))
            {
                Input.Program.MenuAusgabe(ref selectedOption, options);

                switch (selectedOption)
                {
                    case 0:
                        ShowGame();
                        break;
                    default: break;
                }
            }
        }

        public static void ShowTitle()
        {
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
        }

        static void DeleteRow(ref List<ITransform> blocks, int index)
        {
            SetUpGrid();
            Console.Clear();
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].RemoveRow(index);
            }
        }

        static void ShowGame()
        {
            score = 0;
            Console.Clear();

            Block[] blocks = new Block[0];
            List<BlockPrototype> blockPool = new List<BlockPrototype>();
            Random rng = new Random();

            (Block nextBlock, BlockPrototype currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);

            ITransform[] existingTransforms = new ITransform[0];

            DateTime time = DateTime.Now;

            while (!Input.Program.currentInputs.Contains(Input.Program.Input.Exit))
            {
                objects.Add(nextBlock);
                (nextBlock, currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);

             for (int i = 0; i < 10000; i++)
                {
                    foreach (TickFinishedAction action in DrawFrame(ref time, ref staticGrid))
                    {
                        switch (action)
                        {
                            case TickFinishedAction.CreateNewBlock:
                                objects.Add(nextBlock);
                                (nextBlock, currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);

                                // A block has been placed, so it needs to be checked if any rows are full
                                for (int row = 0; row < staticGrid.GetLength(0)-2; row++)
                                {
                                    int fulls = 0;
                                    for (int col = 0; col < staticGrid.GetLength(1); col++)
                                    {
                                        if (staticGrid[row, col])
                                        {
                                            fulls++;
                                        }
                                    }


                                    bool fullRow = fulls == staticGrid.GetLength(1);
                                    

                                    if (fullRow)
                                    {
                                        DeleteRow(ref objects, row);
                                    }
                                }

                                break;
                            default: break;
                        }
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.RemoveLastRow))
                    {
                        DeleteRow(ref objects, GRID_END - 2);
                    }

                    // (int)(ballX)


                    Thread.Sleep(10);
                }

                
                Thread.Sleep(1000);
                objects = new List<ITransform>();
                Console.Clear();
            }

            Input.Program.currentInputs.Clear();

            Console.Clear();
        }

        static TickFinishedAction[] DrawFrame(ref DateTime startTime, ref bool[,] grid)
        {
            TickFinishedAction[] requiredActions = new TickFinishedAction[0] {};

            float deltaTime = (float)(DateTime.Now - startTime).TotalSeconds;
            startTime = DateTime.Now;

            Console.SetCursorPosition(0, 1);
            Console.Write($"FPS: {(int)(1/deltaTime)}; Speed: {block_speed}");

            if (objects != null)
            {

                foreach (var item in objects)
                {
                    if (item.CanEverTick())
                    {
                        item.Tick(deltaTime, ref requiredActions, ref grid);
                    }

                    item.Draw();
                }
            }

            Console.SetCursorPosition(5, 2);
            Console.Write($"Score: {score}");



            return requiredActions;

        }

        static (Block, BlockPrototype) GenerateBlock(ref List<BlockPrototype> pool, BlockPrototype[] allBlocks, ref Random rng)
        {
            if (pool.Count == 0)
            {
                pool = allBlocks.ToList();
            }

            
            int index = rng.Next(0, pool.Count);

            BlockPrototype prototype = pool[index];
            pool.RemoveAt(index);

            var block = new Block(prototype, START_X, START_Y);

            return (block, prototype);
        }
    }

    public enum TickFinishedAction
    {
        CreateNewBlock,
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
        private float time;
        public ConsoleColor color;

        /// <summary>
        /// If the block is "placed", it can no longer move
        /// </summary>
        public bool placed;


        /// <summary>
        /// Removes the row from the block if it exists.
        /// </summary>
        /// <param name="row"></param>
        public void RemoveRow(int row)
        {
            if (!this.placed) return;

            int posInShape = row - this.position.posY;
            var originalPosition = this.position;

            if (posInShape < 0)
            {
                PlaceInGrid(ref Programm.staticGrid);
                return;
            }


            this.position.posY++;

            if (posInShape >= 0 && posInShape < currentShape.Length)
            {
                currentShape[posInShape] = 0x0;
                List<byte> shapeCopy = currentShape.ToList();
                shapeCopy.RemoveAt(posInShape);
                currentShape = shapeCopy.ToArray();
                Redraw(originalPosition);
            }

            PlaceInGrid(ref Programm.staticGrid);
        }

        public void Draw()
        {
            Console.SetCursorPosition(position.posX, position.posY);
            Console.BackgroundColor = this.color;

            string drawText = "";

            for (int i = 0; i < tileWidth; i++)
            {
                drawText += " ";
            }

            for (int row = 0; row < currentShape.Length; row++)
            {
                byte rowContents = currentShape[row];
                for (int j = 0; j < tileHeight; j++)
                {
                    
                    Console.SetCursorPosition(position.posX * Block.tileWidth, (position.posY * Block.tileHeight) + (row * tileHeight) + j);
                    for (int x = 0; x < width; x++)
                    {
                        bool filled = (rowContents >> (width - x) & 0x1) == 1;

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


        private int GetHeightBlocks()
        {
            int count = 0;

            foreach (byte contents in currentShape)
            {
                if (contents == 0)
                {
                    return count;
                }

                count++;
            }

            return count;
        }

        public SpaceInfo getSpaceInfo()
        {
            return this.position;
        }

        public void Redraw(SpaceInfo originalSpace)
        {
            var newPos = this.position;
            var colour = this.color;
            this.color = Programm.backgroundColor;
            this.position = originalSpace;

            if (this.position.rotation != newPos.rotation)
            {
                this.position.rotation -= 1;
                Rotate(this.prototype);
            }

            // Delete the original position
            this.Draw();


            // Redraw in new position
            this.position = newPos;
            this.color = colour;

            if (this.position.rotation != originalSpace.rotation)
            {
                this.position.rotation -= 1;
                Rotate(this.prototype);
            }
            this.Draw();
        }

        private void PlaceInGrid(ref bool[,] currentGrid)
        {
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                Byte rowContents = currentShape[row];
                for (int x = 0; x < width; x++)
                {
                    bool filled = (rowContents >> (width - x) & 0x1) == 1;

                    if (filled)
                    {
                        currentGrid[row + position.posY, x + position.posX] = true;
                    }

                }
            }
        }


        private bool InterferesWithGrid(bool[,] previousGrid)
        { 
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                Byte rowContents = currentShape[row];
                for (int x = 0; x < width; x++)
                {
                    bool filled = (rowContents >> (width - x) & 0x1) == 1;
                    if (filled && previousGrid[row + position.posY, x + position.posX])
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        public void Tick(float deltaTime, ref TickFinishedAction[] requiredActions, ref bool[,] grid)
        {
            if (this.placed) {
                return;
            }
            this.time += deltaTime;
            var position = this.position;
            bool needsRedrawing = false;

            if (Input.Program.ProcessKey(Input.Program.Input.RotateUp))
            {
                
                Rotate(this.prototype);

                if (InterferesWithGrid(grid))
                {
                    Rotate(this.prototype);
                    Rotate(this.prototype);
                    Rotate(this.prototype);
                }
                else
                {
                    needsRedrawing = true;
                }
            }

            if (Input.Program.ProcessKey(Input.Program.Input.Left))
            {
                this.position.posX--;

                if (InterferesWithGrid(grid))
                {
                    this.position.posX++;
                }
                else
                {
                    needsRedrawing = true;
                }
            }

            if (Input.Program.ProcessKey(Input.Program.Input.Right))
            {
                this.position.posX++;

                if (InterferesWithGrid(grid))
                {
                    this.position.posX--;
                } else
                {
                    needsRedrawing = true;
                }
            }




            if (this.time > Programm.block_speed) {
                this.time = 0;
                this.position.posY++;

                int lowerEdgePos = this.position.posY + this.GetHeightBlocks();
                int lowerEdgePosPhysical = lowerEdgePos * Block.tileHeight;
                if (InterferesWithGrid(grid)/*lowerEdgePosPhysical>=Programm.GRID_END*/)
                {
                    Tetris.Programm.score++;
                    Tetris.Programm.block_speed *= 0.98f;
                    this.placed = true;
                    this.prototype = null;
                    this.position.posY--;
                    this.PlaceInGrid(ref grid);
                    Array.Resize(ref requiredActions, requiredActions.Length + 1);
                    requiredActions[requiredActions.Length - 1] = TickFinishedAction.CreateNewBlock;
                }

                needsRedrawing = true;
            }

            if (needsRedrawing)
            {
                Redraw(position);
            }
        }

        public void Rotate(BlockPrototype prototype)
        {
            this.position.rotation = (byte)((this.position.rotation + 1) % 4);

            switch (this.position.rotation)
            {
                case 0:
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
                    break;

                case 1:
                    this.width = (byte)prototype.shape.GetLength(1);
                    var height = this.currentShape.GetLength(0);
                    var width = this.width;
                    this.currentShape = new Byte[width];

                    

                    for (int i = 0; i < width; i++)
                    {
                        Byte contents = 0;

                        for (int j = 0; j < height; j++)
                        {
                            bool filled = prototype.shape[j, i];
                            if (filled)
                            {
                                contents |= 1;
                            }
                            contents <<= 1;
                        }


                        this.currentShape[i] = contents;
                    }
                    break;

                case 2:
                    this.currentShape = new Byte[prototype.shape.GetLength(0)];
                    var height_ = this.currentShape.GetLength(0);
                    for (int i = 0; i < height_; i++)
                    {
                        Byte contents = 0;

                        for (int j = this.width - 1; j >= 0; j--)
                        {
                            bool filled = prototype.shape[i, j];
                            if (filled)
                            {
                                contents |= 1;
                            }
                            contents <<= 1;
                        }

                        this.currentShape[height_ - i - 1] = contents;
                    }
                    break;
                case 3:
                    this.width = (byte)prototype.shape.GetLength(1);
                    var height__ = this.currentShape.GetLength(0);
                    var width_ = this.width;
                    this.currentShape = new Byte[width_];



                    for (int i = 0; i < width_; i++)
                    {
                        Byte contents = 0;

                        for (int j = 0; j < height__; j++)
                        {
                            bool filled = prototype.shape[j, i];
                            if (filled)
                            {
                                contents |= 1;
                            }
                            contents <<= 1;
                        }


                        this.currentShape[width_-i-1] = contents;
                    }
                    break;

                default: throw new Exception();
            }
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
            this.placed = false;
            this.time = 0;
            this.color = prototype.color;

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

        void Tick(float deltaTime, ref TickFinishedAction[] requiredActions, ref bool[,] grid);

        bool CanEverTick();

        void RemoveRow(int index);
    }
}

namespace GameLogic
{
    internal class GameThread
    {
        private static List<ITransform> tickingObjects;
        public static void Start()
        {
            tickingObjects = new List<ITransform>();
            while (Input.Program.running)
            {
                Tick();
            }
        }

        private static void Tick()
        {
            if (tickingObjects.Count != 0)
            {
                foreach (ITransform gObject in tickingObjects.ToArray())
                {
                    if (gObject == null) { continue; }
                   // gObject.Tick(0, ref );
                }
            } else
            {

            }
        }

        public static void AddGameObject(ITransform gObject)
        {
            tickingObjects.Add(gObject);

            Console.WriteLine($"Ticking objects: {tickingObjects}");
        }

        public static ITransform[] GetObjects()
        {
            return tickingObjects.ToArray();
        }
    }
}

namespace Input
{
    internal class Program
    {
        static public List<Input> currentInputs = new List<Input> { };  // globale Variable
        static public bool running = true;


        public const int MAX_X = 150;
        public const int MAX_Y = 100;
        static void Eingabe()
        {
            ConsoleKeyInfo ein;
            ConsoleKey key;

            while (running)
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
             RotateUp,
             RotateDown,
             Select,
             Exit,
             RemoveLastRow,
        }

        private static Input? KeyToInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    return Input.RotateUp;
                case ConsoleKey.DownArrow:
                    return Input.RotateDown;
                case ConsoleKey.LeftArrow:
                    return Input.Left;
                case ConsoleKey.RightArrow:
                    return Input.Right;
                case ConsoleKey.Enter:
                    return Input.Select;
                case ConsoleKey.Escape:
                    return Input.Exit;
                case ConsoleKey.L:
                    return Input.RemoveLastRow;
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
            var gameLogicThread = new Thread(GameLogic.GameThread.Start);
            myThread.Start();
            gameLogicThread.Start();


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

            Console.SetCursorPosition(0, MAX_Y - 1);
            Console.Write("-----------------");

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
                if (ProcessKey(Input.RotateUp))
                {
                    Console.SetCursorPosition(0, MAX_Y - 1);
                    Console.Write("-----------------");
                    if (selectedEntry > 0)
                    {
                        selectedEntry--;
                    }
                }

                if (ProcessKey(Input.RotateDown))
                {
                    Console.SetCursorPosition(0, MAX_Y - 1);
                    Console.Write("-----------------");
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

            Program.running = false;


            System.Environment.Exit(0);

        }
    }
}
