public class PrzekroczonyLimitException : Exception
{
    public PrzekroczonyLimitException(string wiadomosc) : base(wiadomosc) { }
}

public interface INiebezpieczenstwo
{
    void PowiadomNiebezpieczenstwo(string numerKontenera);
}

public abstract class Kontener
{
    public double MasaLadunku { get; protected set; }
    public double Wysokosc { get; }
    public double WagaWlasna { get; }
    public double Glebokosc { get; }
    public string NumerSeryjny { get; }
    public double MaksLadownosc { get; protected set; }
    public string TypProduktu { get; protected set; }

    private static int licznikKontenerow = 1;

    protected Kontener(double wysokosc, double wagaWlasna, double glebokosc, double maksLadownosc, string typKontenera)
    {
        Wysokosc = wysokosc;
        WagaWlasna = wagaWlasna;
        Glebokosc = glebokosc;
        MaksLadownosc = maksLadownosc;
        NumerSeryjny = $"KON-{typKontenera}-{licznikKontenerow++}";
        MasaLadunku = 0;
    }

    public virtual void Oproznij()
    {
        MasaLadunku = 0;
    }

    public virtual void Zaladuj(double masa, string typProduktu)
    {
        if (MasaLadunku > 0 && TypProduktu != typProduktu)
        {
            throw new InvalidOperationException("Kontener zawiera inny typ produktu");
        }

        if (masa > MaksLadownosc)
        {
            throw new PrzekroczonyLimitException($"Próba załadowania {masa}kg do kontenera {NumerSeryjny} o maksymalnej ładowności {MaksLadownosc}kg");
        }

        TypProduktu = typProduktu;
        MasaLadunku = masa;
    }

    public double PodajWage()
    {
        return WagaWlasna + MasaLadunku;
    }

    public override string ToString()
    {
        return $"Kontener {NumerSeryjny}: {GetType().Name}, Produkt: {TypProduktu}, Ładunek: {MasaLadunku}kg, Waga całkowita: {PodajWage()}kg";
    }
}

public class KontenerNaPlyny : Kontener, INiebezpieczenstwo
{
    public bool CzyNiebezpieczny { get; }

    public KontenerNaPlyny(double wysokosc, double wagaWlasna, double glebokosc, double maksLadownosc, bool czyNiebezpieczny) 
        : base(wysokosc, wagaWlasna, glebokosc, maksLadownosc, "L")
    {
        CzyNiebezpieczny = czyNiebezpieczny;
    }

    public override void Zaladuj(double masa, string typProduktu)
    {
        double maksymalnyLadunek = CzyNiebezpieczny ? MaksLadownosc * 0.5 : MaksLadownosc * 0.9;
        
        if (masa > maksymalnyLadunek)
        {
            PowiadomNiebezpieczenstwo(NumerSeryjny);
            throw new PrzekroczonyLimitException($"Nie można załadować {masa}kg do {(CzyNiebezpieczny ? "niebezpiecznego" : "zwykłego")} kontenera na płyny. Maksymalnie: {maksymalnyLadunek}kg");
        }

        base.Zaladuj(masa, typProduktu);
    }

    public void PowiadomNiebezpieczenstwo(string numerKontenera)
    {
        Console.WriteLine($"OSTRZEŻENIE: Niebezpieczna operacja na kontenerze z płynami {numerKontenera}");
    }
}

public class KontenerNaGaz : Kontener, INiebezpieczenstwo
{
    public double Cisnienie { get; }

    public KontenerNaGaz(double wysokosc, double wagaWlasna, double glebokosc, double maksLadownosc, double cisnienie) 
        : base(wysokosc, wagaWlasna, glebokosc, maksLadownosc, "G")
    {
        Cisnienie = cisnienie;
    }

    public override void Oproznij()
    {
        MasaLadunku *= 0.05;
    }

