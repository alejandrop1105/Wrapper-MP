using System;
using System.Collections.Generic;
using System.Globalization;

namespace MercadoPago.Wrapper.Helpers
{
    /// <summary>
    /// Utilidad para formatear montos según el país de operación de MercadoPago.
    /// Chile y Colombia no soportan decimales; el resto usa 2 decimales con punto.
    /// </summary>
    public static class AmountFormatter
    {
        private static readonly HashSet<string> NoDecimalCountries =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CL", // Chile
                "CO"  // Colombia
            };

        /// <summary>
        /// Formatea un monto decimal al string esperado por MercadoPago
        /// según el país de operación.
        /// </summary>
        /// <param name="amount">Monto a formatear.</param>
        /// <param name="country">Código ISO del país (AR, BR, MX, CL, CO, etc.).</param>
        /// <returns>Monto formateado como string (ej: "1000.50" o "1000").</returns>
        public static string Format(decimal amount, string country)
        {
            if (NoDecimalCountries.Contains(country ?? "AR"))
            {
                return Math.Truncate(amount)
                    .ToString("F0", CultureInfo.InvariantCulture);
            }

            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Valida que el formato del monto sea correcto según el país.
        /// </summary>
        public static bool IsValidFormat(string amountString, string country)
        {
            if (string.IsNullOrWhiteSpace(amountString))
                return false;

            if (!decimal.TryParse(amountString,
                NumberStyles.Number, CultureInfo.InvariantCulture,
                out decimal value))
                return false;

            if (NoDecimalCountries.Contains(country ?? "AR"))
            {
                // No debe tener decimales
                return value == Math.Truncate(value);
            }

            return true;
        }
    }
}
