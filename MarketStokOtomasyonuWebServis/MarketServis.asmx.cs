using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace MarketStokOtomasyonuWebServis
{
    /// <summary>
    /// Summary description for MarketServis
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class MarketServis : System.Web.Services.WebService
    {

        [WebMethod]
        public DataSet getUrunler()
        {
            string query = String.Format(@"Select u.barkod AS Barkod,u.urun_adi AS UrunAdi,k.kategori_adi AS KategoriAdi,s.stok_sayisi AS StokAdeti,f.fiyat AS BirimFiyat FROM urunler u
LEFT JOIN StokTakibi s ON u.urun_id=s.urun_id
LEFT JOIN Fiyat f ON u.urun_id=f.urun_id
LEFT JOIN Kategori k ON u.kategori_id = k.kategori_id;");
            return SorguCalistir(query);
        }

        [WebMethod]

        public DataSet getStokBiten()
        {
            string query = String.Format(@"Select urun_adi From Urunler WHERE urun_id IN (Select urun_id from StokTakibi WHERE stok_sayisi=0);");
            return SorguCalistir(query);
        }

        [WebMethod]

        public DataSet getStokAzalan()
        {
            string query = String.Format(@"Select urun_adi From Urunler WHERE urun_id IN (Select urun_id from StokTakibi WHERE stok_sayisi<=5 AND stok_sayisi>0);");
            return SorguCalistir(query);
        }

        [WebMethod]

        public DataSet getKategori()
        {
            string query = String.Format(@"select kategori_adi from Kategori;");
            return SorguCalistir(query);
        }

        /* [WebMethod] CLIENT TARAFINDA YAPILDI.

        public string getCountArama(string barkod, string urun_adi, string kategori_adi)
        {
            string query = String.Format(@"Select Count(*) AS KayitSayisi FROM urunler u
LEFT JOIN StokTakibi s ON u.urun_id=s.urun_id
LEFT JOIN Fiyat f ON u.urun_id=f.urun_id
LEFT JOIN Kategori k ON u.kategori_id = k.kategori_id
WHERE u.barkod LIKE '" + barkod +"%' AND u.urun_adi LIKE '" + urun_adi + "%' AND k.kategori_adi LIKE '" + kategori_adi + "%'");
            DataSet a = new DataSet();
            a = SorguCalistir(query);
            string sayi = a.Tables[0].Rows[0]["KayitSayisi"].ToString();
            return sayi;
        }*/

        [WebMethod]

        public DataSet postUrunSil(string barkod)
        {
            string query = String.Format(@"DELETE FROM Fiyat WHERE urun_id IN (Select urun_id from Urunler WHERE barkod = '" + barkod + "')" +
"DELETE FROM StokTakibi WHERE urun_id IN (Select urun_id from Urunler WHERE barkod = '" + barkod + "')" +
"DELETE FROM Urunler WHERE urun_id IN (Select urun_id from Urunler WHERE barkod = '" + barkod + "')");
            return SorguCalistir(query);
        }
        [WebMethod]

        public DataSet postUrun(string urun_barkod, int kategori_id, string urun_adi, int miktar, string fiyat)
        {
            string query = String.Format(@"INSERT INTO Urunler (barkod,urun_adi,kategori_id) VALUES ('" + urun_barkod + "','" + urun_adi + "'," + kategori_id + ")"


              + "INSERT INTO StokTakibi (urun_id,stok_sayisi) VALUES ((Select TOP 1 urun_id from Urunler WHERE barkod='" + urun_barkod + "')," + miktar + ")" +

             "INSERT INTO Fiyat (urun_id,fiyat) VALUES ((Select TOP 1 urun_id from Urunler WHERE barkod='" + urun_barkod + "')," + (fiyat.Replace(",", ".")) + ")"); // string fiyatı burada replace ettik virgülü noktaya dönüştürdük.

            return SorguCalistir(query);
        }

        [WebMethod]

        public DataSet postUrunGuncelle(string urun_barkod, int kategori_id, string urun_adi, int miktar, string fiyat)
        {
            string query = String.Format(@"UPDATE Urunler SET urun_adi='" + urun_adi + "', kategori_id=" + kategori_id + " WHERE barkod ='" + urun_barkod + "'"


              + "UPDATE StokTakibi SET stok_sayisi=" + miktar + "WHERE urun_id IN (Select TOP 1 urun_id from Urunler WHERE barkod='" + urun_barkod + "')" +

             "UPDATE Fiyat SET fiyat=" + fiyat.Replace(",", ".") + "WHERE urun_id IN (Select TOP 1 urun_id from Urunler WHERE barkod='" + urun_barkod + "')");


            return SorguCalistir(query);
        }


        [WebMethod]

        public DataSet getArama(string barkod, string urun_adi, string kategori_adi)
        {
            string query = String.Format(@"Select u.barkod AS Barkod,u.urun_adi AS UrunAdi,k.kategori_adi AS KategoriAdi,s.stok_sayisi AS StokAdeti,f.fiyat AS BirimFiyat FROM urunler u
LEFT JOIN StokTakibi s ON u.urun_id=s.urun_id
LEFT JOIN Fiyat f ON u.urun_id=f.urun_id
LEFT JOIN Kategori k ON u.kategori_id = k.kategori_id
WHERE u.barkod LIKE '" + barkod + "%' OR u.urun_adi LIKE '" + urun_adi + "%' OR k.kategori_adi LIKE '" + kategori_adi + "%'");

            return SorguCalistir(query);
        }


        /*[WebMethod] CLIENT TARAFINDA YAPILDI.
         * 
        public string getCount()
        {
            string query = String.Format(@"select Count(*) AS sayi from urunler;");
            DataSet a = new DataSet();
            a = SorguCalistir(query);
            string sayi = a.Tables[0].Rows[0]["sayi"].ToString();
            return sayi;
        }*/

        [WebMethod]
        public DataSet SorguCalistir(string query)
        {
            SqlConnection con = new SqlConnection("server =.;database=StokTakibi;integrated security=true"); // SQL bağlantı cümlesini con değişkenine atıyoruz.
            con.Open(); // Connection nesnesini açıyoruz.
            SqlDataAdapter da = new SqlDataAdapter(query, con); // SQL data adapter sınıfından bir nesne üretiyoruz ve sql komutunu çalıştıryoruz.
            //da.SelectCommand.ExecuteNonQuery(); // Burada yaptığım hata sql komutunu tekrar çalıştırmak, çalıştırdığım sql komutunu burada tekrar çalıştırdığım için girdiğim sorgu 2 kere çalışıyor.
            DataSet dt = new DataSet(); // Dataset nesnesi oluşturuyoruz.
            da.Fill(dt); // Dataadapter ile dataset'imizi dolduruyoruz.
            return dt; // dataseti metoda döndürüyoruz.
        }

        [WebMethod]
        public DataSet getTedarikciler()
        {
            string query = String.Format(@"Select tedarikci_adi AS TedarikciAdi, tedarikci_adres AS TedarikciAdres, tedarikci_telno AS TedarikciTelNumarasi from Tedarikciler");
            return SorguCalistir(query);
        }

        [WebMethod]
        public DataSet getTedarikciArama(string tedarikciAdi, string tedarikciTel)
        {
            string query = String.Format(@"Select tedarikci_adi AS TedarikciAdi, tedarikci_adres AS TedarikciAdres, tedarikci_telno AS TedarikciTelNumarasi from Tedarikciler WHERE tedarikci_adi LIKE '" + tedarikciAdi + "%' OR tedarikci_telno LIKE '" + tedarikciTel + "%'");
            return SorguCalistir(query);
        }

        [WebMethod]
        public DataSet postTedarikciGuncelle(string tedarikciAdi, string tedarikciAdres, string tedarikciTel)
        {
            string query = String.Format(@"UPDATE Tedarikciler SET tedarikci_adi='" + tedarikciAdi + "', tedarikci_adres='" + tedarikciAdres + "', tedarikci_telno='" + tedarikciTel + "' WHERE tedarikci_id IN(Select tedarikci_id from tedarikciler WHERE tedarikci_telno='" + tedarikciTel + "')");
            return SorguCalistir(query);
        }

        [WebMethod]

        public DataSet postTedarikci(string tedarikciAdi, string tedarikciAdres, string tedarikciTel)
        {
            string query = String.Format(@"Insert Into Tedarikciler (tedarikci_adi,tedarikci_adres,tedarikci_telno) VALUES ('" + tedarikciAdi + "','" + tedarikciAdres + "','" + tedarikciTel + "')");
            return SorguCalistir(query);
        }

    }
}
