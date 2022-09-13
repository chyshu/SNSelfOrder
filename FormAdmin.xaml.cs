using SNSelfOrder.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
    /// FormAdmin.xaml 的互動邏輯
    /// </summary>
    public partial class FormAdmin : Window
    {
        public FormAdmin()
        {
            InitializeComponent();

            this.Loaded += FormAdmin_Loaded; ;
        }

        private void FormAdmin_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Interfaces.ICloseWindows vm)
            {
                vm.VMClose += (t) =>
                {
                    if (t == TimeOutStatus.CLOSE_APP)
                    {
                        System.Windows.Application.Current.Shutdown();
                        this.Close();
                    }
                    else if (t == TimeOutStatus.SHUTDOWN)
                    {
                        ManagementBaseObject mboShutdown = null;
                        ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
                        mcWin32.Get();

                        // You can't shutdown without security privileges
                        mcWin32.Scope.Options.EnablePrivileges = true;
                        ManagementBaseObject mboShutdownParams =
                                 mcWin32.GetMethodParameters("Win32Shutdown");

                        // Flag 1 means we want to shut down the system. Use "2" to reboot.
                        mboShutdownParams["Flags"] = "1";
                        mboShutdownParams["Reserved"] = "0";
                        foreach (ManagementObject manObj in mcWin32.GetInstances())
                        {
                            mboShutdown = manObj.InvokeMethod("Win32Shutdown",
                                                           mboShutdownParams, null);
                        }
                        System.Windows.Application.Current.Shutdown();
                        this.Close();
                    }
                    else
                    {
                        this.Close();
                    }
                };   
                
            }
             
        }

        private void Grid_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void DatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            (DataContext as SNSelfOrder.ViewModel.FormAdminVM).CalendarCommand.Execute(null);
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine("MenuListBox_SelectionChanged: " + (e.Source as ListBox).SelectedItem + "," + ItemsListBox.Items.Count);
            (e.Source as ListBox).ScrollIntoView((e.Source as ListBox).SelectedItem);
            if (DataContext is Interfaces.ITimeOut vm)
                vm.StartIdleTimer();
            if (ItemsListBox.Items.Count > 0)
            {
                ItemsListBox.ScrollIntoView(ItemsListBox.Items[0]);
            }
        }
    }
}
