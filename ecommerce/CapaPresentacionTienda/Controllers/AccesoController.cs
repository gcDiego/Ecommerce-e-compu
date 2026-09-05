using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CapaPresentacionTienda.Controllers
{
    public class AccesoController : Controller
    {
        // GET: Acceso
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Registrar()
        {
            return View();
        }

        public ActionResult Restablecer()
        {
            return View();
        }

        public ActionResult CambiarClave()
        {
            return View();
        }

        [HttpPost]

        public ActionResult Registrar(Cliente objeto)
        {
            string mensaje = string.Empty;

          
            if (string.IsNullOrWhiteSpace(objeto.Nombres) ||
                string.IsNullOrWhiteSpace(objeto.Apellidos) ||
                string.IsNullOrWhiteSpace(objeto.Correo) ||
                string.IsNullOrWhiteSpace(objeto.Clave) ||
                string.IsNullOrWhiteSpace(objeto.ConfirmarClave))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
               
                ViewData["Nombres"] = objeto.Nombres ?? "";
                ViewData["Apellidos"] = objeto.Apellidos ?? "";
                ViewData["Correo"] = objeto.Correo ?? "";
                return View();
            }

        
            ViewData["Nombres"] = objeto.Nombres;
            ViewData["Apellidos"] = objeto.Apellidos;
            ViewData["Correo"] = objeto.Correo;

         
            if (objeto.Clave != objeto.ConfirmarClave)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View();
            }

        
            int resultado = new CN_Cliente().Registrar(objeto, out mensaje);

            if (resultado == 0)
            {
                ViewBag.Error = null;
                ViewBag.Mensaje = "Su cuenta ha sido creada correctamente 🎉";
                ModelState.Clear();
                return View(new Cliente());
            }
            else
            {
                ViewBag.Error = mensaje;
                return View();
            }
        }


        [HttpPost]

        public ActionResult Index(string correo, string clave)
        {

            Cliente oCliente = null;

            oCliente = new CN_Cliente().Listar().Where(item => item.Correo == correo && item.Clave == CN_Recursos.ConvertirSha256(clave)).FirstOrDefault();

            if (oCliente == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }
            else
            {
                if (oCliente.Restablecer)
                {
                    TempData["IdCliente"] = oCliente.IdCliente;
                    return RedirectToAction("CambiarClave", "Acceso");
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(oCliente.Correo, false);

                    Session["Cliente"] = oCliente;

                    ViewBag.Error = null;

                    return RedirectToAction("Index", "Tienda");
                }

            }

                  
        }

        [HttpPost]
        public ActionResult Restablecer(string correo)
        {
            Cliente cliente = new Cliente();

            cliente = new CN_Cliente().Listar().Where(item => item.Correo == correo).FirstOrDefault();

            if (cliente == null)
            {
                ViewBag.Error = "No se encontro un usuario relacionado a este correo";
                return View();
            }

            string mensaje = string.Empty;
            bool respuesta = new CN_Cliente().RestablecerClave(cliente.IdCliente, correo, out mensaje);

            if (respuesta)
            {
                ViewBag.Mensaje = "Se ha enviado un correo con su nueva contraseña.";
                return View();
            }
            else
            {
                ViewBag.Error = mensaje;
                return View();
            }
        }

        [HttpPost]
        public ActionResult CambiarClave(string idcliente, string claveactual,string nuevaclave, string confirmarclave)
        {
            Cliente oCliente = new Cliente();

            oCliente = new CN_Cliente().Listar().Where(u => u.IdCliente == int.Parse(idcliente)).FirstOrDefault();

            if (oCliente.Clave != CN_Recursos.ConvertirSha256(claveactual))
            {
                TempData["IdCliente"] = idcliente;
                ViewData["vclave"] = claveactual;
                ViewBag.Error = "La contraseña actual no es correcta";
                return View();
            }
            else if (nuevaclave != confirmarclave)
            {
                TempData["IdCliente"] = idcliente;
                ViewData["vclave"] = claveactual;
                ViewBag.Error = "Las Contraseñas no coinciden";
                return View();
            }

            ViewData["vclave"] = "";

            nuevaclave = CN_Recursos.ConvertirSha256(nuevaclave);

            string mensaje = string.Empty;

            bool respuesta = new CN_Cliente().CambiarClave(int.Parse(idcliente), nuevaclave, out mensaje);

            if (respuesta)
            {
                return RedirectToAction("Index");

            }
            else
            {
                TempData["IdCliente"] = idcliente;
                ViewBag.Error = mensaje;
                return View();
            }
        }

        public ActionResult CerrarSesion()
        {
            Session["Cliente"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Acceso");
        }


    }
}