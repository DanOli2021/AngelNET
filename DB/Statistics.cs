using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using static AngelDB.StatisticsAnalisis;

namespace AngelDB
{

    public static class DBStatistics 
    {


        public static string StatisticsCommand(AngelDB.DB db, string command) 
        {

            DbLanguage language = new DbLanguage(db);
            
            Dictionary<string, string> d = new Dictionary<string, string>();
            language.SetCommands(StatisticsCommands.Commands());
            d = language.Interpreter(command);

            if (d == null)
            {
               return language.errorString;
            }

            if (d.Count == 0)
            {
               return "Error: not command found " + command; ;
            }
            string commandkey = d.First().Key;

            switch (commandkey)
            {
                case "analysis":
                    return Analysis(db, d);
                case "confidence_interval_of":
                    return ConfidenceLevel(db, d);
                case "confidence_interval_between":
                    return ConfindenceLevelCompareTowMeans(db, d);
                case "proportion_compare":
                    return ConfindenceLevelCompareTowProportions(db, d);
                case "show_analysis":
                    return ShowAnalysis(db, d);
                default:
                    return "Error: not command found " + command;
            }

        }

        public static string Analysis( AngelDB.DB db, Dictionary<string,string> d ) 
        {

            try
            {
                StatisticsAnalisis st = new StatisticsAnalisis(db);
                string result = st.Data(d["from"], d["variable"]);

                if (!result.StartsWith("Error:"))
                {

                    result = st.Analysis();
                    
                    if (db.statistics.ContainsKey(d["analysis"]))
                    {
                        db.statistics[d["analysis"]] = st;
                    }
                    else
                    {
                        db.statistics.Add(d["analysis"], st);
                    }
                }

                return result;

            }
            catch (Exception e)
            {
                return "Error: analysis: " + e.Message;
            }

        }


        public static string ShowAnalysis(AngelDB.DB db, Dictionary<string, string> d) 
        {
            if (!db.statistics.ContainsKey(d["show_analysis"])) 
            {
                return $"Error: Analysis does not exist {d["show_analisis"]}";
            }

            return JsonConvert.SerializeObject(db.statistics[d["show_analysis"]].description.Result, Formatting.Indented);

        }

        public static string ConfidenceLevel(AngelDB.DB db, Dictionary<string, string> d) 
        {

            string result = "";

            if (db.statistics.ContainsKey(d["confidence_interval_of"]))
            {
                if (d["proportion"] == "null")
                {
                    result = db.statistics[d["confidence_interval_of"]].ConfidenceInterval(d["confidence_level"]);
                }
                else 
                {
                    result = db.statistics[d["confidence_interval_of"]].ConfidenceIntervalProportion(d["confidence_level"], d["proportion"]);
                }
                
            }
            else
            {
                result = "Error: statistics not found";
            }

            return result;

        }


        public static string ConfindenceLevelCompareTowMeans(AngelDB.DB db, Dictionary<string, string> d) 
        {

            if (!db.statistics.ContainsKey(d["confidence_interval_between"]))
            {
                return $"Error: statistics not found {d["confidence_interval_between"]}";
            }

            if (!db.statistics.ContainsKey(d["and"]))
            {
                return $"Error: statistics not found {d["and"]}";
            }

            Descriptive x1 = db.statistics[d["confidence_interval_between"]].description;
            Descriptive x2 = db.statistics[d["and"]].description;

            double mean1 = x1.Result.Mean;
            double mean2 = x2.Result.Mean;

            DataRow[] rows;
            string confidence = d["confidence_level"];

            if (x1.Result.Count <= 30)
            {
                rows = db.tTables.Select($"GL = '{x1.Result.Count}'");
            }
            else
            {
                rows = db.tTables.Select($"GL = 'z'");
            }

            if (rows.Length == 0)
            {
                return $"Error: ConfidenceInterval: Confindence interval not found {confidence}.";
            }
                        
            // z = ( valor_buscado - media ) / desviación standard
            double confidence_factor = double.Parse(rows[0]["P" + confidence].ToString());

            double standard_error = Math.Sqrt((x1.Result.Variance / x1.Result.Count) + (x2.Result.Variance / x2.Result.Count));
            double mean_diference = x1.Result.Mean - x2.Result.Mean;
            
            Dictionary<string, double> limits = new Dictionary<string, double>();
            
            double upper_calculus = (confidence_factor * standard_error) + mean_diference;
            double lower_calculus = (confidence_factor * standard_error * -1) + mean_diference;

            limits.Add("z", confidence_factor);
            limits.Add("Lower", lower_calculus);
            limits.Add("Upper", upper_calculus);

            return JsonConvert.SerializeObject(limits, Formatting.Indented);
        }

