using CapaEntidad.Paypal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_Paypal
    {
        private static string urlpaypal = ConfigurationManager.AppSettings["URLPaypal"];
        private static string clientid = ConfigurationManager.AppSettings["ClientID"];
        private static string secret = ConfigurationManager.AppSettings["Secret"];

        private async Task<string> ObtenerToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(urlpaypal);

                var authtoken = Encoding.ASCII.GetBytes($"{clientid}:{secret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authtoken));

                var requestBody = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage response = await client.PostAsync("/v1/oauth2/token", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(result);
                    return json.access_token;
                }

                return null;
            }
        }


        public async Task<Response_Paypal<Response_Checkout>> CrearSolicitud(Checkout_Order orden)
        {
            Response_Paypal<Response_Checkout> response_paypal = new Response_Paypal<Response_Checkout>();

            string accessToken = await ObtenerToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                response_paypal.Status = false;
                return response_paypal;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(urlpaypal);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var json = JsonConvert.SerializeObject(orden);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/v2/checkout/orders", data);

                response_paypal.Status = response.IsSuccessStatusCode;

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    Response_Checkout checkout = JsonConvert.DeserializeObject<Response_Checkout>(jsonRespuesta);
                    response_paypal.Response = checkout;
                }
            }

            return response_paypal;
        }


        public async Task<Response_Paypal<Response_Capture>> AprobarPago(string token)
        {
            Response_Paypal<Response_Capture> response_paypal = new Response_Paypal<Response_Capture>();

            string accessToken = await ObtenerToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                response_paypal.Status = false;
                return response_paypal;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(urlpaypal);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var data = new StringContent("{}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"/v2/checkout/orders/{token}/capture", data);

                response_paypal.Status = response.IsSuccessStatusCode;

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();
                    Response_Capture capture = JsonConvert.DeserializeObject<Response_Capture>(jsonRespuesta);
                    response_paypal.Response = capture;
                }
            }

            return response_paypal;
        }

    }
}
