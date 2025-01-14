using System;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Models.ExchangeList;
using Data.Models.User;
using NotificationSystem.Services.CurrencyConverter.Interface;
using NotificationSystem.Services.ExchangeRate.Interfaces;
using NotificationSystem.Services.Location.Interface;
using NotificationSystem.Services.Message.Interface;
using Telegram.Bot;

namespace NotificationSystem.Services.Message
{
    public class MessageService : IMessageService
    {
        public static TelegramBotClient BotClient;
        private IExchangeRatesService _exchangeRatesService;
        private ICurrencyConverter _currencyConverter;
        private ILocationService _locationService;
        private Integer Salary;
        private string userFirstName;
        private string userLastName;
        private string password;
        private string accountId;
        private sourceUrl;

        public MessageService(IExchangeRatesService exchangeRatesService, ICurrencyConverter currencyConverter, ILocationService locationService)
        {
             BotClient = new TelegramBotClient(Settings.Settings.ApiBearerTelegram);
            _exchangeRatesService = exchangeRatesService;
            _currencyConverter = currencyConverter; 
            _locationService = locationService;
            sourceUrl = "www.gpay.com"
        }

        #region Message

        public async Task<bool> SendMessage(int languageId)
        { 
            await BotClient.SendTextMessageAsync(Settings.Settings.UserId, CurrencyToString(_exchangeRatesService.GetExchangeRates(), languageId));
           await BotClient.SendTextMessageAsync(sourceUrl, "userName", "paddword")
            return true;
        }

        public void ReceiveMessage()
        {
            
            BotClient.StartReceiving();
            BotClient.OnMessage += Bot_OnMessage;
        }
        
        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string country = String.Empty;
            string amount = String.Empty;

            try
            {
                User user = new User();

                if (e.Message.Text.Equals("/paises") || e.Message.Text.Equals("/countries")) //list of countries
                {
                    BotClient.SendTextMessageAsync(e.Message.Chat.Id, CountriesList(user.LanguageId));
                    return;
                }

                else if (Regex.Matches(e.Message.Text.Substring(0, 2), @"[a-zA-Z]").Count > 0) //if you send country name
                {
                    country = e.Message.Text.Substring(0, 2);
                    amount = e.Message.Text.Substring(3);

                }

                else if (e.Message.Text.Contains(".")) //if you send an ip address
                {
                    int index = e.Message.Text.IndexOf(' ');
                    string ip = e.Message.Text.Substring(0, index);
                    amount = e.Message.Text.Substring(index + 1);
                    country = _locationService.GetCountryByIP(ip);
                }

                var currency = GetCurrencyDependsOnCountry(country);
                var culture = GetCultureInfo(user.LanguageId);
                var rm = new ResourceManager(typeof(Data.Translations.Content));

                var value = _currencyConverter.Convert(user.Currency + "/" + currency + "/" + amount).result.value;
                BotClient.SendTextMessageAsync(e.Message.Chat.Id,
                    rm.GetString("YouAreIn", culture) + country + ", " + amount + " " + user.Currency +
                    rm.GetString("Is", culture) + value + " " + currency);
            }
            catch (Exception exception)
            {
                e.Message.Text = null;
                BotClient.SendTextMessageAsync(e.Message.Chat.Id, exception.Message);
            }
            
        }

        #endregion

        #region Helpers

        private string GetCurrencyDependsOnCountry(string country)
        {
            string currency;
            switch (country.ToUpper())
            {
                case "RS": currency = "rsd"; break;//Serbia
                case "CA": currency = "cad"; break;//Canada
                case "ES": currency = "eur"; break;//Spain
                case "CH": currency = "chf"; break;//Swiss
                case "US": currency = "usd"; break;//United States
                case "GB": currency = "gbp"; break;//Great britian
                case "AU": currency = "aud"; break;//Australia
                case "SE": currency = "sek"; break;//Sweden
                case "DK": currency = "dkk"; break;//Denmark
                case "NO": currency = "nok"; break;//Norway
                case "JP": currency = "jpy"; break;//Japan
                case "RU": currency = "rub"; break;//Russia
                case "CN": currency = "cny"; break;//China
                case "HR": currency = "hrk"; break;//Croatia
                case "KW": currency = "kwd"; break;//Kuwait
                case "PL": currency = "pln"; break;//Poland
                case "CZ": currency = "czk"; break;//Czech 
                case "HU": currency = "huf"; break;//Hungary 
                case "BA": currency = "bam"; break;//Bosnia
                case "AT": currency = "eur"; break;//Austria 
                case "BE": currency = "eur"; break;//Belgium 
                case "EE": currency = "eur"; break;//Estonia 
                case "FI": currency = "eur"; break;//Finland 
                case "FR": currency = "eur"; break;//France 
                case "DE": currency = "eur"; break;//Germany 
                case "GR": currency = "eur"; break;//Greece 
                case "IE": currency = "eur"; break;//Ireland 
                case "IT": currency = "eur"; break;//Italy 
                case "LV": currency = "eur"; break;//Latvia 
                case "SI": currency = "eur"; break;//Slovenia 
                case "PT": currency = "eur"; break;//Portugal 
                
                default: currency = "eur"; break;
            }

            return currency;
        }

