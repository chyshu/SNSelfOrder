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

namespace SNSelfOrder
{
    /// <summary>
    /// Interaction logic for MsgWindow.xaml
    /// </summary>
    public partial class MsgWindow : Window
    {
        private MsgWindowVM vm;
        public MsgWindow()
        {
            InitializeComponent();
            vm = new MsgWindowVM();
            this.DataContext = vm;
        }
        public string SetCaption
        {
            set
            {
                vm.CaptionMessage = value;
            }
        }
        public string SetMessage
        {
            set
            {
                vm.MessageContext = value;
            }
        }
    }



    public class MsgWindowVM : ViewModel.ViewModelBase
    {
        private string captionMessage = "";
        private string messageContext = "";
        private bool canPay = true;

        public ICommand CloseCmd { get; set; }
        public string CaptionMessage { get => captionMessage; set { SetProperty(ref captionMessage, value); } }

        public string MessageContext { get => messageContext; set { SetProperty(ref messageContext, value); } }

        public bool CanPay { get => canPay; set { SetProperty(ref canPay, value); } }        

        private void CloseCmdAction()
        {
            Application.Current.Shutdown();
        }
        public MsgWindowVM()
        {
            this.CloseCmd = new DelegateCommand(CloseCmdAction);
        }
    }

}