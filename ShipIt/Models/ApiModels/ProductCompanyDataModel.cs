using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;

namespace ShipIt.Models.DataModels
{
    // public class DatabaseColumnName : Attribute
    // {
    //     public string Name { get; set; }

    //     public DatabaseColumnName(string name)
    //     {
    //         Name = name;
    //     }
    // }


    // public abstract class DataModel
    // {
    //     protected DataModel()
    //     {
    //     }

    //     public DataModel(IDataReader dataReader)
    //     {
    //         var type = GetType();
    //         var properties = type.GetProperties();

    //         foreach (var property in properties)
    //         {
    //             var attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
    //             property.SetValue(this, dataReader[attribute.Name], null);
    //         }
    //     }

    //     public IEnumerable<NpgsqlParameter> GetNpgsqlParameters()
    //     {
    //         var type = GetType();
    //         var properties = type.GetProperties();
    //         var parameters = new List<NpgsqlParameter>();

    //         foreach (var property in properties)
    //         {
    //             var attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
    //             parameters.Add(new NpgsqlParameter("@" + attribute.Name,property.GetValue(this, null)));
    //         }

    //         return parameters;
    //     }
    // }

    public class ProductCompanyDataModel : DataModel
    {
       [DatabaseColumnName("p_id")]
        public int Id { get; set; }

        [DatabaseColumnName("gtin_cd")]
        public string Gtin { get; set; }

        [DatabaseColumnName("gcp_cd")]
        public string Gcp { get; set; }

        [DatabaseColumnName("gtin_nm")]
        public string ProductName { get; set; }

        [DatabaseColumnName("m_g")]
        public double Weight { get; set; }

        [DatabaseColumnName("l_th")]
        public int LowerThreshold { get; set; }

        [DatabaseColumnName("ds")]
        public int Discontinued { get; set; }

        [DatabaseColumnName("min_qt")]
        public int MinimumOrderQuantity { get; set; }
        
        [DatabaseColumnName("gln_nm")]
        public string CompanyName { get; set; }
        [DatabaseColumnName("gln_addr_02")]
        public string Addr2 { get; set; }
        [DatabaseColumnName("gln_addr_03")]
        public string Addr3 { get; set; }
        [DatabaseColumnName("gln_addr_04")]
        public string Addr4 { get; set; }
        [DatabaseColumnName("gln_addr_postalcode")]
        public string PostalCode { get; set; }
        [DatabaseColumnName("gln_addr_city")]
        public string City { get; set; }
        [DatabaseColumnName("contact_tel")]
        public string Tel { get; set; }
        [DatabaseColumnName("contact_mail")]
        public string Mail { get; set; }

        public ProductCompanyDataModel(IDataReader dataReader) : base(dataReader)
        { }

        public ProductCompanyDataModel()
        { }

        public ProductCompanyDataModel(ProductCompanyDataModel dataModel)
        {
            Id = dataModel.Id;
            Gtin = dataModel.Gtin;
            Gcp = dataModel.Gcp;
            ProductName = dataModel.ProductName;
            Weight = dataModel.Weight;
            LowerThreshold = dataModel.LowerThreshold;
            Discontinued = dataModel.Discontinued;
            MinimumOrderQuantity = dataModel.MinimumOrderQuantity;
            CompanyName = dataModel.CompanyName;
            Addr2 = dataModel.Addr2;
            Addr3 = dataModel.Addr3;
            Addr4 = dataModel.Addr4;
            PostalCode = dataModel.PostalCode;
            City = dataModel.City;
            Tel = dataModel.Tel;
            Mail = dataModel.Mail;

        }
    }

}