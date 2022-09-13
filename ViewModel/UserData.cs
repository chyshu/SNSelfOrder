using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.ViewModel
{
    public class UserData : INotifyPropertyChanged
    {        

        private string mUserID;
        public string UserID
        {
            set
            {
                if (mUserID != value)
                {
                    mUserID = value;
                    OnPropertyChanged("UserID");
                }
            }
            get
            {
                return mUserID;
            }
        }

        private string mUserPassword;
        public string UserPassword
        {
            set
            {
                if (mUserPassword != value)
                {
                    mUserPassword = value;
                    OnPropertyChanged("UserPassword");
                }
            }
            get
            {
                return mUserPassword;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
