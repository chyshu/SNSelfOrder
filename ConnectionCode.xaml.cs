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
using SNSelfOrder.Interfaces;
using SNSelfOrder.ViewModel;


namespace SNSelfOrder
{
     
    public partial class ConnectionCode : Window
    {
        
        public ConnectionCode()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Loaded += ConnectionCode_Loaded; ; ;
        }

        private void ConnectionCode_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Interfaces.ICloseWindows vm)
            {
                vm.VMClose += (t) =>
                {
       
                    this.Close();
                };
                Closing += (s, e) =>
                {
                    e.Cancel = !vm.CanClose();
                };
            }
        }
    }
}
