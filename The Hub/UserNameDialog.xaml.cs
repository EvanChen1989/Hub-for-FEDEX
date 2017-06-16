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

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for UserNameDialog.xaml
    /// </summary>
    public partial class UserNameDialog : Window
    {
        public UserNameDialog()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
