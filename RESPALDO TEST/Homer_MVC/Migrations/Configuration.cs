namespace Homer_MVC.Migrations
{
    internal sealed class Configuration //: DbMigrationsConfiguration<Homer_MVC.Context.Iceberg_Context>
    {
        public Configuration()
        {
            //migraciones automaticas
            //AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = true;
            //ContextKey = "Homer_MVC.Context.Iceberg_Context";
        }

        //protected override void Seed(Homer_MVC.Context.Iceberg_Context context)
        //{
        //    //  This method will be called after migrating to the latest version.

        //    //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
        //    //  to avoid creating duplicate seed data. E.g.
        //    //
        //    //    context.People.AddOrUpdate(
        //    //      p => p.FullName,
        //    //      new Person { FullName = "Andrew Peters" },
        //    //      new Person { FullName = "Brice Lambson" },
        //    //      new Person { FullName = "Rowan Miller" }
        //    //    );
        //    //
        //}
    }
}
