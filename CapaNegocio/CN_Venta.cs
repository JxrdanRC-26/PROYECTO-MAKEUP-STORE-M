using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaNegocio
{
    public class CN_Venta
    {
        private CD_Venta objcd_venta = new CD_Venta();

        public bool RestarStock(int idproducto, int cantidad)
        {
            return objcd_venta.RestarStock(idproducto, cantidad);
        }

        public bool SumarStock(int idproducto, int cantidad)
        {
            return objcd_venta.SumarStock(idproducto, cantidad);
        }

        public int ObtenerCorrelativo()
        {
            return objcd_venta.ObtenerCorrelativo();
        }

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            // Aplicar IVA en el monto total de cada detalle de venta
            foreach (DataRow filaDetalle in DetalleVenta.Rows)
            {
                decimal precioVenta = Convert.ToDecimal(filaDetalle["PrecioVenta"]);
                int cantidad = Convert.ToInt32(filaDetalle["Cantidad"]);

                // Calcular el monto total con IVA
                decimal montoTotalConIVA = CalcularMontoTotalItemConIVA(precioVenta, cantidad);
                filaDetalle["SubTotal"] = montoTotalConIVA;
            }

            // Calcular el monto total con IVA para toda la venta
            decimal montoTotalVentaConIVA = CalcularMontoTotalConIVA(DetalleVenta);

            // Verificar si el monto pagado es suficiente
            if (obj.MontoPago >= montoTotalVentaConIVA)
            {
                // Calcular el cambio correctamente
                decimal cambio = obj.MontoPago - montoTotalVentaConIVA;

                // Actualizar el monto total con IVA en el objeto Venta
                obj.MontoTotal = montoTotalVentaConIVA;

                // Actualizar el cambio en el objeto Venta
                obj.MontoCambio = cambio;

                // Luego, continuar con el resto del código de registro
                return objcd_venta.Registrar(obj, DetalleVenta, out Mensaje);
            }
            else
            {
                // El monto pagado es insuficiente
                Mensaje = "Monto pagado insuficiente";
                return false;
            }
        }

        private decimal CalcularMontoTotalItemConIVA(decimal precioVenta, int cantidad)
        {
            const decimal PorcentajeIVA = 0.16m; // 16% de IVA, ajusta según las leyes locales
            decimal montoTotalItem = precioVenta * cantidad;
            return montoTotalItem * (1 + PorcentajeIVA);
        }

        private decimal CalcularMontoTotalConIVA(DataTable DetalleVenta)
        {
            decimal montoTotalConIVA = 0;

            foreach (DataRow filaDetalle in DetalleVenta.Rows)
            {
                decimal subTotal = Convert.ToDecimal(filaDetalle["SubTotal"]);
                montoTotalConIVA += subTotal;
            }

            return montoTotalConIVA;
        }

        public Venta ObtenerVenta(string numero)
        {
            Venta oVenta = objcd_venta.ObtenerVenta(numero);

            if (oVenta.IdVenta != 0)
            {
                List<Detalle_Venta> oDetalleVenta = objcd_venta.ObtenerDetalleVenta(oVenta.IdVenta);
                oVenta.oDetalle_Venta = oDetalleVenta;
            }

            return oVenta;
        }
    }
}