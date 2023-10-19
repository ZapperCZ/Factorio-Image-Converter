using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Linq;
using System.Windows.Media;

namespace Factorio_Image_Converter
{
    public partial class ResultWindow : INotifyPropertyChanged
    {
        string _blueprintString;
        private Dictionary<string, int> D_RequiredBlocks;   //Dictionary that contains how many blocks are present in the picture <block name, amount>

        public string BlueprintString
        {
            get { return _blueprintString; }
            set
            {
                if(_blueprintString != value)
                {
                    _blueprintString = value;
                    OnPropertyChanged();
                }
            }
        }
        public ResultWindow(string BlueprintString, Dictionary<string, int> D_RequiredBlocks)
        {
            DataContext = this;
            InitializeComponent();
            this.BlueprintString = BlueprintString;
            this.D_RequiredBlocks = D_RequiredBlocks;
            if(D_RequiredBlocks.Count > 0)
                GenerateControls();
        }
        private void GenerateControls()
        {
            int columnAmount = 5;
            int rowAmount = (D_RequiredBlocks.Count / columnAmount) + 1;
            int index = 0;
            bool stop = false;
            List <KeyValuePair<string, int>> SortedRequiredBlocks = D_RequiredBlocks.ToList();
            SortedRequiredBlocks.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            Grid grid = new Grid();

            //Generate rows
            for (int x = 0; x < rowAmount; x++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(48);                        //Height of the image, determined by experimentation to make the image look square
                grid.RowDefinitions.Add(row);
            }
            //Generate columns
            for (int y = 0; y < columnAmount; y++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1, GridUnitType.Star);    //Width of the button, changes with the width of the window
                grid.ColumnDefinitions.Add(column);
            }
            //Generate images
            for (int x = 0; x < rowAmount; x++)      //Row iteration
            {
                for (int y = 0; y < columnAmount; y++)     //Column iteration
                {
                    Image blockImage = new Image();
                    Border blockBorder = new Border();
                    TextPath blockAmountTest = new TextPath();

                    blockBorder.SetValue(Grid.RowProperty, x);
                    blockBorder.SetValue(Grid.ColumnProperty, y);
                    blockBorder.Margin = new Thickness(1);
                    blockImage.SetValue(Grid.RowProperty, x);
                    blockImage.SetValue(Grid.ColumnProperty, y);
                    blockImage.Margin = new Thickness(1);
                    blockAmountTest.SetValue(Grid.RowProperty, x);
                    blockAmountTest.SetValue(Grid.ColumnProperty, y);
                    blockAmountTest.HorizontalAlignment = HorizontalAlignment.Right;
                    blockAmountTest.VerticalAlignment = VerticalAlignment.Bottom;
                    blockAmountTest.Margin = new Thickness(0, 0, 6, 8);

                    blockBorder.BorderBrush = Brushes.Black;
                    blockBorder.BorderThickness = new Thickness(1);
                    //blockBorder.Background = new SolidColorBrush(Color.FromRgb(36, 36, 36));
                    blockImage.Source = new BitmapImage(new Uri("2-Resources/Icons/Factorio/" + SortedRequiredBlocks[index].Key + ".png", UriKind.Relative));

                    //blockAmountTest.FontFamily = new FontFamily("Ariel");
                    blockAmountTest.FontWeight = FontWeights.Bold;
                    blockAmountTest.Fill = Brushes.White;
                    blockAmountTest.Stroke = Brushes.Black;
                    blockAmountTest.FontSize = 14;
                    blockAmountTest.Text = SortedRequiredBlocks[index].Value.ToString();

                    blockBorder.Child = blockImage;
                    grid.Children.Add(blockBorder);
                    grid.Children.Add(blockAmountTest);

                    if (++index >= D_RequiredBlocks.Count)
                    {
                        stop = true;
                        break;
                    }
                }
                if (stop)
                    break;
            }
            stackPanel_Blocks.Children.Add(grid);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BlueprintString);
            MessageBox.Show("Blueprint copied into clipboard!");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
