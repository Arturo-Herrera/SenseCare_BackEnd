using MongoDB.Bson.Serialization.Attributes;

namespace SenseCareLocal.Models.Informe
{
    public class Tratamiento
    {
        [BsonElement("medicamento")]
        public string Medicamento { get; set; }

        [BsonElement("dosis")]
        public string Dosis { get; set; }

        [BsonElement("duracion")]
        public string Duracion { get; set; }
    }

}
