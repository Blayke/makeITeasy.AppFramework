﻿using System;

using makeITeasy.AppFramework.Web.Models;

namespace makeITeasy.CarCatalog.WebApp.dotnet6.Models.Datatables
{
    public class CarDatatable : DatatableBaseConfiguration<CarDatatableSearchViewModel, CarDatatableViewModel>
    {
        public CarDatatable(string url) : base(url)
        {
            Options.PageLength = 5;
            Options.LoadOnDisplay = true;
            Options.ActivateDoubleClickOnRow = true;
            Options.EnablePaging = true;
            Options.Responsive = true;
        }
    }
}
