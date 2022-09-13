using SNSelfOrder.Interfaces;
using SNSelfOrder.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SNSelfOrder
{
    /// <summary>
    /// FormItem2022.xaml 的互動邏輯
    /// </summary>
    public partial class FormItem2022 : Window
    {
        private FormItem2022VM vm;

        public event EventHandler TimeOut;
        public FormItem2022VM FormItem2022ViewModel { get => vm; set { vm = value;  this.DataContext = vm; } }
        public FormItem2022()
        {
            this.Resources = System.Windows.Application.Current.Resources;
            InitializeComponent();
            // vm = new FormItem2022VM(); 
            // this.DataContext = vm;          
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Loaded += FormItem2022_Loaded;
        }

        private void FormItem2022_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Interfaces.ICloseWindows vm)
            {
                vm.VMClose += (t) =>
                {
                    if(t== TimeOutStatus.TimeOut)
                       TimeOut? .Invoke(this, new EventArgs());
                    this.Close();
                };
                Closing += (s, e) =>
                {
                    e.Cancel = !vm.CanClose();
                };
            }
            if (DataContext is Interfaces.ITimeOut vm1)
            {

                vm1.CountDown = PRPosUtils.AlterDisplayTime;
                vm1.StartIdleTimer();
            }
          
        }
 

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //vm.OnPropertyChanged("");
        }

        private void Grid_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
 
    }
}