        public static string ConfindenceLevelCompareTowProportions(AngelDB.DB db, Dictionary<string, string> d)
        {

            if (!db.statistics.ContainsKey(d["proportion_compare"]))
            {
                return $"Error: statistics not found {d["proportion_compare"]}";
            }

            if (!db.statistics.ContainsKey(d["between"]))
            {
                return $"Error: statistics not found {d["between"]}";
            }

            if (!db.statistics.ContainsKey(d["and"]))
            {
                return $"Error: statistics not found {d["and"]}";
            }

            Descriptive main = db.statistics[d["proportion_compare"]].description;
            Descriptive x1 = db.statistics[d["between"]].description;
            Descriptive x2 = db.statistics[d["and"]].description;

            double main_count = main.Result.Count;           
            double p1 = x1.Result.Count / main_count;
            double p2 = x2.Result.Count / main_count;

            DataRow[] rows;
            string confidence = d["confidence_level"];

            if (x1.Result.Count <= 30)
            {
                rows = db.tTables.Select($"GL = '{x1.Result.Count}'");
            }
            else
            {
                rows = db.tTables.Select($"GL = 'z'");
            }

            if (rows.Length == 0)
            {
                return $"Error: ConfidenceInterval: Confindence interval not found {confidence}.";
            }

            // z = ( valor_buscado - media ) / desviación standard
            double confidence_factor = double.Parse(rows[0]["P" + confidence].ToString());

            double standard_error = Math.Sqrt(( (p1 * (1 - p1) ) / x1.Result.Count) + ((p2 * (1 - p2)) / x2.Result.Count));
            Dictionary<string, double> limits = new Dictionary<string, double>();

            double upper_calculus = (confidence_factor * standard_error) + (p1 - p2);
            double lower_calculus = (confidence_factor * standard_error) - (p1 - p2);

            limits.Add("z", confidence_factor);
            limits.Add("Lower", lower_calculus);
            limits.Add("Upper", upper_calculus);

            return JsonConvert.SerializeObject(limits, Formatting.Indented);
        }

    }

    public class StatisticsAnalisis
    {

        public Descriptive description = null;
        private List<double> numeric_data = new List<double>();
        public AngelDB.DB db;

        public StatisticsAnalisis(AngelDB.DB db)
        {
            this.db = db;

            if (db.tTables is null)
            {
                if (System.IO.File.Exists(Environment.CurrentDirectory + "/tables.json"))
                {
                    this.db.tTables = JsonConvert.DeserializeObject<DataTable>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/tables.json"));
                }
            }

            if (db.zTables is null)
            {
                if (System.IO.File.Exists(Environment.CurrentDirectory + "/TablaZ.jSon"))
                {
                    this.db.zTables = JsonConvert.DeserializeObject<DataTable>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/TablaZ.jSon"));
                }
            }

        }

        public string Data(string jSonData, string AnalyzeColumn)
        {
            try
            {
                DataTable data = JsonConvert.DeserializeObject<DataTable>(jSonData);

                foreach (DataRow item in data.Rows)
                {
                    if (AnalyzeColumn != "null")
                    {
                        numeric_data.Add(Convert.ToDouble(item[AnalyzeColumn]));
                    }
                    else
                    {
                        numeric_data.Add(Convert.ToDouble(item[0]));
                    }
                }

                return "Ok.";
            }
            catch (Exception e)
            {
                return $"Error: Data: {e.ToString()}";
            }
        }

        public string Analysis()
        {
            try
            {
                description = new Descriptive(this.numeric_data.ToArray());
                description.Analyze();
                return JsonConvert.SerializeObject(description.Result, Formatting.Indented);
            }
            catch (Exception e)
            {
                return $"Error: Analisys: {e.ToString()}";
            }
        }


