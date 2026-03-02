using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal class BlockPrototype
    {
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
                            { true, true, true, },
                            { true, false, false,  },
                            { false, false, false, },
                            { false, false, false, }
                        }
                    ),
                  new BlockPrototype(
                        ConsoleColor.DarkBlue,
                        new bool[,]
                        {
                            { true, true, true, },
                            { false, false, true,  },
                            { false, false, false, },
                            { false, false, false, }
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

        public ConsoleColor color;
        public bool[,] shape;

        public BlockPrototype(ConsoleColor color, bool[,] shape)
        {
            this.color = color;
            this.shape = shape;
        }
    }
}
