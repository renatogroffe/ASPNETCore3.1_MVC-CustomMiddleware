using System;

namespace ExemploSiteMiddleware.Models
{
    public class Indisponibilidade
    {
        public int Id { get; set; }
        public DateTime InicioIndisponibilidade { get; set; }
        public DateTime TerminoIndisponibilidade { get; set; }
        public string Mensagem { get; set; }
    }
}