        public string ConfidenceInterval(string confidence)
        {
            try
            {

                if (this.description is null)
                {
                    return "Error: ConfidenceInterval: Analyze the data first use method Analisys().";
                }

                if (this.numeric_data.Count == 0)
                {
                    return "Error: ConfidenceInterval: No data to analyze.";
                }

                DataRow[] rows;

                if (this.description.Result.Count <= 30)
                {
                    rows = this.db.tTables.Select($"GL = '{this.description.Result.Count}'");
                }
                else
                {
                    rows = this.db.tTables.Select($"GL = 'z'");
                }

                if (rows.Length == 0)
                {
                    return $"Error: ConfidenceInterval: Confindence interval not found {confidence}.";
                }


                // z = ( valor_buscado - media ) / desviación standard
                double confidence_factor = double.Parse(rows[0]["P" + confidence].ToString());
                Dictionary<string, double> limits = new Dictionary<string, double>();

                double standard_error = description.Result.StdDev / Math.Sqrt(description.Result.Count);

                double upper_calculus = (confidence_factor * standard_error) + description.Result.Mean;
                double lower_calculus = (confidence_factor * standard_error * -1) + description.Result.Mean;

                limits.Add("z", confidence_factor);
                limits.Add("Mean", description.Result.Mean);
                limits.Add("StdDev", description.Result.StdDev);
                limits.Add("Lower", lower_calculus);
                limits.Add("Upper", upper_calculus);

                return JsonConvert.SerializeObject(limits, Formatting.Indented);
            }
            catch (Exception e)
            {
                return $"Error: ConfidenceInterval: {e.ToString()}";
            }
        }


        public string ConfidenceIntervalProportion(string confidence, string sample)
        {
            try
            {

                if (this.description is null)
                {
                    return "Error: ConfidenceInterval: Analyze the data first use method Analisys().";
                }

                if (this.numeric_data.Count == 0)
                {
                    return "Error: ConfidenceInterval: No data to analyze.";
                }

                DataRow[] rows;

                if (this.description.Result.Count <= 30)
                {
                    rows = this.db.tTables.Select($"GL = '{this.description.Result.Count}'");
                }
                else
                {
                    rows = this.db.tTables.Select($"GL = 'z'");
                }

                if (rows.Length == 0)
                {
                    return $"Error: ConfidenceInterval: Confindence interval not found {confidence}.";
                }


                // z = ( valor_buscado - media ) / desviación standard
                double confidence_factor = double.Parse(rows[0]["P" + confidence].ToString());
                Dictionary<string, double> limits = new Dictionary<string, double>();

                double nsample = double.Parse(sample) / description.Result.Count;
                double proportion = Math.Sqrt((nsample * (1 - nsample)) / description.Result.Count);

                double upper_calculus = nsample + (confidence_factor * proportion);
                double lower_calculus = nsample - (confidence_factor * proportion);

                limits.Add("z", confidence_factor);
                limits.Add("Lower", lower_calculus);
                limits.Add("Upper", upper_calculus);

                return JsonConvert.SerializeObject(limits, Formatting.Indented);
            }
            catch (Exception e)
            {
                return $"Error: ConfidenceInterval: {e.ToString()}";
            }
        }


        public string ZTable(double value)
        {

            try
            {

                string string_value = value.ToString().PadRight(6, '0');
                string z = value.ToString().Substring(0, 3);
                string zColumn = value.ToString().Substring(4);

                DataRow[] rows = this.db.zTables.Select($"z = '{z}'");

                if (rows.Length == 0)
                {
                    return $"Error: ZTable: Z value not found {value}.";
                }

                return rows[0]["P"].ToString();
            }
            catch (Exception e)
            {
                return $"Error: ZTable: {e}";
            }
        }


        /// <summary>
        /// The result class the holds the analysis results
        /// </summary>
        public class Statistics
        {
            // sortedData is used to calculate percentiles
            internal double[] sortedData;

            /// <summary>
            /// DescriptiveResult default constructor
            /// </summary>
            public Statistics() { }

