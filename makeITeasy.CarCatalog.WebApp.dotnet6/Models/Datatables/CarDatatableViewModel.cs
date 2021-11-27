﻿using System;

using makeITeasy.AppFramework.Core.Interfaces;
using makeITeasy.AppFramework.Web.Attributes;
using makeITeasy.CarCatalog.Models;

namespace makeITeasy.CarCatalog.WebApp.dotnet6.Models.Datatables
{
    public class CarDatatableViewModel : IMapFrom<Car>
    {
        [TableColumn(Name = nameof(ID), Title = "ID", IsRowId = true, Priority = 1, Visible = false)]
        public long ID { get; set; }

        [TableColumn(Name = nameof(Name), Title = "Nom", Priority = 2)]
        public string Name { get; set; }

        [TableColumn(Name = nameof(BrandName), Title = "Marque", SortDataSource = "Brand.Name", Priority = 3)]

        public string BrandName { get; set; }

        [TableColumn(Name = nameof(BrandCountryCountryCode), Title = "Pays", SortDataSource = "Brand.Country.CountryCode")]

        public string BrandCountryCountryCode { get; set; }

        [TableColumn(Name = nameof(IsModernCar), Title = "Actual", Sortable = false, Priority = 4)]
        public bool IsModernCar { get; set; }

        [TableColumn(Name = nameof(IsItalianCar), Title = "Italian", Sortable = false, Priority = 5)]
        public bool IsItalianCar { get; set; }

        [TableColumn(Name = nameof(Edit), Title = "", Priority = 2)]
        public string Edit { get; set; } = string.Empty;
    }
}
