using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace MercadoPago.Demo.WinForms.Data.Repositories
{
    // ─── Modelos locales de la demo ───

    public class AppConfigEntity
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string PublicKey { get; set; }
        public string Environment { get; set; }
        public string WebhookSecret { get; set; }
        public int WebhookPort { get; set; }
        public string Country { get; set; }
        public string UserId { get; set; }

        // Homologación
        public string PlatformId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
    }

    public class OperationLogEntity
    {
        public int Id { get; set; }
        public string OperationType { get; set; }
        public string ExternalId { get; set; }
        public string ExternalReference { get; set; }
        public string Status { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public string ErrorMessage { get; set; }
        public string CreatedAt { get; set; }
    }

    public class WebhookLogEntity
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public string ResourceId { get; set; }
        public string Action { get; set; }
        public string RawJson { get; set; }
        public int Processed { get; set; }
        public string ReceivedAt { get; set; }
    }

    // ─── Repositorios ───

    /// <summary>Repositorio de configuración de la aplicación.</summary>
    public class ConfigRepository
    {
        public AppConfigEntity Get()
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                return db.QueryFirstOrDefault<AppConfigEntity>(
                    "SELECT * FROM AppConfig WHERE Id = 1");
            }
        }

        public void Save(AppConfigEntity config)
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                db.Execute(@"
                    UPDATE AppConfig SET 
                        AccessToken = @AccessToken,
                        PublicKey = @PublicKey,
                        Environment = @Environment,
                        WebhookSecret = @WebhookSecret,
                        WebhookPort = @WebhookPort,
                        Country = @Country,
                        UserId = @UserId,
                        PlatformId = @PlatformId,
                        ClientId = @ClientId,
                        ClientSecret = @ClientSecret,
                        RefreshToken = @RefreshToken
                    WHERE Id = 1", config);
            }
        }
    }

    /// <summary>Repositorio de log de operaciones.</summary>
    public class OperationLogRepository
    {
        public void Insert(OperationLogEntity entry)
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                db.Execute(@"
                    INSERT INTO OperationLog 
                        (OperationType, ExternalId, ExternalReference, 
                         Status, Amount, Currency, 
                         RequestJson, ResponseJson, ErrorMessage)
                    VALUES 
                        (@OperationType, @ExternalId, @ExternalReference,
                         @Status, @Amount, @Currency,
                         @RequestJson, @ResponseJson, @ErrorMessage)",
                    entry);
            }
        }

        public List<OperationLogEntity> GetRecent(int limit = 100)
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                return db.Query<OperationLogEntity>(
                    "SELECT * FROM OperationLog ORDER BY Id DESC LIMIT @Limit",
                    new { Limit = limit }).ToList();
            }
        }
    }

    /// <summary>Repositorio de webhooks recibidos.</summary>
    public class WebhookLogRepository
    {
        public void Insert(WebhookLogEntity entry)
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                db.Execute(@"
                    INSERT INTO WebhookLog 
                        (EventType, ResourceId, Action, RawJson)
                    VALUES (@EventType, @ResourceId, @Action, @RawJson)",
                    entry);
            }
        }

        public List<WebhookLogEntity> GetRecent(int limit = 100)
        {
            using (var db = DatabaseInitializer.CreateConnection())
            {
                return db.Query<WebhookLogEntity>(
                    "SELECT * FROM WebhookLog ORDER BY Id DESC LIMIT @Limit",
                    new { Limit = limit }).ToList();
            }
        }
    }
}
