﻿// Maze.cs

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

        protected Node mazeStart;
        protected Node mazeFinish;

        public Node MazeStart
        {
            get { return mazeStart; }
        }
        public Node MazeFinish
        {
            get { return mazeFinish; }
        }

        protected float minX, maxX;
        protected float minY, maxY;

        // Expand our current region of the XY-plane to match the given aspect ratio.
        public void ExpandGraphRegion( float aspectRatio )
        {
            float deltaX = maxX - minX;
            float deltaY = maxY - minY;
            float currentAspectRatio = deltaX / deltaY;

            if( currentAspectRatio < aspectRatio )
            {
                float delta = 0.5f * ( deltaY * aspectRatio - deltaX );
                minX -= delta;
                maxX += delta;
            }
            else if( currentAspectRatio > aspectRatio )
            {
                float delta = 0.5f * ( deltaX / aspectRatio - deltaY );
                minY -= delta;
                maxY += delta;
            }
        }

        public void GraphSpaceToImageSpace( Bitmap bitmap, ref Node.Point point, out int row, out int col )
        {
            float t = ( point.X - minX ) / ( maxX - minX );
            col = ( int )( t * ( float )bitmap.Width );
            t = ( point.Y - minY ) / ( maxY - minY );
            row = ( int )( ( 1.0f - t ) * ( float )bitmap.Height );
        }
    }

    class RectangularGraph : ShapeGraph
    {
        public RectangularGraph( int rowCount, int colCount )
        {
            minX = -1.0f;
            maxX = ( float )colCount;
            minY = -1.0f;
            maxY = ( float )rowCount;

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
                float y = ( float )( rowCount - row - 1 );

                for( int col = 0; col < colCount; col++ )
                {
                    float x = ( float )col;    

                    Node.Point location = new Node.Point( x, y );
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

            mazeStart = nodeMatrix[ 0, 0 ];
            mazeFinish = nodeMatrix[ rowCount - 1, colCount - 1 ];
        }

        private int rowCount;
        private int colCount;
    }

    class CircularGraph : ShapeGraph
    {
        public CircularGraph( int concentricCircleCount )
        {
            minX = -( float )concentricCircleCount * 0.5f - 0.5f;
            maxX = ( float )concentricCircleCount * 0.5f + 0.5f;
            minY = -( float )concentricCircleCount * 0.5f - 0.5f;
            maxY = ( float )concentricCircleCount * 0.5f + 0.5f;

            this.concentricCircleCount = concentricCircleCount;
        }

        public override void GenerateGraphShape()
        {
            ClearAll();

            Node[][] nodeRingArray = new Node[ concentricCircleCount ][];

            int nodesPerRing = 7;
            float radiusDelta = 0.5f;
            float radius = 0.5f;

            for( int ringIndex = 0; ringIndex < concentricCircleCount; ringIndex++ )
            {
                Node[] nodeRing = new Node[ nodesPerRing ];
                nodeRingArray[ ringIndex ] = nodeRing;

                for( int index = 0; index < nodesPerRing; index++ )
                {
                    float angle = ( float )index / ( float )nodesPerRing * 2.0f * ( float )Math.PI;
                    float x = radius * ( float )Math.Cos( angle );
                    float y = radius * ( float )Math.Sin( angle );

                    Node.Point location = new Node.Point( x, y );
                    Node node = CreateNode( location );
                    nodeRing[ index ] = node;
                    Insert( node );
                }

                radius += radiusDelta;

                if( ringIndex < concentricCircleCount - 1 )
                {
                    float angle = ( 1.0f / ( float )nodesPerRing ) * 2.0f * ( float )Math.PI;
                    float arcLength = radius * angle;
                    if( arcLength > 1.0f )
                        nodesPerRing *= 2;
                }                
            }

            Node centerNode = CreateNode( new Node.Point( 0.0f, 0.0f ) );
            Insert( centerNode );
            mazeStart = centerNode;
            mazeFinish = nodeRingArray[ concentricCircleCount - 1 ][0];

            Adjacency adjacency = new Adjacency( centerNode, nodeRingArray[0][0] );
            Insert( adjacency );

            for( int ringIndex = 0; ringIndex < concentricCircleCount; ringIndex++ )
            {
                Node[] nodeRing = nodeRingArray[ ringIndex ];
                Node[] nextNodeRing = null;
                int indexScalar = 1;
                if( ringIndex < concentricCircleCount - 1 )
                {
                    nextNodeRing = nodeRingArray[ ringIndex + 1 ];
                    if( nextNodeRing.Length > nodeRing.Length )
                        indexScalar = 2;
                }

                for( int index = 0; index < nodeRing.Length; index++ )
                {
                    int nextIndex = ( index + 1 ) % nodeRing.Length;
                    Node nodeA = nodeRing[ index ];
                    Node nodeB = nodeRing[ nextIndex ];
                    adjacency = new Adjacency( nodeA, nodeB );
                    Insert( adjacency );

                    if( nextNodeRing != null )
                    {
                        nodeA = nodeRing[ index ];
                        nodeB = nextNodeRing[ index * indexScalar ];
                        adjacency = new Adjacency( nodeA, nodeB );
                        Insert( adjacency );
                    }
                }
            }
        }

        private int concentricCircleCount;
    }

    class Maze
    {
        public Maze( ShapeGraph graph )
        {
            this.graph = graph;

            solutionSet = new List< Graph.Adjacency >();
        }

        public Graph.Node MazeNodeCreator( Node.Point point )
        {
            return new Maze.Node( point );
        }

        public void ImageSpaceAdjacency( Bitmap bitmap, Graph.Adjacency adjacency, out System.Drawing.Point drawPointA, out System.Drawing.Point drawPointB )
        {
            Node.Point pointA = adjacency.NodeA.Location;
            Node.Point pointB = adjacency.NodeB.Location;

            int row, col;
            graph.GraphSpaceToImageSpace( bitmap, ref pointA, out row, out col );
            drawPointA = new System.Drawing.Point( col, row );
            graph.GraphSpaceToImageSpace( bitmap, ref pointB, out row, out col );
            drawPointB = new System.Drawing.Point( col, row );
        }

        public bool Render( Bitmap bitmap )
        {
            if( spanningTree == null || spanningTree.SetOfNodes.Count == 0 )
                return false;

            float aspectRatio = ( float )bitmap.Width / ( float )bitmap.Height;
            graph.ExpandGraphRegion( aspectRatio );

            Graphics graphics = Graphics.FromImage( bitmap );

            Rectangle rectangle = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
            graphics.FillRectangle( new SolidBrush( Color.Black ), rectangle );

            float minDistance = 999.0f;
            foreach( Graph.Adjacency adjacency in spanningTree.SetOfAdjacencies )
            {
                System.Drawing.Point drawPointA, drawPointB;
                ImageSpaceAdjacency( bitmap, adjacency, out drawPointA, out drawPointB );
                float dx = drawPointA.X - drawPointB.X;
                float dy = drawPointA.Y - drawPointB.Y;
                float distance = ( float )Math.Sqrt( dx * dx + dy * dy );
                if( minDistance > distance )
                    minDistance = distance;
            }

            Pen pen = new Pen( Color.White);
            int penWidth = ( int )( minDistance / 2.0f );
            pen.Width = penWidth > 0 ? penWidth : 1;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;

            // Go draw all the path-ways of the maze.
            foreach( Graph.Adjacency adjacency in spanningTree.SetOfAdjacencies )
            {
                System.Drawing.Point drawPointA, drawPointB;
                ImageSpaceAdjacency( bitmap, adjacency, out drawPointA, out drawPointB );
                graphics.DrawLine( pen, drawPointA, drawPointB );
            }

            // Draw the solution too if one was generated.
            pen.Width = 1;
            pen.Color = Color.Red;
            foreach( Graph.Adjacency adjacency in solutionSet )
            {
                System.Drawing.Point drawPointA, drawPointB;
                ImageSpaceAdjacency( bitmap, adjacency, out drawPointA, out drawPointB );
                graphics.DrawLine( pen, drawPointA, drawPointB );
            }
            
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

                    Graph.Adjacency mazePathway = new Graph.Adjacency( nodeA, nodeB );
                    spanningTree.Insert( mazePathway );
                }

                if( setCount == 1 )
                    break;
            }
        }

        public bool Solve()
        {
            solutionSet.Clear();

            if( spanningTree == null || spanningTree.SetOfAdjacencies.Count == 0 )
                return false;

            if( graph.MazeStart == null || graph.MazeFinish == null )
                return false;

            // At this point, if we fail to find a solution, it means there is a problem
            // with our maze generation algorithm.

            foreach( Node node in spanningTree.SetOfNodes )
                node.visited = false;

            foreach( Graph.Adjacency adjacency in spanningTree.SetOfAdjacencies )
            {
                Node nodeA = ( Node )adjacency.NodeA;
                Node nodeB = ( Node )adjacency.NodeB;

                nodeA.adjNodeList.Add( nodeB );
                nodeB.adjNodeList.Add( nodeA );
            }

            Queue< Node > queue = new Queue< Node >();
            queue.Enqueue( ( Node )graph.MazeStart );

            while( queue.Count > 0 )
            {
                Node node = queue.Dequeue();
                node.visited = true;

                if( node == graph.MazeFinish )
                    break;

                foreach( Node adjNode in node.adjNodeList )
                {
                    if( !adjNode.visited )
                    {
                        adjNode.nodeParent = node;
                        queue.Enqueue( adjNode );
                    }
                }
            }

            for( Node node = ( Node )graph.MazeFinish; node != null; node = node.nodeParent )
                node.partOfSolution = true;

            foreach( Graph.Adjacency adjacency in spanningTree.SetOfAdjacencies )
            {
                Node nodeA = ( Node )adjacency.NodeA;
                Node nodeB = ( Node )adjacency.NodeB;

                if( nodeA.partOfSolution && nodeB.partOfSolution )
                    solutionSet.Add( adjacency );
            }

            return true;
        }

        private ShapeGraph graph;
        private Graph spanningTree;
        private List< Graph.Adjacency > solutionSet;

        private class Node : Graph.Node
        {
            public Node( Point point ) : base( point )
            {
                nodeLink = null;
                visited = false;
                partOfSolution = false;
                adjNodeList = new List< Node >();
                nodeParent = null;
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

            private Node nodeLink;

            public bool visited;
            public bool partOfSolution;
            public List< Node > adjNodeList;
            public Node nodeParent;
        }
    }
}

// Maze.cs