using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Factorio_Image_Converter
{
    public partial class ResultWindow : INotifyPropertyChanged
    {
        string _blueprintString;
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
        public ResultWindow(string BlueprintString)
        {
            DataContext = this;
            InitializeComponent();
            this.BlueprintString = BlueprintString;
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
