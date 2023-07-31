namespace SpeedRunners.Utils
{
    public abstract class DALBase
    {
        protected DALBase(DbHelper db)
        {
            Db = db;
        }

        public DbHelper Db { get; set; }
    }
}
