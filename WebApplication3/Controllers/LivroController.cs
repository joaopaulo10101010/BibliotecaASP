using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DataBase;
using WebApplication3.Models;
using MySql.Data;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication3.Controllers
{
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
        public IActionResult Criar(string titulo, string autor, string editora, string genero, string ano, string isbn, string quantidade)
        {

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
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            


            return RedirectToAction("Criar");
        }
    }
}


/*
    Copa do mundo de 2026
 
 */