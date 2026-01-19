using System.Windows;

namespace Chess
{
    public partial class Confirm : Window
    {
        public bool ForfeitConfirmed { get; private set; }

        public Confirm()
        {
            InitializeComponent();
            ForfeitConfirmed = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ForfeitConfirmed = false;
            DialogResult = false;
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ForfeitConfirmed = true;
            DialogResult = true;
            Close();
        }
    }
}