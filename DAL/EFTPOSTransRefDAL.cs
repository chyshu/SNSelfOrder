using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.DAL
{

    public interface IEFTPOSTransRefDAL
    {
        EFTPOSTransRef GetLastReference();
        int SetLastReference(string RefNo);
    }
    
    public class EFTPOSTransRef
    {
        private string lastReference = "";

        public string LastReference { get => lastReference; set => lastReference = value; }
    }
    public class EFTPOSTransRefDAL: IEFTPOSTransRefDAL
    {
        string connstring = "";
        public EFTPOSTransRefDAL(string constr)
        {
            connstring = constr;
        }
        public EFTPOSTransRef GetLastReference()
        {
            EFTPOSTransRef ret = new EFTPOSTransRef();
            using (SQLiteConnection connection = new SQLiteConnection(connstring))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"select lastReference from EFTPOSTransRef  where sid=1";
                DataTable EFTPOSTransRefDT = new DataTable();
                da.Fill(EFTPOSTransRefDT);
                if (EFTPOSTransRefDT.Rows.Count > 0)
                {
                    DataRow EFTPOSTransRef = EFTPOSTransRefDT.Rows[0];
                    ret.LastReference = EFTPOSTransRef["lastReference"].ToString();
                }
            }
            return ret;
        }

        public int  SetLastReference(  string RefNo)
        {
            int ret = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connstring))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"update  EFTPOSTransRef set  lastReference=@lastReference   where sid=1";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("lastReference", RefNo);
                ret =cmd.ExecuteNonQuery();
            }
            return ret;
        }
        
    }
}
