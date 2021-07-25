using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Take data from file supplied by the user and save it to textBox
        private void openFileBtnClick(object sender, RoutedEventArgs e)
        {
            // Create an open file dialog box.
            var openDlg = new OpenFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory
            };
            // Did they click on the OK button? Window class also has a DialogResult hence the whole namespace needs to be specified.
            if (openDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Load all text of selected file.
                string dataFromFile = File.ReadAllText(openDlg.FileName);
                // Show string in TextBox.
                textBox.Text = dataFromFile;
                outcomeLabel.Content = "Click on 'Process and Save' to process the open file...";
            }
        }

        // Process the data from the textBox and save it to a file
        private void processFileBtnClick(object sender, RoutedEventArgs e)
        {
            /* The objective is to get an array of SWIFt objects from dataFromFile. dataFromFile, Pattern -> GetPageRgxCollection
               -> GetSWIFTObjects -> SortSWIFTObjects {LINQ} -> Remove950etc. {LINQ} -> PrintToFile */

            outcomeLabel.Content = "Processing File...";

            // Split text in the textBox into Regex matches by using this regex pattern i.e. dataFromFile, Pattern -> GetPageRgxCollection
            string pageSplitPattern = @"\d\d\/\d\d\/\d\d\-\d\d\:\d\d\:\d\d(.|\n)+?{CHK.*?\n.*?\n";
            MatchCollection messageMatches = GetPageRgxCollection(textBox.Text, pageSplitPattern);

            // Get List of SWIFT Messages from the Regex matches i.e. GetPageRgxCollection -> GetSWIFTObjects
            List<SWIFTMessage> swiftMessages = new List<SWIFTMessage>();
            foreach (Match messageMatch in messageMatches)
                swiftMessages.Add(new SWIFTMessage(messageMatch));

            // Sort the list of SWIFT Messages first by Sender then by NPTR No. i.e. GetSWIFTObjects -> SortSWIFTObjects {LINQ} -> Remove950 {LINQ}
            var SortedMessages = from message in swiftMessages
                                 where message.Type == 202 || message.Type == 203 || message.Type == 299 || message.Type == 199 // Remove 950 and other irrelevant SWIFT messages
                                 orderby message.Sender, message.NPTR
                                 select message;


            // Calculations to fit one SWIFT Message exactly into one or multiple pages
            // Get number of Columns in page or possible characters in a single line. This is determined by page width and margin
            int tempColumns;
            int numberofColumnsinPage = int.TryParse(textBox2.Text, out tempColumns) ? tempColumns : 73; // Number of Rows in a Page, as per the page size to aid printing 

            // Get number of rows in a single page. This is determined by length of the Page
            int tempRows;
            int numberofRowsinPage =  int.TryParse(textBox1.Text, out tempRows) ? tempRows : 55; // Number of Rows in a Page, as per the page size to aid printing 

            // Build a string to fit exactly into one or more pages
            StringBuilder sbtemp = new StringBuilder();
            foreach (var m in SortedMessages)
            {
                string[] rows = m.CompleteMessage.Split('\n');
                int? numberofWrappedRows = (from row in rows
                                           select row.Length / numberofColumnsinPage).Sum();
                int numberofReturns = NumberofReturns(m.CompleteMessage);
                int numberofRowsinMessage = (int)numberofWrappedRows + numberofReturns;
                 
                sbtemp.Append(m.CompleteMessage + new string('\n', numberofRowsinPage - numberofRowsinMessage % numberofRowsinPage));
                /* Code for Debug only: System.Windows.Forms.MessageBox.Show("Number of Rows in Page = " + numberofRowsinPage.ToString() +
                                                     "\nNumber of Wrapped Rows = " + numberofWrappedRows.ToString() +
                                                     "\nNumber of Returns =" + numberofReturns.ToString()); */

            }
            textBox.Text = sbtemp.ToString();

            // Create a dialog box to save files.
            var saveDlg = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory
            };
            // Did they click on the OK button?
            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Save data in the TextBox to the named file.
                File.WriteAllText(saveDlg.FileName, textBox.Text);
                outcomeLabel.Content = "File Saved....";
            }
        }
        
        // Helper Function to Calculate number of new lines in a file
        private int NumberofReturns(string str)
        {
            return str.Count(x => x == '\n');
        }

        // Function to get a Match Collection from a string. This is used to split pages in processFileBtnClick function
        private MatchCollection GetPageRgxCollection(string text, string pageSplitPattern)
        {
            Regex pageMatchHelper = new Regex(pageSplitPattern);
            return pageMatchHelper.Matches(text);
        }
    }
}
