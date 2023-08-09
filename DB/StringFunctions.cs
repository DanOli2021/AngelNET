using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Data;
using System.Security;
using System.IO;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Bibliography;
using System.Linq;
using DocumentFormat.OpenXml.Math;
using CsvHelper.Configuration;
using CsvHelper;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace AngelDBTools
{
    public static class StringFunctions
    {
        public static string[] SmartSplit(string line, char separator = ',', char stringDelimiter = '\'')
        {
            var inQuotes = false;
            var token = "";
            var lines = new List<string>();
            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (inQuotes) // process string in quotes, 
                {
                    if (ch == stringDelimiter)
                    {
                        if (i < line.Length - 1 && line[i + 1] == stringDelimiter)
                        {
                            i++;
                            token += stringDelimiter;
                        }
                        else inQuotes = false;
                    }
                    else token += ch;
                }
                else
                {
                    if (ch == stringDelimiter) inQuotes = true;
                    else if (ch == separator)
                    {
                        lines.Add(token.Trim());
                        token = "";
                    }
                    else token += ch;
                }
            }
            lines.Add(token.Trim());
            return lines.ToArray();
        }

        public static string SQLInsertStringValues(Dictionary<string, string> fields)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int fieldsLenght = fields.Count;

            sb.Append(" ( ");

            int n = 0;

            foreach (var item in fields)
            {
                ++n;

                if (n == fieldsLenght)
                {
                    sb.Append(item.Key);
                }
                else
                {
                    sb.Append(item.Key + ",");
                }

            }

            sb.Append(" ) VALUES ( ");

            n = 0;

            foreach (var item in fields)
            {
                ++n;

                if (n == fieldsLenght)
                {
                    sb.Append(item.Value);
                }
                else
                {
                    sb.Append(item.Value + ",");
                }

            }

            sb.Append(" ) ");

            return sb.ToString();

        }


        public static string SQLUpdateStringValues(Dictionary<string, string> fields)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int fieldsLenght = fields.Count;

            int n = 0;

            foreach (var item in fields)
            {
                ++n;
                if (n == fieldsLenght)
                {
                    sb.Append(item.Key + " = " + item.Value);
                }
                else
                {
                    sb.Append(item.Key + " = " + item.Value + ",");
                }
            }

            return sb.ToString();

        }


        public static bool IsStringValidPassword(string s)
        {

            if (s.Length < 8)
            {
                return false;
            }

            /*             char[] c = s.ToCharArray();

                        foreach (char item in c)
                        {
                            if (!IsPasswordChar(item))
                            {
                                return false;
                            }
                        }
             */

            return true;

        }


        public static bool IsStringAlphaNumberOrUndescore(string s)
        {

            char[] c = s.ToCharArray();

            foreach (char item in c)
            {
                if (!IsAlphaNumberOrUndescore(item))
                {
                    return false;
                }
            }

            return true;

        }

        public static bool IsStringAlphaOrNumber(string s)
        {

            char[] c = s.ToCharArray();

            foreach (char item in c)
            {
                if (!IsAlphaOrNumber(item))
                {
                    return false;
                }
            }

            return true;

        }



        public static bool IsStringNumber(string s)
        {

            char[] c = s.ToCharArray();

            foreach (char item in c)
            {
                if (!IsNumber(item))
                {
                    return false;
                }
            }

            return true;

        }

        public static bool IsStringFloatNumber(string s)
        {

            char[] c = s.ToCharArray();

            foreach (char item in c)
            {
                if (!IsFloatNumber(item))
                {
                    return false;
                }
            }

            return true;

        }


        public static string GetFirstElement(string element, char separator)
        {
            return element.Split(separator)[0];
        }

        public static bool IsNumber(char c)
        {

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            return false;

        }


        public static bool IsFloatNumber(char c)
        {

            if ((c >= '0' && c <= '9') || c == '.' || c == '-')
            {
                return true;
            }

            return false;

        }


        public static bool IsAlphaOrNumber(char c)
        {

            if (c >= 'a' && c <= 'z')
            {
                return true;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            return false;
        }


        public static bool IsAlphaNumberOrUndescore(char c)
        {

            if (c >= 'a' && c <= 'z')
            {
                return true;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            if (c == '_')
            {
                return true;
            }

            if (c == '.')
            {
                return true;
            }


            if (c == '@')
            {
                return true;
            }


            return false;

        }

        public static bool IsPasswordChar(char c)
        {

            if (c >= '!' && c <= '@')
            {
                return true;
            }

            if (c >= 'a' && c <= 'z')
            {
                return true;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            if (c == '_')
            {
                return true;
            }

            return false;

        }



        public static bool IsAlphaOrUndescore(char c)
        {

            if (c >= 'a' && c <= 'z')
            {
                return true;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }

            if (c == '_')
            {
                return true;
            }

            return false;

        }


        public static bool IsValidPath(string path)
        {
            return TryGetFullPath(path, out _);
        }

        public static bool TryGetFullPath(string path, out string result)
        {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool status = false;

            try
            {
                result = Path.GetFullPath(path);
                status = true;
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }

        static IEnumerable<string> WholeChunks(string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, chunkSize);
        }


        public static void ToCSV(this DataTable dtDataTable, string strFilePath, string delimiter = "\"")
        {

            if (dtDataTable is null)
            {
                return;
            }

            if (delimiter == "null")
            {
                delimiter = "\"";
            }

            StreamWriter sw = new StreamWriter(strFilePath, false, encoding: System.Text.Encoding.UTF8);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString()!;

                        if (dr[i].GetType() == typeof(string))
                        {
                            value = delimiter + value + delimiter;
                            sw.Write(value);
                        }
                        else if (dr[i].GetType() == typeof(decimal))
                        {
                            sw.Write(dr[i].ToString().Replace(",", "."));
                        }
                        else if (dr[i].GetType() == typeof(float))
                        {
                            sw.Write(dr[i].ToString().Replace(",", "."));
                        }
                        else if (dr[i].GetType() == typeof(double))
                        {
                            sw.Write(dr[i].ToString().Replace(",", "."));
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }

                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }

            sw.Close();

        }

        public static string ToCSVString(this DataTable dtDataTable)
        {
            System.Text.StringBuilder sw = new System.Text.StringBuilder();
            //headers    

            int n = 0;
            Dictionary<string, int> d = new Dictionary<string, int>();

            foreach (DataRow r in dtDataTable.Rows)
            {
                ++n;

                foreach (DataColumn c in dtDataTable.Columns)
                {
                    if (!d.ContainsKey(c.ColumnName))
                    {
                        if (r[c.ColumnName].GetType() == typeof(string))
                        {
                            d.Add(c.ColumnName, c.ColumnName.Length + 2);
                        }
                        else
                        {
                            d.Add(c.ColumnName, c.ColumnName.Length);
                        }

                    }

                    if (r[c.ColumnName].ToString().Length > d[c.ColumnName])
                    {
                        d[c.ColumnName] = r[c.ColumnName].ToString().Length;
                    }

                }

                if (n == 20)
                {
                    break;
                }
            }

            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Append(dtDataTable.Columns[i].ColumnName.PadRight(d[dtDataTable.Columns[i].ColumnName]));
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Append(",");
                }
            }

            sw.AppendLine();

            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {

                        string value = dr[i].ToString()!;

                        if (dr[i].GetType().Name == "String")
                        {
                            value = String.Format("\"{0}\"", value).PadRight(d[dtDataTable.Columns[i].ColumnName]);
                            sw.Append(value);
                        }
                        else
                        {
                            sw.Append(dr[i].ToString().PadRight(d[dtDataTable.Columns[i].ColumnName]));
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Append(",");
                    }
                }
                sw.AppendLine();
            }

            return sw.ToString();

        }


        public static string SaveEncriptedFile(string file_name, string ToSave, string password)
        {
            try
            {
                File.WriteAllText(file_name, CryptoString.Encrypt(ToSave, "hbjklios", password));
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static string RestoreEncriptedFile(string file_name, string password)
        {
            try
            {
                return CryptoString.Decrypt(File.ReadAllText(file_name), "hbjklios", password);
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }



        public static string SaveEncriptedConfig(string file_name, MyEnvironment ToSave)
        {
            try
            {
                File.WriteAllText(file_name, CryptoString.Encrypt(System.Text.Json.JsonSerializer.Serialize(ToSave), "hbjklios", "iuybncsa"));
                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static MyEnvironment RestoreEncriptedConfig(string file_name)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<MyEnvironment>(CryptoString.Decrypt(File.ReadAllText(file_name), "hbjklios", "iuybncsa"));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string FormatStringFromPatter(string data, string pattern, bool AcceptEmpty, Dictionary<string, string> d)
        {

            data += " ";

            while (true)
            {
                int pos = data.IndexOf(pattern, StringComparison.Ordinal);
                if (pos < 0) break;

                string search_pattern = "";

                for (int n = (pos + 1); n < data.Length; ++n)
                {
                    string char_key = data.Substring(n, 1);

                    if (char_key == " ")
                    {
                        if (d.ContainsKey(search_pattern))
                        {

                            if (string.IsNullOrEmpty(d[search_pattern])) return data;
                            data = data.Replace(pattern + search_pattern, d[search_pattern]);
                        }

                        search_pattern = "";

                    }
                    else
                    {
                        search_pattern += char_key;
                    }

                }
            }

            return data;

        }

        public static string GetAccount(string ContainerName)
        {
            ContainerName = ContainerName.Trim().ToLower().Replace("&", "0-0");

            var nPos = ContainerName.ToLower().IndexOf("ventas-");

            if (nPos == -1)
                nPos = 0;
            else
                nPos = 7;

            if (nPos == 0)
            {
                nPos = ContainerName.ToLower().IndexOf("ventaspendientes-");

                if (nPos == -1)
                    nPos = 0;
                else
                    nPos = 17;
            }

            if (nPos == 0)
            {
                nPos = ContainerName.ToLower().IndexOf("pedidos-");

                if (nPos == -1)
                    nPos = 0;
                else
                    nPos = 8;
            }

            if (nPos == 0)
            {
                nPos = ContainerName.ToLower().IndexOf("cobranza-");

                if (nPos == -1)
                    nPos = 0;
                else
                    nPos = 9;
            }

            if (nPos == 0)
            {
                nPos = ContainerName.ToLower().IndexOf("skus-");
                if (nPos == -1)
                    nPos = 0;
                else
                    nPos = 5;
            }

            if (nPos == 0)
            {
                nPos = ContainerName.ToLower().IndexOf("tabla-");

                if (nPos == -1)
                    nPos = 0;
                else
                    nPos = 6;
            }

            string storageAccount = "";

            switch (ContainerName.Substring(nPos, 1).ToUpper())
            {
                case "0":
                case "A":
                case "B":
                case "C":
                    {
                        storageAccount = "ventas1";
                        break;
                    }

                case "1":
                case "D":
                case "E":
                case "F":
                    {
                        storageAccount = "ventas2";
                        break;
                    }

                case "G":
                case "H":
                case "I":
                    {
                        storageAccount = "ventas3";
                        break;
                    }

                case "J":
                case "K":
                case "L":
                    {
                        storageAccount = "ventas4";
                        break;
                    }

                case "M":
                case "N":
                case "O":
                    {
                        storageAccount = "ventas5";
                        break;
                    }

                case "P":
                case "Q":
                case "R":
                    {
                        storageAccount = "ventas6";
                        break;
                    }

                case "S":
                case "T":
                case "U":
                    {
                        storageAccount = "ventas7";
                        break;
                    }

                case "V":
                case "W":
                case "X":
                    {
                        storageAccount = "ventas8";
                        break;
                    }

                case "Y":
                case "Z":
                    {
                        storageAccount = "ventas9";
                        break;
                    }

            }

            return storageAccount;

        }


        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        public static string getMd5Hash(string input)
        {
            // Create a new instance of the MD5 object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            int i;
            for (i = 0; i <= data.Length - 1; i++)
                sBuilder.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }


        public static string ReadCSV(string file, bool first_as_header, string value_separator = ",", string columns_as_number = "")
        {
            try
            {

                if (columns_as_number == "null") columns_as_number = "";
                if (value_separator == "null") value_separator = ",";

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = first_as_header,
                    Delimiter = value_separator,
                };

                DataTable dt = null;

                using (var reader = new StreamReader(file))

                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Do any configuration to `CsvReader` before creating CsvDataReader.
                    using (var dr = new CsvDataReader(csv))
                    {
                        dt = new DataTable();

                        string[] columns_as_number_array = columns_as_number.Split(",");

                        foreach (string s in columns_as_number_array)
                        {
                            if (!string.IsNullOrEmpty(s))
                            {
                                dt.Columns.Add(s, typeof(decimal));
                            }
                        }

                        dt.Load(dr);
                    }
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(dt, Newtonsoft.Json.Formatting.Indented);


            }
            catch (Exception e)
            {
                return $"Error: ReadCSV: {e}";
            }
        }

        static public string ConvertStringToDbColumn(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in normalized)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    // Verifica y convierte caracteres especiales a '_'
                    if (Char.IsLetterOrDigit(c) || c == '_')
                    {
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        stringBuilder.Append('_');
                    }
                }
            }

            string noAccent = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            string validDbColumnName = noAccent.Replace(' ', '_').ToLower();

            return validDbColumnName;
        }


        public static string ExtractContentInFirstParentheses(string input)
        {
            // La expresión regular busca cualquier cosa entre el primer par de paréntesis
            var regex = new Regex(@"\(([^)]*)\)");
            var match = regex.Match(input);

            // Si encontramos un match, lo devolvemos. Si no, devolvemos null
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string ConvertJsonToHtmlTable(string json)
        {
            // Deserializar el JSON a un DataTable
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);

            StringBuilder htmlTable = new StringBuilder();
            htmlTable.AppendLine("<table>");

            // Encabezados de la tabla
            htmlTable.AppendLine("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                htmlTable.AppendLine($"<th>{column.ColumnName}</th>");
            }
            htmlTable.AppendLine("</tr>");

            // Filas de la tabla
            foreach (DataRow row in dt.Rows)
            {
                htmlTable.AppendLine("<tr>");
                foreach (DataColumn column in dt.Columns)
                {
                    htmlTable.AppendLine($"<td>{row[column].ToString()}</td>");
                }
                htmlTable.AppendLine("</tr>");
            }

            htmlTable.AppendLine("</table>");

            return htmlTable.ToString();
        }


    }



}