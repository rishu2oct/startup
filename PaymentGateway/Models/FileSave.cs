using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentGateway.Models
{
    public static class FileSave
    {
        public static void FileSaveData(string path, string data)
        {

            System.IO.StreamWriter SW = null;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                SW = System.IO.File.CreateText(path);
                SW.WriteLine(data);
            }
            catch (Exception ex)
            {

                SW = System.IO.File.CreateText(path);
                SW.WriteLine(ex.Message);
            }
            finally
            {


                SW.Flush();
                SW.Dispose();
                SW.Close();

            }

        }
    }
}