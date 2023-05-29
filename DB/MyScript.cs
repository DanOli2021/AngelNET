using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace AngelDB
{
    public class MyScript : IDisposable
    {

        public Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        public Dictionary<string, CompiledScripts> Compiled_scripts = new Dictionary<string, CompiledScripts>();
        private bool disposedValue;

        public string Eval(string code, string data, AngelDB.DB db)
        {
            try
            {
                Globals g = new Globals();
                g.db = db;
                g.data = data;

                ScriptOptions options = ScriptOptions.Default.WithImports(new[] { "System", "System.Net", "System.Collections.Generic" });
                object o = CSharpScript.EvaluateAsync(code, options, g).GetAwaiter().GetResult();

                if (o != null)
                {
                    return o.ToString();
                }

                return "";

            }
            catch (Exception e)
            {
                return $"Error: {e.ToString()}";
            }

        }

        public string EvalExpresion(string key, string code, string data, AngelDB.DB db, DataRow r)
        {
            try
            {

                Globals g = new Globals();
                g.db = db;
                g.data = data;
                g.data_row = r;

                if (scripts.ContainsKey(key))
                {
                    return scripts[key].RunAsync(g).Result.ReturnValue.ToString();
                }

                ScriptOptions options = ScriptOptions.Default.WithImports(new[] { "System", "System.Net", "System.Collections.Generic" });

                var script = CSharpScript.Create(code, options, typeof(Globals));
                script.Compile();

                string result = script.RunAsync(g).Result.ReturnValue.ToString();

                scripts.Add(key, script);
                return result;

            }
            catch (Exception e)
            {
                return $"Error: {e.ToString()}";
            }

        }

        public string EvalFile(Dictionary<string, string> d, AngelDB.DB db, bool use_memory = false)
        {

            FileInfo fileInfo = null;
            string file = "";

            try
            {

                GC.Collect();

                file = d["script_file"];

                if (d["on_main_directory"] == "true")
                {
                    file = db.BaseDirectory + "/" + d["script_file"];
                }
                else if (d["on_database"] == "true")
                {
                    file = db.BaseDirectory + db.os_directory_separator + db.account + db.os_directory_separator + db.database + db.os_directory_separator + d["script_file"];
                }
                else if (d["on_table"] != "null")
                {
                    file = db.BaseDirectory + db.os_directory_separator + db.account + db.os_directory_separator + db.database + db.os_directory_separator + d["on_table"] + db.os_directory_separator + d["script_file"];
                }
                else if (d["on_application_directory"] == "true")
                {
                    file = Environment.CurrentDirectory + "/" + d["script_file"];
                }
                else if (db.apps_directory != "null")
                {
                    if (!string.IsNullOrEmpty(db.apps_directory))
                    {
                        file = db.apps_directory + db.os_directory_separator + d["script_file"];
                    }
                }

                if (!File.Exists(file))
                {
                    return $"Error: The script file does not exists {file}";
                }

                Globals g = new Globals();
                g.db = db;
                g.data = d["data"];
                g.message = d["message"];

                db.script_file_datetime = File.GetLastWriteTime(file);
                db.script_file = file;

                if (Compiled_scripts.ContainsKey(file))
                {
                    if (db.script_file_datetime == Compiled_scripts[file].script_date)
                    {
                        var o1 = Compiled_scripts[file].script.RunAsync(g).Result.ReturnValue;

                        if (o1 is null)
                        {
                            return "";
                        }
                        else
                        {
                            return o1.ToString();
                        }

                    }
                }

                string code = File.ReadAllText(file);
                int pos = code.IndexOf("END GLOBALS");

                if (pos > 0)
                {
                    string globals = code.Substring(0, pos + 11);
                    int lines = globals.Split('\n').Length;
                    code = new string('\n', lines - 1) + code.Substring(pos + 11);
                }


                List<string> l = new List<string>();
                l.Add(typeof(AngelDB.DB).Assembly.FullName);
                ScriptOptions options;

                fileInfo = new FileInfo(file);

                ImmutableArray<string> array = ImmutableArray.Create(fileInfo.Directory.FullName);

                options = ScriptOptions.Default.
                            WithFilePath(AppDomain.CurrentDomain.BaseDirectory + fileInfo.Name).
                            AddImports("System").
                            WithSourceResolver(new SourceFileResolver(array, fileInfo.Directory.FullName)).
                            AddReferences(l).WithEmitDebugInformation(true).WithFileEncoding(Encoding.UTF8);

                var script = CSharpScript.Create(code, options, typeof(Globals));
                script.Compile();

                if (Compiled_scripts.ContainsKey(file))
                {
                    Compiled_scripts.Remove(file);
                }

                Compiled_scripts.Add(file, new CompiledScripts { script = script, script_date = db.script_file_datetime });
                var o = Compiled_scripts[file].script.RunAsync(g).Result.ReturnValue;

                if (o is null)
                {
                    return "";
                }
                else
                {
                    return o.ToString();
                }
                //object o = CSharpScript.EvaluateAsync(code, options, g).GetAwaiter().GetResult();

                //if (o != null)
                //{
                //    return o.ToString();
                //}

                //return "";

            }
            catch (Exception e2)
            {
                string error = e2.ToString();
                int pos = error.IndexOf("End of stack trace from previous location");
                if (pos > 0)
                {
                    error = error.Substring(0, pos);
                }

                return $"Error: {file} {error}";
            }

        }

        public string EvalFileForBlazor(string file, AngelDB.DB db, string data, string message)
        {

            FileInfo fileInfo = null;

            try
            {

                GC.Collect();

                if (!File.Exists(file))
                {
                    return $"Error: The script file does not exists {file}";
                }

                Globals g = new Globals();
                g.db = db;
                g.data = data;
                g.message = message;

                db.script_file_datetime = File.GetLastWriteTime(file);
                db.script_file = file;

                if (Compiled_scripts.ContainsKey(file))
                {
                    if (db.script_file_datetime == Compiled_scripts[file].script_date)
                    {
                        var o1 = Compiled_scripts[file].script.RunAsync(g).Result.ReturnValue;

                        if (o1 is null)
                        {
                            return "";
                        }
                        else
                        {
                            return o1.ToString();
                        }

                    }
                }

                string code = File.ReadAllText(file);
                int pos = code.IndexOf("END GLOBALS");

                if (pos > 0)
                {
                    string globals = code.Substring(0, pos + 11);
                    int lines = globals.Split('\n').Length;
                    code = new string('\n', lines - 1) + code.Substring(pos + 11);
                }

                List<string> l = new List<string>();
                l.Add(typeof(AngelDB.DB).Assembly.FullName);
                ScriptOptions options;

                fileInfo = new FileInfo(file);

                ImmutableArray<string> array = ImmutableArray.Create(fileInfo.Directory.FullName);

                options = ScriptOptions.Default.
                            WithFilePath(AppDomain.CurrentDomain.BaseDirectory + fileInfo.Name).
                            AddImports("System").
                            WithSourceResolver(new SourceFileResolver(array, fileInfo.Directory.FullName)).
                            AddReferences(l).WithEmitDebugInformation(true).WithFileEncoding(Encoding.UTF8);

                var script = CSharpScript.Create(code, options, typeof(Globals));

                var compilation = script.GetCompilation();

                script.Compile();

                if (Compiled_scripts.ContainsKey(file))
                {
                    Compiled_scripts.Remove(file);
                }

                Compiled_scripts.Add(file, new CompiledScripts { script = script, script_date = db.script_file_datetime });
                var o = Compiled_scripts[file].script.RunAsync(g).Result.ReturnValue;

                if (o is null)
                {
                    return "";
                }
                else
                {
                    return o.ToString();
                }
                //object o = CSharpScript.EvaluateAsync(code, options, g).GetAwaiter().GetResult();

                //if (o != null)
                //{
                //    return o.ToString();
                //}

                //return "";

            }
            catch (Exception e2)
            {
                string error = e2.ToString();
                int pos = error.IndexOf("End of stack trace from previous location");
                if (pos > 0)
                {
                    error = error.Substring(0, pos);
                }

                return $"Error: {file} {error}";
            }

        }


        public string CompileFileForBlazor(string file, AngelDB.DB db, string file_assembly = "")
        {

            FileInfo fileInfo = null;

            try
            {

                if (!File.Exists(file))
                {
                    return $"Error: The script file does not exists {file}";
                }

                Globals g = new Globals();
                g.db = db;
                g.data = "";
                g.message = "";

                string code = File.ReadAllText(file);
                int pos = code.IndexOf("END GLOBALS");

                if (pos > 0)
                {
                    string globals = code.Substring(0, pos + 11);
                    int lines = globals.Split('\n').Length;
                    code = new string('\n', lines - 1) + code.Substring(pos + 11);
                }

                List<string> l = new List<string>();
                l.Add(typeof(AngelDB.DB).Assembly.FullName);
                ScriptOptions options;

                fileInfo = new FileInfo(file);

                ImmutableArray<string> array = ImmutableArray.Create(fileInfo.Directory.FullName);

                options = ScriptOptions.Default.
                            WithFilePath(AppDomain.CurrentDomain.BaseDirectory + fileInfo.Name).
                            AddImports("System").
                            WithSourceResolver(new SourceFileResolver(array, fileInfo.Directory.FullName)).
                            AddReferences(l).WithEmitDebugInformation(true).WithFileEncoding(Encoding.UTF8);

                var script = CSharpScript.Create(code, options, typeof(Globals));
                var compilation = script.GetCompilation();

                if (string.IsNullOrEmpty(file_assembly))
                {
                    file_assembly = Path.ChangeExtension(file, "dll");
                }

                var result = compilation.Emit(file_assembly);

                if (result.Success)
                {
                    return "Ok.";
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Error: ");

                foreach (var item in result.Diagnostics)
                {
                    sb.AppendLine(item.ToString());
                }

                return sb.ToString();

            }
            catch (Exception e2)
            {
                string error = e2.ToString();
                int pos = error.IndexOf("End of stack trace from previous location");
                if (pos > 0)
                {
                    error = error.Substring(0, pos);
                }

                return $"Error: {file} {error}";
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
                scripts = new Dictionary<string, Script>();
                Compiled_scripts = null;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~MyScript()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


    }


}


public class Globals
{
    public AngelDB.DB db;
    public string return_result = "";
    public string data = "";
    public string message = "";
    public DataRow data_row;
}

public class CompiledScripts
{
    public System.DateTime script_date;
    public Script script;
}

