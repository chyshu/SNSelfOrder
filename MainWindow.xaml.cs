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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SNSelfOrder.ViewModel;
 

namespace SNSelfOrder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM mainWindowVM;
        
        public MainWindow()
        {
            this.Resources = System.Windows.Application.Current.Resources;
            InitializeComponent();
            mainWindowVM = new MainWindowVM();

            this.DataContext = mainWindowVM;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            /*
            MainWindowVM Created
            MainWindow Created
            Window_IsVisibleChanged
            MainWindow_ContentRendered
             */

            Closing += MainWindow_Closing;
            ContentRendered += MainWindow_ContentRendered;
            Debug.WriteLine("MainWindow Created");

        }
        private bool firstTime = true;

        public bool FirstTime { get => firstTime; set => firstTime = value; }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow_ContentRendered");
            if (FirstTime)
            {
                FirstTime = false;

                await mainWindowVM.BootingProcedure();
            }
        }



        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // throw new NotImplementedException();
            Debug.WriteLine("MainWindow_Closing");
            mainWindowVM.DisposeAll();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainWindow_Loaded");
        }

        private void MainWindow_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
              e.Handled = true;
        }

        private void MainMenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("MainMenuListBox_SelectionChanged: " + (e.Source as ListBox).SelectedItem + "," + 
                ItemsListBox.Items.Count);
            (e.Source as ListBox).ScrollIntoView((e.Source as ListBox).SelectedItem);
            if (ItemsListBox.Items.Count > 0)
            {
                ItemsListBox.ScrollIntoView(ItemsListBox.Items[0]);
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            Point clickPoint = e.GetPosition(this);
            Debug.WriteLine("Window_PreviewMouseDown:" + " - Source: " + clickPoint);
            if ((mainWindowVM.DisplayStartup))
            {
                if ((clickPoint.X >= 0) && (clickPoint.X <= 150) && (clickPoint.Y <= 150)) { }
                else if ((clickPoint.X >= (1080 - 150)) && (clickPoint.Y <= 150)) { }
                else if ((clickPoint.X >= (1080 / 2 - 120)) && (clickPoint.X <= ((1080 / 2) + 120)) && (clickPoint.Y >= 280) && (clickPoint.Y <= 420)) { }
                else

                    mainWindowVM.StartTimeoutTimer();
            }
            else
            {
                mainWindowVM.StartTimeoutTimer();
            }*/
            // 
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("MainWindow_IsVisibleChanged");
        }

        private void EftCtrl_DisplayEvent(object sender, EventArgs e)
        {
            //  throw new NotImplementedException();
            Debug.WriteLine("EftCtrl_DisplayEvent");
        }

        private void EftCtrl_PrintReceiptEvent(object sender, AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEvent e)
        {
            // throw new NotImplementedException();
            Debug.WriteLine("EftCtrl_PrintReceiptEvent");
        }

        private void EftCtrl_TransactionEvent(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            Debug.WriteLine("EftCtrl_TransactionEvent");
        }

        private void EftCtrl_ReprintReceiptEvent(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            Debug.WriteLine("EftCtrl_ReprintReceiptEvent");
        }

        private void EftCtrl_GetLastTransactionEvent(object sender, EventArgs e)
        {
            /*
            DateTime ttime = DateTime.Now;
            Debug.WriteLine(" GetLastTransactionEven " + ttime.ToString("yyyyMMdd HHmmss"));
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+ "ResponseCode = " + this.eftCtrl.ResponseCode);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "ResponseText = " + this.eftCtrl.ResponseText);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "ResponseType = " + this.eftCtrl.ResponseType);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "Status = " + this.eftCtrl.ResponseCode);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "LastTransTxnRef = " + this.eftCtrl.TxnRef);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "AuthCode = " + this.eftCtrl.AuthCode);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "TransType = " + this.eftCtrl.TxnType);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "TxnSucess = " + this.eftCtrl.LastTxnSuccess);

            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "AccountType = " + this.eftCtrl.AccountType);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "CardType = " + this.eftCtrl.CardType);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "DateOfTxn = " + this.eftCtrl.Date);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "CardName = " + this.eftCtrl.CardName);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "Stan = " + this.eftCtrl.Stan);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "TimeOfTxn = " + this.eftCtrl.Time);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "DateExpiry = " + this.eftCtrl.DateExpiry);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "Rrn = " + this.eftCtrl.Rrn);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "PurchaseAnalysisData = " + this.eftCtrl.PurchaseAnalysisData);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "DataField = " + this.eftCtrl.DataField);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "Pan = " + this.eftCtrl.Pan);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "AmtCash = " + this.eftCtrl.AmtCash);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "AmtPurchase = " + this.eftCtrl.AmtPurchase);
            Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ")+  "Sucess = " + this.eftCtrl.Success);
            Debug.WriteLine("============================================================");
            */
        }

        private void EftCtrl_StatusEvent(object sender, EventArgs e)
        {
            Debug.WriteLine("EftCtrl_StatusEvent " + e);

        }

        private void eftCtrl_GetLastReceiptEvent(object sender, EventArgs e)
        {
            Debug.WriteLine("eftCtrl_GetLastReceiptEvent");
        }

        private void eftCtrl_DisplaySettlementEvent(object sender, EventArgs e)
        {
            Debug.WriteLine("eftCtrl_DisplaySettlementEvent");
        }

        private void eftCtrl_DisplayStatusEvent(object sender, EventArgs e)
        {
            Debug.WriteLine("eftCtrl_DisplayStatusEvent");
        }
        /*
        private DependencyObject GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
        private void MainMenuGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViwer = GetScrollViewer(sender as DependencyObject) as ScrollViewer;
            Debug.WriteLine(e.Delta);
            if (scrollViwer != null)
            {
                if (e.Delta < 0)
                {
                    scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset + 3);
                }
                else if (e.Delta > 0)
                {
                    scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset - 3);
                }
            }

        }*/
    }
}
