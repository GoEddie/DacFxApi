using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CountLinesOfTSqlCode
{
    public class SqlVisitor : TSqlFragmentVisitor
    {
        private readonly string _proc;

        public SqlVisitor(string proc)
        {
            _proc = proc;
            StatementCount = -1;   /*we get the create call with all children and then the indeividual statements*/
        }

        public int StatementCount { get; private set; } 

        public override void Visit(TSqlStatement node)
        {
            StatementCount++;
        }

        public override void ExplicitVisit(SelectStatement node)
        {
            
        }
    }
}