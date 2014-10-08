using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaOrganizer
{
    public partial class Form1 : Form
    {
        private FolderBrowserDialog fbd;
        

        public Form1()
        {


            InitializeComponent();

            fbd = new FolderBrowserDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = fbd.ShowDialog();

            textBox2.Text = fbd.SelectedPath;
            Program.log.Debug("Source Path Selected: " + fbd.SelectedPath);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = fbd.ShowDialog();

            textBox3.Text = fbd.SelectedPath;
            Program.log.Debug("Destination Path Selected: " + fbd.SelectedPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";

            string source = textBox2.Text;

            string destination = textBox3.Text;

            bool overwrite = checkBox1.Checked;

            textBox1.AppendText("Collecting files ");
            Program.log.Debug("Collecting files");
            Dictionary<string, DateTime> files = CollectFilesToOrganize(source);
            Program.log.Debug("Collecting files DONE");

            textBox1.AppendText(Environment.NewLine);
            textBox1.AppendText(files.Count + " files to organize.");
            textBox1.AppendText(Environment.NewLine);
            textBox1.AppendText(Environment.NewLine + "Copying files ...");
            textBox1.AppendText(Environment.NewLine);

            Program.log.Debug("Copying files");

            foreach (string key in files.Keys)
            {
                DateTime img = files[key];
                string folderName = GetImageFolderName(img);

                if (!Directory.Exists(destination + "\\" + folderName))
                    Directory.CreateDirectory(destination + "\\" + folderName);

                string logEntry = key + " ->> " + folderName + " ";
                textBox1.AppendText(Environment.NewLine + key + " ->> " + folderName + " ... ");


                if (!File.Exists(destination + "\\" + folderName + "\\" + Path.GetFileName(key)))
                {
                    File.Copy(key, destination + "\\" + folderName + "\\" + Path.GetFileName(key));
                    textBox1.AppendText("Done");
                    logEntry += "Done";
                }
                else if (overwrite)
                {
                    File.Copy(key, destination + "\\" + folderName + "\\" + Path.GetFileName(key), true);
                    textBox1.AppendText("Overwrite");
                    logEntry += "Overwrite";
                }
                else
                {
                    string dest = destination + "\\" + folderName + "\\" + GetNewFileName(key, destination + "\\" + folderName + "\\");
                    File.Copy(key, dest);
                    textBox1.AppendText("Renamed = " + dest);
                    logEntry += "Renamed = " + dest;
                }

                Program.log.Debug(logEntry);
                //System.Threading.Thread.Sleep(500);
            }

            Program.log.Debug("Copying files DONE");

            textBox1.AppendText(Environment.NewLine);
            textBox1.AppendText("Completed.");
            textBox1.AppendText(Environment.NewLine);
        }

        private Dictionary<string, DateTime> CollectFilesToOrganize(string source)
        {
            Dictionary<string, DateTime> result = new Dictionary<string, DateTime>();

            if (!Directory.Exists(source))
                return result;

            string[] dirs = Directory.GetDirectories(source);
            foreach (string dir in dirs)
            {
                Dictionary<string, DateTime> _r = CollectFilesToOrganize(dir);
                foreach (string k in _r.Keys)
                    result.Add(k, _r[k]);
            }

            string[] files = Directory.GetFiles(source);

            foreach (string file in files)
            {
                Program.log.Debug("File: " + file);

                try
                {
                    string ext = Path.GetExtension(file);
                    if (ext.ToLower().Equals(".jpg"))
                    {
                        try
                        {
                            Image img = Image.FromFile(file, true);
                            result.Add(file, DateTaken(img));
                        }
                        catch (Exception)
                        {
                            result.Add(file, File.GetLastWriteTime(file));
                        }
                    }
                    else
                    {
                        result.Add(file, File.GetLastWriteTime(file));
                    }
                }
                catch (Exception)
                {
                    result.Add(file, DateTime.Parse("01-01-2000"));
                }

                textBox1.AppendText(".");
            }

            return result;
        }

        private string GetImageFolderName(DateTime image)
        {
            return image.ToString("yyyy-MM (MMM)");
        }

        private DateTime DateTaken(Image getImage)
        {
            int DateTakenValue = 0x9003; //36867;

            if (!getImage.PropertyIdList.Contains(DateTakenValue))
                return DateTime.Parse("01-01-2000");

            string dateTakenTag = System.Text.Encoding.ASCII.GetString(getImage.GetPropertyItem(DateTakenValue).Value);
            string[] parts = dateTakenTag.Split(':', ' ');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            int hour = int.Parse(parts[3]);
            int minute = int.Parse(parts[4]);
            int second = int.Parse(parts[5]);

            return new DateTime(year, month, day, hour, minute, second);
        }

        private string GetNewFileName(string fileSource, string destination)
        {
            int aux = 1;
            while (File.Exists(destination + Path.GetFileNameWithoutExtension(fileSource) + "_" + aux + Path.GetExtension(fileSource)))
                aux += 1;

            return Path.GetFileNameWithoutExtension(fileSource) + "_" + aux + Path.GetExtension(fileSource);
        }



    }
}
