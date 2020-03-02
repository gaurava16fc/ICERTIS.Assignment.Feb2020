using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ICERTIS.Assignment.Feb2020
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            int year = 0;

            if (args.Length <= 0)
            {
                Console.WriteLine("Required argument (Year) is missing. Please try again");
                Console.ReadLine();
                return;
            }
            else if (args.Length > 0)
            {
                int.TryParse(args[0], out year);
                if (year <= 0)
                {
                    Console.WriteLine("Invalid Argument....");
                    Console.ReadLine();
                    return;
                }
            }

            Console.WriteLine("List of Products where sale exceeds the forecast in the given year: ");
            Console.WriteLine("*********************************************************************");

            ProcessWorker worker = new ProcessWorker(year);
            var taskResult = worker.StartComparison();
            if (taskResult != null)
            {
                var productList = taskResult.Result;
                if (productList != null && productList.Count > 0)
                {
                    foreach (var entity in productList)
                    {
                        Console.WriteLine(entity.ProductName);
                    }
                }
                else
                {
                    Console.WriteLine("No Product found which exceeds the forecast value in given year...");
                }
            }
            else
            {
                Console.WriteLine("No Product found which exceeds the forecast value in given year...");
            }
            Console.WriteLine("  ");
            Console.WriteLine("Press any key to quit the program....");
            Console.ReadLine();
        }
    }

    public class ForeCastProductData
    {
        public string Status { get; set; }
        public Dictionary<string, int> data { get; set; }
    }

    public class ActualProductData
    {
        public string Status { get; set; }
        public Dictionary<string, int> data { get; set; }
    }

    public class ProductEntity
    {
        public string ProductName { get; set; }
        public int Sale { get; set; }
    }


    public class ProcessWorker
    {
        private readonly int givenYear;
        private readonly string apiBaseAddress;
        private readonly string forecastAPIAddress;
        private readonly string actualAPIAddress;

        public ProcessWorker(int givenYear)
        {
            this.givenYear = givenYear;
            this.apiBaseAddress = ConfigurationManager.AppSettings["APIBaseURL"].ToString();
            this.forecastAPIAddress = this.apiBaseAddress + "forecast/" + Convert.ToString(givenYear) + "/";
            this.actualAPIAddress = this.apiBaseAddress + "orders/" + Convert.ToString(givenYear) + "/";
        }

        // Forecast Data URL: http://assessments.reliscore.com/api/forecast/YYYY/
        public async Task<List<ProductEntity>> GetForecastProductData()
        {
            try
            {
                List<ProductEntity> listProduct = new List<ProductEntity>();
                string json_string = await (new WebClient()).DownloadStringTaskAsync(actualAPIAddress);
                ForeCastProductData items = JsonConvert.DeserializeObject<ForeCastProductData>(json_string);
                if (items!=null && items.data!=null && items.data.Count > 0)
                {
                    foreach(var _itm in items.data)
                    {
                        listProduct.Add(new ProductEntity() { ProductName = _itm.Key, Sale = _itm.Value });
                    }
                }
                return listProduct;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Forecast Data URL: http://assessments.reliscore.com/api/orders/YYYY/
        public async Task<List<ProductEntity>> GetActualProductData()
        {
            try
            {
                List<ProductEntity> listProduct = new List<ProductEntity>();
                string json_string = (new WebClient()).DownloadString(this.actualAPIAddress);
                ActualProductData items = JsonConvert.DeserializeObject<ActualProductData>(json_string);
                if (items != null && items.data != null && items.data.Count > 0)
                {
                    foreach (var _itm in items.data)
                    {
                        listProduct.Add(new ProductEntity() { ProductName = _itm.Key, Sale = _itm.Value });
                    }
                }
                return listProduct;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<ProductEntity>> StartComparison()
        {
            try
            {
                var forecastProductList = await GetForecastProductData();
                var actualProductList = await GetActualProductData();
                //// test code....
                //if (actualProductList != null)
                //    actualProductList.Find(x => x.ProductName == "product2").Sale = 32;

                var dataList = actualProductList.Where(a => forecastProductList.Any(f => f.ProductName == a.ProductName && a.Sale > f.Sale)).ToList();

                var q1 = from b in actualProductList select b;
                var q2 = (from b in actualProductList select b).ToArray();


                return dataList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
