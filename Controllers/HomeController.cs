using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AlcantaraNew.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using AlcantaraNew.Classes;
using AlcantaraNew.ViewModels;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Html;
using System.Net;
namespace AlcantaraNew.Controllers
{
    public class HomeController : Controller
    {
        private readonly CacheManager Cache;
        private readonly UserManager<User> userManager;
        private readonly int ProductsCountInPage = 21;
        private readonly int CacheAgeSeconds = 60 * 60 * 24 * 30; //IMG Cach Max time
        private static readonly object orderLock = new object();
        private static readonly object orderNumLock = new object();
        private readonly AlcantaraDBContext DBContext;
        public HomeController(CacheManager cacheManager, UserManager<User> _userManager,AlcantaraDBContext alcantaraDBContext)
        {
            Cache = cacheManager;
            userManager = _userManager;
            DBContext = alcantaraDBContext;
        }
        public async Task<IActionResult> Index()
        {
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            ViewBag.amd = getAMD();
            ViewBag.Culture = Culture;
            var m = (await Cache.Cache(CacheManager.Key.HomePageSections) as IEnumerable<HomePageSection>).OrderBy(_ => _._Index).ToList();
            var catalogs = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>);
            var products = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>);
            if (m != null && m.Count() == 5)
            {
                var sec2 = m.ElementAt(1);
                if (sec2.HomePageSectionDatas != null && sec2.HomePageSectionDatas.Count() > 0 && sec2.HomePageSectionDatas.First().Catalog != null)  
                {
                    var prs = catalogs.FirstOrDefault(_ => _.Id == sec2.HomePageSectionDatas.First().Catalog.Id)?.Products.Select(_ => _.Id);
                    if (prs != null && prs.Count() > 0)
                    {
                        int s = 0;
                        int t = 8;
                        if (prs.Count() > t)
                        {
                            Random ranGen = new Random();
                            s = ranGen.Next(0, prs.Count() % t + 1);
                            prs = prs.Skip(s).Take(t);
                        }
                        ViewBag.Sec2_Products = products.Where(_ => prs.Any(pr => pr == _.Id)).ToList();
                    }
                }

                var sec4 = m.ElementAt(3);
                if (sec4.HomePageSectionDatas != null && sec4.HomePageSectionDatas.Count() > 0 && sec4.HomePageSectionDatas.First().Catalog != null)
                {
                    var prs = catalogs.FirstOrDefault(_ => _.Id == sec4.HomePageSectionDatas.First().Catalog.Id)?.Products.Take(15).Select(_ => _.Id);
                    if (prs != null && prs.Count() > 0) ViewBag.Sec4_Products = products.Where(_ => prs.Any(pr => pr == _.Id)).ToList();
                }
            }
            ViewBag._Model = m;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> AboutUs()
        {
            return await Task.Run(() => View());
        }
        public async Task<IActionResult> ContactUs()
        {
            return await Task.Run(() => View());
        }
        public async Task<IActionResult> Policy()
        {
            return await Task.Run(() => View());
        }
        public async Task<IActionResult> Products(int id, string brand = null, IEnumerable<int> atributeIds = null, string serchKeys = null)
        {
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            ViewBag.Culture = Culture;
            ViewBag.SerchKeys = serchKeys;
            ViewBag.amd = getAMD();
            ViewBag.MinSum = 0;
            ViewBag.MaxSum = 100000;
            List<ProductView> ProductList = new List<ProductView>();
            try
            {
                if (id > 0 || (serchKeys != null && serchKeys.Count() > 0))
                {
                    Catalog catalog = null;
                    if (id>0) catalog = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>).FirstOrDefault(_ => _.Id == id);
                    else if (serchKeys != null && serchKeys.Count() > 0)
                    {
                        catalog = new Catalog() { ChaildCatalogs = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>).Where(_ => _.FatherCatalog == null).ToList(), Name = "All" };
                    }
                    if (catalog != null)
                    {
                        #region Products
                        ViewBag.Catalog = catalog;

                        var products = await GetProducts(catalog, brand, atributeIds, serchKeys);
                        ViewBag.MinSum = products.SelectMany(_ => _.ProductTypes).Min(_ => _.Sale > 0 ? _.Sale : _.Price);
                        ViewBag.MaxSum = products.SelectMany(_ => _.ProductTypes).Max(_ => _.Sale > 0 ? _.Sale : _.Price);
                        ViewBag.Atributes =  GetAtributes(products);
                        IEnumerable<string> brands = GetBrands(await GetProducts(catalog));
                        if (catalog.Id<=0 && !string.IsNullOrEmpty(serchKeys)) brands = brands?.Take(10);
                        ViewBag.Brands = brands;
                        ViewBag.CurrentBrand = brand;
                        ViewBag.CurrentAtributes = atributeIds?.Where(_ => _>0);
                        ProductList =  GetProductViews(products?.Take(ProductsCountInPage));
                        #endregion
                    }
                }
                ViewBag.Products = ProductList;
                return View();
            }
            catch (Exception e)
            {
                ViewBag.Products = ProductList;
                return View();
            }
        }
        [HttpPost]
        public async Task<JsonResult> Products([FromBody]ProductLoadMoreAjax req)
        {
            try
            {
                if (req != null && req.id>0)
                {
                    var catalog = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>).FirstOrDefault(_ => _.Id == req.id);
                    if (catalog != null)
                    {
                        var products = (await GetProducts(catalog, req.brand, req.atributeIds, req.serchKeys, req.min, req.max))?.Skip(req.index * ProductsCountInPage).Take(ProductsCountInPage);
                        if (products != null && products.Count() > 0)
                        {
                            return new JsonResult(new { res = GetProductViews(products) });
                        }
                    }
                }
                return new JsonResult(new { });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> QuickView(int id)
        {
            try
            {
                if (id <= 0) return new JsonResult(new { });
                var product = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>)?.FirstOrDefault(_ => _.Id == id);
                if (product == null) return new JsonResult(new { });

                string amd = getAMD();
                string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                var productType = product.ProductTypes?.FirstOrDefault(_ => _.IsMine);
                var Title = CultureData.GetDefoultName(productType.CultureTitle, Culture);
                var Description = CultureData.GetDefoultName(productType.CultureDescription, Culture);
                var Brand = CultureData.GetDefoultName(productType.CultureBrand, Culture);
                var Price = productType.Price.ToString("N", new CultureInfo("en")).Replace(".00", string.Empty);
                var Sale = productType.Sale.ToString("N", new CultureInfo("en")).Replace(".00", string.Empty);
                var Img = productType.Images?.OrderBy(_ => _.Id).FirstOrDefault()?.Id;
                //var Atributes = product.ProductTypes?.SelectMany(p => p.ProductAtributes).SelectMany(p => p.AtributeValues).Select(p => p.AtributeValue).Distinct().GroupBy(p => p.FK_Atribute).ToDictionary(p => p.Key, p => p.ToList());
                //var LinkAtrVal = productType.LinkAtributeValue;
                int rat = 0;
                if (product?.Reviews != null && product.Reviews.Count() > 0) rat = (int)product.Reviews.Average(_ => _.Rating);

                return new JsonResult(new
                {
                    res = new
                    {
                        id= product.Id,
                        pr_src = Url.Action("Product", "Home", new { id = product.Id }),
                        buy_src= Url.Action("Checkout", "Home", new { id = product.Id }),
                        img_src = (Img == null ? "//:0" : Url.Action("GetProductImg", "Home", new { id = Img })),
                        title = System.Net.WebUtility.HtmlDecode(Title),
                        desc = System.Net.WebUtility.HtmlDecode(Description),
                        brand = Brand,
                        price = Price,
                        sale = Sale,
                        rating = rat,
                        amd
                    }
                });
            }
            catch (Exception e)
            {
                return new JsonResult(new { });
            }
        }
        #region Private Get Functions
        private async Task<IEnumerable<Product>> GetProducts(Catalog catalog, string Brand = null, IEnumerable<int> AtributeValueIds = null, string serchKeys = null, decimal? min = null, decimal? max = null)
        {
            try
            {
                if (catalog == null) return null;
                var skeys = serchKeys?.Trim().Split(' ', ',');
                string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                List<Product> TempProducts = new List<Product>();
                var TempAtrValIds = AtributeValueIds?.Where(_ => _>0);
                if (catalog.Products != null && catalog.Products.Count() > 0)
                {
                    IEnumerable<Product> Products = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>)?.Where(_=>catalog.Products.Any(p=>p.Id==_.Id));
                    var pr = Products.Where(_ => _.IsActive)
                        .Where(_ => _.ProductTypes != null && _.ProductTypes.Count() > 0 && _.ProductTypes.FirstOrDefault(pt => pt.IsMine).IsActive && (string.IsNullOrEmpty(Brand) || CultureData.GetDefoultName(_.ProductTypes.FirstOrDefault(pt => pt.IsMine).CultureBrand, Culture).Trim() == Brand.Trim()))
                        .Where(_ => _.ProductTypes.Any(pt => pt.IsActive && pt.ProductAtributes.Any(atr => atr.ProductQuantity > 0) && (min==null || (pt.Sale<=0?pt.Price:pt.Sale)>=min) && (max == null || (pt.Sale <= 0 ? pt.Price : pt.Sale) <= max)))
                        .Where(_ => TempAtrValIds == null || TempAtrValIds.Count() == 0 || _.ProductTypes.Any(pt => pt.IsActive && pt.ProductAtributes.SelectMany(pa => pa.AtributeValues).Any(av => TempAtrValIds.Any(tav => tav == av.AtributeValue.Id))));
                    if (pr != null && pr.Count() > 0 && skeys != null && skeys.Length > 0)
                    {
                        pr = pr.Where(_ => hasKeyInProduct(_, skeys));
                    }
                    if (pr != null && pr.Count() > 0) TempProducts.AddRange(pr);
                }
                if (catalog.ChaildCatalogs != null && catalog.ChaildCatalogs.Count() > 0)
                {
                    foreach (var ca in catalog.ChaildCatalogs)
                    {
                        var TempPr = await GetProducts(ca, Brand, TempAtrValIds, serchKeys, min, max);
                        if (TempPr != null && TempPr.Count() > 0) TempProducts.AddRange(TempPr);
                    }
                }
                return TempProducts.OrderBy(_ => _.Id);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private bool hasKeyInProduct(Product product, IEnumerable<string> keys)
        {
            try
            {
                if (product == null || product.ProductTypes == null || product.ProductTypes.Count() == 0 || keys == null || keys.Count() == 0) return false;
                keys = keys.Select(_ => _.ToUpper());
                foreach (var prType in product.ProductTypes)
                {
                    if (prType.CultureTitle == null || prType.CultureTitle.Count() == 0) continue;
                    bool HasKey = prType.CultureTitle.Union(prType.CultureBrand).Union(prType.CultureDescription).SelectMany(_ => _.Text.Split(' ', ' ')).Union(prType.SerchKeys.Split(' ', ','))
                        .Where(_ => !string.IsNullOrEmpty(_)).Distinct().Any(_ => keys.Any(k => _.ToUpper().StartsWith(k)));
                    if (HasKey) return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private IDictionary<Atribute, List<AtributeValue>> GetAtributes(IEnumerable<Product> products)
        {
            var TempAtributes = products.SelectMany(_ => _.ProductTypes).SelectMany(_ => _.ProductAtributes)
                .SelectMany(_ => _.AtributeValues).Select(_ => _.AtributeValue).Distinct().GroupBy(_ => _.FK_Atribute).Where(_ => _.Key.IsActive).ToDictionary(_ => _.Key, _ => _.ToList());
            return TempAtributes;
        }
        private IEnumerable<string> GetBrands(IEnumerable<Product> products)
        {
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            IEnumerable<string> brands = null;
            if (products != null && products.Count() > 0)
            {
                brands = products.Select(_ => _.ProductTypes.FirstOrDefault(pt => pt.IsMine)).Where(_ => !string.IsNullOrEmpty(CultureData.GetDefoultName(_.CultureBrand, Culture)))
                    .Select(_ => CultureData.GetDefoultName(_.CultureBrand, Culture)).Distinct();
            }
            return brands;
        }
        private List<ProductView> GetProductViews(IEnumerable<Product> products)
        {
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            List<ProductView> ProductList = new List<ProductView>();
            if (products != null && products.Count() > 0)
            {
                foreach (var pr in products)
                {
                    var pt = pr?.ProductTypes?.OrderBy(_ => _.Id)?.FirstOrDefault(_ => _.IsMine);
                    var RatingList = pr?.Reviews?.Where(_ => _.Status == true)?.Select(_ => _.Rating);
                    int Rating = 0;
                    if (RatingList != null && RatingList.Count() > 0) Rating = (int)RatingList.Average();
                    var imgs = pt?.Images?.OrderBy(_ => _.Id);
                    var TempPT = new ProductView()
                    {
                        Id = pr.Id,
                        Rating = Rating,
                        ImgId = imgs?.FirstOrDefault()?.Id ?? 0,
                        ImgId2= imgs?.ElementAtOrDefault(1)?.Id ?? 0,
                        Title = System.Net.WebUtility.HtmlDecode(CultureData.GetDefoultName(pt.CultureTitle, Culture)),
                        Price = pt.Price.ToString("N", new CultureInfo("en")).Replace(".00", String.Empty),
                        Sale = pt.Sale.ToString("N", new CultureInfo("en")).Replace(".00", String.Empty),
                        Brand = CultureData.GetDefoultName(pt.CultureBrand, Culture).Trim(),
                        LinkAtrVal = pt.LinkAtributeValue
                    };
                    if (TempPT != null) ProductList.Add(TempPT);
                }
            }
            return ProductList;
        }
        private string getAMD()
        {
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            if (Culture == "hy") return "Դրամ";
            else if (Culture == "ru") return "Драм";
            return "AMD";
        }
        #endregion
        public async Task<IActionResult> Product(int id, int typeId = -1)
        {
            ViewBag.amd = getAMD();
            if (id<0) return RedirectToAction("Index");
            var product = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).FirstOrDefault(_ => _.Id == id);
            if (product == null || !product.IsActive) return RedirectToAction("Index");
            ViewBag.Reviews = (await Cache.Cache(CacheManager.Key.Review) as IEnumerable<Review>)?.Where(_ => _.FK_Product.Id == id && _.Status != null && (bool)_.Status).ToArray();
            ViewBag.ID = id;
            string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            ViewBag.Culture = Culture;
            ProductType productType = null;
            if (typeId<0)
            {
                productType = product.ProductTypes.FirstOrDefault(_ => _.Id == typeId && _.IsActive);
                if (productType == null) productType = product.ProductTypes.FirstOrDefault(_ => _.IsMine && _.IsActive);
            }
            else productType = product.ProductTypes.FirstOrDefault(_ => _.IsMine && _.IsActive);

            ProductView resp = null;
            if (product != null)
            {
                resp = new ProductView()
                {
                    Id = product.Id,
                    Title = CultureData.GetDefoultName(productType.CultureTitle, Culture),
                    Description = CultureData.GetDefoultName(productType.CultureDescription, Culture),
                    Brand = CultureData.GetDefoultName(productType.CultureBrand, Culture),
                    Price = productType.Price.ToString("N", new CultureInfo("en")).Replace(".00", string.Empty),
                    Sale = productType.Sale.ToString("N", new CultureInfo("en")).Replace(".00", string.Empty),
                    Imgs = productType.Images?.OrderBy(_ => _.Id).Select(_ => _.Id),
                    Atributes = product.ProductTypes?.SelectMany(p => p.ProductAtributes).SelectMany(p => p.AtributeValues).Select(p => p.AtributeValue).Distinct().GroupBy(p => p.FK_Atribute).ToDictionary(p => p.Key, p => p.ToList()),
                    LinkAtrVal = productType.LinkAtributeValue
                };
            }
            ViewBag.Product = resp;

            #region Products Slide
            var Catalog = (await Cache.Cache(CacheManager.Key.Catalog) as IEnumerable<Catalog>).FirstOrDefault(_ => _.Products.Any(p => p.Id == id));
            if (Catalog != null)
            {
                ViewBag.SlidePrs = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).Where(_ => Catalog.Products.Any(pr => pr.Id == _.Id && _.Id != id)).Where(_ => _.ProductTypes.Any(pt => pt.IsActive && pt.ProductAtributes.Any(atr => atr.ProductQuantity > 0))).Take(15).ToList();
            }
            #endregion

            return View();
        }
        [HttpPost]
        public async Task<JsonResult> getAtributes(int id)
        {
            if (id<=0) return null;
            var product = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).FirstOrDefault(_ => _.Id == id);
            if (product == null) return null;
            var atr = product.ProductTypes?.FirstOrDefault(_ => _.IsMine && _.IsActive)?.ProductAtributes.Select(_ => _.AtributeValues.Select(av => av.AtributeValue.Id.ToString()));
            return new JsonResult(atr);
        }

        public async Task<IActionResult> Favorites()
        {
            try
            {
                string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                ViewBag.Culture = Culture;
                ViewBag.amd = getAMD();
                var priceCulture = new CultureInfo("en");
                string idsJson = Request.Cookies["FavoritIds"];
                IEnumerable<favoriteJson> ids = null;
                if (!string.IsNullOrEmpty(idsJson)) ids = JArray.Parse(idsJson).ToObject<IEnumerable<favoriteJson>>();
                if (ids != null && ids.Count() > 0)
                {
                    var TempProducts = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).Where(_ => ids.Any(atr => atr.id == _.Id.ToString())).Where(_ => _.IsActive && _.ProductTypes.FirstOrDefault(pt => pt.IsMine) != null && _.ProductTypes.First(pt => pt.IsMine).IsActive).ToList();
                    if (TempProducts != null)
                    {
                        if (TempProducts.Count != ids.Count())
                        {
                            var newIds = TempProducts.Select(_ => _.Id);
                            Response.Cookies.Append("FavoritIds", JsonConvert.SerializeObject(newIds));
                        }
                        ViewBag.Products = TempProducts.Select((_) =>
                        {
                            var pt = _.ProductTypes.FirstOrDefault(p => p.IsMine);
                            return new ProductView()
                            {
                                Id = _.Id,
                                ImgId = pt?.Images?.OrderBy(i=>i.Id).FirstOrDefault()?.Id ?? -1,
                                Price = pt?.Price.ToString("N", priceCulture).Replace(".00", string.Empty),
                                Sale = pt?.Sale.ToString("N", priceCulture).Replace(".00", string.Empty),
                                Title = System.Net.WebUtility.HtmlDecode(CultureData.GetDefoultName(pt?.CultureTitle, Culture)),
                                LinkAtrVal = pt.LinkAtributeValue,
                                Atributes = _.ProductTypes?.SelectMany(p => p.ProductAtributes).SelectMany(p => p.AtributeValues).Select(p => p.AtributeValue).Distinct().GroupBy(p => p.FK_Atribute).ToDictionary(p => p.Key, p => p.ToList())
                            };
                        });
                    }
                }
                return View();
            }
            catch (Exception)
            {
                ViewBag.Products = null;
                return View();
            }
        }
        public async Task<IActionResult> Cart()
        {
            try
            {
                string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                ViewBag.Culture = Culture;
                ViewBag.amd = getAMD();
                var gs = (await Cache.Cache(CacheManager.Key.GlobalSetings) as IEnumerable<GlobalSetings>).FirstOrDefault();
                ViewBag.ShippingOrderSum = gs?.ShippingOrderSum ?? 0;
                ViewBag.shippingWhenMore = gs?.ShippingWhenMore ?? 0;
                ViewBag.shippingWhenLess = gs?.ShippingWhenLess ?? 0;
                ViewBag.Shipping = 0;
                var priceCulture = new CultureInfo("en");
                string idsJson = Request.Cookies["ProductList"];
                IEnumerable<ProductJson> ids = null;
                if (!string.IsNullOrEmpty(idsJson)) ids = JArray.Parse(idsJson).ToObject<IEnumerable<ProductJson>>();
                if (ids == null || ids.Count() == 0) { Response.Cookies.Append("ProductList", ""); return View(); }
                var products = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).Where(_ => ids.Any(prId => prId.id == _.Id)).ToList();
                if (products == null || products.Count() == 0) { Response.Cookies.Append("ProductList", ""); return View(); }

                List<ProductBagViewModel> resp = new List<ProductBagViewModel>();

                foreach (var pr in products)
                {
                    IEnumerable<ProductJson> reqEls = ids.Where(_ => _.id == pr.Id);
                    if (reqEls != null && reqEls.Count() > 0)
                    {
                        foreach (var reqEl in reqEls)
                        {
                            var mainPrType = pr.ProductTypes.FirstOrDefault(_ => _.IsActive && _.IsMine);
                            var prTypeAtrs = mainPrType?.ProductAtributes;
                            if (prTypeAtrs != null && prTypeAtrs.Count() > 0)
                            {
                                foreach (var prTypeAtr in prTypeAtrs)
                                {
                                    var atrValIds = prTypeAtr.AtributeValues.Select(_ => _.AtributeValue.Id);
                                    bool isEmptyAtr = false;
                                    if (reqEl.atrs.Count() == 1 && reqEl.atrs.First() == 0) isEmptyAtr = true;
                                    if ((atrValIds.Count() == reqEl.atrs.Count() && atrValIds.All(_ => reqEl.atrs.Any(r => r == _))) || isEmptyAtr)
                                    {
                                        int? imgId = mainPrType?.Images?.FirstOrDefault()?.Id;
                                        ICollection<CultureData> cultureDatas = mainPrType?.CultureTitle;
                                        decimal price = mainPrType.Price;
                                        decimal sale = mainPrType.Sale;
                                        if (pr.LinkAtribute != null)
                                        {
                                            var pt = pr.ProductTypes.FirstOrDefault(_ => reqEl.atrs.Any(id => id == _.LinkAtributeValue?.Id));
                                            if (pt != null)
                                            {
                                                if (pt.Images != null && pt.Images.Count() > 0) imgId = pt.Images.FirstOrDefault()?.Id;
                                                if (!string.IsNullOrEmpty(CultureData.GetDefoultName(pt.CultureTitle, Culture))) cultureDatas = pt.CultureTitle;
                                                if (pt.Price != 0 || pt.Sale != 0)
                                                {
                                                    price = pt.Price;
                                                    sale = pt.Sale;
                                                }
                                            }
                                        }
                                        resp.Add(new ProductBagViewModel()
                                        {
                                            ProductId = pr.Id.ToString(),
                                            Title = System.Net.WebUtility.HtmlDecode(CultureData.GetDefoultName(cultureDatas, Culture)),
                                            MaxQuantity = prTypeAtr.ProductQuantity,
                                            Quantity = reqEl.count,
                                            Atributes = prTypeAtr.AtributeValues.Select(_ => _.AtributeValue),
                                            Price = price.ToString("N", priceCulture).Replace(".00", ""),
                                            dPrice = price,
                                            dSale = sale,
                                            Sale = sale.ToString("N", priceCulture).Replace(".00", ""),
                                            ImgId = imgId?.ToString()
                                        });
                                        if (isEmptyAtr) break;
                                    }
                                }
                            }
                        }
                    }
                }
                ViewBag.Products = resp;

                if (resp != null && resp.Count() > 0) ViewBag.Shipping = await shippingSum(resp.Sum(_ => _.dPrice));
                return View();
            }
            catch (Exception e)
            {
                ViewBag.Products = null;
                return View();
            }

        }
        [HttpPost]
        public async Task<JsonResult> GetCartJson()
        {
            try
            {
                string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                //ViewBag.Culture = Culture;
                var gs = (await Cache.Cache(CacheManager.Key.GlobalSetings) as IEnumerable<GlobalSetings>).FirstOrDefault();
                var priceCulture = new CultureInfo("en");
                string idsJson = Request.Cookies["ProductList"];
                IEnumerable<ProductJson> ids = null;
                if (!string.IsNullOrEmpty(idsJson)) ids = JArray.Parse(idsJson).ToObject<IEnumerable<ProductJson>>();
                if (ids == null || ids.Count() == 0) { Response.Cookies.Append("ProductList", ""); return new JsonResult(null); }
                var products = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).Where(_ => ids.Any(prId => prId.id == _.Id)).ToList();
                if (products == null || products.Count() == 0) { Response.Cookies.Append("ProductList", ""); return new JsonResult(null); }

                List<dynamic> resp = new List<dynamic>();

                foreach (var pr in products)
                {
                    IEnumerable<ProductJson> reqEls = ids.Where(_ => _.id == pr.Id);
                    if (reqEls != null && reqEls.Count() > 0)
                    {
                        foreach (var reqEl in reqEls)
                        {
                            var mainPrType = pr.ProductTypes.FirstOrDefault(_ => _.IsActive && _.IsMine);
                            var prTypeAtrs = mainPrType?.ProductAtributes;
                            if (prTypeAtrs != null && prTypeAtrs.Count() > 0)
                            {
                                foreach (var prTypeAtr in prTypeAtrs)
                                {
                                    var atrValIds = prTypeAtr.AtributeValues.Select(_ => _.AtributeValue.Id);
                                    bool isEmptyAtr = false;
                                    if (reqEl.atrs.Count() == 1 && reqEl.atrs.First() == 0) isEmptyAtr=true;
                                    if ((atrValIds.Count() == reqEl.atrs.Count() && atrValIds.All(_ => reqEl.atrs.Any(r => r == _)))  || isEmptyAtr)
                                    {
                                        int? imgId = mainPrType?.Images?.FirstOrDefault()?.Id;
                                        ICollection<CultureData> cultureDatas = mainPrType?.CultureTitle;
                                        decimal price = mainPrType.Price;
                                        decimal sale = mainPrType.Sale;
                                        if (pr.LinkAtribute != null)
                                        {
                                            var pt = pr.ProductTypes.FirstOrDefault(_ => reqEl.atrs.Any(id => id == _.LinkAtributeValue?.Id));
                                            if (pt != null)
                                            {
                                                if (pt.Images != null && pt.Images.Count() > 0) imgId = pt.Images.FirstOrDefault()?.Id;
                                                if (!string.IsNullOrEmpty(CultureData.GetDefoultName(pt.CultureTitle, Culture))) cultureDatas = pt.CultureTitle;
                                                if (pt.Price != 0 || pt.Sale != 0)
                                                {
                                                    price = pt.Price;
                                                    sale = pt.Sale;
                                                }
                                            }
                                        }
                                        resp.Add(new
                                        {
                                            id = pr.Id.ToString(),
                                            t = System.Net.WebUtility.HtmlDecode( CultureData.GetDefoultName(cultureDatas, Culture)),
                                            max_n = prTypeAtr.ProductQuantity,
                                            n = reqEl.count,
                                            price = (sale <= 0 ? price : sale).ToString("N", priceCulture).Replace(".00", ""),
                                            d_price = (sale <= 0 ? price : sale),
                                            atrs = isEmptyAtr ? new string[] { "0" } : atrValIds?.Select(_ => _.ToString()),
                                            img = imgId?.ToString()
                                        });
                                        if (isEmptyAtr) break;
                                    }
                                }
                            }
                        }
                    }
                }

                int count = 0;
                decimal total = 0;
                if (resp != null && resp.Count() > 0)
                {
                    total = resp.Sum(_ => (decimal)_.d_price * (int)_.n) + await shippingSum(resp.Sum(_ => (decimal)_.d_price * (int)_.n));
                    count = resp.Sum(_ => _.n);
                }
                return new JsonResult(new { resp, asd = getAMD(), count, total });
            }
            catch (Exception e)
            {
                return new JsonResult(null);
            }

        }
        public async Task<decimal> shippingSum(decimal orderSum)
        {
            var gs = (await Cache.Cache(CacheManager.Key.GlobalSetings) as IEnumerable<GlobalSetings>).FirstOrDefault();
            if (gs == null) return 0;
            if (orderSum > gs.ShippingOrderSum) return gs.ShippingWhenMore;
            return gs.ShippingWhenLess;
        }
        public async Task<IActionResult> Checkout(string promoCode, int? id = null)
        {
            try
            {
                PromoCode promo = null;
                if (!string.IsNullOrEmpty(promoCode))
                {
                    promo = (await Cache.Cache(CacheManager.Key.Promo) as IEnumerable<PromoCode>).FirstOrDefault(_ => _.Name.ToUpper() == promoCode.ToUpper());
                    if (promo == null) promoCode = "";
                }
                string haveFailedRequest = HttpContext.Session.GetString("idramReqFailed");
                if (!string.IsNullOrEmpty(haveFailedRequest))
                {
                    ViewBag.ModelError = haveFailedRequest;
                    HttpContext.Session.Remove("idramReqFailed");
                }
                ViewBag.promoCode = promoCode;
                ViewBag.amd = getAMD();
                ViewBag.PromoSale = 0;
                ViewBag.OrderValue = 0;
                string idsJson = Request.Cookies["ProductList"];
                IEnumerable<ProductJson> ids = null;
                if (!string.IsNullOrEmpty(idsJson) && id == null) ids = JArray.Parse(idsJson).ToObject<IEnumerable<ProductJson>>();
                else if (id != null) ids = new ProductJson[] { new ProductJson() { id = (int)id, count = 1, atrs = new int[] { 0 } } };
                if (ids == null || ids.Count() == 0) return RedirectToAction("Cart");
                var products = (await Cache.Cache(CacheManager.Key.Product) as IEnumerable<Product>).Where(_ => ids.Any(prId => prId.id == _.Id)).ToList();
                if (products == null || products.Count() == 0) return RedirectToAction("Cart");
                decimal sum = 0;
                foreach (var pr in products)
                {
                    IEnumerable<ProductJson> reqEls = ids.Where(_ => _.id == pr.Id);
                    if (reqEls != null && reqEls.Count() > 0)
                    {
                        foreach (var reqEl in reqEls)
                        {
                            var mainPrType = pr.ProductTypes.FirstOrDefault(_ => _.IsActive && _.IsMine);
                            var prTypeAtrs = mainPrType?.ProductAtributes;
                            
                            if (prTypeAtrs != null && prTypeAtrs.Count() > 0)
                            {
                                foreach (var prTypeAtr in prTypeAtrs)
                                {
                                    var atrValIds = prTypeAtr.AtributeValues.Select(_ => _.AtributeValue.Id);
                                    bool isEmptyAtr = false;
                                    if (reqEl.atrs.Count() == 1 && reqEl.atrs.First() == 0) isEmptyAtr = true;
                                    if (atrValIds.Count() == reqEl.atrs.Count() && atrValIds.All(_ => reqEl.atrs.Any(r => r == _)) || isEmptyAtr)
                                    {
                                        decimal price = mainPrType.Price;
                                        decimal sale = mainPrType.Sale;
                                        if (pr.LinkAtribute != null)
                                        {
                                            var pt = pr.ProductTypes.FirstOrDefault(_ => reqEl.atrs.Any(_id => _id == _.LinkAtributeValue?.Id));
                                            if (pt != null)
                                            {
                                                if (pt.Price != 0 || pt.Sale != 0)
                                                {
                                                    price = pt.Price;
                                                    sale = pt.Sale;
                                                }
                                            }
                                        }
                                        sum += reqEl.count * (sale != 0 ? sale : price);
                                        if (isEmptyAtr) break;
                                    }
                                }
                            }
                        }
                    }
                }
                //-------------Promo Sale----------------------
                decimal promoSale = 0;
                if (!string.IsNullOrEmpty(promoCode))
                {
                    if (promo != null && promo.Expired > DateTime.Now && promo.isActive)
                    {
                        promoSale = promo.SalePercent;
                    }
                }
                //---------------------------------------------
                ViewBag.PromoSale = promoSale;
                ViewBag.OrderValue = sum;
                ViewBag.Shipping = await shippingSum(sum * (1 - promoSale / 100));
                CheckoutViewModel model = new CheckoutViewModel();
                if (User.Identity.IsAuthenticated)
                {
                    var user = await userManager.GetUserAsync(User);
                    model.Phone = user?.PhoneNumber ?? "";
                    model.Address = (user?.Country == null ? "" : user.Country + (string.IsNullOrEmpty(user.City) ? "" : (" " + user?.City)) + (string.IsNullOrEmpty(user?.Address) ? "" : (" " + user.Address))).TrimStart();
                    model.Email = user?.Email;
                    model.Description = (user?.FirstName ?? "" + " " + user?.LastName ?? "").Trim();
                    if (user?.SelectedAddress != null)
                    {
                        var adr = user.SelectedAddress;
                        model.Address = ((adr?.Country ?? "") + (string.IsNullOrEmpty(adr?.City) ? "" : (" " + adr.City)) + (string.IsNullOrEmpty(adr?.Country) ? "" : (" " + adr.Country)) + (string.IsNullOrEmpty(adr?.Address) ? "" : (" " + adr.Address))).TrimStart();
                        model.Description = (adr?.FName ?? "" + " " + adr?.LName ?? "").Trim();
                    }
                }
                ViewBag.CheckoutViewModel = model;
                return View();
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel _m, int? id = null)
        {
            try
            {
                ViewBag.CheckoutViewModel = _m;
                ViewBag.PromoSale = 0;
                ViewBag.OrderValue = 0;
                ViewBag.Shipping = 0;
                if (string.IsNullOrEmpty(_m.Payment) || !(_m.Payment == "Cash" || _m.Payment == "Idram"))
                {
                    ModelState.AddModelError("", "Incorrect Payment.");
                    return View();
                }
                if (ModelState.IsValid)
                {
                    string Culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                    ViewBag.Culture = Culture;

                    string idsJson = Request.Cookies["ProductList"];
                    IEnumerable<ProductJson> ids = null;

                    if (!string.IsNullOrEmpty(idsJson) && id == null) ids = JArray.Parse(idsJson).ToObject<IEnumerable<ProductJson>>();
                    else if (id != null) ids = new ProductJson[] { new ProductJson() { id = (int)id, count = 1, atrs = new int[] { 0 } } };

                    if ((ids == null || ids.Count() == 0) && id == null) 
                    {
                        ModelState.AddModelError("","Bag is empty.");
                        return View();
                    }
                    var products = await DBContext.Products
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.Images)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.CultureBrand)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.CultureTitle)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.CultureDescription)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.LinkAtributeValue)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.FirstAtribute)
                        .Include(_ => _.ProductTypes).ThenInclude(pt => pt.ProductAtributes).ThenInclude(at => at.AtributeValues).ThenInclude(atv => atv.AtributeValue)
                        .Where(_ => ids.Any(prId => prId.id == _.Id)).ToListAsync();
                    if (products == null || products.Count() == 0)
                    {
                        ModelState.AddModelError("","Incorrect data. Please clear browser history and try again.");
                        return View();
                    }
                    decimal ProductSum = 0;
                    List<OrderProductInfo> OrderProductInfo = new List<OrderProductInfo>();
                    lock (orderLock)
                    {
                        foreach (var pr in products)
                        {
                            IEnumerable<ProductJson> reqEls = ids.Where(_ => _.id == pr.Id);
                            if (reqEls != null && reqEls.Count() > 0)
                            {
                                foreach (var reqEl in reqEls)
                                {
                                    var mainPrType = pr.ProductTypes.FirstOrDefault(_ => _.IsActive && _.IsMine);
                                    var prTypeAtrs = mainPrType?.ProductAtributes;
                                    if (prTypeAtrs != null && prTypeAtrs.Count() > 0)
                                    {
                                        List<OrderAtributeValue> OrderAtrVals = new List<OrderAtributeValue>();
                                        foreach (var prTypeAtr in prTypeAtrs)
                                        {
                                            var atrValIds = prTypeAtr.AtributeValues.Select(_ => _.AtributeValue.Id);
                                            bool isEmptyAtr = false;
                                            if (reqEl.atrs.Count() == 1 && reqEl.atrs.First() == 0) isEmptyAtr = true;
                                            if (atrValIds.Count() == reqEl.atrs.Count() && atrValIds.All(_ => reqEl.atrs.Any(r => r == _)) || isEmptyAtr)
                                            {
                                                if (prTypeAtr.ProductQuantity < reqEl.count || prTypeAtr.ProductQuantity <= 0) 
                                                {
                                                    ModelState.AddModelError("","Please change your bag, some products is ended.");
                                                    return View();
                                                }
                                                prTypeAtr.ProductQuantity -= reqEl.count;
                                                string imgId = mainPrType?.Images?.FirstOrDefault()?.Id.ToString();
                                                var cultureTitle = mainPrType?.CultureTitle;
                                                var cultureDescription = mainPrType?.CultureDescription;
                                                var cultureBrand = mainPrType?.CultureBrand;
                                                decimal price = mainPrType.Price;
                                                decimal sale = mainPrType.Sale;
                                                var OrderPrTy = mainPrType;
                                                if (pr.LinkAtribute != null)
                                                {
                                                    var pt = pr.ProductTypes.FirstOrDefault(_ => _.LinkAtributeValue!=null && reqEl.atrs.Any(_id => _id == _.LinkAtributeValue.Id));
                                                    OrderPrTy = pt;
                                                    if (pt != null)
                                                    {
                                                        if (pt.Images != null && pt.Images.Count() > 0) imgId = pt.Images.FirstOrDefault()?.Id.ToString();
                                                        if (!string.IsNullOrEmpty(CultureData.GetDefoultName(pt.CultureTitle, Culture))) cultureTitle = pt.CultureTitle;
                                                        if (!string.IsNullOrEmpty(CultureData.GetDefoultName(pt.CultureDescription, Culture))) cultureTitle = pt.CultureDescription;
                                                        if (!string.IsNullOrEmpty(CultureData.GetDefoultName(pt.CultureBrand, Culture))) cultureTitle = pt.CultureBrand;

                                                        if (pt.Price != 0 || pt.Sale != 0)
                                                        {
                                                            price = pt.Price;
                                                            sale = pt.Sale;
                                                        }

                                                    }
                                                }

                                                prTypeAtr.AtributeValues.ToList().ForEach(_ => OrderAtrVals.Add(new OrderAtributeValue()
                                                {
                                                    Atribute = CultureData.GetDefoultName(_.AtributeValue.FK_Atribute.CultureName, Culture, _.AtributeValue.FK_Atribute.Name),
                                                    Value = CultureData.GetDefoultName(_.AtributeValue.CultureName, Culture, _.AtributeValue.Value),
                                                    AtributeValue = _.AtributeValue,
                                                }));

                                                ProductSum += reqEl.count * (sale != 0 ? sale : price);

                                                #region OrderInf
                                                OrderProductInfo.Add(new Models.OrderProductInfo()
                                                {
                                                    AtributeAndValue = OrderAtrVals,//1
                                                    ProductSum = (sale != 0 ? sale : price),
                                                    Quantity = reqEl.count,
                                                    ProductTitle = CultureData.GetDefoultName(cultureTitle, Culture),
                                                    ProductDescription = CultureData.GetDefoultName(cultureDescription, Culture),
                                                    Brand = CultureData.GetDefoultName(cultureBrand, Culture),
                                                    ProductImgId = imgId,
                                                    ProductType = OrderPrTy,
                                                    ProductAtributes = prTypeAtr,//2
                                                    Product = pr//3
                                                });

                                                #endregion
                                                if (isEmptyAtr) break;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        //change product count
                    }
                    decimal PromoSale = 0;
                    if (!string.IsNullOrEmpty(_m.PromoCode))
                    {
                        var promo = (await Cache.Cache(CacheManager.Key.Promo) as IEnumerable<PromoCode>).FirstOrDefault(_ => _.Name.ToUpper() == _m.PromoCode.ToUpper());
                        if (promo != null && promo.isActive && promo.Expired > DateTime.Now)
                        {
                            PromoSale = promo.SalePercent;
                        }
                    }
                    decimal ShipingSum = await shippingSum(ProductSum * (1 - PromoSale / 100));
                    ViewBag.PromoSale = PromoSale;
                    ViewBag.OrderValue = ProductSum;
                    ViewBag.Shipping = ShipingSum;
                    #region Create New Order
                    var tempUser = User.Identity.IsAuthenticated ? await userManager.GetUserAsync(User) : null;
                    Order newOrder = null;
                    lock (orderNumLock)
                    {
                        Random random = new Random();
                        var orderNum = RandomOrderId(random);
                        for (;( Cache.Cache(CacheManager.Key.Order).Result as IEnumerable<Order>).Any(_ => _.OrderNumber == orderNum); orderNum = RandomOrderId(random)) ;


                        newOrder = new Order()
                        {
                            Address = _m.Address + (string.IsNullOrEmpty(_m.City) ? "" : ", " + _m.City) + (string.IsNullOrEmpty(_m.ZIP) ? "" : ", " + _m.ZIP),
                            Email = _m.Email,
                            FName = _m.FirstName,
                            LName = _m.LastName,
                            PaymentMethod = _m.Payment,
                            Phone = _m.Phone,
                            Status = Order.StatusType.Pending.ToString(),
                            PromoCode = _m.PromoCode,
                            User = tempUser,
                            ProductsSum = ProductSum,
                            ShipingSum = ShipingSum,
                            PromoSale = PromoSale,
                            ProductsInfo = OrderProductInfo,
                            Created = DateTime.Now,
                            OrderNumber = orderNum

                        };
                        DBContext.Orders.Add(newOrder);

                    }
                    #endregion
                    int status = await DBContext.SaveChangesAsync();
                    if (status > 0)
                    {

                        HttpContext.Session.SetString("Order", newOrder.Id == 0 ? "" : newOrder.Id.ToString());
                        HttpContext.Session.SetString("Promo", _m.PromoCode ?? "");

                        if (_m.Payment == "Cash")
                        {
                            return RedirectToAction("CheckoutOrder");
                        }
                        else if (_m.Payment == "Idram")
                        {
                            string uri = "https://web.idram.am/payment.aspx";
                            string idramId = (await Cache.Cache(CacheManager.Key.GlobalSetings) as IEnumerable<GlobalSetings>).FirstOrDefault()?.IdramId;
                            StringBuilder s = new StringBuilder();
                            var content = new Dictionary<string, string>(new[]
                            {
                                new KeyValuePair<string, string>("EDP_LANGUAGE", Culture.ToUpper()=="HY"?"AM":Culture.ToUpper()),
                                new KeyValuePair<string, string>("EDP_REC_ACCOUNT", idramId),
                                new KeyValuePair<string, string>("EDP_DESCRIPTION","Description=" + $"'{ Encoding.UTF8.GetString( Encoding.Default.GetBytes(string.Join(" ", newOrder.ProductsInfo.Select(_=>_.ProductTitle+", Qty: "+_.Quantity.ToString())).Trim())) }'"),
                                new KeyValuePair<string, string>("EDP_AMOUNT", (newOrder.ShipingSum+newOrder.ProductsSum*(1-newOrder.PromoSale/100)).ToString(".00")),
                                new KeyValuePair<string, string>("EDP_BILL_NO", newOrder.OrderNumber.ToString()),
                                new KeyValuePair<string, string>("EDP_EMAIL", "alcantara@alcantara.am")
                            });
                            s.Append("<html>");
                            s.AppendFormat("<body onload='document.forms[0].submit()'>");
                            s.AppendFormat("<form  action='{0}' method='post'>", uri);
                            foreach (var co in content)
                            {
                                s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", co.Key, co.Value);
                            }
                            s.Append("</form></body></html>");
                            return base.Content(s.ToString(), "text/html");
                        }
                    }
                }
                return View();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error. ");
                return View();
            }

        }

        private long RandomOrderId(Random random)
        {
            byte[] arr = new byte[8];
            random.NextBytes(arr);
            var i64 = BitConverter.ToInt64(arr, 0);
            var res = i64 % 10000000000;
            res = Math.Abs(res);
            if (res < 1000000000) res += 1000000000;
            return res;
        }

        [HttpGet]
        public async Task<IActionResult> CheckoutOrder([FromQuery]string EDP_BILL_NO = null)
        {
            try
            {
                ViewBag.amd = getAMD();
                Response.Cookies.Append("ProductList", "");
                int.TryParse(HttpContext.Session.GetString("Order"), out int id);
                Order order = null;
                if ( id > 0 && EDP_BILL_NO == null) order = await DBContext.Orders
                        .Include(_=>_.ProductsInfo).ThenInclude(_=>_.AtributeAndValue).ThenInclude(_=>_.AtributeValue).ThenInclude(_=> _.FK_Atribute).ThenInclude(_=> _.Values)
                        .Include(_=>_.ProductsInfo).ThenInclude(_=>_.AtributeAndValue).ThenInclude(_=>_.AtributeValue).ThenInclude(_=> _.FK_Atribute).ThenInclude(_=> _.CultureName)
                        .Include(_=>_.ProductsInfo).ThenInclude(_=>_.AtributeAndValue).ThenInclude(_=>_.AtributeValue).ThenInclude(_=> _.CultureName)
                        .FirstOrDefaultAsync(_ => _.Id == id && _.Status == Order.StatusType.Pending.ToString());
                else if (!string.IsNullOrEmpty(EDP_BILL_NO)) order = await DBContext.Orders
                        .Include(_ => _.ProductsInfo).ThenInclude(_ => _.AtributeAndValue).ThenInclude(_ => _.AtributeValue).ThenInclude(_ => _.FK_Atribute).ThenInclude(_ => _.Values)
                        .Include(_ => _.ProductsInfo).ThenInclude(_ => _.AtributeAndValue).ThenInclude(_ => _.AtributeValue).ThenInclude(_ => _.FK_Atribute).ThenInclude(_ => _.CultureName)
                        .Include(_ => _.ProductsInfo).ThenInclude(_ => _.AtributeAndValue).ThenInclude(_ => _.AtributeValue).ThenInclude(_ => _.CultureName)
                        .FirstOrDefaultAsync(_ => EDP_BILL_NO == _.OrderNumber.ToString() && _.Status == Order.StatusType.Pending.ToString());
                if (order == null) return View();
                ViewBag.Order = order;
                ViewBag.culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                return await Task.Run(() => View());
            }
            catch (Exception e)
            {
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> idramReqFailed()
        {
            HttpContext.Session.SetString("idramReqFailed", "Payment not received!!!\nPlease pay with another payment options or try again.");
            string promo = HttpContext.Session.GetString("Promo");
            string orderId = HttpContext.Session.GetString("Order");//orderId
            if (!string.IsNullOrEmpty(orderId))
            {
                Order order = await DBContext.Orders.Include(_ => _.ProductsInfo).ThenInclude(_ => _.ProductAtributes).FirstOrDefaultAsync(_ => _.Id.ToString() == orderId);
                if (order != null)
                {
                    order.Status = Order.StatusType.Canceled.ToString();
                    order.ProductsInfo.ToList().ForEach(_ => _.ProductAtributes.ProductQuantity += _.Quantity);
                    await DBContext.SaveChangesAsync();
                }
            }
            HttpContext.Session.Remove("Order");
            return await Task.Run(() => RedirectToAction("Checkout", new { promoCode = promo ?? "" }));
        }
        #region Idram
        [HttpPost]
        public async Task<IActionResult> idramRes()//string EDP_PRECHECK, string EDP_BILL_NO, string EDP_REC_ACCOUNT, string EDP_AMOUNT, string EDP_PAYER_ACCOUNT = null, string EDP_TRANS_ID = null, string EDP_TRANS_DATE = null,[FromForm] string EDP_CHECKSUM = null)
        {
            try
            {
                string EDP_PRECHECK = Request.Form["EDP_PRECHECK"];
                string EDP_BILL_NO = Request.Form["EDP_BILL_NO"];
                string EDP_REC_ACCOUNT = Request.Form["EDP_REC_ACCOUNT"];
                string EDP_AMOUNT = Request.Form["EDP_AMOUNT"];
                string idramId = (await DBContext.GlobalSetings.FirstOrDefaultAsync())?.IdramId;

                if (!string.IsNullOrEmpty(EDP_PRECHECK) && !string.IsNullOrEmpty(EDP_BILL_NO) && !string.IsNullOrEmpty(EDP_REC_ACCOUNT) && !string.IsNullOrEmpty(EDP_AMOUNT))
                {
                    if (EDP_PRECHECK == "YES")
                    {
                        if (idramId != null && EDP_REC_ACCOUNT == idramId)
                        {
                            var order = await DBContext.Orders.FirstOrDefaultAsync(_ => _.OrderNumber.ToString() == EDP_BILL_NO);
                            if (order != null)
                            {
                                decimal amout = order.ShipingSum + order.ProductsSum * (1 - order.PromoSale / 100);
                                decimal reqAmout = Convert.ToDecimal(EDP_AMOUNT, new CultureInfo("en-US"));

                                if (reqAmout <= 0) return Ok();
                                else if (reqAmout >= amout) return Ok("OK");
                                else if ((100 * (amout - reqAmout) / amout) < 1) //< 1%
                                {
                                    return Ok("OK");
                                }
                                else return Ok();//http-400
                            }
                            else return Ok();//http-400
                        }
                        else
                        {
                            return Ok();//http-400
                        }
                    }
                }
                string EDP_PAYER_ACCOUNT = Request.Form["EDP_PAYER_ACCOUNT"];
                string EDP_TRANS_ID = Request.Form["EDP_TRANS_ID"];
                string EDP_TRANS_DATE = Request.Form["EDP_TRANS_DATE"];
                string EDP_CHECKSUM = Request.Form["EDP_CHECKSUM"];
                if (string.IsNullOrEmpty(EDP_CHECKSUM)) EDP_CHECKSUM = Request.Form["&EDP_CHECKSUM"];
                if (!string.IsNullOrEmpty(EDP_REC_ACCOUNT) && EDP_REC_ACCOUNT == idramId && !string.IsNullOrEmpty(EDP_PAYER_ACCOUNT) && !string.IsNullOrEmpty(EDP_AMOUNT) && !string.IsNullOrEmpty(EDP_BILL_NO) && !string.IsNullOrEmpty(EDP_TRANS_ID) && !string.IsNullOrEmpty(EDP_TRANS_DATE) && !string.IsNullOrEmpty(EDP_CHECKSUM))
                {
                    string SECRET_KEY = (await DBContext.GlobalSetings.FirstOrDefaultAsync())?.IdramSecretKey;
                    string textHash = $"{EDP_REC_ACCOUNT}:{EDP_AMOUNT}:{SECRET_KEY}:{EDP_BILL_NO}:{EDP_PAYER_ACCOUNT}:{EDP_TRANS_ID}:{EDP_TRANS_DATE}";
                    string Hash = MD5Hash(textHash);
                    if (Hash.ToUpper() == EDP_CHECKSUM)
                    {
                        IdramPaymentHistory ph = new IdramPaymentHistory()
                        {
                            Created = DateTime.Now,
                            CheckSum = EDP_CHECKSUM,
                            TransactionDate = EDP_TRANS_DATE,
                            TransactionID = EDP_TRANS_ID,
                            IdramID = EDP_REC_ACCOUNT,
                            PayerIdramId = EDP_PAYER_ACCOUNT,
                            OrderNumber = EDP_BILL_NO,
                            OrderPayedSum = EDP_AMOUNT
                        };
                        DBContext.IdramPaymentHistories.Add(ph);
                        await DBContext.SaveChangesAsync();
                        return Ok("OK");
                    }
                }
                return Ok();
            }
            catch (Exception)
            {
                return Ok();//http-400
            }
        }
        public string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetLanguage(string culture, string returnUrl)
        {
            HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return await Task.Run(() => LocalRedirect(returnUrl));
        }
        public async Task<IActionResult> GetProductImg(int Id)
        {
            if (Id <= 0) return NotFound();
            var TempImg = await Cache.Img(Id);
            if (TempImg != null)
            {
                Response.Headers["Cache-Control"] = $"public,max-age={CacheAgeSeconds}";
                return File(TempImg, "image/jpeg");
            }
            return NotFound();
        }
        public async Task<IActionResult> GetIndexImg(string Id)
        {
            if (string.IsNullOrEmpty(Id)) return NotFound();
            var TempImg = await DBContext.HomePageSectionDatas.FirstOrDefaultAsync(_ => _.Id == Id);
            if (TempImg != null && TempImg.Img != null && TempImg.Img.Count() > 0)
            {
                Response.Headers["Cache-Control"] = $"public,max-age={CacheAgeSeconds}";
                return File(TempImg.Img, "image/jpeg");
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<JsonResult> Subscribe(string email)
        {
            if (string.IsNullOrEmpty(email)) return new JsonResult(new { err = "Email is empty." });
            if (!Classes.EmailService.isEmailAddres(email)) return new JsonResult(new { err = "Incorrect email address." });
            try
            {
                string le = email.ToLower();
                var has = await DBContext.SubscribeEmails.FirstOrDefaultAsync(_ => _.Email == le);
                if (has != null) return new JsonResult(new { res = "Email already added." });
                DBContext.SubscribeEmails.Add(new SubscribeEmail() { Created = DateTime.Now, Email = email.ToLower(), isNew = true });
                int status = await DBContext.SaveChangesAsync();
                if (status > 0) return new JsonResult(new { res = "Email is add." }); ;
                return new JsonResult(new { err = "Email not added." });
            }
            catch (Exception)
            {
                return new JsonResult(new { err = "Exeption" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> requestEmail(string email, string title, string mess)
        {
            if (Classes.EmailService.isEmailAddres(email))
            {
                if (title != null && title.Length > 150) title = title.Substring(0, 150);
                if (mess != null && mess.Length > 500) mess = mess?.Substring(0, 500);
                DBContext.RequestEmails.Add(new RequestEmail() { Created = DateTime.Now, Title = title, Email = email, Message = mess, isNew = true });
                if (await DBContext.SaveChangesAsync() > 0)
                {
                    return new JsonResult(new { res = true });
                }
                return new JsonResult(new { err = "Email not added." });
            }
            return new JsonResult(new { err = "Incorrect email." });
        }
        [HttpPost]
        public async Task<JsonResult> Review(int Rating, string Description, int ProductId)
        {
            try
            {
                if (Rating < 0 || Rating > 5 || string.IsNullOrEmpty(Description) || ProductId<=0) return new JsonResult(new { err = "Incorrect Data." });
                var user = await userManager.GetUserAsync(User);
                var product = await DBContext.Products.FirstOrDefaultAsync(_ => _.Id == ProductId);
                if (user != null && product != null)
                {
                    bool? TempStatus = null;
                    var gs = await DBContext.GlobalSetings.FirstOrDefaultAsync();
                    if (gs != null && gs.EnableAllReviews) TempStatus = true;
                    DBContext.Reviews.Add(new Models.Review() { Description = Description, Rating = Rating, Status = TempStatus, FK_Product = product, FK_User = user, Date = DateTime.Now });
                    if (await DBContext.SaveChangesAsync() > 0) return new JsonResult(new { res = true });
                }
                return new JsonResult(new { err = "Incorrect Data." });
            }
            catch (Exception e)
            {
                return new JsonResult(new { err = "Exception" });
            }
        }
    }
}
