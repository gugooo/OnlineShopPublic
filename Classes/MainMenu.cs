using AlcantaraNew.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Classes
{
    public interface IMainMenu
    {
        Task<IEnumerable<IMenuElement>> GetMenu(string Culture);
    }
    public interface IMenuElement
    {
        string Name { get; }
        int Id { get; }
        IEnumerable<IMenuElement> ChildSection { get; }
    }

    class TwoLevelMenu : IMainMenu
    {
        private readonly CacheManager Cache;
        public TwoLevelMenu(CacheManager cacheManager)
        {
            Cache = cacheManager;
        }
        public async Task<IEnumerable<IMenuElement>> GetMenu(string Culture)
        {
            try
            {
                var TempCatalog = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>).Where(_ => _.FatherCatalog == null && _.IsActive).OrderBy(_ => _.Id).ToList();
                if (TempCatalog == null || TempCatalog.Count == 0) return null;
                var me = TempCatalog.Select(_ => new MElement()
                {
                    Name = CultureData.GetDefoultName(_.CultureName, Culture, _.Name),
                    Id = _.Id,
                    ChildSection = _.ChaildCatalogs?.Where(ch => ch.IsActive).OrderBy(o => o.Id).Select(ch => new MElement() { ChildSection = null, Name = CultureData.GetDefoultName(ch.CultureName, Culture, ch.Name), Id = ch.Id })
                });
                return me;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
    class MElement : IMenuElement
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public IEnumerable<IMenuElement> ChildSection { get; set; }
    }
}
