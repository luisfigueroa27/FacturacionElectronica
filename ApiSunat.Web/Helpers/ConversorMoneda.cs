using System;
using System.Text.RegularExpressions;

namespace ApiSunat.Web.Helpers
{
    // Esta clase convierte un número decimal a su representación en letras para moneda
    public static class ConversorMoneda
    {
        private static readonly string[] _unidades = { "CERO", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        private static readonly string[] _decenas = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
        private static readonly string[] _centenas = { "_", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

        private static string ConvertirParteEntera(long numero)
        {
            if (numero == 0) return "CERO";
            if (numero < 0) return "MENOS " + ConvertirParteEntera(Math.Abs(numero));

            string letras = "";

            if ((numero / 1000000) > 0)
            {
                if ((numero / 1000000) == 1)
                    letras += "UN MILLON ";
                else
                    letras += ConvertirGrupo((numero / 1000000)) + " MILLONES ";
                numero %= 1000000;
            }

            if ((numero / 1000) > 0)
            {
                if ((numero / 1000) == 1)
                    letras += "MIL ";
                else
                    letras += ConvertirGrupo((numero / 1000)) + " MIL ";
                numero %= 1000;
            }

            if (numero > 0)
            {
                letras += ConvertirGrupo(numero);
            }

            return letras.Trim();
        }

        private static string ConvertirGrupo(long numero)
        {
            string grupo = "";

            if (numero == 100)
                grupo = "CIEN";
            else if (numero > 99)
            {
                grupo = _centenas[numero / 100] + " ";
                numero %= 100;
            }

            if (numero > 0)
            {
                if (numero < 10)
                    grupo += _unidades[numero];
                else if (numero < 20)
                    grupo += _decenas[numero - 10];
                else
                {
                    grupo += _decenas[(numero / 10) + 8];
                    if ((numero % 10) > 0)
                        grupo += " Y " + _unidades[numero % 10];
                }
            }
            return grupo;
        }

        /// <summary>
        /// Convierte un número decimal a letras en formato moneda.
        /// </summary>
        /// <param name="monto">El monto a convertir</param>
        /// <param name="mayusculas">Indica si el resultado debe estar en mayúsculas</param>
        /// <param name="monedaPlural">Nombre de la moneda en plural (ej. SOLES)</param>
        /// <param name="centavoPlural">Nombre del centavo en plural (ej. CÉNTIMOS)</param>
        /// <returns></returns>
        public static string Convertir(decimal monto, bool mayusculas = true, string monedaPlural = "SOLES", string centavoPlural = "CÉNTIMOS")
        {
            long parteEntera = (long)Math.Truncate(monto);
            int parteDecimal = (int)Math.Round((monto - parteEntera) * 100, 2);

            string letrasEntera = ConvertirParteEntera(parteEntera);
            string letrasDecimal = $"{parteDecimal:00}/100";

            string resultado = $"SON {letrasEntera} Y {letrasDecimal} {monedaPlural}";

            // Caso especial para 1 SOL
            if (parteEntera == 1)
            {
                resultado = $"SON UN Y {letrasDecimal} {monedaPlural}";
            }

            return mayusculas ? resultado.ToUpper() : resultado.ToLower();
        }
    }
}