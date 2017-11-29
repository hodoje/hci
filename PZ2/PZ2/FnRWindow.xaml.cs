using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PZ2
{
    /// <summary>
    /// Interaction logic for FnRWindow.xaml
    /// </summary>
    public partial class FnRWindow : Window
    {
        private static List<int> fontSizes = MainWindow.fontSizes;

        private static List<RichTextBox> rtbList = MainWindow.rtbList;
        private static List<string> activeRtbFilePath = MainWindow.activeRtbFilePath;
        private static List<string> activeRtbFormatAsString = MainWindow.activeRtbFormatAsString;

        private static int index = MainWindow.index;
        private static bool changed = MainWindow.changed;

        public FnRWindow()
        {
            InitializeComponent();
            FindTextBox.Focus();

        }

        private void FindAndReplaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            string findWord = FindTextBox.Text;
            string replaceWord = ReplaceTextBox.Text;

            if (findWord.Equals(String.Empty) || replaceWord.Equals(String.Empty))
            {
                MessageBoxResult emptyInput = MessageBox.Show("You need to enter both 'Find' and 'Replace' arguments.",
                    "Find & Replace", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string completeText = new TextRange(rtbList[index].Document.ContentStart, rtbList[index].Document.ContentEnd).Text.Replace(findWord, replaceWord);
                rtbList[index].Document.Blocks.Clear();
                rtbList[index].Document.Blocks.Add(new Paragraph(new Run(completeText)));
                rtbList[index].CaretPosition = rtbList[index].Document.Blocks.LastBlock.ElementEnd;
            }
        }

        private void FnRWindow_OnClosing(object sender, CancelEventArgs e)
        {
            rtbList[index].Focus();
        }
    }
}
