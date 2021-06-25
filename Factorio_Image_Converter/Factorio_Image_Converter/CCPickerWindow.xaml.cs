using System.Windows;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

namespace Factorio_Image_Converter
{
    public partial class CCPickerWindow : Window
    {
        public Color resultColor;
        Color currentColor;
        List<Color> AvailableColors;

        public CCPickerWindow(List<Color> AvailableColors)
        {
            this.AvailableColors = AvailableColors;
            InitializeComponent();
            GenerateControls();
        }
        private void GenerateControls()
        {
            //TODO: Generate this only once and then somehow reference it when opening a new window
            int maxY = AvailableColors.Count / 6;
            int index = 0;
            bool stop = false;
            for(int y = 0; y <= maxY; y++)
            {
                Grid grid = new Grid();
                grid.Height = 40;
                for (int x = 0; x < 6; x++)
                {
                    ColumnDefinition column = new ColumnDefinition();
                    column.Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions.Add(column);

                    Button colorBtn = new Button();
                    colorBtn.SetValue(Grid.ColumnProperty, x);
                    colorBtn.Margin = new Thickness(3);
                    colorBtn.Background = new System.Windows.Media.SolidColorBrush(DrawingC2MediaC(AvailableColors[index]));
                    colorBtn.BorderBrush = System.Windows.Media.Brushes.Black;
                    colorBtn.BorderThickness = new Thickness(2);

                    grid.Children.Add(colorBtn);

                    Debug.Write(" - ");
                    if (index++ >= AvailableColors.Count - 1)
                    {
                        stop = true;
                        break;
                    }
                }
                stackPanel.Children.Add(grid);
                Debug.WriteLine(" ");
                if (stop)
                    break;
            }
        }
        private void btn_Confirm_Color(object sender, RoutedEventArgs e)
        {
            resultColor = Color.Red;
        }
        private System.Windows.Media.Color DrawingC2MediaC(System.Drawing.Color inputColor)
        {
            System.Windows.Media.Color newColor = new System.Windows.Media.Color();
            newColor = System.Windows.Media.Color.FromArgb(inputColor.A, inputColor.R, inputColor.G, inputColor.B);
            return newColor;
        }
    }
}
