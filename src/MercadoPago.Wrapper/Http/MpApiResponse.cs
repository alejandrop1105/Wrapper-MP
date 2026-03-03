using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Http
{
    /// <summary>
    /// Respuesta genérica de la API de MercadoPago.
    /// </summary>
    public class MpApiResponse<T>
    {
        /// <summary>Código de estado HTTP.</summary>
        public int StatusCode { get; set; }

        /// <summary>Indica si la solicitud fue exitosa (2xx).</summary>
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

        /// <summary>Datos deserializados de la respuesta.</summary>
        public T Data { get; set; }

        /// <summary>JSON crudo de la respuesta.</summary>
        public string RawJson { get; set; }

        /// <summary>Mensaje de error (si aplica).</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Código de error de MP (si aplica).</summary>
        public string ErrorCode { get; set; }

        /// <summary>Causas del error (si aplica).</summary>
        public List<string> Causes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Respuesta paginada de la API de MercadoPago.
    /// </summary>
    public class MpPaginatedResponse<T>
    {
        [JsonProperty("paging")]
        public MpPaging Paging { get; set; }

        [JsonProperty("results")]
        public List<T> Results { get; set; } = new List<T>();
    }

    /// <summary>
    /// Información de paginación.
    /// </summary>
    public class MpPaging
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
