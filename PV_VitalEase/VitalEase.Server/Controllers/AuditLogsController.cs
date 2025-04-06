using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using VitalEase.Server.Data;
using VitalEase.Server.Models;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos logs de auditoria.
    /// </summary>
    public class AuditLogsController : Controller
    {
        /// <summary>
        /// Contexto da base de dados utilizado para operações relativas aos logs de auditoria.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Inicializa uma nova instância do <see cref="AuditLogsController"/>.
        /// </summary>
        /// <param name="context">
        /// O contexto da base de dados (<see cref="VitalEaseServerContext"/>) injetado para efetuar operações de acesso à base de dados.
        /// </param>
        public AuditLogsController(VitalEaseServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os logs de auditoria da base de dados.
        /// </summary>
        /// <remarks>
        /// Antes de recolher os logs, invoca o método <see cref="DeleteLogs"/> para eliminar logs desnecessários ou antigos.
        /// Em seguida, tenta recolher todos os logs disponíveis na base de dados. Caso a lista de logs esteja vazia, 
        /// retorna um BadRequest com uma lista vazia. Se os logs forem recolhidos com sucesso, são retornados em formato JSON.
        /// Se ocorrer alguma exceção durante o processo, retorna um BadRequest com uma mensagem de erro e os detalhes da exceção.
        /// </remarks>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém os logs de auditoria ou uma mensagem de erro.
        /// </returns>
        [HttpGet("api/getLogs")]
        public async Task<IActionResult> GetLogs()
        {
            await DeleteLogs();
            try
            {
                // Fetch all logs from the database
                var logs = await _context.AuditLogs.ToListAsync();


                if (logs.IsNullOrEmpty()) {
                    return BadRequest(new List<AuditLog>());
                }

                // Return the logs in JSON format
                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Handle error and return a bad request response
                return BadRequest(new { message = "Error fetching logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os logs de auditoria filtrados com base nos parâmetros facultativos fornecidos.
        /// </summary>
        /// <param name="userId">
        /// Opcional. O identificador do utilizador para filtrar os logs. Se fornecido, apenas os logs correspondentes a esse utilizador serão incluídos.
        /// </param>
        /// <param name="userEmail">
        /// Opcional. O email do utilizador. Se fornecido, o método procura o identificador do utilizador associado a esse email e filtra os logs por esse ID.
        /// </param>
        /// <param name="dateFrom">
        /// Opcional. A data de início para filtrar os logs. Apenas serão incluídos os logs com o <c>Timestamp</c> igual ou superior a esta data.
        /// </param>
        /// <param name="dateTo">
        /// Opcional. A data de fim para filtrar os logs. Apenas serão incluídos os logs com o <c>Timestamp</c> inferior ou igual a esta data.
        /// </param>
        /// <param name="actionType">
        /// Opcional. Um filtro por tipo de acção. Apenas serão incluídos os logs cuja propriedade <c>Action</c> contenha este valor.
        /// </param>
        /// <param name="status">
        /// Opcional. Um filtro pelo estado. Apenas serão incluídos os logs cuja propriedade <c>Status</c> contenha este valor.
        /// </param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que contém, em caso de sucesso, uma lista de logs filtrados em formato JSON. 
        /// Se ocorrer um erro, retorna um BadRequest com uma mensagem de erro.
        /// </returns>
        [HttpGet("api/getLogsFilter")]
        public async Task<IActionResult> GetLogs(int? userId, string? userEmail, DateTime? dateFrom, DateTime? dateTo, string? actionType, string? status)
        {
            await DeleteLogs();
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (userId.HasValue && userId >= 0)
                {
                    query = query.Where(log => log.UserId == userId);
                }

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userIdByEmail = await _context.Users.Where(user => user.Email == userEmail).Select(user => user.Id).FirstOrDefaultAsync();
                    query = query.Where(log => log.UserId == userIdByEmail);
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= dateFrom);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(log => log.Timestamp <= dateTo);
                }

                if (!string.IsNullOrEmpty(actionType))
                {
                    query = query.Where(log => log.Action.Contains(actionType));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(log => log.Status.Contains(status));
                }

                // Fetch all logs with UserID 
                var logs = await query.ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                //Handle error and return a bad request response
                return BadRequest(new { message = $"Error fetching logs from user with the id" });
            }
        }

        /// <summary>
        /// Elimina os registos de auditoria que tenham sido criados há mais de 6 meses.
        /// </summary>
        /// <remarks>
        /// Este método faz o seguinte:
        /// <list type="bullet">
        ///   <item>Obtém a data e hora atual.</item>
        ///   <item>Seleciona todos os registos de auditoria com um Timestamp anterior ou igual à data atual menos 6 meses.</item>
        ///   <item>Se não existirem registos a eliminar, retorna um BadRequest com uma lista vazia.</item>
        ///   <item>Se existirem, elimina os registos selecionados e guarda as alterações na base de dados.</item>
        ///   <item>Retorna os registos eliminados com uma resposta OK.</item>
        ///   <item>Em caso de erro, retorna um BadRequest com uma mensagem de erro.</item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Um <see cref="IActionResult"/> contendo os registos eliminados ou uma mensagem de erro em caso de falha.
        /// </returns>
        [HttpDelete]
        private async Task <IActionResult> DeleteLogs()
        {
            try
            {
            DateTime now = DateTime.Now;
            var logs = await _context.AuditLogs.Where(logs => logs.Timestamp <= now.AddMonths(-6)).ToListAsync();

            if (logs.IsNullOrEmpty())
            {
                return BadRequest(new List<AuditLog>());
            }
           
            _context.AuditLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return Ok(logs);

            }
            catch
            {
                return BadRequest(new { message = $"Error Deleting Logs" });
            }
        }
    }
}
