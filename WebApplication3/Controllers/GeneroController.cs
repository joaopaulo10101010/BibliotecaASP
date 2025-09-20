using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication3.Authenticacao;
using WebApplication3.DataBase;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [SessionAuthorize]
    public class GeneroController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            using var conn = db.GetConnection();
            List<Genero> lista = new List<Genero>();
            using (var cmd = new MySqlCommand("sp_genero_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    lista.Add(new Genero()
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),
                    });
                }
                return View(lista);
            }
        }

        public IActionResult CadastrarGenero()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CadastrarGenero(string genero)
        {
            using var conn = db.GetConnection();

            if (string.IsNullOrEmpty(genero) == false)
            {
                using (var cmd = new MySqlCommand("sp_genero_criar", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_nome", genero);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id, string nome)
        {
            Genero autor = new Genero() { Id = id, Nome = nome };
            return View(autor);
        }
        [HttpPost]
        public IActionResult Editar(Genero model)
        {
            if (model.Id <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                ModelState.AddModelError("", "Informe corretamente a autor");
            }
            using var conn2 = db.GetConnection();

            using var cmd = new MySqlCommand("sp_genero_atualizar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", model.Id);
            cmd.Parameters.AddWithValue("p_nome", model.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "genero Atualizado";

            return RedirectToAction("Index");
        }
    }
}
