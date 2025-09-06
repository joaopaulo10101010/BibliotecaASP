using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication3.Models
{
    public class Livros
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = "";

        public int? AutorId { get; set; }

        public int? EditoraId { get; set; }

        public int? GeneroId { get; set; }

        public short? Ano { get; set; }

        public string? Isbn { get; set; }

        public int QuantidadeTotal { get; set; }

        public int QuantidadeDisponivel { get; set; }

        public DateTime CriadoEm { get; set; }

        public string Autor { get; set; }

        public string Editora { get; set; }

        public string Genero { get; set; }



        public List<SelectListItem> AutorNome { get; set; } = new();

        public List<SelectListItem> EditoraNome { get; set; } = new();

        public List<SelectListItem> GeneroNome { get; set; } = new();
    }
}
