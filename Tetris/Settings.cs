using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    /// <summary>
    /// This should've been an abstract class.
    /// Anyway, this is how settings are represented.
    /// </summary>
    internal interface ISetting
    {
        string GetName();
        string GetValue();
        string GetDescription();
        void Select();
    }

    public class NameSetting : ISetting
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

    public class NamePreservationSetting : ISetting
    {
        public string GetName()
        {
            return "Preserve Player";
        }

        public string GetValue()
        {
            return Input.Program.gameData.PreservePlayerName ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "During game start, you won't be asked for your name\nand the last player will be used for that.";
        }

        public void Select()
        {
            Input.Program.gameData.PreservePlayerName = !Input.Program.gameData.PreservePlayerName;
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
            return Input.Program.gameData.ExtremeMode ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "Makes the game a bit harder.\n This was originally a bug and functionality is limited.";
        }

        public void Select()
        {
            Input.Program.gameData.ExtremeMode = !Input.Program.gameData.ExtremeMode;
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
            return Input.Program.gameData.ShowTitleScreen ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "Whether the title screen should be shown for 4s or be skipped instead.";
        }

        public void Select()
        {
            Input.Program.gameData.ShowTitleScreen = !Input.Program.gameData.ShowTitleScreen;
            Input.Program.StoreGameData();
        }
    }


    public class MusicSetting : ISetting
    {
        public string GetName()
        {
            return "Play Music";
        }

        public string GetValue()
        {
            return Input.Program.gameData.PlayMusic ? "Yes" : "No";
        }

        public string GetDescription()
        {
            return "If true, the original Tetris music will play.";
        }

        public void Select()
        {
            Input.Program.gameData.PlayMusic = !Input.Program.gameData.PlayMusic;
            Input.Program.StoreGameData();


            Program.PlayMusic();
        }
    }
}
