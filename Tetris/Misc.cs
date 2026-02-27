using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{


    /// <summary>
    /// Contains information about what might've happened
    /// during a frame that a block can't handle by itself.
    /// </summary>
    public enum TickFinishedAction
    {
        CreateNewBlock,
        Died,
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
