using System;

namespace Laan.SQL.Parser
{
    public class UpdateStatement : ProjectionStatement
    {
        public int? Top { get; set; }
        public string TableName { get; set; }
    }
}