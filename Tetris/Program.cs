using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Input;

namespace Tetris
{
    internal class Program
    {
        /// <summary>
        /// The factor with which blocks take shorter to move automatically
        /// after a block has been placed.
        /// </summary>
        public const float BLOCK_SPEED_INCREASE = 0.99f;
        public const ConsoleColor BACKGROUND_COLOR = ConsoleColor.Black;
        public const byte GRID_END = 24;
        public const byte GRID_WIDTH = 10;
        public const byte NEXT_POS_Y = 5;
        public const byte NEXT_POS_X = 20;
        public const byte START_Y = 0;
        public const byte START_X = GRID_WIDTH / 2;
        public const int MAX_PLAYER_NAME_LENGTH = 15;


        public static int score;
        public static string playerName = "";
        private static List<IGameObject> objects = new List<IGameObject>();
        public static float block_speed = 1f;
        public static bool slowDownDrawing = false;
        public static BlockPrototype[] blockPrototypes = {
                new BlockPrototype(
                        ConsoleColor.Blue,
                        new bool[,]
                        {
                            { true, false, false, false },
                            { true, false, false, false },
                            { true, false, false, false },
                            { true, false, false, false }
                        }
                    ),
                new BlockPrototype(
                        ConsoleColor.Magenta,
                        new bool[,]
                        {
                            { true, false, false },
                            { true, true, false },
                            { true, false, false },
                        }
                    ),
                new BlockPrototype(
                        ConsoleColor.Green,
                        new bool[,]
                        {
                            { true, false, false },
                            { true, true, false },
                            { false, true, false },
                        }
                    ),
                 new BlockPrototype(
                        ConsoleColor.Red,
                        new bool[,]
                        {
                            { false, true, false },
                            { true, true, false },
                            { true, false, false },
                        }
                    ),
                  new BlockPrototype(
                        ConsoleColor.DarkYellow,
                        new bool[,]
                        {
                            { true, true, true, true },
                            { true, false, false, false },
                            { false, false, false, false },
                            { false, false, false, false }
                        }
                    ),
                   new BlockPrototype(
                        ConsoleColor.Yellow,
                        new bool[,]
                        {
                            { true, true },
                            { true, true },
                        }
                    )
            };


        private static string[] options = new string[] { "Start Game", "Leaderboard", "Settings", "Manual", "Quit", };



