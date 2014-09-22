using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CountLinesOfTSqlCode
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || CheckArg(args[0], "?"))
            {
                PrintArgs();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File: \"{0}\" does not exist :(", args[0]);
                PrintArgs();
                return;
            }
            
            var scripts = GetScriptsInModel(args[0]);


            var parser = new TSql110Parser(true);

            foreach (var script in scripts)
            {
                var reader = new StringReader(script) as TextReader;
                IList<ParseError> errors = null;
                var fragment = parser.Parse(reader, out errors);
                SQLVisitor visitor = new SQLVisitor("");
                fragment.AcceptChildren(visitor);
                visitor.DumpStatistics(); 
            }

        }

    class SQLVisitor : TSqlFragmentVisitor 
    {
        private readonly string _proc;
        private int SELECTcount = 0; 
        private int INSERTcount = 0; 
        private int UPDATEcount = 0; 
        private int DELETEcount = 0;

        public SQLVisitor(string proc)
        {
            _proc = proc;
        }

        private string GetNodeTokenText(TSqlFragment fragment) 
        { 
            StringBuilder tokenText = new StringBuilder(); 
            for (int counter = fragment.FirstTokenIndex; counter <= fragment.LastTokenIndex; counter++) 
            { 
                tokenText.Append(fragment.ScriptTokenStream[counter].Text); 
            }

            return tokenText.ToString(); 
        }

        private int statementCount = -1;

        public override void Visit(TSqlStatement node)
        {
            statementCount++;
            Console.WriteLine("found statement statement with text: " + GetNodeTokenText(node));
        }

        
        public void DumpStatistics() 
        { 
            Console.WriteLine(string.Format("Found {0} statements in proc: {1}", 
               statementCount , _proc)); 
        } 
    } 

        private static List<string> GetScriptsInModel(string fileName)
        {
            var scripts = new List<string>();

            var model = new TSqlModel(fileName, DacSchemaModelStorageType.File);

            foreach (var procedure in model.GetObjects(DacQueryScopes.Default, Procedure.TypeClass))
            {
                AddScript(procedure, scripts);
            }

            foreach (var func in model.GetObjects(DacQueryScopes.Default, ScalarFunction.TypeClass))
            {
                AddScript(func, scripts);
            }

            foreach (var func in model.GetObjects(DacQueryScopes.Default, TableValuedFunction.TypeClass))
            {
                AddScript(func, scripts);
            }

            foreach (var trigger in model.GetObjects(DacQueryScopes.Default, DmlTrigger.TypeClass))
            {
                AddScript(trigger, scripts);
            }

            return scripts;
        }

        private static void AddScript(TSqlObject procedure, List<string> scripts)
        {
            var script = "";
            if (procedure.TryGetScript(out script))
            {
                scripts.Add(script);
            }
        }


        private static void PrintArgs()
        {
            Console.WriteLine("IndexProperties DacFx Api Sample:\r\n\r\n\tDumpIndexProperties PathToDacFile\r\n");
        }

        private static bool CheckArg(string arg, string argToCheck)
        {
            if (arg.Length < 1 + argToCheck.Length)
                return false;

            return arg.Substring(1, argToCheck.Length).ToLowerInvariant() == argToCheck;
        }
    }
}
