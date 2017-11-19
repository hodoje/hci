using System;
using System.Collections.Generic;
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

namespace PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<int> fontSizes = new List<int>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        private static List<RichTextBox> rtbList = new List<RichTextBox>();
        private static int index;                                                               // Staticki indeks za pracenje trenutnog RTB-a

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

            tab.Content = tabChild;
            tab.Header = "New";

            rtbList.Add(tabChild);

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

            temp = rtbList[index].Selection.GetPropertyValue(Inline.FontFamilyProperty);         // U temp nam se vraca font
            CmbFontFamily.SelectedItem = temp;                                                   // Prikazemo u Comboboxu

            temp = rtbList[index].Selection.GetPropertyValue(Inline.FontSizeProperty);           // U temp nam se vraca font size
            if (temp.ToString() == "{DependencyProperty.UnsetValue}")                            // Prilikom selektovanja teksta gde su razlicite velicine, vratice se "{DependencyProperty.UnsetValue}" string
                CmbFontSize.Text = "";                                                           // Ukoliko se to desilo, zelimo da preimenujemo Text properti da bude ""
            else
                CmbFontSize.Text = temp.ToString();                                              // Ukoliko je sve u redu, prikazemo velicinu

            Number_Of_Words_In_Rtb();                                                            // Prikazati broj reci u tekstu

           // temp = rtbList[index].Selection.GetPropertyValue(Inline.ForegroundProperty);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //private void CloseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        private void CmbFontFamily_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbFontFamily.SelectedItem != null)
                rtbList[index].Selection.ApplyPropertyValue(Inline.FontFamilyProperty, CmbFontFamily.SelectedItem);
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
                        rtbList[index].Selection.ApplyPropertyValue(Inline.FontSizeProperty, CmbFontSize.Text);
                        rtbList[index].Focus(); // Vracamo fokus na sam editor   
                    }
                }
            }
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

        private void CmbColors_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
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

            tab.Content = tabChild;
            tab.Header = "New";

            rtbList.Add(tabChild);

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
                TabItem tab = new TabItem();                                                                         // Pravimo novi TabItem koji ce sadrzati RTB

                RichTextBox tabChild = new RichTextBox();                                                            // Pravimo novi RTB

                tabChild.SelectionChanged += RtbEditor_SelectionChanged;                                             // Ovde podesavamo neke opcije
                tabChild.BorderThickness = new Thickness(1, 1, 1, 1);                                                //
                tabChild.Margin = new Thickness(0, 0, 0, 0);                                                         //
                tabChild.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;                                     //
                tabChild.AcceptsTab = true;                                                                          //

                FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open);                              // Otvaramo novi tok podataka
                TextRange range = new TextRange(tabChild.Document.ContentStart, tabChild.Document.ContentEnd);       // Odredjujemo pokazivace na pocetak i kraj podataka
                if(dialog.FilterIndex == 1 || dialog.FilterIndex == 3)                                               // Ucitavamo podatke iz toka podataka, format zavisi od izabranog filtera
                    range.Load(fileStream, DataFormats.Text);
                else
                    range.Load(fileStream, DataFormats.Rtf);
                
                string absoluteFileName = dialog.FileName;                                                           // Ovde izvlacimo ime fajla
                int slashLastIndex = absoluteFileName.LastIndexOf('\\');                                             //
                string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1);                            //
                
                tab.Header = relativeFileName;                                                                       // Ovde to ime dodeljujemo u header tab-a

                tab.Content = tabChild;                                                                              // Sadrzaj tab-a bice nas RTB

                rtbList.Add(tabChild);                                                                               // Nas RTB dodajemo u listu RTB-ova

                TabCntrl.Items.Add(tab);                                                                             // U tab control element dodajemo nas tab

                foreach (TabItem el in TabCntrl.Items)                                                               // Prolazimo kroz sve tab-ove iz tab control elementa i
                {
                    if (el.Equals(tab))                                                                              // ako se podudara sa onim upravo dodatim
                    {
                        el.Focus();                                                                                  // Prebacuje se fokus na njega
                    }
                }
            }
        }

        private void Save_File_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "All files (*.*)|*.*| Rich Text Format (*.rtf)|*.rtf| Txt (*.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbList[index].Document.ContentStart, rtbList[index].Document.ContentEnd);
                if(dialog.FilterIndex == 1 || dialog.FilterIndex == 3)
                    range.Save(fileStream, DataFormats.Text);
                else
                    range.Save(fileStream, DataFormats.Rtf);

                string absoluteFileName = dialog.FileName;                                                           // Ovde izvlacimo ime fajla
                int slashLastIndex = absoluteFileName.LastIndexOf('\\');                                             //
                string relativeFileName = absoluteFileName.Substring(slashLastIndex + 1);                            //

                TabItem tab = (TabItem) rtbList[index].Parent;
                tab.Header = relativeFileName;
            }
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
    }
}
