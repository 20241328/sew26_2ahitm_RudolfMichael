using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Input;


// Michael Rudolf - 2AHITM - Tetris

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
        public const ConsoleColor GRID_COLOR = ConsoleColor.DarkGray;
        public const byte GRID_HEIGHT = 25;
        public const byte GRID_WIDTH = 10;
        public const byte NEXT_POS_Y = 5;
        public const byte NEXT_POS_X = 20;
        public const byte START_Y = 1;
        public const byte START_X = GRID_WIDTH / 2;
        public const int MAX_PLAYER_NAME_LENGTH = 15;


        public static int score;
        public static string playerName = "";
        public static float block_speed = 1f;
        public static bool slowDownDrawing = false;
        


        private static readonly string[] options = new string[] { "Start Game", "Highscores", "Settings", "Manual", "Quit", };

        private static SoundPlayer soundPlayer;
        private static List<IGameObject> objects = new List<IGameObject>();



        private static readonly string[] manual = new string[]
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



        
        public static ISetting[] settings =
        {
            new NameSetting(),
            new NamePreservationSetting(),
            new ExtremeModeSetting(),
            new TitleScreenSetting(),
            new MusicSetting(),
        };

        /// <summary>
        /// Builds the grid's outer sides as described.
        /// </summary>
        private static void SetUpGrid()
        {
            staticGrid = new bool[GRID_HEIGHT + 2, GRID_WIDTH + 2];

            for (int i = 0; i < staticGrid.GetLength(0); i++)
            {
                staticGrid[i, 0] = true;
                staticGrid[i, GRID_WIDTH + 1] = true;

                if (i == 0 || i >= GRID_HEIGHT)
                {
                    for (int j = 1; j < staticGrid.GetLength(1) - 1; j++)
                    {
                        staticGrid[i, j] = true;

                    }
                }
            }

            DrawGrid();
        }

        private static void DrawGrid()
        {
            int startX = 1 * Block.tileWidth;
            int startY = START_Y * Block.tileHeight;

            int lengthX = GRID_WIDTH * Block.tileWidth;
            int stopY = GRID_HEIGHT * Block.tileHeight;

            for (int i = startY; i < stopY; i++)
            {
                DrawBar(lengthX, (startX, i), GRID_COLOR);
            }

            Console.BackgroundColor = BACKGROUND_COLOR;
        }

        static void Main(string[] args)
        {
            // Set the encoding to en-/de-code special characters
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;


            Console.Clear();

            Input.Program.Start(args);
            Input.Program.LoadGameData();

            Console.Clear();


            if (Input.Program.gameData.ShowTitleScreen)
            {
                slowDownDrawing = true;
                ShowTitle();
                slowDownDrawing = false;
            }

            // Start the sound if available & requested
            PlayMusic();
            
            // Ask the player for their name if it hasn't been loaded from the game data.
            if (playerName == "") RequestPlayerName();



            int selectedOption = 0;

            while (!Input.Program.currentInputs.Contains(Input.Program.Input.Exit))
            {
                Input.Program.MenuAusgabe(ref selectedOption, options);

                switch (selectedOption)
                {
                    case 0:
                        ShowGame();
                        Input.Program.StoreGameData();
                        break;
                    case 1:
                        ShowHighscores();
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
                Console.SetCursorPosition(10, 6 + i);
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
            foreach (BlockPrototype prototype in BlockPrototype.letters)
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
            DrawGrid();
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].RemoveRow(index);
            }
        }

        static void ShowGame()
        {
            score = 0;
            Console.Clear();

            SetUpGrid();

            List<BlockPrototype> blockPool = new List<BlockPrototype>();
            Random rng = new Random();
            DateTime time = DateTime.Now;

            // Generate the first block and place it at the start position
            (Block nextBlock, _) = GenerateBlock(ref blockPool, BlockPrototype.blockPrototypes, ref rng);
            nextBlock.outsideGrid = false;
            nextBlock.position.posX = START_X;
            nextBlock.position.posY = START_Y; objects.Add(nextBlock);


            // Generate the next block and place it where you can see the next block
            (nextBlock, _) = GenerateBlock(ref blockPool, BlockPrototype.blockPrototypes, ref rng);
            if (!Input.Program.gameData.ExtremeMode)
            {
                nextBlock.outsideGrid = true;
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
                            
                            nextBlock.color = Program.BACKGROUND_COLOR;
                            nextBlock.Draw();
                            nextBlock.color = nextBlock.prototype.color;

                            // 2. Update the position
                            nextBlock.position.posX = START_X;
                            nextBlock.position.posY = START_Y;
                            nextBlock.outsideGrid = false;
                            nextBlock.placed = false;

                            // 3. Add it back to the game objects for it to be ticked
                            objects.Add(nextBlock);

                            // Generate the next block
                            (nextBlock, _) = GenerateBlock(ref blockPool, BlockPrototype.blockPrototypes, ref rng);
                            nextBlock.placed = true;
                            nextBlock.Draw();
                            objects.Add(nextBlock);

                            // A block has been placed, so it needs to be checked if any rows are full
                            for (int row = 1; row < staticGrid.GetLength(0) - 2; row++)
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
                                SetUpGrid();
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
                    DeleteRow(ref objects, GRID_HEIGHT - 1);
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

            Console.SetCursorPosition(0, 0);
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

            Block block1 = new Block(prototype, START_X, START_Y);
            var block = block1;
            block.outsideGrid = true;


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

            Input.Program.gameData.LastPlayer = playerName;

            Console.Clear();
        }


        /// <summary>
        /// Shows highscores and allows the player to go to all pages.
        /// </summary>
        static void ShowHighscores()
        {
            List<Score> bestScores = new List<Score>();

            foreach (Score pScore in Input.Program.gameData.Highscores)
            {
                bool foundPair = false;

                for (int i = 0; i < bestScores.Count; i++)
                {
                    if (bestScores[i].Value < pScore.Value)
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

            DrawHighscores(page * 10, 10, bestScores);

            while (!Input.Program.ProcessKey(Input.Program.Input.Exit))
            {
                if (Input.Program.ProcessKey(Input.Program.Input.Left))
                {
                    if (page > 0) DrawHighscores(--page * 10, 10, bestScores);
                }

                if (Input.Program.ProcessKey(Input.Program.Input.Right))
                {
                    if (page < bestScores.Count / 10) DrawHighscores(++page * 10, 10, bestScores);
                }

                Thread.Sleep(16);
            }

            Console.Clear();
        }

        static void DrawHighscores(int startIndex, int length, List<Score> scoresSorted)
        {
            Console.Clear();

            int rowLength = 3 + MAX_PLAYER_NAME_LENGTH + 5 + 2 * 4;

            int center = 10 + rowLength / 2;

            var lastIndex = startIndex + length;

            if (lastIndex > scoresSorted.Count) lastIndex = scoresSorted.Count;

            string title = $"Highscores ({startIndex + 1}...{lastIndex} from {scoresSorted.Count})";

            Console.SetCursorPosition(center - title.Length / 2, 5);    // Center the text above the board
            Console.Write(title);


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
                DrawBar(rowLength, (10, posY), Console.BackgroundColor);
                // Draw the text
                Console.SetCursorPosition(10, posY);
                Console.Write($"{i + 1,3}.\t{item.PlayerName,MAX_PLAYER_NAME_LENGTH}\t{item.Value,5}");
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
            if (score.PlayerName == playerName)
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

        /// <summary>
        /// Opens and hosts the settings screen until it's exited.
        /// </summary>
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


        /// <summary>
        /// Creates a bar with the background being the specified color.
        /// The background color stays as provided after the function exits.
        /// </summary>
        /// <param name="length">The length (or the rest of the line if null)</param>
        /// <param name="pos">The position of the bar (or the current cursor position if null)</param>
        /// <param name="color">The color the bar should have (or the background color if unspecified)</param>
        public static void DrawBar(int? length, (int, int)? pos, ConsoleColor color = Program.BACKGROUND_COLOR)
        {
            string text = "";
            (int posX, int posY) = pos ?? (Console.CursorLeft, Console.CursorTop);
            int? length_ = length ?? Console.WindowWidth - posX;

            for (int i = 0; i < length_; i++)
            {
                text += ' ';
            }

            Console.SetCursorPosition(posX, posY);
            Console.BackgroundColor = color;
            Console.Write(text);
        }

        /// <summary>
        /// Starts music or stops music, depending on the setting.
        /// </summary>
        public static void PlayMusic()
        {
            const string musicFileName = "791018.wav";
            if (Input.Program.gameData.PlayMusic)
            {
                if (File.Exists(musicFileName))
                {
                    soundPlayer = new SoundPlayer(musicFileName);
                    soundPlayer.PlayLooping();
                }
            } else
            {
                if (soundPlayer != null)
                {
                    soundPlayer.Stop();
                    soundPlayer.Dispose();
                    soundPlayer = null;
                }
            }
        }
    }
}