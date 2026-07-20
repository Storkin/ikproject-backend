namespace IkProjesi.Helpers;

public static class YardimciMetotlar
{
    public static string TarihFormatla(DateTime tarih)
    {
        return tarih.ToString("dd.MM.yyyy");
    }

    public static bool BosMu(string metin)
    {
        if (metin == null)
        {
            return true;
        }
        if (metin.Trim() == "")
        {
            return true;
        }
        return false;
    }

    public static int GunFarkiHesapla(DateTime baslangic, DateTime bitis)
    {
        TimeSpan fark = bitis - baslangic;
        int gunSayisi = fark.Days + 1;
        return gunSayisi;
    }

    public static string MaasFormatla(decimal maas)
    {
        return maas.ToString("N2") + " TL";
    }
}
