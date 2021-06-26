using System.Windows;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;

namespace Factorio_Image_Converter
{
    public partial class CCPickerWindow : Window
    {
        public string resultBlock;
        public bool isTile;
        string currentBlock;
        List<UBlock> AvailableBlocks;
        List<UTile> AvailableTiles;

        public CCPickerWindow(List<UBlock> AvailableBlocks, List<UTile> AvailableTiles)
        {
            this.AvailableBlocks = AvailableBlocks;
            this.AvailableTiles = AvailableTiles;
            currentBlock = "none";  //TODO: Bind BlockName TextBlock to this
            InitializeComponent();
            GenerateControls();
        }
        private void GenerateControls()
        {
            //TODO: Generate this only when opening the window for the first time and then somehow reference it when opening it again window
            int maxY = (AvailableBlocks.Count + AvailableTiles.Count) / 6;  //6 is the amount of colors in 1 row
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
                    colorBtn.Margin = new Thickness(1);
                    if(index < AvailableBlocks.Count)
                    {
                        colorBtn.Background = new System.Windows.Media.SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(AvailableBlocks[index].color)));
                        colorBtn.Name = "false0" + AvailableBlocks[index].name.Replace('-','_');       //header indicates if block is tile or not
                    }
                    else
                    {
                        colorBtn.Background = new System.Windows.Media.SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(AvailableTiles[index - AvailableBlocks.Count].color)));
                        colorBtn.Name = "true0" + AvailableTiles[index - AvailableBlocks.Count].name.Replace('-', '_');
                    }
                    colorBtn.BorderBrush = System.Windows.Media.Brushes.Black;
                    colorBtn.BorderThickness = new Thickness(2);
                    colorBtn.Click += new RoutedEventHandler(btn_Color_Click);

                    grid.Children.Add(colorBtn);

                    if (index++ >= AvailableBlocks.Count + AvailableTiles.Count - 1)
                    {
                        stop = true;
                        break;
                    }
                }
                stackPanel.Children.Add(grid);
                if (stop)
                    break;
            }
        }
        private void btn_Color_Click(object sender, RoutedEventArgs e)
        {
            //Find block corresponding to button color
            //Load block icon, name and color
            Button colorBtn = (Button)sender;
            string blockName = colorBtn.Name.Substring(colorBtn.Name.IndexOf('0')+1).Replace('_', '-');
            BlockColor.Background = colorBtn.Background;
            BlockIcon.Source = new BitmapImage(new Uri("2-Resources/Icons/Factorio/" + blockName + ".png", UriKind.Relative));
            BlockName.Text = blockName.Replace('-',' ');
            currentBlock = blockName;
            isTile = Convert.ToBoolean(colorBtn.Name.Substring(0, colorBtn.Name.IndexOf('0')));
        }
        private void btn_Confirm_Color(object sender, RoutedEventArgs e)
        {
            resultBlock = currentBlock;
            Close();
        }
        private System.Windows.Media.Color DrawingC2MediaC(Color inputColor)
        {
            System.Windows.Media.Color newColor = new System.Windows.Media.Color();
            newColor = System.Windows.Media.Color.FromArgb(inputColor.A, inputColor.R, inputColor.G, inputColor.B);
            return newColor;
        }
    }
}