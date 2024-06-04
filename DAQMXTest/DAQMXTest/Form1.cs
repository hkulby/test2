using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;

//using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DAQMXTest
{
    public partial class Form1 : Form
    {
        private Task analogInTask;
        private Task analogOutTask;
        private AnalogSingleChannelReader reader;
        private AnalogMultiChannelWriter writer;
        private System.Windows.Forms.Timer timer;
        private int samples;
        private int temp;
        private double PSUVoltage;
        private double PSUCurrent;
        private int count;
        private int PSUTime;
        private double elapsedTime;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("Time", "Time");
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns.Add("Temperature", "Temperature");
            dataGridView1.Columns[1].Width = 100;

            //DelayUpDown = this.DelayUpDown;
            //SamplesUpDown = this.SamplesUpDown;
            //TempUpDown = this.TempUpDown;
            //PSUVoltageUpDown = this.PSUVoltageUpDown;
            //PSUCurrentUpDown = this.PSUCurrentUpDown;

            DelayUpDown.Maximum = 10000;
            DelayUpDown.Minimum = 500;
            SamplesUpDown.Maximum = 100000;
            SamplesUpDown.Minimum = 1000;
            TempUpDown.Minimum = 30;
            PSUVoltageUpDown.Maximum = 10;
            PSUCurrentUpDown.Maximum = 10;
            PSUVoltageUpDown.Minimum = 2;
            PSUCurrentUpDown.Minimum = 1;
            PSUTimeUpDown.Maximum = 3600;
            PSUTimeUpDown.Minimum = 10;


            DeviceComboBox.Items.AddRange(DaqSystem.Local.Devices);
            if (DeviceComboBox.Items.Count > 0)
            {
                DeviceComboBox.SelectedIndex = 0;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            // Creating and configuring the task for line4
            using (Task line4Task = new Task())
            {
                line4Task.DOChannels.CreateChannel("Dev1/port2/line4", "Line4", ChannelLineGrouping.OneChannelForEachLine);

                // Create a writer for the task
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(line4Task.Stream);

                // Start the task
                line4Task.Start();

                // Turn on line4
                writer.WriteSingleSampleSingleLine(false, true);
                Thread.Sleep(5000);

                // Turn off line4 (if it was on)
                //writer.WriteSingleSampleSingleLine(true, false);

                // Stop the task
                //line4Task.Stop();
            }

            using (Task line5Task = new Task())
            {
                line5Task.DOChannels.CreateChannel("Dev1/port2/line5", "Line5", ChannelLineGrouping.OneChannelForEachLine);

                // Create a writer for the task
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(line5Task.Stream);

                // Start the task
                line5Task.Start();

                // Turn on line5
                //writer.WriteSingleSampleSingleLine(true, true);
                //Thread.Sleep(5000);

                // Turn off line5
                writer.WriteSingleSampleSingleLine(false, false);

                // Stop the task
                //line5Task.Stop();
            }




            // Dispose of existing tasks if they exist
            if (analogInTask != null)
            {
                analogInTask.Dispose();
            }
            if (analogOutTask != null)
            {
                analogOutTask.Dispose();
            }


            analogOutTask = new Task();
            AOChannel voltageChannel;
            AOChannel currentChannel;
            writer = new AnalogMultiChannelWriter(analogOutTask.Stream);

            PSUCurrent = (double)PSUCurrentUpDown.Value;
            PSUVoltage = (double)PSUVoltageUpDown.Value;
            PSUTime = (int)PSUTimeUpDown.Value * 1000;

            // Create voltage channel
            voltageChannel = analogOutTask.AOChannels.CreateVoltageChannel(
            DeviceComboBox.Text + "/ao0",
            "VoltageChannel",
            0,
            10,
            AOVoltageUnits.Volts
            );

            // Create current channel
            currentChannel = analogOutTask.AOChannels.CreateVoltageChannel(
            DeviceComboBox.Text + "/ao1",
            "CurrentChannel",
            0,
            10,
            AOVoltageUnits.Volts
            );

            analogOutTask.Start();

            // Write initial values
            writer.WriteSingleSample(true, new double[] { PSUVoltage, PSUCurrent });

            // Perform analog output for duration
            Thread.Sleep(PSUTime);

            analogOutTask.Stop();

            ResetOutputChannels();

            using (Task line5Task = new Task())
            {
                line5Task.DOChannels.CreateChannel("Dev1/port2/line5", "Line5", ChannelLineGrouping.OneChannelForEachLine);

                // Create a writer for the task
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(line5Task.Stream);

                // Start the task
                line5Task.Start();

                // Turn on line5
                writer.WriteSingleSampleSingleLine(false, true);
                Thread.Sleep(5000);

                // Turn off line5
                //writer.WriteSingleSampleSingleLine(true, false);

                // Stop the task
                //line5Task.Stop();

            }

            // Creating and configuring the task for line4
            using (Task line4Task = new Task())
            {
                line4Task.DOChannels.CreateChannel("Dev1/port2/line4", "Line4", ChannelLineGrouping.OneChannelForEachLine);

                // Create a writer for the task
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(line4Task.Stream);

                // Start the task

                line4Task.Start();

                // Turn off line4 (if it was on)
                writer.WriteSingleSampleSingleLine(false, false);

                // Stop the task
                //line4Task.Stop();

            }



            //Configure analogIn
            analogInTask = new Task();
            AIChannel myAIChannel;
            reader = new AnalogSingleChannelReader(analogInTask.Stream);
            int interval = (int)DelayUpDown.Value;
            samples = (int)SamplesUpDown.Value;
            temp = (int)TempUpDown.Value;

            if (interval > 99)
            {
                timer = new System.Windows.Forms.Timer { Interval = interval };
                timer.Tick += Timer_Tick;
                timer.Start();
                count = 0;
                elapsedTime = 0;
            }
            else
            {
                MessageBox.Show("Delay must be greater than 100");
                return;
            }

            myAIChannel = analogInTask.AIChannels.CreateVoltageChannel(
            DeviceComboBox.Text + "/ai56",
            "myAIChannel",
            AITerminalConfiguration.Rse,
            0,
            10,
            AIVoltageUnits.Volts
            );
        }


        private void ResetOutputChannels()
        {
            writer.WriteSingleSample(true, new double[] { 0, 0 });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (count < samples)
            {
                double voltage = reader.ReadSingleSample();
                double temperature = ((voltage - 1.25) / 0.005);

                elapsedTime += (double)DelayUpDown.Value / 1000; // Increment elapsed time

                textBox1.Text = temperature.ToString();
                dataGridView1.Rows.Add(elapsedTime, temperature);
                //MessageBox.Show("Data Read");

                if (voltage > (temp * 0.005) + 1.25)
                {
                    timer.Stop();
                    WriteDataGridViewToCSV(textBox5.Text);
                }

                count++;
            }
            else
            {
                timer.Stop();
                WriteDataGridViewToCSV(textBox5.Text);
            }
        }

        private void WriteDataGridViewToCSV(string fileName)
        {
            // Ensure the filename ends with .csv
            if (!fileName.EndsWith(".csv"))
            {
                fileName += ".csv";
            }

            // Combine with a directory path if needed
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false)) // 'false' to overwrite the file
                {
                    // Write the header
                    sw.WriteLine("Time (s),Temperature (C)");

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            List<string> rowData = new List<string>();
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                rowData.Add(cell.Value?.ToString());
                            }
                            string rowString = string.Join(",", rowData);
                            sw.WriteLine(rowString);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error writing to file: {ex.Message}");
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void DelayUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        private void PSUVoltageUpDown_ValueChanged(object sender, EventArgs e)
        {
            PSUVoltage = (double)PSUVoltageUpDown.Value;
            UpdateOutput();
        }

        private void PSUCurrentUpDown_ValueChanged(object sender, EventArgs e)
        {
            PSUCurrent = (double)PSUCurrentUpDown.Value;
            UpdateOutput();
        }

        private void UpdateOutput()
        {
            if (writer != null)
            {
                writer.WriteSingleSample(true, new double[] { PSUVoltage, PSUCurrent });
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }

            if (analogInTask != null)
            {
                analogInTask.Dispose();
            }

            if (analogOutTask != null)
            {
                analogOutTask.Dispose();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void SamplesUpDown_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}