    public override void Zaladuj(double masa, string typProduktu)
    {
        if (masa > MaksLadownosc)
        {
            PowiadomNiebezpieczenstwo(NumerSeryjny);
            throw new PrzekroczonyLimitException($"Nie można załadować {masa}kg do kontenera na gaz. Maksymalnie: {MaksLadownosc}kg");
        }
        
        base.Zaladuj(masa, typProduktu);
    }

    public void PowiadomNiebezpieczenstwo(string numerKontenera)
    {
        Console.WriteLine($"OSTRZEŻENIE: Niebezpieczna operacja na kontenerze z gazem {numerKontenera}");
    }
}

public class KontenerChlodniczy : Kontener
{
    public double Temperatura { get; }
    private static readonly Dictionary<string, double> TemperaturyProduktow = new Dictionary<string, double>
    {
        {"Banany", 13.3},
        {"Czekolada", 18},
        {"Ryby", 2},
        {"Mięso", -15},
        {"Lody", -18},
        {"Mrożona pizza", -30},
        {"Ser", 7.2},
        {"Kiełbasa", 5},
        {"Masło", 20.5},
        {"Jajka", 19}
    };

    public KontenerChlodniczy(double wysokosc, double wagaWlasna, double glebokosc, double maksLadownosc, double temperatura) 
        : base(wysokosc, wagaWlasna, glebokosc, maksLadownosc, "C")
    {
        Temperatura = temperatura;
    }

    public override void Zaladuj(double masa, string typProduktu)
    {
        if (!TemperaturyProduktow.ContainsKey(typProduktu))
        {
            throw new ArgumentException($"Nieznany produkt: {typProduktu}");
        }

        double wymaganaTemperatura = TemperaturyProduktow[typProduktu];
        if (Temperatura < wymaganaTemperatura)
        {
            throw new InvalidOperationException($"Temperatura {Temperatura}C za niska dla {typProduktu} (wymagana {wymaganaTemperatura}C)");
        }

        if (masa > MaksLadownosc)
        {
            throw new PrzekroczonyLimitException($"Waga przekroczona o {masa}kg. Maksymalnie: {MaksLadownosc}kg");
        }

        base.Zaladuj(masa, typProduktu);
    }
}

public class StatekKontenerowy
{
    public List<Kontener> Kontenery { get; }
    public double MaksPredkosc { get; }
    public int MaksLiczbaKontenerow { get; }
    public double MaksWaga { get; }

    public StatekKontenerowy(double maksPredkosc, int maksLiczbaKontenerow, double maksWaga)
    {
        Kontenery = new List<Kontener>();
        MaksPredkosc = maksPredkosc;
        MaksLiczbaKontenerow = maksLiczbaKontenerow;
        MaksWaga = maksWaga;
    }

    public void ZaladujKontener(Kontener kontener)
    {
        if (Kontenery.Count >= MaksLiczbaKontenerow)
        {
            throw new InvalidOperationException("Statek pełny");
        }

        double calkowitaWaga = Kontenery.Sum(k => k.PodajWage()) + kontener.PodajWage();
        if (calkowitaWaga > MaksWaga * 1000)
        {
            throw new PrzekroczonyLimitException($"Przekroczono wagę: {MaksWaga} ton");
        }

        Kontenery.Add(kontener);
    }

    public void ZaladujKontenery(List<Kontener> kontenery)
    {
        foreach (var k in kontenery)
        {
            ZaladujKontener(k);
        }
    }

