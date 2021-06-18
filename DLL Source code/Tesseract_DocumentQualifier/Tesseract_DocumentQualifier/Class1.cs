using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Tesseract_DocumentQualifier
{
    public class Class1
    {
        public static string CheckDocument(string imagePath, string fieldsCsvFilePath, string outputFolderPath)
        {
            //outputFolderPath = outputFolderPath + "/Tesseract_output";
            //Run tesseract commands through command prompt process
            //ExecuteCommandSync("tesseract C:/Users/Sikha.P/Downloads/sample2.png C:/Users/Sikha.P/Downloads/output -l eng --psm 6 tsv");
            string fileName = Path.GetFileName(imagePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            ExecuteCommandSync("tesseract " + imagePath + "  " + outputFolderPath + "/" + fileNameWithoutExtension + "_tesseract_output -l eng --psm 6 tsv");
            //ExecuteCommandSync("tesseract " + imagePath + "  " + outputFolderPath + "/output -l eng --psm 6 tsv");
            Thread.Sleep(5000);
            string[] fields = File.ReadAllLines(fieldsCsvFilePath);
            int requiredFieldsCount = 0;
            Image image = Image.FromFile(imagePath);
            string[] lines = File.ReadAllLines(outputFolderPath + "/" + fileNameWithoutExtension + "_tesseract_output.tsv");
            int counter = 0;
            foreach (string field in fields)
            {
                if (counter == 0)
                {
                    counter++;
                    continue;
                }
                bool isExist = false;
                string[] fieldRow = field.Split(',');
             
                string[] aliases = fieldRow[0].Split('|');
                //map through each row 
                
                if (fieldRow[1] == "1")
                {
                    requiredFieldsCount++;
                }
               
                foreach (string line in lines)
                {
                    string[] row = line.Split('\t');
                    
                        if (checkAliases(aliases, row[11].ToLower()))
                        {
                            //check "conf" coulmn value , if its not -1 ,proceed else contine 
                            if (row[10] != "-1" && row[10] != "conf")
                            {
                                if (fieldRow[1] == "1")
                                {
                                    requiredFieldsCount--;
                                }
                                isExist = true;
                                //get co-ordinates on the same row and draw a reactange with that with a specific color
                                using (Graphics g = Graphics.FromImage(image))
                                {
                                    Color customColor = Color.FromArgb(50, Color.Red);
                                    SolidBrush shadowBrush = new SolidBrush(customColor);

                                    Rectangle rect = new Rectangle(Convert.ToInt32(row[6]), Convert.ToInt32(row[7]), Convert.ToInt32(row[8]), Convert.ToInt32(row[9]));
                                    g.FillRectangles(shadowBrush, new RectangleF[] { rect });
                                }
                                break;
                            }
                        }
                    
                    
                }
                if(!isExist)
                {
                    string requiredStatus = fieldRow[1] == "1" ? "REQUIRED-" : "OPTIONAL-";
                    addToLogs("Missing field details-" + requiredStatus + string.Join("|", aliases), outputFolderPath);
                }

            }
            image.Save(outputFolderPath + "/" + fileNameWithoutExtension + "_outputImage.png");
           
            string result=  requiredFieldsCount == 0 ? fileName + " IS QUALIFIED FOR IQ BOT" : fileName + " IS NOT QUALIFIED FOR IQ BOT";
            addToLogs(result, outputFolderPath);
            return result;
        }
        private static bool checkAliases(string[] aliases,string text)
        {
            
            bool status = false;
            foreach (string field in aliases)
            {
                if(text.Contains(field.ToLower()))
                {
                    status = true;
                }
            }
            return status;
        }

        private static void addToLogs(string message,string outputFolderPath)
        {
            using (StreamWriter w = File.AppendText(outputFolderPath+"/log.txt"))
            {
                Log(message, w);
            }

            using (StreamReader r = File.OpenText(outputFolderPath+"/log.txt"))
            {
                DumpLog(r);
            }
        }
        private static void ExecuteCommandSync(string command)
        {
          
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = @"C:\Windows\System32",
                    Verb = "runas",
                    Arguments = "/c " + command,
                    WindowStyle = ProcessWindowStyle.Hidden


            }).WaitForExit();

                //var proc1 = new ProcessStartInfo();
                //proc1.UseShellExecute = true;
                //proc1.WorkingDirectory = @"C:\Windows\System32";
                //proc1.FileName = @"C:\Windows\System32\cmd.exe";
                //proc1.Verb = "runas";
                //proc1.Arguments = "/c " + command;
                //proc1.WindowStyle = ProcessWindowStyle.Hidden;
                //Process.Start(proc1);
                //proc1.Kill();
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()} INFO    {logMessage} ");
        }

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}

//C: \Users\Sikha.P\OneDrive - Automation Anywhere Software Private Limited\Documents\SIKHA\Projects\OCR\IQBot_qulifier\
//C: \Users\Sikha.P\OneDrive - Automation Anywhere Software Private Limited\Documents\SIKHA\Projects\OCR\IQBot_qulifier INFO C:\Users\Sikha.P\OneDrive - Automation Anywhere Software Private Limited\Documents\SIKHA\Projects\OCR\IQBot_qulifier\docs C:\Users\Sikha.P\OneDrive - Automation Anywhere Software Private Limited\Documents\SIKHA\Projects\OCR\IQBot_qulifier\docs\output
