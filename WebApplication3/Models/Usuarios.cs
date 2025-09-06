namespace WebApplication3.Models
{
    public class Usuarios
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
        public string senha_hash { get; set; }
        public Boolean ativo {  get; set; }
        public DateTime criado_em { get; set; }
        public string role { get; set; }
    }
}
