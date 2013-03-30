// Program.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CmdLineParser;

namespace CSharpMaze
{
    class Program
    {

        static void PrintUsage()
        {
            System.Console.WriteLine( "Usage: CSharpMaze [rect|circle <rows cols|rings>] [imageFile <file>] [solve] [seed <seed>]" );
        }

        // This is the program entry point.
        static void Main( string[] args )
        {
            CmdLineArgument rectArg = new CmdLineArgument( "rect", 2 );
            CmdLineArgument circArg = new CmdLineArgument( "circ", 1 );
            CmdLineArgument imageFileArg = new CmdLineArgument( "imageFile", 1 );
            CmdLineArgument solveMazeArg = new CmdLineArgument( "solve" );
            CmdLineArgument seedArg = new CmdLineArgument( "seed", 1 );

            if( !CmdLineArgument.ParseCmdLine( args ) )
            {
                PrintUsage();
                return;
            }

            if( rectArg.OptionGiven() && circArg.OptionGiven() )
            {
                PrintUsage();
                System.Console.WriteLine( "Supply rectangular maze arguments or circular maze arguments, but not both." );
                return;
            }

            ShapeGraph graph = null;

            if( rectArg.OptionGiven() )
            {
                int rows = Convert.ToInt32( rectArg.GetOptionArgument(0) );
                int cols = Convert.ToInt32( rectArg.GetOptionArgument(1) );
                graph = new RectangularGraph( rows, cols );
            }
            else if( circArg.OptionGiven() )
            {
                int rings = Convert.ToInt32( circArg.GetOptionArgument(0) );
                graph = new CircularGraph( rings );
            }

            if( graph == null )
            {
                PrintUsage();
                System.Console.WriteLine( "Please supply the maze type: \"rect\" or \"circ\"." );
                return;
            }

            String imageFile = null;
            if( imageFileArg.OptionGiven() )
                imageFile = imageFileArg.GetOptionArgument( 0 );
            else
            {
                PrintUsage();
                System.Console.WriteLine( "Please supply the image file name." );
                return;
            }

            Maze maze = new Maze( graph );

            int seed = 0;
            if( seedArg.OptionGiven() )
                seed = Convert.ToInt32( seedArg.GetOptionArgument( 0 ) );
            else
            {
                int second = DateTime.Now.Second;
                int millisecond = DateTime.Now.Millisecond;
                seed = second * 1000 + millisecond;
            }

            maze.Generate( seed );

            if( solveMazeArg.OptionGiven() )
                maze.Solve();

            Bitmap bitmap = new Bitmap( 1024, 1024, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
            maze.Render( bitmap );
            bitmap.Save( imageFile );
        }
    }
}

// Program.cs