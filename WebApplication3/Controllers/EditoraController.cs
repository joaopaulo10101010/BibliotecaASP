using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication3.DataBase;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class EditoraController : Controller
    {
        private readonly Database db = new Database();

        [HttpGet]
        public IActionResult Index()
        {
            using var conn = db.GetConnection();
            List<Editora> lista = new List<Editora>();
            using (var cmd = new MySqlCommand("sp_editora_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                using var rd = cmd.ExecuteReader();
                while(rd.Read())
                {
                    lista.Add(new Editora()
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),
                    });
                }
                return View(lista);
            }
        }
        
        public IActionResult Editar(int id, string nome)
        {
            Editora editora = new Editora() { Id = id ,Nome = nome};
            return View(editora);
        }
        [HttpPost]
        public IActionResult Editar(Editora model)
        {

            if (model.Id <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                ModelState.AddModelError("", "Informe corretamente a editora");
            }
            using var conn2 = db.GetConnection();

            using var cmd = new MySqlCommand("sp_editora_atualizar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", model.Id);
            cmd.Parameters.AddWithValue("p_nome", model.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Editora Atualizado";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Excluir(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_editora_excluir", conn) { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.ExecuteNonQuery();
                TempData["Ok"] = "Editora Excluido";
            }
            catch (Exception ex)
            {
                TempData["Ok"] = ex.Message;
            }


            return RedirectToAction(nameof(Index));
        }

        public IActionResult CadastrarEditora()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CadastrarEditora(string editora, string autor, string genero)
        {
            using var conn = db.GetConnection();
            if (string.IsNullOrEmpty(editora) == false)
            {
                using (var cmd = new MySqlCommand("sp_editora_criar", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_nome", editora);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }

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


            return RedirectToAction("CadastrarEditora");
        }
    }
}
