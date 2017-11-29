using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<int> fontSizes = new List<int>() { 8, 9, 10, 11, 12, 14, 16, 18 , 20, 22, 24, 26, 28, 36, 48, 72 };

        public static List<RichTextBox> rtbList = new List<RichTextBox>();
        public static List<string> activeRtbFilePath = new List<string>();
        public static List<string> activeRtbFormatAsString = new List<string>();

        public static int index;                                                               // Staticki indeks za pracenje trenutnog RTB-a
        public static bool changed;

        public MainWindow()
        {
            InitializeComponent();
            CmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);        // Inicijalizacija liste fontova
            CmbFontSize.ItemsSource = fontSizes;
            DataContext = this;

            TabItem tab = new TabItem();

            RichTextBox tabChild = new RichTextBox();

            tabChild.SelectionChanged += RtbEditor_SelectionChanged;
            tabChild.BorderThickness = new Thickness(1, 1, 1, 1);
            tabChild.Margin = new Thickness(0, 0, 0, 0);
            tabChild.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tabChild.AcceptsTab = true;
            tabChild.Loaded += (senderr, ee) => tabChild.Focus();                               // Postavlja fokus na element kad se ucita
            tabChild.KeyUp += (senderr, ee) =>
            {
                if (ee.Key == Key.F5)
                {
                    var txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition);
                    var selekcija = rtbList[index].Selection.Text;

                    if (selekcija != "")
                    {
                        if (selekcija[selekcija.Length - 2] == '\r' && selekcija[selekcija.Length - 1] == '\n')
                        {
                            rtbList[index].Document.Blocks.Clear();
                            rtbList[index].CaretPosition = rtbList[index].Document.ContentStart;
                        }
                    }

                    txtRange.Text = DateTime.Now.ToString();
                    txtRange.ApplyPropertyValue(Inline.ForegroundProperty, rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty));
                    txtRange.ApplyPropertyValue(Inline.FontFamilyProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty));
                    txtRange.ApplyPropertyValue(Inline.FontSizeProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty));
                    txtRange.ApplyPropertyValue(Inline.FontWeightProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontWeightProperty));
                    txtRange.ApplyPropertyValue(Inline.FontStyleProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontStyleProperty));
                    txtRange.ApplyPropertyValue(Inline.TextDecorationsProperty, rtbList[index].Selection.GetPropertyValue(Inline.TextDecorationsProperty));

                    rtbList[index].CaretPosition = rtbList[index].CaretPosition.GetPositionAtOffset(txtRange.Text.Length);
                    changed = true;
                }
            };
            tabChild.TextChanged += (senderr, ee) => changed = true;

            //////////////////////////
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

            TextBlock tb = new TextBlock();
            tb.Text = "New";
            sp.Children.Add(tb);

            Button b = new Button();
            b.Content = "X";
            b.Background = new ImageBrush();
            b.BorderThickness = new Thickness(0, 0, 0, 0);
            b.Width = 17;
            b.Height = 17;
            b.Foreground = Brushes.Red;
            b.Click += CloseTab;
            sp.Children.Add(b);

            tab.Header = sp;
            /////////////////////////

            tab.Content = tabChild;

            rtbList.Add(tabChild);

            activeRtbFilePath.Add("");                              // First open tab doesn't have a path yet

            activeRtbFormatAsString.Add("");                        // First open tab doesn't have a DataFormat yet

            TabCntrl.Items.Add(tab);

            foreach (TabItem el in TabCntrl.Items)
            {
                if (el.Equals(tab))
                {
                    el.Focus();
                }
            }
        }

        private void RtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {

            foreach (var el in rtbList)                                 // Sender ce biti neki RTB, a posto ih imamo vise, moramo naci koji je taj,
            {                                                           // Tako da pronalazimo njegov indeks i onda posle koristimo
                if (el.Equals(sender as RichTextBox))
                {
                    index = rtbList.IndexOf(el);
                }
            }

            object temp = rtbList[index].Selection.GetPropertyValue(Inline.FontWeightProperty);                  // Vraca da li je bold ili normalan
            BtnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));      // Proverava da li je dugme pritisnuto
            // CLR properti su svi oni propertiji koje smo koristili do sada, mana kod njih je sto se oni skladiste u memoriji aplikacije, bilo da im se menja njihova podrazumevana vrednost ili ne
            // Oni samo predstavljaju omotace (wrapper-e) oko "private" promenljivih. Koriste Get/Set kako bi preuzimali ili smestali vrednosti privatnih promenljivih.
            // Vrednost CLR propertija direktno zavisi samo od "private" promenljive za koju je namenjen.
            // DependencyProperty su propertiji su propertiji koji se kreiraju samo kada se koriste. Dakle, ako imamo dugme sa 50 propertija, samo oni propertiji kojima ce se promeniti
            // podrazumevana vrednost definisana u "metadata" ce biti smesteni u memoriju
            // Vrednost DependencyPropertija zavisi od eksternih izvora (animacija, data binding, stilovi...)
            // Ovi propertiji se cuvaju u recniku (dictionary-u) kljuceva i vrednosti unutar bazne klase DependencyObject. Kljuc je ime propertija, a vrednost je vrednost propertija
            // Ovaj properti ima dve metode: "GetValue" i "SetValue". Umesto preuzimanja i cuvanja vrednosti iz nekog polja, kao kod (CLR-a), oni preuzimaju i cuvaju vrednosti u taj recnik.
            // Ono sto je zanimljivo kod ovih metoda jeste da kad se pozove "GetValue" on trazi vrednost unutar recnika vrednosti tog objekta gde se poziva (npr. neki TextBox), ukoliko ne moze
            // da nadje vrednost, ona poziva "GetValue" metodu roditeljskog elementa da vidi da li tu ima vrednost za taj neki properti, ako nema, nastavlja se dalje dok se ne nadje (ili ne ne nadje).
            // Sto se tice "SetValue" metode, ukoliko recimo podesimo vrednost za neki properti na nivou Window objekta, nece se samo azurirati vrednost propertija u recniku Window-a, vec ce se opaliti 
            // "property-change" dogadjaj gde sve sto zavisi od toga propertija ce znati za taj dogadjaj. 
            // Ako je to sto je culo taj dogadjaj, takodje neki DependencyProperty, on ce takodje opaliti takav dogadjaj.
            // Tako da, ako npr. promenimo vrednost FontFamily propertija na Window-u, promenice se font u svim ostalim kontrolama.

            temp = rtbList[index].Selection.GetPropertyValue(Inline.FontStyleProperty);
            BtnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

            temp = rtbList[index].Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            BtnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty);            // U temp nam se vraca font
            CmbFontFamily.SelectedItem = temp;                                                      // Prikazemo u Comboboxu

            if (temp.ToString() == "{DependencyProperty.UnsetValue}")
            {
                CmbFontFamily.Text = "";
                CmbFontFamily.SelectedItem = null;
            }
            else if (!rtbList[index].Selection.IsEmpty)
            {
                CmbFontFamily.SelectedItem = CmbFontFamily.SelectedItem;
                CmbFontFamily.Text = temp.ToString();
            }
            else if (rtbList[index].Selection.IsEmpty || CmbFontFamily.SelectedItem == null)
            {
                if (rtbList[index].CaretPosition.GetPositionAtOffset(1, LogicalDirection.Forward) != null)
                {
                    TextRange txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition.GetPositionAtOffset(1, LogicalDirection.Forward));

                    CmbFontFamily.SelectedItem = txtRange.GetPropertyValue(Inline.FontFamilyProperty);
                    CmbFontFamily.Text = txtRange.GetPropertyValue(Inline.FontFamilyProperty).ToString();
                }
            }
            else if (rtbList[index].Selection.IsEmpty && CmbFontSize.SelectedItem != null)
            {
                TextRange txtRange = new TextRange(rtbList[index].Document.ContentStart,
                    rtbList[index].Document.ContentEnd);
                CmbFontFamily.SelectedItem = txtRange.GetPropertyValue(Inline.FontFamilyProperty);
                CmbFontFamily.Text = txtRange.GetPropertyValue(Inline.FontFamilyProperty).ToString();
            }

            temp = rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty);              // U temp nam se vraca font size
            if (temp.ToString() == "{DependencyProperty.UnsetValue}")                               // Prilikom selektovanja teksta gde su razlicite velicine, vratice se "{DependencyProperty.UnsetValue}" string
            {
                CmbFontSize.Text = "";                                                              // Ukoliko se to desilo, zelimo da preimenujemo Text properti da bude ""
                CmbFontSize.SelectedItem = null;
            }
            else if (!rtbList[index].Selection.IsEmpty)
            {
                CmbFontSize.SelectedItem = CmbFontSize.SelectedItem;
                CmbFontSize.Text = temp.ToString();
            }
            else if (rtbList[index].Selection.IsEmpty || CmbFontSize.SelectedItem == null)                          // NIJE SAVRSENO
            {
                if (rtbList[index].CaretPosition.GetPositionAtOffset(1, LogicalDirection.Forward) != null)
                {
                    TextRange txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition.GetPositionAtOffset(1, LogicalDirection.Forward));

                    CmbFontSize.SelectedItem = txtRange.GetPropertyValue(Inline.FontSizeProperty);
                    CmbFontSize.Text = txtRange.GetPropertyValue(Inline.FontSizeProperty).ToString();
                }
            }
            else if (rtbList[index].Selection.IsEmpty && CmbFontSize.SelectedItem != null)
            {
                TextRange txtRange = new TextRange(rtbList[index].Document.ContentStart,
                    rtbList[index].Document.ContentEnd);
                CmbFontSize.SelectedItem = txtRange.GetPropertyValue(Inline.FontSizeProperty);
                CmbFontSize.Text = txtRange.GetPropertyValue(Inline.FontSizeProperty).ToString();
            }

            Number_Of_Words_In_Rtb();                                                               // Prikazati broj reci u tekstu

            temp = rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty);            // Preuzmemo trenutnu boju teksta
            if (temp != null && temp.ToString() != "{DependencyProperty.UnsetValue}")
            {
                ClrPcker.SelectedColor = (Color?)ColorConverter.ConvertFromString(temp.ToString());  // Podesimo trenutnu izabranu boju da bude prikazana u listi
            }
            else
            {
                ClrPcker.SelectedColor = null;
            }
        }

        private void CloseTab(object sender, RoutedEventArgs routedEventArgs)
        {
            Button x = (Button)sender;
            StackPanel sp = (StackPanel)x.Parent;
            TabItem t = (TabItem)sp.Parent;
            RichTextBox selectedRtb = (RichTextBox)t.Content;
            int selRtbIndex = 0;

            foreach (var rtb in rtbList)
            {
                if (rtb.Equals(selectedRtb))
                {
                    selRtbIndex = rtbList.IndexOf(rtb);
                    rtbList[selRtbIndex].Focus();
                }
            }
            if (selRtbIndex == 0)
            {

            }
            else
            {
                TabItem delT = (TabItem)rtbList[selRtbIndex].Parent;
                TabCntrl.Items.Remove(delT);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if(changed == true)
            {
                MessageBoxResult result = MessageBox.Show("You are about to exit the program, do you want to save your progress before closing?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == MessageBoxResult.No)
                {

                    e.Cancel = false;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    TabItem tab = (TabItem) rtbList[index].Parent;
                    if (((TextBlock) ((StackPanel) tab.Header).Children[0]).Text == "New")
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "All files (*.*)|*.*| Rich Text Format (*.rtf)|*.rtf| Txt (*.txt)|*.txt";
                        if (dialog.ShowDialog() == true)
                        {
                            FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create);
                            TextRange range = new TextRange(rtbList[index].Document.ContentStart,
                                rtbList[index].Document.ContentEnd);
                            if (dialog.FilterIndex == 1 || dialog.FilterIndex == 3)
                                range.Save(fileStream, DataFormats.Text);
                            else
                                range.Save(fileStream, DataFormats.Rtf);

                            string absoluteFileName = dialog.FileName; // Ovde izvlacimo ime fajla
                            int slashLastIndex = absoluteFileName.LastIndexOf('\\'); //
                            string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1); //

                            e.Cancel = false;
                        }
                    }
                    else
                    {
                        TabItem activeTab = (TabItem)TabCntrl.SelectedItem;
                        RichTextBox activeRtb = (RichTextBox)activeTab.Content;
                        int activeRtbIndex = 0;

                        foreach (var rtb in rtbList)
                        {
                            if (rtb.Equals(activeRtb))
                            {
                                activeRtbIndex = rtbList.IndexOf(rtb);
                            }
                        }

                        if (activeRtbFormatAsString[activeRtbIndex] != "")
                        {
                            DataFormat df = DataFormats.GetDataFormat(activeRtbFormatAsString[activeRtbIndex]);
                            FileStream fileStream = new FileStream(activeRtbFilePath[activeRtbIndex], FileMode.Create, FileAccess.Write);
                            TextRange range = new TextRange(rtbList[activeRtbIndex].Document.ContentStart, rtbList[activeRtbIndex].Document.ContentEnd);
                            range.Save(fileStream, df.Name);
                            fileStream.Close();
                            changed = false;
                        }
                    }
                    
                }
            }
        }

        private void CmbFontFamily_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFontFamily.SelectedItem != null)
            {
                if (CmbFontFamily.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}")
                {

                }
                else if (!rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty).Equals(CmbFontFamily.SelectedItem))
                {
                    rtbList[index].Selection.ApplyPropertyValue(Inline.FontFamilyProperty, CmbFontFamily.SelectedItem);
                    changed = true;
                    rtbList[index].Focus();
                }
            }
            rtbList[index].Focus();
        }

        private void ClrPcker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ClrPcker.SelectedColor != null)
            {
                SolidColorBrush boja = new SolidColorBrush((Color)ClrPcker.SelectedColor);
                if (rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty).ToString() != boja.ToString())
                {
                    rtbList[index].Selection.ApplyPropertyValue(Inline.ForegroundProperty, boja);
                    changed = true;
                }             
                ClrPcker.Focusable = false;
            }
            ClrPcker.IsOpen = false;
            rtbList[index].Focus();
        }

        private void CmbFontSize_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CmbFontSize.Text != "")
            {
                if (!HasPunctuations(CmbFontSize.Text))
                {
                    if (HasOnlyNumbers(CmbFontSize.Text))
                    {
                        if (rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty).ToString() !=
                            CmbFontSize.Text)
                        {
                            rtbList[index].Selection.ApplyPropertyValue(Inline.FontSizeProperty, CmbFontSize.Text);
                            changed = true;
                        }
                    }
                    else
                    {
                        MessageBoxResult result =
                            MessageBox.Show("This is not a valid number.", "Hedit", MessageBoxButton.OK);
                        var font = rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty);
                        if (font.ToString() != "{DependencyProperty.UnsetValue}")
                        {
                            CmbFontSize.SelectedItem = font;
                            CmbFontSize.Text = font.ToString();
                        }
                        CmbFontSize.SelectedItem = null;
                        CmbFontSize.Text = "";
                    }
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("This is not a valid number.", "Hedit", MessageBoxButton.OK);
                    var font = rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty);
                    if (font.ToString() != "{DependencyProperty.UnsetValue}")
                    {
                        CmbFontSize.SelectedItem = font;
                        CmbFontSize.Text = font.ToString();
                    }
                    CmbFontSize.SelectedItem = null;
                    CmbFontSize.Text = "";
                }
            }
            else
            {
                if (rtbList[index].CaretPosition != null)
                {
                    TextRange txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition);

                    CmbFontSize.SelectedItem = txtRange.GetPropertyValue(Inline.FontSizeProperty);
                    CmbFontSize.Text = txtRange.GetPropertyValue(Inline.FontSizeProperty).ToString();

                    rtbList[index].Focus();
                    changed = true;
                }

            }
            rtbList[index].Focus();             
        }

        private void DateTimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition);
            var selekcija = rtbList[index].Selection.Text;

            if (selekcija != "")
            {
                if (selekcija[selekcija.Length - 2] == '\r' && selekcija[selekcija.Length - 1] == '\n')
                {
                    rtbList[index].Document.Blocks.Clear();
                    rtbList[index].CaretPosition = rtbList[index].Document.ContentStart;
                }
            }

            txtRange.Text = DateTime.Now.ToString();
            txtRange.ApplyPropertyValue(Inline.ForegroundProperty, rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty));
            txtRange.ApplyPropertyValue(Inline.FontFamilyProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty));
            txtRange.ApplyPropertyValue(Inline.FontSizeProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty));
            txtRange.ApplyPropertyValue(Inline.FontWeightProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontWeightProperty));
            txtRange.ApplyPropertyValue(Inline.FontStyleProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontStyleProperty));
            txtRange.ApplyPropertyValue(Inline.TextDecorationsProperty, rtbList[index].Selection.GetPropertyValue(Inline.TextDecorationsProperty));

            rtbList[index].CaretPosition = rtbList[index].CaretPosition.GetPositionAtOffset(txtRange.Text.Length);
            changed = true;
        }

        private bool HasPunctuations(string input)
        {
            bool result = input.IndexOfAny("[](){}*,:=;...#".ToCharArray()) != -1;  //returns false if there is a punctuation in an array

            return result;
        }

        private bool HasOnlyNumbers(string input)
        {
            foreach (char c in input)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        private void New_File_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabItem tab = new TabItem();

            RichTextBox tabChild = new RichTextBox();

            tabChild.SelectionChanged += RtbEditor_SelectionChanged;
            tabChild.BorderThickness = new Thickness(1, 1, 1, 1);
            tabChild.Margin = new Thickness(0, 0, 0, 0);
            tabChild.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tabChild.AcceptsTab = true;
            tabChild.Loaded += (senderr, ee) => tabChild.Focus();
            tabChild.KeyUp += (senderr, ee) =>
            {
                if (ee.Key == Key.F5)
                {
                    var txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition);
                    var selekcija = rtbList[index].Selection.Text;

                    if (selekcija != "")
                    {
                        if (selekcija[selekcija.Length - 2] == '\r' && selekcija[selekcija.Length - 1] == '\n')
                        {
                            rtbList[index].Document.Blocks.Clear();
                            rtbList[index].CaretPosition = rtbList[index].Document.ContentStart;
                        }
                    }

                    txtRange.Text = DateTime.Now.ToString();
                    txtRange.ApplyPropertyValue(Inline.ForegroundProperty, rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty));
                    txtRange.ApplyPropertyValue(Inline.FontFamilyProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty));
                    txtRange.ApplyPropertyValue(Inline.FontSizeProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty));
                    txtRange.ApplyPropertyValue(Inline.FontWeightProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontWeightProperty));
                    txtRange.ApplyPropertyValue(Inline.FontStyleProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontStyleProperty));
                    txtRange.ApplyPropertyValue(Inline.TextDecorationsProperty, rtbList[index].Selection.GetPropertyValue(Inline.TextDecorationsProperty));

                    rtbList[index].CaretPosition = rtbList[index].CaretPosition.GetPositionAtOffset(txtRange.Text.Length);
                    changed = true;
                }
            };
            tabChild.TextChanged += (senderr, ee) => changed = true;

            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

            TextBlock tb = new TextBlock();
            tb.Text = "New";
            sp.Children.Add(tb);

            Button b = new Button();
            b.Content = "X";
            b.Background = new ImageBrush();
            b.BorderThickness = new Thickness(0, 0, 0, 0);
            b.Width = 17;
            b.Height = 17;
            b.Foreground = Brushes.Red;
            b.Click += CloseTab;
            sp.Children.Add(b);

            tab.Header = sp;

            tab.Content = tabChild;

            rtbList.Add(tabChild);

            activeRtbFilePath.Add("");                          // New file doesn't have a path yet

            activeRtbFormatAsString.Add("");                    // New file doesn't have a DataFormat yet

            TabCntrl.Items.Add(tab);

            foreach (TabItem el in TabCntrl.Items)
            {
                if (el.Equals(tab))
                {
                    el.Focus();
                }
            }
        }

        private void Open_File_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();                                                            // Pravimo novi dijalog           
            dialog.Filter = "All files (*.*)|*.*| Rich Text Format (*.rtf)|*.rtf| Txt (*.txt)|*.txt";                // Podesavamo filter
            if (dialog.ShowDialog() == true)                                                                         // Kad se dijalog otvori
            {
                string absoluteFileName = dialog.FileName;                                                           // Ovde izvlacimo ime fajla
                int slashLastIndex = absoluteFileName.LastIndexOf('\\');                                             //
                string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1);                            //
                int dotLastIndex = absoluteFileName.LastIndexOf('.');
                string fileExtension = absoluteFileName.Substring(dotLastIndex);

                TabItem tab = new TabItem();                                                                         // Pravimo novi TabItem koji ce sadrzati RTB

                RichTextBox tabChild = new RichTextBox();                                                            // Pravimo novi RTB

                tabChild.SelectionChanged += RtbEditor_SelectionChanged;                                             // Ovde podesavamo neke opcije
                tabChild.BorderThickness = new Thickness(1, 1, 1, 1);                                                //
                tabChild.Margin = new Thickness(0, 0, 0, 0);                                                         //
                tabChild.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;                                     //
                tabChild.AcceptsTab = true;                                                                          //
                tabChild.Loaded += (senderr, ee) => tabChild.Focus();                                                //
                tabChild.KeyUp += (senderr, ee) =>                                                                   //
                {
                    if (ee.Key == Key.F5)
                    {
                        var txtRange = new TextRange(rtbList[index].CaretPosition, rtbList[index].CaretPosition);
                        var selekcija = rtbList[index].Selection.Text;

                        if (selekcija != "")
                        {
                            if (selekcija[selekcija.Length - 2] == '\r' && selekcija[selekcija.Length - 1] == '\n')
                            {
                                rtbList[index].Document.Blocks.Clear();
                                rtbList[index].CaretPosition = rtbList[index].Document.ContentStart;
                            }
                        }

                        txtRange.Text = DateTime.Now.ToString();
                        txtRange.ApplyPropertyValue(Inline.ForegroundProperty, rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty));
                        txtRange.ApplyPropertyValue(Inline.FontFamilyProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty));
                        txtRange.ApplyPropertyValue(Inline.FontSizeProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty));
                        txtRange.ApplyPropertyValue(Inline.FontWeightProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontWeightProperty));
                        txtRange.ApplyPropertyValue(Inline.FontStyleProperty, rtbList[index].Selection.GetPropertyValue(Inline.FontStyleProperty));
                        txtRange.ApplyPropertyValue(Inline.TextDecorationsProperty, rtbList[index].Selection.GetPropertyValue(Inline.TextDecorationsProperty));

                        rtbList[index].CaretPosition = rtbList[index].CaretPosition.GetPositionAtOffset(txtRange.Text.Length);
                        changed = true;
                    }
                };
                tabChild.TextChanged += (senderr, ee) => changed = true;

                FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open);                              // Otvaramo novi tok podataka
                TextRange range = new TextRange(tabChild.Document.ContentStart, tabChild.Document.ContentEnd);       // Odredjujemo pokazivace na pocetak i kraj podataka
                if (fileExtension == ".rtf")                                                                         // Ucitavamo podatke iz toka podataka, format zavisi od izabranog filtera
                {
                    range.Load(fileStream, DataFormats.Rtf);
                    activeRtbFormatAsString.Add(DataFormats.Rtf.ToString());                                         // Cuvamo DataFormat ucitanog fajla
                }
                else
                {
                    range.Load(fileStream, DataFormats.Text);                                                        // do not load docx files
                    activeRtbFormatAsString.Add(DataFormats.Text.ToString());                                        // Cuvamo DataFormat ucitanog fajla
                }                                                                                                 

                fileStream.Close();

                activeRtbFilePath.Add(absoluteFileName);

                ////////////////
                StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};

                TextBlock tb = new TextBlock();
                tb.Text = relativeFileName;
                sp.Children.Add(tb);

                Button b = new Button();
                b.Content = "X";
                b.Background = new ImageBrush();
                b.BorderThickness = new Thickness(0,0,0,0);
                b.Width = 17;
                b.Height = 17;
                b.Foreground = Brushes.Red;
                b.Click += CloseTab;
                sp.Children.Add(b);

                tab.Header = sp;                                                                                     // Ovde to ime dodeljujemo u header tab-a
                ///////////////

                tab.Content = tabChild;                                                                              // Sadrzaj tab-a bice nas RTB

                tabChild.CaretPosition = tabChild.CaretPosition.DocumentEnd;                                         // Nakon ucitavanja fajla prebacujemo caret na kraj

                rtbList.Add(tabChild);                                                                               // Nas RTB dodajemo u listu RTB-ova

                TabCntrl.Items.Add(tab);                                                                             // U tab control element dodajemo nas tab

                foreach (TabItem el in TabCntrl.Items)                                                               // Prolazimo kroz sve tab-ove iz tab control elementa i
                {
                    if (el.Equals(tab))                                                                              // ako se podudara sa onim upravo dodatim
                    {
                        el.Focus();                                                                                  // Prebacuje se fokus na njega
                    }
                }
                changed = false;
            }
        }

        private void Save_File_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (changed)
            {
                TabItem tab = (TabItem)rtbList[index].Parent;
                if (((TextBlock)((StackPanel)tab.Header).Children[0]).Text == "New")            // Pristupamo TextBlock-u koji se nalazi unutar StackPanel-a koji je sadrzan u header-u tab-a
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Filter = "All files (*.*)|*.*| Rich Text Format (*.rtf)|*.rtf| Txt (*.txt)|*.txt";
                    if (dialog.ShowDialog() == true)
                    {
                        FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create);
                        TextRange range = new TextRange(rtbList[index].Document.ContentStart,
                            rtbList[index].Document.ContentEnd);
                        if (dialog.FilterIndex == 1 || dialog.FilterIndex == 3)
                        {
                            range.Save(fileStream, DataFormats.Text);
                            activeRtbFormatAsString[index] = (DataFormats.Text.ToString());
                        }
                        else
                        {
                            range.Save(fileStream, DataFormats.Rtf);
                            activeRtbFormatAsString[index] = (DataFormats.Rtf.ToString());
                        }
                        fileStream.Close();

                        string absoluteFileName = dialog.FileName;                                // Ovde izvlacimo ime fajla
                        int slashLastIndex = absoluteFileName.LastIndexOf('\\');                  //
                        string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1); //
                        int dotLastIndex = absoluteFileName.LastIndexOf('.');
                        string fileExtension = absoluteFileName.Substring(dotLastIndex);

                        StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

                        TextBlock tb = new TextBlock();
                        tb.Text = relativeFileName;
                        sp.Children.Add(tb);

                        Button b = new Button();
                        b.Content = "X";
                        b.Background = new ImageBrush();
                        b.BorderThickness = new Thickness(0, 0, 0, 0);
                        b.Width = 17;
                        b.Height = 17;
                        b.Foreground = Brushes.Red;
                        b.Click += CloseTab;
                        sp.Children.Add(b);

                        tab.Header = sp;

                        activeRtbFilePath[index] = absoluteFileName;
                    }
                    changed = false;
                }
            }
            
            else
            {
                TabItem activeTab = (TabItem) TabCntrl.SelectedItem;
                RichTextBox activeRtb = (RichTextBox)activeTab.Content;
                int activeRtbIndex = 0;

                foreach (var rtb in rtbList)
                {
                    if (rtb.Equals(activeRtb))
                    {
                        activeRtbIndex = rtbList.IndexOf(rtb);
                    }                    
                }

                if (activeRtbFormatAsString[activeRtbIndex] != "")
                {
                    DataFormat df = DataFormats.GetDataFormat(activeRtbFormatAsString[activeRtbIndex]);
                    FileStream fileStream = new FileStream(activeRtbFilePath[activeRtbIndex], FileMode.Create, FileAccess.Write);
                    TextRange range = new TextRange(rtbList[activeRtbIndex].Document.ContentStart, rtbList[activeRtbIndex].Document.ContentEnd);
                    range.Save(fileStream, df.Name);
                    fileStream.Close();
                    changed = false;
                }
            }
        }

        private void SaveAs_File_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "All files (*.*)|*.*| Rich Text Format (*.rtf)|*.rtf| Txt (*.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbList[index].Document.ContentStart, rtbList[index].Document.ContentEnd);
                if (dialog.FilterIndex == 1 || dialog.FilterIndex == 3)
                {
                    range.Save(fileStream, DataFormats.Text);
                    activeRtbFormatAsString[index] = (DataFormats.Text.ToString());
                }
                else
                {
                    range.Save(fileStream, DataFormats.Rtf);
                    activeRtbFormatAsString[index] = (DataFormats.Rtf.ToString());
                }
                fileStream.Close();

                string absoluteFileName = dialog.FileName;                                                           // Ovde izvlacimo ime fajla
                int slashLastIndex = absoluteFileName.LastIndexOf('\\');                                             //
                string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1);                            //

                TabItem tab = (TabItem)rtbList[index].Parent;

                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

                TextBlock tb = new TextBlock();
                tb.Text = relativeFileName;
                sp.Children.Add(tb);

                Button b = new Button();
                b.Content = "X";
                b.Background = new ImageBrush();
                b.BorderThickness = new Thickness(0, 0, 0, 0);
                b.Width = 17;
                b.Height = 17;
                b.Foreground = Brushes.Red;
                b.Click += CloseTab;
                sp.Children.Add(b);

                tab.Header = sp;

                activeRtbFilePath[index] = absoluteFileName;
            }
            changed = false;
        }

        private int Count_Number_Of_Words(string s)
        {
            int counter = 0;
            char[] delimiters = new char[] {' ', '\r', '\t', '\n'};
            if (String.IsNullOrWhiteSpace(s))
            {
                counter = 0;
            }
            else
            {
                counter = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
                changed = true;
            }
            
            return counter;
        }

        private void Number_Of_Words_In_Rtb()
        {
            TextRange textRange = new TextRange(rtbList[index].Document.ContentStart, rtbList[index].Document.ContentEnd);
            int counter = Count_Number_Of_Words(textRange.Text);
            StatusBarTextBlock.Text = "Words: " + counter;
        }

        private void TabPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)              // NE ZNAM ZASTO, ALI RADI! DO NOT TOUCH!
        {
            TabPanel tp = e.Source as TabPanel;
            TabCntrl.Height = e.NewSize.Height;
            TabCntrl.Width = e.NewSize.Width;

            rtbList[index].Height = tp.Height;
            rtbList[index].Width = tp.Width;
        }

        private void FindAndReplace_OnClick(object sender, RoutedEventArgs e)
        {
            FnRWindow newWindow = new FnRWindow();
            newWindow.DataContext = this;
            newWindow.ShowDialog();
        }

        private void CmbFontSize_OnDropDownOpened(object sender, EventArgs e)
        {
            CmbFontSize.SelectedItem = null;
        }
    }
}
