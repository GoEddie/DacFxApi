using System;
using System.Collections.Generic;
using System.IO;
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

            foreach (
                TSqlObject index in
                    model.GetObjects(DacQueryScopes.Default, Index.TypeClass /*Index lets you access different types, 
                                                                                            *Table.TypeClass will give tables and Procedures.TypeClass gives procedures*/)
                )
            {
                //index is a weakly typed TSqlObject that can be queried to get the properties and relationships
                DumpIndex(index);
            }
        }

        private static void DumpIndex(TSqlObject index)
        {
            //Each TSqlObject has a name property:
            ObjectIdentifier indexName = index.Name;

            //Top level objects like tables, procedures and indexes will let you get the underlying script to generate them, doing this on things like columns fails
            string script = "";

            if (!index.TryGetScript(out script))
            {
                script = "Can only script top level objects";
            }

            //To get to individual properties we need to use the static schema container classes, each property can be called directly or you can ask an object for all it's child properties
            var allowPageLocks = index.GetProperty<bool?>(Index.AllowPageLocks);
            var isClustered = index.GetProperty<bool?>(Index.Clustered);

            Console.WriteLine("Index: " + indexName);
            Console.WriteLine("Properties: Is Clustered: {0}, Allow Page Locks: {1}", isClustered, allowPageLocks);
            
            //To get the columns we need to ask for the relationships of the index and then enumerate through them
            foreach (ModelRelationshipInstance column in index.GetReferencedRelationshipInstances(Index.Columns))
            {
                DumpColumn(column, "Column");
            }
            
            //Included columns are referenced using the relationships but are a slightly different class
            foreach (ModelRelationshipInstance column in index.GetReferencedRelationshipInstances(Index.IncludedColumns))
            {
                DumpColumn(column, "Included");
            }

            Console.WriteLine("Script:");
            Console.WriteLine(script);
            Console.WriteLine("===============================");
        }

        private static void DumpColumn(ModelRelationshipInstance column, string type)
        {
            Console.WriteLine("{0}: {1}", type, column.ObjectName);
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