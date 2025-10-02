using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DataBase;
using WebApplication3.Models;
using MySql.Data;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication3.Authenticacao;

namespace WebApplication3.Controllers
{
    [SessionAuthorize]
    public class LivroController : Controller
    {
        private readonly Database db = new Database();


        private List<SelectListItem> CarregarAutores(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_autor_listar", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            }
            return list;
        }
        private List<SelectListItem> CarregarEditoras(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_editora_listar", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            }
            return list;
        }
        private List<SelectListItem> CarregarGeneros(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_genero_listar", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            }
            return list;
        }


        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var conn = db.GetConnection();

            Livros? livro = null;
            using (var cmd = new MySqlCommand("sp_livro_obter", conn) { CommandType = System.Data.CommandType.StoredProcedure})
            {
                cmd.Parameters.AddWithValue("p_id", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    livro = new Livros
                    {
                        Id = rd.GetInt32("id"),
                        Titulo = rd.GetString("titulo"),
                        AutorId = rd["autor"] == DBNull.Value ? null : (int?)rd.GetInt32("autor"),
                        EditoraId = rd["editora"] == DBNull.Value ? null : (int?)rd.GetInt32("editora"),
                        GeneroId = rd["genero"] == DBNull.Value ? null : (int?)rd.GetInt32("genero"),
                        Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt32("ano"),
                        Isbn = rd["isbn"] as string,
                        QuantidadeTotal = rd.GetInt32("quantidade_total")
                    };
                }
            }

            if (livro == null) return NotFound();

            ViewBag.Autores = CarregarAutores(conn);
            ViewBag.Editoras = CarregarEditoras(conn);
            ViewBag.Generos = CarregarGeneros(conn);
                
            return View();
        }
        [HttpPost]
        public IActionResult Editar(Livros model)
        {
            if (model.Id <= 0) return NotFound();
            if(string.IsNullOrWhiteSpace(model.Titulo) || model.QuantidadeTotal < 1)
            {
                ModelState.AddModelError("","Informe titulo e quantidade total (>-1)");
            }

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_atualizar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id",model.Id);
            cmd.Parameters.AddWithValue("p_titulo", model.Titulo);
            cmd.Parameters.AddWithValue("p_autor", model.AutorId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_editora", model.EditoraId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_genero", model.GeneroId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_ano", model.Ano ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_isbn", (object?)model.Isbn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_novo_total", model.QuantidadeTotal);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Livro Atualizado";

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_livro_excluir", conn) { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.ExecuteNonQuery();
                TempData["Ok"] = "Livro Excluido";
            }
            catch (Exception ex)
            {
                TempData["Ok"] = ex.Message;
            }


            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Index()

        {

            var lista = new List<Livros>();

            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_livro_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                lista.Add(new Livros
                {

                    Id = rd.GetInt32("id"),

                    Titulo = rd.GetString("titulo"),

                    AutorId = rd["autor"] == DBNull.Value ? null : (int?)rd.GetInt32("autor"),

                    EditoraId = rd["editora"] == DBNull.Value ? null : (int?)rd.GetInt32("editora"),

                    GeneroId = rd["genero"] == DBNull.Value ? null : (int?)rd.GetInt32("genero"),

                    Autor = rd["autor_nome"] as string,

                    Editora = rd["editora_nome"] as string,

                    Genero = rd["genero_nome"] as string,

                    Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),

                    Isbn = rd["isbn"] as string,

                    QuantidadeTotal = rd.GetInt32("quantidade_total"),

                    QuantidadeDisponivel = rd.GetInt32("quantidade_disponivel"),

                    CriadoEm = rd.GetDateTime("criado_em")
                });

            }
            Console.WriteLine("a quantidade da lista é " + lista.Count.ToString());
            return View(lista);

        }



        [HttpGet]

        public IActionResult Criar()
        {

            using var conn = db.GetConnection();

            ViewBag.Autores = CarregarAutores(conn);

            ViewBag.Editoras = CarregarEditoras(conn);

            ViewBag.Generos = CarregarGeneros(conn);

            return View();

        }
        [HttpPost]
        public IActionResult Criar(string titulo, string autor, string editora, string genero, string ano, string isbn, string quantidade, IFormFile? capa)
        {
            try
            {
                string? relPath = null;
                if(capa != null && capa.Length > 0)
                {
                    var ext = Path.GetExtension(capa.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "capas");
                    Directory.CreateDirectory(saveDir);
                    var absPath = Path.Combine(saveDir, fileName);
                    using var fs = new FileStream(absPath, FileMode.Create);
                    capa.CopyTo(fs);
                    relPath = Path.Combine("capas", fileName).Replace("\\", "/");
                }


                Console.WriteLine($"Titulo: {titulo}\nAutor: {autor}\nEditora: {editora}\nGenero: {genero}\nAno: {ano}\nIsbn: {isbn}\nQuantidade: {quantidade}");

                using var conn = db.GetConnection();
                using (var cmd = new MySqlCommand("sp_livro_criar", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_titulo", titulo);
                    cmd.Parameters.AddWithValue("p_autor", autor);
                    cmd.Parameters.AddWithValue("p_editora", editora);
                    cmd.Parameters.AddWithValue("p_genero", genero);
                    cmd.Parameters.AddWithValue("p_ano", ano);
                    cmd.Parameters.AddWithValue("p_isbn", isbn);
                    cmd.Parameters.AddWithValue("p_quantidade", quantidade);
                    cmd.Parameters.AddWithValue("p_capa_arquivo", (object?)relPath ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                TempData["ok"] = "Cadastro Realizado";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["ok"] = "Cadastro Não Realizado";
                Console.WriteLine($"Uma Exception aconteceu: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}


/*

 */