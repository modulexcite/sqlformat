using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laan.SQL.Parser.Expressions;

namespace Laan.SQL.Parser
{
    class InsertStatementParser : StatementParser<InsertStatement>
    {

    // INSERT [INTO] table_name [(column_list)] { 
    //      {
    //        VALUES ( { DEFAULT | NULL | expression }[,...n] )[,...n]
    //        | derived_table
    //        | execute_statement    
    //      }

        public InsertStatementParser( ITokenizer tokenizer ) : base( tokenizer ) { }

        private InsertStatement _statement;

        private void ProcessColumnList()
        {
            if ( Tokenizer.IsNextToken( Constants.OpenBracket ) )
            {
                using ( Tokenizer.ExpectBrackets() )
                {
                    _statement.Columns = ( GetIdentifierList() );
                }
            }
        }

        private void ProcessTerminator()
        {
            _statement.Terminated = HasTerminator();
        }

        private void ProcessValues()
        {
            do
            {
                using ( Tokenizer.ExpectBrackets() )
                {
                    _statement.Values.Add( GetIdentifierList() );
                }
            }
            while ( Tokenizer.TokenEquals( Constants.Comma ) );
        }

        private void ProcessSelect()
        {
            ReadNextToken();
            SelectExpression selectExpression = new SelectExpression();

            var parser = new SelectStatementParser( Tokenizer );
            _statement.SourceStatement = parser.Execute();
        }

        private void ProcessExec()
        {
            throw new NotImplementedException();
            //var parser = new ExecuteProcedureStatementParser( Tokenizer );
            //_statement.Procedure = parser.Execute<ExecuteProcuedreStatement>();
        }

        public override InsertStatement Execute()
        {
            _statement = new InsertStatement();

            if ( Tokenizer.TokenEquals( "INTO" ) )
            {
                // consume the current token
            }
            _statement.TableName = GetTableName();

            ProcessColumnList();

            if ( Tokenizer.TokenEquals( Constants.Values ) )
                ProcessValues();
            else if ( Tokenizer.IsNextToken( Constants.Select ) )
                ProcessSelect();
            else if ( Tokenizer.IsNextToken( Constants.Exec ) )
                ProcessExec();
            else
                throw new SyntaxException( String.Format("syntax error after 'INSERT INTO {0} '", _statement.TableName ) );

            ProcessTerminator();

            return _statement;
        }
    }
}
