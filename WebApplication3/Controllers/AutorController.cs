using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication3.DataBase;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class AutorController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            using var conn = db.GetConnection();
            List<Autor> lista = new List<Autor>();
            using (var cmd = new MySqlCommand("sp_autor_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    lista.Add(new Autor()
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),
                    });
                }
                return View(lista);
            }
        }

        public IActionResult CadastrarAutor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CadastrarAutor(string autor)
        {
            using var conn = db.GetConnection();

            if (string.IsNullOrEmpty(autor) == false)
            {
                using (var cmd = new MySqlCommand("sp_autor_criar", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_nome", autor);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }

            return RedirectToAction("CadastrarAutor");
        }

        public IActionResult Editar(int id, string nome)
        {
            Autor autor = new Autor() { Id = id, Nome = nome };
            return View(autor);
        }
        [HttpPost]
        public IActionResult Editar(Autor model)
        {
            if (model.Id <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                ModelState.AddModelError("", "Informe corretamente a autor");
            }
            using var conn2 = db.GetConnection();

            using var cmd = new MySqlCommand("sp_autor_atualizar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", model.Id);
            cmd.Parameters.AddWithValue("p_nome", model.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "autor Atualizado";

            return RedirectToAction(nameof(Index));
        }
    }
}
