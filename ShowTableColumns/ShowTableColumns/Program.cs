using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace DumpIndexProperties
{
    internal class Program
    {
        private static void Main(string[] args)
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
            
            var model = new TSqlModel(args[0], DacSchemaModelStorageType.File);
            var table = model.GetObjects(DacQueryScopes.All, ModelSchema.Table);

            var sqlObjects = table as TSqlObject[] ?? table.ToArray();

            if (!sqlObjects.Any())
            {
                Console.WriteLine("Model does not contain any tables :(");
                return;
            }

            ShowColumnsDataType(sqlObjects.First());

            //to do roughly the same thing in linq but specifying a column name:
            var dataType = table.First()
                .GetReferencedRelationshipInstances(Table.Columns)
                .First(p => p.ObjectName.ToString() == "[dbo].[AWBuildVersion].[SystemInformationID]")
                .Object.GetReferenced(Column.DataType)
                .First()
                .Name;

            
        }

        private static void ShowColumnsDataType(TSqlObject table)
        {
            foreach (var child in table.GetReferencedRelationshipInstances(Table.Columns))
            {
                var type = child.Object.GetReferenced(Column.DataType).FirstOrDefault();
                var isNullable = type.GetProperty<bool?> (DataType.UddtNullable);
                var length = type.GetProperty<int?>(DataType.UddtLength);

                //do something useful with this information!
            }
        }
    
       

        private static void PrintArgs()
        {
            Console.WriteLine("ShowTableColumns DacFx Api Sample:\r\n\r\n\tShowTableColumns PathToDacFile\r\n");
        }
        
        private static bool CheckArg(string arg, string argToCheck)
        {
            if (arg.Length < 1 + argToCheck.Length)
                return false;

            return arg.Substring(1, argToCheck.Length).ToLowerInvariant() == argToCheck;
        }
    }
}