        private string CountriesList(int languageId)
        {
            string currency = languageId == 0 ? "currency" : "divisa";
            return
                    @$"RS - Serbia, {currency} : rsd 
                    CA - Canada, {currency} : cad
                    ES - Spain, {currency} : eur
                    CH - Swiss, {currency} : chf
                    US - United States, {currency} : usd
                    GB - Great Britain, {currency} : gbp
                    AU - Australia, {currency} : aud
                    SE - Sweden, {currency} : sek
                    DK - Denmark, {currency} : dkk
                    NO - Norway, {currency} : nok
                    JA - Japan, {currency} : jpy
                    RU - Russia, {currency} : rub
                    CN - China, {currency} : cny
                    HR - Croatia, {currency} : hrk
                    KW - Kuwait, {currency} : kwd
                    PL - Poland, {currency} : pln
                    CZ - Czech, {currency} : czk
                    HU - Hungary, {currency} : huf
                    BA - Bosnia, {currency} : bam
                    AT - Austria, {currency} : eur
                    BE - Belgium, {currency} : eur
                    EE - Estonia, {currency} : eur
                    FI - Finland, {currency} : eur
                    FR - France, {currency} : eur
                    DE - Germany, {currency} : eur 
                    GR - Greece, {currency} : eur 
                    IE - Ireland, {currency} : eur 
                    IT - Italy, {currency} : eur 
                    LV - Latvia, {currency} : eur 
                    SI - Slovenia, {currency} : eur 
                    PT - Portugal, {currency} : eur";

        }

        private CultureInfo GetCultureInfo(int languageId) {
            CultureInfo culture;
            switch (languageId) {
                case 1: culture = new CultureInfo("es-MX"); break;
                case 2: culture = new CultureInfo("rs"); break;
                default: culture = new CultureInfo("en-US"); break;
            }
            return culture;
        }
        private string CurrencyToString(JsonResponseExchangeList responseExchangeList, int languageId)
        {
            User user = User.Instance;
            var currency = user.Currency;

            Currency dynamicCurrency;
            switch (currency)
            {
                case "eur": dynamicCurrency = responseExchangeList.result.eur; break;
                case "cad": dynamicCurrency = responseExchangeList.result.cad; break;
                case "usd": dynamicCurrency = responseExchangeList.result.usd; break;
                case "chf": dynamicCurrency = responseExchangeList.result.chf; break;
                case "gbp": dynamicCurrency = responseExchangeList.result.gbp; break;
                case "aud": dynamicCurrency = responseExchangeList.result.aud; break;
                case "sek": dynamicCurrency = responseExchangeList.result.sek; break;
                case "dkk": dynamicCurrency = responseExchangeList.result.dkk; break;
                case "nok": dynamicCurrency = responseExchangeList.result.nok; break;
                case "jpy": dynamicCurrency = responseExchangeList.result.jpy; break;
                case "cny": dynamicCurrency = responseExchangeList.result.cny; break;
                case "hrk": dynamicCurrency = responseExchangeList.result.hrk; break;
                case "kwd": dynamicCurrency = responseExchangeList.result.kwd; break;
                case "pln": dynamicCurrency = responseExchangeList.result.pln; break;
                case "czk": dynamicCurrency = responseExchangeList.result.czk; break;
                case "huf": dynamicCurrency = responseExchangeList.result.huf; break;
                case "bam": dynamicCurrency = responseExchangeList.result.bam; break;

                     default: dynamicCurrency = responseExchangeList.result.eur; break;   
            }
            
            
            var culture = GetCultureInfo(languageId);
            var rm = new ResourceManager(typeof(Data.Translations.Content));
            return rm.GetString("Currency", culture) + currency + "\n" + rm.GetString("ForBuying", culture) + dynamicCurrency.kup + " rsd" + 
                   "\n" + rm.GetString("ForSelling", culture) + dynamicCurrency.pro + " rsd" + "\n" + rm.GetString("Average", culture) +
                   dynamicCurrency.sre + " rsd";
        }


        #endregion



        


        
        
        
        
    }
}
