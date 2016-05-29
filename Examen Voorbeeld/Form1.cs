using Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Examen_Voorbeeld
{
    public partial class Form1 : Form
    {
        private string selectedPath;
        private DB<Student> students;
        private int importFileCount;

        public Form1()
        {
            InitializeComponent();
            students = new DB<Student>();
            folderBrowserDialog1.SelectedPath = Directory.GetCurrentDirectory();

            bckGrndWorker.WorkerReportsProgress = true;
            bckGrndWorker.WorkerSupportsCancellation = true;

            //bckGrndWorker.DoWork += bgwImport;
            //bckGrndWorker.ProgressChanged += bgwProgress;
            //bckGrndWorker.RunWorkerCompleted += bgwComplete;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = Directory.EnumerateFiles(selectedPath).Count();
            bckGrndWorker.RunWorkerAsync();
            btnCancel.Enabled = true;            
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                lbPath.Text = folderBrowserDialog1.SelectedPath;
                selectedPath = folderBrowserDialog1.SelectedPath;
                if (Directory.EnumerateFiles(selectedPath, "*.xml").Count() > 0)
                {
                    btnImport.Enabled = true;
                }
                else
                {
                    btnImport.Enabled = false;
                }
            }
        }

        private async void btnValidate_Click(object sender, EventArgs e)
        {
            var valResult = students.ValidateAll();
            StringBuilder sb = new StringBuilder(valResult ? "All elements are valid" : "Warning: validation errors detected\r\n");
            if (!valResult)
            {
                var invalidCount = Task.Run<int>(() => (from s in students where !s.Validate() select s).Count());
                var tooOldCount = Task.Run<int>(() => (from s in students where s != null && Helper.getLeeftijd(s) > 100 select s).Count());
                var tooYoungCount = Task.Run<int>(() => (from s in students where s != null && Helper.getLeeftijd(s) < 15 select s).Count());
                var tooOldList = Task.Run<IEnumerable<Student>>(() => from s in students where s != null && Helper.getLeeftijd(s) > 100 select s);
                var tooYoungList = Task.Run<IEnumerable<Student>>(() => from s in students where s != null && Helper.getLeeftijd(s) < 15 select s);
                await Task.WhenAll(invalidCount, tooOldCount, tooYoungCount, tooOldList, tooYoungList);
                sb.Append("Invalid: ")
                    .Append(invalidCount.Result)
                    .Append("\r\nToo old: ")
                    .Append(tooOldCount.Result)
                    .Append("\r\n")
                    .Append(Helper.Print(tooOldList.Result))
                    .Append("\r\nToo young: ")
                    .Append(tooYoungCount.Result)
                    .Append("\r\n")
                    .Append(Helper.Print(tooYoungList.Result));
            }
            tbValidationResults.Text = sb.ToString();
            btnValidate.Enabled = false;
        }

        private void bgwImport(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Parallel.ForEach<String>(Directory.EnumerateFiles(selectedPath), filePath =>
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {

                    var xdoc = XDocument.Parse(File.OpenText(filePath).ReadToEnd());
                    var student = xdoc.Descendants("Student")
                        .Select(x => new Student
                        {
                            ID = int.Parse(x.Element("Id").Value),
                            Voornaam = x.Element("FirstName").Value,
                            Achternaam = x.Element("LastName").Value,
                            GeboorteDatum = DateTime.Parse(x.Element("Dob").Value)
                        }).FirstOrDefault();

                    if (student != null)
                    {
                        Helper.SetAuditCreationInfo(student, xdoc.Element("Message").Attribute("Source").Value, DateTime.Parse(xdoc.Element("Message").Attribute("CreationDateTime").Value));
                        students.Upsert(student);
                        worker.ReportProgress(1);
                        Thread.Sleep(20);
                    }
                }
            });
        }

        private void bgwProgress(object sender, ProgressChangedEventArgs e)
        {
            importFileCount++;
            progressBar1.Value++;
        }

        private void bgwComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            btnValidate.Enabled = true;
            btnCancel.Enabled = false;
            btnImport.Enabled = true;
            progressBar1.Value = 0;
            tbResult.Text = string.Format("Import Complete {0} \r\n> {1} files imported \r\n> {2} students in the database.", DateTime.Now, importFileCount, students.Count());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            bckGrndWorker.CancelAsync();
        }
    }
}