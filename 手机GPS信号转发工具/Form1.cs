using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace 手机GPS信号转发工具
{
    public partial class Form1 : Form
    {
        bool IsWork = false;
        Process por = new Process();
        List<string> newlines = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.Items.Clear();


            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(ports);
            this.comboBox1.Items.AddRange(ports);

            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 3;
            this.comboBox1.Enabled = this.checkBox1.Checked;
            this.comboBox2.Enabled = this.checkBox1.Checked;

            if (DateTime.Now.CompareTo(new DateTime(2015, 4, 15, 14, 15, 00)) >= 0)
            {
                MessageBox.Show("软件已经过期", "错误");
                this.Close();
            }


        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //por.StandardInput.WriteLine("dir");

            string cmdstring ="\""+ this.textBox2.Text + "\" logcat -v tag -s NMEA2LOG";
            por.StandardInput.WriteLine(cmdstring);
            //por.StandardInput.WriteLine("Exit");

            StreamReader sr = por.StandardOutput;
            char[]buffer = new char[1024];
            string line = "";
            while (true)
            {
                if (IsWork == false)
                {
                    return;
                } 
                try
                {
                    //int len = sr.Read(buffer, 0, 100);
                    //newLine = "";
                    //for (int i = 0; i < len; i++)
                    //{
                    //    newLine += buffer[i];
                    //}
                    //this.backgroundWorker1.ReportProgress(50);


                    line = sr.ReadLine();
                    line += "\n";
                    if (line.Contains("$GP"))
                    {
                        int Index = line.IndexOf("$GP");
                        string substring = line.Substring(Index);

                        string[] cmds = substring.Split(new char[] { '$' });
                        foreach (string singel in cmds)
                        {
                            string result = singel;
                            if (result.Contains("GP"))
                            {
                                if (result.Contains("\n"))
                                {
                                    int enterIndex = result.IndexOf('\n');
                                    result = result.Remove(enterIndex);
                                }
                                string oneline = "$" + result + "\r\n";
                                byte[] wtiteBytes = Encoding.Default.GetBytes(oneline);
                                newlines.Add(oneline);
                                if(this.serialPort1.IsOpen)
                                    this.serialPort1.Write(wtiteBytes, 0, wtiteBytes.Length);
                                this.backgroundWorker1.ReportProgress(50);
                            }
                        }
                    }
                }
                catch
                {
                    line = "chu cuo la ";
                }


            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int count = newlines.Count;
            for (int i = 0; i < count; i++)
            {
                this.textBox1.AppendText(newlines[0]);
                newlines.RemoveAt(0);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsWork == true)
            {
                this.backgroundWorker1.CancelAsync();
                IsWork = false;
                this.button2.Text = "开启";
                por.Close();
                if(this.serialPort1.IsOpen)
                    this.serialPort1.Close();
            }
            else
            {
                if (textBox2.Text.Contains("adb.exe") == false)
                {
                    MessageBox.Show("请先选择adb.exe文件", "提示");
                    return;
                }

                if (this.comboBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择串口号", "提示");
                    return;
                }
                if (this.comboBox2.SelectedIndex == -1)
                {
                    MessageBox.Show("请选择波特率", "提示");
                    return;
                }

                if (this.checkBox1.Checked)
                {
                    this.serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
                    this.serialPort1.BaudRate = Convert.ToInt32(this.comboBox2.SelectedItem.ToString());
                    this.serialPort1.Parity = System.IO.Ports.Parity.None;
                    this.serialPort1.DataBits = 8;
                    try
                    {
                        this.serialPort1.Open();
                    }
                    catch
                    {
                        MessageBox.Show("串口开启失败！", "提示");
                        return;
                    }
                }

                por.StartInfo.FileName = "cmd.exe";
                por.StartInfo.UseShellExecute = false;
                por.StartInfo.RedirectStandardInput = true;
                por.StartInfo.RedirectStandardOutput = true;
                por.StartInfo.RedirectStandardError = true;
                por.StartInfo.CreateNoWindow = true;

                por.Start();

                IsWork = true;
                try
                {
                    this.backgroundWorker1.RunWorkerAsync();
                }
                catch
                {
                    MessageBox.Show("任务启动失败，请重新启动！程序将自动关闭", "提示");
                    return;
                }
                this.button2.Text = "停止";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "adb.exe|adb.exe";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox2.Text = ofd.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.comboBox1.Enabled = this.checkBox1.Checked;
            this.comboBox2.Enabled = this.checkBox1.Checked;
        }




    }
}
