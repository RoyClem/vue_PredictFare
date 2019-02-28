using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Core.Data;

namespace TaxiUI
{
    [ApiController]
    public class HomeController : Controller
    {

        static readonly string _TaxiFarePath    = Path.Combine(Environment.CurrentDirectory, "Data", "taxi-fare.csv");
        static readonly string _ModelPath       = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");


        [Route("api/GetPrediction")]
        [HttpPost]
        public JsonResult GetPrediction([FromBody]TaxiTrip trip)
        {
            MLContext mlContext = new MLContext(seed: 0);
            ITransformer loadedModel;

            using (var stream = new FileStream(_ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            var predictionFunction = loadedModel.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(mlContext);

            // Add a trip to test the trained model's prediction of cost in the Predict method by creating an instance of TaxiTrip
            //var model = new TaxiTrip()
            //{
            //    VendorId        = trip.VendorId,
            //    RateCode        = trip.RateCode,
            //    PassengerCount  = trip.NumPass,
            //    TripDistance    = trip.TripDis,
            //    PaymentType     = trip.PayType
            //};

            var prediction = predictionFunction.Predict(trip);
            return new JsonResult(Math.Round(prediction.FareAmount, 2));
        }

        [Route("api/LoadData")]
        [HttpGet]
        public JsonResult LoadData()
        {
            List<string> vendors = new List<string>();
            List<string> rates = new List<string>();
            List<string> payTypes = new List<string>();

            using (var reader = new StreamReader(_TaxiFarePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if(!vendors.Contains(values[0]))
                        vendors.Add(values[0]);

                    if (!rates.Contains(values[1]))
                        rates.Add(values[1]);

                    if (!payTypes.Contains(values[5]))
                        payTypes.Add(values[5]);
                }
            }

            vendors.Sort((x,y)      => string.Compare(x, y));
            rates.Sort((x, y)       => string.Compare(x, y));
            payTypes.Sort((x, y)    => string.Compare(x, y));

            return new JsonResult(new { vendors, rates, payTypes });
        }
    }

    //public class TaxiTrip
    //{
    //    public string VendorId;
    //    public string RateCode;
    //    public string PayType;
    //    public float NumPass;
    //    public float TripDis;
    //    public float FareAmt;
    //}

}
