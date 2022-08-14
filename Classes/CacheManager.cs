using AlcantaraNew.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlcantaraNew.Classes;

namespace AlcantaraNew.Classes
{
    public class CacheManager
    {
        private readonly AlcantaraDBContext DBContext;
        private readonly IMemoryCache _Cache;
        private readonly UserManager<User> userManager;
        public CacheManager(AlcantaraDBContext context,IMemoryCache cache, UserManager<User> _userManager)
        {
            DBContext = context;
            _Cache = cache;
            userManager = _userManager;
        }
        //*****************************************************************************************
        public enum Key { GlobalSetings, LiveChatSessions, Users, Atributes, Catalog, Product, Review, Promo, Order, HomePageSections };
        private async Task<IEnumerable<object>> getDB_Object(Key key)
        {
            switch (key)
            {
                case Key.GlobalSetings:return await DBContext.GlobalSetings.ToListAsync(); ;
                case Key.LiveChatSessions:return await DBContext.LiveChatSessions.ToListAsync();
                case Key.Users:return await userManager.Users.ToListAsync();
                case Key.Atributes:return await DBContext.Atributes.Include(_=>_.CultureName).Include(_ => _.Values).ThenInclude(_=>_.CultureName).ToListAsync();
                case Key.Catalog: var cats = await DBContext.Catalogs.Include(_=>_.ChaildCatalogs).Include(_=>_.CultureName).Include(_=>_.Products).ToListAsync();
                    cats?.ForEach(_ => _.Products = _.Products.DistinctBy(p => p.Id).ToArray());
                    return cats;
                case Key.Product: var pr= await DBContext.Products
                        .Include(_ => _.ProductTypes).ThenInclude(_ => _.CultureBrand)
                        .Include(_ => _.ProductTypes).ThenInclude(_ => _.CultureTitle)
                        .Include(_ => _.ProductTypes).ThenInclude(_ => _.CultureDescription)
                        .Include(_=>_.ProductTypes).ThenInclude(_=>_.Images)
                        .Include(_=>_.ProductTypes).ThenInclude(_=>_.FirstAtribute)
                        .Include(_=>_.ProductTypes).ThenInclude(_=>_.LinkAtributeValue).ThenInclude(_=>_.CultureName)
                        .Include(_=>_.ProductTypes).ThenInclude(_=>_.LinkAtributeValue).ThenInclude(_=>_.FK_Atribute)
                        .Include(_ => _.ProductTypes).ThenInclude(_ => _.ProductAtributes).ThenInclude(_ => _.AtributeValues).ThenInclude(_=>_.AtributeValue).ThenInclude(_=>_.FK_Atribute)
                        .Include(_ => _.ProductTypes).ThenInclude(_ => _.ProductAtributes).ThenInclude(_ => _.AtributeValues).ThenInclude(_ => _.AtributeValue).ThenInclude(_=>_.CultureName)
                        .Include(_ => _.Reviews).ToListAsync();
                    pr?.ForEach(_ => _.ProductTypes.ToList().ForEach(pt => pt.Images.ToList().ForEach(i => i.IMG = null)));
                    return pr;
                case Key.Review: return await DBContext.Reviews.Include(_=>_.FK_User).Include(_=>_.FK_Product).ToListAsync();
                case Key.Promo:return await DBContext.PromoCodes.ToListAsync();
                case Key.Order:return await DBContext.Orders.ToListAsync();
                case Key.HomePageSections:return await DBContext.HomePageSections
                         .Include(_ => _.HomePageSectionDatas).ThenInclude(_ => _.Title)
                         .Include(_ => _.HomePageSectionDatas).ThenInclude(_ => _.Description)
                         .Include(_ => _.HomePageSectionDatas).ThenInclude(_ => _.Catalog)
                         .ToListAsync();
                default:return null;
            }
        }
        //*****************************************************************************************
        public async Task Update(Key key)
        {
            var temp = await getDB_Object(key);
            _Cache.Set(key, temp, TimeSpan.FromDays(7));
        }
        public async Task<IEnumerable<object>> Cache(Key key)
        {
            if (!_Cache.TryGetValue(key, out IEnumerable<object> temp))
            {
                temp = await getDB_Object(key);
                _Cache.Set(key, temp, TimeSpan.FromDays(7));
            }
            
            return temp;
        }
        public async Task<byte[]> Img(int Id)
        {
            if (Id <= 0) return null;
            if (!_Cache.TryGetValue(Id, out byte[] temp))
            {
                temp = (await DBContext.ProductIMGs.FirstOrDefaultAsync(_ => _.Id == Id))?.IMG;
                _Cache.Set(Id, temp, TimeSpan.FromDays(1));
            }
            return temp;
        } 
    }
}
