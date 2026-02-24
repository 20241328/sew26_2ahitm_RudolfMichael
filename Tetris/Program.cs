using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Input;
using Tetris;

namespace Tetris
{


    internal class Program
    {
        private static List<ITransform> objects = new List<ITransform>();
        public static bool slowDownDrawing = false;
        public static ConsoleColor backgroundColor = ConsoleColor.Black;
        public static Byte GRID_END = 24;//(Input.Program.MAX_Y - 10) / Block.tileHeight;
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

        private static string[] manual = new string[]
        {
            "Use the arrow keys to move and rotate the blocks.",
            "For every block that lands, your score is increased.",
            "The goal is to fill the rows leaving no gaps, as that makes them disappear.",
            "Eventually, the blocks will reach the top, ending the game.",
            "Escape returns to the menu.",
            
        };
        private static string[] options = new string[] { "Start Game", "Leaderboard", "Settings", "Manual", "Quit",  };

        public static byte START_X = 5;
        public static byte START_Y = 5;
        public static byte NEXT_POS_X = 20;
        public static byte NEXT_POS_Y = 5;
        public static bool SKIP_TITLE = true;
        public static float block_speed = 1f;
        public static string PLAYER_NAME = "Player 1";

        /// <summary>
        /// The grid that is "set in stone", meaning it doesn't include 
        /// blocks whilst they are still moving.
        /// 
        /// The grid's outer sides are filled with "true"s.
        /// </summary>
        public static bool[,] staticGrid;

        

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
                }
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;


            Console.Clear();

            Input.Program.Start(args);
            Console.Clear();

            if (!SKIP_TITLE)
            {
                ShowTitle();
            }

            Input.Program.LoadGameData();

            RequestPlayerName();


            
            int selectedOption = 0;
           
            

