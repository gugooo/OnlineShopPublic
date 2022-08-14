using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class HomePageSection
    {
        public HomePageSection()
        {
            this.HomePageSectionDatas = new HashSet<HomePageSectionData>();
        }
        public string Id { get; set; }
        public int _Index { get; set; }
        public bool IsActive { get; set; }
        public bool IsFixedData { get; set; }
        public int Position { get; set; }
        public string SectionName { get; set; }
        public ICollection<HomePageSectionData> HomePageSectionDatas { get; set; }
    }
    public class HomePageSectionData
    {
        public HomePageSectionData()
        {
            this.Title = new HashSet<CultureData>();
            this.Description = new HashSet<CultureData>();
        }
        public string Id { get; set; }
        public int _Index { get; set; }
        public byte[] Img { get; set; }
        public ICollection<CultureData> Title { get; set; }
        public ICollection<CultureData> Description { get; set; }
        public bool TextIsWhith { get; set; }
        public Catalog Catalog { get; set; }

        public HomePageSection FK_HomePageSection { get; set; }

    }
}
