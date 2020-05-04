using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using ExemploSiteMiddleware.Models;

namespace ExemploSiteMiddleware
{
    public class ChecagemIndisponibilidade
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ChecagemIndisponibilidade> _logger;

        public ChecagemIndisponibilidade(RequestDelegate next,
            ILogger<ChecagemIndisponibilidade> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            IConfiguration config = (IConfiguration)httpContext
                .RequestServices.GetService(typeof(IConfiguration));
            Indisponibilidade indisponibilidade = null;

            using (SqlConnection conexao = new SqlConnection(
                config.GetConnectionString("BaseMiddlewareDisponibilidade")))
            {
                indisponibilidade = conexao.QueryFirstOrDefault<Indisponibilidade>(
                    "SELECT TOP 1 * FROM dbo.Indisponibilidade " +
                    "WHERE @DataProcessamento BETWEEN InicioIndisponibilidade " +
                      "AND TerminoIndisponibilidade " +
                    "ORDER BY InicioIndisponibilidade",
                    new { DataProcessamento = DateTime.Now });

                conexao.Close();
            }

            if (indisponibilidade == null)
                await _next(httpContext);
            else
            {
                httpContext.Response.StatusCode = 403;
                _logger.LogWarning(
                    JsonSerializer.Serialize(indisponibilidade));
                await httpContext.Response.WriteAsync(
                    $"<h1>{indisponibilidade.Mensagem}</h1>");
            }
        }
    }

    public static class ChecagemIndisponibilidadeExtensions
    {
        public static IApplicationBuilder UseChecagemIndisponibilidade(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ChecagemIndisponibilidade>();
        }
   }
}