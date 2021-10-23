using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.Models.DAO;
using Store.Models.EF;
namespace Store.Controllers
{
    public class CartController : BaseController
    {
        DBStore db = new DBStore();
        // GET: Cart
        public List<CartItem> GetCart()
        {
            List<CartItem> LstCart = Session["Cart"] as List<CartItem>;
            if(LstCart==null)
            {
                LstCart = new List<CartItem>();
                Session["Cart"] = LstCart;
                return LstCart;
            }
            return LstCart;
        }
        public ActionResult AddToCart(string id,string cfid, string url)
        {
            //Kiểm tra sản phẩm
            var sp = new ProductDAO().GetById(id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Lấy giỏ hàng
            List<CartItem> LstCart = GetCart();
            //Nếu sản phẩm đã tồn tại
            CartItem CartCheck = LstCart.SingleOrDefault(n => n.masp == id && n.mach==cfid);
            if (CartCheck != null)
            {
                if (sp.SoLuongTon < CartCheck.soluong)
                {
                    SetAlert("Số Lượng Sản Phẩm Không Đủ", "warning");
                }
                CartCheck.soluong++;
                CartCheck.thanhtien = CartCheck.soluong * CartCheck.dongia;
                return Redirect(url);
            }
            
            CartItem itemcart = new CartItem(id,cfid);
            if (sp.SoLuongTon < itemcart.soluong) SetAlert("Số Lượng Sản Phẩm Không Đủ", "warning"); ;
                LstCart.Add(itemcart);
            return Redirect(url);
        }
        [HttpGet]
        public ActionResult UpdateCart(string id, string cfid)
        {
            if(Session["Cart"]==null)
            {
                return RedirectToAction("Index", "Home");
            }
            var sp = new ProductDAO().GetById(id);
            if(sp==null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<CartItem> LstCart = GetCart();
            CartItem CartCheck = LstCart.SingleOrDefault(n => n.masp == id && n.mach == cfid);
            if (CartCheck == null) return RedirectToAction("Index", "Home");
            ViewBag.Cart = LstCart;
            return View(CartCheck);
        }
        [HttpPost]
        public ActionResult UpdateCart(CartItem cartItem)
        {
            var sp = new ProductDAO().GetById(cartItem.masp);
            if (sp.SoLuongTon < cartItem.soluong)
            {
                return null;
            }
            List<CartItem> LstCart = GetCart();
            CartItem ItemUpdate = LstCart.Find(n => n.masp == cartItem.masp && n.mach == cartItem.mach);
            ItemUpdate.soluong = cartItem.soluong;
            if (ItemUpdate.soluong == 0) DeleteCart(ItemUpdate.masp, ItemUpdate.mach);
            ItemUpdate.thanhtien = ItemUpdate.soluong * ItemUpdate.dongia;
            return RedirectToAction("Index");
        }
        public ActionResult DeleteCart(string id,string cfid)
        {
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var sp = new ProductDAO().GetById(id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            List<CartItem> LstCart = GetCart();
            CartItem CartCheck = LstCart.SingleOrDefault(n => n.masp == id && n.mach == cfid);
            if (CartCheck == null) return RedirectToAction("Index", "Home");
            LstCart.Remove(CartCheck);
            return RedirectToAction("Index");
        }
        public ActionResult Order(DonDatHang order,KhachHang customer)
        {
            if (Session["Cart"] == null)
                return RedirectToAction("Index", "Home");
            KhachHang Customer = new KhachHang();
            if(Session["Customer"]==null)
            {
                Customer = customer;
                Customer.MaKH = "KH" + (string)DateTime.Now.ToString("hh:mm:ss");
                db.KhachHangs.Add(Customer);
                db.SaveChanges();
            }
            else
            {
                ThanhVien mem = Session["Customer"] as ThanhVien;
                Customer.TenKH = mem.HoTen;
                Customer.DiaChi = mem.DiaChi;
                Customer.Email = mem.Email;
                Customer.SoDienThoai = mem.SoDienThoai;
                db.KhachHangs.Add(Customer);
                db.SaveChanges();
            }
            order.MaKH = Customer.MaKH;
            var dao = new OrderDAO().Order(order);
            List<CartItem> LstCart = GetCart();
            if(dao)
            {
                foreach (var item in LstCart)
                {
                    ChiTietDonDatHang dtorder = new ChiTietDonDatHang();
                    dtorder.MaCTDDH = new CommonDAO().lastid();
                    
                    dtorder.MaDDH = order.MaDDH;
                    dtorder.MaSP = item.masp;
                    dtorder.TenSP = item.tensp;
                    dtorder.SoLuong = item.soluong;
                    dtorder.DonGia = item.dongia;
                    db.ChiTietDonDatHangs.Add(dtorder);
                    db.SaveChanges();
                } 
            }
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }
        public double Total()
        {
            List<CartItem> LstCart = Session["Cart"] as List<CartItem>;
            if (LstCart == null)
            {
                return 0;
            }
            return LstCart.Sum(n => n.soluong);
        }
        public decimal TotalMoney()
        {
            List<CartItem> LstCart = Session["Cart"] as List<CartItem>;
            if (LstCart == null)
            {
                return 0;
            }
            return LstCart.Sum(n => n.thanhtien);
        }
        public ActionResult Index()
        {
            List<CartItem> cart = GetCart();
            return View(cart);
        }
    }
}