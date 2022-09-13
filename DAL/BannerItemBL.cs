using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNSelfOrder.Models;

namespace SNSelfOrder.DAL
{
    public class BannerItemBL
    {
        public IBannerItemDAL bannerItemDAL;
        public string cnStr = "";
        public BannerItemBL(string conn,IBannerItemDAL bannerItemDAL)
        {
            cnStr = conn;
            this.bannerItemDAL = bannerItemDAL;
        }
        public ObservableCollection<BannerItem> GetAllBannerItems()
        {
            return bannerItemDAL.SelectAllBanners(cnStr);
        }

    }
}
