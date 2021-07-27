using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.Asset
{
    public class MMod
    {
        public int ID { get; set; }
        public int Tag { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string FileUrl { get; set; }
        public string AuthorID { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        public int Download { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
