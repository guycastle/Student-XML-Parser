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
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = true;
            var filePaths = Directory.EnumerateFiles(selectedPath);
            progressBar1.Maximum = filePaths.Count(); ;
            foreach (var filePath in filePaths)
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
                    Helper.SetAuditCreationInfo(student, xdoc.Element("Message").Attribute("Source").Value);
                    student.CreationDate = DateTime.Parse(xdoc.Element("Message").Attribute("CreationDateTime").Value);
                    students.Upsert(student);
                }
                //Thread.Sleep(300);
                importFileCount++;
                progressBar1.Value++;
                btnValidate.Enabled = true;
            }
            progressBar1.Value = 0;
            tbResult.Text = string.Format("Import Complete {0} \r\n> {1} files imported \r\n> {2} students in the database.", DateTime.Now, importFileCount, students.Count());
            btnCancel.Enabled = false;
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

        private void btnValidate_Click(object sender, EventArgs e)
        {
            bool valResult = students.ValidateAll();
            StringBuilder sb = new StringBuilder(valResult ? "All elements are valid" : "Warning: validation errors detected\r\n");
            if (!valResult)
            {
                sb.Append("Invalid: ")
                    .Append(students.Count(x => !x.Validate()))
                    .Append("\r\nToo old: ")
                    .Append(students.Count(x => x.GeboorteDatum.Year + 100 < DateTime.Now.Year))
                    .Append("\r\n")
                    .Append(Helper.Print(students.Where(x => x.GeboorteDatum.Year + 100 < DateTime.Now.Year)))
                    .Append("\r\nToo young: ")
                    .Append(students.Count(x => x.GeboorteDatum.Year + 15 > DateTime.Now.Year))
                    .Append("\r\n")
                    .Append(Helper.Print(students.Where(x => x.GeboorteDatum.Year + 15 > DateTime.Now.Year)));
            }
            tbValidationResults.Text = sb.ToString();
        }

        private void bckGrndWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