            /// <summary>
            /// Count
            /// </summary>
            public uint Count;
            /// <summary>
            /// Sum
            /// </summary>
            public double Sum;
            /// <summary>
            /// Arithmatic mean
            /// </summary>
            public double Mean;
            /// <summary>
            /// Geometric mean
            /// </summary>
            public double GeometricMean;
            /// <summary>
            /// Harmonic mean
            /// </summary>
            public double HarmonicMean;
            /// <summary>
            /// Minimum value
            /// </summary>
            public double Min;
            /// <summary>
            /// Maximum value
            /// </summary>
            public double Max;
            /// <summary>
            /// The range of the values
            /// </summary>
            public double Range;
            /// <summary>
            /// Sample variance
            /// </summary>
            public double Variance;
            /// <summary>
            /// Sample standard deviation
            /// </summary>
            public double StdDev;
            /// <summary>
            /// Skewness of the data distribution
            /// </summary>
            public double Skewness;
            /// <summary>
            /// Kurtosis of the data distribution
            /// </summary>
            public double Kurtosis;
            /// <summary>
            /// Interquartile range
            /// </summary>
            public double IQR;
            /// <summary>
            /// Median, or second quartile, or at 50 percentile
            /// </summary>
            public double Median;
            /// <summary>
            /// First quartile, at 25 percentile
            /// </summary>
            public double FirstQuartile;
            /// <summary>
            /// Third quartile, at 75 percentile
            /// </summary>
            public double ThirdQuartile;
            /// <summary>
            /// Sum of Error
            /// </summary>
            internal double SumOfError;
            /// <summary>
            /// The sum of the squares of errors
            /// </summary>
            internal double SumOfErrorSquare;
            /// <summary>
            /// Percentile
            /// </summary>
            /// <param name="percent">Pecentile, between 0 to 100</param>
            /// <returns>Percentile</returns>
            /// 

            public double Percentile(double percent)
            {
                return Descriptive.percentile(sortedData, percent);
            }
        } // end of class DescriptiveResult


        /// <summary>
        /// Descriptive class
        /// </summary>
        public class Descriptive
        {
            private double[] data;
            private double[] sortedData;

            /// <summary>
            /// Descriptive results
            /// </summary>
            public Statistics Result = new Statistics();

            #region Constructors
            /// <summary>
            /// Descriptive analysis default constructor
            /// </summary>
            public Descriptive() { } // default empty constructor

            /// <summary>
            /// Descriptive analysis constructor
            /// </summary>
            /// <param name="dataVariable">Data array</param>
            public Descriptive(double[] dataVariable)
            {
                data = dataVariable;
            }
            #endregion //  Constructors

            /// <summary>
            /// Run the analysis to obtain descriptive information of the data
            /// </summary>
            public void Analyze()
            {

                // initializations
                Result.Count = 0;
                Result.Min = Result.Max = Result.Range = Result.Mean =
                Result.Sum = Result.StdDev = Result.Variance = 0.0d;

                double sumOfSquare = 0.0d;
                double sumOfESquare = 0.0d; // must initialize

                double[] squares = new double[data.Length];
                double cumProduct = 1.0d; // to calculate geometric mean
                double cumReciprocal = 0.0d; // to calculate harmonic mean

                // First iteration
                for (int i = 0; i < data.Length; i++)
                {
                    if (i == 0) // first data point
                    {
                        Result.Min = data[i];
                        Result.Max = data[i];
                        Result.Mean = data[i];
                        Result.Range = 0.0d;
                    }
                    else
                    { // not the first data point
                        if (data[i] < Result.Min) Result.Min = data[i];
                        if (data[i] > Result.Max) Result.Max = data[i];
                    }
                    Result.Sum += data[i];
                    squares[i] = Math.Pow(data[i], 2); //TODO: may not be necessary
                    sumOfSquare += squares[i];

                    cumProduct *= data[i];
                    cumReciprocal += 1.0d / data[i];
                }



                Result.Count = (uint)data.Length;
                double n = (double)Result.Count; // use a shorter variable in double type
                Result.Mean = Result.Sum / n;
                Result.GeometricMean = Math.Pow(cumProduct, 1.0 / n);
                Result.HarmonicMean = 1.0d / (cumReciprocal / n); // see http://mathworld.wolfram.com/HarmonicMean.html
                Result.Range = Result.Max - Result.Min;

                // second loop, calculate Stdev, sum of errors
                //double[] eSquares = new double[data.Length];
                double m1 = 0.0d;
                double m2 = 0.0d;
                double m3 = 0.0d; // for skewness calculation
                double m4 = 0.0d; // for kurtosis calculation
                                  // for skewness
                for (int i = 0; i < data.Length; i++)
                {
                    double m = data[i] - Result.Mean;
                    double mPow2 = m * m;
                    double mPow3 = mPow2 * m;
                    double mPow4 = mPow3 * m;

                    m1 += Math.Abs(m);

                    m2 += mPow2;

                    // calculate skewness
                    m3 += mPow3;

                    // calculate skewness
                    m4 += mPow4;

                }

                Result.SumOfError = m1;
                Result.SumOfErrorSquare = m2; // Added for Excel function DEVSQ
                sumOfESquare = m2;

                // var and standard deviation
                Result.Variance = sumOfESquare / ((double)Result.Count - 1);
                Result.StdDev = Math.Sqrt(Result.Variance);

                // using Excel approach
                double skewCum = 0.0d; // the cum part of SKEW formula
                for (int i = 0; i < data.Length; i++)
                {
                    skewCum += Math.Pow((data[i] - Result.Mean) / Result.StdDev, 3);
                }
                Result.Skewness = n / (n - 1) / (n - 2) * skewCum;

                // kurtosis: see http://en.wikipedia.org/wiki/Kurtosis (heading: Sample Kurtosis)
                double m2_2 = Math.Pow(sumOfESquare, 2);
                Result.Kurtosis = ((n + 1) * n * (n - 1)) / ((n - 2) * (n - 3)) *
                    (m4 / m2_2) -
                    3 * Math.Pow(n - 1, 2) / ((n - 2) * (n - 3)); // second last formula for G2

                // calculate quartiles
                sortedData = new double[data.Length];
                data.CopyTo(sortedData, 0);
                Array.Sort(sortedData);

                // copy the sorted data to result object so that
                // user can calculate percentile easily
                Result.sortedData = new double[data.Length];
                sortedData.CopyTo(Result.sortedData, 0);

                Result.FirstQuartile = percentile(sortedData, 25);
                Result.ThirdQuartile = percentile(sortedData, 75);
                Result.Median = percentile(sortedData, 50);
                Result.IQR = percentile(sortedData, 75) -
                percentile(sortedData, 25);

            } // end of method Analyze


