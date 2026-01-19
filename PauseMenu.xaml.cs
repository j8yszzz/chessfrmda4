using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class PauseMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public PauseMenu()
        {
            InitializeComponent();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Continue);
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.BackToHome);
        }
    }
}