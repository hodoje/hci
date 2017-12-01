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
        public static List<bool> activeRtbChanged = MainWindow.activeRtbChanged;

        private static int index = MainWindow.index;

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
                StringBuilder sb = new StringBuilder();
                string completeText =
                    new TextRange(rtbList[index].Document.ContentStart, rtbList[index].Document.ContentEnd).Text;

                int previousIndex = 0;
                int idx = completeText.IndexOf(findWord, StringComparison.CurrentCultureIgnoreCase);
                while (idx != -1)
                {
                    sb.Append(completeText.Substring(previousIndex, idx - previousIndex));
                    sb.Append(replaceWord);
                    idx += findWord.Length;

                    previousIndex = idx;
                    idx = completeText.IndexOf(findWord, idx, StringComparison.CurrentCultureIgnoreCase);
                }
                sb.Append(completeText.Substring(previousIndex));

                rtbList[index].SelectAll();
                rtbList[index].Selection.Text = sb.ToString();
                rtbList[index].CaretPosition = rtbList[index].CaretPosition.GetPositionAtOffset(0);
                activeRtbChanged[index] = true;
            }
        }

        private void FnRWindow_OnClosing(object sender, CancelEventArgs e)
        {
            rtbList[index].Focus();
        }
    }
}
