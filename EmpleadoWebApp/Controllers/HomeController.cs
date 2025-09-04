using EmpleadoWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace EmpleadoWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ================= MVC =================
        public IActionResult Index(
            int pageNumber = 1,
            int pageSize = 5,
            int? IdEmpleado = null,
            string Nombre = null,
            DateTime? FechaInicio = null,
            DateTime? FechaFin = null,
            int? IdSucursal = null)
        {
            var (empleados, totalRecords) = ObtenerEmpleados(pageNumber, pageSize, IdEmpleado, Nombre, FechaInicio, FechaFin, IdSucursal);

            // Paginación
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(empleados);
        }

        // ================= API REST =================
        [HttpGet]
        [Route("api/[controller]/GetEmpleados")]
        public IActionResult GetEmpleadosJson(
            int pageNumber = 1,
            int pageSize = 5,
            int? IdEmpleado = null,
            string Nombre = null,
            DateTime? FechaInicio = null,
            DateTime? FechaFin = null,
            int? IdSucursal = null)
        {
            var (empleados, totalRecords) = ObtenerEmpleados(pageNumber, pageSize, IdEmpleado, Nombre, FechaInicio, FechaFin, IdSucursal);

            var result = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Empleados = empleados
            };

            return Ok(result);
        }

        // ================= Método privado común =================
        private (List<Empleado> empleados, int totalRecords) ObtenerEmpleados(
            int pageNumber, int pageSize,
            int? IdEmpleado, string Nombre,
            DateTime? FechaInicio, DateTime? FechaFin,
            int? IdSucursal)
        {
            var empleados = new List<Empleado>();
            int totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetEmpleadosPaginados", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@IdEmpleado", (object)IdEmpleado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", (object)Nombre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInicio", (object)FechaInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object)FechaFin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdSucursal", (object)IdSucursal ?? DBNull.Value);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empleados.Add(new Empleado
                        {
                            IdEmpleado = Convert.ToInt32(reader["IdEmpleado"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellidos = reader["Apellidos"].ToString(),
                            Direccion = reader["Direccion"].ToString(),
                            Edad = Convert.ToInt32(reader["Edad"]),
                            Sexo = reader["Sexo"].ToString(),
                            FechaIngreso = Convert.ToDateTime(reader["FechaIngreso"]),
                            Salario = Convert.ToDecimal(reader["Salario"]),
                            Sucursal = reader["Sucursal"].ToString()
                        });
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                    }
                }
            }

            return (empleados, totalRecords);
        }
    }
}