            /// <summary>
            /// Calculate percentile of a sorted data set
            /// </summary>
            /// <param name="sortedData"></param>
            /// <param name="p"></param>
            /// <returns></returns>
            internal static double percentile(double[] sortedData, double p)
            {
                // algo derived from Aczel pg 15 bottom
                if (p >= 100.0d) return sortedData[sortedData.Length - 1];

                double position = (double)(sortedData.Length + 1) * p / 100.0;
                double leftNumber = 0.0d, rightNumber = 0.0d;

                double n = p / 100.0d * (sortedData.Length - 1) + 1.0d;

                if (position >= 1)
                {
                    leftNumber = sortedData[(int)System.Math.Floor(n) - 1];
                    rightNumber = sortedData[(int)System.Math.Floor(n)];
                }
                else
                {
                    leftNumber = sortedData[0]; // first data
                    rightNumber = sortedData[1]; // first data
                }

                if (leftNumber == rightNumber)
                    return leftNumber;
                else
                {
                    double part = n - System.Math.Floor(n);
                    return leftNumber + part * (rightNumber - leftNumber);
                }
            } // end of internal function percentile

        } // end of class Descriptive        

    }

    public class Bayes
    {

        public Dictionary<string, BayesNode> nodes = new Dictionary<string, BayesNode>();
        public decimal TotalProbability { get; set; }
        public string problem;

        public Bayes(string problem)
        {
            this.problem = problem;
        }

        public string AddBayesNode(string NodeName, double probality, double probability_dependency)
        {

            if (probality >= 1)
            {
                return "Error: The value of probality must always be less than 1.";
            }

            if (probability_dependency >= 1)
            {
                return "Error: The value of probability_dependency must always be less than 1.";
            }

            if (string.IsNullOrEmpty(NodeName))
            {
                return "Error: The NodeNameA parameter must not be an empty string";
            }

            BayesNode node = new BayesNode();

            node.NodeName = NodeName;
            node.Probality = probality;
            node.ProbalityDependency = probability_dependency;

            if (!nodes.ContainsKey(NodeName))
            {
                this.nodes.Add(NodeName, node);
            }
            else
            {
                this.nodes[NodeName] = node;
            }

            return "Ok.";
        }


        public double CalculationTotalProbability()
        {

            double total = 0;

            foreach (string key in nodes.Keys)
            {
                total += nodes[key].Probality * nodes[key].ProbalityDependency;
            }

            return total;
        }


        public string CalculationTotalProbabilityDependency()
        {

            double total = CalculationTotalProbability();
            Dictionary<string, object> data = new Dictionary<string, object>();

            foreach (string key in this.nodes.Keys)
            {
                double dependency = (nodes[key].Probality * nodes[key].ProbalityDependency) / total;
                nodes[key].ProbalityResult = dependency;
            }

            return "Ok.";

        }

    }


    public class BayesNode
    {
        public string NodeName { get; set; }
        public double Probality { get; set; }
        public double ProbalityDependency { get; set; }
        public double ProbalityResult { get; set; }
    }

}




