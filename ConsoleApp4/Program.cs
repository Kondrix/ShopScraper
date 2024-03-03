using System.Linq;
using System.Net;
using HtmlAgilityPack;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using static SkapiecScraper.Program;
using System.Text.RegularExpressions;
using System.Globalization;
using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SkapiecScraper
{
    class Program
    {
        public class Product
        {
            public string Name { get; set; }
            [BsonRepresentation(BsonType.Decimal128)]
            public decimal Price { get; set; }
            [BsonRepresentation(BsonType.Decimal128)]
            public decimal LinkID { get; set; }
            public string CategoryID { get; set; }
            public DateTime InsertionTime { get; set; }
        }
        static async Task Main(string[] args)
        {


            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("skapiec_scraper");
            var productsCollection = database.GetCollection<Product>("products");


            for (int i = 1; i <= 29; i++)
            {

                for (int page = 1; page <= 1; page++)
                {
                    var query = "query"; // ustawienie zapytania, dla którego mają zostać znalezione produkty
                    var url = $"https://www.skapiec.pl/dep/{i}/page/{page}?query={WebUtility.UrlEncode(query)}";
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);


                    var narrowProductNodes =
                        doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-box-narrow-container')]");

                    if (narrowProductNodes != null)
                    {

                        foreach (var productNode in narrowProductNodes)
                        {
                            var productNameText = productNode.SelectSingleNode(".//h2[@title]")
                            .Attributes["title"].Value.Trim();




                            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'product-box-narrow__price')]");
                            var priceText = priceNode?.InnerText.Trim() ?? "Unavailable";



                            if (priceNode != null)
                            {
                                var currency = priceNode.SelectSingleNode(".//span[contains(@class, 'price--secondary price--fw-500 price--fs-16')]").InnerText.Trim().Replace("od ", "");
                                if (priceText.Contains("<!-- -->"))
                                {
                                    priceText = priceText.Replace("<!-- -->", "");
                                    priceText = priceText.Replace("-->", "");
                                }
                                if (priceText.Contains("od"))
                                {
                                    priceText = priceText.Replace("od", "");
                                }
                                var index = priceText.IndexOf("zł");
                                if (index != -1)
                                {
                                    priceText = priceText.Substring(0, index).Replace(".", ",");


                                }
                            }

                            var productID = productNode.SelectSingleNode(".//a[contains(@href, '/site/')]");
                            if (productID != null)
                            {
                                var productLinkID = productID.Attributes["href"].Value;
                                var productId = productLinkID.Substring(productLinkID.LastIndexOf("/") + 1);
                                var categoryId = url.Split(new[] { "/dep/" }, StringSplitOptions.None)[1].Split('/')[0];
                                priceText = Regex.Replace(priceText, @"\s+", "");


                                switch (categoryId)
                                {
                                    case "1":
                                        categoryId = "Uroda";
                                        break;
                                    case "2":
                                        categoryId = "RTV";
                                        break;
                                    case "3":
                                        categoryId = "Foto";
                                        break;
                                    case "5":
                                        categoryId = "Telefony";
                                        break;
                                    case "6":
                                        categoryId = "AGD";
                                        break;
                                    case "8":
                                        categoryId = "Ogród";
                                        break;
                                    case "10":
                                        categoryId = "Biuro i firma";
                                        break;
                                    case "11":
                                        categoryId = "Militaria";
                                        break;
                                    case "12":
                                        categoryId = "Motoryzacja";
                                        break;
                                    case "13":
                                        categoryId = "Gry";
                                        break;
                                    case "14":
                                        categoryId = "Sport";
                                        break;
                                    case "15":
                                        categoryId = "Biżuteria i zegarki";
                                        break;
                                    case "16":
                                        categoryId = "Zdrowie";
                                        break;
                                    case "18":
                                        categoryId = "Muzyka";
                                        break;
                                    case "19":
                                        categoryId = "Moda";
                                        break;
                                    case "21":
                                        categoryId = "Dla dzieci";
                                        break;
                                    case "22":
                                        categoryId = "Dom i wnętrze";
                                        break;
                                    case "23":
                                        categoryId = "Zwierzęta i hobby";
                                        break;
                                    case "24":
                                        categoryId = "Budowa i remont";
                                        break;
                                    case "25":
                                        categoryId = "Dekilatesy";
                                        break;
                                    case "26":
                                        categoryId = "Erotyka";
                                        break;
                                    case "28":
                                        categoryId = "Książki";
                                        break;
                                    case "29":
                                        categoryId = "Filmy";
                                        break;
                                    default:
                                        categoryId = "Brak kategorii";
                                        break;
                                };
                             
                               
                                
                                Console.WriteLine(productNameText+ "1");
                                var product = new Product
                                {
                                    Name = productNameText,
                                    Price = decimal.Parse(priceText),
                                    LinkID = decimal.Parse(productId),
                                    CategoryID = categoryId,
                                    InsertionTime = DateTime.Now
                                };

                                if(product !=null)
                                {
                                    await productsCollection.InsertOneAsync(product); // use async version of InsertOne() method
                                }

                            }
                        }
                    }

                    var wideProductNodes =
                        doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-box-wide-d-desc')]");


                    if (wideProductNodes != null)
                    {

                        //Console.WriteLine($"Found {wideProductNodes.Count} wide product nodes");

                        foreach (var productNode2 in wideProductNodes)
                        {



                            var productNameText2 = productNode2.SelectSingleNode(".//h2[@class='product-box-wide-d-main__title']").InnerText.Trim();



                            var priceNode2 = productNode2.SelectSingleNode(".//div[contains(@class, 'product-box-wide-d-price')]");

                            var priceText2 = priceNode2?.InnerText.Trim() ?? "Unavailable";



                            if (priceNode2 != null)
                            {

                                var currency2 = priceNode2.SelectSingleNode(".//span[contains(@class, 'price--secondary price--fw-500 price--fs-16')]").InnerText.Trim().Replace("od ", "");
                                if (priceText2.Contains("<!-- -->"))
                                {
                                    priceText2 = priceText2.Replace("<!-- -->", "");
                                }
                                if (priceText2.Contains("od"))
                                {
                                    priceText2 = priceText2.Replace("od", "");
                                }
                                var index2 = priceText2.IndexOf("zł");

                                if (index2 != -1)
                                {
                                    priceText2 = priceText2.Substring(0, index2).Replace(".", ",");


                                }





                                var productID2 = productNode2.SelectSingleNode(".//a[contains(@href, '/comp/')]");
                                if (productID2 != null)
                                {

                                    var productLinkID2 = productID2.Attributes["href"].Value;
                                    var productId2 = productLinkID2.Substring(productLinkID2.LastIndexOf("/") + 1);
                                    var categoryId2 = url.Split(new[] { "/dep/" }, StringSplitOptions.None)[1].Split('/')[0];
                                    priceText2 = Regex.Replace(priceText2, @"\s+", "");


                                    switch (categoryId2)
                                    {
                                        case "1":
                                            categoryId2 = "Uroda";
                                            break;
                                        case "2":
                                            categoryId2 = "RTV";
                                            break;
                                        case "3":
                                            categoryId2 = "Foto";
                                            break;
                                        case "5":
                                            categoryId2 = "Telefony";
                                            break;
                                        case "6":
                                            categoryId2 = "AGD";
                                            break;
                                        case "8":
                                            categoryId2 = "Ogród";
                                            break;
                                        case "10":
                                            categoryId2 = "Biuro i firma";
                                            break;
                                        case "11":
                                            categoryId2 = "Militaria";
                                            break;
                                        case "12":
                                            categoryId2 = "Motoryzacja";
                                            break;
                                        case "13":
                                            categoryId2 = "Gry";
                                            break;
                                        case "14":
                                            categoryId2 = "Sport";
                                            break;
                                        case "15":
                                            categoryId2 = "Biżuteria i zegarki";
                                            break;
                                        case "16":
                                            categoryId2 = "Zdrowie";
                                            break;
                                        case "18":
                                            categoryId2 = "Muzyka";
                                            break;
                                        case "19":
                                            categoryId2 = "Moda";
                                            break;
                                        case "21":
                                            categoryId2 = "Dla dzieci";
                                            break;
                                        case "22":
                                            categoryId2 = "Dom i wnętrze";
                                            break;
                                        case "23":
                                            categoryId2 = "Zwierzęta i hobby";
                                            break;
                                        case "24":
                                            categoryId2 = "Budowa i remont";
                                            break;
                                        case "25":
                                            categoryId2 = "Dekilatesy";
                                            break;
                                        case "26":
                                            categoryId2 = "Erotyka";
                                            break;
                                        case "28":
                                            categoryId2 = "Książki";
                                            break;
                                        case "29":
                                            categoryId2 = "Filmy";
                                            break;
                                        default:
                                            categoryId2 = "Brak kategorii";
                                            break;
                                    };
                                    Console.WriteLine("----2"+productNameText2);


                                    var product2 = new Product
                                    {
                                        Name = productNameText2,
                                        Price = decimal.Parse(priceText2),
                                        LinkID = decimal.Parse(productId2),
                                        CategoryID = categoryId2,
                                        InsertionTime = DateTime.Now
                                    };



                                    await productsCollection.InsertOneAsync(product2); 
                                }
                            }
                        }
                    }

                }

            }
        }

       


    }
}