using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Data;
using Newtonsoft.Json;

namespace AngelDB
{
	public class PhytonScripts
	{

		public string console_out = "";

		private ScriptEngine m_engine = IronPython.Hosting.Python.CreateEngine();
		private ScriptScope m_scope = null;
		private DB db;
		public PhytonScripts(DB db)
		{

			var paths = m_engine.GetSearchPaths();
			paths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + "py");
			paths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + "lib");

			m_engine.SetSearchPaths(paths);
			m_scope = m_engine.CreateScope();
			m_scope.SetVariable("db", db);
			this.db = db;

		}

		public void SetStdOut()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine("import sys");
			sb.AppendLine("db_console_out = ''");
			sb.AppendLine("class OutPutEnviroment:");
			sb.AppendLine("    def write( self, maintext ):");
			sb.AppendLine("        global db_console_out");
			sb.AppendLine("        db_console_out += maintext");
			sb.AppendLine("sys.stdout = OutPutEnviroment()");

			ScriptSource source = m_engine.CreateScriptSourceFromString(sb.ToString());
			source.Execute(m_scope);

		}
		public string EvalDictionary(Dictionary<string, string> d)
		{
			return Eval(d["py"], d["message"]);
		}


		public string Eval(string code, string message, string additionalinfo = "")
		{

			try
			{

				ScriptSource source = m_engine.CreateScriptSourceFromString(code);
				m_scope.SetVariable("message", message);
				dynamic result = source.Execute(m_scope);

				if (m_scope.ContainsVariable("db_console_out"))
				{
					console_out = m_scope.GetVariable("db_console_out");
					m_scope.SetVariable("db_console_out", "");
				}

				if (result == null)
				{
					result = "";
				}

				return (string)result;
			}
			catch (Exception e)
			{
				string messageType;
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				IList<Microsoft.Scripting.Runtime.DynamicStackFrame> l = m_engine.GetService<ExceptionOperations>().GetStackFrames(e);

				m_engine.GetService<ExceptionOperations>().GetExceptionMessage(e, out message, out messageType);

				sb.AppendLine("Error:");
				sb.AppendLine(message.Replace("Error:", ""));

				foreach (Microsoft.Scripting.Runtime.DynamicStackFrame item in l)
				{
					sb.AppendLine($"Line: {item.GetFileLineNumber()}");

					if (item.GetFileName() != "<string>")
					{
						sb.AppendLine($"FileName: {item.GetFileName()}");
					}
				}

				if (!string.IsNullOrEmpty(additionalinfo))
				{
					sb.AppendLine($"Store procedure: {additionalinfo}");
				}

				return sb.ToString();
			}
		}


		public string StartDeployTable()
		{
			string result = db.Prompt("CREATE TABLE system_store FIELD LIST script_name, description, code, version");
			if (result.StartsWith("Error:")) return result;
			return "Ok.";
		}


		public string EvalDB(Dictionary<string, string> d, DB db)
		{
			string result;
			string version = "";

			if (d["version"] != "null")
			{
				version = $"{d["version"]}";
			}
			else
			{
				version = $" ORDER BY timestamp LIMIT 1";
			}

			if (d["development"] == "true")
			{
				result = db.Prompt($"SELECT * FROM system_store PARTITION KEY development WHERE id = '{d["py_db"]}->{version}'");
			}
			else
			{
				result = db.Prompt($"SELECT * FROM system_store PARTITION KEY production WHERE id = '{d["py_db"]}->{version}'");
			}

			if (result.StartsWith("Error:")) return result;
			if (result == "{}") return $"Error: The program could not be located: {d["py_db"]}";

			DataTable t = JsonConvert.DeserializeObject<DataTable>(result);
			return Eval(t.Rows[0]["code"].ToString(), d["message"].ToString(), d["py_db"] + ":" + d["version"]);
		}


		public string DeployFile(Dictionary<string, string> d, DB db)
		{
			string result = StartDeployTable();
			if (result.StartsWith("Error:")) return result;

			if (!File.Exists(d["py_deploy_file"])) return $"Error: File not exists: {d["py_deploy_file"]}";

			string code = File.ReadAllText(d["py_deploy_file"]);

			var paths = m_engine.GetSearchPaths();
			paths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + "py");
			paths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + "lib");

			m_engine.SetSearchPaths(paths);
			m_scope = m_engine.CreateScope();
			m_scope.SetVariable("db", db);
			this.db = db;


			Dictionary<string, object> process = new Dictionary<string, object>();
			process.Add("id", d["as"] + "->" + d["version"]);
			process.Add("script_name", d["as"]);
			process.Add("description", d["description"]);
			process.Add("code", code);
			process.Add("version", d["version"]);
			return db.Prompt($"INSERT INTO system_store PARTITION KEY development VALUES {JsonConvert.SerializeObject(process)}");

		}

		public string EvalFile(Dictionary<string, string> d, DB db)
		{

			string file;

			if (d["on_main_directory"] == "true")
			{
				file = db.BaseDirectory + "/" + d["py_file"];
			}
			else if (d["on_database"] == "true")
			{
				file = db.BaseDirectory + db.os_directory_separator + db.account + db.os_directory_separator + db.database + db.os_directory_separator + d["py_file"];
			}
			else if (d["on_table"] != "null")
			{
				file = db.BaseDirectory + db.os_directory_separator + db.account + db.os_directory_separator + db.database + db.os_directory_separator + d["on_table"] + db.os_directory_separator + d["py_file"];
			}
			else if (d["on_application_directory"] == "true")
			{
				file = AppDomain.CurrentDomain.BaseDirectory + "py" + db.os_directory_separator + d["py_file"];
			}
			else
			{
				file = d["py_file"];
			}

			if (!File.Exists(file)) return $"Error: File not exists: {file}";

			//ScriptScope m_scope = m_engine.CreateScope();
			this.db = db;

			try
			{
				ScriptSource source = m_engine.CreateScriptSourceFromFile(file);
				source.Execute(m_scope);
				//dynamic dynamic1 = m_scope.GetVariable("mainclass");
				//dynamic mainclass = m_engine.Operations.CreateInstance(dynamic1, db, d["message"]);
				//return mainclass.main();
				return "Ok.";
			}
			catch (Exception e)
			{
				string message;
				string messageType;
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				IList<Microsoft.Scripting.Runtime.DynamicStackFrame> l = m_engine.GetService<ExceptionOperations>().GetStackFrames(e);

				m_engine.GetService<ExceptionOperations>().GetExceptionMessage(e, out message, out messageType);

				sb.AppendLine("Error:" + d["py_file"] + ":");
				sb.AppendLine(message.Replace("Error: ", ""));

				foreach (Microsoft.Scripting.Runtime.DynamicStackFrame item in l)
				{
					sb.AppendLine($"Line: {item.GetFileLineNumber()}");

					if (item.GetFileName() != "<string>")
					{
						sb.AppendLine($"FileName: {item.GetFileName()}");
					}
				}

				return sb.ToString();
			}

		}


		public string DeployToProduction(Dictionary<string, string> d, DB db)
		{
			string result = StartDeployTable();
			if (result.StartsWith("Error:")) return result;

			result = db.Prompt($"SELECT * FROM system_store PARTITION KEY development WHERE script_name = '{d["production"]}' AND version = '{d["version"]}' ORDER BY timestamp DESC LIMIT 1");

			if (result.StartsWith("Error:")) return result;
			if (result == "{}") return $"Error: The process does not exist : {d["production"]}";

			DataRow r = JsonConvert.DeserializeObject<DataTable>(result).Rows[0];
			Dictionary<string, object> process = new Dictionary<string, object>();
			process.Add("id", r["script_name"].ToString());
			process.Add("script_name", r["script_name"].ToString());
			process.Add("description", r["description"].ToString());
			process.Add("code", r["code"].ToString());
			process.Add("version", r["version"].ToString());
			return db.Prompt($"INSERT INTO system_store PARTITION KEY production VALUES {JsonConvert.SerializeObject(process)}");

		}

		public string GetConsole()
		{
			return this.console_out;
		}


		public string SaveToFile(Dictionary<string, string> d, DB db)
		{

			try
			{
				string result = db.Prompt($"SELECT * FROM system_store PARTITION KEY development WHERE script_name = '{d["process"]}' AND version = '{d["version"]}' ORDER BY timestamp DESC LIMIT 1");

				if (result.StartsWith("Error:")) return result;
				if (result == "{}") return $"Error: The process does not exist : {d["production"]}";

				if (File.Exists(d["file"]))
				{
					File.Delete(d["file"]);
				}

				DataRow r = JsonConvert.DeserializeObject<DataTable>(result).Rows[0];

                File.WriteAllText(d["file"], r["code"].ToString());
				return "Ok.";

			}
			catch (System.Exception e)
			{
				return $"Error: Saving process to file: {d["file"]}: {e}";
			}

		}


	}
}