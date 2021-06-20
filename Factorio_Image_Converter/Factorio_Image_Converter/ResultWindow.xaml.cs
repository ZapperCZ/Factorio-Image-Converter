using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Factorio_Image_Converter
{
    public partial class ResultWindow : Window
    {
        string BlueprintString;
        public ResultWindow(string BlueprintString)
        {
            InitializeComponent();
            this.BlueprintString = BlueprintString;
            BlueprintOutput.Text = BlueprintString;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BlueprintString);
            MessageBox.Show("Blueprint copied into clipboard!");
        }
    }
}
