using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    public class TableKeyGeneratorData_Int : TableKeyGeneratorData<int>
    {
        public TableKeyGeneratorData_Int (List<(string key, int value)> drawKeyPairs, List<GeneratorData> nodes) : base (drawKeyPairs, nodes)
        {

        }

        protected override string GetFromatValue (int value)
        {
            return $"{value}";
        }

        protected override string ShowTypeName => "int";
    }

    public class TableKeyGeneratorData_Float : TableKeyGeneratorData<float>
    {
        public TableKeyGeneratorData_Float (List<(string key, float value)> drawKeyPairs, List<GeneratorData> nodes) : base (drawKeyPairs, nodes)
        {

        }

        protected override string GetFromatValue (float value)
        {
            return $"{value}f";
        }

        protected override string ShowTypeName => "float";
    }

    public class TableKeyGeneratorData_String : TableKeyGeneratorData<string>
    {
        /// <summary>
        /// key就是value的場合
        /// </summary>
        /// <param name="drawKeys"></param>
        /// <param name="nodes"></param>
        public TableKeyGeneratorData_String (List<string> drawKeys, List<GeneratorData> nodes) : base (drawKeys.ConvertAll (key =>
        {
            return (key, key);
        }), nodes)
        {

        }

        public TableKeyGeneratorData_String (List<(string key, string value)> drawKeyPairs, List<GeneratorData> nodes) : base (drawKeyPairs, nodes)
        {

        }

        protected override string GetFromatValue (string value)
        {
            return $"\"{value}\"";
        }

        protected override string ShowTypeName => "string";
    }

    public abstract class TableKeyGeneratorData<T> : GeneratorData
    {
        public TableKeyGeneratorData (List<(string key, T vale)> drawKeyPairs, List<GeneratorData> nodes) : base (nodes)
        {
            this.drawKeyPairs = drawKeyPairs.ToList ();
        }

        protected override void GeneratorContent ()
        {
            for (int i = 0; i < drawKeyPairs.Count; i++)
            {
                string key = drawKeyPairs[i].key;

                string processKey = GetProhibitKeyReplace (key);

                var value = GetFromatValue (drawKeyPairs[i].value);

                string keyLine = $"public const {ShowTypeName} {processKey} = {value};";

                ProcessAddLine (keyLine);

                if (i < drawKeyPairs.Count - 1)
                {
                    ProcessAddLine ("");
                }
            }
        }

        protected override bool ExitHook
        {
            get
            {
                return false;
            }
        }

        List<(string key, T value)> drawKeyPairs;

        /// <summary>
        /// 舉例來說 string的前後要加上"", float結尾要加上f
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract string GetFromatValue (T value);

        protected abstract string ShowTypeName { get; }
    }
}