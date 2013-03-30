// Program.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CSharpMaze
{
    class Program
    {
        // This is the program entry point.
        static void Main( string[] args )
        {
            ShapeGraph graph = null;

            if( args.Length > 0 )
            {
                if( args[0] == "rect" )
                {
                    if( args.Length > 2 )
                    {
                        int rows = Convert.ToInt32( args[1] );
                        int cols = Convert.ToInt32( args[2] );
                        graph = new RectangularGraph( rows, cols );
                    }
                }
                else if( args[0] == "circle" )
                {
                    if( args.Length > 1 )
                    {
                        int concentricCircles = Convert.ToInt32( args[1] );
                        graph = new CircularGraph( concentricCircles );
                    }
                }
            }
            
            if( graph == null )
            {
                System.Console.WriteLine( "Usage: CSharpMaze [rect|circle] [(rows,cols)|circle]" );
                return;
            }

            Maze maze = new Maze( graph );

            int second = DateTime.Now.Second;
            int millisecond = DateTime.Now.Millisecond;
            int seed = second * 1000 + millisecond;
            maze.Generate( seed );

            Bitmap bitmap = new Bitmap( 1024, 1024, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
            maze.Render( bitmap );
            bitmap.Save( "Maze.jpg" );
        }
    }
}

// Program.cs