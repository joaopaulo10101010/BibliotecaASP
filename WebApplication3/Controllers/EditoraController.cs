using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication3.DataBase;

namespace WebApplication3.Controllers
{
    public class EditoraController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            return View();
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
