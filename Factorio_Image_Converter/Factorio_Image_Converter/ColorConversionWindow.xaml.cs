using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Factorio_Image_Converter
{
    public partial class ColorConversionWindow : Window
    {
        List<System.Drawing.Color> ImageColors;
        List<UBlock> AvailableBlocks;
        List<UTile> AvailableTiles;
        public ColorConversionWindow(List<System.Drawing.Color> ImageColors, List<UBlock> AvailableBlocks, List<UTile> AvailableTiles)
        {
            this.ImageColors = ImageColors;
            this.AvailableBlocks = AvailableBlocks;
            this.AvailableTiles = AvailableTiles;
            InitializeComponent();
            GenerateControls(ImageColors);
        }
        private void GenerateControls(List<System.Drawing.Color> SourceColors)
        {
            foreach(System.Drawing.Color color in SourceColors)
            {
                Debug.WriteLine("adding new control");
                #region Control_Creation
                Border mainBorder = new Border();
                Grid grid = new Grid();
                ColumnDefinition column0 = new ColumnDefinition();
                ColumnDefinition column1 = new ColumnDefinition();
                ColumnDefinition column2 = new ColumnDefinition();
                ColumnDefinition column3 = new ColumnDefinition();
                ColumnDefinition column4 = new ColumnDefinition();
                ColumnDefinition column5 = new ColumnDefinition();
                Border sourceColorBorder = new Border();
                Canvas sourceColorCanvas = new Canvas();
                TextBlock sourceColorHex = new TextBlock();
                System.Windows.Controls.Image arrowImage = new System.Windows.Controls.Image();
                Border resultColorBorder = new Border();
                Canvas resultColorCanvas = new Canvas();
                TextBlock resultBlockName = new TextBlock();
                Border resultBlockBorder = new Border();
                System.Windows.Controls.Image resultBlockImage = new System.Windows.Controls.Image();
                #endregion

                mainBorder.Padding = new Thickness(2);
                mainBorder.Margin = new Thickness(0, 2, 0, 2);
                mainBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
                mainBorder.BorderThickness = new Thickness(1);
                mainBorder.Child = grid;

                #region ColumnDefinition
                column0.Width = new GridLength(1, GridUnitType.Star);
                column1.Width = new GridLength(4.5, GridUnitType.Star);
                column2.Width = new GridLength(1, GridUnitType.Star);
                column3.Width = new GridLength(1, GridUnitType.Star);
                column4.Width = new GridLength(4, GridUnitType.Star);
                column5.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column0);
                grid.ColumnDefinitions.Add(column1);
                grid.ColumnDefinitions.Add(column2);
                grid.ColumnDefinitions.Add(column3);
                grid.ColumnDefinitions.Add(column4);
                grid.ColumnDefinitions.Add(column5);
                #endregion

                sourceColorBorder.SetValue(Grid.ColumnProperty, 0);
                sourceColorBorder.BorderBrush = System.Windows.Media.Brushes.Black;
                sourceColorBorder.BorderThickness = new Thickness(1);

                sourceColorCanvas.Background = new SolidColorBrush(DrawingC2MediaC(color));
                sourceColorBorder.Child = sourceColorCanvas;

                grid.Children.Add(sourceColorBorder);
                stackPanel.Children.Add(mainBorder);
            }
        }
        private System.Windows.Media.Color DrawingC2MediaC(System.Drawing.Color inputColor)
        {
            System.Windows.Media.Color newColor = new System.Windows.Media.Color();
            newColor = System.Windows.Media.Color.FromArgb(inputColor.A, inputColor.R, inputColor.G, inputColor.B);
            return newColor;
        }
    }
}
