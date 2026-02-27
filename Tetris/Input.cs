using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

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

                if (gameData.PreservePlayerName)
                {
                    Tetris.Program.playerName = gameData.LastPlayer;
                }
            }
            else
            {
                gameData = GameData.GetNew();
            }
        }

        /// <summary>
        /// Stores persistent data to the disk.
        /// </summary>
        public static void StoreGameData()
        {
            var name = gameData.LastPlayer;

            if (!gameData.PreservePlayerName)
            {
                gameData.LastPlayer = "";
            }

            string jsonString = JsonSerializer.Serialize(gameData);
            File.WriteAllText(SAVE_FILE_NAME, jsonString);

            gameData.LastPlayer = name;
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




        public static void Start(string[] _)
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


            while (!currentInputs.Contains(Input.Exit))
            {
                Console.ForegroundColor = ConsoleColor.White;


                Console.SetCursorPosition(10, 2);
                Console.WriteLine("Menu (Quit with 'esc' or select quit)");

                for (int i = 0; i < options.Length; i++)
                {
                    Tetris.Program.DrawBar(longestEntry + 2, (10, 4 + i), i == selectedEntry ? ConsoleColor.Blue : Tetris.Program.BACKGROUND_COLOR);
                    Console.SetCursorPosition(11, 4 + i);
                    Console.WriteLine(options[i]);
                }


                string warningText = $"Scale down font (height: {Console.WindowHeight}, required: {MAX_Y})";
                bool resolutionSufficient = !(Console.WindowHeight < MAX_Y);
                if (!resolutionSufficient)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(warningText);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Tetris.Program.DrawBar(warningText.Length + 1, null);
                }




                var currentResolutionX = Console.WindowHeight;
                while (true)
                {
                    if (ProcessKey(Input.RotateUp))
                    {
                        if (selectedEntry > 0)
                        {
                            selectedEntry--;
                            break;
                        }
                    }

                    if (ProcessKey(Input.Down))
                    {
                        if (selectedEntry < options.Length - 1)
                        {
                            selectedEntry++;
                            break;
                        }
                    }

                    if (ProcessKey(Input.Exit))
                    {
                        Program.running = false;
                        StoreGameData();
                        Environment.Exit(0);
                    }

                    if (ProcessKey(Input.Select))
                    {
                        if (resolutionSufficient)
                        {
                            return;
                        }
                    }

                    if (Console.WindowHeight != currentResolutionX)
                    {
                        break;
                    }

                    Thread.Sleep(16);
                }
            }

            Program.running = false;
            StoreGameData();

            Environment.Exit(0);

        }
    }

    public class GameData
    {
        [JsonPropertyName("highscores")]
        public List<Score> Highscores { get; set; }

        [JsonPropertyName("preserve_player_name")]
        public bool PreservePlayerName { get; set; }

        [JsonPropertyName("last_player")]
        public string LastPlayer { get; set; }

        [JsonPropertyName("extreme_mode")]
        public bool ExtremeMode { get; set; }

        [JsonPropertyName("show_title_screen")]
        public bool ShowTitleScreen { get; set; }

        [JsonPropertyName("play_music")]
        public bool PlayMusic { get; set; }


        /// <summary>
        /// Sets the new highscore for a player if it is that player's best.
        /// </summary>
        /// <param name="player">The player to asign the score to</param>
        /// <param name="score">The score that player reached</param>
        public void RegisterScore(string player, int score)
        {
            if (Highscores == null)
            {
                Highscores = new List<Score>();
            }

            for (int i = 0; i < Highscores.Count; i++)
            {
                Score playerScore = Highscores[i];

                if (playerScore.PlayerName != player) continue;
                if (playerScore.Value > score) return;

                Highscores[i].Value = score;
                return;
            }

            Highscores.Add(new Score(player, score));
        }

        public static GameData GetNew()
        {
            var data = new GameData
            {
                Highscores = new List<Score>(),
                PreservePlayerName = false,
                ExtremeMode = false,
                ShowTitleScreen = true,
                PlayMusic = true
            };

            return data;
        }
    }

    public class Score
    {
        [JsonPropertyName("player_name")]
        public string PlayerName { get; set; }
        [JsonPropertyName("value")]
        public int Value { get; set; }


        public Score(string playerName, int value)
        {
            this.PlayerName = playerName;
            this.Value = value;
        }
    }
}
