using SNSelfOrder.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SNSelfOrder.ViewModel
{
    public class ConnectionCodeVM : ViewModelBase, IInputClose
    {
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        private string connectionCode = "";

        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }

        private bool windowIsVisible = true;

        private ICommand buttonCommand;

        private string displayMessage = "";
        public bool WindowIsVisible
        {
            get { return this.windowIsVisible; }
            set
            {
                SetProperty(ref windowIsVisible, value);
            }
        }

        public string ConnectionCode { get => connectionCode; set { SetProperty(ref connectionCode, value); } }

        public ICommand ButtonCommand => buttonCommand ?? (buttonCommand = new DelegateCommand<string>(ButtonCommandAction));

        public Action VMClose { get; set; }
        public Action<object> InputClose { get; set; }
        public string DisplayMessage { get => displayMessage; set { SetProperty(ref displayMessage, value); } }

        public bool CanClose()
        {
            return (ConnectionCode.Length > 0);
        }
        public ConnectionCodeVM()
        {

        }
        /**/
        private  void ButtonCommandAction(string str)
        {
            // Debug.WriteLine("ButtonCommandAction "+str);
            if (str == "C")
                ConnectionCode = "";
            else if (str == "B")
            {
                if (ConnectionCode.Length > 0)
                {
                    ConnectionCode = ConnectionCode.Substring(0, ConnectionCode.Length - 1);
                }
            }
            else if (str == "E")
            {
                if(CanClose())
                    InputClose?.Invoke(this.ConnectionCode);
            }
            else 
            {
                ConnectionCode += str;
            }
        }


    }
}
