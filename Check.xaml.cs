using System.Windows;

namespace Chess
{
    public partial class Check : Window
    {
        public Check()
        {
            InitializeComponent();
        }

        private void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}