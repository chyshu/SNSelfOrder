using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.ViewModel
{
    /*
     *  this is class common for all your viewmodels. 
     *  move all common logic to this class.
     */

    public abstract  class ViewModelBase : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T origValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(origValue, newValue))
            {
                return false;
            }

            origValue = newValue;
            OnPropertyChanged(propertyName);           
            return true;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                // Debug.WriteLine("---->"+propertyName);
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
      

        public virtual void BeforeDisplay()
        {
            OnPropertyChanged("");
        }      
    }
     
}
