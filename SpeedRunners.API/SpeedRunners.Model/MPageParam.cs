namespace SpeedRunners.Model
{
    public class MPageParam
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// 计算出偏移量
        /// </summary>
        public int Offset => PageSize * ((PageNo > 0 ? PageNo : 0) - 1);
        public string Keywords { get; set; }
        public string FuzzyKeywords => $"%{Keywords}%";
    }
}
