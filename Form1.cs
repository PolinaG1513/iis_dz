using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Data;
using System.Linq;
using MathNet.Numerics.IntegralTransforms;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FftSharp;

namespace ДЗ1
{
    public partial class Form1 : Form
    {
        private string selectedElement;
        public string fileName_u;
        public string fileName_i;
        public Form1()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName_u = openFileDialog.FileName;
                ReadData(fileName_u);
                labelName.Text = $"{fileName_u}";
            } 
        }

        public void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName_i = openFileDialog.FileName;
                ReadData(fileName_i);
                labelNew.Text = $"{fileName_i}";
            }
        }

        public void ReadData(string fileName)
        {
            try
            {
                double duration = getStringArray(fileName).Length / 10;
                TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                string formattedTime = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                labelDuration.Text = $"Продолжительность: {formattedTime}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string[] getOldLines(string fileName)
        {
            string[] oldLines = File.ReadAllLines(fileName);
            return oldLines;
        }



        public string[] getStringArray(string fileName)
        {
            string[] oldLines = getOldLines(fileName);

            string[] stringLines = new string[oldLines.Length / 2 + (oldLines.Length % 2 == 0 ? 0 : 1)];
            for (int i = 0; i < oldLines.Length; i += 2)
            {
                stringLines[i / 2] = oldLines[i];
            }
            return stringLines;
        }

        public double[] getLastDoubleArray(string[] stringLines)
        {
            double[] lastDoubleArray = new double[80];

            string[] values = stringLines[333].Split('\t');

            for (int i = 0; i < lastDoubleArray.Length; i++)
            {
                lastDoubleArray[i] = Convert.ToDouble(values[i]);
            }


            return lastDoubleArray;
        }



        private void comboItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedElement = comboItem.SelectedItem.ToString();
        }

        private void buttonPlot_Click(object sender, EventArgs e)
        {
            string fileName = fileName_u;
            if (string.IsNullOrEmpty(selectedElement))
            {
                MessageBox.Show("Выберите элемент в ComboBox.");
                return;
            }

            string filePath1 = labelName.Text;
            string filePath2 = labelNew.Text;
            if (!File.Exists(filePath1))
            {
                MessageBox.Show("Файл не найден.");
                return;
            }

            string[] oldLines = getOldLines(fileName);
            string[] lines = new string[oldLines.Length / 2 + (oldLines.Length % 2 == 0 ? 0 : 1)];

            try
            {
                if (selectedElement == "Сигналы Ib(t)")
                {
                    fileName = fileName_i;
                    double[] signalData_u = getLastDoubleArray(getStringArray(fileName));
                    PlotSignal(fileName, signalData_u);
                }
                else if (selectedElement == "Сигналы Ub(t)")
                {
                    
                    fileName = fileName_u;
                    double[] signalData_i = getLastDoubleArray(getStringArray(fileName));
                    PlotSignal(fileName, signalData_i);
                }
                else if (selectedElement == "Спектр сигнала")
                {
                    fileName = fileName_i;
                    PlotSignal_new(fileName);
                }
                else if (selectedElement == "График p(t)")
                {
                    PlotPSignal(fileName_i, fileName_u);
                }
                else if (selectedElement == "Кривые P(t), Q(t), S(t)")
                {
                    PlotSignal_QS(fileName_i, fileName_u);
                }
                else if (selectedElement == "Гармоники(амплитуда)")
                {
                    PlotSignal_amplitude(fileName_i);
                }
                else if (selectedElement == "Гармоники(фаза)")
                {
                    PlotSignal_phase(fileName_i);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }

        private void PlotSignal(string fileName, double[] signalData)
        {      
            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Line;

            double secondsPer = 0.1;
            for (int i = 0; i < signalData.Length; i++)
            {
                series.Points.AddXY(i * secondsPer, signalData[i]);
            }

            chart1.Series.Add(series);

            if (fileName == fileName_u)
            {
                chart1.ChartAreas[0].AxisX.Title = "Время, с";
                chart1.ChartAreas[0].AxisY.Title = "Напряжение, В";
            }
            else if (fileName == fileName_i)
            {
                chart1.ChartAreas[0].AxisX.Title = "Время, с";
                chart1.ChartAreas[0].AxisY.Title = "Сила тока, А";
            }

           
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;


             chart1.ChartAreas[0].AxisX.Minimum = 0; // Минимальное значение оси X
             chart1.ChartAreas[0].AxisX.Maximum = signalData.Length * secondsPer; // Максимальное значение оси X
             chart1.ChartAreas[0].AxisY.Minimum = signalData.Min(); // Минимальное значение оси Y (минимальное значение данных)
             chart1.ChartAreas[0].AxisY.Maximum = signalData.Max(); // Максимальное значение оси Y (максимальное значение данных
        }

        private void PlotSignal_new(string fileName)
        {
            double[] signalData = getLastDoubleArray(getStringArray(fileName));

            int N = signalData.Length;
            double delta_t = 0.0025;
            double delta_f = 0.015625 / delta_t;
            double[] freq = new double[N];
            System.Numerics.Complex[] result = new System.Numerics.Complex[N];
            List<double> result_double = new List<double>();

            double[] new_result = new double[64];
            for (int i = 0; i < new_result.Length; i++)
            {
                new_result[i] = signalData[i];
            }

            for (int k = 0; k < 64; k++)
            {
                for (int n = 0; n < 64; n++)
                {
                    double arg = -2 * Math.PI * k * n / 64;
                    var complex = new System.Numerics.Complex(Math.Cos(-arg), Math.Sin(arg));
                    result[k] += signalData[n] * complex;
                }
            }

            for (int i = 0; i < N; i++)
            {
                freq[i] = i * delta_f; //x
            }

            System.Numerics.Complex[] fft = FftSharp.FFT.Forward(new_result);
            List<double> mnim = new List<double>();
            foreach (var complexNumber in fft)
            {
                double im = complexNumber.Imaginary;
                double rel = complexNumber.Real;
                double absmnim = Math.Sqrt(Math.Pow(im, 2) + Math.Pow(rel, 2));
                mnim.Add(absmnim);
            }

            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Title = "Время, с";
            chart1.ChartAreas[0].AxisY.Title = "Частота, Гц";

            Series series = new Series();
            series.ChartType = SeriesChartType.Line;

            double[] per_time = new double[80];
            per_time[0] = 0;
            for (int i = 1; i < per_time.Length; i++)
            {
                per_time[i] = per_time[i-1] + 0.00125;
                Console.WriteLine(per_time[i]);
            }

            double pertime = 0;

            for (int i = 0; i < mnim.Count; i++)
            {
                series.Points.AddXY(per_time[i], mnim[i]);
                pertime += 1 / 800;
            }

            chart1.ChartAreas[0].AxisX.Minimum = per_time[0];
            chart1.ChartAreas[0].AxisX.Maximum = per_time[79]; 
            chart1.ChartAreas[0].AxisY.Minimum = Math.Round(mnim.Min(), 1); 
            chart1.ChartAreas[0].AxisY.Maximum = Math.Round(mnim.Max(), 1); 

            chart1.Series.Add(series);
        }


        private void PlotPSignal(string fileName_i, string fileName_u)
        {

            double[] doubleLines_U = getLastDoubleArray(getStringArray(fileName_u));
            double[] doubleLines_I = getLastDoubleArray(getStringArray(fileName_i));


            double[] signalData = new double[80];

            for (int i = 0; i < signalData.Length; i++)
            {
                signalData[i] = doubleLines_U[i] * doubleLines_I[i];
            }


            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Line;

            double secondsPer = 0.00125;
            for (int i = 0; i < signalData.Length; i++)
            {
                series.Points.AddXY(i * secondsPer, signalData[i]);
            }

            chart1.Series.Add(series);

            chart1.ChartAreas[0].AxisX.Title = "Время, с";
            chart1.ChartAreas[0].AxisY.Title = "Мгновенная мощность, Вт";

            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;


            chart1.ChartAreas[0].AxisX.Minimum = 0; 
            chart1.ChartAreas[0].AxisX.Maximum = signalData.Length * secondsPer;
            chart1.ChartAreas[0].AxisY.Minimum = signalData.Min(); 
            chart1.ChartAreas[0].AxisY.Maximum = signalData.Max(); 

        }

        private void PlotSignal_QS(string fileName_i, string fileName_u)
        {
            string[] stringLines_U = getStringArray(fileName_u);//массив без повторений
            string[] stringLines_I = getStringArray(fileName_i);

            Console.WriteLine(stringLines_I.ToString());
            Console.WriteLine(stringLines_I[10]);

            List<double> doubleLines_U = new List<double>();
            List<double> doubleLines_I = new List<double>();


            for (int d = 0; d < stringLines_U.Length; d++)
            {
                string line = stringLines_U[d];

                string[] parts = line.Split('\t');

                foreach (string part in parts)
                {
                    double number;
                    if (double.TryParse(part, out number))
                    {
                        doubleLines_U.Add(number);
                    }

                }
            }

            for (int a = 0; a < stringLines_I.Length; a++)
            {
                string line = stringLines_I[a];

                string[] parts = line.Split('\t');

                foreach (string part in parts)
                {
                    double number;
                    if (double.TryParse(part, out number))
                    {
                        doubleLines_I.Add(number);
                    }

                }
            }

            

            List<double> cal_integral_p = new List<double>();
            double p;
            List<int> cal_ends = new List<int>();
            for (int z = 0; z < doubleLines_U.Count; z += 800)
            {
                p = 0;
                int end = Math.Min(z + 800, doubleLines_U.Count);
                cal_ends.Add(end);
                for (int j = z; j < end; j++)
                {
                    p += doubleLines_U[j] * doubleLines_I[j];
                }

                cal_integral_p.Add(0.00125 * p);
            }

            chart1.ChartAreas[0].AxisX.Title = "Время, с";
            chart1.ChartAreas[0].AxisY.Title = "Мощности, Вт";


            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Line;

            series.Name = "P";
            series.Color = Color.Red;

            for (int j = 0; j < cal_integral_p.Count; j += 1)
            {
                series.Points.AddXY(j, cal_integral_p[j]);
            }
            chart1.Series.Add(series);








            List<double> cal_integral_u = new List<double>();
            double u;
            for (int z = 0; z < doubleLines_U.Count; z += 800)
            {
                u = 0;
                int end = Math.Min(z + 800, doubleLines_U.Count);
                for (int j = z; j < end; j++)
                {
                    u += Math.Pow(doubleLines_U[j], 2);
                }

                cal_integral_u.Add(Math.Sqrt(0.00125 * u));
            }


            List<double> cal_integral_i = new List<double>();
            double i;
            for (int z = 0; z < doubleLines_I.Count; z += 800)
            {
                i = 0;
                int end = Math.Min(z + 800, doubleLines_I.Count);
                for (int j = z; j < end; j++)
                {
                    i += Math.Pow(doubleLines_I[j], 2);
                }

                cal_integral_i.Add(Math.Sqrt(0.00125 * i));
            }

            List<double> cal_integral_s = new List<double>();
            for (int x = 0; x < cal_integral_i.Count; x++)
            {
                cal_integral_s.Add(cal_integral_i[x] * cal_integral_u[x]);
            }



            Series series3 = new Series();
            series3.ChartType = SeriesChartType.Line;

            series3.Name = "S";
            series3.Color = Color.Green;

            for (int j = 0; j < cal_integral_s.Count; j += 1)
            {
                series3.Points.AddXY(j, cal_integral_s[j]);
            }
            chart1.Series.Add(series3);



            List<double> cal_integral_q = new List<double>();
            for (int q = 0; q < cal_integral_s.Count; q++)
            {
                cal_integral_q.Add(Math.Sqrt(Math.Pow(cal_integral_s[q], 2) - Math.Pow(cal_integral_p[q], 2)));
            }

            Series series4 = new Series();
            series4.ChartType = SeriesChartType.Line;
            series4.Name = "Q";
            series4.Color = Color.Purple;
            for (int b = 0; b < cal_integral_q.Count; b += 1)
            {
                series4.Points.AddXY(b, cal_integral_q[b]);
            }
            chart1.Series.Add(series4);


            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 79;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 2500;
        }

    


        private void PlotSignal_amplitude(string filename_i)
        {
            string[] data = getStringArray(filename_i);
            List<double> doubleLines_i = new List<double>();


            for (int d = 0; d < data.Length; d++)
            {
                string line = data[d];

                string[] parts = line.Split('\t');

                foreach (string part in parts)
                {
                    double number;
                    if (double.TryParse(part, out number))
                    {
                        doubleLines_i.Add(number);
                    }

                }
            }


            List<double> amp1 = new List<double>();
            List<double> amp2 = new List<double>();
            List<double> amp3 = new List<double>();


            for (int z = 0; z < doubleLines_i.Count; z += 80)
            {
                int end = Math.Min(z + 64, doubleLines_i.Count);

                List<double> values = new List<double>();
                for (int j = z; j < end; j++)
                {
                    values.Add(doubleLines_i[j]);
                }
                
                if (values.Count == 64)

                {
                    System.Numerics.Complex[] fft2 = FftSharp.FFT.Forward(values.ToArray());
                    double freq1 = 800 / 64;
                    int in1 = Convert.ToInt32(50 / freq1);
                    int in2 = Convert.ToInt32(150 / freq1);
                    int in3 = Convert.ToInt32(250 / freq1);
                    List<double> amplitude = new List<double>();

                    foreach (var complexNumber in fft2)
                    {
                        double im = complexNumber.Imaginary;
                        double rel = complexNumber.Real;
                        double absmnim = Math.Sqrt(Math.Pow(im, 2) + Math.Pow(rel, 2));
                        amplitude.Add(absmnim);
                    }
                    amp1.Add(amplitude[in1]);
                    amp2.Add(amplitude[in2]);
                    amp3.Add(amplitude[in3]);
                }
            }

            chart1.ChartAreas[0].AxisX.Title = "Время, с";
            chart1.ChartAreas[0].AxisY.Title = "Амплитуда";

            chart1.Series.Clear();
            Series series5 = new Series();
            series5.ChartType = SeriesChartType.Line;
            series5.Name = "Amplitude1";
            series5.Color = Color.Orange;

            double time1 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series5.Points.AddXY(time1, amp1[j]);
                time1 += 0.1;
            }
            chart1.Series.Add(series5);


            Series series6 = new Series();
            series6.ChartType = SeriesChartType.Line;
            series6.Name = "Amplitude2";
            series6.Color = Color.Red;

            double time2 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series6.Points.AddXY(time2, amp2[j]);
                time2 += 0.1;
            }
            chart1.Series.Add(series6);


            Series series7 = new Series();
            series7.ChartType = SeriesChartType.Line;
            series7.Name = "Amplitude3";
            series7.Color = Color.Purple;

            double time3 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series7.Points.AddXY(time3, amp3[j]);
                time3 += 0.1;
            }

            chart1.Series.Add(series7);

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 70;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 350;
        }

        private void PlotSignal_phase(string filename_i)
        {
            string[] data = getStringArray(filename_i);
            List<double> doubleLines_i = new List<double>();


            for (int d = 0; d < data.Length; d++)
            {
                string line = data[d];

                string[] parts = line.Split('\t');

                foreach (string part in parts)
                {
                    double number;
                    if (double.TryParse(part, out number))
                    {
                        doubleLines_i.Add(number);
                    }

                }
            }

            List<double> phase1 = new List<double>();
            List<double> phase2 = new List<double>();
            List<double> phase3 = new List<double>();

            for (int z = 0; z < doubleLines_i.Count; z += 80)
            {
                int end = Math.Min(z + 64, doubleLines_i.Count);

                List<double> values = new List<double>();
                for (int j = z; j < end; j++)
                {
                    values.Add(doubleLines_i[j]);
                }

                if (values.Count == 64)

                {
                    System.Numerics.Complex[] fft2 = FftSharp.FFT.Forward(values.ToArray());
                    double[] phases = FftSharp.FFT.Phase(fft2);
                    double freq1 = 800 / 64;
                    int in1 = Convert.ToInt32(50 / freq1);
                    int in2 = Convert.ToInt32(150 / freq1);
                    int in3 = Convert.ToInt32(250 / freq1);
                    List<double> phase = new List<double>();

                    foreach (var complexNumber in fft2)
                    {
                        double im = complexNumber.Imaginary;
                        double rel = complexNumber.Real;
                        double absmnim = Math.Sqrt(Math.Pow(im, 2) + Math.Pow(rel, 2));
                        phase.Add(absmnim);
                    }
                    phase1.Add(phases[in1]);
                    phase2.Add(phases[in2]);
                    phase3.Add(phases[in3]);
                }
            }

            chart1.ChartAreas[0].AxisX.Title = "Время, с";
            chart1.ChartAreas[0].AxisY.Title = "Фаза";

            chart1.Series.Clear();
            Series series8 = new Series();
            series8.ChartType = SeriesChartType.Line;
            series8.Name = "Phase1";
            series8.Color = Color.Orange;

            double time4 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series8.Points.AddXY(time4, phase1[j]);
                time4 += 0.1;
            }
            chart1.Series.Add(series8);


            Series series9 = new Series();
            series9.ChartType = SeriesChartType.Line;
            series9.Name = "Phase2";
            series9.Color = Color.Red;

            double time5 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series9.Points.AddXY(time5, phase2[j]);
                time5 += 0.1;
            }
            chart1.Series.Add(series9);


            Series series10 = new Series();
            series10.ChartType = SeriesChartType.Line;
            series10.Name = "Phase3";
            series10.Color = Color.Purple;

            double time6 = 0;

            for (int j = 0; j < data.Length; j += 1)
            {
                series10.Points.AddXY(time6, phase3[j]);
                time6 += 0.1;
            }

            chart1.Series.Add(series10);

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 70;
            chart1.ChartAreas[0].AxisY.Minimum = -4;
            chart1.ChartAreas[0].AxisY.Maximum = 4;
        }

    }
}
