﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Laan.SQL.Parser;

namespace Laan.SQL.Formatter
{
    public interface IFormattingEngine
    {
        string Execute( string sql );
        int TabSize { get; set; }
        bool UseTabChar { get; set; }
    }

    public class FormattingEngine : IFormattingEngine
    {
        private Dictionary<Type, Type> _formatters;

        /// <summary>
        /// Initializes a new instance of the FormattingEngine class.
        /// </summary>
        public FormattingEngine()
        {
            IndentStep = 0;
            TabSize = 4;
            UseTabChar = false;

            _formatters = new Dictionary<Type, Type>
            {
                { typeof( SelectStatement ), typeof( SelectStatementFormatter ) },
                { typeof( UpdateStatement ), typeof( UpdateStatementFormatter ) },
                { typeof( DeleteStatement ), typeof( DeleteStatementFormatter ) },
                { typeof( GoTerminator ), typeof( GoTerminatorFormatter ) },
//                { typeof( CreateTableStatement ), typeof( CreateTableStatementFormatter ) }
            };
        }

        public string Execute( string sql )
        {
            string indent = UseTabChar ? "\t" : new string( ' ', TabSize );

            var outSql = new StringBuilder();

            List<IStatement> statements = ParserFactory.Execute( sql );
            foreach ( IStatement statement in statements )
            {

                // this is a quick and dirty service locator that maps statements to formatters
                Type formatterType;
                if ( !_formatters.TryGetValue( statement.GetType(), out formatterType ) ) 
                    throw new Exception( "Formatter not implemented for statement: " + statement.GetType().Name );

                var formatter = Activator.CreateInstance( formatterType, indent, IndentStep, outSql, statement ) as IStatementFormatter;

                if ( formatter == null )
                    throw new Exception( "Formatter not instantiated: " + formatterType.Name );
                formatter.Execute();

                if ( statement != statements.Last() )
                    outSql.AppendLine( "\n" );
            }
            return outSql.ToString();
        }

        public int IndentStep { get; set; }
        public int TabSize { get; set; }
        public bool UseTabChar { get; set; }
    }
}
