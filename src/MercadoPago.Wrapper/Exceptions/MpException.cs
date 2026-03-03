using System;

namespace MercadoPago.Wrapper.Exceptions
{
    /// <summary>
    /// Excepción base para errores de la API de MercadoPago.
    /// </summary>
    public class MpException : Exception
    {
        /// <summary>Código HTTP de la respuesta.</summary>
        public int StatusCode { get; }

        /// <summary>Código de error de MercadoPago (ej: "bad_request").</summary>
        public string ErrorCode { get; }

        /// <summary>Detalle de las causas del error devueltas por la API.</summary>
        public string[] Causes { get; }

        public MpException(string message, int statusCode = 0, string errorCode = null, string[] causes = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode ?? string.Empty;
            Causes = causes ?? Array.Empty<string>();
        }

        public MpException(string message, Exception innerException, int statusCode = 0)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = string.Empty;
            Causes = Array.Empty<string>();
        }
    }

    /// <summary>Error de autenticación (401).</summary>
    public class MpAuthenticationException : MpException
    {
        public MpAuthenticationException(string message = "Token de acceso inválido o expirado.")
            : base(message, 401, "unauthorized") { }
    }

    /// <summary>Error de validación (400).</summary>
    public class MpValidationException : MpException
    {
        public MpValidationException(string message, string[] causes = null)
            : base(message, 400, "bad_request", causes) { }
    }

    /// <summary>Recurso no encontrado (404).</summary>
    public class MpNotFoundException : MpException
    {
        public MpNotFoundException(string message = "Recurso no encontrado.")
            : base(message, 404, "not_found") { }
    }

    /// <summary>Límite de rate exceeded (429).</summary>
    public class MpRateLimitException : MpException
    {
        public MpRateLimitException(string message = "Límite de solicitudes excedido. Reintente en unos segundos.")
            : base(message, 429, "rate_limit") { }
    }

    /// <summary>Error interno del servidor de MP (500+).</summary>
    public class MpServerException : MpException
    {
        public MpServerException(string message, int statusCode)
            : base(message, statusCode, "server_error") { }
    }
}