        private static string[] manual = new string[]
        {
            "Use the arrow keys to move and rotate the blocks.",
            "For every block that lands, your score is increased.",
            "The goal is to fill the rows leaving no gaps, as that makes them disappear.",
            "As you progress, the game gets harder because the blocks fall faster.",
            "Eventually, the blocks will reach the top, ending the game.",
            "Escape returns to the menu.",

        };

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
                        }
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
                        }
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
                        }
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
                        }
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
                        }
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
                        }
                    ),

        };
        public static ISetting[] settings =
        {
            new NameSetting(),
            new NamePreservationSetting(),
            new ExtremeModeSetting(),
            new TitleScreenSetting(),
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
            Console.InputEncoding = Encoding.UTF8;


            Console.Clear();

            Input.Program.Start(args);
            Input.Program.LoadGameData();
            Console.Clear();


            if (Input.Program.gameData.showTitleScreen)
            {
                slowDownDrawing = true;
                ShowTitle();
                slowDownDrawing = false;
            }

            

            if (playerName == "") RequestPlayerName();



            int selectedOption = 0;

            while (!Input.Program.currentInputs.Contains(Input.Program.Input.Exit))
            {
                Input.Program.MenuAusgabe(ref selectedOption, options);

                switch (selectedOption)
                {
                    case 0:
                        SetUpGrid();
                        ShowGame();
                        Input.Program.StoreGameData();
                        break;
                    case 1:
                        ShowLeaderboard();
                        break;
                    case 2:
                        ShowSettings();
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
                Console.SetCursorPosition(10, 10 + i);
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
                int letterOffset = charOffset;
                Block myLetter = new Block(prototype, (byte)(letterOffset), 5);
                myLetter.Draw();
                charOffset += prototype.shape.GetLength(1) + 1;
            }

            Thread.Sleep(3000);
            Console.Clear();
        }

        static void DeleteRow(ref List<IGameObject> blocks, int index)
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

            //Block[] blocks = new Block[0];
            List<BlockPrototype> blockPool = new List<BlockPrototype>();
            Random rng = new Random();
            DateTime time = DateTime.Now;

            // Generate the first block and place it at the start position
            (Block nextBlock, BlockPrototype currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);
            nextBlock.notAnObsticle = false;
            nextBlock.position.posX = START_X;
            nextBlock.position.posY = START_Y; objects.Add(nextBlock);


            // Generate the next block and place it where you can see the next block
            (nextBlock, currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);
            if (!Input.Program.gameData.extremeMode)
            {
                nextBlock.notAnObsticle = true;
                nextBlock.placed = true;
            }
            nextBlock.Draw();
            objects.Add(nextBlock);

            while (true)
            {
                foreach (TickFinishedAction action in DrawFrame(ref time, ref staticGrid))
                {
                    switch (action)
                    {
                        case TickFinishedAction.CreateNewBlock:
                            if (objects.Count != 0) {
                                // Remove the latest block, which is only there for displaying 
                                // what will be next to the user
                                objects.RemoveAt(objects.Count - 1);
                            }
                            // Update it so it can be at the center
                            // 1. Redraw it black so it's no longer visible
                            //    on the right side.
                            nextBlock.placed = false;
                            nextBlock.color = Program.BACKGROUND_COLOR;
                            nextBlock.Draw();
                            nextBlock.color = nextBlock.prototype.color;

                            // 2. Update the position
                            nextBlock.position.posX = START_X;
                            nextBlock.position.posY = START_Y;
                            nextBlock.notAnObsticle = false;

                            // 3. Add it back to the game objects for it to be ticked
                            objects.Add(nextBlock);

                            // Generate the next block
                            (nextBlock, currentPrototype) = GenerateBlock(ref blockPool, blockPrototypes, ref rng);
                            nextBlock.placed = true;
                            nextBlock.Draw();
                            objects.Add(nextBlock);

                            // A block has been placed, so it needs to be checked if any rows are full
                            for (int row = 0; row < staticGrid.GetLength(0) - 2; row++)
                            {
                                // This algorithm seems painfuly inefficient, and it is, but
                                // for some reason, nothing else seems to work.
                                // TODO: Clean this mess up
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
                            // Store the score if necessary
                            Input.Program.gameData.RegisterScore(playerName, score);
                            var repeat = ShowDeathScreen();
                            CleanUpGame();

                            if (repeat)
                            {
                                continue;
                            }
                            return;
                        default: break;
                    }
                }


                // This exists for debug purposes only, 
                // but the intent should be clear and this makes for a great
                // way to cheat.
                if (Input.Program.ProcessKey(Input.Program.Input.RemoveLastRow))
                {
                    DeleteRow(ref objects, GRID_END - 1);
                }

                if (Input.Program.ProcessKey(Input.Program.Input.Exit))
                {
                    CleanUpGame();
                    Console.Clear();
                    return;
                }


                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Shows the death screen with the player's score and such.
        /// </summary>
        /// <returns>Whether the game should be instantly restartet (true) or not.</returns>
        static bool ShowDeathScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;

            bool selectedRestart = true;
            while (true)
            {
                Console.SetCursorPosition(22, 7);
                Console.Write("You Died");
                Console.SetCursorPosition(21, 8);
                Console.Write("Score: " + score);


                Console.SetCursorPosition(10, 20);
                Console.BackgroundColor = selectedRestart ? ConsoleColor.Blue : Program.BACKGROUND_COLOR;
                Console.Write("Play Again");
                Console.SetCursorPosition(35, 20);
                Console.BackgroundColor = !selectedRestart ? ConsoleColor.Blue : Program.BACKGROUND_COLOR;
                Console.Write("Menu");
                Console.BackgroundColor = Program.BACKGROUND_COLOR;


                while (true)
                {
                    if (Input.Program.ProcessKey(Input.Program.Input.Left))
                    {
                        selectedRestart = true;
                        break;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.Right))
                    {
                        selectedRestart = false; break;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.Select))
                    {
                        return selectedRestart;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.Exit))
                    {
                        return true;
                    }

                    Thread.Sleep(16);
                }
            }
        }


        /// <summary>
        /// Removes static rests of the game.
        /// This does not register the score.
        /// </summary>
        static void CleanUpGame()
        {
            // Reset the game state
            score = 0;
            block_speed = 1f;
            objects = new List<IGameObject>();
            Input.Program.currentInputs.Clear();
            Console.Clear();
        }

        /// <summary>
        /// Draws a single frame and gives back actions that the method
        /// can't handle by itself.
        /// </summary>
        /// <param name="startTime">The start time of the last frame. Frame 0 requires the current time, all other ones require references to the same variable.</param>
        /// <param name="grid">Should be the static grid usually (always).</param>
        /// <returns></returns>
        static TickFinishedAction[] DrawFrame(ref DateTime startTime, ref bool[,] grid)
        {
            TickFinishedAction[] requiredActions = new TickFinishedAction[0] { };

            float deltaTime = (float)(DateTime.Now - startTime).TotalSeconds;
            startTime = DateTime.Now;

            Console.SetCursorPosition(0, 1);
            Console.Write($"FPS: {(int)(1 / deltaTime)}; Speed: {block_speed}, Score: {score}");

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


        /// <summary>
        /// Generates a new block in the way Tetris originally did.
        /// There's a block pool which contains all of the blocks in the beginning.
        /// One block is taken from the pool at random.
        /// Once there are no blocks left, the pool resets.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="allBlocks"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
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
            block.notAnObsticle = true;


            block.position.posX = 20;
            block.position.posY = 10;

            return (block, prototype);
        }

        /// <summary>
        /// Shows a screen to the player asking the player
        /// to enter their name and stores the result to the PLAYER_NAME
        /// variable if OK.
        /// </summary>
        public static void RequestPlayerName()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(20, 10);
            Console.Write("Player name: ___________");
            Console.SetCursorPosition(33, 10);

            Console.CursorVisible = true;
            playerName = Console.ReadLine();
            Console.CursorVisible = false;

            // Check name 
            Regex regex = new Regex(@"[a-zA-Z][a-zA-Z0-9]*");

            if (!regex.IsMatch(playerName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(20, 11);
                Console.WriteLine("Expression must match this regex pattern: '[a-zA-Z][a-zA-Z0-9]*'.");
                Console.ForegroundColor = ConsoleColor.Red;
                RequestPlayerName();        // Maybe the recursion should be avoided here but I couldn't be bothered less to be honest.
                                            // Nobody is realistically gonna spam enter.
            }

            if (playerName.Length > MAX_PLAYER_NAME_LENGTH)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(20, 11);
                Console.WriteLine($"This name is too long (maximum: {MAX_PLAYER_NAME_LENGTH}, actual: {playerName.Length}).");
                Console.ForegroundColor = ConsoleColor.Red;
                RequestPlayerName();
            }

            Input.Program.gameData.lastPlayer = playerName;

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

            DrawLeaderboard(page * 10, 10, bestScores);

            while (!Input.Program.ProcessKey(Input.Program.Input.Exit))
            {
                if (Input.Program.ProcessKey(Input.Program.Input.Left))
                {
                    if (page > 0) DrawLeaderboard(--page * 10, 10, bestScores);
                }

                if (Input.Program.ProcessKey(Input.Program.Input.Right))
                {
                    if (page < bestScores.Count / 10) DrawLeaderboard(++page * 10, 10, bestScores);
                }

                Thread.Sleep(16);
            }

            Console.Clear();
        }

        static void DrawLeaderboard(int startIndex, int length, List<Score> scoresSorted)
        {
            Console.Clear();

            int rowLength = 3 + MAX_PLAYER_NAME_LENGTH + 5 + 2 * 4;
            string emptyRowString = "";

            int center = 10 + rowLength / 2;

            var lastIndex = startIndex + length;

            if (lastIndex > scoresSorted.Count) lastIndex = scoresSorted.Count;

            string title = $"Leaderboard ({startIndex + 1}...{lastIndex} from {scoresSorted.Count})";

            Console.SetCursorPosition(center - title.Length / 2, 5);    // Center the text above the board
            Console.Write(title);



            for (int i = 0; i < rowLength; i++)
            {
                emptyRowString += ' ';
            }

            int itemsCount = 0;
            for (int i = startIndex; i < startIndex + length && i < scoresSorted.Count; i++)
            {
                itemsCount++;
                Score item = scoresSorted[i];

                Console.BackgroundColor = GetBackgroundColorForScore(item, i);

                if (Console.BackgroundColor == Program.BACKGROUND_COLOR)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                var posY = 10 + i - startIndex;
                // Draw the background
                Console.SetCursorPosition(10, posY);
                Console.Write(emptyRowString);      
                // Draw the text
                Console.SetCursorPosition(10, posY);
                Console.Write($"{i + 1,3}.\t{item.playerName,MAX_PLAYER_NAME_LENGTH}\t{item.value,5}");
            }


            int buttonsY = 10 + itemsCount + 5;
            bool startOfList = startIndex == 0;
            var listEnded = startIndex + length > scoresSorted.Count;
            DrawButton("Previous", !startOfList, 10, buttonsY);
            DrawButton("Next", !listEnded, 10 + rowLength - 4, buttonsY);
        }

        /// <summary>
        /// Gets the color that should be used as a background for the row 
        /// with the given score in the leaderboard.
        /// </summary>
        /// <param name="score"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ConsoleColor GetBackgroundColorForScore(Score score, int index)
        {
            if (score.playerName == playerName)
            {
                return ConsoleColor.Red;
            }

            switch (index)
            {
                case 0:
                    return ConsoleColor.DarkYellow;
                case 1:
                    return ConsoleColor.Gray;
                case 2:
                    return ConsoleColor.DarkGray;
                default:
                    return Program.BACKGROUND_COLOR;
            }
        }

        static void DrawButton(string text, bool selectable, int posX, int posY)
        {
            Console.SetCursorPosition(posX, posY);
            Console.BackgroundColor = selectable ? ConsoleColor.White : Program.BACKGROUND_COLOR;
            Console.ForegroundColor = selectable ? Program.BACKGROUND_COLOR : ConsoleColor.White;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = Program.BACKGROUND_COLOR;
        }

        static void ShowSettings()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;

            int selectedEntry = 0;
            int rowLength = 20 + 2 * 8 + 10;
            string emptyRowString = "";
            for (int i = 0; i < rowLength+2; i++)
            {
                emptyRowString += ' ';
            }

            while (true)
            {
                for (int i = 0; i < settings.Length; i++)
                {
                    ISetting setting = settings[i];
                    Console.BackgroundColor = i == selectedEntry ? ConsoleColor.Blue : Program.BACKGROUND_COLOR;
                    Console.SetCursorPosition(10, 5 + i);
                    Console.Write(emptyRowString);
                    Console.SetCursorPosition(12, 5 + i);
                    Console.Write(setting.GetName());
                    Console.SetCursorPosition(46, 5 + i);
                    Console.Write($"{setting.GetValue(),10}");
                    

                    
                    if (i == selectedEntry)
                    {
                        Console.BackgroundColor = Program.BACKGROUND_COLOR;
                        string[] descriptionLines = setting.GetDescription().Split('\n');

                        for (int j = 0; j < descriptionLines.Length; j++)
                        {
                            string line = descriptionLines[j].Trim();
                            Console.SetCursorPosition(10, 40 + j);
                            Console.Write(line + emptyRowString + emptyRowString);
                        }

                        for (int j = 0; j < 5 - descriptionLines.Length; j++)
                        {
                            Console.SetCursorPosition(10, 40 + j + descriptionLines.Length);
                            Console.Write(emptyRowString + emptyRowString + emptyRowString);
                        }
                    }
                }

                while (true)
                {
                    if (Input.Program.ProcessKey(Input.Program.Input.Down))
                    {
                        if (selectedEntry < settings.Length - 1) selectedEntry++;
                        break;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.RotateUp))
                    {
                        if (selectedEntry > 0) selectedEntry--;
                        break;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.Select))
                    {
                        settings[selectedEntry].Select();
                        break;
                    }

                    if (Input.Program.ProcessKey(Input.Program.Input.Exit))
                    {
                        Console.Clear();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// This should've been an abstract class.
    /// Anyway, this is how settings are represented.
    /// </summary>
    public interface ISetting
    {
        string GetName();
        string GetValue();
        string GetDescription();
        void Select();
    }

    public class NameSetting: ISetting
    {
        public string GetName()
        {
            return "Current Player";
        }

        public string GetValue()
        {
            return Program.playerName;
        }

        public string GetDescription()
        {
            return "The name of the player who's currently playing the game. Required for the leaderboard.";
        }

        public void Select()
        {
            Console.Clear();
            Program.RequestPlayerName();
            Console.Clear();
        }
    }

    public class NamePreservationSetting: ISetting
    {
        public string GetName()
        {
            return "Preserve Player";
        }

        public string GetValue()
        {
            return Input.Program.gameData.preservePlayerName ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "During game start, you won't be asked for your name\nand the last player will be used for that.";
        }

        public void Select()
        {
            Input.Program.gameData.preservePlayerName = !Input.Program.gameData.preservePlayerName;
            Input.Program.StoreGameData();
        }
    }

    public class ExtremeModeSetting : ISetting
    {
        public string GetName()
        {
            return "Extreme Mode";
        }

        public string GetValue()
        {
            return Input.Program.gameData.extremeMode ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "Makes the game a bit harder.";
        }

        public void Select()
        {
            Input.Program.gameData.extremeMode = !Input.Program.gameData.extremeMode;
            Input.Program.StoreGameData();
        }
    }

    public class TitleScreenSetting : ISetting
    {
        public string GetName()
        {
            return "Show Title Screen";
        }

        public string GetValue()
        {
            return Input.Program.gameData.showTitleScreen ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "Whether the title screen should be shown for 4s or be skipped instead.";
        }

        public void Select()
        {
            Input.Program.gameData.showTitleScreen = !Input.Program.gameData.showTitleScreen;
            Input.Program.StoreGameData();
        }
    }

    /// <summary>
    /// Contains information about what might've happened
    /// during a frame that a block can't handle by itself.
    /// </summary>
    public enum TickFinishedAction
    {
        CreateNewBlock,
        Died,
    }

    public class BlockPrototype
    {
        public ConsoleColor color;
        public bool[,] shape;

        public BlockPrototype(ConsoleColor color, bool[,] shape)
        {
            this.color = color;
            this.shape = shape;
        }
    }

    /// <summary>
    /// A block as it exists in the grid. 
    /// This can draw and rotate and more.
    /// </summary>
    public struct Block : IGameObject
    {
        public const byte tileWidth = 4;
        public const byte tileHeight = 2;

        /// <summary>
        /// The block prototype the block stems from.
        /// This is used for rotation mainly. 
        /// After the block has been placed, this is set to null to save memory.
        /// </summary>
        public BlockPrototype prototype;
        public PositionInfo position;

        /// <summary>
        /// The shape already rotated and potentially cut in bit-map format.
        /// </summary>
        public byte[] currentShape;
        public byte width;
        private float time;
        public ConsoleColor color;

        public bool notAnObsticle;

        /// <summary>
        /// If the block is "placed", it can no longer move
        /// </summary>
        public bool placed;


        /// <summary>
        /// Removes the row from the block if it exists.
        /// </summary>
        /// <param name="row">The row in the grid layout (absolute).</param>
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
                    var targetY = (position.posY * Block.tileHeight) + (row * tileHeight) + j;
                    if (targetY < Console.BufferHeight)
                    {
                        Console.SetCursorPosition(position.posX * Block.tileWidth, targetY);
                    }
                    
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

            Console.BackgroundColor = Program.BACKGROUND_COLOR;
        }

        /// <summary>
        /// Gets the height of the block.
        /// </summary>
        /// <returns></returns>
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

        public PositionInfo GetSpaceInfo()
        {
            return this.position;
        }

        /// <summary>
        /// Erases the previous position and reraws the block in the new position.
        /// </summary>
        /// <param name="originalSpace"></param>
        public void Redraw(PositionInfo originalSpace)
        {
            var newPos = this.position;
            var colour = this.color;
            this.color = Program.BACKGROUND_COLOR;
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
            if (this.notAnObsticle) return false;
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                byte rowContents = currentShape[row];
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

        /// <summary>
        /// Whether the structure is outside the grid, touching another block or not.
        /// This only works while the block hasn't made an entry to the grid, meaning 
        /// that it shouldn't be placed at that point.
        /// </summary>
        /// <param name="previousGrid"></param>
        /// <returns></returns>
        private bool InterferesWithGrid(bool[,] previousGrid)
        {
            for (int row = 0; row < currentShape.GetLength(0); row++)
            {
                byte rowContents = currentShape[row];
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

        /// <summary>
        /// Does whatever is necessary e.g. moving the block, rotating it and such.
        /// </summary>
        /// <param name="deltaTime">The time difference (in s) between now and the last time this has been executed.</param>
        /// <param name="requiredActions">There for when the block triggers something it can't handle itself.</param>
        /// <param name="grid">Where blocks have been placed statically already.</param>
        public void Tick(float deltaTime, ref TickFinishedAction[] requiredActions, ref bool[,] grid)
        {
            if (this.placed)
            {
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
                }
                else
                {
                    needsRedrawing = true;
                }
            }




            if (this.time > Program.block_speed | Input.Program.ProcessKey(Input.Program.Input.Down))
            {
                this.time = 0;
                this.position.posY++;

                int lowerEdgePos = this.position.posY + this.GetHeightBlocks();
                int lowerEdgePosPhysical = lowerEdgePos * Block.tileHeight;
                if (InterferesWithGrid(grid)/*lowerEdgePosPhysical>=Programm.GRID_END*/)
                {
                    Tetris.Program.score++;
                    Tetris.Program.block_speed *= Program.BLOCK_SPEED_INCREASE;
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
                    this.currentShape = new byte[prototype.shape.GetLength(0)];
                    for (int i = 0; i < this.currentShape.GetLength(0); i++)
                    {
                        byte contents = 0;

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
                    this.currentShape = new byte[width];



                    for (int i = 0; i < width; i++)
                    {
                        byte contents = 0;

                        for (int j = height-1; j >= 0; j--)
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
                    this.currentShape = new byte[prototype.shape.GetLength(0)];
                    var height_ = this.currentShape.GetLength(0);
                    for (int i = 0; i < height_; i++)
                    {
                        byte contents = 0;

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
                    this.currentShape = new byte[width_];



                    for (int i = 0; i < width_; i++)
                    {
                        byte contents = 0;

                        for (int j = 0; j < height__; j++)
                        {
                            bool filled = prototype.shape[j, i];
                            if (filled)
                            {
                                contents |= 1;
                            }
                            contents <<= 1;
                        }


                        this.currentShape[width_ - i - 1] = contents;
                    }
                    break;

                default: throw new Exception();
            }
        }

        public bool CanEverTick()
        {
            return true;
        }

        public Block(BlockPrototype prototype, byte x, byte y)
        {
            this.prototype = prototype;
            this.position = new PositionInfo(x, y);
            this.width = (byte)prototype.shape.GetLength(1);
            this.currentShape = new byte[prototype.shape.GetLength(0)];
            this.placed = false;
            this.time = 0;
            this.color = prototype.color;
            this.notAnObsticle = false;

            for (int i = 0; i < this.currentShape.GetLength(0); i++)
            {
                byte contents = 0;

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


    public struct PositionInfo
    {
        public byte rotation;
        public byte posX;
        public byte posY;


        public PositionInfo(byte posX, byte posY)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = 0;
        }

        public PositionInfo(byte posX, byte posY, byte rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }
    }


    /// <summary>
    /// The worst mistake of this game.
    /// Anyway, this is for anything that needs drawing and potentially re-drawing.
    /// For things, that need to have executed Tick(...) on them every frame.
    /// </summary>
    public interface IGameObject
    {
        /// <summary>
        /// Draws the object once.
        /// </summary>
        void Draw();
        /// <summary>
        /// Draws the object and removes it in the position provided.
        /// </summary>
        /// <param name="originalSpace"></param>
        void Redraw(PositionInfo originalSpace);

        /// <summary>
        /// Retrieves the current position and rotation of the object.
        /// </summary>
        /// <returns></returns>
        PositionInfo GetSpaceInfo();

        void Tick(float deltaTime, ref TickFinishedAction[] requiredActions, ref bool[,] grid);

        /// <summary>
        /// Whether the object should be ticked.
        /// </summary>
        /// <returns></returns>
        bool CanEverTick();

        /// <summary>
        /// For blocks only. This proves that this should've been a class
        /// instead of an interface all along.
        /// </summary>
        /// <param name="index"></param>
        void RemoveRow(int index);
    }
}

namespace Input
{
    internal class Program
    {
        /// <summary>
        /// The inputs that haven't been handled yet.
        /// </summary>
        static public List<Input> currentInputs = new List<Input> { };
        static public bool running = true;


        public const int MAX_X = 150;
        public const int MAX_Y = 55;

        private const string SAVE_FILE_NAME = "game_data.json";

        /// <summary>
        /// All data that should be persistent.
        /// </summary>
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

        /// <summary>
        /// Loads persistent data from the disk.
        /// </summary>
        public static void LoadGameData()
        {
            if (File.Exists(SAVE_FILE_NAME))
            {
                string jsonString = File.ReadAllText(SAVE_FILE_NAME);
                gameData = JsonSerializer.Deserialize<GameData>(jsonString);

                if (gameData.preservePlayerName)
                {
                    Tetris.Program.playerName = gameData.lastPlayer;
                }
            } else
            {
                gameData = GameData.GetNew();
            }
        }

        /// <summary>
        /// Stores persistent data to the disk.
        /// </summary>
        public static void StoreGameData()
        {
            var name = gameData.lastPlayer;

            if (!gameData.preservePlayerName)
            {
                gameData.lastPlayer = "";
            }

            string jsonString = JsonSerializer.Serialize(gameData);
            File.WriteAllText(SAVE_FILE_NAME, jsonString);

            gameData.lastPlayer = name;
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
            Console.ForegroundColor = ConsoleColor.White;


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



                Console.SetCursorPosition(10, 2);
                Console.WriteLine("Menu (Quit with 'esc' or select quit)");

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
            StoreGameData();

            System.Environment.Exit(0);

        }
    }

    public class GameData
    {
        [JsonPropertyName("highscores")]
        public List<Score> highscores { get; set; }

        [JsonPropertyName("preserve_player_name")]
        public bool preservePlayerName { get; set; }

        [JsonPropertyName("last_player")]
        public string lastPlayer { get; set; }

        [JsonPropertyName("extreme_mode")]
        public bool extremeMode { get; set; }

        [JsonPropertyName("show_title_screen")]
        public bool showTitleScreen { get; set; }


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
            data.preservePlayerName = false;
            data.extremeMode = false;
            data.showTitleScreen = true;

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
