using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication3.Authenticacao;
using WebApplication3.DataBase;
using WebApplication3.Models;
namespace WebApplication3.Controllers
{
    public class EmprestimoController : Controller
    {
        private readonly Database db = new Database();
        private const string CART_KEY = "Carrinho";
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Vitrine(string? q)
        {
            var itens = new List<Livros>();
            var titulos = new List<string>();

            using var conn = db.GetConnection();

            using (var cmd = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = System.Data.CommandType.StoredProcedure})
            {
                cmd.Parameters.AddWithValue("p_q", q ?? "");
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    itens.Add(new Livros
                    {
                        Id = rd.GetInt32("id"),
                        Titulo = rd.GetString("titulo"),
                        CapaArquivo = rd["capa_arquivo"] as string
                    });
                }
            }
            using (var cmdAll = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmdAll.Parameters.AddWithValue("p_q", "");
                using var rd2 = cmdAll.ExecuteReader();
                while (!rd2.Read())
                {
                    var titulo = rd2.GetString("titulo");
                    if (!string.IsNullOrWhiteSpace(titulo) && !titulo.Contains(titulo))
                        titulos.Add(titulo);
                }
            }

            ViewBag.q = q ?? "";
            ViewBag.Titulos = titulos;

            return View(itens);
        }

        private List<int> GetCartIds()

        {

            var csv = HttpContext.Session.GetString(CART_KEY);

            var list = new List<int>();

            if (string.IsNullOrWhiteSpace(csv)) return list;



            foreach (var s in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))

                if (int.TryParse(s, out var id) && !list.Contains(id))

                    list.Add(id);



            return list;

        }



        private void SaveCartIds(List<int> ids)

        {

            var csv = string.Join(",", ids);

            if (string.IsNullOrEmpty(csv))

                HttpContext.Session.Remove(CART_KEY);

            else

                HttpContext.Session.SetString(CART_KEY, csv);

        }


        [HttpPost, ValidateAntiForgeryToken]

        public IActionResult AdicionarAoCarrinho(int id)

        {

            var ids = GetCartIds();

            if (!ids.Contains(id)) ids.Add(id); // 1 por livro 

            SaveCartIds(ids);



            TempData["ok"] = "Livro adicionado ao carrinho.";

            return RedirectToAction(nameof(Vitrine));

        }



        // GET Carrinho: monta linhas a partir dos IDs + carrega leitores para o datalist 

        [HttpGet]

        public IActionResult Carrinho()

        {

            var ids = GetCartIds();

            var linhas = new List<Carrinho>();



            if (ids.Count > 0)

            {

                var idsCsv = string.Join(",", ids);



                using var conn = db.GetConnection();

                using (var cmd = new MySqlCommand("sp_livro_listar_por_ids", conn) { CommandType = CommandType.StoredProcedure })

                {

                    cmd.Parameters.AddWithValue("p_ids", idsCsv);

                    using var rd = cmd.ExecuteReader();

                    while (rd.Read())

                    {

                        linhas.Add(new Carrinho

                        {

                            LivroId = rd.GetInt32("id"),

                            Titulo = rd.GetString("titulo"),

                            CapaArquivo = rd["capa_arquivo"] as string,

                            Quantidade = 1 // sempre 1 por livro 

                        });

                    }

                }



                // Carrega leitores (id, nome) para o datalist 

                var leitores = new List<(int Id, string Nome)>();

                using (var cmd2 = new MySqlCommand("sp_leitor_listar", conn) { CommandType = CommandType.StoredProcedure })

                using (var rd2 = cmd2.ExecuteReader())

                {

                    while (rd2.Read())

                        leitores.Add((rd2.GetInt32("id_leitor"), rd2.GetString("nomeleitor")));

                }

                ViewBag.Leitores = leitores;

            }

            else

            {

                ViewBag.Leitores = new List<(int, string)>();

            }



            return View(linhas.OrderBy(x => x.Titulo).ToList());

        }



        // Remove 1 livro (pelo ID) do carrinho 

        [HttpPost, ValidateAntiForgeryToken]

        public IActionResult RemoverDoCarrinho(int id)

        {

            var ids = GetCartIds();

            if (ids.Remove(id))

                SaveCartIds(ids);



            return RedirectToAction(nameof(Carrinho));

        }



        // =================== Finalizar (transação + SPs) =================== 

        [HttpPost, ValidateAntiForgeryToken]

        public IActionResult Finalizar(int idLeitor, DateTime dataPrevista)

        {

            // validações simples antes de abrir transação 

            if (idLeitor <= 0)

            {

                TempData["ok"] = "Selecione um leitor válido da lista.";

                return RedirectToAction(nameof(Carrinho));

            }



            var ids = GetCartIds();

            if (ids.Count == 0)

            {

                TempData["ok"] = "Carrinho vazio.";

                return RedirectToAction(nameof(Vitrine));

            }



            var idBibliotecario = HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

            if (idBibliotecario == 0) return RedirectToAction("Login", "Auth");



            using var conn = db.GetConnection();

            using var tx = conn.BeginTransaction();



            try

            {

                // 1) Cabeçalho (OUT id gerado) 

                int idEmp;

                using (var cmd = new MySqlCommand("sp_emprestimo_criar", conn, tx) { CommandType = CommandType.StoredProcedure })

                {

                    cmd.Parameters.AddWithValue("p_id_leitor", idLeitor);

                    cmd.Parameters.AddWithValue("p_id_bibliotecario", idBibliotecario);

                    cmd.Parameters.AddWithValue("p_data_prevista", dataPrevista.Date);

                    var pOut = new MySqlParameter("p_id_gerado", MySqlDbType.Int32) { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(pOut);

                    cmd.ExecuteNonQuery();

                    idEmp = Convert.ToInt32(pOut.Value);

                }



                // 2) Itens (cada livro com qtd = 1; SP valida/baixa estoque) 

                foreach (var livroId in ids)

                {

                    using var cmdI = new MySqlCommand("sp_emprestimo_adicionar_item", conn, tx) { CommandType = CommandType.StoredProcedure };

                    cmdI.Parameters.AddWithValue("p_id_emprestimo", idEmp);

                    cmdI.Parameters.AddWithValue("p_id_livro", livroId);

                    cmdI.Parameters.AddWithValue("p_qtd", 1);

                    cmdI.ExecuteNonQuery();

                }



                tx.Commit();

                HttpContext.Session.Remove(CART_KEY);

                TempData["ok"] = $"Empréstimo #{idEmp} criado!";

            }

            catch (MySqlException ex)

            {

                tx.Rollback();

                TempData["ok"] = $"Falha ao finalizar: {ex.Message}";

            }



            return RedirectToAction(nameof(Vitrine));

        }

    }

} 
    

