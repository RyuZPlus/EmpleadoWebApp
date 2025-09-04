namespace EmpleadoWebApp.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Direccion { get; set; }
        public int Edad { get; set; }
        public string Sexo { get; set; }
        public DateTime FechaIngreso { get; set; }
        public decimal Salario { get; set; }
        public string Sucursal { get; set; }
    }
}
