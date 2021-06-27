using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Factorio_Image_Converter
{
    public partial class ColorConversionWindow : Window
    {
        List<System.Drawing.Color> ImageColors;
        List<UBlock> AvailableBlocks;
        List<UTile> AvailableTiles;
        public Dictionary<string, string> D_colorConversion = new Dictionary<string, string>();
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
            //TODO: Load colors that are already present in the dictionary
            foreach(System.Drawing.Color color in SourceColors)
            {
                //Debug.WriteLine("adding new control");
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
                Button resultColorButton = new Button();
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

                sourceColorHex.SetValue(Grid.ColumnProperty, 1);
                sourceColorHex.Text = ColorTranslator.ToHtml(color);
                sourceColorHex.Padding = new Thickness(8, 4, 8, 4);
                sourceColorHex.Name = "SourceColor";

                arrowImage.SetValue(Grid.ColumnProperty,2);
                arrowImage.Source = new BitmapImage(new Uri("2-Resources/Icons/General/arrow.png", UriKind.Relative));

                resultColorButton.BorderBrush = System.Windows.Media.Brushes.Black;
                resultColorButton.BorderThickness = new Thickness(1.1);     //because for some reason 1 for button thickness isn't the same as 1 for border thickness
                resultColorButton.SetValue(Grid.ColumnProperty, 3);
                resultColorButton.Background = System.Windows.Media.Brushes.White;
                resultColorButton.Click += new RoutedEventHandler(btn_ColorPick);
                resultColorButton.Name = "ResultColor";

                resultBlockName.SetValue(Grid.ColumnProperty, 4);
                resultBlockName.Text = "none";
                resultBlockName.Padding = new Thickness(8, 4, 8, 4);
                resultBlockName.Name = "ResultName";

                resultBlockBorder.SetValue(Grid.ColumnProperty, 5);
                resultBlockBorder.Padding = new Thickness(1);
                resultBlockBorder.BorderBrush = System.Windows.Media.Brushes.Black;
                resultBlockBorder.BorderThickness = new Thickness(1);

                resultBlockImage.Source = new BitmapImage(new Uri("2-Resources/Icons/General/white.png", UriKind.Relative));
                resultBlockBorder.Child = resultBlockImage;
                resultBlockBorder.Name = "ResultIconBorder";

                grid.Children.Add(sourceColorBorder);
                grid.Children.Add(sourceColorHex);
                grid.Children.Add(arrowImage);
                grid.Children.Add(resultColorButton);
                grid.Children.Add(resultBlockName);
                grid.Children.Add(resultBlockBorder);
                stackPanel.Children.Add(mainBorder);
            }
        }
        private void btn_ColorPick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Grid grid = (Grid)btn.Parent;
            string resultNameString = "";
            string sourceColorHex = "";
            string resultColorHex = "";

            Button resultColor = new Button();
            TextBlock sourceColor = new TextBlock();
            TextBlock resultName = new TextBlock();
            System.Windows.Controls.Image resultIcon = new System.Windows.Controls.Image();

            foreach(FrameworkElement control in grid.Children)
            {
                UIElement element = (UIElement)control;
                if (control.Name == "ResultColor")
                {
                    resultColor = (Button)element;
                }
                else if (control.Name == "ResultName")
                {
                    resultName = (TextBlock)element;
                }
                else if(control.Name == "ResultIconBorder")
                {
                    Border b = (Border)element;
                    resultIcon = (System.Windows.Controls.Image)b.Child;
                }
                else if(control.Name == "SourceColor")
                {
                    sourceColor = (TextBlock)element;
                }
            }

            sourceColorHex = sourceColor.Text.ToLower();

            CCPickerWindow CCPicker = new CCPickerWindow(AvailableBlocks, AvailableTiles);
            CCPicker.ShowDialog();
            
            //FIX: Crash when no color selected
            if (!CCPicker.isTile)   //is block
            {
                UBlock uBlock = AvailableBlocks.Find(block => block.name == CCPicker.resultBlock);
                resultColorHex = uBlock.color;
                resultColor.Background = new SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(uBlock.color)));
            }
            else
            {
                UTile uTile = AvailableTiles.Find(tile => tile.name == CCPicker.resultBlock);
                resultColorHex = uTile.color;
                resultColor.Background = new SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(uTile.color)));
            }
            resultNameString = CCPicker.resultBlock;

            if (resultNameString.Contains("left"))
            {
                resultNameString = resultNameString.Substring(0, resultNameString.IndexOf("left") - 1);
            }
            resultName.Text = resultNameString.Replace('-',' ');
            resultIcon.Source = new BitmapImage(new Uri("2-Resources/Icons/Factorio/" + CCPicker.resultBlock + ".png", UriKind.Relative));

            if (!D_colorConversion.ContainsKey(sourceColorHex))
            {
                D_colorConversion.Add(sourceColorHex, resultColorHex);
            }
            else
            {
                //Overwrite existing definition
                D_colorConversion.Remove(sourceColorHex);
                D_colorConversion.Add(sourceColorHex, resultColorHex);
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
