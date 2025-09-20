using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.DataBase;
using MySql.Data.MySqlClient;
using WebApplication3.Authenticacao;

namespace WebApplication3.Controllers
{
    public class AuthController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(string email, string senha, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Error = "informe e-mail e senha";
                return View();
            }

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_usuario_obter_por_email", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_email", email);
            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
            {
                ViewBag.Error = "Usuario nao encontrado";
                return View();
            }

            var id = rd.GetInt32("id");
            var nome = rd.GetString("nome");
            var role = rd.GetString("role");
            var ativo = rd.GetBoolean("ativo");
            var senhaHash = rd["senha_hash"] as string ?? "";

            if (!ativo)
            {
                ViewBag.Error = "Usuario inativo.";
                return View();
            }

            bool ok;
            try
            {
                ok = BCrypt.Net.BCrypt.Verify(senha, senhaHash);
            }
            catch (Exception ex)
            {
                ok = false;
            }

            if (!ok)
            {
                ViewBag.Error = "Senha invalida";
                return View();
            }
            HttpContext.Session.SetInt32(SessionKeys.UserId, id);
            HttpContext.Session.SetString(SessionKeys.UserName, nome);
            HttpContext.Session.SetString(SessionKeys.UserEmail, email);
            HttpContext.Session.SetString(SessionKeys.UserRole, role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost,ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AcessoNegado() => View();
    }
}
