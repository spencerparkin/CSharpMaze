// Maze.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace CSharpMaze
{
    class Graph
    {
        public Graph()
        {
            setOfNodes = new List< Node >();
            setOfAdjacencies = new List< Adjacency >();
        }

        // We assume here that the given node is not already a member of this graph.
        // It may well be a member of other any other graph.
        public void Insert( Node node )
        {
            setOfNodes.Add( node );
        }

        public void Remove( Node node )
        {
            setOfNodes.Remove( node );
        }

        // We assume here that the nodes associated with the given adjacency are
        // both members of this graph.
        public void Insert( Adjacency adjacency )
        {
            setOfAdjacencies.Add( adjacency );
        }

        public void Remove( Adjacency adjacency )
        {
            setOfAdjacencies.Remove( adjacency );
        }

        public void ClearAll()
        {
            setOfNodes.Clear();
            ClearAdjacencies();
        }

        public void ClearAdjacencies()
        {
            setOfAdjacencies.Clear();
        }

        public class Node
        {
            public Node( Point location )
            {
                this.location = location;
            }

            public struct Point
            {
                public Point( float x, float y )
                {
                    this.x = x;
                    this.y = y;
                }

                private float x, y;

                public float X
                {
                    set { x = value; }
                    get { return x; }
                }

                public float Y
                {
                    set { y = value; }
                    get { return y; }
                }
            }

            private Point location;

            public Point Location
            {
                set { location = value; }
                get { return location; }
            }
        }

        public class Adjacency
        {
            public Adjacency( Node nodeA, Node nodeB )
            {
                this.nodeA = nodeA;
                this.nodeB = nodeB;
            }

            private Node nodeA, nodeB;

            public Node NodeA
            {
                set { nodeA = value; }
                get { return nodeA; }
            }

            public Node NodeB
            {
                set { nodeB = value; }
                get { return nodeB; }
            }
        }

        // We do not use linked-lists here, because we want to allow for
        // the possibility of a single node existing in multiple graphs.
        // I'm not entirely sure if the linked-list class would have this
        // limitation, but I know that what we use here will work for us.
        private List< Node > setOfNodes;
        private List< Adjacency > setOfAdjacencies;

        public List< Node > SetOfNodes
        {
            get { return setOfNodes; }
        }

        public List< Adjacency > SetOfAdjacencies
        {
            get { return setOfAdjacencies; }
        }
    }

    abstract class ShapeGraph : Graph
    {
        public delegate Node NodeCreatorFunc( Node.Point location );

        private NodeCreatorFunc nodeCreator;

        public NodeCreatorFunc NodeCreator
        {
            set { nodeCreator = value; }
            get { return nodeCreator; }
        }

        public Node CreateNode( Node.Point location )
        {
            return nodeCreator( location );
        }

        public abstract void GenerateGraphShape();
    }

    class RectangularGraph : ShapeGraph
    {
        public RectangularGraph( int rowCount, int colCount )
        {
            this.rowCount = rowCount;
            this.colCount = colCount;
        }

        public override void GenerateGraphShape()
        {
            ClearAll();

            Node[,] nodeMatrix = new Node[ rowCount, colCount ];

            // Create all the nodes.
            for( int row = 0; row < rowCount; row++ )
            {
                for( int col = 0; col < colCount; col++ )
                {
                    Node.Point location = new Node.Point( row, col );
                    Node node = CreateNode( location );
                    Insert( node );
                    nodeMatrix[ row, col ] = node;
                }
            }

            // Create all the adjacencies between the nodes.
            for( int row = 0; row < rowCount; row++ )
            {
                for( int col = 0; col < colCount; col++ )
                {
                    if( row < rowCount - 1 )
                    {
                        Adjacency adjacency = new Adjacency( nodeMatrix[ row, col ], nodeMatrix[ row + 1, col ] );
                        Insert( adjacency );
                    }

                    if( col < colCount - 1 )
                    {
                        Adjacency adjacency = new Adjacency( nodeMatrix[ row, col ], nodeMatrix[ row, col + 1 ] );
                        Insert( adjacency );
                    }
                }
            }
        }

        private int rowCount;
        private int colCount;
    }

    /*
    class CircularGraph : ShapeGraph
    {
        public CircularGraph( int concentricCircleCount )
        {
            this.concentricCircleCount = concentricCircleCount;
        }

        private int concentricCircleCount;
    }
    */

    class Maze
    {
        public Maze( ShapeGraph graph )
        {
            this.graph = graph;
        }

        public Graph.Node MazeNodeCreator( Node.Point point )
        {
            return new Maze.Node( point );
        }

        public bool Render( Bitmap bitmap )
        {
            return true;
        }

        public void Generate( int seed )
        {
            graph.NodeCreator = MazeNodeCreator;
            graph.GenerateGraphShape();

            if( spanningTree == null )
                spanningTree = new Graph();

            spanningTree.ClearAll();
            foreach( Graph.Node node in graph.SetOfNodes )
                spanningTree.Insert( node );
            
            List< Graph.Adjacency > temporaryAdjacencyList = new List< Graph.Adjacency >( graph.SetOfAdjacencies.Count );
            foreach( Graph.Adjacency adjacency in graph.SetOfAdjacencies )
                temporaryAdjacencyList.Add( adjacency );
            
            Random random = new Random( seed );

            List< Graph.Adjacency > shuffledAdjacencyList = new List< Graph.Adjacency >( graph.SetOfAdjacencies.Count );
            while( temporaryAdjacencyList.Count > 0 )
            {
                int index = random.Next( 0, temporaryAdjacencyList.Count - 1 );
                Graph.Adjacency adjacency = temporaryAdjacencyList[ index ];
                temporaryAdjacencyList.RemoveAt( index );
                shuffledAdjacencyList.Add( adjacency );
            }

            int setCount = graph.SetOfNodes.Count;
            foreach( Graph.Adjacency adjacency in shuffledAdjacencyList )
            {
                Node nodeA = ( Node )adjacency.NodeA;
                Node nodeB = ( Node )adjacency.NodeB;

                if( !Node.AreMembersOfSameSet( nodeA, nodeB ) )
                {
                    Node.UnifySetsForEachOf( nodeA, nodeB );
                    setCount--;
                }

                if( setCount == 1 )
                    break;
            }
        }

        private ShapeGraph graph;
        private Graph spanningTree;

        private class Node : Graph.Node
        {
            public Node( Point point ) : base( point )
            {
                nodeLink = null;
            }

            public Node FindRepresentative()
            {
                Node nodeRep = this;

                if( nodeLink != null )
                {
                    nodeRep = nodeLink.FindRepresentative();

                    // This assignment is purely an optimization and is not
                    // needed for the correctness of our algorithm.
                    nodeLink = nodeRep;
                }

                return nodeRep;
            }

            public static bool AreMembersOfSameSet( Node nodeA, Node nodeB )
            {
                Node nodeARep = nodeA.FindRepresentative();
                Node nodeBRep = nodeB.FindRepresentative();

                if( nodeARep == nodeBRep )
                    return true;
                return false;
            }

            public static bool UnifySetsForEachOf( Node nodeA, Node nodeB )
            {
                Node nodeARep = nodeA.FindRepresentative();
                Node nodeBRep = nodeB.FindRepresentative();

                if( nodeARep == nodeBRep )
                    return false;

                // Arbitrarily making nodeA subordinate, if you will, to nodeB.
                nodeARep.nodeLink = nodeBRep;
                return true;
            }

            Node nodeLink;
        }
    }
}

// Maze.cs