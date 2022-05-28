using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace Texteditor.Zeichnen_
{
    /// <summary>
    /// Interaktionslogik für UserControl_Zeichnen.xaml
    /// </summary>
    public partial class UserControl_Zeichnen : UserControl
    {
        public Stack<DoStroke> DoStrokes { get; set; }
        public Stack<DoStroke> UndoStrokes { get; set; }

        private bool handle = true;
        public UserControl_Zeichnen()
        {
            InitializeComponent();

            DoStrokes = new Stack<DoStroke>();
            UndoStrokes = new Stack<DoStroke>();
            myInkcanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "*";
            saveFileDialog.DefaultExt = ".png";
            saveFileDialog.Filter = "Bilder (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (saveFileDialog.ShowDialog() == true)
            {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(myInkcanvas);
                double dpi = 96d;
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);
                rtb.Render(myInkcanvas);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
                encoder.Save(fs);
                fs.Close();
            }
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            myInkcanvas.Strokes.Clear();
        }

        #region Color
        private void FrontColor_Click(object sender, RoutedEventArgs e)
        {
            Button FrontColorBtn = (Button)sender;
            if (FrontColorBtn.Name == "Black")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
            }
            else if (FrontColorBtn.Name == "Red")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(255, 0, 0);
            }
            else if (FrontColorBtn.Name == "Green")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 128, 0);
            }
            else if (FrontColorBtn.Name == "Blue")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 255);
            }
            else if (FrontColorBtn.Name == "Yellow")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(255, 255, 0);
            }
            else if (FrontColorBtn.Name == "White")
            {
                myInkcanvas.DefaultDrawingAttributes.Color = Color.FromRgb(255, 255, 255);
            }
        }

        private void BackColor_Click(object sender, RoutedEventArgs e)
        {
            Button BackColorBtn = (Button)sender;
            if (BackColorBtn.Name == "BlackB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.Black);
            }
            else if (BackColorBtn.Name == "RedB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.Red);
            }
            else if (BackColorBtn.Name == "GreenB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.Green);
            }
            else if (BackColorBtn.Name == "BlueB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.Blue);
            }
            else if (BackColorBtn.Name == "YellowB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.Yellow);
            }
            else if (BackColorBtn.Name == "WhiteB")
            {
                myInkcanvas.Background = new SolidColorBrush(Colors.White);
            }
        }
        #endregion
        private void SliderValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderValue.Value == 0)
            {
                sliderValue.Value = 1.0;
            }
            myInkcanvas.DefaultDrawingAttributes.Width = sliderValue.Value;
            myInkcanvas.DefaultDrawingAttributes.Height = sliderValue.Value;
        }

        private void Select_PenModus_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "pen":
                    myInkcanvas.EditingMode = InkCanvasEditingMode.Ink;

                    break;
                case "eraser":

                    myInkcanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    myInkcanvas.Cursor = Cursors.Cross;

                    break;
                case "select":
                    myInkcanvas.EditingMode = InkCanvasEditingMode.Select;
                    break;
            }
        }

        #region Undo and Redo
        public struct DoStroke
        {
            public string ActionFlag { get; set; }
            public System.Windows.Ink.Stroke Stroke { get; set; }
        }
        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            if (handle)
            {
                DoStrokes.Push(new DoStroke
                {
                    ActionFlag = e.Added.Count > 0 ? "ADD" : "REMOVE",
                    Stroke = e.Added.Count > 0 ? e.Added[0] : e.Removed[0]
                });
            }
        }
        private void UndoCanvas_Click(object sender, RoutedEventArgs e)
        {
            handle = false;

            if (DoStrokes.Count > 0)
            {
                DoStroke @do = DoStrokes.Pop();
                if (@do.ActionFlag.Equals("ADD"))
                {
                    myInkcanvas.Strokes.Remove(@do.Stroke);
                }
                else
                {
                    myInkcanvas.Strokes.Add(@do.Stroke);
                }

                UndoStrokes.Push(@do);
            }
            handle = true;
        }
        private void RedoCanvas_Click(object sender, RoutedEventArgs e)
        {
            handle = false;
            if (UndoStrokes.Count > 0)
            {
                DoStroke @do = UndoStrokes.Pop();
                if (@do.ActionFlag.Equals("ADD"))
                {
                    myInkcanvas.Strokes.Add(@do.Stroke);
                }
                else
                {
                    myInkcanvas.Strokes.Remove(@do.Stroke);
                }
            }
            handle = true;
        }
        #endregion Undo and Redo

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridLength length = new GridLength(0, GridUnitType.Auto);
            canvasRow1.Height = length;
            canvasRow2.Height = length;
        }

    }
}
