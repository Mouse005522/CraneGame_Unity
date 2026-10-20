using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kun.Tool
{
    public static class EnivTool
    {
        public static CommandLineArgsCache CreateCommandLineArgsCache () 
        {
            var args = Environment.GetCommandLineArgs ().ToList ();
            return new CommandLineArgsCache (args);
        }
    }

    public class CommandLineArgsCache
    {
        public CommandLineArgsCache (List<string> commandLineArgs) 
        {
            LoggerRouter.Error ("commandLineArgs");
            commandLineArgs.ForEach (commandLineArg => LoggerRouter.Error (commandLineArg));

            parTable = new Dictionary<string, string> ();

            var headers = commandLineArgs.FindAll (arg => arg.ToCharArray ()[0] == '-');

            var headerPairs = headers.ConvertAll (header => 
            {
                var headerIndex = commandLineArgs.IndexOf (header);

                return (headerIndex, header);
            });

            headerPairs.ForEach (headerPair => 
            {
                var next = headerPair.headerIndex + 1;

                //確定接著的參數真的有值
                if (headerPairs.Exists (pair => pair.headerIndex == next) == false && next < commandLineArgs.Count)
                {
                    var par = commandLineArgs[next];
                    parTable[headerPair.header] = par;
                }
                else
                {
                    parTable[headerPair.header] = "";
                }
            });

            parTable.ForEach ((key,value)=> 
            {
                LoggerRouter.Error ($"key -> {key}, value -> {value}");
            });
        }

        Dictionary<string, string> parTable = new Dictionary<string, string> ();

        public bool TryGetValue (string key, out string value)
        {
            return parTable.TryGetValue (key, out value);
        }
    }
}