using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNSelfOrder.Models;

namespace SNSelfOrder.DAL
{
    public interface IBannerItemDAL
    {
        ObservableCollection<BannerItem> SelectAllBanners(string connstring);
    }
    public class BannerItemDAL: IBannerItemDAL
    {
   
        public ObservableCollection<BannerItem> SelectAllBanners(string connstring)
        {
            //Get the BannerItems from the Database
            ObservableCollection<BannerItem> ret = new ObservableCollection<BannerItem>();
            using (SQLiteConnection connection = new SQLiteConnection(connstring))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
            }
             return ret;
        }
    }
}
