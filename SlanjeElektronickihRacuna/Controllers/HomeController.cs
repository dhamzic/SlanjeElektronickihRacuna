using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlanjeElektronickihRacuna.eRacuni;
using SlanjeElektronickihRacuna.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using eDokumenti;
using eDokumenti.APIV2_mojERacun;
using RestSharp;
using RestSharp.Serialization.Json;

namespace SlanjeElektronickihRacuna.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// Funkcija se poziva prilikom otvaranja url-a početne stranice.
        /// Ukoliko je primljena forma podataka validna, poziva se
        /// funkcija za slanje elektroničkog računa, ukoliko nije
        /// refresh-a se početna stranica sa pripadajućim porukama
        /// upozorenja.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Index(InformacijePosiljatelja ip)
        {
            if (ModelState.IsValid)
            {
                PosaljiElektronickiRacun(ip);
                return View();
            }
            return View(ip);
        }

        /// <summary>
        /// Funkcija prima podatke obrasca te šalje pripadajući elektronički račun
        /// prema servisu mojih eRačuna.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public Send.ReturnResult PosaljiElektronickiRacun(InformacijePosiljatelja ip)
        {
            //Čitanje xml-a.
            var result = new StringBuilder();
            using (var reader = new StreamReader(ip.file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }
            //Pretvorba file-a u string xml
            String xml = result.ToString();

            //Punjenje korisničkih podataka (moj eRačun) pošiljatelja
            eDokumenti.APIV2_mojERacun.Send posiljateljPodaci = new eDokumenti.APIV2_mojERacun.Send()
            {
                Username = int.Parse(ip.KorisnickoIme),
                Password = ip.Lozinka,
                CompanyId = ip.OibTvrtke,
                CompanyBu = ip.PoslovnaJedinica,
                SoftwareId = ip.SoftverId,
                File = xml
            };
            //Oznaka da je riječ o demo slanju, a ne produkcijskom
            eRacun.MojeService.Demo = ip.Demo;
            eDokumenti.APIV2_mojERacun.Send.ReturnResult povratnaVrijednostSlanja;
            //Zovi metodu slanja
            povratnaVrijednostSlanja = ip.PosaljiRacun(posiljateljPodaci);
            if (povratnaVrijednostSlanja.ErrorMsg == "OK" && povratnaVrijednostSlanja.mRes[0].Created != null)
            {
                //Slanje uspješno
                ViewBag.slanjeOk = "Elektronički račun uspješno je poslan prema servisu Moj-eRačun.";
            }
            else
            {
                //Slanje neuspješno
                ViewBag.slanjeNotOk = "Elektronički račun nije poslan prema servisu Moj-eRačun. Provjerite pravilnost XML-a na sljedećoj poveznici: ";
            }
            return povratnaVrijednostSlanja;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        
    }
}
