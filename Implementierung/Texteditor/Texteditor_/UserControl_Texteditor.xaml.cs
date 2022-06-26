using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Texteditor.Texteditor_
{
    /// <summary>
    /// Interaktionslogik für UserControl_Texteditor.xaml
    /// </summary>
    public partial class UserControl_Texteditor : UserControl
    {
        SpeechSynthesizer reader;
        public UserControl_Texteditor()
        {
            InitializeComponent();

            comBoxFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            comBoxFontSize.ItemsSource = new List<int>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            reader = new SpeechSynthesizer();

            menuItemspeechStop.IsEnabled = false;
            menuItemRemove.IsEnabled = false;
            menuItemCopy.IsEnabled = false;
            menuItemCut.IsEnabled = false;
        }

        #region Datei
        private void New_Click(object sender, RoutedEventArgs e)
        {
            var text = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);

            if (text.IsEmpty)
            {
                MainWindow newWindow = new MainWindow();
                newWindow.Show();
                Window.GetWindow(this).Close();
            }
            else
            {
                var result = MessageBox.Show("Möchten Sie die Änderungen speichern", "Speichern", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    MainWindow newWindow = new MainWindow();
                    newWindow.Show();
                    Window.GetWindow(this).Close();
                }
                else if (result == MessageBoxResult.No)
                {
                    MainWindow newWindow = new MainWindow();
                    newWindow.Show();
                    Window.GetWindow(this).Close();
                }
            }
        }

        private void NewWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Öffnen";
            openFile.Filter = "Plain Text File (*.txt)|*.txt|Rich Text File (*.rtf)|*.rtf|" +
                              "XAML File (*.xaml)|*.xaml|All files (*.*)|*.*";
            if (openFile.ShowDialog() == true)
            {
                if (openFile.FileName.EndsWith("txt"))
                {
                    FileStream fileStream = new FileStream(openFile.FileName, FileMode.Open);
                    TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Text);
                    fileStream.Close();
                }
                else if (openFile.FileName.EndsWith("rtf"))
                {
                    FileStream fileStream = new FileStream(openFile.FileName, FileMode.Open);
                    TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Rtf);
                    fileStream.Close();
                }
                else if (openFile.FileName.EndsWith("Xaml"))
                {
                    FileStream fileStream = new FileStream(openFile.FileName, FileMode.Open);
                    TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Xaml);
                    fileStream.Close();
                }
            }
        }

        readonly SaveFileDialog saveFileDialog = new SaveFileDialog();
        public void Save()
        {
            TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);

            if (saveFileDialog.FileName == "")
            {
                saveFileDialog.FileName = "Untitled";
                saveFileDialog.DefaultExt = "*.txt";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.Title = "Speichen";
                saveFileDialog.Filter = "Plain Text File (*.txt)|*.txt|Rich Text File (*.rtf)|*.rtf|XAML File (*.xaml)|*.xaml|All files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        if (saveFileDialog.FileName.EndsWith("txt"))
                        {
                            range.Save(fileStream, DataFormats.Text);
                        }
                        else if (saveFileDialog.FileName.EndsWith("rtf"))
                        {
                            range.Save(fileStream, DataFormats.Rtf);
                        }
                        else if (saveFileDialog.FileName.EndsWith("xaml"))
                        {
                            range.Save(fileStream, DataFormats.Xaml);
                        }
                    }
                }
            }
            else
            {
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    if (saveFileDialog.FileName.EndsWith("txt"))
                    {
                        range.Save(fileStream, DataFormats.Text);
                    }
                    else if (saveFileDialog.FileName.EndsWith("rtf"))
                    {
                        range.Save(fileStream, DataFormats.Text);
                    }
                    else if (saveFileDialog.FileName.EndsWith("xaml"))
                    {
                        File.WriteAllText(saveFileDialog.FileName, range.Text);
                    }
                }

            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.FileName = "Untitled";
            saveFileDialog.Title = "Speichen unter";
            saveFileDialog.Filter = "Plain Text File (*.txt)|*.txt|Rich Text File (*.rtf)|*.rtf|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "*.txt";
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Text);
            }
        }

        private void SavePDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_pdf = new SaveFileDialog();
            save_pdf.FileName = "Untitled";
            save_pdf.Title = "Speichern als PDF";
            save_pdf.Filter = "(*.pdf)|*.pdf";

            if (save_pdf.ShowDialog() == true)
            {
                iTextSharp.text.Document doc = new iTextSharp.text.Document();
                PdfWriter.GetInstance(doc, new FileStream(save_pdf.FileName, FileMode.Create));
                doc.Open();
                string richText = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd).Text;
                doc.Add(new iTextSharp.text.Paragraph(richText));
                doc.Close();
            }
            Process.Start(save_pdf.FileName);
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveDB_Click(object sender, RoutedEventArgs e)
        {
            string richtextbox = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd).Text;
            string dbName = "Editor";
            //string connectionString = $@"Server=KALO\SQLEXPRESS; Database={dbName};
            //                          Trusted_Connection=True; MultipleActiveResultSets=True";

            string connectionString = $@"Server=localhost\;Initial Catalog={dbName};" +
                                      "User id=sa;" +
                                      "Password=mssqlserver;" +
                                      "MultipleActiveResultSets=True";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string sqlSelect = "INSERT INTO Data(Text, Zeitstempel)"
                                 + $" VALUES('{richtextbox}' , GETDATE());";

                using (SqlCommand cmd = new SqlCommand(sqlSelect, con))
                {
                    try
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader != null)
                        {
                            MessageBox.Show("Text ist in Datenbank gespeichert");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}");
                    }
                }
            }
        }

        private void Schließen_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.SelectAll();
            if (richtxtbox.Selection.Text.Length == 2)
            {
                Window.GetWindow(this).Close();
            }
            else if (richtxtbox.Selection.Text.Length > 2)
            {
                var result = MessageBox.Show("Möchten Sie die Änderungen speichern", "Speichern", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    MainWindow newWindow = new MainWindow();
                    newWindow.Show();
                    Window.GetWindow(this).Close();
                }
                else if (result == MessageBoxResult.No)
                {
                    Window.GetWindow(this).Close();
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialg = new PrintDialog();

            if (printDialg.ShowDialog() == true)
            {
                printDialg.PrintVisual(richtxtbox as Visual, "printing as visual");
            }
        }

        private void Beenden_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion Datei

        #region Einfügen
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Image einfügen";
            openFileDialog.Filter = "Image Files(*.png;*.bmp;*.jpg;*.jpeg;*.gif)|*.png;*.bmp;*.jpg;*.gif:*.jpeg|All files (*.*)|*.* ";
            var image = new Image();

            if (openFileDialog.ShowDialog() == true)
            {
                var imgsrc = new BitmapImage();
                imgsrc.BeginInit();
                imgsrc.StreamSource = File.Open(openFileDialog.FileName, FileMode.Open);
                imgsrc.EndInit();
                image.Source = imgsrc;

                image.Height = 200;
                image.Width = 200;

                var para = new Paragraph();
                para.Inlines.Add(image);
                richtxtbox.Document.Blocks.Add(para);
            }


        }
        #endregion  Einfügen

        #region Bearbeiten
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
            richtxtbox.Copy();
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Paste();
            richtxtbox.Selection.Text = "";
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(richtxtbox.Selection.Text);
            richtxtbox.Selection.Text = "";
        }

        private void Löschen_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Selection.Text = "";
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Undo();
        }
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Redo();
        }
        private void Datum_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Selection.Text = DateTime.Now.ToString();
        }

        private void Speech_Click(object sender, RoutedEventArgs e)
        {
            reader.Dispose();
            string textSelected = richtxtbox.Selection.Text;
            string allText = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd).Text;

            if (allText == "")
            {
                MessageBox.Show("Bitte Schreiben Sie den Text zuerst", "Speech Text",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (textSelected == "")
            {
                MessageBox.Show("Bitte Markieren Sie den Text zuerst", "Speech Text",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                reader = new SpeechSynthesizer();
                reader.SpeakAsync(richtxtbox.Selection.Text);
                menuItemspeechStop.IsEnabled = true;
            }
        }

        private void SpeechStop_Click(object sender, RoutedEventArgs e)
        {
            if (reader != null)
            {
                reader.Dispose();
                menuItemspeech.IsEnabled = true;
                menuItemspeechStop.IsEnabled = false;
            }
        }
        #endregion Bearbeiten

        #region Ansicht 
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.SelectAll();

            EditingCommands.IncreaseFontSize.Execute(null, richtxtbox);
        }
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.SelectAll();
            EditingCommands.DecreaseFontSize.Execute(null, richtxtbox);
        }
        private void ZoomStandard_Click(object sender, RoutedEventArgs e)
        {
            TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
            range.ApplyPropertyValue(FlowDocument.FontSizeProperty, richtxtbox.FontSize = 12);
        }
        #endregion Ansicht 

        private void AllZoomIn(object sender, RoutedEventArgs e)
        {

            TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
            range.ApplyPropertyValue(FlowDocument.FontSizeProperty, richtxtbox.FontSize += 10);
        }
        private void AllZoomout(object sender, RoutedEventArgs e)
        {
            if (richtxtbox.FontSize < 3)
            {
                richtxtbox.FontSize = 2;
            }
            else
            {
                richtxtbox.FontSize -= 10;
            }

        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Khaledkalo/Texteditor");
        }
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.Show();
        }

        #region FontFamily FontSize
        private void CmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comBoxFontFamily.SelectedItem != null)
            {
                richtxtbox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, comBoxFontFamily.SelectedItem);
            }
        }
        private void CmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (comBoxFontSize.SelectedItem != null)
            {
                richtxtbox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, comBoxFontSize.Text); 
            }

        }
        #endregion

        #region Font, Backround Color
        private void FontColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                richtxtbox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B)));
            }
        }
        private void BackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                richtxtbox.Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B)));
            }
        }
        #endregion

        #region DarkMode
        private void Toggle_Checked_Dark(object sender, RoutedEventArgs e)
        {
            richtxtbox.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            richtxtbox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            mainMmenu.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            mainMmenu.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            menuItemNew.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemNewWin.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemOpen.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemSpeichern.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemSpPdf.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemSpDB.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemSpUnter.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemSchließen.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemDrucken.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemBeenden.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            menuItemImage.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            menuItemCopy.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemPaste.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemCut.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemRemove.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemUndo.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemRedo.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemspeech.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemTime.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            menuItemZoomIn.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemZoomOut.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemZoomStandard.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            menuItemGithub.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemContactus.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            menuItemInfo.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            comBoxFontFamily.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            comBoxFontSize.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51));

            comBoxFontFamily.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            comBoxFontSize.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            lblDarkMode.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            fontColor.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            backgroundColor.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        private void Toggle_Unchecked_Light(object sender, RoutedEventArgs e)
        {
            richtxtbox.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            richtxtbox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            mainMmenu.Background = new SolidColorBrush(Color.FromRgb(230, 243, 255));
            mainMmenu.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            comBoxFontFamily.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            comBoxFontSize.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            comBoxFontFamily.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            comBoxFontSize.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            lblDarkMode.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            fontColor.Background = new SolidColorBrush(Color.FromRgb(230, 243, 255));
            backgroundColor.Background = new SolidColorBrush(Color.FromRgb(230, 243, 255));
        }
        #endregion  DarkMode

        #region TextFormat
        private void Fett_Click(object sender, RoutedEventArgs e)
        {
            if (richtxtbox != null)
            {
                if (richtxtbox.Selection.GetPropertyValue(TextElement.FontWeightProperty) is FontWeight &&
                    ((FontWeight)richtxtbox.Selection.GetPropertyValue(TextElement.FontWeightProperty)) == FontWeights.Normal)
                {
                    richtxtbox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
                else
                {
                    richtxtbox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                }
            }
        }
        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            if (richtxtbox != null)
            {
                if (richtxtbox.Selection.GetPropertyValue(TextElement.FontStyleProperty) is FontStyle style && style == FontStyles.Normal)
                {
                    richtxtbox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                }
                else
                {
                    richtxtbox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                }
            }
        }
        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            TextRange selectionRange = new TextRange(richtxtbox.Selection.Start, richtxtbox.Selection.End);

            if (selectionRange.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Underline)
            {
                richtxtbox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
            else
            {
                richtxtbox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
        }
        private void ResetFormatting_Click(object sender, RoutedEventArgs e)
        {
            richtxtbox.Selection.ClearAllProperties();
        }
        private void TexalligmentLeft_Click(object sender, RoutedEventArgs e)
        {
            TextRange range = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);
            range.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
            range.ApplyPropertyValue(FlowDocument.FontSizeProperty, richtxtbox.FontSize = 12);
        }
        private void TexalligmentCenter_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignCenter.Execute(null, richtxtbox);

        }
        private void TexalligmentRight_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignRight.Execute(null, richtxtbox);
        }
        #endregion
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd).Text;

            string searchText = searchTextBox.Text;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("\n\nBitte geben Sie Suchtext oder Quelltext ein, aus dem gesucht werden soll",
                                     "Suchen", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Regex regex = new Regex(searchText);
                int count_MatchFound = Regex.Matches(text, regex.ToString()).Count;

                for (TextPointer startPointer = richtxtbox.Document.ContentStart;
                                 startPointer.CompareTo(richtxtbox.Document.ContentEnd) <= 0;
                                 startPointer = startPointer.GetNextContextPosition(LogicalDirection.Forward))
                {
                    if (startPointer.CompareTo(richtxtbox.Document.ContentEnd) == 0)
                    {
                        break;
                    }

                    string parsedString = startPointer.GetTextInRun(LogicalDirection.Forward);

                    int indexOfParseString = parsedString.IndexOf(searchText);

                    if (indexOfParseString >= 0)
                    {
                        startPointer = startPointer.GetPositionAtOffset(indexOfParseString);

                        if (startPointer != null)
                        {
                            TextPointer nextPointer = startPointer.GetPositionAtOffset(searchText.Length);
                            TextRange searchedTextRange = new TextRange(startPointer, nextPointer);
                            searchedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                        }
                    }
                }

                if (count_MatchFound == 0)
                {
                    MessageBox.Show("\n\n\nNichts gefunden  \n\n", "Suchen", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }



        private void SelectionChanged_Click(object sender, RoutedEventArgs e)
        {
            if (richtxtbox.Selection.Text != "")
            {
                menuItemRemove.IsEnabled = true;
                menuItemCopy.IsEnabled = true;
                menuItemCut.IsEnabled = true;
            }

            // 1- Zeilen und Spalten zählen
            TextPointer tp1 = richtxtbox.Selection.Start.GetLineStartPosition(0);
            TextPointer tp2 = richtxtbox.Selection.Start;

            int column = tp1.GetOffsetToPosition(tp2);

            int someBigNumber = int.MaxValue;
            int lineMoved, currentLineNumber;

            richtxtbox.Selection.Start.GetLineStartPosition(-someBigNumber, out lineMoved);
            currentLineNumber = -lineMoved;
            lblCursorPosition.Text = "Zeile " + currentLineNumber.ToString() + ", Spalte " + column.ToString();


            // 2- Wörter zählen
            TextRange content = new TextRange(richtxtbox.Document.ContentStart, richtxtbox.Document.ContentEnd);

            string whole_text = content.Text;
            string trimmed_text = whole_text.Trim();

            string[] lines = trimmed_text.Split(Environment.NewLine.ToCharArray());

            int space_count = 0;
            string new_text = "";

            foreach (string line in lines)
            {
                foreach (string av in line.Split(' '))
                {
                    if (av == "")
                    {
                        space_count++;
                    }
                    else
                    {
                        new_text = new_text + av + ",";
                    }
                }
            }

            new_text = new_text.TrimEnd(',');
            lines = new_text.Split(',');
            countWörter.Text = $"Wörter " + lines.Length.ToString();
        }

        // Resopnsive Design
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridLength length = new GridLength(0, GridUnitType.Auto);
            rtbRow1.Height = length;
            rtbRow2.Height = length;
        }


    }
}
