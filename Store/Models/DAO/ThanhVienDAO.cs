using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Store.Models.EF;

namespace Store.Models.DAO
{
    public class ThanhVienDAO
    {
        private DBStore db = null;
        public ThanhVienDAO()
        {
            db = new DBStore();
        }

        public int login(ThanhVien tv)
        {
            var bytes = Encoding.UTF8.GetBytes(tv.MatKhau);
            var hash = MD5.Create().ComputeHash(bytes);
            Convert.ToBase64String(hash);
            return db.ThanhViens.Where(x => x.TaiKhoan == tv.TaiKhoan && x.MatKhau == tv.MatKhau).Count();
        }

        public bool register(ThanhVien tv)
        {
            if (db.ThanhViens.Where(x => x.TaiKhoan == tv.TaiKhoan).Count() <= 0)
            {
                var bytes = Encoding.UTF8.GetBytes(tv.MatKhau);
                var hash = MD5.Create().ComputeHash(bytes);
                tv.MatKhau = Convert.ToBase64String(hash);
                db.Configuration.ValidateOnSaveEnabled = false;
                var id = db.ThanhViens.Count();
                tv.MaThanhVien = (id + 1).ToString();
                db.ThanhViens.Add(tv);
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}