
public class Rates
{
    public double AUD { get; set; }
    public double BGN { get; set; }
    public double BRL { get; set; }
    public double CAD { get; set; }
    public double CHF { get; set; }
    public double CNY { get; set; }
    public double CZK { get; set; }
    public double DKK { get; set; }
    public double GBP { get; set; }
    public double HKD { get; set; }
    public double HRK { get; set; }
    public double HUF { get; set; }
    public double IDR { get; set; }
    public double ILS { get; set; }
    public double INR { get; set; }
    public double JPY { get; set; }
    public double KRW { get; set; }
    public double MXN { get; set; }
    public double MYR { get; set; }
    public double NOK { get; set; }
    public double NZD { get; set; }
    public double PHP { get; set; }
    public double PLN { get; set; }
    public double RON { get; set; }
    public double RUB { get; set; }
    public double SEK { get; set; }
    public double SGD { get; set; }
    public double THB { get; set; }
    public double TRY { get; set; }
    public double USD { get; set; }
    public double ZAR { get; set; }

    public double getRate(string name)
    {
        switch (name) {
            case "AUD": return AUD; break;
            case "BGN": return BGN; break;
            case "BRL": return BRL; break;
            case "CAD": return CAD; break;
            case "CHF": return CHF; break;
            case "CNY": return CNY; break;
            case "CZK": return CZK; break;
            case "DKK": return DKK; break;
            case "GBP": return GBP; break;
            case "HKD": return HKD; break;
            case "HRK": return HRK; break;
            case "HUF": return HUF; break;
            case "IDR": return IDR; break;
            case "ILS": return ILS; break;
            case "INR": return INR; break;
            case "JPY": return JPY; break;
            case "KRW": return KRW; break;
            case "MXN": return MXN; break;
            case "MYR": return MYR; break;
            case "NOK": return NOK; break;
            case "NZD": return NZD; break;
            case "PHP": return PHP; break;
            case "PLN": return PLN; break;
            case "RON": return RON; break;
            case "RUB": return RUB; break;
            case "SEK": return SEK; break;
            case "SGD": return SGD; break;
            case "THB": return THB; break;
            case "TRY": return TRY; break;
            case "USD": return USD; break;
            case "ZAR": return ZAR; break;
            default: return -1; break; 
        }

    }

    public class RootObject
    {
        public string @base { get; set; }
        public string date { get; set; }
        public Rates rates { get; set; }
    }
}