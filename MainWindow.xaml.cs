using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Controls;


namespace MiniPaintApp
{
    public partial class MainWindow : Window
    {
        private bool _isDrawing = false;
        private Point _startPoint;
        private SolidColorBrush _currentBrush = new SolidColorBrush(Colors.Black);
        private double _brushSize = 2;
        private Shape _currentShape;
        private bool _isTextMode = false;
        private TextBox _textBox;
        private double _blurAmount = 0;
        private double _opacityAmount = 1;
        private Canvas _drawingLayer; // Layer for drawing objects

        public MainWindow()
        {
            InitializeComponent();
            _drawingLayer = new Canvas(); // Create a drawing layer
            DrawingCanvas.Children.Add(_drawingLayer); // Add the layer to the main canvas
        }

        private void BrushSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _brushSize = e.NewValue; // Set brush size to slider value
        }
        private void ApplyBlurEffect(UIElement element)
        {
            if (_blurAmount > 0)
            {
                var blurEffect = new System.Windows.Media.Effects.BlurEffect
                {
                    Radius = _blurAmount
                };
                element.Effect = blurEffect;
            }
            else
            {
                element.Effect = null; // Remove blur when the value is 0
            }
        }
        private void BlurSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Get the blur amount from the slider
            _blurAmount = e.NewValue;

            // Apply the blur effect to the canvas or shapes
            ApplyBlurEffect(DrawingCanvas);
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _opacityAmount = e.NewValue; // Set opacity from slider
            ApplyOpacity();
        }
        private void ApplyOpacity()
        {
            // Apply opacity to the drawing layer
            _drawingLayer.Opacity = _opacityAmount;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isTextMode)
            {
                // Add text if in text mode
                Point position = e.GetPosition(DrawingCanvas);
                _textBox = new TextBox
                {
                    Width = 150,
                    Height = 30,
                    Margin = new Thickness(position.X, position.Y, 0, 0),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Foreground = _currentBrush
                };

                _textBox.Focus();
                _drawingLayer.Children.Add(_textBox);
                _textBox.KeyDown += TextBox_KeyDown; // Handle text input
                _isTextMode = false; // Reset text mode after adding text box
            }
            else
            {
                // Start drawing shape or free draw
                _isDrawing = true;
                _startPoint = e.GetPosition(DrawingCanvas);
                string selectedShape = (ShapePicker.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (selectedShape != "Free Draw")
                {
                    _currentShape = selectedShape switch
                    {
                        "Line" => new Line
                        {
                            Stroke = _currentBrush,
                            StrokeThickness = _brushSize,
                            X1 = _startPoint.X,
                            Y1 = _startPoint.Y
                        },
                        "Rectangle" => new Rectangle
                        {
                            Stroke = _currentBrush,
                            StrokeThickness = _brushSize
                        },
                        "Ellipse" => new Ellipse
                        {
                            Stroke = _currentBrush,
                            StrokeThickness = _brushSize
                        },
                        _ => null
                    };

                    if (_currentShape != null)
                    {
                        _drawingLayer.Children.Add(_currentShape);
                    }
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // When Enter key is pressed, remove focus from the text box
            if (e.Key == Key.Enter)
            {
                _textBox.IsReadOnly = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _currentShape == null)
            {
                // Free draw mode
                Point currentPoint = e.GetPosition(DrawingCanvas);
                Line line = new Line
                {
                    Stroke = _currentBrush,
                    StrokeThickness = _brushSize,
                    X1 = _startPoint.X,
                    Y1 = _startPoint.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y
                };

                _drawingLayer.Children.Add(line);
                _startPoint = currentPoint;
            }
            else if (_isDrawing && _currentShape != null)
            {
                // Update shape size
                Point currentPoint = e.GetPosition(DrawingCanvas);
                if (_currentShape is Line line)
                {
                    line.X2 = currentPoint.X;
                    line.Y2 = currentPoint.Y;
                }
                else if (_currentShape is Rectangle rect)
                {
                    double x = Math.Min(_startPoint.X, currentPoint.X);
                    double y = Math.Min(_startPoint.Y, currentPoint.Y);
                    double width = Math.Abs(_startPoint.X - currentPoint.X);
                    double height = Math.Abs(_startPoint.Y - currentPoint.Y);

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    rect.Width = width;
                    rect.Height = height;
                }
                else if (_currentShape is Ellipse ellipse)
                {
                    double x = Math.Min(_startPoint.X, currentPoint.X);
                    double y = Math.Min(_startPoint.Y, currentPoint.Y);
                    double width = Math.Abs(_startPoint.X - currentPoint.X);
                    double height = Math.Abs(_startPoint.Y - currentPoint.Y);

                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                    ellipse.Width = width;
                    ellipse.Height = height;
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _currentShape = null;
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            _drawingLayer.Children.Clear();
        }

        private void ColorPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update brush color based on selection
            string selectedColor = (ColorPicker.SelectedItem as ComboBoxItem)?.Content.ToString();
            _currentBrush = selectedColor switch
            {
                "Red" => new SolidColorBrush(Colors.Red),
                "Blue" => new SolidColorBrush(Colors.Blue),
                "Green" => new SolidColorBrush(Colors.Green),
                "Yellow" => new SolidColorBrush(Colors.Yellow),
                "Purple" => new SolidColorBrush(Colors.Purple),
                "Orange" => new SolidColorBrush(Colors.Orange),
                "Pink" => new SolidColorBrush(Colors.Pink),
                "White" => new SolidColorBrush(Colors.White),
                "Brown" => new SolidColorBrush(Colors.Brown),
                "Gray" => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Black),
            };
        }

        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)DrawingCanvas.ActualWidth,
                                                                      (int)DrawingCanvas.ActualHeight,
                                                                      96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(DrawingCanvas);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files|*.png;*.jpeg;*.jpg;*.bmp",
                FileName = "Drawing"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    BitmapEncoder encoder = saveFileDialog.FileName.ToLower().EndsWith("png") ? new PngBitmapEncoder() :
                                            saveFileDialog.FileName.ToLower().EndsWith("jpg") || saveFileDialog.FileName.ToLower().EndsWith("jpeg") ? new JpegBitmapEncoder() :
                                            new BmpBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fileStream);
                }
            }
        }


        private void TextTool_Click(object sender, RoutedEventArgs e)
        {
            _isTextMode = true;
        }
    }
}
