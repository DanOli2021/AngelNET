using MathNet.Numerics.LinearAlgebra.Solvers;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AngelDB
{

    public static class PythonCommands
    {
        public static Dictionary<string, string> Commands()
        {
            Dictionary<string, string> commands = new Dictionary<string, string>
            {
                { @"IS INSTALLED", @"IS INSTALLED#free" },
                { @"EXEC", @"EXEC#free;MESSAGE#freeoptional" },
                { @"FUNCTION", @"FUNCTION#free;MESSAGE#freeoptional" },
                { @"SET PATH", @"SET PATH#free" },
                { @"ENGINE SHUTDOWN", @"ENGINE SHUTDOWN#free" },
                { @"FILE", @"FILE#free;ON APPLICATION DIRECTORY#optional;MESSAGE#freeoptional" },
                { @"CLASS", @"CLASS#free;MESSAGE#freeoptional" },
            };

            return commands;

        }

    }


    public class PythonScript
    {

        public AngelDB.DB main_db { get; set; }
        public AngelDB.DB db { get; set; }
        public PyModule scope = null;

        public string ExecutePythonScript(string commmand, AngelDB.DB db, AngelDB.DB server_db)
        {

            this.db = db;
            this.main_db = db;

            if (server_db is not null)
            {
                this.db = server_db;
            }

            DbLanguage l = new DbLanguage();
            l.SetCommands(PythonCommands.Commands());
            Dictionary<string, string> d = l.Interpreter(commmand);

            if (!string.IsNullOrEmpty(l.errorString)) return l.errorString;

            if (d.First().Key == "set_path")
            {
                Environment.SetEnvironmentVariable("PYTHON_PATH", d["set_path"]);
                return "Ok.";
            }

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PYTHON_PATH")))
            {
                return "Error: PYTHON_PATH not found. Install https://www.python.org/downloads/, Install for all users, then Use command PYTHON SET PATH <C:/Program Files/Python311/python311.dll> ";
            }

            if (!File.Exists(Environment.GetEnvironmentVariable("PYTHON_PATH"))) return "Error: File not found: " + Environment.GetEnvironmentVariable("PYTHON_PATH");

            if (Runtime.PythonDLL == null)
            {
                Runtime.PythonDLL = Environment.GetEnvironmentVariable("PYTHON_PATH");
            }

            if (PythonEngine.IsInitialized == false)
            {
                PythonEngine.Initialize();
            }

            string result = "";

            switch (d.First().Key)
            {
                case "is_installed":

                    result = IsPythonInstalled();
                    break;

                case "exec":

                    result = PythonExecute(d["exec"], db, server_db, d["message"]);
                    break;

                case "file":

                    result = ExecutePythonFile(d["file"], d["message"], d["on_application_directory"]);
                    break;

                case "engine_shutdown":

                    PythonEngine.Shutdown();
                    return "Ok.";

                case "class":

                    result = PythonExecuteClass(d["class"], db, server_db, d["message"]);
                    break;

                default:
                    result = "Error: Command not found: " + commmand;
                    break;
            }

            return result;

        }

        public string IsPythonInstalled()
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python";
                start.Arguments = "--version";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;

                using (Process process = Process.Start(start))
                {
                    return process.StandardOutput.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                return $"Error: Is PythonInstalled {e}";
            }
        }



        public string ExecutePythonFile(string fileName, string message, string on_application_directory)
        {

            string file;

            if (on_application_directory == "true")
            {
                file = AppDomain.CurrentDomain.BaseDirectory + db.os_directory_separator + fileName;
            }
            else
            {
                file = fileName;
            }

            if (!File.Exists(fileName))
            {
                return "Error: Python File not found: " + fileName;
            }

            string scriptPython = File.ReadAllText(fileName);
            return PythonExecute(scriptPython, db, main_db, message);

        }

        public string PythonExecute(string scriptPython, AngelDB.DB db, AngelDB.DB server_db, string message)
        {
            var state = PythonEngine.BeginAllowThreads();
            string result = "";

            using (Py.GIL())
            {
                try
                {
                    if (scope is null)
                    {
                        scope = Py.CreateScope();
                    }

                    if (scope.Contains("mainclass"))
                    {
                        scope.Remove("mainclass");
                    }

                    scope.Set("db", db.ToPython());
                    scope.Set("server_db", server_db.ToPython());
                    scope.Set("message", message.ToPython());
                    scope.Exec(scriptPython);

                    if (scope.Contains("mainclass"))
                    {
                        dynamic myClass = scope.Get("mainclass");
                        dynamic myObject = myClass();
                        result = myObject.main(db, server_db, message);
                    };

                }
                catch (PythonException ex)
                {
                    result = "Error: Python Script: " + ex.Message;
                }
            }

            PythonEngine.EndAllowThreads(state);
            return result;

        }

        public string PythonExecuteClass(string className, AngelDB.DB db, AngelDB.DB server_db, string message)
        {

            var state = PythonEngine.BeginAllowThreads();
            string result = "";

            try
            {
                using (Py.GIL())
                {
                    if (scope is null)
                    {
                        return "Error: Scope has not been initialized, You need to run a python script first";
                    }

                    if( scope.Contains(className) == false)
                    {
                        return "Error: Class not found: " + className;
                    }

                    dynamic myClass = scope.Get(className);
                    dynamic myObject = myClass();
                    result = myObject.main(db, server_db, message);

                    if (string.IsNullOrEmpty(result)) 
                    {
                        result = "";
                    }

                }

            }
            catch (PythonException ex)
            {
                result = "Error: Python Script: " + ex.Message;
            }

            PythonEngine.EndAllowThreads(state);
            return result;

        }




    }

}











