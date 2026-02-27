using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tetris
{
    /// <summary>
    /// A block as it exists in the grid. 
    /// This can draw and rotate and more.
    /// </summary>
    internal struct Block : IGameObject
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

        /// <summary>
        /// The block is displayed on the side as the next block if true.
        /// </summary>
        public bool outsideGrid;

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
            if (this.outsideGrid) return;

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
            this.color = outsideGrid ? Program.BACKGROUND_COLOR : Program.GRID_COLOR;
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
            if (this.outsideGrid) return false;
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

            if (prototype == null) return;

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

                        for (int j = height - 1; j >= 0; j--)
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
            this.outsideGrid = false;

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
}
