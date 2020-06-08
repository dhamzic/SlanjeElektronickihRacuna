using eDokumenti;
using eDokumenti.APIV2_mojERacun;
using Microsoft.AspNetCore.Http;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace SlanjeElektronickihRacuna.eRacuni
{
    public class InformacijePosiljatelja
    {
        [Display(Name = "Korisničko ime")]
        [Required(ErrorMessage = "Korisničko ime je obavezan podatak.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Korisničko ime čini niz brojeva")]
        public string KorisnickoIme { get; set; }

        [Required(ErrorMessage = "Korisnička lozinka je obavezan podatak.")]
        public string Lozinka { get; set; }

        [RegularExpression("^[0-9]{11}$", ErrorMessage = "OIB se sastoji od točno 13 brojeva.")]
        [Display(Name = "OIB tvrtke")]
        [Required(ErrorMessage = "OIB tvrtke pošiljatelja je obavezan podatak.")]
        public string OibTvrtke { get; set; }

        [Display(Name = "Poslovna jedinica")]
        public string PoslovnaJedinica { get; set; }

        [Display(Name = "ID softvera")]
        [Required(ErrorMessage = "ID softvera je obavezan podatak.")]
        public string SoftverId { get; set; }

        [Required(ErrorMessage = "XML elektroničkog računa je obavezan podatak.")]
        [AllowedExtensions(new string[] { ".xml" })]
       
        public IFormFile file { get; set; }

        [Display(Name = "Slanje na Demo servis")]
        public Boolean Demo { get; set; }



        /// <summary>
        /// Funckija koja šalje pripadajući objekt klase prema servisu mojih eRačuna te vraća rezultat slanja.
        /// </summary>
        /// <param name="mSend"></param>
        /// <returns></returns>
        public eDokumenti.APIV2_mojERacun.Send.ReturnResult PosaljiRacun(Send mSend)
        {
            Send.ReturnResult returnResult;
            Send.ReturnResult statusDescription = new Send.ReturnResult();
            try
            {
                RestClient restClient;
                string url = "";
                //Provjera je li riječ o produkcijskom ili demo slanju.
                if (eRacun.MojeService.Demo == true)
                {
                    restClient = new RestClient("https://demo.moj-eracun.hr");
                    url = @"https://demo.moj-eracun.hr";
                }
                else
                {
                    restClient = new RestSharp.RestClient("https://www.moj-eracun.hr");
                    url = @"https://www.moj-eracun.hr";
                }
                
                //Postavljanje REST POST zahtjeva
                string tempPath = Path.GetTempPath();
                System.IO.File.WriteAllText(tempPath + @"\urlSlanja.xml", url);

                RestRequest restRequest = new RestRequest("/apis/v2/send", Method.POST);
                restRequest.AddHeader("content-type", "application/json");
                restRequest.AddHeader("charset", "utf-8");
                restRequest.AddHeader("Accept", "application/json");

                var variable = new { Username = mSend.Username, Password = mSend.Password, CompanyId = mSend.CompanyId, CompanyBu = mSend.CompanyBu, SoftwareId = mSend.SoftwareId, File = mSend.File };
                restRequest.AddJsonBody(variable);
                IRestResponse restResponse = restClient.Execute(restRequest);

                RestResponse restResponse1 = new RestResponse()
                {
                    Content = restResponse.Content
                };
                statusDescription.mRes = (new JsonDeserializer()).Deserialize<List<Send.SendResponse>>(restResponse1);
                statusDescription.ErrorMsg = restResponse.StatusDescription;
                returnResult = statusDescription;
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                statusDescription.mRes = null;
                statusDescription.ErrorMsg = exception.Message;
                returnResult = statusDescription;
            }
            return returnResult;
        }
    }
}
