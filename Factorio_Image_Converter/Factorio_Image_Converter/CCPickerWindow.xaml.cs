using System.Windows;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Factorio_Image_Converter
{
    public partial class CCPickerWindow : INotifyPropertyChanged
    {
        public string resultBlock;
        public bool isTile;
        string _currentBlock = "none";
        List<UBlock> AvailableBlocks;
        List<UTile> AvailableTiles;

        public string CurrentBlock
        {
            get { return _currentBlock.Replace('-',' '); }
            set 
            {
                _currentBlock = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CCPickerWindow(List<UBlock> AvailableBlocks, List<UTile> AvailableTiles)
        {
            DataContext = this;
            this.AvailableBlocks = AvailableBlocks;
            this.AvailableTiles = AvailableTiles;
            InitializeComponent();
            GenerateControls();
        }
        private void GenerateControls()
        {
            //TODO: Generate this only when opening the window for the first time and then somehow reference it when opening it again window
            //TODO: Add the original color for easier orientation

            int columnAmount = 6;
            int rowAmount = ((AvailableBlocks.Count + AvailableTiles.Count) / columnAmount) + 1;  //Amount of rows, 6 is the amount of colors in 1 row
            int index = 0;
            bool stop = false;
            Grid grid = new Grid();

            //Generate rows
            for(int x = 0; x < rowAmount; x++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(40);                        //Height of the button, determined by experimentation to make the button look square
                grid.RowDefinitions.Add(row);
            }
            //Generate columns
            for (int y = 0; y < columnAmount; y++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1, GridUnitType.Star);    //Width of the button, changes with the width of the window
                grid.ColumnDefinitions.Add(column);
            }
            //Generate controls
            for (int x = 0; x < rowAmount; x++)      //Row iteration
            {
                for (int y = 0; y < columnAmount; y++)      //Column iteration
                {
                    Button colorBtn = new Button();
                    colorBtn.SetValue(Grid.RowProperty, x);
                    colorBtn.SetValue(Grid.ColumnProperty, y);
                    colorBtn.Margin = new Thickness(1);
                    if(index < AvailableBlocks.Count)
                    {
                        colorBtn.Background = new System.Windows.Media.SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(AvailableBlocks[index].color)));
                        colorBtn.Name = "false0" + AvailableBlocks[index].name.Replace('-','_');       //header indicates if block is tile or not, 0 is the separator
                    }
                    else
                    {
                        colorBtn.Background = new System.Windows.Media.SolidColorBrush(DrawingC2MediaC(ColorTranslator.FromHtml(AvailableTiles[index - AvailableBlocks.Count].color)));
                        colorBtn.Name = "true0" + AvailableTiles[index - AvailableBlocks.Count].name.Replace('-', '_');
                    }
                    colorBtn.BorderBrush = System.Windows.Media.Brushes.Black;
                    colorBtn.BorderThickness = new Thickness(2);
                    colorBtn.Click += new RoutedEventHandler(btn_Color_Click);
                    colorBtn.MouseDoubleClick += new MouseButtonEventHandler(btn_Confirm_Color);
                    colorBtn.MouseEnter += new MouseEventHandler(btn_Color_Click);

                    grid.Children.Add(colorBtn);

                    if (++index >= AvailableBlocks.Count + AvailableTiles.Count)
                    {
                        stop = true;
                        break;
                    }
                }
                if (stop)
                    break;
            }
            stackPanel.Children.Add(grid);
        }
        private void btn_Color_Click(object sender, RoutedEventArgs e)
        {
            //Find block corresponding to button color
            //Load block icon, name and color
            Button colorBtn = (Button)sender;
            string blockName = colorBtn.Name.Substring(colorBtn.Name.IndexOf('0')+1).Replace('_', '-');
            BlockColor.Background = colorBtn.Background;
            BlockIcon.Source = new BitmapImage(new Uri("2-Resources/Icons/Factorio/" + blockName + ".png", UriKind.Relative));
            CurrentBlock = blockName;
            isTile = Convert.ToBoolean(colorBtn.Name.Substring(0, colorBtn.Name.IndexOf('0')));
        }
        private void btn_Confirm_Color(object sender, RoutedEventArgs e)
        {
            resultBlock = CurrentBlock.Replace(' ','-');
            Close();
        }
        private System.Windows.Media.Color DrawingC2MediaC(Color inputColor)    //Converts Drawing Color to Media Color
        {
            System.Windows.Media.Color newColor;
            newColor = System.Windows.Media.Color.FromArgb(inputColor.A, inputColor.R, inputColor.G, inputColor.B);
            return newColor;
        }
    }
}