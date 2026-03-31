using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace MercadoPago.Wrapper.Helpers
{
    /// <summary>
    /// Valida que la referencia externa no contenga información sensible (PII).
    /// Emite warnings en el log si detecta patrones sospechosos.
    /// </summary>
    public static class ExternalReferenceValidator
    {
        // Patrones comunes de PII
        private static readonly Regex EmailPattern =
            new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
                RegexOptions.Compiled);

        private static readonly Regex PhonePattern =
            new Regex(@"\b\d{7,15}\b", RegexOptions.Compiled);

        private static readonly Regex DniCuitPattern =
            new Regex(@"\b\d{2}[\.\-]?\d{6,8}[\.\-]?\d{0,1}\b",
                RegexOptions.Compiled);

        /// <summary>
        /// Verifica si la referencia externa podría contener información sensible.
        /// Retorna true si es segura, false si detecta posibles PII.
        /// </summary>
        public static bool Validate(string externalReference, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(externalReference))
                return true;

            var warnings = new List<string>();

            if (EmailPattern.IsMatch(externalReference))
                warnings.Add("email");

            if (PhonePattern.IsMatch(externalReference))
                warnings.Add("teléfono");

            if (DniCuitPattern.IsMatch(externalReference))
                warnings.Add("DNI/CUIT");

            if (warnings.Any())
            {
                logger?.Warning(
                    "⚠️ La referencia externa '{ExternalReference}' podría contener " +
                    "información sensible ({PiiTypes}). MercadoPago requiere que " +
                    "la referencia externa NO contenga PII.",
                    externalReference, string.Join(", ", warnings));
                return false;
            }

            return true;
        }
    }
}
