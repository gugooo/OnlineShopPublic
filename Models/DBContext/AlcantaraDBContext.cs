using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class AlcantaraDBContext : IdentityDbContext<User>
    {
        public DbSet<LiveChatSession> LiveChatSessions { get; set; }
        public DbSet<GlobalSetings> GlobalSetings { get; set; }
        public DbSet<Atribute> Atributes { get; set; }
        public DbSet<AtributeValue> AtributeValues { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductIMG> ProductIMGs { get; set; }
        public DbSet<CultureData> CultureDatas { get; set; }
        public DbSet<ProductAtributes> ProductAtributes { get; set; }
        public DbSet<ProductAtributeValue> ProductAtributeValues { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<IdramPaymentHistory> IdramPaymentHistories { get; set; }
        public DbSet<HomePageSection> HomePageSections { get; set; }
        public DbSet<HomePageSectionData> HomePageSectionDatas { get; set; }
        public DbSet<SerchHistory> SerchHistories { get; set; }
        public DbSet<SubscribeEmail> SubscribeEmails { get; set; }
        public DbSet<MailingHistory> MailingHistories { get; set; }
        public DbSet<RequestCell> RequestCells { get; set; }
        public DbSet<RequestEmail> RequestEmails { get; set; }
        public AlcantaraDBContext(DbContextOptions<AlcantaraDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AtributeValue>().HasOne(_ => _.FK_Atribute).WithMany(_ => _.Values).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CultureData>().HasOne(_ => _.FK_Title).WithMany(_ => _.CultureTitle).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CultureData>().HasOne(_ => _.FK_Description).WithMany(_ => _.CultureDescription).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CultureData>().HasOne(_ => _.FK_Brand).WithMany(_ => _.CultureBrand).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CultureData>().HasOne(_ => _.FK_HP_Title).WithMany(_ => _.Title).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CultureData>().HasOne(_ => _.FK_HP_Description).WithMany(_ => _.Description).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasOne(_ => _.FK_UserSend).WithMany(_ => _.UserSend).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(_ => _.FK_AdminSend).WithMany(_ => _.AdminSend).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Catalog>().HasOne(_ => _.FatherCatalog).WithMany(_ => _.ChaildCatalogs).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HomePageSectionData>().HasOne(_ => _.FK_HomePageSection).WithMany(_ => _.HomePageSectionDatas).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Review>().HasOne(_ => _.FK_Product).WithMany(_ => _.Reviews).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Review>().HasOne(_ => _.FK_User).WithMany(_ => _.Reviews).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>().HasOne(_ => _.User).WithMany(_ => _.Orders).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<OrderAtributeValue>().HasOne(_ => _.ProductInfo).WithMany(_ => _.AtributeAndValue).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<OrderProductInfo>().HasOne(_ => _.Order).WithMany(_ => _.ProductsInfo).OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
