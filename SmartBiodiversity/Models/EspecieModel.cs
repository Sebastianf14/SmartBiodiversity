using System.Text.Json.Serialization;


namespace SmartBiodiversity.Models
{
    public class EspecieModel
    {
        private string _id;

        [JsonPropertyName("id")]
        public string Id { get => _id; set => _id = value; }

        [JsonPropertyName("id_especies")]
        public string IdEspeciesSnake { set => _id = value; }

        [JsonPropertyName("idEspecies")]
        public string IdEspeciesCamel { set => _id = value; }

        private string _nombre;

        [JsonPropertyName("nombreComun")]
        public string Nombre { get => _nombre; set => _nombre = value; }

        [JsonPropertyName("nombre_comunesp")]
        public string NombreComunespSnake { set => _nombre = value; }

        [JsonPropertyName("nombreComunesp")]
        public string NombreComunespCamel { set => _nombre = value; }

        private string _descripcion;

        [JsonPropertyName("descripcion")]
        public string DescripcionLarga { get => _descripcion; set => _descripcion = value; }

        [JsonPropertyName("descripcionesp")]
        public string DescripcionespSnake { set => _descripcion = value; }

        [JsonPropertyName("descripcionEsp")]
        public string DescripcionEspCamel { set => _descripcion = value; }

        private string _habitat;

        [JsonPropertyName("habitat")]
        public string DescripcionCorta { get => _habitat; set => _habitat = value; }

        [JsonPropertyName("habitatEsp")]
        public string HabitatEspSnake { set => _habitat = value; }

        private string _categoriaId;

        [JsonPropertyName("categoriaId")]
        public string CategoriaId { get => _categoriaId; set => _categoriaId = value; }

        [JsonPropertyName("id_categoriasesp")]
        public string IdCategoriasespSnake { set => _categoriaId = value; }

        [JsonPropertyName("idCategoriasesp")]
        public string IdCategoriasespCamel { set => _categoriaId = value; }

        [JsonPropertyName("idCategoria")]
        public string IdCategoria { set => _categoriaId = value; }

        public string Tamano { get; set; } = "N/A";
        public string Dieta { get; set; } = "N/A";
        public string Estado { get; set; } = "N/A";

        public string ImagenUrl { get; set; } = "flora_icono.png";
    }

    public class CategoriaItem
    {
        private string _id;
        [JsonPropertyName("id")]
        public string Id { get => _id; set => _id = value; }

        [JsonPropertyName("id_categorias")]
        public string IdCategoriasSnake { set => _id = value; }

        private string _nombre;
        [JsonPropertyName("nombre")]
        public string Nombre { get => _nombre; set => _nombre = value; }

        [JsonPropertyName("nombrecat")]
        public string NombrecatSnake { set => _nombre = value; }
    }
}