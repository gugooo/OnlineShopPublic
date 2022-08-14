using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Classes
{
    public static class CountriesList
    {
        static CountriesList()
        {
            CultureInfo[] CList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var CoutyriesName = CList.Select(_ => new RegionInfo(_.LCID)).Select(_ => _.EnglishName).Distinct().ToList();
            CoutyriesName.Remove("World");
            CoutyriesName.Sort();
            Countries = new SelectList(CoutyriesName);
        }
        public static SelectList Countries { get; }
    }
}
