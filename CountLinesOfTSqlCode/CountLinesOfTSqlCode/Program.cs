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

            bool detailedOutput = DetailedOutput();

            var scripts = GetScriptsInModel(args[0]);
            
            
            var lineCount = 0;

            foreach (var script in scripts)
            {
                var scriptLineCount = GetStatementCountFromFile(script);

                lineCount += scriptLineCount;
    
                if (detailedOutput)
                {
                    Console.WriteLine("{0} contains: {1} statements", script.Name, scriptLineCount);
                }
            }

            Console.WriteLine(lineCount);
        }

        private static int GetStatementCountFromFile(CodeUnit script)
        {
            try
            {
                var parser = new TSql110Parser(true);
                var reader = new StringReader(script.Code) as TextReader;
                IList<ParseError> errors = null;
                var fragment = parser.Parse(reader, out errors);

                if (errors != null && errors.Count> 0)
                {
                    Console.WriteLine("Error unable to parse script file: \"{0}\"", script.Name);

                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.Message);
                    }

                    return 0;
                }

                var visitor = new SqlVisitor(script.Name);
                fragment.AcceptChildren(visitor);
                return visitor.StatementCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing script: \"{0}\" error: \"{1}\"", script.Name, ex.Message);
                return 0;
            }
        }

        private static bool DetailedOutput()
        {
            return Environment.CommandLine.ToLowerInvariant().IndexOf("detailed") > -1;
        }

        private static IEnumerable<CodeUnit> GetScriptsInModel(string fileName)
        {
            var scripts = new List<CodeUnit>();

            var model = new TSqlModel(fileName, DacSchemaModelStorageType.File);

            scripts.AddRange(GetCodeUnits(model.GetObjects(DacQueryScopes.Default, Procedure.TypeClass)));
            scripts.AddRange(GetCodeUnits(model.GetObjects(DacQueryScopes.Default, ScalarFunction.TypeClass)));
            scripts.AddRange(GetCodeUnits(model.GetObjects(DacQueryScopes.Default, TableValuedFunction.TypeClass)));
            scripts.AddRange(GetCodeUnits(model.GetObjects(DacQueryScopes.Default, DmlTrigger.TypeClass)));

            return scripts;
        }

        private static IEnumerable<CodeUnit> GetCodeUnits(IEnumerable<TSqlObject> objects )
        {
            var units = new List<CodeUnit>();

            foreach (var trigger in objects)
            {
                units.Add(new CodeUnit()
                {
                    Name = trigger.Name.ToString(),
                    Code = GetScript(trigger)
                });
            }

            return units;
        } 


        private static string GetScript(TSqlObject procedure)
        {
            var script = "";
            if (procedure.TryGetScript(out script))
                return script;

            return "";  //could throw an exception or logged this if we care??
        }


        private static void PrintArgs()
        {
            Console.WriteLine("IndexProperties DacFx Api Sample:\r\n\r\n\tCountLinesOfTSqlCode PathToDacFile [/detailed]\r\n");
        }

        private static bool CheckArg(string arg, string argToCheck)
        {
            if (arg.Length < 1 + argToCheck.Length)
                return false;

            return arg.Substring(1, argToCheck.Length).ToLowerInvariant() == argToCheck;
        }
    }
}