            while (!Input.Program.currentInputs.Contains(Input.Program.Input.Exit))
            {
                Input.Program.MenuAusgabe(ref selectedOption, options);

                switch (selectedOption)
                {
                    case 0:
                        SetUpGrid();
                        ShowGame();
                        break;
                    case 1:
                        ShowLeaderboard();
                        break;
                    case 3:
                        ShowManual();
                        break;
                    case 4:
                        Input.Program.StoreGameData();
                        return;
                    default: break;
                }
            }
        }

        static void ShowManual()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;

            for (int i = 0; i < manual.Length; i++)
            {
                Console.SetCursorPosition(10, 10+i);
                Console.Write(manual[i]);
            }


            WaitForExit();

            Console.Clear();
        }

        static void WaitForExit()
        {
            while (!Input.Program.ProcessKey(Input.Program.Input.Exit))
            {
                Thread.Sleep(16);
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

                            case TickFinishedAction.Died:
                                Console.Clear();

                                // Store the score if necessary
                                Input.Program.gameData.RegisterScore(PLAYER_NAME, score);

                                // Reset the game state
                                score = 0;
                                block_speed = 1f;

                                return;
                            default: break;
                        }
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.RemoveLastRow))
                    {
                        DeleteRow(ref objects, GRID_END - 1);
                    }


                    Thread.Sleep(10);
                }

                
                Thread.Sleep(1000);
                objects = new List<ITransform>();
                Console.Clear();
            }

            Input.Program.currentInputs.Clear();

            Console.Clear();
        }


        static void RequestPlayerName()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(20, 10);
            Console.Write("Player name: ___________");
            Console.SetCursorPosition(33, 10);

            Console.CursorVisible = true;
            PLAYER_NAME = Console.ReadLine();
            Console.CursorVisible = false;

            // Check name 
            Regex regex = new Regex(@"[a-zA-Z][a-zA-Z0-9]*");

            if (!regex.IsMatch(PLAYER_NAME))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(20, 11);
                Console.WriteLine("Expression must match this regex pattern: '[a-zA-Z][a-zA-Z0-9]*'.");
                Console.ForegroundColor = ConsoleColor.Red;
                RequestPlayerName();
            }

            Console.Clear();
        }


        static void ShowLeaderboard()
        {
            List<Score> bestScores = new List<Score>();

            foreach (Score pScore in Input.Program.gameData.highscores)
            {
                bool foundPair = false;

                for (int i = 0; i < bestScores.Count; i++)
                {
                    if (bestScores[i].value < pScore.value)
                    {
                        foundPair = true;
                        bestScores.Insert(i, pScore);
                        break;
                    }
                }

                if (!foundPair)
                {
                    bestScores.Add(pScore);
                }
            }

            int page = 0;

            DrawLeaderboard(page*10, 10, bestScores);

            while (!Input.Program.ProcessKey(Input.Program.Input.Exit))
            {
                if (Input.Program.ProcessKey(Input.Program.Input.Left))
                {
                    if (page > 0) DrawLeaderboard(--page*10, 10, bestScores);
                }

                if (Input.Program.ProcessKey(Input.Program.Input.Right))
                {
                    if (page < bestScores.Count / 10) DrawLeaderboard(++page*10, 10, bestScores);
                }

                Thread.Sleep(16);
            }

            Console.Clear();
        }

        static void DrawLeaderboard(int startIndex, int length, List<Score> scoresSorted)
        {
            Console.Clear();

            int rowLength = 3 + 12 + 5 + 2 * 4;
            string emptyRowString = "";

            int center = 10 + rowLength / 2;

            var lastIndex = startIndex + length;

            if (lastIndex > scoresSorted.Count) lastIndex = scoresSorted.Count;

            string title = $"Leaderboard ({startIndex+1}...{lastIndex} from {scoresSorted.Count})";

            Console.SetCursorPosition(center - title.Length / 2, 5);    // Center the text above the board
            Console.Write(title);



            for (int i = 0; i < rowLength; i++)
            {
                emptyRowString += ' ';
            }

            for (int i = startIndex; i < startIndex + length && i < scoresSorted.Count; i++)
            {
                Score item = scoresSorted[i];

                switch (i)
                {
                    case 0:
                        Console.BackgroundColor = ConsoleColor.DarkYellow;  // Gold?
                        goto BLACK;
                    case 1:
                        Console.BackgroundColor = ConsoleColor.Gray;        // Silver?
                        goto BLACK;
                    case 2:
                        Console.BackgroundColor = ConsoleColor.DarkGray;    // K.A.?
                        goto BLACK;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = Program.backgroundColor;
                        break;
                    BLACK:
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }

                if (item.playerName == PLAYER_NAME)
                {
                    // Highlight the current user
                    Console.BackgroundColor = ConsoleColor.Red;
                }

                var posY = 10 + i - startIndex;
                Console.SetCursorPosition(10, posY);
                Console.Write(emptyRowString);      // For the background
                Console.SetCursorPosition(10, posY);
                Console.Write($"{i+1,3}.\t{item.playerName,12}\t{item.value,5}");
            }

            Console.SetCursorPosition(10, 12+length-startIndex);
            Console.BackgroundColor = startIndex == 0 ? Program.backgroundColor : ConsoleColor.White;
            Console.ForegroundColor = startIndex == 0 ? ConsoleColor.White : Program.backgroundColor;
            Console.Write("Previous");
            Console.SetCursorPosition(10+rowLength-4, 12 + length - startIndex);
            var listEnded = startIndex + length > scoresSorted.Count;
            Console.BackgroundColor = listEnded ? Program.backgroundColor : ConsoleColor.White;
            Console.ForegroundColor = listEnded ? ConsoleColor.White : Program.backgroundColor;
            Console.Write("Next");

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = Program.backgroundColor;
        }
        

        static TickFinishedAction[] DrawFrame(ref DateTime startTime, ref bool[,] grid)
        {
            TickFinishedAction[] requiredActions = new TickFinishedAction[0] {};

            float deltaTime = (float)(DateTime.Now - startTime).TotalSeconds;
            startTime = DateTime.Now;

            Console.SetCursorPosition(0, 1);
            Console.Write($"FPS: {(int)(1/deltaTime)}; Speed: {block_speed}, Score: {score}");

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
        Died,
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
                PlaceInGrid(ref Program.staticGrid);
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

            PlaceInGrid(ref Program.staticGrid);
        }

        /// <summary>
        /// Draws the block in its current position.
        /// </summary>
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
                            if (Program.slowDownDrawing)
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

            Console.BackgroundColor = Program.backgroundColor;
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

        /// <summary>
        /// Erases the previous position and reraws the block in the new position.
        /// </summary>
        /// <param name="originalSpace"></param>
        public void Redraw(SpaceInfo originalSpace)
        {
            var newPos = this.position;
            var colour = this.color;
            this.color = Program.backgroundColor;
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

        /// <summary>
        /// Updates the given grid to match the blocks current form.
        /// This also returns whether the block overlaps with any other blocks, in which case true is returned, false will be returned otherwise.
        /// If true is returned, the block might not be fully placed in the grid.
        /// </summary>
        /// <param name="currentGrid"></param>
        /// <returns></returns>
        private bool PlaceInGrid(ref bool[,] currentGrid)
        {
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                Byte rowContents = currentShape[row];
                for (int x = 0; x < width; x++)
                {
                    bool filled = (rowContents >> (width - x) & 0x1) == 1;

                    if (!filled) continue;

                    bool filledBefore = currentGrid[row + position.posY, x + position.posX];

                    if (filledBefore)
                    {
                        
                        return true;
                    }
                    
                    currentGrid[row + position.posY, x + position.posX] = true;

                }
            }

            return false;
        }


        private bool InterferesWithGrid(bool[,] previousGrid)
        { 
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                Byte rowContents = currentShape[row];
                for (int x = 0; x < width; x++)
                {
                    bool filled = (rowContents >> (width - x) & 0x1) == 1;

                    if (!filled) continue;

                    if (previousGrid.GetLength(0) <= row + position.posY || row + position.posY < 0)
                    {
                        return true;
                    }

                    if (previousGrid.GetLength(1) <= x + position.posX || x + position.posX < 0)
                    {
                        return true;
                    }

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




            if (this.time > Program.block_speed | Input.Program.ProcessKey(Input.Program.Input.Down)) {
                this.time = 0;
                this.position.posY++;

                int lowerEdgePos = this.position.posY + this.GetHeightBlocks();
                int lowerEdgePosPhysical = lowerEdgePos * Block.tileHeight;
                if (InterferesWithGrid(grid)/*lowerEdgePosPhysical>=Programm.GRID_END*/)
                {
                    Tetris.Program.score++;
                    Tetris.Program.block_speed *= 0.99f;
                    this.placed = true;
                    this.prototype = null;
                    this.position.posY--;
                    if (this.PlaceInGrid(ref grid))
                    {
                        Array.Resize(ref requiredActions, requiredActions.Length + 1);
                        requiredActions[requiredActions.Length - 1] = TickFinishedAction.Died;
                    }
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

namespace Input
{
    internal class Program
    {
        static public List<Input> currentInputs = new List<Input> { };  // globale Variable
        static public bool running = true;


        public const int MAX_X = 150;
        public const int MAX_Y = 55;

        public static GameData gameData;

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

        public static void LoadGameData()
        {
            if (File.Exists("game_data.json"))
            {
                string jsonString = File.ReadAllText("game_data.json");
                gameData = JsonSerializer.Deserialize<GameData>(jsonString);
            } else
            {
                gameData = GameData.GetNew();
            }
        }

        public static void StoreGameData()
        {
            string jsonString = JsonSerializer.Serialize(gameData);
            Console.WriteLine("Storing: " + jsonString);
            Console.WriteLine("Meaning #scores: " + gameData.highscores.Count);

            File.WriteAllText("game_data.json", jsonString);
        }

        public enum Input
        {
             Left,
             Right, 
             RotateUp,
             Down,
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
                    return Input.Down;
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
            // parallele Methode zum Einlesen des Tastendrucks
            var myThread = new Thread(Eingabe);
            myThread.Start();


            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorVisible = false;

            Console.SetWindowSize(MAX_X, MAX_Y);    // Konsolen Größe 
            Console.Title = "Tetris";

            Console.Clear();


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
                Console.ForegroundColor = ConsoleColor.White;
                if (ProcessKey(Input.RotateUp))
                {
                    Console.SetCursorPosition(0, MAX_Y - 1);
                    Console.Write("-----------------");
                    if (selectedEntry > 0)
                    {
                        selectedEntry--;
                    }
                }

                if (ProcessKey(Input.Down))
                {
                    Console.SetCursorPosition(0, MAX_Y - 1);
                    Console.Write("-----------------");
                    if (selectedEntry < options.Length - 1)
                    {
                        selectedEntry++;
                        Console.Clear();
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

                Thread.Sleep(16);
            }

            Program.running = false;


            System.Environment.Exit(0);

        }
    }

    public class GameData
    {
        [JsonPropertyName("highscores")]
        public List<Score> highscores { get; set; }


        /// <summary>
        /// Sets the new highscore for a player if it is that player's best.
        /// </summary>
        /// <param name="player">The player to asign the score to</param>
        /// <param name="score">The score that player reached</param>
        public void RegisterScore(string player, int score)
        {
            if (highscores == null)
            {
                highscores = new List<Score>();
            }

            for (int i = 0; i < highscores.Count; i++)
            {
                Score playerScore = highscores[i];

                if (playerScore.playerName != player) continue;
                if (playerScore.value > score) return;

                highscores[i].value = score;
                return;
            }

            highscores.Add(new Score(player, score));
        }

        public static GameData GetNew()
        {
            GameData data = new GameData();
            data.highscores = new List<Score>();

            return data;
        }
}

    public class Score
    {
        [JsonPropertyName("player_name")]
        public string playerName { get; set; }
        [JsonPropertyName("value")]
        public int value { get; set; }


        public Score(string playerName, int value)
        {
            this.playerName = playerName;
            this.value = value;
        }
    }
}
