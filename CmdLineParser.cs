// CmdLineParser.cs

using System;
using System.Collections.Generic;

namespace CmdLineParser
{
    class CmdLineArgument
    {
        private static List< CmdLineArgument > cmdLineArgList;

        public delegate void UnknownOptionFunc( string optionName );
        public delegate void HandleOptionFunc( CmdLineArgument cmdLineArg );

        public static UnknownOptionFunc unknownOptionFunc;
        public HandleOptionFunc handleOptionFunc;

        static CmdLineArgument()
        {
            unknownOptionFunc = null;
        }

        public CmdLineArgument( string optionName, int argCount = 0 )
        {
            this.optionName = optionName;
            this.argCount = argCount;

            if( cmdLineArgList == null )
                cmdLineArgList = new List< CmdLineArgument >();
            cmdLineArgList.Add( this );

            handleOptionFunc = null;
            optionGiven = false;

            argOptionList = new List< string >();
        }

        public bool OptionGiven()
        {
            return optionGiven;
        }

        public string GetOptionArgument( int index )
        {
            if( index < 0 || index > argOptionList.Count - 1 )
                return "";
            return argOptionList[ index ];
        }

        private static CmdLineArgument LookupArg( string token )
        {
            foreach( CmdLineArgument cmdLineArg in cmdLineArgList )
                if( cmdLineArg.optionName == token )
                    return cmdLineArg;
            return null;
        }

        public static bool ParseCmdLine( string[] cmdLine )
        {
            for( int index = 0; index < cmdLine.Length; index++ )
            {
                string token = cmdLine[ index ];
                CmdLineArgument cmdLineArg = LookupArg( token );
                if( cmdLineArg == null )
                {
                    if( unknownOptionFunc != null )
                        unknownOptionFunc( token );
                    return false;
                }
                else
                {
                    cmdLineArg.optionGiven = true;
                    for( int argIndex = 0; argIndex < cmdLineArg.argCount; argIndex++ )
                    {
                        if( index + 1 + argIndex >= cmdLine.Length )
                            return false;
                        string optionArgString = cmdLine[ index + 1 + argIndex ];
                        cmdLineArg.argOptionList.Add( optionArgString );
                    }
                    index += cmdLineArg.argCount;
                }
            }

            // Now that everything is parsed, go call all registered handlers.
            foreach( CmdLineArgument cmdLineArg in cmdLineArgList )
                if( cmdLineArg.optionGiven && cmdLineArg.handleOptionFunc != null )
                    cmdLineArg.handleOptionFunc( cmdLineArg );

            return true;
        }

        private string optionName;
        private int argCount;
        private bool optionGiven;
        private List< string > argOptionList;

        public string OptionName
        {
            get { return optionName; }
        }
    }
}

// CmdLineParser.cs