using System;
using System.Data;
using Accord.IO;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using Accord.MachineLearning.Bayes;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Accord.Statistics.Models.Regression;
using System.Diagnostics;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Controls;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;

namespace ChatDisentanglement
{
    class model
    {
        private LogisticRegression regression_model;
        private IterativeReweightedLeastSquares<LogisticRegression> learner;

        public model ()
        {

            learner = new IterativeReweightedLeastSquares<LogisticRegression>()
            {
                Tolerance = 0.00001,  // Let's set some convergence parameters
                MaxIterations = 1000,  // maximum number of iterations to perform
                Regularization = 0
            };
        }

        public static Tuple<double[][], int[]> load_data(string path, double time_threshold = 40000)
        {
            /*Simple Data read
            DataTable table = new ExcelReader(path).GetWorksheet("Sheet1");
            double[][] inputs = table.ToJagged<double>("time_diference", "same_author", "prev_mention_curr", "curr_mention_prev", "mention_same_person",
                                            "prev_starts_with_greeting", "curr_starts_with_greeting", "prev_is_long", "curr_is_long", "prev_thanks",
                                            "curr_thanks", "keywords_similarity");

            int[] outputs = table.Columns["same_thread"].ToArray<int>();

            ScatterplotBox.Show("Yin-Yang", inputs, outputs).Hold();

            return new Tuple<double[][], int[]>(inputs, outputs);
            */
           
            List<double[]> X = new List<double[]>();
            List<int> Y = new List<int>();
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    });


                    var table = result.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        double[] x = new double[16]; //12 is x feature dimension
                        int y;

                        x[0] = Int32.Parse(row["time_difference"].ToString());

                        if(x[0] > time_threshold) { continue; } //only consider pair of message within threshold

                        x[1] = Double.Parse(row["same_author"].ToString());
                        x[2] = Double.Parse(row["prev_mention_curr"].ToString());
                        x[3] = Double.Parse(row["curr_mention_prev"].ToString());
                        x[4] = Double.Parse(row["same_mention"].ToString());
                        x[5] = Double.Parse(row["prev_greeting"].ToString());
                        x[6] = Double.Parse(row["curr_greeting"].ToString());
                        x[7] = Double.Parse(row["prev_long"].ToString());
                        x[8] = Double.Parse(row["curr_long"].ToString());
                        x[9] = Double.Parse(row["prev_thanks"].ToString());
                        x[10] = Double.Parse(row["curr_thanks"].ToString());
                        x[11] = Double.Parse(row["prev_ques"].ToString());
                        x[12] = Double.Parse(row["curr_ques"].ToString());
                        x[13] = Double.Parse(row["prev_answer"].ToString());
                        x[14] = Double.Parse(row["curr_answer"].ToString());
                        x[15] = Double.Parse(row["keywords_similarity"].ToString());
                        y = Int32.Parse(row["same_thread"].ToString());

                        //Console.WriteLine(x[0] + " " + x[1] + " " + x[2] + " " + x[3] + " " + x[4] + " " + x[5] + " " + x[6] + " " + x[7] + " " + x[8] + " " + x[9] + " " + x[10] + " " + x[11] + " " + x[12] + " " + x[13] + " " + x[14] + " " + x[15]);
                        X.Add(x);
                        Y.Add(y);
                    }
                }
            }
            return new Tuple<double[][], int[]>(X.ToArray(), Y.ToArray());
        }

        public void train(string train_path, string test_path = "", double time_diff_threshold = 400)
        {
            Tuple<double[][], int[]> data = load_data(train_path);
            double[][] X_train = data.Item1;
            int[] y_train = data.Item2;

            double[][] X_test = null;
            int[] y_test = null;

            if (test_path == "")
            {
                //partition train data into test and train split
            }
            else
            {
                Tuple<double[][], int[]> test_data = load_data(test_path);
                X_test = test_data.Item1;
                y_test = test_data.Item2;
            }

            regression_model = learner.Learn(X_train, y_train); //train the classifier

            bool[] answers = regression_model.Decide(X_test);

            int len = answers.Length;

            int accur = 0;
            int total = len;

            for(int i = 0; i < len; i++)
            {
                int ans = answers[i] == true ? 1 : 0;
                Console.WriteLine("prediction: " + ans + " actual: " + y_test[i]);
                if (ans==y_test[i]) { accur++; }
            }

            Console.WriteLine("Accuracy: " + accur*100 / total + "%");
            regression_model.Save(@"C:/projects/ChatDisentanglement/save"); //Save the classifier
            Console.ReadLine();
        }
    }

    internal class Features
    {
        public double time_difference { get; set; }
        public double same_author { get; set; }
        public double prev_mention_curr { get; set; }
        public double curr_mention_prev { get; set; }
        public double mention_same_person { get; set; }
        public double prev_starts_with_greeting { get; set; }
        public double curr_starts_with_greeting { get; set; }
        public double prev_is_question { get; set; }
        public double curr_is_question { get; set; }
        public double prev_is_long { get; set; }
        public double curr_is_long { get; set; }
        public double prev_thanks { get; set; }
        public double curr_thanks { get; set; }
        public double keywords_overlap { get; set; }
        public int same_thread { get; set; }
    }
}