    public void UsunKontener(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            Kontenery.Remove(kontener);
        }
        else
        {
            throw new ArgumentException($"Brak kontenera: {numerSeryjny}");
        }
    }

    public void RozladujKontener(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            kontener.Oproznij();
        }
        else
        {
            throw new ArgumentException($"Brak kontenera:  {numerSeryjny}");
        }
    }

    public void ZamienKontener(string numerSeryjny, Kontener nowyKontener)
    {
        var index = Kontenery.FindIndex(k => k.NumerSeryjny == numerSeryjny);
        if (index >= 0)
        {
            var staryKontener = Kontenery[index];
            Kontenery.RemoveAt(index);
            
            try
            {
                ZaladujKontener(nowyKontener);
            }
            catch
            {
                Kontenery.Insert(index, staryKontener);
                throw;
            }
        }
        else
        {
            throw new ArgumentException($"Brak kontenera:  {numerSeryjny}");
        }
    }

    public static void PrzeniesKontener(StatekKontenerowy zStatku, StatekKontenerowy naStatek, string numerSeryjny)
    {
        var kontener = zStatku.Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener == null)
        {
            throw new ArgumentException($"Brak kontenera: {numerSeryjny}");
        }

        try
        {
            naStatek.ZaladujKontener(kontener);
            zStatku.UsunKontener(numerSeryjny);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Nie udało się przenieść kontenera: {ex.Message}");
        }
    }

    public void WypiszInfoKontenera(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            Console.WriteLine(kontener);
        }
        else
        {
            Console.WriteLine($"Brak kontenera:  {numerSeryjny}");
        }
    }

    public void WypiszInfoStatku()
    {
        Console.WriteLine($"Informacje o statku - Prędkość: {MaksPredkosc} węzłów, Maks kontenerów: {MaksLiczbaKontenerow}, Maks waga: {MaksWaga} ton");
        Console.WriteLine($"Aktualne obciążenie: {Kontenery.Count} kontenerów, Waga całkowita: {Kontenery.Sum(k => k.PodajWage()) / 1000} ton");
        Console.WriteLine("Kontenery na statku:");
        foreach (var k in Kontenery)
        {
            Console.WriteLine($"  {k}");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var kontenerPlyny1 = new KontenerNaPlyny(200, 500, 100, 2000, false);
            var kontenerPlyny2 = new KontenerNaPlyny(200, 500, 100, 2000, true);
            var kontenerGaz = new KontenerNaGaz(150, 400, 90, 1500, 2.5);
            var kontenerChlodniczy = new KontenerChlodniczy(220, 600, 120, 2500, 14);

            kontenerPlyny1.Zaladuj(1500, "Mleko");
            kontenerPlyny2.Zaladuj(800, "Paliwo");
            kontenerGaz.Zaladuj(1200, "Hel");
            kontenerChlodniczy.Zaladuj(2000, "Banany");

            try
            {
                kontenerPlyny1.Zaladuj(1000, "Mleko");
            }
            catch (PrzekroczonyLimitException ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }

            var statek1 = new StatekKontenerowy(20, 10, 50);
            var statek2 = new StatekKontenerowy(18, 8, 40);

            statek1.ZaladujKontener(kontenerPlyny1);
            statek1.ZaladujKontener(kontenerGaz);
            statek1.ZaladujKontener(kontenerChlodniczy);

            try
            {
                statek1.ZaladujKontener(kontenerPlyny2);
            }
            catch (PrzekroczonyLimitException ex)
            {
                Console.WriteLine($"Błąd ładowania: {ex.Message}");
            }

            statek1.WypiszInfoStatku();

            StatekKontenerowy.PrzeniesKontener(statek1, statek2, kontenerGaz.NumerSeryjny);

            Console.WriteLine("\nPo przeniesieniu:");
            statek1.WypiszInfoStatku();
            Console.WriteLine();
            statek2.WypiszInfoStatku();

            var nowyKontenerChlodniczy = new KontenerChlodniczy(220, 600, 120, 2500, 15);
            nowyKontenerChlodniczy.Zaladuj(1375, "Lody");
            statek1.ZamienKontener(kontenerChlodniczy.NumerSeryjny, nowyKontenerChlodniczy);

            Console.WriteLine("\nPo zamianie:");
            statek1.WypiszInfoStatku();

            statek2.RozladujKontener(kontenerGaz.NumerSeryjny);
            Console.WriteLine("\nPo rozładowaniu gazu:");
            statek2.WypiszInfoKontenera(kontenerGaz.NumerSeryjny);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}