using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLanches.Models
{
    public class FavoriteProduct
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? Name { get; set; }

        public string? Details { get; set; }

        public decimal Price { get; set; }

        public string? UrlImage { get; set; }

        public bool IsFavorite { get; set; }
    }
}
