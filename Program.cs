// Program.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMaze
{
    class Program
    {
        // This is the program entry point.
        static void Main( string[] args )
        {
            RectangularGraph rectangularGraph = new RectangularGraph( 10, 20 );

            Maze maze = new Maze( rectangularGraph );

            int second = DateTime.Now.Second;
            int millisecond = DateTime.Now.Millisecond;
            int seed = second * 1000 + millisecond;
            maze.Generate( seed );
        }
    }
}

// Program.cs