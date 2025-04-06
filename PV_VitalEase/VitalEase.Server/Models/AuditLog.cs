namespace VitalEase.Server.Models
{
    /// <summary>
    /// Representa um registo de auditoria que contém informações sobre uma ação realizada por um utilizador.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Obtém ou define o identificador único do registo de auditoria.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtém ou define o carimbo de data/hora em que a ação foi registada.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Obtém ou define a descrição da ação realizada.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Obtém ou define o estado ou resultado da ação.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Obtém ou define o identificador do utilizador associado à ação.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Regista a ação atual, definindo a descrição, o estado e atualizando o carimbo de data/hora para o momento atual.
        /// </summary>
        /// <param name="action">A descrição da ação a registar.</param>
        /// <param name="status">O estado ou resultado da ação.</param>
        public void LogAction(string action, string status)
        {
            Action = action;
            Status = status;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Filtra uma lista de registos de auditoria, retornando apenas aqueles cujo valor da propriedade <c>Action</c> corresponda ao valor especificado.
        /// </summary>
        /// <param name="action">A ação pela qual filtrar os registos.</param>
        /// <param name="logs">A lista de registos de auditoria a ser filtrada.</param>
        /// <returns>
        /// Uma lista de registos de auditoria que possuem o valor especificado na propriedade <c>Action</c>.
        /// </returns>
        public static List<AuditLog> FilterLogsByAction(string action, List<AuditLog> logs)
        {
            return logs.FindAll(log => log.Action == action);
        }

        /// <summary>
        /// Obtém todos os registos de auditoria.
        /// </summary>
        /// <returns>
        /// Uma lista contendo todos os registos de auditoria.
        /// </returns>
        /// <remarks>
        /// Este método está atualmente sem implementação completa e retorna uma nova lista vazia.
        /// </remarks>
        public static List<AuditLog> GetAllLogs()
        {
            // Implementation here
            return new List<AuditLog>();
        }
    }
}
