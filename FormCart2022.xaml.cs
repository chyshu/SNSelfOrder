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
    /// FormCart2022.xaml 的互動邏輯
    /// </summary>
    public partial class FormCart2022 : Window
    {
        private FormCart2022VM vm;

        public event EventHandler TimeOut;

        public FormCart2022VM FormCart2022ViewModel { get => vm; set { vm = value; this.DataContext = vm; } }
        public FormCart2022()
        {
            this.Resources = System.Windows.Application.Current.Resources;
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Loaded += FormCart2022_Loaded; ;
        }
 
        private void FormCart2022_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Interfaces.ICloseWindows vm)
            {
                vm.VMClose += (t) =>
                {
                    //Debug.WriteLine("FormCart2022 Closed");
                    if (t == TimeOutStatus.TimeOut)
                        TimeOut?.Invoke(this, new EventArgs());
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

        private void Grid_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
