namespace WebApplication3.Models
{
    public class Carrinho
    {
        public int LivroId { get; set; }
        public string Titulo { get; set; } = "";
        public string? CapaArquivo { get; set; }
        public int Quantidade { get; set; }
        public int Disponivel { get; set; }
    }
}
